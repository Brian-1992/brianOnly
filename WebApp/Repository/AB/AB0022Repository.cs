using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0022Repository : JCLib.Mvc.BaseRepository
    {
        public AB0022Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(ME_DOCM_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DOCNO, TWN_DATE(APPTIME) AS APPTIME
                    , APPID || ' ' || USER_NAME(APPID) AS APP_NAME
                    , USER_NAME(APPID) AS APPID 
                    , APPDEPT || ' ' || INID_NAME(APPDEPT) AS APPDEPT_NAME
                    , (SELECT FLOWID || ' ' || FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID) FLOWID
                    , TOWH || ' ' || WH_NAME(TOWH) AS TOWH_NAME
                    , FRWH || ' ' || WH_NAME(FRWH) AS FRWH_NAME
                    , a.CREATE_USER AS CREATE_USER
                    , (SELECT 'Y' FROM ME_DOCD D,MI_MAST E WHERE A.DOCNO=D.DOCNO AND E.MMCODE=D.MMCODE AND WEXP_ID='Y' AND ROWNUM<=1 ) LIST_ID
                    FROM ME_DOCM A, MI_WHMAST B WHERE 1=1 and B.INID = :INID and B.WH_NO = a.APPDEPT ";

            p.Add(":INID", string.Format("{0}", query.INID));

            if (query.DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :DOCNO ";
                p.Add(":DOCNO", string.Format("%{0}%", query.DOCNO));
            }

            if (query.APPID != "")
            {
                //sql += " AND A.APPID = :APPID ";
                sql += " AND ( A.APPID = user_id(:APPID) or :APPID is null ) ";
                p.Add(":APPID", string.Format("{0}", query.APPID));
            }

            if (query.APPDEPT != "")
            {
                sql += " AND A.APPDEPT LIKE :APPDEPT ";
                p.Add(":APPDEPT", string.Format("%{0}%", query.APPDEPT));
            }

            if (query.DOCTYPE != "")
            {
                //sql += " AND A.DOCTYPE LIKE :DOCTYPE ";
                //p.Add(":DOCTYPE", string.Format("%{0}%", query.DOCTYPE));
                sql += " AND A.DOCTYPE = :DOCTYPE ";
                p.Add(":DOCTYPE", query.DOCTYPE);
            }

            if (query.FLOWID != "")
            {
                string tmp = query.FLOWID.Replace(",", "','");
                //string temp = 
                sql += string.Format(" AND A.FLOWID IN ('{0}')", tmp);
                //p.Add(":FLOWID", tmp);
                //p.Add(":FLOWID", query.FLOWID);

            }

            if (query.TOWH != "")
            {
                sql += " AND A.TOWH LIKE :TOWH ";
                p.Add(":TOWH", string.Format("%{0}%", query.TOWH));
            }

            if (query.FRWH != "")
            {
                sql += " AND A.FRWH LIKE :FRWH ";
                p.Add(":FRWH", string.Format("%{0}%", query.FRWH));
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

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetFrwhCombo(string userId) // inid
        {
            //string sql = @"SELECT WH_NO AS VALUE
            //                , WH_NO || ' ' || WH_NAME AS COMBITEM 
            //            FROM MI_WHMAST 
            //            WHERE INID=:INID
            //            order by WH_NO";
            //return DBWork.Connection.Query<COMBO_MODEL>(sql, new { INID = inid }, DBWork.Transaction);

            string sql = @"SELECT a.WH_NO as VALUE,
                                  a.WH_NO || ' ' || b.WH_NAME as COMBITEM
                             FROM MI_WHID a, MI_WHMAST b
                            WHERE a.WH_USERID = :USERID
                              AND b.WH_NO = a.WH_NO
                              AND a.TASK_ID = '1'
                              and b.cancel_id = 'N'
                            ORDER BY a.WH_NO";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { USERID = userId }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetAppidCombo(string userid)
        {
            string sql = @"select distinct b.WH_USERID as VALUE,
                                           (b.WH_USERID || ' ' || c.una) as COMBITEM
                             from MI_WHID a, MI_WHID b, UR_ID c
                            where a.WH_USERID = :userid
                              and b.WH_NO = a.WH_NO
                              and c.tuser = b.WH_USERID
                            order by b.WH_USERID";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { USERID = userid }, DBWork.Transaction);
        }
        public string GetTowh(string docno)
        {
            string sql = @"SELECT TOWH FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        // 取得核撥庫房, 只可選上級庫
        public IEnumerable<MI_WHMAST> GetTowh(string inid, int wh_grade)
        {
            var sql = @"SELECT WH_NO, WH_GRADE || ' ' || WH_NO || ' ' || WH_NAME AS WH_NAME, SUPPLY_INID 
                          FROM MI_WHMAST 
                         WHERE wh_kind = '0' 
                           and WH_GRADE < TO_CHAR(:WH_GRADE) 
                           and WH_GRADE < 3 
                           and cancel_id = 'N' 
                         order by WH_NO";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { INID = inid, WH_GRADE = wh_grade }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            // 是否公藥E_IFPUBLIC  0-非公藥, 1-存點為病房，上級庫為住院藥局 PH1A, 2-存點為病房，上級庫為藥庫 PH1S, 3-存點為病房，設為備用藥，上級庫為住院藥局
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.M_CONTPRICE, A.BASE_UNIT 
                             FROM MI_MAST A INNER JOIN MI_WINVCTL B ON A.MMCODE=B.MMCODE 
                            WHERE 1=1 
                              and a.e_orderdcflag = 'N'";

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

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT 
                        FROM MI_MAST A 
                        INNER JOIN MI_WINVCTL B ON A.MMCODE=B.MMCODE 
                        WHERE 1=1 
                          and a.e_orderdcflag = 'N'";
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

        public int MasterCreate(ME_DOCM me_docm)
        {
            var sql = @"INSERT INTO ME_DOCM (DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT, APPTIME, USEID, USEDEPT, TOWH, FRWH, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, MAT_CLASS)  
                                    VALUES (:DOCNO, :DOCTYPE, :FLOWID, :APPID, :APPDEPT, SYSDATE, '', '', :TOWH, :FRWH, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :MAT_CLASS)";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> MasterGet(string docno)
        {
            var sql = @" SELECT DOCNO FROM ME_DOCM WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int MasterUpdate(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FRWH=:FRWH, TOWH=:TOWH, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO";
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
            var sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, aplyitem_note)  
                                    VALUES (:DOCNO, :SEQ, :MMCODE, :APPQTY, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :aplyitem_note)";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailUpdate(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET MMCODE=:MMCODE, APPQTY=:APPQTY, aplyitem_note = :aplyitem_note, UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailDelete(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public int Copy(string newDocno, string oldDocno, string user, string ip)
        {
            // copy master
            var sql = @"INSERT INTO ME_DOCM (DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT, APPTIME, USEID, USEDEPT, FRWH, TOWH, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                                    SELECT :NEWDOCNO, DOCTYPE, '0401', :CREATE_USER, APPDEPT, SYSDATE, :CREATE_USER, USEDEPT, FRWH, TOWH, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP FROM ME_DOCM where DOCNO=:OLDDOCNO";
            DBWork.Connection.Execute(sql, new { NEWDOCNO = newDocno, OLDDOCNO = oldDocno, CREATE_USER = user, UPDATE_USER = user, UPDATE_IP = ip }, DBWork.Transaction);

            // copy detail
            sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, APVQTY, ACKQTY, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                               SELECT :NEWDOCNO, SEQ, MMCODE, '0','0','0', SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP FROM ME_DOCD where DOCNO=:OLDDOCNO";

            return DBWork.Connection.Execute(sql, new { NEWDOCNO = newDocno, OLDDOCNO = oldDocno, CREATE_USER = user, UPDATE_USER = user, UPDATE_IP = ip }, DBWork.Transaction);
        }

        public int UpdateStatus(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = :FLOWID, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public SP_MODEL CreateMeDoceByDocno(string docno)
        {
            //OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString);
            //conn.Open();
            //OracleCommand cmd = new OracleCommand("CREATE_ME_DOCE_BY_DOCNO", conn);
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.Add("I_DOCNO", OracleDbType.Varchar2).Value = docno;
            ////cmd.Parameters.Add("I_ACTID", OracleDbType.Varchar2).Value = I_ACTID;
            ////cmd.Parameters.Add("I_QTYID", OracleDbType.Varchar2).Value = I_QTYID;
            ////cmd.Parameters.Add("I_FLOWID", OracleDbType.Varchar2).Value = I_FLOWID;
            ////cmd.Parameters.Add("I_UPDUSR", OracleDbType.Varchar2).Value = updusr;
            ////cmd.Parameters.Add("I_UPDIP", OracleDbType.Varchar2).Value = updip;
            //cmd.Parameters.Add("O_RETID", OracleDbType.Varchar2, 1).Direction = ParameterDirection.Output;
            //cmd.Parameters.Add("O_ERRMSG", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;
            //cmd.ExecuteNonQuery();

            //SP_MODEL sp = new SP_MODEL
            //{
            //    O_RETID = cmd.Parameters["O_RETID"].Value.ToString(),
            //    O_ERRMSG = cmd.Parameters["O_ERRMSG"].Value.ToString()
            //};
            //conn.Close();
            //return sp;

            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: docno, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("CREATE_ME_DOCE_BY_DOCNO", p, commandType: CommandType.StoredProcedure);

            string errmsg = string.Empty;
            if (p.Get<OracleString>("O_ERRMSG") != null)
            {
                errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            }

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = errmsg
            };
            return sp;
        }

        public bool DOC_CHECK_EXP(string docno)
        {
            string sql = @"select DOC_CHECK_EXP(:docno) from dual";

            string temp = DBWork.Connection.QueryFirstOrDefault<string>(sql, new { docno = docno}, DBWork.Transaction);

            return temp == "Y";
        }

        public string IsWexpid(string mmcode) {
            string sql = @"select wexp_id(:mmcode) from dual";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode = mmcode }, DBWork.Transaction);
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

            public string INID;
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

        public int DeleteEmptyMaster(string userId)
        {
            string sql = string.Format(@"
                           delete from ME_DOCM a
                            where appid = '{0}'
                              and doctype = 'RN'
                              and not exists (select 1 from ME_DOCD where docno = a.docno)
                            ", userId);
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        #region print
        public IEnumerable<AB0022> GetPrint(string[] docnos) {

            var p = new DynamicParameters();
            string sql = @"select a.frwh as frwh, a.towh as towh, 
                                  TWN_TIME(a.apptime) as apptime,
                                  a.docno as docno,
                                  get_param('ME_DOCM','FLOWID',a.flowid) as flowid,
                                  b.mmcode as mmcode, 
                                  base_unit(b.mmcode) as base_unit,
                                  (select mmname_c from MI_MAST where mmcode = b.mmcode) as mmname_c,
                                  (select mmname_e from MI_MAST where mmcode = b.mmcode) as mmname_e,
                                  b.appqty as appqty,
                                  b.apvqty as apvqty,
                                  inv_qty(a.towh, b.mmcode) as invqty,
                                  docexp_lot(a.docno, b.seq) as docexp_lot,
                                  (select m_agenno||'_'||substr(agen_name(m_agenno),1,4) from MI_MAST where mmcode = b.mmcode) as agen,
                                  b.aplyitem_note as aplyitem_note
                             from ME_DOCM a, ME_DOCD b
                            where a.docno in :docnos
                              and b.docno = a.docno";
            p.Add("docnos", docnos);

            return DBWork.Connection.Query<AB0022>(sql, p, DBWork.Transaction);
        }
        #endregion

        #region 2021-02-25 檢查是否全院停用
        public bool CheckOrderdcflagN(string mmcode) {
            string sql = @"select 1 from MI_MAST
                            where mmcode = :mmcode
                              and e_orderdcflag = 'N'";

            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         mmcode = mmcode
                                                     },
                                                     DBWork.Transaction) == null);
        }

        // 檢查申請單是否有全院停用品項
        public IEnumerable<string> CheckOrderdcflagYByDocno(string docno) {
            string sql = @"select a.mmcode 
                             from ME_DOCD a, MI_MAST b
                            where a.docno = :docno
                              and a.mmcode = b.mmcode
                              and b.e_orderdcflag <> 'N'";
            return DBWork.Connection.Query<string>(sql, new { docno = docno }, DBWork.Transaction);
        }
        #endregion

        #region 2021-02-25 檢查是否0庫存
        public bool CheckInvqty0(string mmcode, string docno)
        {
            string sql = @"select 1 from MI_WHINV
                            where mmcode = :mmcode
                              and wh_no = (select frwh from ME_DOCM where docno = :docno)
                              and inv_qty <= 0";

            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         mmcode = mmcode, 
                                                         docno = docno
                                                     },
                                                     DBWork.Transaction) == null);
        }

        // 檢查申請單是否0庫存
        public IEnumerable<string> CheckInvqty0ByDocno(string docno)
        {
            string sql = @"select b.mmcode 
                             from ME_DOCM a, ME_DOCD b, MI_WHINV c
                            where a.docno = :docno
                              and b.docno = a.docno
                              and c.mmcode = b.mmcode
                              and c.wh_no = a.frwh
                              and c.inv_qty <= 0";
            return DBWork.Connection.Query<string>(sql, new { docno = docno }, DBWork.Transaction);
        }
        #endregion

        #region 20201-05-06新增: 修改、刪除時檢查flowId
        public bool ChceckFlowId01(string docno)
        {
            string sql = @"
                select 1 from ME_DOCM
                 where docno = :docno
                   and (substr(flowId, length(flowId)-1 , 2) = '01'
                       or substr(flowId, length(flowId)-1 , 2) = '00'
                       or substr(flowId, length(flowId)-1 , 2) = '1')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) != null;
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

        public bool CheckIsTowhCancelByDocno(string docno, string wh)
        {
            string sql = string.Format(@"
               select 1 from ME_DOCM  a
                where a.docno = :docno
                  and exists (select 1 from MI_WHMAST 
                               where wh_no = a.{0}
                                 and cancel_id = 'N')
            ", wh);
            return DBWork.Connection.ExecuteScalar(sql, new { docno, wh }, DBWork.Transaction) == null;
        }
        #endregion
    }
}