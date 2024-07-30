using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;


namespace WebApp.Repository.BG
{
    public class BG0003Repository : JCLib.Mvc.BaseRepository
    {
        public BG0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢
        public IEnumerable<BG0003> GetAll(string START_DATE, string END_DATE, string CLASS_KIND, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = "";

            if (CLASS_KIND == "1")
            {
                sql = @"  SELECT '' MMCODE_2,
                                 '' APPDEPT,
                                 TO_CHAR (TO_NUMBER (TO_CHAR (MDM.APPTIME, 'YYYYMM')) - 191100) YYYMM,
                                 MDD.MMCODE MMCODE,
                                 MMT.MMNAME_E MMNAME_E,
                                 MMT.MMNAME_C MMNAME_C,
                                 MMT.BASE_UNIT BASE_UNIT,
                                 TO_CHAR (MMT.UPRICE, 'FM99999999990.00') UPRICE,
                                 MMT.M_AGENNO M_AGENNO,
                                 (SELECT AGEN_NAMEC
                                  FROM PH_VENDER
                                  WHERE AGEN_NO = MMT.M_AGENNO) AGEN_NAMEC,
                                 (SELECT MMS.MAT_CLASS || ' ' || MMS.MAT_CLSNAME
                                  FROM MI_MATCLASS MMS
                                  WHERE MMS.MAT_CLASS = MDM.MAT_CLASS) MAT_CLASS,
                                 SUM (MDD.APPQTY) APPQTY,
                                 TO_CHAR (SUM (MDD.APPQTY) * UPRICE, 'FM99999999990.00') ESTPAY
                          FROM  ME_DOCM MDM, ME_DOCD MDD, MI_MAST MMT
                          WHERE   MDM.DOCNO = MDD.DOCNO
                          AND     MDD.MMCODE = MMT.MMCODE
                          AND TRUNC (MDM.APPTIME) >= TO_DATE ( :START_DATE, 'YYYY/MM/DD')
                          AND TRUNC (MDM.APPTIME) <= TO_DATE ( :END_DATE, 'YYYY/MM/DD')
                          AND MMT.M_STOREID = '0'
                          AND MDM.DOCTYPE = 'MR4'
                          AND MMT.M_CONTID <> '0'
                          AND MDM.MAT_CLASS = '02'
                          AND MDM.APPLY_KIND = '1'
                          AND MDD.APPQTY <> 0
                          GROUP BY TO_CHAR (TO_NUMBER (TO_CHAR (MDM.APPTIME, 'YYYYMM')) - 191100),
                                   MDD.MMCODE,
                                   MMT.MMNAME_E,
                                   MMT.MMNAME_C,
                                   MMT.BASE_UNIT,
                                   MMT.UPRICE,
                                   MMT.M_AGENNO,
                                   MDM.MAT_CLASS
                          HAVING (SUM (MDD.APPQTY) * UPRICE) >= 100000
                          ORDER BY MDD.MMCODE";
            }
            else if (CLASS_KIND == "2")
            {
                sql = @"  SELECT '' M_AGENNO,
                                 '' APPDEPT,
                                 MDD.MMCODE MMCODE,
                                 TO_CHAR (TO_NUMBER (TO_CHAR (MDM.APPTIME, 'YYYYMM')) - 191100) YYYMM_2,
                                 (SELECT URID.INID || ' ' || URID.INID_NAME
                                  FROM UR_INID URID
                                  WHERE URID.INID = MDM.APPDEPT) INID,
                                 MDM.DOCNO DOCNO,
                                 MDD.MMCODE MMCODE_2,
                                 AA.MMNAME_E MMNAME_E_2,
                                 AA.MMNAME_C MMNAME_C_2,
                                 AA.BASE_UNIT BASE_UNIT_2,
                                 AA.UPRICE UPRICE_2,
                                 MDD.APPQTY APPQTY_2,
                                 AA.ESTPAY ESTPAY_2
                          FROM ME_DOCM MDM,
                               ME_DOCD MDD,
                               (  SELECT MDD2.MMCODE,
                                         MMT.MMNAME_E,
                                         MMT.MMNAME_C,
                                         MMT.BASE_UNIT,
                                         TO_CHAR (MMT.UPRICE, 'FM99999999990.00') UPRICE,
                                         TO_CHAR (SUM (MDD2.APPQTY * MMT.UPRICE), 'FM99999999990.00') ESTPAY
                                  FROM ME_DOCM MDM2, ME_DOCD MDD2, MI_MAST MMT
                                  WHERE     MDM2.DOCNO = MDD2.DOCNO
                                  AND MDD2.MMCODE = MMT.MMCODE
                                  AND TRUNC (MDM2.APPTIME) >= TO_DATE ( :START_DATE, 'YYYY/MM/DD')
                                  AND TRUNC (MDM2.APPTIME) <= TO_DATE ( :END_DATE, 'YYYY/MM/DD')
                                  AND MDM2.MAT_CLASS = '02'
                                  AND MMT.M_STOREID = '0'
                                  AND MMT.M_CONTID <> '0'
                                  AND MDM2.APPLY_KIND = '1'
                                  AND MDM2.DOCTYPE = 'MR4'
                                  AND MDD2.APPQTY <> 0
                                  GROUP BY MDD2.MMCODE,
                                           MMT.MMNAME_E,
                                           MMT.MMNAME_C,
                                           MMT.BASE_UNIT,
                                           MMT.UPRICE
                                  HAVING SUM (MDD2.APPQTY * MMT.UPRICE) >= 100000) AA
                          WHERE     MDM.DOCNO = MDD.DOCNO
                          AND MDD.MMCODE = AA.MMCODE
                          AND TRUNC (MDM.APPTIME) >= TO_DATE ( :START_DATE, 'YYYY/MM/DD')
                          AND TRUNC (MDM.APPTIME) <= TO_DATE ( :END_DATE, 'YYYY/MM/DD')
                          ORDER BY MDD.MMCODE, MDM.APPDEPT";
            }
            else if (CLASS_KIND == "3")
            {
                sql = @"  SELECT DISTINCT MDM.APPDEPT,
                                 (SELECT URID.INID_NAME
                                  FROM UR_INID URID
                                  WHERE URID.INID = MDM.APPDEPT) INID_NAME,
                                 '' MMCODE,
                                 '' MMCODE_2,
                                 '' M_AGENNO
                          FROM ME_DOCM MDM,
                               ME_DOCD MDD,
                               MI_MAST MMT
                          WHERE     MDM.DOCNO = MDD.DOCNO
                          AND MDD.MMCODE = MMT.MMCODE
                          AND TRUNC (MDM.APPTIME) >= TO_DATE ( :START_DATE, 'YYYY/MM/DD')
                          AND TRUNC (MDM.APPTIME) <= TO_DATE ( :END_DATE, 'YYYY/MM/DD')
                          AND MDM.MAT_CLASS = '02'
                          AND MMT.M_STOREID = '0'
                          AND MDM.DOCTYPE = 'MR4'
                          AND MDM.APPDEPT NOT IN (SELECT DISTINCT APPDEPT
                                                  FROM ME_DOCM MDM2,
                                                       ME_DOCD MDD2,
                                                       MI_MAST MMT2
                                                  WHERE     MDM2.DOCNO = MDD2.DOCNO
                                                  AND MDD2.MMCODE = MMT2.MMCODE
                                                  AND TRUNC (MDM2.APPTIME) >= TO_DATE ( :START_DATE, 'YYYY/MM/DD')
                                                  AND TRUNC (MDM2.APPTIME) <= TO_DATE ( :END_DATE, 'YYYY/MM/DD')
                                                  AND MDM.MAT_CLASS = '02'
                                                  AND MMT2.M_STOREID = '0'
                                                  AND MDM.DOCTYPE = 'MR4'
                                                  AND MDM.APPLY_KIND = '1')
                          ORDER BY MDM.APPDEPT";
            }

            p.Add(":START_DATE", string.Format("{0}", START_DATE));
            p.Add(":END_DATE", string.Format("{0}", END_DATE));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BG0003>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //匯出
        public DataTable GetExcel(string START_DATE, string END_DATE, string CLASS_KIND)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = "";

            if (CLASS_KIND == "1")
            {
                sql = @"  SELECT TO_CHAR (TO_NUMBER (TO_CHAR (MDM.APPTIME, 'YYYYMM')) - 191100) 年月,
                                 MDD.MMCODE 院內碼,
                                 MMT.MMNAME_E 英文品名,
                                 MMT.MMNAME_C 中文品名,
                                 MMT.BASE_UNIT 計量單位,
                                 TO_CHAR (MMT.UPRICE, 'FM99999999990.00') 單價,
                                 MMT.M_AGENNO 廠商碼,
                                 (SELECT AGEN_NAMEC
                                  FROM PH_VENDER
                                  WHERE AGEN_NO = MMT.M_AGENNO) 廠商名稱,
                                 (SELECT MMS.MAT_CLASS || ' ' || MMS.MAT_CLSNAME
                                  FROM MI_MATCLASS MMS
                                  WHERE MMS.MAT_CLASS = MDM.MAT_CLASS) 物料類別,
                                 SUM (MDD.APPQTY) 數量,
                                 TO_CHAR (SUM (MDD.APPQTY) * UPRICE, 'FM99999999990.00') 預估申購金額
                          FROM  ME_DOCM MDM, ME_DOCD MDD, MI_MAST MMT
                          WHERE   MDM.DOCNO = MDD.DOCNO
                          AND     MDD.MMCODE = MMT.MMCODE
                          AND TRUNC (MDM.APPTIME) >= TO_DATE ( :START_DATE + 19110000, 'YYYY/MM/DD')
                          AND TRUNC (MDM.APPTIME) <= TO_DATE ( :END_DATE + 19110000, 'YYYY/MM/DD')
                          AND MMT.M_STOREID = '0'
                          AND MDM.DOCTYPE = 'MR4'
                          AND MMT.M_CONTID <> '0'
                          AND MDM.MAT_CLASS = '02'
                          AND MDM.APPLY_KIND = '1'
                          AND MDD.APPQTY <> 0
                          GROUP BY TO_CHAR (TO_NUMBER (TO_CHAR (MDM.APPTIME, 'YYYYMM')) - 191100),
                                   MDD.MMCODE,
                                   MMT.MMNAME_E,
                                   MMT.MMNAME_C,
                                   MMT.BASE_UNIT,
                                   MMT.UPRICE,
                                   MMT.M_AGENNO,
                                   MDM.MAT_CLASS
                          HAVING (SUM (MDD.APPQTY) * UPRICE) >= 100000
                          ORDER BY MDD.MMCODE";
            }
            else if (CLASS_KIND == "2")
            {
                sql = @"  SELECT TO_CHAR (TO_NUMBER (TO_CHAR (MDM.APPTIME, 'YYYYMM')) - 191100) 年月,
                                 (SELECT URID.INID || ' ' || URID.INID_NAME
                                  FROM UR_INID URID
                                  WHERE URID.INID = MDM.APPDEPT) 責任中心,
                                 MDM.DOCNO 申請單號,
                                 MDD.MMCODE 院內碼,
                                 AA.MMNAME_E 英文品名,
                                 AA.MMNAME_C 中文品名,
                                 AA.BASE_UNIT 計量單位,
                                 AA.UPRICE 單價,
                                 MDD.APPQTY 數量,
                                 AA.ESTPAY 預估申購金額
                          FROM ME_DOCM MDM,
                               ME_DOCD MDD,
                               (  SELECT MDD2.MMCODE,
                                         MMT.MMNAME_E,
                                         MMT.MMNAME_C,
                                         MMT.BASE_UNIT,
                                         TO_CHAR (MMT.UPRICE, 'FM99999999990.00') UPRICE,
                                         TO_CHAR (SUM (MDD2.APPQTY * MMT.UPRICE), 'FM99999999990.00') ESTPAY
                                  FROM ME_DOCM MDM2, ME_DOCD MDD2, MI_MAST MMT
                                  WHERE     MDM2.DOCNO = MDD2.DOCNO
                                  AND MDD2.MMCODE = MMT.MMCODE
                                  AND TRUNC (MDM2.APPTIME) >= TO_DATE ( :START_DATE + 19110000, 'YYYY/MM/DD')
                                  AND TRUNC (MDM2.APPTIME) <= TO_DATE ( :END_DATE + 19110000, 'YYYY/MM/DD')
                                  AND MDM2.MAT_CLASS = '02'
                                  AND MMT.M_STOREID = '0'
                                  AND MMT.M_CONTID <> '0'
                                  AND MDM2.APPLY_KIND = '1'
                                  AND MDM2.DOCTYPE = 'MR4'
                                  AND MDD2.APPQTY <> 0
                                  GROUP BY MDD2.MMCODE,
                                           MMT.MMNAME_E,
                                           MMT.MMNAME_C,
                                           MMT.BASE_UNIT,
                                           MMT.UPRICE
                                  HAVING SUM (MDD2.APPQTY * MMT.UPRICE) >= 100000) AA
                          WHERE     MDM.DOCNO = MDD.DOCNO
                          AND MDD.MMCODE = AA.MMCODE
                          AND TRUNC (MDM.APPTIME) >= TO_DATE ( :START_DATE + 19110000, 'YYYY/MM/DD')
                          AND TRUNC (MDM.APPTIME) <= TO_DATE ( :END_DATE + 19110000, 'YYYY/MM/DD')
                          ORDER BY MDD.MMCODE, MDM.APPDEPT";
            }
            else if (CLASS_KIND == "3")
            {
                sql = @"  SELECT DISTINCT MDM.APPDEPT 責任中心,
                                 (SELECT URID.INID_NAME
                                  FROM UR_INID URID
                                  WHERE URID.INID = MDM.APPDEPT) 責任中心名稱
                          FROM ME_DOCM MDM,
                               ME_DOCD MDD,
                               MI_MAST MMT
                          WHERE     MDM.DOCNO = MDD.DOCNO
                          AND MDD.MMCODE = MMT.MMCODE
                          AND TRUNC (MDM.APPTIME) >= TO_DATE ( :START_DATE + 19110000, 'YYYY/MM/DD')
                          AND TRUNC (MDM.APPTIME) <= TO_DATE ( :END_DATE + 19110000, 'YYYY/MM/DD')
                          AND MDM.MAT_CLASS = '02'
                          AND MMT.M_STOREID = '0'
                          AND MDM.DOCTYPE = 'MR4'
                          AND MDM.APPDEPT NOT IN (SELECT DISTINCT APPDEPT
                                                  FROM ME_DOCM MDM2,
                                                       ME_DOCD MDD2,
                                                       MI_MAST MMT2
                                                  WHERE     MDM2.DOCNO = MDD2.DOCNO
                                                  AND MDD2.MMCODE = MMT2.MMCODE
                                                  AND TRUNC (MDM2.APPTIME) >= TO_DATE ( :START_DATE + 19110000, 'YYYY/MM/DD')
                                                  AND TRUNC (MDM2.APPTIME) <= TO_DATE ( :END_DATE + 19110000, 'YYYY/MM/DD')
                                                  AND MDM.MAT_CLASS = '02'
                                                  AND MMT2.M_STOREID = '0'
                                                  AND MDM.DOCTYPE = 'MR4'
                                                  AND MDM.APPLY_KIND = '1')
                          ORDER BY MDM.APPDEPT";
            }

            p.Add(":START_DATE", string.Format("{0}", START_DATE));
            p.Add(":END_DATE", string.Format("{0}", END_DATE));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

    }
}