using System;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace WebApp.Repository.F
{
    public class FA0038ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public double F6 { get; set; }
        public double F7 { get; set; }
        public double F8 { get; set; }
        public string F9 { get; set; }
        public string F10 { get; set; }
        public string F11 { get; set; }
        public string F12 { get; set; }

    }
    public class FA0038Repository : JCLib.Mvc.BaseRepository
    {
        public FA0038Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0038ReportMODEL> GetPrintDataA(string p1, string p2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT X.WH F1,X.WH_NAME F2,X.MMCODE F3,X.MMNAME_E F4,X.FLOW F5,
                        NVL(X.QTY,0)F6,NVL(X.M_CONTPRICE,0)F7,NVL(X.AMT,0)F8,TWN_DATE(X.APPTIME)F9,X.APPID F10,X.APPDEPT F11 
                        FROM (
	                        SELECT A.TOWH WH,
	                               WH_NAME(A.TOWH) WH_NAME,
	                               B.MMCODE,
	                               C.MMNAME_E,
	                               '撥入' FLOW,
	                               CASE WHEN B.APVQTY=0 THEN B.APPQTY END QTY,
	                               C.M_CONTPRICE,
	                               CASE WHEN B.APVQTY=0 THEN B.APPQTY END * C.M_CONTPRICE AMT,
	                               A.APPTIME,
	                               A.APPID,
	                               USER_NAME(A.APPID) APPDEPT   
	                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C, MI_WHMAST D
	                        WHERE A.DOCNO = B.DOCNO
	                        AND B.MMCODE = C.MMCODE
	                        AND A.TOWH = D.WH_NO
	                        AND D.WH_KIND = '0'
	                        AND D.WH_GRADE IN ('2','3','4')
	                        AND SUBSTR(FLOWID,1,2) >= '01'
	                        AND SUBSTR(FLOWID,1,2) <= '11'
	                        AND SUBSTR(FLOWID,3,2) = '99'
	                        AND SUBSTR(TWN_DATE(A.APPTIME),1,5) = '轉檔月份'
	                        UNION ALL
	                        SELECT A.FRWH WH,
	                               WH_NAME(A.FRWH) WH_NAME,
	                               B.MMCODE,
	                               C.MMNAME_E,
	                               '退回' FLOW,
	                               CASE WHEN B.APVQTY=0 THEN B.APPQTY END QTY,
	                               C.M_CONTPRICE,
	                               CASE WHEN B.APVQTY=0 THEN B.APPQTY END * C.M_CONTPRICE AMT,
	                               A.APPTIME,
	                               A.APPID,
	                               USER_NAME(A.APPID)   
	                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C, MI_WHMAST D
	                        WHERE A.DOCNO = B.DOCNO
	                        AND B.MMCODE = C.MMCODE
	                        AND C.MAT_CLASS = '01' 
	                        AND A.FRWH = D.WH_NO
	                        AND D.WH_KIND = '0'
	                        AND D.WH_GRADE IN ('2','3','4')
	                        AND SUBSTR(FLOWID,1,2) >= '01'
	                        AND SUBSTR(FLOWID,1,2) <= '11'
	                        AND SUBSTR(FLOWID,3,2) = '99'
                        ) X WHERE 1 = 1 
                        
                         ";

            if (p1 != "" & p2 != "")
            {
                p1 += "01";
                p2 += "31";
                sql += " AND TWN_DATE(X.APPTIME) BETWEEN :p1 AND :p2 ";
                p.Add(":p1", string.Format("{0}", p1));
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p1 != "" & p2 == "")
            {
                p1 += "01";
                sql += " AND TWN_DATE(X.APPTIME) >= :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (p1 == "" & p2 != "")
            {
                p2 += "31";
                sql += " AND TWN_DATE(X.APPTIME) <= :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }
            
            sql += " ORDER BY F1 ASC, F9 DESC";
            return DBWork.Connection.Query<FA0038ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcelA(string p1, string p2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT X.WH 庫別,X.WH_NAME 庫別名稱,X.MMCODE 院內碼,X.MMNAME_E 名稱,X.FLOW 類別,
                        NVL(X.QTY,0)數量,NVL(X.M_CONTPRICE,0)單價,NVL(X.AMT,0)小計,TWN_DATE(X.APPTIME)申請日期,X.APPID 建立者,
                        X.APPDEPT 姓名 
                        FROM (
	                        SELECT A.TOWH WH,
	                               WH_NAME(A.TOWH) WH_NAME,
	                               B.MMCODE,
	                               C.MMNAME_E,
	                               '撥入' FLOW,
	                               CASE WHEN B.APVQTY=0 THEN B.APPQTY END QTY,
	                               C.M_CONTPRICE,
	                               CASE WHEN B.APVQTY=0 THEN B.APPQTY END * C.M_CONTPRICE AMT,
	                               A.APPTIME,
	                               A.APPID,
	                               USER_NAME(A.APPID) APPDEPT   
	                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C, MI_WHMAST D
	                        WHERE A.DOCNO = B.DOCNO
	                        AND B.MMCODE = C.MMCODE
	                        AND C.MAT_CLASS = '01'
	                        AND A.TOWH = D.WH_NO
	                        AND D.WH_KIND = '0'
	                        AND D.WH_GRADE IN ('2','3','4')
	                        AND SUBSTR(FLOWID,1,2) >= '01'
	                        AND SUBSTR(FLOWID,1,2) <= '11'
	                        AND SUBSTR(FLOWID,3,2) = '99'
	                        AND SUBSTR(TWN_DATE(A.APPTIME),1,5) = '轉檔月份'
	                        UNION ALL
	                        SELECT A.FRWH WH,
	                               WH_NAME(A.FRWH) WH_NAME,
	                               B.MMCODE,
	                               C.MMNAME_E,
	                               '退回' FLOW,
	                               CASE WHEN B.APVQTY=0 THEN B.APPQTY END QTY,
	                               C.M_CONTPRICE,
	                               CASE WHEN B.APVQTY=0 THEN B.APPQTY END * C.M_CONTPRICE AMT,
	                               A.APPTIME,
	                               A.APPID,
	                               USER_NAME(A.APPID)   
	                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C, MI_WHMAST D
	                        WHERE A.DOCNO = B.DOCNO
	                        AND B.MMCODE = C.MMCODE
	                        AND A.FRWH = D.WH_NO
	                        AND D.WH_KIND = '0'
	                        AND D.WH_GRADE IN ('2','3','4')
	                        AND SUBSTR(FLOWID,1,2) >= '01'
	                        AND SUBSTR(FLOWID,1,2) <= '11'
	                        AND SUBSTR(FLOWID,3,2) = '99'
                        ) X WHERE 1 = 1 
                        
                         ";

            if (p1 != "" & p2 != "")
            {
                p1 += "01";
                p2 += "31";
                sql += " AND TWN_DATE(X.APPTIME) BETWEEN :p1 AND :p2 ";
                p.Add(":p1", string.Format("{0}", p1));
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p1 != "" & p2 == "")
            {
                p1 += "01";
                sql += " AND TWN_DATE(X.APPTIME) >= :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (p1 == "" & p2 != "")
            {
                p2 += "31";
                sql += " AND TWN_DATE(X.APPTIME) <= :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }

            sql += " ORDER BY 庫別 ASC, 申請日期 DESC";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<FA0038ReportMODEL> GetPrintDataB(string p1, string p2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT '' F1 ,'' F2, '' F3, '' F4, '' F5, 
                        '' F6 ,''F7,''F8,''F9, ''F10,
                        '' F11, '' F12
                    FROM DUAL WHERE 1 = 1 ";

            
            return DBWork.Connection.Query<FA0038ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcelB(string p1, string p2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT '' 庫別,'' 庫別名稱,'' 等級,'' 院內碼,'' 名稱,
                        '' 類別,'' 數量,'' 單價,'' 小計,'' 調帳日期,
                        '' 建立者,'' 姓名 
                        FROM DUAL WHERE 1 = 1 
                         ";
            
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<FA0038ReportMODEL> GetPrintDataC(string p1, string p2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT '' F1 ,'' F2, '' F3, '' F4, '' F5, 
                        '' F6 ,''F7,''F8,''F9, ''F10,
                        '' F11, '' F12, '' F13, '' F14 
                    FROM DUAL WHERE 1 = 1 ";


            return DBWork.Connection.Query<FA0038ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcelC(string p1, string p2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT '' SUPPLYNO,'' SUPPLYCHNNAME,'' BANKACCOUNT,'' ISLOCALBANK,'' BANKCODE,
                        '' BANKSUBCODE,'' SKORDERCODE,'' ORDERENGNAME,'' 應付金額,'' 進貨數量,
                        '' 優惠單價,'' 合約價,'' 合約碼,'' ACCOUNTDATE 
                        FROM DUAL WHERE 1 = 1 
                         ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<FA0038ReportMODEL> GetPrintDataD(string p1, string p2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT '' F1 ,'' F2, '' F3, '' F4, '' F5, 
                        '' F6 
                    FROM DUAL WHERE 1 = 1 ";


            return DBWork.Connection.Query<FA0038ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcelD(string p1, string p2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT '' SUPPLYNO,'' SUPPLYCHNNAME,'' SKORDERCODE,'' ORDERENGNAME,'' SUMINOUTAMOUNT,
                        '' MAMAGERATE  
                        FROM DUAL WHERE 1 = 1 
                         ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<COMBO_MODEL> GetKindCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='FA0038' AND DATA_NAME='KIND' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetWhmastCombo()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT ,
                        WH_NO || ' ' || WH_NAME as COMBITEM 
                        FROM MI_WHMAST
                        WHERE WH_KIND='0' AND WH_GRADE='3' 
                        ORDER BY WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

    }
}
