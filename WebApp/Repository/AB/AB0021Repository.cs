using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using JCLib.DB;
using JCLib.Mvc;
using Dapper;
using WebApp.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AB
{
    public class AB0021Repository : JCLib.Mvc.BaseRepository
    {
        public AB0021Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(ME_DOCM_QUERY_PARAMS query,string[] str_FLOWID, string[] str_matclass, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT DOCNO, TWN_DATE(APPTIME) AS APPTIME
                    , APPID || ' ' || USER_NAME(APPID) AS APP_NAME
                    , USER_NAME(APPID) AS APPID 
                    , APPDEPT || ' ' || INID_NAME(APPDEPT) AS APPDEPT_NAME
                    , (SELECT FLOWID || ' ' || FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID AND DOCTYPE=:DOCTYPE) FLOWID
                    , (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS
                    , FRWH || ' ' || WH_NAME(FRWH) FRWH
                    , TOWH || ' ' || WH_NAME(TOWH) TOWH
                    , APPLY_NOTE

                    FROM ME_DOCM A WHERE 1=1";

            if (query.APPDEPT != "")
            {
                sql += " AND A.APPDEPT = :APPDEPT ";
                p.Add(":APPDEPT", query.APPDEPT);
            }

            if (query.DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :DOCNO ";
                p.Add(":DOCNO", string.Format("%{0}%", query.DOCNO));
            }

            if (query.DOCTYPE != "")
            {
                sql += " AND A.DOCTYPE = :DOCTYPE ";
                p.Add(":DOCTYPE", string.Format("{0}", query.DOCTYPE));
            }

            //if (query.FLOWID != "")
            //{
            //    sql += " AND A.FLOWID = :FLOWID ";
            //    p.Add(":FLOWID", string.Format("{0}", query.FLOWID));
            //}
            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_FLOWID.Length > 0)
            {
                string sql_FLOWID = "";
                sql += @"AND (";
                foreach (string tmp_FLOWID in str_FLOWID)
                {
                    if (string.IsNullOrEmpty(sql_FLOWID))
                    {
                        sql_FLOWID = @"A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                    else
                    {
                        sql_FLOWID += @" OR A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                }
                sql += sql_FLOWID + ") ";
            }
            //if (query.MAT_CLASS != "")
            //{
            //    if (query.MAT_CLASS == "03-08")
            //        sql += " AND (A.MAT_CLASS = '03' or A.MAT_CLASS = '04' or A.MAT_CLASS = '05' or A.MAT_CLASS = '06' or A.MAT_CLASS = '07' or A.MAT_CLASS = '08' )";
            //    else
            //        sql += " AND A.MAT_CLASS = :MAT_CLASS ";
            //    p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
            //}
            if (str_matclass.Length > 0)
            {
                string sql_matclass = "";
                sql += @"AND (";
                foreach (string tmp_matclass in str_matclass)
                {
                    if (string.IsNullOrEmpty(sql_matclass))
                    {
                        sql_matclass = @"A.MAT_CLASS = '" + tmp_matclass + "'";
                    }
                    else
                    {
                        sql_matclass += @" OR A.MAT_CLASS = '" + tmp_matclass + "'";
                    }
                }
                sql += sql_matclass + ") ";
            }
            if (query.APPTIME_S != "" && query.APPTIME_E != "")
            {
                //sql += " AND TWN_TIME(A.APPTIME) BETWEEN :APPTIME_S || '000000' AND :APPTIME_E || '2399999'";
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE(:APPTIME_S, 'yyyy/mm/dd') AND TO_DATE(:APPTIME_E, 'yyyy/mm/dd')";
                p.Add(":APPTIME_S", string.Format("{0}", query.APPTIME_S));
                p.Add(":APPTIME_E", string.Format("{0}", query.APPTIME_E));
            }
            if (query.APPTIME_S != "" && query.APPTIME_E == "")
            {
                //sql += " AND TWN_TIME(A.APPTIME) BETWEEN :APPTIME_S || '000000' AND '9999999999999'";
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE(:APPTIME_S, 'yyyy/mm/dd') AND TO_DATE('3000/01/01', 'yyyy/mm/dd')";
                p.Add(":APPTIME_S", string.Format("{0}", query.APPTIME_S));
            }
            if (query.APPTIME_S == "" && query.APPTIME_E != "")
            {
                //sql += " AND TWN_TIME(A.APPTIME) BETWEEN '0000000000000' AND :APPTIME_E || '9999999'";
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE('1900/01/01', 'yyyy/mm/dd') AND TO_DATE(:APPTIME_E, 'yyyy/mm/dd')";
                p.Add(":APPTIME_E", string.Format("{0}", query.APPTIME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            //string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT, DATA_VALUE || ' ' || DATA_DESC AS COMBITEM
            //            FROM PARAM_D
            //            WHERE GRP_CODE='ME_DOCM' and DATA_NAME='FLOWID_RN1'
            //            order by DATA_VALUE";
            string sql = @"SELECT FLOWID AS VALUE, FLOWID || ' ' || FLOWNAME AS COMBITEM FROM ME_FLOW WHERE DOCTYPE='RN1'";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS
                        WHERE MAT_CLASS >='02' and MAT_CLASS <='08'
                        order by MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<ME_DOCD> GetMeDocd(ME_DOCD_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            //var sql = @"SELECT a.DOCNO, a.SEQ, a.MMCODE, a.APPQTY, a.APL_CONTIME, a.APLYITEM_NOTE, b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT
            //            FROM ME_DOCD a LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE WHERE 1=1 ";
            string sql = @"SELECT a.DOCNO, a.SEQ, a.MMCODE, a.APPQTY, a.APL_CONTIME, a.APLYITEM_NOTE, b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT,
                        CASE WHEN C.INV_QTY IS NULL THEN 0 ELSE C.INV_QTY END AS INV_QTY_FR,
                        CASE WHEN D.INV_QTY IS NULL THEN 0 ELSE D.INV_QTY END AS INV_QTY_TO
                        FROM ME_DOCD a LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE 
                        LEFT JOIN MI_WHINV C ON A.MMCODE=C.MMCODE AND C.WH_NO=:FRWH
                        LEFT JOIN MI_WHINV D ON A.MMCODE=D.MMCODE AND D.WH_NO=:TOWH
                        WHERE 1=1 ";

            if (query.DOCNO != "")
            {
                sql += " AND a.DOCNO=:p0 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO));
            }

            p.Add(":FRWH", string.Format("{0}", query.FRWH));
            p.Add(":TOWH", string.Format("{0}", query.TOWH));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string GeFrwh(string userId)
        {
            var sql = @"SELECT WH_NO FROM MI_WHMAST WHERE INID=USER_INID(:USERID) AND WH_KIND='1' AND WH_GRADE='2' and cancel_id = 'N'";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { USERID = userId }, DBWork.Transaction);
        }

        public string GeTowh()
        {
            var sql = @"SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1'";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public string GetMaxSeq(string docno)
        {
            var sql = @"SELECT MAX(SEQ) + 1 AS MAXSEQ FROM ME_DOCD WHERE DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        // 檢查是否為衛材中央庫房人員
        public bool IsWhGrade1(string wh_no)
        {
            string sql = @"select 1 from MI_WHMAST where WH_NO=:WH_NO AND WH_KIND='1' and WH_GRADE='1'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no }, DBWork.Transaction) == null);
        }
        // 檢查是否為中央庫房人員
        public bool IsWhGradeUser(string id)
        {
            string sql = @"SELECT 1 FROM MI_WHID A, MI_WHMAST B WHERE A.WH_NO=B.WH_NO AND A.WH_USERID=:TUSER AND B.WH_KIND='1' and B.WH_GRADE='1'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction) == null);
        }

        public IEnumerable<MI_WHID> GetWhTaskId(string userId)
        {
            //string sql = @"select TASK_ID from MI_WHID where WH_USERID=:WH_USERID AND WH_NO=WHNO_MM1";
            string sql = @"select WHM1_TASK(:WH_USERID) AS TASK_ID from MI_WHID";
            return DBWork.Connection.Query<MI_WHID>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }

        public string GetTaskid(string id)
        {
            string sql = @"select WHM1_TASK(:WH_USERID) AS TASK_ID from MI_WHID";
            string rtn = DBWork.Connection.QueryFirstOrDefault(sql, new { WH_USERID = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.M_CONTPRICE, A.BASE_UNIT FROM MI_MAST A
                        INNER JOIN MI_WHINV B ON A.MMCODE=B.MMCODE
                        WHERE B.INV_QTY>0 AND A.M_STOREID='1'"; // M_STOREID 0非庫備,1庫備

            if (query.MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
            }

            if (query.WH_NO != "")
            {
                sql += " AND B.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", query.WH_NO));
            }

            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string mat_class, string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A
                        INNER JOIN MI_WHINV B ON A.MMCODE=B.MMCODE
                        WHERE A.M_STOREID='1' ";
            if (mat_class == "02") {
                sql += " and B.INV_QTY>0 ";
            }
            if (mat_class != "")
            {
                sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", mat_class));
            }

            if (wh_no != "")
            {
                sql += " AND B.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> GetDocnoCombo(string appdept, string doctype)
        {
            var sql = @"SELECT DOCNO FROM ME_DOCM WHERE APPDEPT=:APPDEPT AND DOCTYPE=:DOCTYPE";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { APPDEPT = appdept, DOCTYPE = doctype }, DBWork.Transaction);
        }

        // 檢查是否有院內碼
        public bool CheckMmcodeExist(string mmcode, string mat_class, string wh_no)
        {
            string sql = @"SELECT 1 FROM MI_MAST A
                        INNER JOIN MI_WHINV B ON A.MMCODE=B.MMCODE
                        WHERE A.M_STOREID='1' and A.MMCODE=:MMCODE  AND A.MAT_CLASS=:MAT_CLASS AND B.WH_NO=:WH_NO";
            if (mat_class == "02") {
                sql += " and  B.INV_QTY>0";
            }
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode, MAT_CLASS = mat_class, WH_NO = wh_no }, DBWork.Transaction) == null);
        }

        public int MasterCreate(ME_DOCM me_docm)
        {
            var sql = @"INSERT INTO ME_DOCM (DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT, APPTIME, MAT_CLASS, APPLY_NOTE, FRWH, TOWH, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                    VALUES (:DOCNO, :DOCTYPE, :FLOWID, :APPID, :APPDEPT, SYSDATE, :MAT_CLASS, :APPLY_NOTE, :FRWH, :TOWH, SYSDATE
                                        , :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> MasterGet(string docno)
        {
            var sql = @" SELECT DOCNO FROM ME_DOCM WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int MasterUpdate(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET APPLY_NOTE=:APPLY_NOTE, UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP WHERE DOCNO=:DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int MasterDelete(string docno)
        {
            var sql = @" DELETE from ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int MasterReject(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID=:FLOWID, APPLY_NOTE=:APPLY_NOTE,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int DetailAllDelete(string docno)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DetailCreate(ME_DOCD me_docd)
        {
            var sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, APLYITEM_NOTE, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                    VALUES (:DOCNO, :SEQ, :MMCODE, :APPQTY, :APLYITEM_NOTE, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            //var sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, APLYITEM_NOTE)  
            //                        VALUES (:DOCNO, :SEQ, :MMCODE, :APPQTY, :APLYITEM_NOTE)";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailUpdate(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET MMCODE=:MMCODE, APPQTY=:APPQTY, APLYITEM_NOTE=:APLYITEM_NOTE, UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailDelete(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public int UpdateStatus(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCD SET APL_CONTIME = SYSDATE WHERE DOCNO = :DOCNO";
            DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
            sql = @"UPDATE ME_DOCM SET FLOWID = :FLOWID, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public SP_MODEL PostDoc(string docno, string updusr, string updip)
        {
            //OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString);
            //conn.Open();
            //OracleCommand cmd = new OracleCommand("POST_DOC", conn);
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.Add("I_DOCNO", OracleDbType.Varchar2).Value = docno;
            //cmd.Parameters.Add("I_UPDUSR", OracleDbType.Varchar2).Value = updusr;
            //cmd.Parameters.Add("I_UPDIP", OracleDbType.Varchar2).Value = updip;
            //cmd.Parameters.Add("O_RETID", OracleDbType.Varchar2, 1).Direction = ParameterDirection.Output;
            //cmd.Parameters.Add("O_ERRMSG", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;
            //cmd.ExecuteNonQuery();
            //SP_MODEL sp = new SP_MODEL
            //{
            //    O_RETID = cmd.Parameters["O_RETID"].Value.ToString(),
            //    O_ERRMSG = cmd.Parameters["O_ERRMSG"].Value.ToString()
            //};
            //conn.Close();

            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: docno, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: updusr, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: updip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = p.Get<OracleString>("O_ERRMSG").Value
            };

            return sp;
        }

        public class ME_DOCM_QUERY_PARAMS
        {
            public string DOCNO;
            public string DOCTYPE;
            public string APPDEPT;
            public string FLOWID;
            public string MAT_CLASS;

            public string APPTIME_S;
            public string APPTIME_E;
        }

        public class ME_DOCD_QUERY_PARAMS
        {
            public string DOCNO;
            public string FRWH;
            public string TOWH;
        }

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }

        public IEnumerable<COMBO_MODEL> GetFrwhCombo(string id)
        {
            string sql = @"select A.WH_NO VALUE , WH_NO||' '||WH_NAME TEXT ,A.WH_NO || ' ' || A.WH_NAME COMBITEM from MI_WHMAST A
                        WHERE WH_KIND='1'
                        AND EXISTS(SELECT 'X' FROM UR_ID B WHERE ( A.SUPPLY_INID=B.INID OR A.INID=B.INID ) AND TUSER=:TUSER)
                        AND NOT EXISTS(SELECT 'X' FROM MI_WHID B WHERE TASK_ID IN ('2','3') AND WH_USERID=:TUSER)
                        and cancel_id = 'N'
                        UNION ALL 
                        SELECT A.WH_NO ,A.WH_NO||' '||WH_NAME,A.WH_NO || ' ' || A.WH_NAME COMBITEM FROM MI_WHMAST A,MI_WHID B
                        WHERE A.WH_NO=B.WH_NO AND TASK_ID IN ('2','3') AND WH_USERID=:TUSER
                        and cancel_id = 'N'
                        ORDER BY 1";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }

        public IEnumerable<COMBO_MODEL> GetFrwhCombo2()
        {
            string sql = @"SELECT DISTINCT A.FRWH as VALUE, WH_NAME(A.FRWH) as TEXT,
                        A.FRWH || ' ' || WH_NAME(A.FRWH) as COMBITEM 
                        FROM ME_DOCM A WHERE A.DOCTYPE = 'RN1' ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<AB0003Model> GetLoginInfo(string id, string ip)
        {
            string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
                        WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
                        :UPDATE_IP,
                        (SELECT COUNT(*) AS CNT FROM ME_DOCM 
                            WHERE DOCTYPE='MR2' AND APPLY_KIND='1' 
                            AND APPTIME BETWEEN NEXT_DAY(SYSDATE-7,1) AND NEXT_DAY(SYSDATE-7,1)+6 ) MR2,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR3_EDAY')),'N') MR3,
                        NVL((SELECT 'Y' FROM DUAL WHERE (SELECT EXTRACT(DAY FROM SYSDATE) FROM DUAL) 
                            BETWEEN (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_BDAY')
                            AND (SELECT DATA_VALUE FROM PARAM_D WHERE DATA_NAME = 'MR4_EDAY')),'N') MR4 
                        FROM UR_ID
                        WHERE UR_ID.TUSER=:TUSER";

            return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id, UPDATE_IP = ip });
        }

        #region 2020-05-12 新增: 檢核該單位一年內, 曾經有調入量和核撥量記錄
        public bool CheckPastYear(string wh_no, string mmcode) {
            // 檢核本月是否有調入量和核撥量
            string sql = @"select 1 from MI_WHINV
                            where wh_no = :wh_no
                              and mmcode = :mmcode
                              and (trn_inqty > 0 or apl_inqty > 0)";
            bool result = !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         wh_no = wh_no,
                                                         mmcode = mmcode
                                                     },
                                                     DBWork.Transaction) == null);
            if (result) {
                return result;
            }


            // 一年內曾經有調入量和核撥量記錄
            sql = @"select 1 from MI_WINVMON
                            where wh_no = :wh_no
                              and mmcode = :mmcode
                              and data_ym >= (TWN_YYYMM (ADD_MONTHS (SYSDATE, -12)))
                              and (trn_inqty > 0 or apl_inqty > 0)";

            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         wh_no = wh_no,
                                                         mmcode = mmcode
                                                     },
                                                     DBWork.Transaction) == null);
        }
        #endregion

        #region 2021-07-30 新增: 新增主檔、送核撥 檢查申請庫房是否作廢
        public bool CheckIsTowhCancelByWhno(string towh)
        {
            string sql = @"
               select 1 from MI_WHMAST 
                where wh_no = :towh
                  and cancel_id = 'N'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { towh }, DBWork.Transaction) == null;
        }

        public bool CheckIsTowhCancelByDocno(string docno)
        {
            string sql = @"
               select 1 from ME_DOCM  a
                where a.docno = :docno
                  and exists (select 1 from MI_WHMAST 
                               where wh_no = a.frwh
                                 and cancel_id = 'N')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) == null;
        }
        #endregion
    }
}