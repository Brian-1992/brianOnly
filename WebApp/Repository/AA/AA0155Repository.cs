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

namespace WebApp.Repository.AA
{
    public class AA0155Repository : JCLib.Mvc.BaseRepository
    {
        public AA0155Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<ME_DOCM> GetAllM(string tuser, AA0155M query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT distinct DOCNO, TWN_DATE(APPTIME) AS APPTIME, A.MAT_CLASS,
                    APPID || ' ' || USER_NAME(APPID) AS APP_NAME,
                    USER_NAME(APPID) AS APPID, 
                    APPDEPT || ' ' || INID_NAME(APPDEPT) AS APPDEPT_NAME,
                    (SELECT FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID) FLOWID,
                    TOWH, FRWH,
                    TOWH || ' ' || WH_NAME(TOWH) AS TOWH_NAME,
                    FRWH || ' ' || WH_NAME(FRWH) AS FRWH_NAME,
                    (case when a.towh in (whno_1x('1'),whno_1x('0')) then '換入(民→軍)' else '換出(軍→民)' end) as APPLY_KIND,
                    B.WH_KIND
                    FROM ME_DOCM A, MI_WHMAST B WHERE 1=1 and doctype in ('XR2','XR3') 
                    AND (A.TOWH = B.WH_NO OR A.FRWH = B.WH_NO)
                    AND B.WH_GRADE IN ('1','5') AND B.WH_KIND IN ('0','1')
                    AND EXISTS (
                        SELECT 1 FROM MI_WHID C, MI_WHMAST D 
                        WHERE C.WH_USERID = :TUSER
                        AND C.WH_NO = D.WH_NO
                        AND D.WH_GRADE IN ('1','5') AND D.WH_KIND IN ('0','1')
                        AND (A.TOWH = C.WH_NO OR A.FRWH = C.WH_NO)
                    )
                    "; 

            p.Add(":TUSER", string.Format("{0}", tuser));

            if (query.DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :DOCNO ";
                p.Add(":DOCNO", string.Format("%{0}%", query.DOCNO));
            }

            if (query.MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS LIKE :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("%{0}%", query.MAT_CLASS));
            }

            if (query.FRWH != "")
            {
                sql += " AND A.FRWH = :FRWH ";
                p.Add(":FRWH", query.FRWH);
            }

            if (query.FLOWID != "")
            {
                sql += " AND substr(A.FLOWID, 3,2) = :FLOWID";
                p.Add(":FLOWID", query.FLOWID);
            }

            if (query.APPTIME_S != "")
            {
                sql += " AND twn_date(A.APPTIME) >= :APPTIME_S ";
                p.Add(":APPTIME_S", string.Format("{0}", query.APPTIME_S));
            }
            if (query.APPTIME_E != "")
            {
                sql += " AND twn_date(A.APPTIME) <= :APPTIME_E ";
                p.Add(":APPTIME_E", string.Format("{0}", query.APPTIME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> GetAllD(AA0155D query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.DOCNO, a.SEQ, a.MMCODE, a.APPQTY, a.M_CONTPRICE, a.UP, a.AMT, b.MMNAME_C, b.MMNAME_E, b.BASE_UNIT
                    FROM ME_DOCD a left join MI_MAST b on a.MMCODE = b.MMCODE WHERE 1=1 ";

            if (query.DOCNO != "")
            {
                sql += " AND A.DOCNO = :DOCNO ";
                p.Add(":DOCNO", query.DOCNO);
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetMeDocd(ME_DOCD_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.DOCNO, a.SEQ, a.MMCODE, a.APPQTY, a.UP, a.AMT, a.APL_CONTIME, a.APLYITEM_NOTE, b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT
                        FROM ME_DOCD a LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE WHERE 1=1 ";
            //var sql = @"SELECT a.DOCNO, a.SEQ, a.MMCODE, a.APPQTY, a.UP, a.AMT, a.APL_CONTIME, a.APLYITEM_NOTE, b.MMNAME_C, b.MMNAME_E, c.M_CONTPRICE, b.BASE_UNIT
            //            FROM ME_DOCD a 
            //            LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE
            //            LEFT JOIN MI_MAST c on '004' || substr(a.MMCODE, 4, length(a.MMCODE))=c.MMCODE 
            //            WHERE 1=1";

            if (query.DOCNO != "")
            {
                sql += " AND a.DOCNO=:p0 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhId(string userId)
        {
            var sql = @"select ' ' AS VALUE, '' AS COMBITEM from dual
                        union select WH_NO, WH_NO || ' ' || WH_NAME(WH_NO) from MI_WHID where WH_USERID = :WH_USERID
                        union select 'PH1X', 'PH1X' || ' ' || WH_NAME('PH1X') from dual";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcode(AA0155D query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, C.M_CONTPRICE, A.BASE_UNIT FROM MI_MAST A
                        INNER JOIN MI_WINVCTL B ON A.MMCODE=B.MMCODE
                        LEFT JOIN MI_MAST c on '004' || substr(a.MMCODE, 4, length(a.MMCODE))=c.MMCODE 
                        WHERE 1=1 ";

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

        public IEnumerable<ComboItemModel> GetMclassCombo(string tuser,string is_query)
        {
            string addSql = "";
            if (is_query == "Y")
                addSql = @"select '' as VALUE,'全部' as TEXT,'' as COMBITEM, '' as EXTRA1  
                    from dual
                    union";

            var sql = @"with temp_whkinds as (
                        select b.WH_NO, b.WH_KIND, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as TASK_ID
                        from MI_WHID a, MI_WHMAST b
                        where WH_USERID = :TUSER
                        and a.WH_NO = b.wh_no
                        and b.WH_GRADE = '1'
                        and a.TASK_ID in ('1','2','3')
                    )
                    " + addSql + @"
                    select distinct b.MAT_CLASS as VALUE,b.MAT_CLSNAME as TEXT,b.MAT_CLASS || ' ' ||  b.MAT_CLSNAME as COMBITEM, a.WH_KIND as EXTRA1  
                    from temp_whkinds a, MI_MATCLASS b
                    where (a.TASK_ID = b.MAT_CLSID)
                    ";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<AA0155M> GetFrwhCombo(string wh_kind, string tuser)
        {
            string addSql = "";
            object sqlParam = null;
            if (wh_kind != "" && wh_kind != null)
            {
                addSql = "and b.wh_kind = :WH_KIND";
                sqlParam = new { WH_KIND = wh_kind, TUSER = tuser };
            }
            else
                sqlParam = new { TUSER = tuser };

            var sql = @"with temp_whkinds as (
                        select b.wh_no, b.wh_kind
                        from MI_WHID a, MI_WHMAST b
                        where wh_userid = :TUSER
                        and a.wh_no = b.wh_no
                        and b.wh_grade = '1'
                        and a.task_id in ('1','2','3')
                        " + addSql + @"
                    )
                    select b.WH_NO, b.WH_KIND, b.WH_GRADE, (b.wh_no||' '||b.wh_name) as WH_NO_NAME,
                    (select wh_no||' '||wh_name from MI_WHMAST where wh_grade = '5' and wh_kind = a.wh_kind) TOWH,
                    '換入(民→軍)' as APPLY_KIND
                    from temp_whkinds a, MI_WHMAST b
                    where a.wh_no = b.wh_no
                    union
                    select b.WH_NO, b.WH_KIND, b.WH_GRADE, (b.wh_no||' '||b.wh_name) as WH_NO_NAME,
                    (select wh_no||' '||wh_name from MI_WHMAST where wh_grade = '1' and wh_kind = a.wh_kind) TOWH,
                    '換出(軍→民)' as APPLY_KIND
                    from temp_whkinds a, MI_WHMAST b
                    where a.wh_kind = b.wh_kind
                    and b.wh_grade = '5'
                    ";

            return DBWork.Connection.Query<AA0155M>(sql, sqlParam, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetFlowidCombo()
        {
            var sql = @"select distinct substr(flowid,3,2) as VALUE, flowName as TEXT 
                    from ME_FLOW
                    where doctype in ('XR2','XR3')
                    union
                    select '' as VALUE, '全部' as TEXT from dual
                    ";

            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string docno, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,
                               A.M_CONTPRICE
                          FROM MI_MAST A
                         where 1=1
                           AND A.MAT_CLASS = (select mat_class from ME_DOCM where docno = :DOCNO) 
                           AND EXISTS ( SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO =(select frwh from ME_DOCM where docno = :DOCNO) ) ";
            p.Add(":DOCNO", docno);

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(A.MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
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
                                    VALUES (:DOCNO, :DOCTYPE, :FLOWID, :APPID, USER_INID(:APPID), SYSDATE, :APPID, USER_INID(:APPID), :TOWH, :FRWH, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :MAT_CLASS)";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> MasterGet(string docno)
        {
            var sql = @" SELECT DOCNO FROM ME_DOCM WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int MasterUpdate(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET APPID=:UPDATE_USER, APPDEPT = USER_INID(:UPDATE_USER), APPTIME = SYSDATE, USEID = :UPDATE_USER, USEDEPT = USER_INID(:UPDATE_USER),
                FRWH=:FRWH, TOWH=:TOWH, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO";
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
            var sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, AMT, UP, STAT, 
                                    CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                    VALUES (:DOCNO, :SEQ, :MMCODE, :APPQTY, :APPQTY*:UP, :UP, (case when (select towh from ME_DOCM where docno = :DOCNO) in (whno_1x('1'),whno_1x('0')) then '1' else '2' end), 
                                    SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailUpdate(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET MMCODE=:MMCODE, APPQTY=:APPQTY, AMT=:APPQTY*:UP, UP=:UP, UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailUpdateP(string docno)
        {
            var sql = @"UPDATE ME_DOCD A SET up = (select m_contprice from MI_MAST where mmcode = A.mmcode),
                    amt = appqty*(select m_contprice from MI_MAST where mmcode = A.mmcode)
                    where a.docno = :DOCNO
                    ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DetailDelete(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        // 檢查申請單院內碼項次的申請數量不得<=0,有的話則不能提出申請
        public bool CheckMeDocdAppqty(string docno)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO and APPQTY<=0";
            return (DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
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
        }

        public class ME_DOCD_QUERY_PARAMS
        {
            public string DOCNO;
        }

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
        }

        public bool CheckMmcodeValid(string mmcode)
        {
            string sql = @"select 1 
                             FROM MI_MAST A
                            where 1=1
                              and a.mmcode = :mmcode
                              and substr(a.mmcode, 1,3) = '005'
                              and exists (select 1 from MI_MAST 
                                           where mmcode = '004'||substr(a.mmcode, 4, length(a.MMCODE)))
                              and exists (select 1 from MI_WINVCTL 
                                           where mmcode = '004'||substr(a.mmcode, 4, length(a.MMCODE)) and wh_no = 'PH1X' and ctdmdccode = '0')";
            return !(DBWork.Connection.ExecuteScalar(sql, new { mmcode = mmcode }, DBWork.Transaction) == null);
        }

        public bool ChceckFlowId01(string docno)
        {
            string sql = @"
                select 1 from ME_DOCM
                 where docno = :docno
                 and flowid in ('1801', '1901')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) != null;
        }
    }
}