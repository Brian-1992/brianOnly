using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AA
{
    public class AA0035Repository : JCLib.Mvc.BaseRepository
    {
        public AA0035Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(string docno, string apptime1, string apptime2, string frwh, string userid, int page_index, int page_size, string sorters) {

            var p = new DynamicParameters();
            
            string sql = string.Format(@"SELECT a.DOCNO AS DOCNO, 
                                  a.FLOWID AS FLOWID, 
                                  a.FRWH AS FRWH, 
                                  a.APPID || ' ' || USER_NAME(a.APPID) AS APP_NAME,
                                  USER_NAME(a.APPID) AS APPID, 
                                  a.APPDEPT || ' ' || INID_NAME(a.APPDEPT) AS APPDEPT_NAME,
                                  b.WH_NO || ' ' ||b.WH_NAME AS FRWH_N,
                                  TWN_DATE(a.APPTIME) AS APPTIME,
                                  (SELECT FLOWNAME FROM V_FLOW WHERE FLOWID = a.FLOWID) as FLOWID_N,
                                  a.CREATE_USER as CREATE_USER,
                                  c.UNA as CREATE_USER_NAME,
                                  (SELECT 'Y' FROM ME_DOCD D,MI_MAST E WHERE A.DOCNO=D.DOCNO AND E.MMCODE=D.MMCODE AND WEXP_ID='Y' AND ROWNUM<=1 ) LIST_ID
                             FROM ME_DOCM a, MI_WHMAST b, UR_ID c {0}
                            WHERE a.FLOWID like '05%'
                              AND a.DOCTYPE = 'SP'
                              AND b.WH_NO = a.FRWH
                              AND c.TUSER = a.CREATE_USER", frwh == string.Empty ? ", MI_WHID d" : string.Empty);
            if (docno != string.Empty) {
                sql += " AND a.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", docno));
            }
            if (apptime1 != string.Empty && apptime2 == string.Empty)
            {
                sql += " AND TRUNC(a.APPTIME, 'DD') >= TO_DATE(:p1,'YYYY-MM-DD') ";
                p.Add(":p1", string.Format("{0}", DateTime.Parse(apptime1).ToString("yyyy-MM-dd")));
            }
            if (apptime1 != string.Empty && apptime2 != string.Empty)
            {
                sql += " AND TRUNC(a.APPTIME, 'DD') <= TO_DATE(:p2,'YYYY-MM-DD') ";
                p.Add(":p2", string.Format("{0}", DateTime.Parse(apptime1).ToString("yyyy-MM-dd")));
            }
            if (apptime1 != string.Empty && apptime2 != string.Empty)
            {
                sql += " AND TRUNC(a.APPTIME, 'DD') BETWEEN TO_DATE(:p1,'YYYY-MM-DD') AND TO_DATE(:p2,'YYYY-MM-DD') ";
                p.Add(":p1", string.Format("{0}", DateTime.Parse(apptime1).ToString("yyyy-MM-dd")));
                p.Add(":p2", string.Format("{0}", DateTime.Parse(apptime2).ToString("yyyy-MM-dd")));
            }

            if (frwh != string.Empty)
            {
                sql += " AND a.FRWH = :p3";
                p.Add(":p3", string.Format("{0}", frwh));
            }
            else {
                sql += @"  and d.wh_userid = :p4 
                           and a.frwh = d.wh_no";
                p.Add(":p4", string.Format("{0}", userid));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCM> Get(string docno) {
            string sql = @"SELECT a.DOCNO AS DOCNO, 
                                  a.FLOWID AS FLOWID, 
                                  a.FRWH AS FRWH, 
                                  a.APPID || ' ' || USER_NAME(a.APPID) AS APP_NAME,
                                  a.APPDEPT || ' ' || INID_NAME(a.APPDEPT) AS APPDEPT_NAME,
                                  b.WH_NO || ' ' ||b.WH_NAME AS FRWH_N,
                                  TWN_DATE(a.APPTIME) AS APPTIME,
                                  (SELECT FLOWNAME FROM V_FLOW WHERE FLOWID = a.FLOWID) as FLOWID_N
                             FROM ME_DOCM a, MI_WHMAST b
                            WHERE a.FLOWID like '05%'
                              AND a.DOCTYPE = 'SP'
                              AND b.WH_NO = a.FRWH
                              AND a.DOCNO = :DOCNO";

            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);

        }

        public string GetDocno()
        {

            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12);     

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            string output = p.Get<OracleString>("O_DOCNO").Value;
            return output;

        }

        public string CreateMeDoce(string docno, string seq, string wh_no) {

            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: docno, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_SEQ", value: seq, dbType: OracleDbType.Int32, direction: ParameterDirection.Input, size: 8);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("CREATE_ME_DOCE", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;

            string errmsg = string.Empty;
            if (p.Get<OracleString>("O_ERRMSG") != null)
            {
                errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            }

            //string errmsg = p.Get<OracleString>("O_ERRMSG") == null ? string.Empty : p.Get<OracleString>("O_ERRMSG").Value;
            return retid;
        }

        public int MasterCreate(ME_DOCM me_docm) {
            string sql = @"INSERT INTO ME_DOCM (DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT, APPTIME, FRWH, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, MAT_CLASS)
                           VALUES (:DOCNO, 'SP','0501', :APPID, :APPDEPT, SYSDATE, :FRWH, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :MAT_CLASS)";

            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int MasterDelete(string docno)
        {
            var sql = @" DELETE from ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int MasterUpdate(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FRWH=:FRWH, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        // 檢查申請單號是否存在
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        // 檢查master是否有detail
        public bool CheckMeDocdExists(string docno)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
        }

        public IEnumerable<ME_DOCD> GetMeDocds(string docno)
        {
            string sql = @"SELECT DOCNO AS DOCNO,
                                  SEQ AS SEQ
                             FROM ME_DOCD
                            WHERE DOCNO = :DOCNO";

            var p = new DynamicParameters();
            p.Add(":DOCNO", string.Format("{0}", docno));

            return DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetMeDocds(string docno, string wh_no , int page_index, int page_size, string sorters) {
            string sql = @"SELECT a.DOCNO AS DOCNO,
                                  a.SEQ AS SEQ,
                                  a.MMCODE AS MMCODE,
                                  b.MMNAME_C AS MMNAME_C,
                                  b.MMNAME_E AS MMNAME_E,
                                  a.APVQTY AS APVQTY,
                                  a.APPQTY AS APPQTY,
                                  b.BASE_UNIT AS BASE_UNIT,
                                  (CASE WHEN b.M_CONTPRICE is not null THEN b.M_CONTPRICE ELSE 0 END )AS M_CONTPRICE,
                                  c.INV_QTY AS INV_QTY,
                                  a.aplyitem_note
                             FROM ME_DOCD a
                             LEFT JOIN MI_MAST b on b.MMCODE = a.MMCODE 
                             LEFT JOIN MI_WHINV c on (c.WH_NO = :WH_NO and c.MMCODE = a.MMCODE and c.MMCODE = b.MMCODE) 
                            WHERE a.DOCNO = :DOCNO
                            ";

            var p = new DynamicParameters();
            p.Add(":DOCNO", string.Format("{0}", docno));
            p.Add(":WH_NO", string.Format("{0}", wh_no));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int DetailCreate(ME_DOCD me_docd) {
            string sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, APVQTY, APVTIME, APVID, CREATE_TIME,CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, aplyitem_note )
                           VALUES (:DOCNO, :SEQ, :MMCODE, :APPQTY, :APVQTY, SYSDATE, :APVID, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :APLYITEM_NOTE)";

            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetMeDocdMaxSeq(string docno) {
            string sql = @"SELECT (CASE WHEN MAX(SEQ) > 0 THEN (MAX(SEQ)+1) ELSE 1 END) AS SEQ  
                             FROM ME_DOCD WHERE DOCNO = :DOCNO";

            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public bool CheckDocdExists(string docno, string seq) {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, SEQ=seq }, DBWork.Transaction) == null);
        }

        public int DetailDelete(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ=seq }, DBWork.Transaction);
        }
        public int DetailUpdate(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET APPQTY=:APPQTY,APVQTY=:APVQTY,APLYITEM_NOTE = :APLYITEM_NOTE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO and SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }
        public bool CheckMeDocdExists(string docno, string seq)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO and SEQ=:SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, SEQ=seq }, DBWork.Transaction) == null);
        }

        public IEnumerable<ME_DOCE> GetMeDoces(string docno, string seq) {
            string sql = @"SELECT a.DOCNO AS DOCNO, 
                                  a.SEQ AS SEQ,
                                  a.EXPDATE AS EXPDATE, 
                                  a.EXPDATE AS ORI_EXPDATE,
                                  a.MMCODE AS MMCODE, 
                                  a.APVQTY AS APVQTY, 
                                  TWN_DATE(a.APVTIME) AS APVTIME, 
                                  b.WARNYM AS WARNYM,
                                  c.TUSER || ' ' || c.UNA AS UPDATE_USER_NAME
                             FROM ME_DOCE a
                             LEFT JOIN ME_EXPM b on (b.mmcode = a.mmcode and b.exp_date = a.expdate)
                             LEFT JOIN UR_ID c on(c.TUSER = a.UPDATE_USER)
                            WHERE DOCNO = :DOCNO AND SEQ = :SEQ";

            return DBWork.Connection.Query<ME_DOCE>(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public int MeDoceDelete(string docno, string seq)
        {
            string sql = @"DELETE FROM ME_DOCE WHERE DOCNO = :DOCNO AND SEQ = : SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }
        public int MeDoceUpdate(ME_DOCE me_doce)
        {
            string sql = @"UPDATE ME_DOCE SET EXPDATE = :EXPDATE, APVQTY = :APVQTY, UPDATE_TIME=SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                            WHERE DOCNO = :DOCNO AND SEQ = :SEQ AND MMCODE = :MMCODE AND EXPDATE= :ORI_EXPDATE";
            return DBWork.Connection.Execute(sql, me_doce, DBWork.Transaction);
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
                O_RETID = retid,
                O_ERRMSG = errmsg
            };
            return sp;
        }


        public IEnumerable<ComboItemModel> GetWhnoCombo(string wh_userId)
        {
            string sql = @"SELECT WH_NO AS VALUE, WH_NO || ' ' || WH_NAME AS TEXT
                             FROM MI_WHMAST a 
                            WHERE EXISTS (
                                     SELECT *
                                       FROM MI_WHID b
                                      WHERE a.WH_NO = b.WH_NO
                                        AND WH_USERID = :WH_USERID
                            )
                            ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }

        public string IsWexpid(string mmcode)
        {
            string sql = @"select wexp_id(:mmcode) from dual";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }



        public class AA0035_MM_MATMAST
        {
            public string MMCODE { get; set; }
            public string MMNAME_C { get; set; }
            public string MMNAME_E { get; set; }
        }
        public IEnumerable<AA0035_MM_MATMAST> GetMmdataByMmcode(string mmcode)
        {
            string sql = @"select MMCODE, MMNAME_C, MMNAME_E
                             from MI_MAST
                            where MMCODE=:MMCODE";
            return DBWork.Connection.Query<AA0035_MM_MATMAST>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            //var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT 
            //              FROM MI_MAST A, MI_WHMM B, MI_WHINV C 
            //             WHERE 1=1 
            //               AND B.WH_NO = :WH_NO
            //               AND c.WH_NO = B.WH_NO
            //               AND c.MMCODE = A.MMCODE";

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A INNER JOIN MI_WINVCTL B ON A.MMCODE=B.MMCODE WHERE 1=1 ";
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

        #region 2021-07-23 新增報表
        public IEnumerable<AA0025ReportMODEL> GetPrintData(string apptime1, string apptime2)
        {
            var p = new DynamicParameters();

            string sql = @"select TWN_DATE(a.APPTIME) F1,
                                  (select WH_NAME from MI_WHMAST 
                                    where WH_NO=a.FRWH) F2,
                                  b.MMCODE F3,
                                  c.mmname_e as F4,
                                  SUBSTR((select p.AGEN_NO || '_' || p.AGEN_NAMEC
                                            from PH_VENDER p
                                           where c.M_AGENNO=p.AGEN_NO
                                         ),0,6) F5,
                                  -b.APVQTY F6,
                                  c.DISC_CPRICE F7,
                                  c.M_CONTPRICE F8,
                                  '合約單價' F9,
                                  (-b.APVQTY * c.M_CONTPRICE) F10,
                                  b.aplyitem_note F11
                             from ME_DOCM a, ME_DOCD b, MI_MAST c
                            where a.FLOWID = '0599'
                              AND a.DOCTYPE = 'SP'
                              and a.DOCNO=b.DOCNO 
                              and b.mmcode = c.mmcode
                           ";

            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", apptime1));
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", apptime2));
            }

            sql += "        order by a.apptime, b.mmcode";
            return DBWork.Connection.Query<AA0025ReportMODEL>(sql, p, DBWork.Transaction);
        }
        #endregion
    }
}