using System;
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
    public class AB0098Repository : JCLib.Mvc.BaseRepository
    {
        public AB0098Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(ME_DOCM_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DOCNO, TWN_DATE(APPTIME) AS APPTIME
                    , (SELECT FLOWID || ' ' || FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID) FLOWID
                    , (SELECT FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID) FLOWID_N
                    , FRWH || ' ' || WH_NAME(FRWH) AS FRWH_NAME
                    , FRWH
                    FROM ME_DOCM A WHERE 1=1 ";

            if (query.USERNAME != "汪佳蓉" && query.USERID != "admin") {
                sql += "  and exists (select 1 from MI_WHMAST where wh_no = a.frwh and wh_kind = '0')";
            }

            if (query.FLOWID != "")
            {
                sql += " AND A.FLOWID=:FLOWID ";
                p.Add(":FLOWID", query.FLOWID);
            }

            if (query.DOCTYPE != "")
            {
                sql += " AND A.DOCTYPE=:DOCTYPE ";
                p.Add(":DOCTYPE", query.DOCTYPE);
            }

            if (query.FRWH != "")
            {
                sql += " AND A.FRWH=:FRWH ";
                p.Add(":FRWH", query.FRWH);
            }

            if (query.APPTIME_S != "" && query.APPTIME_E != "")
            {
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE(:APPTIME_S, 'yyyy/mm/dd') AND TO_DATE(:APPTIME_E, 'yyyy/mm/dd')";
                p.Add(":APPTIME_S", string.Format("{0}", query.APPTIME_S));
                p.Add(":APPTIME_E", string.Format("{0}", query.APPTIME_E));
            }
            if (query.APPTIME_S != "" && query.APPTIME_E == "")
            {
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE(:APPTIME_S, 'yyyy/mm/dd') AND TO_DATE('3000/01/01', 'yyyy/mm/dd')";
                p.Add(":APPTIME_S", string.Format("{0}", query.APPTIME_S));
            }
            if (query.APPTIME_S == "" && query.APPTIME_E != "")
            {
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE('1900/01/01', 'yyyy/mm/dd') AND TO_DATE(:APPTIME_E, 'yyyy/mm/dd')";
                p.Add(":APPTIME_E", string.Format("{0}", query.APPTIME_E));
            }

            return DBWork.PagingQuery<ME_DOCM>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetAllMeDocd(ME_DOCD_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.DOCNO, A.MMCODE, A.SEQ, A.APVQTY, A.APLYITEM_NOTE, A.INV_QTY, A.UP, A.AMT
                        , A.TRANSKIND
                        , (SELECT DATA_VALUE || ' ' || DATA_DESC AS COMBITEM FROM PARAM_D WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='TRANSKIND' AND DATA_VALUE=A.TRANSKIND) as TRANSKIND_NAME
                        , TWN_DATE(A.APVTIME) AS APVTIME
                        , TWN_DATE(A.ACKTIME) AS ACKTIME
                        , b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT FROM ME_DOCD a 
                        LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE
                        INNER JOIN ME_DOCM C ON A.DOCNO=C.DOCNO
                        WHERE 1=1 ";

            if (query.DOCNO != "")
            {
                sql += " AND a.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO));
            }

            if (query.SEQ != null && query.SEQ != "")
            {
                sql += " AND a.SEQ = :p1 ";
                p.Add(":p1", string.Format("{0}", query.SEQ));
            }


            return DBWork.PagingQuery<ME_DOCD>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHID> GetFrwhCombo(string userid)
        {
            var sql = @"select WH_NO, WH_NO || ' ' || wh_name(WH_NO) AS WH_NAME from MI_WHID where wh_userid=:WH_USERID AND TASK_ID='1'";

            if (userid == "A0004739" || userid == "admin") {
                sql += @"
                    union 
                select WH_NO, WH_NO || ' ' || wh_name(WH_NO) AS WH_NAME 
                  from MI_WHMAST 
                 where wh_kind = '1'
                   and wh_grade = '2'
                ";
            }

            return DBWork.Connection.Query<MI_WHID>(sql, new { WH_USERID = userid}, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetFlowidCombo(string doctype)
        {
            var sql = @"select FLOWID as VALUE,FLOWNAME as TEXT, FLOWID || ' ' || FLOWNAME as COMBITEM from ME_FLOW where doctype=:DOCTYPE";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { DOCTYPE = doctype }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetTranskindCombo()
        {
            var sql = @"SELECT DATA_VALUE AS VALUE, DATA_VALUE || ' ' || DATA_DESC AS COMBITEM FROM PARAM_D WHERE GRP_CODE='ME_DOCD' AND DATA_NAME='TRANSKIND'";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public int MasterCreate(ME_DOCM me_docm)
        {
            var sql = @"INSERT INTO ME_DOCM (DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT, APPTIME, TOWH, FRWH, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, MAT_CLASS)  
                                    VALUES (:DOCNO, :DOCTYPE, :FLOWID, :APPID, :APPDEPT, SYSDATE, :TOWH, :FRWH, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :MAT_CLASS)";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> MasterGet(string docno)
        {
            var sql = @" SELECT DOCNO, TWN_DATE(APPTIME) AS APPTIME
                    , (SELECT FLOWID || ' ' || FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID AND DOCTYPE='AJ') FLOWID
                    , (SELECT FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID ) FLOWID_N 
                    , FRWH || ' ' || WH_NAME(FRWH) AS FRWH_NAME
                    , FRWH  
                    FROM ME_DOCM A WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int MasterUpdate(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FRWH=:FRWH, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int MasterDelete(string docno)
        {
            var sql = @" DELETE from ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DetailAllDelete(string docno)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DetailCreate(ME_DOCD me_docd)
        {
            var sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APVQTY, TRANSKIND, APLYITEM_NOTE, INV_QTY, UP, AMT, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                    VALUES (:DOCNO, :SEQ, :MMCODE, :APVQTY, :TRANSKIND, :APLYITEM_NOTE, :INV_QTY, :UP, :AMT, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailUpdate(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET MMCODE=:MMCODE, APVQTY=:APVQTY, TRANSKIND=:TRANSKIND, INV_QTY=:INV_QTY, APLYITEM_NOTE=:APLYITEM_NOTE, UP=:UP, AMT=:AMT, UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                        WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailDelete(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public bool CheckMeDocdExists(string docno)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
        }

        public string GetFrwh(string docno)
        {
            string sql = @"SELECT FRWH FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string Get_INV_QTY(string wh_no, string mmcode)
        {
            string sql = @"SELECT INV_QTY FROM MI_WHINV WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT, A.M_CONTPRICE FROM MI_MAST A INNER JOIN MI_WINVCTL B ON A.MMCODE=B.MMCODE WHERE 1=1 ";
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

                sql += " AND (upper(A.MMCODE) LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR upper(A.MMNAME_E) LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR upper(A.MMNAME_C) LIKE :MMNAME_C) ";
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

        public SP_MODEL PostDoc(string docno, string updusr, string updip)
        {
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
            public string APPID;
            public string APPDEPT;
            public string FLOWID;
            public string FRWH;
            public string TOWH;

            public string APPTIME_S;
            public string APPTIME_E;

            public string USERID;
            public string USERNAME;
        }

        public class ME_DOCD_QUERY_PARAMS
        {
            public string DOCNO;
            public string SEQ;
        }

        public bool HasDetail(string docno) {
            string sql = @"select count(*) from ME_DOCD where docno = :docno";
            return (DBWork.Connection.QueryFirstOrDefault<int>(sql, new { docno = docno }, DBWork.Transaction)) > 0;
        }
        public bool HasNoteEmpty(string docno) {
            string sql = @"select count(*) from ME_DOCD 
                            where docno = :docno 
                              and trim(aplyitem_note) is null";
            return (DBWork.Connection.QueryFirstOrDefault<int>(sql, new { docno = docno }, DBWork.Transaction)) > 0;
        }
        public bool HasZero(string docno)
        {
            string sql = @"select count(*) from ME_DOCD 
                            where docno = :docno 
                              and apvqty = 0";
            return (DBWork.Connection.QueryFirstOrDefault<int>(sql, new { docno = docno }, DBWork.Transaction)) > 0;
        }

        #region 20201-05-06新增: 修改、刪除時檢查flowId
        public bool ChceckFlowId02(string docno)
        {
            string sql = @"
                select 1 from ME_DOCM
                 where docno = :docno
                   and flowId = '1702'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) != null;
        }
        #endregion

        public string GetHospCode() {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
        public string GetDailyDocno()
        {
            string sql = @"select GET_DAILY_DOCNO from DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
    }
}