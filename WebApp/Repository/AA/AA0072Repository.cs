using System;
using System.Collections.Generic;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using TSGH.Models;

namespace WebApp.Repository.AA
{
    public class AA0072ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string MAT_CLASS { get; set; }
        public string MAT_CLSNAME { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string MMNAME_C { get; set; }
        public string BASE_UNIT { get; set; }
        public string WH_NO { get; set; }
        public string WH_NAME { get; set; }
        public string DATA_YM { get; set; }
        public float APL_OUTQTY { get; set; }
        public float AVG_PRICE { get; set; }
        public float LUMP_SUM { get; set; }
    }

    public class AA0072ApplyMODEL : JCLib.Mvc.BaseModel
    {
        public string CLASS_WH { get; set; }
        public float APL_CNT { get; set; }
    }

    public class AA0072Repository : JCLib.Mvc.BaseRepository
    {
        public AA0072Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        // GetSqlstr和GetApplyData有取WH_NO全部選項做條件的處理,如這邊的規則有調整,另外兩個地方也需要連帶做調整
        public IEnumerable<MI_WHMAST> GetWH(string vtpe)
        {
            string sql = string.Empty;
            if (vtpe == "Y")
            {
                sql += " SELECT 'ALL_A' AS WH_NO,'台北門診全部' AS WH_NAME FROM DUAL ";
            }
            else
            {
                sql = @"SELECT WH_NO, WH_NAME " +
                        "FROM MI_WHMAST " +
                        "WHERE WH_KIND ='1' AND WH_GRADE = '2' ";
            }

            sql += " ORDER BY 1 ";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { WH_USERID = DBWork.UserInfo.UserId }, DBWork.Transaction);
        }

        public IEnumerable<AA0072ReportMODEL> GetQueryData(string pFromWhere, string pClass, string pFromYM, string pToYM, string pWhNo, string pMmCode, string pDocType, int page_index, int page_size, string sorters, string pformpgm)
        {
            DynamicParameters sqlParam = new DynamicParameters();
            string sqlStr = GetSqlstr(pFromWhere, pClass, pFromYM, pToYM, pWhNo, pMmCode, pDocType, pformpgm, out sqlParam);

            sqlParam.Add("OFFSET", (page_index - 1) * page_size);
            sqlParam.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0072ReportMODEL>((pFromWhere == "rdlc" ? sqlStr : GetPagingStatement(sqlStr, sorters)), sqlParam, DBWork.Transaction);
        }

        public DataTable GetExcelData(string fromWhere, string pClass, string pFromYM, string pToYM, string pWhNo, string pMmCode, string pDocType, string pPrintTitle, string pUserID, string pfrompgm)
        {
            DynamicParameters sqlParam = new DynamicParameters();
            string sqlStr = GetSqlstr(fromWhere, pClass, pFromYM, pToYM, pWhNo, pMmCode, pDocType, pfrompgm, out sqlParam);
            // string sqlStr2 = @"select Trim(INID_NAME(USER_INID(:USRID))) as INID_NAME_USER from PARAM_D where rownum = 1";
            DataTable dt = new DataTable();
            //string collumnName = DBWork.Connection.ExecuteScalar(sqlStr2, new { USRID = pUserID }, DBWork.Transaction).ToString() + pPrintTitle + "單位申領明細報表";

            //dt.Columns.Add(collumnName);

            using (var rdr = DBWork.Connection.ExecuteReader(sqlStr, sqlParam, DBWork.Transaction))
            {
                dt.Load(rdr);
            }

            return dt;
        }

        public String GetSqlstr(string fromWhere, string pClass, string pFromYM, string pToYM, string pWhNo, string pMmCode, string pDocType, string pFrompgm, out DynamicParameters pSqlParam)
        {
            pSqlParam = new DynamicParameters();
            StringBuilder sqlStr = new StringBuilder();


            Int16 nowYM = Convert.ToInt16((DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM")); //因 String 無法<、>、>=、<=等來比較, 故轉為
            string apptimeFrom = (Convert.ToInt16(pFromYM.Substring(0, 3)) + 1911).ToString() + pFromYM.Substring(3, 2);
            string apptimeTo = (Convert.ToInt16(pToYM.Substring(0, 3)) + 1911).ToString() + pToYM.Substring(3, 2);
            string orderByStr = "ORDER BY 7, 1, 3, 9";//7=WH_NO, 1=A.MAT_CLASS, 3=A.MMCODE, 9=C.DATA_YM
            string sqlStrNow = @"SELECT A.MAT_CLASS, " +
                                "    (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS MAT_CLSNAME, " +
                                "    A.MMCODE, A.MMNAME_E, A.MMNAME_C, A.BASE_UNIT, B.WH_NO, " +
                                "    (SELECT WH_NAME FROM MI_WHMAST WHERE WH_NO = B.WH_NO) AS WH_NAME, ";

            sqlStrNow += @" C.DATA_YM, B.APL_INQTY as APL_OUTQTY, C.AVG_PRICE, B.APL_INQTY * C.AVG_PRICE LUMP_SUM "; // 衛星庫房的撥發量相當於接收量，故 撥發總量(APL_OUTQTY)  改為讀取 入庫總量(APL_INQTY)

            string sqlStrBefore = sqlStrNow;

            if (fromWhere == "xls")
            {
                sqlStrNow = @"SELECT B.WH_NO AS 庫房代碼, ";

                sqlStrNow += @" (SELECT WH_NAME FROM MI_WHMAST WHERE WH_NO = B.WH_NO) AS 庫房名稱, ";

                sqlStrNow += @" A.MAT_CLASS AS 物料分類, " +
                            "    (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS 物料分類名稱, " +
                            "   A.MMCODE AS 院內碼, A.MMNAME_C AS 中文品名, A.MMNAME_E AS 英文品名, A.BASE_UNIT AS 計量單位,  ";

                sqlStrNow += @" C.DATA_YM AS 撥發年月,B.APL_INQTY AS 核撥總量, C.AVG_PRICE AS 撥發單價, B.APL_INQTY * C.AVG_PRICE AS 撥發成本 "; // 衛星庫房的撥發量相當於接收量，故 撥發總量(APL_OUTQTY)  改為讀取 入庫總量(APL_INQTY)

                sqlStrBefore = sqlStrNow;
            }

            // 庫房代碼下拉選單取得的全部選項
            string sqlStrGetWhno = "";
            if (pFrompgm == "AA0072")
            {
                sqlStrGetWhno = @" AND B.WH_NO in (SELECT WH_NO " +
                        " FROM MI_WHMAST " +
                        " WHERE WH_KIND ='1' AND WH_GRADE = '2')";
            }
            else if (pFrompgm == "AB0063")
            {
                sqlStrGetWhno = @"AND B.WH_NO in (SELECT DISTINCT WH_NO " +
                        " FROM MI_WHID A " +
                        " WHERE WH_USERID = '" + DBWork.UserInfo.UserId + "' " +
                        " and (select 1 from MI_WHMAST where WH_NO=A.WH_NO and WH_KIND='1')=1 " +
                        " UNION " +
                        " SELECT DISTINCT WH_NO " +
                        " FROM MI_WHMAST " +
                        " WHERE INID = USER_INID('" + DBWork.UserInfo.UserId + "') AND WH_KIND = '1') ";
            }

            sqlStrNow += @"FROM MI_MAST A, MI_WHINV B, MI_WHCOST C " +
                            "WHERE 1 = 1 " +
                            (pDocType == "2" ? " " : " AND A.M_STOREID = :DOCTYPE ") +
                            "    AND A.MMCODE = B.MMCODE " +
                            "    AND A.MMCODE = C.MMCODE " +
                            (pWhNo.Length > 0 ? (pWhNo == "ALL_A" ? " AND B.WH_NO LIKE '____A%'" : (pWhNo == "全部" ? sqlStrGetWhno : "  AND B.WH_NO = :WH_NO ")) : sqlStrGetWhno) +
                            "    AND C.DATA_YM = '" + nowYM.ToString() + "' ";
            sqlStrBefore += @"FROM MI_MAST A, MI_WINVMON B, MI_WHCOST C " +
                            "WHERE 1 = 1 " +
                            (pDocType == "2" ? " " : " AND A.M_STOREID = :DOCTYPE ") +
                            "    AND A.MMCODE = B.MMCODE " +
                            "    AND A.MMCODE = C.MMCODE " +
                            (pWhNo.Length > 0 ? (pWhNo == "ALL_A" ? " AND B.WH_NO LIKE '____A%'" : (pWhNo == "全部" ? sqlStrGetWhno : "  AND B.WH_NO = :WH_NO ")) : sqlStrGetWhno) +
                            "    AND B.DATA_YM >= :FromYM AND B.DATA_YM <= :ToYM " +
                            "    AND B.DATA_YM = C.DATA_YM ";
            string[] vMatClass;
            if (pClass != null)
            {
                if (pClass.Length > 0)
                {
                    vMatClass = pClass.Split(',');
                    string sql_MAT_CLASS = "";
                    sqlStrNow += @"AND (";
                    sqlStrBefore += @"AND (";
                    foreach (string tmp_MAT_CLASS in vMatClass)
                    {
                        if (string.IsNullOrEmpty(sql_MAT_CLASS))
                        {
                            sql_MAT_CLASS = @"A.MAT_CLASS = '" + tmp_MAT_CLASS + "'";
                        }
                        else
                        {
                            sql_MAT_CLASS += @" OR A.MAT_CLASS = '" + tmp_MAT_CLASS + "'";
                        }
                    }
                    sqlStrNow += sql_MAT_CLASS + ") ";
                    sqlStrBefore += sql_MAT_CLASS + ") ";
                }
            }
            if (!string.IsNullOrWhiteSpace(pMmCode))
            {
                sqlStrNow += @" AND A.MMCODE  = :MMCODE ";
                sqlStrBefore += @" AND A.MMCODE = :MMCODE ";
            }
            if (pFrompgm == "AB0063")
            {
                sqlStrNow += @" AND B.APL_INQTY > 0 
                                AND EXISTS (
                                SELECT WH_NO FROM MI_WHID X WHERE X.WH_USERID = '" + DBWork.UserInfo.UserId + "' AND X.WH_NO=B.WH_NO " +
                                " UNION " +
                                " SELECT WH_NO FROM MI_WHMAST X WHERE X.INID = USER_INID('" + DBWork.UserInfo.UserId + "') AND X.WH_KIND = '1' AND X.WH_NO=B.WH_NO " +
                                ") ";

                sqlStrBefore += @" AND B.APL_INQTY > 0 
                                AND EXISTS (
                                SELECT WH_NO FROM MI_WHID X WHERE X.WH_USERID = '" + DBWork.UserInfo.UserId + "' AND X.WH_NO=B.WH_NO " +
                                " UNION " +
                                " SELECT WH_NO FROM MI_WHMAST X WHERE X.INID = USER_INID('" + DBWork.UserInfo.UserId + "') AND X.WH_KIND = '1' AND X.WH_NO=B.WH_NO " +
                                ") ";
            }
            else if (pFrompgm == "AA0072")
            {
                sqlStrNow += @" AND B.APL_INQTY > 0 
                                AND EXISTS ( SELECT WH_NO FROM MI_WHMAST X WHERE X.WH_KIND ='1' AND X.WH_GRADE = '2' AND X.WH_NO=B.WH_NO ) ";

                sqlStrBefore += @" AND B.APL_INQTY > 0 
                                AND EXISTS ( SELECT WH_NO FROM MI_WHMAST X WHERE X.WH_KIND ='1' AND X.WH_GRADE = '2' AND X.WH_NO=B.WH_NO ) ";
            }

            if (Convert.ToInt16(pToYM) >= nowYM)
            {
                pToYM = nowYM.ToString();
            }

            //起訖年月皆大於當日時, 在JS檔顯示查無資料
            //起始年月等於當時, 且訖止年月大於或等於當時, 則起訖皆視為當月, 只取當月資料
            //起始年月小於當時, 且訖止年月大於或等於當時, 則取當月及過去資料
            //起始年月小於當時, 且訖止年月小於當時, 則取過去資料
            if (nowYM == Convert.ToInt16(pToYM))
            {
                sqlStr.Append(sqlStrNow);
            }

            if (nowYM > Convert.ToInt16(pFromYM))
            {
                if (sqlStr.ToString() != "")
                {
                    sqlStr.Append("UNION ALL ");
                }

                sqlStr.Append(sqlStrBefore);
            }

            sqlStr.Append(orderByStr);

            //pSqlParam.Add("MATCLASS", vMatClass);
            pSqlParam.Add("FromYM", pFromYM);
            pSqlParam.Add("ToYM", pToYM);
            pSqlParam.Add("DOCTYPE", pDocType);
            pSqlParam.Add("WH_NO", pWhNo);
            pSqlParam.Add("MMCODE", pMmCode);

            return sqlStr.ToString();
        }

        public IEnumerable<AA0072ApplyMODEL> GetApplyData(string pClass, string pFromYM, string pToYM, string pWhNo, string pDocType, string pFrompgm)
        {
            DynamicParameters sqlParam = new DynamicParameters();
            StringBuilder sqlStr = new StringBuilder();
            Int16 nowYM = Convert.ToInt16((DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM")); //因 String 無法<、>、>=、<=等來比較, 故轉為
            string apptimeFrom = (Convert.ToInt16(pFromYM.Substring(0, 3)) + 1911).ToString() + pFromYM.Substring(3, 2);
            string apptimeTo = (Convert.ToInt16(pToYM.Substring(0, 3)) + 1911).ToString() + pToYM.Substring(3, 2);
            string[] vMatClass = pClass.Split(',');
            string[] vWhNo = pWhNo.Split(','); ;

            // 庫房代碼下拉選單取得的全部選項
            string sqlStrGetWhno = "";
            if (pFrompgm == "AA0072")
            {
                sqlStrGetWhno = @" AND TOWH in (SELECT WH_NO " +
                        " FROM MI_WHMAST " +
                        " WHERE WH_KIND ='1' AND WH_GRADE = '2')";
            }
            else if (pFrompgm == "AB0063")
            {
                sqlStrGetWhno = @" AND TOWH in (SELECT DISTINCT WH_NO " +
                        " FROM MI_WHID A " +
                        " WHERE WH_USERID = '" + DBWork.UserInfo.UserId + "' " +
                        " and (select 1 from MI_WHMAST where WH_NO=A.WH_NO and WH_KIND='1')=1 " +
                        " UNION " +
                        " SELECT DISTINCT WH_NO " +
                        " FROM MI_WHMAST " +
                        " WHERE INID = USER_INID('" + DBWork.UserInfo.UserId + "') AND WH_KIND = '1') ";
            }


            sqlStr.Append("SELECT MAT_CLASS || TOWH AS CLASS_WH, COUNT(1) AS APL_CNT " +
                        "FROM ME_DOCM " +
                        "WHERE TO_CHAR(APPTIME, 'YYYYMM') >= '" + apptimeFrom + "' " +
                        "    AND TO_CHAR(APPTIME, 'YYYYMM') <= '" + apptimeTo + "' " +
                        (pClass.Length > 0 ? "    AND MAT_CLASS IN :MATCLASS " : "") +
                        (pDocType.Length > 0 ? (pDocType == "1" ? "    AND DOCTYPE IN('MR1', 'MR2') " : "    AND DOCTYPE IN('MR3', 'MR4') ") : "") +
                        //(pWhNo.Length > 0 ? "    AND TOWH = :WH_NO " : "") +
                        (pWhNo.Length > 0 ? (pWhNo == "ALL_A" ? " AND TOWH LIKE '____A%'" : ((pWhNo == "全部" || pWhNo == "") ? sqlStrGetWhno : " AND TOWH = :WH_NO ")) : sqlStrGetWhno) +
                        "GROUP BY MAT_CLASS || TOWH " +
                        "ORDER BY 1");

            sqlParam.Add("MATCLASS", vMatClass);
            sqlParam.Add("FromYM", pFromYM);
            sqlParam.Add("ToYM", pToYM);
            sqlParam.Add("DOCTYPE", pDocType);
            sqlParam.Add("WH_NO", vWhNo);

            return DBWork.Connection.Query<AA0072ApplyMODEL>(sqlStr.ToString(), sqlParam, DBWork.Transaction);
        }

        public string getInidName(string pUserID)
        {
            var p = new DynamicParameters();
            var sql = @" select Trim(INID_NAME(USER_INID(:USRID))) as INID_NAME_USER from PARAM_D where rownum = 1 ";

            return DBWork.Connection.ExecuteScalar(sql, new { USRID = pUserID }, DBWork.Transaction).ToString();
        }

        #region 2020-05-18 從AA0048改放入AA0072
        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS WHERE MAT_CLSID IN ('2','3','6') 
                        order by mat_class";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        #endregion

        public string GetUridInid(string id)
        {
            string sql = @"SELECT INID FROM UR_ID WHERE TUSER=:TUSER ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public bool Checkwhno(string id)
        {
            string sql = @"SELECT 1 FROM PARAM_D WHERE GRP_CODE='AB0063' AND DATA_NAME='WH_NO' AND DATA_VALUE=:DATA_VALUE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DATA_VALUE = id }, DBWork.Transaction) == null);
        }
    }
}