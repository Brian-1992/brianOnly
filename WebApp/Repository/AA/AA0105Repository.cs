using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;

namespace WebApp.Repository.AA
{
    public class AA0105Repository : JCLib.Mvc.BaseRepository
    {
        public AA0105Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_WINVCTL> GetAll(string wh_no, string mmcode, string Search_Type, string MatClass, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = "";

            //查詢基準量
            if (Search_Type == "1")
            {
                sql += @"SELECT A.WH_NO, A.MMCODE, A.SAFE_DAY, A.OPER_DAY, A.SHIP_DAY,
                     A.DAVG_USEQTY, A.HIGH_QTY, A.LOW_QTY, A.MIN_ORDQTY, A.SUPPLY_WHNO,
                    (SELECT SAFE_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) SAFE_QTY,
                    (SELECT OPER_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) OPER_QTY,
                    (SELECT SHIP_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) SHIP_QTY,
                    (SELECT AVG_USEQTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) AVG_USEQTY, 
                    (SELECT WH_NAME(A.WH_NO) FROM DUAL) WH_NAME, " +
                    "(SELECT MMNAME_C FROM MI_MAST WHERE MMCODE = A.MMCODE) MMNAME_C, " +
                    "(SELECT MMNAME_E FROM MI_MAST WHERE MMCODE = A.MMCODE) MMNAME_E, " +
                    "(SELECT BASE_UNIT FROM MI_MAST WHERE MMCODE = A.MMCODE) BASE_UNIT, " +
                    "(SELECT APL_INQTY FROM MI_WHINV WHERE WH_NO = A.WH_NO AND MMCODE = A.MMCODE) APL_INQTY " +
                    "FROM MI_WINVCTL a WHERE 1 = 1 " +
                    "AND EXISTS (SELECT 1 FROM MI_MAST WHERE MMCODE = A.MMCODE AND MAT_CLASS = :MatClass)";
            }
            //查詢超出
            else if (Search_Type == "2")
            {
                sql += @"SELECT A.DOCNO, B.MMCODE, 
                    (SELECT MMNAME_C FROM MI_MAST WHERE MMCODE = B.MMCODE) MMNAME_C,
                    (SELECT MMNAME_E FROM MI_MAST WHERE MMCODE = B.MMCODE) MMNAME_E,
                    (SELECT BASE_UNIT FROM MI_MAST WHERE MMCODE = B.MMCODE) BASE_UNIT,
                    (SELECT HIGH_QTY FROM MI_WINVCTL WHERE WH_NO = A.TOWH AND MMCODE = B.MMCODE) HIGH_QTY,
                    (SELECT APL_INQTY FROM MI_WHINV WHERE WH_NO = A.TOWH AND MMCODE = B.MMCODE) APL_INQTY,
                    B.APPQTY,
                    (SELECT AVG_PRICE FROM MI_WHCOST WHERE DATA_YM = SUBSTR(TWN_DATE(SYSDATE),1,5) AND MMCODE = B.MMCODE) AVG_PRICE,
                    A.TOWH WH_NO,
                    (SELECT WH_NAME(A.TOWH) FROM DUAL) WH_NAME,
                    (SELECT DATA_DESC FROM PARAM_D WHERE DATA_NAME = 'GTAPL_REASON' AND DATA_VALUE = B.GTAPL_RESON) GTAPL_REASON
                    FROM ME_DOCM A JOIN ME_DOCD B
                    ON A.DOCNO = B.DOCNO AND B.GTAPL_RESON IS NOT NULL
                    WHERE 1 = 1
                    AND EXTRACT( MONTH FROM A.APPTIME ) = EXTRACT( MONTH FROM SYSDATE )
                    AND EXISTS (SELECT 1 FROM MI_MAST WHERE MMCODE = B.MMCODE AND MAT_CLASS = :MatClass)";
            }

            p.Add(":MatClass", string.Format("{0}", MatClass));

            if (wh_no != "")
            {
                //基準量找WH_NO，超出量找A.TOWH
                sql += Search_Type == "1" ? " AND WH_NO = :p0 " : " AND A.TOWH = :p0 ";
                p.Add(":p0", string.Format("{0}", wh_no));
            }
            if (mmcode != "")
            {
                sql += " AND MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WINVCTL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_WINVCTL> SearchReportData_Basic(string wh_no, string mmcode, string MatClass)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.WH_NO, A.MMCODE, A.SAFE_DAY, A.OPER_DAY, A.SHIP_DAY,
                     A.DAVG_USEQTY, A.HIGH_QTY, A.LOW_QTY, A.MIN_ORDQTY, A.SUPPLY_WHNO,
                    (SELECT SAFE_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) SAFE_QTY,
                    (SELECT OPER_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) OPER_QTY,
                    (SELECT SHIP_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) SHIP_QTY,
                    (SELECT AVG_USEQTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) AVG_USEQTY, 
                    (SELECT WH_NAME(A.WH_NO) FROM DUAL) WH_NAME, " +
                    "(SELECT MMNAME_C FROM MI_MAST WHERE MMCODE = A.MMCODE) MMNAME_C, " +
                    "(SELECT MMNAME_E FROM MI_MAST WHERE MMCODE = A.MMCODE) MMNAME_E, " +
                    "(SELECT BASE_UNIT FROM MI_MAST WHERE MMCODE = A.MMCODE) BASE_UNIT, " +
                    "(SELECT APL_INQTY FROM MI_WHINV WHERE WH_NO = A.WH_NO AND MMCODE = A.MMCODE) APL_INQTY " +
                    "FROM MI_WINVCTL a WHERE 1 = 1 " +
                    "AND EXISTS (SELECT 1 FROM MI_MAST WHERE MMCODE = A.MMCODE AND MAT_CLASS = :MatClass)";

            p.Add(":MatClass", string.Format("{0}", MatClass));

            if (wh_no != "")
            {
                sql += " AND WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", wh_no));
            }
            if (mmcode != "")
            {
                sql += " AND MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }

            return DBWork.Connection.Query<MI_WINVCTL>(sql, p);
        }

        public IEnumerable<MI_WINVCTL> SearchReportData_Over(string wh_no, string mmcode, string MatClass)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO, B.MMCODE, 
                    (SELECT MMNAME_C FROM MI_MAST WHERE MMCODE = B.MMCODE) MMNAME_C,
                    (SELECT MMNAME_E FROM MI_MAST WHERE MMCODE = B.MMCODE) MMNAME_E,
                    (SELECT BASE_UNIT FROM MI_MAST WHERE MMCODE = B.MMCODE) BASE_UNIT,
                    (SELECT HIGH_QTY FROM MI_WINVCTL WHERE WH_NO = A.TOWH AND MMCODE = B.MMCODE) HIGH_QTY,
                    (SELECT APL_INQTY FROM MI_WHINV WHERE WH_NO = A.TOWH AND MMCODE = B.MMCODE) APL_INQTY,
                    B.APPQTY,
                    (SELECT AVG_PRICE FROM MI_WHCOST WHERE DATA_YM = SUBSTR(TWN_DATE(SYSDATE),1,5) AND MMCODE = B.MMCODE) AVG_PRICE,
                    A.TOWH WH_NO,
                    (SELECT WH_NAME(A.TOWH) FROM DUAL) WH_NAME,
                    (SELECT DATA_DESC FROM PARAM_D WHERE DATA_NAME = 'GTAPL_REASON' AND DATA_VALUE = B.GTAPL_RESON) GTAPL_REASON
                    FROM ME_DOCM A JOIN ME_DOCD B
                    ON A.DOCNO = B.DOCNO AND B.GTAPL_RESON IS NOT NULL
                    WHERE 1 = 1
                    AND EXTRACT( MONTH FROM A.APPTIME ) = EXTRACT( MONTH FROM SYSDATE )
                    AND EXISTS (SELECT 1 FROM MI_MAST WHERE MMCODE = B.MMCODE AND MAT_CLASS = :MatClass)";

            p.Add(":MatClass", string.Format("{0}", MatClass));

            if (wh_no != "")
            {
                sql += " AND A.TOWH = :p0 ";
                p.Add(":p0", string.Format("{0}", wh_no));
            }
            if (mmcode != "")
            {
                sql += " AND B.MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }

            return DBWork.Connection.Query<MI_WINVCTL>(sql, p);
        }

        public DataTable GetExcel(string wh_no, string mmcode, string Search_Type, string MatClass)
        {
            var p = new DynamicParameters();

            var sql = "";

            //查詢基準量
            if (Search_Type == "1")
            {
                sql += @"SELECT A.WH_NO 庫房代碼, A.MMCODE 院內碼, A.SAFE_DAY 安全日, A.OPER_DAY 作業日, A.SHIP_DAY 運補日,
                     A.DAVG_USEQTY 日平均消耗量, A.HIGH_QTY 基準量, A.LOW_QTY 最低庫存量, A.MIN_ORDQTY 最小撥補量, 
                    (SELECT SAFE_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) 安全量,
                    (SELECT OPER_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) 作業量,
                    (SELECT SHIP_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) 運補量,
                    (SELECT AVG_USEQTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) 日平均消耗量, 
                    (SELECT WH_NAME(A.WH_NO) FROM DUAL) 庫房名稱, " +
                    "(SELECT MMNAME_C FROM MI_MAST WHERE MMCODE = A.MMCODE) 中文品名, " +
                    "(SELECT MMNAME_E FROM MI_MAST WHERE MMCODE = A.MMCODE) 英文品名, " +
                    "(SELECT BASE_UNIT FROM MI_MAST WHERE MMCODE = A.MMCODE) 計量單位, " +
                    "(SELECT APL_INQTY FROM MI_WHINV WHERE WH_NO = A.WH_NO AND MMCODE = A.MMCODE) 累計撥發量 " +
                    "FROM MI_WINVCTL a WHERE 1 = 1 " +
                    "AND EXISTS (SELECT 1 FROM MI_MAST WHERE MMCODE = A.MMCODE AND MAT_CLASS = :MatClass)";
            }
            //查詢超出
            else if (Search_Type == "2")
            {
                sql += @"SELECT A.DOCNO 申請單號, B.MMCODE 院內碼, 
                    (SELECT MMNAME_C FROM MI_MAST WHERE MMCODE = B.MMCODE) 中文品名,
                    (SELECT MMNAME_E FROM MI_MAST WHERE MMCODE = B.MMCODE) 英文品名,
                    (SELECT BASE_UNIT FROM MI_MAST WHERE MMCODE = B.MMCODE) 計量單位,
                    (SELECT HIGH_QTY FROM MI_WINVCTL WHERE WH_NO = A.TOWH AND MMCODE = B.MMCODE) 基準量,
                    (SELECT APL_INQTY FROM MI_WHINV WHERE WH_NO = A.TOWH AND MMCODE = B.MMCODE) 累計撥發量,
                    B.APPQTY 申請量,
                    (SELECT AVG_PRICE FROM MI_WHCOST WHERE DATA_YM = SUBSTR(TWN_DATE(SYSDATE),1,5) AND MMCODE = B.MMCODE) 庫存單價,
                    A.TOWH 庫房代碼,
                    (SELECT WH_NAME(A.TOWH) FROM DUAL) 庫房名稱,
                    (SELECT DATA_DESC FROM PARAM_D WHERE DATA_NAME = 'GTAPL_REASON' AND DATA_VALUE = B.GTAPL_RESON) 超量原因 
                    FROM ME_DOCM A JOIN ME_DOCD B
                    ON A.DOCNO = B.DOCNO AND B.GTAPL_RESON IS NOT NULL
                    WHERE 1 = 1
                    AND EXTRACT( MONTH FROM A.APPTIME ) = EXTRACT( MONTH FROM SYSDATE )
                    AND EXISTS (SELECT 1 FROM MI_MAST WHERE MMCODE = B.MMCODE AND MAT_CLASS = :MatClass)";
            }

            p.Add(":MatClass", string.Format("{0}", MatClass));

            if (wh_no != "")
            {
                //基準量找WH_NO，超出量找A.TOWH
                sql += Search_Type == "1" ? " AND WH_NO = :p0 " : " AND A.TOWH = :p0 ";
                p.Add(":p0", string.Format("{0}", wh_no));
            }
            if (mmcode != "")
            {
                sql += " AND MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //     public IEnumerable<MI_WINVCTL> Get(string wh_no, string mmcode)
        //     {
        //         var sql = @"SELECT A.WH_NO, A.MMCODE, A.SAFE_DAY, A.OPER_DAY, A.SHIP_DAY,
        //      A.DAVG_USEQTY, A.HIGH_QTY, A.LOW_QTY, A.MIN_ORDQTY, A.SUPPLY_WHNO,
        //     (SELECT SAFE_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) SAFE_QTY,
        //     (SELECT OPER_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) OPER_QTY,
        //     (SELECT SHIP_QTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) SHIP_QTY,
        //     (SELECT AVG_USEQTY FROM V_MM_WHINVCTL WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) AVG_USEQTY
        //FROM MI_WINVCTL a WHERE 1=1 AND A.WH_NO = :WH_NO and A.MMCODE = :MMCODE";
        //         return DBWork.Connection.Query<MI_WINVCTL>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        //     }

        public IEnumerable<COMBO_MODEL> GetWH_NOComboOne()
        {
            var p = new DynamicParameters();

            string sql = @"SELECT WH_NO as VALUE, 
                            WH_NAME as COMBITEM 
                            from MI_WHMAST WHERE WH_GRADE='2' AND WH_KIND = '1'  ORDER BY WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT {0} A.MMCODE , A.MMNAME_C, A.MMNAME_E " +
                "from MI_MAST A JOIN MI_WINVCTL B ON (A.MMCODE = B.MMCODE) WHERE B.WH_NO = :WH_NO ";

            if (mmcode != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", mmcode);
                p.Add(":MMNAME_E_I", mmcode);
                p.Add(":MMNAME_C_I", mmcode);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add(":WH_NO", wh_no);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS WHERE MAT_CLSID IN ('2','3') ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
    }

}