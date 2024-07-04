using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;

namespace WebApp.Repository.AB
{
    public class AB0070Repository : JCLib.Mvc.BaseRepository
    {
        public AB0070Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0070> SearchReportData(string WH_NO, string DiffCls, string TR_IO,
            string DiffDate_Start, string DiffDate_End, string MMCODE_Start, string MMCODE_End)
        {
            var p = new DynamicParameters();

            var sql = @"select a.MMCODE, b.MMNAME_E, b.BASE_UNIT, a.TR_DOCNO,
                        (select APLYITEM_NOTE from ME_DOCD where DOCNO=TR_DOCNO and MMCODE=a.MMCODE) as T_NOTES1, --備註1
                        TR_INV_QTY, --數量 
                        (select UPRICE from MI_WHCOST where DATA_YM=(TO_CHAR(TR_DATE,'YYYYMM')-191100) and MMCODE=a.MMCODE) as UPRICE,
                        (select UPRICE*TR_INV_QTY from MI_WHCOST 
                        where DATA_YM=(TO_CHAR(TR_DATE,'YYYYMM')-191100) and MMCODE=a.MMCODE) as T_MONEY, --金額
                        (select TOWH from ME_DOCM where DOCNO=TR_DOCNO) TOWH, --入庫別
                        (select FRWH from ME_DOCM where DOCNO=TR_DOCNO) FRWH, --出庫別
                        (select APPID from ME_DOCM where DOCNO=TR_DOCNO) APPID,
                        (TO_CHAR(TR_DATE,'YYYYMMDD')-19110000) as T_DATE,
                        (TO_CHAR(TR_DATE,'hh24mi')) as T_HM,
                        (select APPLY_NOTE from ME_DOCM where DOCNO=TR_DOCNO) T_NOTES, --備註
                        (select APPQTY from ME_DOCD where DOCNO = TR_DOCNO and MMCODE= a.MMCODE) T_APPQTY, --申請數量
                        (select LOT_NO from ME_DOCE where DOCNO=TR_DOCNO and MMCODE=a.MMCODE and ROWNUM = 1) LOT_NO, --藥品批號
                        (select UNA from UR_ID where TUSER=(select UPDATE_USER from ME_DOCD where DOCNO = TR_DOCNO and MMCODE= a.MMCODE)) T_OP  --操作者
                        from MI_WHTRNS a, MI_MAST b
                        where a.MMCODE = b.MMCODE";

            if (WH_NO != "")
            {
                sql += " AND WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", WH_NO));
            }
            if (DiffCls != "")
            {
                sql += " and TR_DOCTYPE = :DiffCls ";
                p.Add(":DiffCls", string.Format("{0}", DiffCls));
            }
            if (TR_IO != "")
            {
                if (TR_IO == "IO")
                {
                    sql += " and TR_IO IN ('I', 'O') ";
                }
                else
                {
                    sql += " and TR_IO = :TR_IO ";
                    p.Add(":TR_IO", string.Format("{0}", TR_IO));
                }
            }
            if (DiffDate_Start != "" & DiffDate_End != "")
            {
                sql += " and trunc(TR_DATE) >= TWN_TODATE(:DiffDate_Start) and trunc(TR_DATE) <= TWN_TODATE(:DiffDate_End) ";
                p.Add(":DiffDate_Start", string.Format("{0}", DiffDate_Start));
                p.Add(":DiffDate_End", string.Format("{0}", DiffDate_End));
            }
            if (MMCODE_Start != "" & MMCODE_End != "")
            {
                sql += " and a.MMCODE >= :MMCODE_Start and a.MMCODE <= :MMCODE_End ";
                p.Add(":MMCODE_Start", string.Format("{0}", MMCODE_Start));
                p.Add(":MMCODE_End", string.Format("{0}", MMCODE_End));
            }
            sql += " order by MMCODE ";

            return DBWork.Connection.Query<AB0070>(sql, p);
        }

        /// <summary>
        /// 取得院內碼下拉式選單資料
        /// </summary>
        /// <param name="mmcode">院內碼，可對院內碼、院內碼中英文名稱進行模糊搜尋</param>
        /// <param name="page_index"></param>
        /// <param name="page_size"></param>
        /// <param name="sorters"></param>
        /// <returns></returns>
        public IEnumerable<MI_MAST> GetMMCODEComboOne(string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MMCODE, 
                    MMNAME_E,
                    MMNAME_C
                    FROM MI_MAST A
                    WHERE MAT_CLASS = '01' ";

            if (mmcode != "")
            {
                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

            }
            else
            {
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWH_NOComboOne()
        {
            var p = new DynamicParameters();

            string sql = @"select WH_NO as VALUE, WH_NO ||'_'||WH_NAME as COMBITEM
                            from MI_WHMAST
                            where WH_KIND='0' and (WH_GRADE='1' or WH_GRADE='2') 
                            order by WH_GRADE, WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetDiffClsComboOne()
        {
            var p = new DynamicParameters();

            string sql = @"select DOCNAME as COMBITEM, DOCTYPE as VALUE
                            from ME_DOCT
                            where length(DOCTYPE)=2
                            order by DOCTYPE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }
    }
}