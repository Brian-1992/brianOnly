using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AB
{
    public class AB0062ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F01 { get; set; }
        public string F02 { get; set; }
        public string F03 { get; set; }
        public string F04 { get; set; }
        public string F05 { get; set; }
        public string F06 { get; set; }
        public string F07 { get; set; }
        public string F08 { get; set; }
        public float F09 { get; set; }
        public string F10 { get; set; }
        public float F11 { get; set; }
        public string F12 { get; set; }
        public float F13 { get; set; }
    }
    public class AB0062Repository : JCLib.Mvc.BaseRepository
    {
        public AB0062Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0062M> GetAllM(string apptime1, string apptime2, string mat_class, string mmcode, string wh_no, string whtype, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO,
                        A.FRWH,(SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                        A.TOWH,(SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        C.MAT_CLASS,(SELECT C.MAT_CLASS|| ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=C.MAT_CLASS) MAT_CLASS_N,
                        B.MMCODE,C.MMNAME_C,C.MMNAME_E,(C.MMNAME_C||' '||C.MMNAME_E) AS MMNAME_CE,
                        C.BASE_UNIT,A.APPTIME,TWN_DATE(A.APPTIME) APPTIME_T,
                        B.APPQTY,B.ACKTIME,TWN_DATE(B.ACKTIME) ACKTIME_T,
                        B.ACKQTY,B.APVTIME,TWN_DATE(B.APVTIME) APVTIME_T,B.APVQTY 
                        FROM ME_DOCM A,ME_DOCD B,MI_MAST C
                        WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                        AND A.DOCTYPE IN ('TR1') AND A.FLOWID='0299' ";
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(A.APPTIME) BETWEEN :p0 AND :p1";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND  TWN_DATE(A.APPTIME) >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(A.APPTIME) <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (mat_class != "")
            {
                sql += " AND C.MAT_CLASS = :p2 ";
                p.Add(":p2", string.Format("{0}", mat_class));
            }
            if (mmcode != "")
            {
                sql += " AND B.MMCODE = :p3 ";
                p.Add(":p3", string.Format("{0}", mmcode));
            }
            if (wh_no != "")
            {
                if (whtype == "all")
                {
                    sql += " AND ( A.FRWH = :p4 OR A.TOWH = :p4 ) ";
                }
                else
                {
                    if (whtype == "towh")
                    {
                        sql += " AND ( A.TOWH = :p4 ) ";
                    }
                    else
                    {
                        sql += " AND ( A.FRWH = :p4 ) ";
                    }
                }
                p.Add(":p4", string.Format("{0}", wh_no));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0062M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<AB0062ReportMODEL> GetPrintData(string apptime1, string apptime2, string mat_class, string mmcode, string wh_no, string whtype)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO AS F01,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) F02,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) F03,
                        (SELECT C.MAT_CLASS|| ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=C.MAT_CLASS) F04,
                        B.MMCODE AS F05,(C.MMNAME_C||' '||C.MMNAME_E) AS F06,
                        C.BASE_UNIT AS F07,TWN_DATE(A.APPTIME) F08,
                        B.APPQTY F09,TWN_DATE(B.ACKTIME) F10,
                        B.ACKQTY AS F11,TWN_DATE(B.APVTIME) F12,B.APVQTY AS F13
                        FROM ME_DOCM A,ME_DOCD B,MI_MAST C
                        WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                        AND A.DOCTYPE IN ('TR1') AND A.FLOWID='0299'  ";
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(A.APPTIME) BETWEEN :p0 AND :p1";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND  TWN_DATE(A.APPTIME) >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(A.APPTIME) <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (mat_class != "")
            {
                sql += " AND C.MAT_CLASS = :p2 ";
                p.Add(":p2", string.Format("{0}", mat_class));
            }
            if (mmcode != "")
            {
                sql += " AND B.MMCODE = :p3 ";
                p.Add(":p3", string.Format("{0}", mmcode));
            }
            if (wh_no != "")
            {
                if (whtype == "all")
                {
                    sql += " AND ( A.FRWH = :p4 OR A.TOWH = :p4 ) ";
                }
                else
                {
                    if (whtype == "towh")
                    {
                        sql += " AND ( A.TOWH = :p4 ) ";
                    }
                    else
                    {
                        sql += " AND ( A.FRWH = :p4 ) ";
                    }
                }
                p.Add(":p4", string.Format("{0}", wh_no));
            }

            return DBWork.Connection.Query<AB0062ReportMODEL>(sql, p, DBWork.Transaction);
        }
        public string GetTaskid(string id)
        {
            string sql = @"SELECT TASK_ID FROM MI_WHID WHERE WH_USERID=:WH_USERID 
                            AND WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<COMBO_MODEL> GetWhnoCombo(string inid)
        {

            var p = new DynamicParameters();

            string sql = @"SELECT WH_NO AS VALUE,WH_NAME AS TEXT, WH_NO || ' ' || WH_NAME as COMBITEM  
                        FROM MI_WHMAST  
                        WHERE 1 = 1 AND WH_KIND ='1' AND WH_GRADE ='2' ";
            if (inid != "")
            {
                sql += " AND INID = :inid ";
                p.Add(":inid", string.Format("{0}", inid));
            }
            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }
   
        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, string p1,  int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A WHERE 1=1  ";
            if (p1 != "")
            {
                sql += " AND mat_class = :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }
            
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public string GetTwnsystime()
        {
            string sql = @"SELECT TWN_SYSTIME FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE  MAT_CLSID IN ('1','2','3','6','7')     
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        //匯出
        public DataTable GetExcel(string apptime1, string apptime2, string mat_class, string mmcode, string wh_no, string whtype)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO 申請單號,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) 調出庫,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) 調入庫,
                        (SELECT C.MAT_CLASS|| ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=C.MAT_CLASS) 物料分類,
                        B.MMCODE 院內碼,(C.MMNAME_C||' '||C.MMNAME_E) AS 品名,
                        C.BASE_UNIT 計量單位,TWN_DATE(A.APPTIME) 申請日期,
                        B.APPQTY 申請數量,TWN_DATE(B.ACKTIME) 調入日期,
                        B.ACKQTY 調入數量,TWN_DATE(B.APVTIME) 調出日期,
                        B.APVQTY 調出數量 
                        FROM ME_DOCM A,ME_DOCD B,MI_MAST C
                        WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                        AND A.DOCTYPE IN ('TR1') AND A.FLOWID='0299' ";
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(A.APPTIME) BETWEEN :p0 AND :p1";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND  TWN_DATE(A.APPTIME) >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(A.APPTIME) <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (mat_class != "")
            {
                sql += " AND C.MAT_CLASS = :p2 ";
                p.Add(":p2", string.Format("{0}", mat_class));
            }
            if (mmcode != "")
            {
                sql += " AND B.MMCODE = :p3 ";
                p.Add(":p3", string.Format("{0}", mmcode));
            }
            if (wh_no != "")
            {
                if (whtype == "all")
                {
                    sql += " AND ( A.FRWH = :p4 OR A.TOWH = :p4 ) ";
                }
                else
                {
                    if (whtype == "towh")
                    {
                        sql += " AND ( A.TOWH = :p4 ) ";
                    }
                    else
                    {
                        sql += " AND ( A.FRWH = :p4 ) ";
                    }
                }
                p.Add(":p4", string.Format("{0}", wh_no));
            }
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}
