using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;
using System.Linq;

namespace WebApp.Repository.AB
{
    public class AB0077Repository : JCLib.Mvc.BaseRepository
    {
        public AB0077Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0077> GetAll(string WH_NO, string DiffCls, string TR_IO, string DiffDate_Start,
            string DiffDate_End, string CONTorDISCOUNT, string CONTID, string MMCODE_Start, string MMCODE_End, string tr_mcode, string tr_docno)
        {
            var p = new DynamicParameters();

            var sql = @"select TR_DOCNO, WH_NO, TWN_TIME(TR_DATE) T_DATE, (to_char(TR_DATE,'HH24')||to_char(TR_DATE,'MI')||to_char(TR_DATE,'SS')) T_HM, 
                               TR_DOCNAME(TR_DOCNO) TR_DOCTYPE, 
                               a.MMCODE, b.MMNAME_E, b.BASE_UNIT, TR_IO, TR_INV_QTY, 
                               (select UPRICE*TR_INV_QTY from MI_WHCOST 
                               where DATA_YM=(TO_CHAR(TR_DATE,'YYYYMM')-191100) and MMCODE=a.MMCODE) as T_MONEY, --金額
                               (select TOWH from ME_DOCM where DOCNO=TR_DOCNO) TOWH, --入庫別
                               (select FRWH from ME_DOCM where DOCNO=TR_DOCNO) FRWH, --出庫別
                               (select APLYITEM_NOTE from ME_DOCD where DOCNO = TR_DOCNO and MMCODE= a.MMCODE) T_NOTES, --備註
                               DOCEXP_LOT(TR_DOCNO, TR_DOCSEQ) as LOTNO_EXP_QTY,   -- 批號-校旗-數量
                               (case when TR_DOCTYPE='IN' then 
                                       (select UNA from UR_ID 
                                         where TUSER=(select UPDATE_USER from ME_DOCM
                                                       where DOCNO = TR_DOCNO))       
                                     else
                                       (select UNA from UR_ID 
                                         where TUSER=(select UPDATE_USER from ME_DOCD 
                                                       where DOCNO = TR_DOCNO and MMCODE= a.MMCODE))
                                end) as T_OP,  --操作者
                               CONT_PRICE(SUBSTR(TWN_DATE(TR_DATE),1,5),A.MMCODE) CONT_PRICE, --單價
                               APPID(TR_DOCNO) APPID, --領藥人
                               (select MCODE_NAME from MI_MCODE where MCODE=a.TR_MCODE) as TR_MCODE  --庫存異動
                          from MI_WHTRNS a, MI_MAST b
                         where a.MMCODE = b.MMCODE
                           and a.TR_MCODE NOT IN ('WAYI','WAYO')";

            if (WH_NO != "")
            {
                //庫別下拉選單為「全部藥局」(All-DS)
                if (WH_NO == "All-DS")
                {
                    sql += " and WH_NO in (select WH_NO from MI_WHMAST where WH_KIND='0' and WH_GRADE='2') ";
                }
                //庫別下拉選單為「全部藥局_藥庫」(ALL)
                else if (WH_NO == "ALL")
                {
                    sql += " and WH_NO in (select WH_NO from MI_WHMAST where WH_KIND='0' and WH_GRADE in ('1','2')) ";
                }
                else
                {
                    sql += " AND WH_NO = :WH_NO ";
                    p.Add(":WH_NO", string.Format("{0}", WH_NO));
                }
            }
            if (DiffCls.Trim() != "")
            {
                sql += " and TR_DOCTYPE = :DiffCls ";
                p.Add(":DiffCls", string.Format("{0}", DiffCls.Trim()));
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
                //sql += " and trunc(TR_DATE) >= TWN_TODATE(:DiffDate_Start) and trunc(TR_DATE) <= TWN_TODATE(:DiffDate_End) ";
                sql += @" AND (
                                (TWN_DATE(TR_DATE) >= :DiffDate_Start and TWN_DATE(TR_DATE) <= :DiffDate_End AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                (TWN_DATE(TR_DATE) >= :DiffDate_Start and TWN_DATE(TR_DATE) <= :DiffDate_End AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                (TWN_DATE(TR_DATE) >= :DiffDate_Start and TWN_DATE(TR_DATE) <= :DiffDate_End AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                (SUBSTR(TR_DOCNO,1,7) >= :DiffDate_Start and SUBSTR(TR_DOCNO,1,7) <= :DiffDate_End AND TR_MCODE='USEO') OR
                                (SUBSTR(TR_DOCNO,1,7) >= :DiffDate_Start and SUBSTR(TR_DOCNO,1,7) <= :DiffDate_End AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                (SUBSTR(TR_DOCNO,1,7) >= :DiffDate_Start and SUBSTR(TR_DOCNO,1,7) <= :DiffDate_End AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                               )
                ";
                p.Add(":DiffDate_Start", string.Format("{0}", DiffDate_Start));
                p.Add(":DiffDate_End", string.Format("{0}", DiffDate_End));
            }
            if (CONTID != "")
            {
                // A: 合約品項：MI_MAST.CONTRACNO IN ('1','2','01','02')
                if (CONTID == "A")
                {
                    sql += " and b.CONTRACNO IN ('1','2','01','02', '3', '03') ";
                }
                // B: 非合約品項：MI_MAST.CONTRACNO IN ('0Y','0N')
                else if (CONTID == "B")
                {
                    sql += " and b.CONTRACNO IN ('0Y','0N') ";
                }
                // C.其它：005CYC01、005FLO06、005STR07、005TUB04的MI_MAST.CONTRACNO='X'
                else if (CONTID == "C") {
                    sql += " and (b.CONTRACNO not IN ('1','2','01','02', '0Y','0N', '3', '03') or " +
                        "         b.CONTRACNO is null) ";
                }
            }
            if (MMCODE_Start != "")
            {
                sql += " and a.MMCODE >= :MMCODE_Start ";
                p.Add(":MMCODE_Start", string.Format("{0}", MMCODE_Start));
            }
            if (MMCODE_End != "")
            {
                sql += " and a.MMCODE >= :MMCODE_Start and a.MMCODE <= :MMCODE_End ";
                p.Add(":MMCODE_End", string.Format("{0}", MMCODE_End));
            }
            if (tr_mcode != string.Empty) {
                sql += "  and a.tr_mcode = :tr_mcode";
                p.Add(":tr_mcode", string.Format("{0}", tr_mcode));
            }
            if (tr_docno != string.Empty)
            {
                sql += "  and a.tr_docno like :tr_docno";
                p.Add(":tr_docno", string.Format("%{0}%", tr_docno));
            }

            // sql += " order by TR_DOCTYPE, TOWH, MMCODE ";

            return DBWork.PagingQuery<AB0077>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AB0077> GetReport(string WH_NO, string DiffCls, string TR_IO, string DiffDate_Start,
            string DiffDate_End, string CONTorDISCOUNT, string CONTID, string MMCODE_Start, string MMCODE_End, string tr_mcode, string tr_docno)
        {
            var p = new DynamicParameters();

            var sql = @"select TR_DOCNO, WH_NO, TWN_DATE(TR_DATE) T_DATE, 
                               (to_char(TR_DATE,'HH24')||to_char(TR_DATE,'MI')||to_char(TR_DATE,'SS')) T_HM, 
                               TR_DOCNAME(TR_DOCNO) TR_DOCTYPE, 
                               a.MMCODE, b.MMNAME_E, b.BASE_UNIT, TR_IO, TR_INV_QTY, 
                               (select UPRICE*TR_INV_QTY from MI_WHCOST 
                               where DATA_YM=(TO_CHAR(TR_DATE,'YYYYMM')-191100) and MMCODE=a.MMCODE) as T_MONEY, --金額
                               (select TOWH from ME_DOCM where DOCNO=TR_DOCNO) TOWH, --入庫別
                               (select FRWH from ME_DOCM where DOCNO=TR_DOCNO) FRWH, --出庫別
                               (select APLYITEM_NOTE from ME_DOCD where DOCNO = TR_DOCNO and MMCODE= a.MMCODE) T_NOTES, --備註
                               DOCEXP_LOT(TR_DOCNO, TR_DOCSEQ) as LOTNO_EXP_QTY,   -- 批號-校旗-數量
                               (case when TR_DOCTYPE='IN' then 
                                       (select UNA from UR_ID 
                                         where TUSER=(select UPDATE_USER from ME_DOCM
                                                       where DOCNO = TR_DOCNO))       
                                     else
                                       (select UNA from UR_ID 
                                         where TUSER=(select UPDATE_USER from ME_DOCD 
                                                       where DOCNO = TR_DOCNO and MMCODE= a.MMCODE))
                                end) as T_OP,  --操作者
                               CONT_PRICE(SUBSTR(TWN_DATE(TR_DATE),1,5),A.MMCODE) CONT_PRICE, --單價
                               USER_NAME(APPID(TR_DOCNO)) APPID, --領藥人
                               (select MCODE_NAME from MI_MCODE where MCODE=a.TR_MCODE) as TR_MCODE  --庫存異動
                          from MI_WHTRNS a, MI_MAST b
                         where a.MMCODE = b.MMCODE
                           and a.TR_MCODE NOT IN ('WAYI','WAYO')";

            if (WH_NO != "")
            {
                //庫別下拉選單為「全部藥局」(All-DS)
                if (WH_NO == "All-DS")
                {
                    sql += " and WH_NO in (select WH_NO from MI_WHMAST where WH_KIND='0' and WH_GRADE='2') ";
                }
                //庫別下拉選單為「全部藥局_藥庫」(ALL)
                else if (WH_NO == "ALL")
                {
                    sql += " and WH_NO in (select WH_NO from MI_WHMAST where WH_KIND='0' and WH_GRADE in ('1','2')) ";
                }
                else
                {
                    sql += " AND WH_NO = :WH_NO ";
                    p.Add(":WH_NO", string.Format("{0}", WH_NO));
                }
            }
            if (DiffCls.Trim() != "")
            {
                sql += " and TR_DOCTYPE = :DiffCls ";
                p.Add(":DiffCls", string.Format("{0}", DiffCls.Trim()));
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
                sql += @" AND (
                                (TWN_DATE(TR_DATE) >= :DiffDate_Start and TWN_DATE(TR_DATE) <= :DiffDate_End AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                (TWN_DATE(TR_DATE) >= :DiffDate_Start and TWN_DATE(TR_DATE) <= :DiffDate_End AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                (TWN_DATE(TR_DATE) >= :DiffDate_Start and TWN_DATE(TR_DATE) <= :DiffDate_End AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                (SUBSTR(TR_DOCNO,1,7) >= :DiffDate_Start and SUBSTR(TR_DOCNO,1,7) <= :DiffDate_End AND TR_MCODE='USEO') OR
                                (SUBSTR(TR_DOCNO,1,7) >= :DiffDate_Start and SUBSTR(TR_DOCNO,1,7) <= :DiffDate_End AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                (SUBSTR(TR_DOCNO,1,7) >= :DiffDate_Start and SUBSTR(TR_DOCNO,1,7) <= :DiffDate_End AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                               )
                ";
                p.Add(":DiffDate_Start", string.Format("{0}", DiffDate_Start));
                p.Add(":DiffDate_End", string.Format("{0}", DiffDate_End));
            }
            if (CONTID != "")
            {
                // A: 合約品項：MI_MAST.CONTRACNO IN ('1','2','01','02')
                if (CONTID == "A")
                {
                    sql += " and b.CONTRACNO IN ('1','2','01','02', '3', '03') ";
                }
                // B: 非合約品項：MI_MAST.CONTRACNO IN ('0Y','0N')
                else if (CONTID == "B")
                {
                    sql += " and b.CONTRACNO IN ('0Y','0N') ";
                }
                // C.其它：005CYC01、005FLO06、005STR07、005TUB04的MI_MAST.CONTRACNO='X'
                else if (CONTID == "C")
                {
                    sql += " and (b.CONTRACNO not IN ('1','2','01','02', '0Y','0N', '3', '03') or " +
                        "         b.CONTRACNO is null) ";
                }
            }
            if (MMCODE_Start != "")
            {
                sql += " and a.MMCODE >= :MMCODE_Start ";
                p.Add(":MMCODE_Start", string.Format("{0}", MMCODE_Start));
            }
            if (MMCODE_End != "")
            {
                sql += " and a.MMCODE >= :MMCODE_Start and a.MMCODE <= :MMCODE_End ";
                p.Add(":MMCODE_End", string.Format("{0}", MMCODE_End));
            }
            if (tr_mcode != string.Empty)
            {
                sql += "  and a.tr_mcode = :tr_mcode";
                p.Add(":tr_mcode", string.Format("{0}", tr_mcode));
            }
            if (tr_docno != string.Empty)
            {
                sql += "  and a.tr_docno like :tr_docno";
                p.Add(":tr_docno", string.Format("%{0}%", tr_docno));
            }
            sql += " order by TR_DOCTYPE, TOWH, MMCODE ";

            return DBWork.Connection.Query<AB0077>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string WH_NO, string DiffCls, string TR_IO, string DiffDate_Start,
            string DiffDate_End, string CONTorDISCOUNT, string CONTID, string MMCODE_Start, string MMCODE_End, string tr_mcode, string tr_docno)
        {
            var p = new DynamicParameters();

            var sql = @"select tr_docno as 表單號碼,
                               (select MCODE_NAME from MI_MCODE where MCODE=a.TR_MCODE) as 庫存異動,
                               TWN_DATE(TR_DATE) 異動日期, (to_char(TR_DATE,'HH24')||to_char(TR_DATE,'MI')||to_char(TR_DATE,'SS')) 異動時間, 
                               TR_DOCNAME(TR_DOCNO) as 類別, 
                               a.MMCODE, b.MMNAME_E as 藥品名稱, b.BASE_UNIT as 單位, TR_INV_QTY, 
                               (select UPRICE*TR_INV_QTY from MI_WHCOST 
                               where DATA_YM=(TO_CHAR(TR_DATE,'YYYYMM')-191100) and MMCODE=a.MMCODE) as 金額, --金額
                               (select TOWH from ME_DOCM where DOCNO=TR_DOCNO) 入庫別, --入庫別
                               (select FRWH from ME_DOCM where DOCNO=TR_DOCNO) 出庫別, --出庫別
                               (select APLYITEM_NOTE from ME_DOCD where DOCNO = TR_DOCNO and MMCODE= a.MMCODE) 備註, --備註
                               DOCEXP_LOT(TR_DOCNO, TR_DOCSEQ) as ""批號-效期-數量"",   -- 批號-校旗-數量
                               (case when TR_DOCTYPE='IN' then 
                                       (select UNA from UR_ID 
                                         where TUSER=(select UPDATE_USER from ME_DOCM
                                                       where DOCNO = TR_DOCNO))       
                                     else
                                       (select UNA from UR_ID 
                                         where TUSER=(select UPDATE_USER from ME_DOCD 
                                                       where DOCNO = TR_DOCNO and MMCODE= a.MMCODE))
                                end) 操作者,  --操作者
                               CONT_PRICE(SUBSTR(TWN_DATE(TR_DATE),1,5),A.MMCODE) 單價, --單價
                               USER_NAME(APPID(TR_DOCNO)) 領藥人 --領藥人
                          from MI_WHTRNS a, MI_MAST b
                         where a.MMCODE = b.MMCODE
                           and a.TR_MCODE NOT IN ('WAYI','WAYO')";

            if (WH_NO != "")
            {
                //庫別下拉選單為「全部藥局」(All-DS)
                if (WH_NO == "All-DS")
                {
                    sql += " and WH_NO in (select WH_NO from MI_WHMAST where WH_KIND='0' and WH_GRADE='2') ";
                }
                //庫別下拉選單為「全部藥局_藥庫」(ALL)
                else if (WH_NO == "ALL")
                {
                    sql += " and WH_NO in (select WH_NO from MI_WHMAST where WH_KIND='0' and WH_GRADE in ('1','2')) ";
                }
                else
                {
                    sql += " AND WH_NO = :WH_NO ";
                    p.Add(":WH_NO", string.Format("{0}", WH_NO));
                }
            }
            if (DiffCls.Trim() != "")
            {
                sql += " and TR_DOCTYPE = :DiffCls ";
                p.Add(":DiffCls", string.Format("{0}", DiffCls.Trim()));
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
                sql += @" AND (
                                (TWN_DATE(TR_DATE) >= :DiffDate_Start and TWN_DATE(TR_DATE) <= :DiffDate_End AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                (TWN_DATE(TR_DATE) >= :DiffDate_Start and TWN_DATE(TR_DATE) <= :DiffDate_End AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                (TWN_DATE(TR_DATE) >= :DiffDate_Start and TWN_DATE(TR_DATE) <= :DiffDate_End AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                (SUBSTR(TR_DOCNO,1,7) >= :DiffDate_Start and SUBSTR(TR_DOCNO,1,7) <= :DiffDate_End AND TR_MCODE='USEO') OR
                                (SUBSTR(TR_DOCNO,1,7) >= :DiffDate_Start and SUBSTR(TR_DOCNO,1,7) <= :DiffDate_End AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                (SUBSTR(TR_DOCNO,1,7) >= :DiffDate_Start and SUBSTR(TR_DOCNO,1,7) <= :DiffDate_End AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                               )
                ";
                p.Add(":DiffDate_Start", string.Format("{0}", DiffDate_Start));
                p.Add(":DiffDate_End", string.Format("{0}", DiffDate_End));
            }
            if (CONTID != "")
            {
                // A: 合約品項：MI_MAST.CONTRACNO IN ('1','2','01','02')
                if (CONTID == "A")
                {
                    sql += " and b.CONTRACNO IN ('1','2','01','02', '3', '03') ";
                }
                // B: 非合約品項：MI_MAST.CONTRACNO IN ('0Y','0N')
                else if (CONTID == "B")
                {
                    sql += " and b.CONTRACNO IN ('0Y','0N') ";
                }
                // C.其它：005CYC01、005FLO06、005STR07、005TUB04的MI_MAST.CONTRACNO='X'
                else if (CONTID == "C")
                {
                    sql += " and (b.CONTRACNO not IN ('1','2','01','02', '0Y','0N', '3', '03') or " +
                        "         b.CONTRACNO is null) ";
                }
            }
            if (MMCODE_Start != "")
            {
                sql += " and a.MMCODE >= :MMCODE_Start ";
                p.Add(":MMCODE_Start", string.Format("{0}", MMCODE_Start));
            }
            if (MMCODE_End != "")
            {
                sql += " and a.MMCODE >= :MMCODE_Start and a.MMCODE <= :MMCODE_End ";
                p.Add(":MMCODE_End", string.Format("{0}", MMCODE_End));
            }
            if (tr_mcode != string.Empty)
            {
                sql += "  and a.tr_mcode = :tr_mcode";
                p.Add(":tr_mcode", string.Format("{0}", tr_mcode));
            }
            if (tr_docno != string.Empty)
            {
                sql += "  and a.tr_docno like :tr_docno";
                p.Add(":tr_docno", string.Format("%{0}%", tr_docno));
            }
            sql += " order by 類別, 入庫別, MMCODE ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
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

            string sql = @"select DATA_VALUE as VALUE, DATA_DESC as COMBITEM
                            from PARAM_D
                            where GRP_CODE='MI_WHMAST' and DATA_NAME='WH_GRADE0077'
                            order by DATA_SEQ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetDiffClsComboOne()
        {
            var p = new DynamicParameters();

            string sql = @"select '全部' COMBITEM, ' ' VALUE from dual 
                            union all
                            select DOCTYPE_NAME as COMBITEM, DOCTYPE as VALUE
                            from MI_DOCTYPE
                             where DOCTYPE_NAME not like '一般物品%'
                               and DOCTYPE_NAME not like '衛材%'
                               and DOCTYPE_NAME not like '能設%'
                               and DOCTYPE_NAME not like '通信%'
                               and DOCTYPE_NAME not like '氣體%' 
                             order by COMBITEM
                            ";

            List<COMBO_MODEL> list = DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction).ToList();

            list = list.OrderBy(x => x.VALUE).Select(x=>x).ToList();
            return list;
        }

        public IEnumerable<COMBO_MODEL> GetMiMcodes() {
            string sql = @"select MCODE as value, MCODE_NAME as text
                             from MI_MCODE
                            order by MCODE
                           ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction).ToList();
        }
    }
}