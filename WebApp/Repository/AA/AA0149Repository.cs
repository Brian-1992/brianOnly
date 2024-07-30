using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0149Repository : JCLib.Mvc.BaseRepository
    {

        public AA0149Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(ME_DOCM_QUERY_PARAMS query, string[] str_flowid, string[] str_matclass, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT docno, --單據號碼,
                                           (SELECT mat_class || ' ' || mat_clsname FROM mi_matclass WHERE mat_class = a.mat_class) mat_class, --物料分類,
                                           twn_date(apptime) as apptime, --繳回申請時間,
                                           towh || ' ' || WH_NAME(towh) AS towh, --入庫庫房,
                                           frwh || ' ' || WH_NAME(frwh) AS frwh, --出庫庫房,
                                           ( CASE
                                               WHEN a.docno = nvl(srcdocno, docno)THEN 'N'
                                               ELSE 'Y'
                                             END ) AS srcdocno, --轉申購單,
                                           ( CASE
                                               WHEN mat_class = '01' THEN (SELECT flowId
                                                                                  || ' 藥品'
                                                                                  || flowName
                                                                           FROM   ME_FLOW
                                                                           WHERE  doctype IN ( 'RN' )
                                                                                  AND flowId = a.flowId)
                                               ELSE (SELECT DATA_VALUE
                                                            || ' 衛材'
                                                            || DATA_DESC
                                                     FROM   PARAM_D
                                                     WHERE  grp_code = 'ME_DOCM'
                                                            AND data_name = 'FLOWID_RN1'
                                                            AND a.flowid = data_value)
                                             END ) AS flowid --單據狀態
                                    FROM   ME_DOCM a
                                    WHERE  1 = 1
                                           AND doctype IN ( 'RN', 'RN1' )
                                           AND a.towh IN (SELECT wh_no
                                                          FROM   MI_WHID
                                                          WHERE  wh_userId = :userId) 
                                 ";

            if (!string.IsNullOrWhiteSpace(query.USERID))
            {
                p.Add(":userId", query.USERID);
            }

            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_flowid.Length > 0)
            {
                string sql_FLOWID = "";
                sql += @"AND (";
                foreach (string tmp_FLOWID in str_flowid)
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

            if (str_matclass.Length > 0)
            {
                string sql_matclass = "";
                sql += @"AND (";
                foreach (string tmp_matclass in str_matclass)
                {
                    if (string.IsNullOrEmpty(sql_matclass))
                    {
                        if (tmp_matclass.Contains("SUB_"))
                            sql_matclass = @"(select count(*) from ME_DOCD C left join MI_MAST D on C.MMCODE = D.MMCODE where A.DOCNO = C.DOCNO and D.MAT_CLASS_SUB = '" + tmp_matclass.Replace("SUB_", "") + "') > 0";
                        else
                            sql_matclass = @"A.MAT_CLASS = '" + tmp_matclass + "'";
                    }
                    else
                    {
                        if (tmp_matclass.Contains("SUB_"))
                            sql_matclass += @" OR (select count(*) from ME_DOCD C left join MI_MAST D on C.MMCODE = D.MMCODE where A.DOCNO = C.DOCNO and D.MAT_CLASS_SUB = '" + tmp_matclass.Replace("SUB_", "") + "') > 0";
                        else
                            sql_matclass += @" OR A.MAT_CLASS = '" + tmp_matclass + "'";
                    }
                }
                sql += sql_matclass + ") ";
            }

            if (query.APPTIME_S != "")
            {
                sql += " AND TRUNC(a.APPTIME, 'DD') >= TO_DATE(:fromdate, 'YYYY-MM-DD') ";
                p.Add(":fromdate", string.Format("{0}", query.APPTIME_S));
            }
            if (query.APPTIME_E != "")
            {
                sql += " AND TRUNC(a.APPTIME, 'DD') <= TO_DATE(:todate, 'YYYY-MM-DD') ";
                p.Add(":todate", string.Format("{0}", query.APPTIME_E));
            }

            return DBWork.PagingQuery<ME_DOCM>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetAllMeDocd(ME_DOCD_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT     a.docno, --單號,
                                               a.seq, --項次,
                                               a.mmcode, --院內碼,
                                               a.appqty, --繳回數量,
                                               b.mmname_c, --中文品名,
                                               b.mmname_e, --英文品名,
                                               b.base_unit, --計量單位,
                                               b.disc_uprice, --優惠最小單價,
                                               (
                                                      SELECT inv_qty
                                                      FROM   mi_whinv
                                                      WHERE  mmcode = a.mmcode
                                                      AND    wh_no=c.frwh ) AS inv_qty, --出庫庫房存量
                                               c.flowid
                                    FROM       me_docd a
                                    inner join mi_mast b
                                    ON         b.mmcode = a.mmcode
                                    inner join me_docm c
                                    ON         c.docno=a.docno
                                    WHERE      1=1
                                    ";

            if (query.DOCNO != "")
            {
                sql += " AND a.DOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", query.DOCNO));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public ME_DOCM GetME_DOCM(string strDocNo)
        {
            string strSql = "SELECT * FROM ME_DOCM WHERE DOCNO=:DOCNO";

            return DBWork.Connection.QueryFirstOrDefault<ME_DOCM>(strSql, new { DOCNO = strDocNo }, DBWork.Transaction);
        }

        public string GetTwnsystime()
        {
            string sql = @"SELECT TWN_SYSTIME FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetWhno_me1()
        {
            string sql = @"SELECT WHNO_ME1 FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetWhno_mm1()
        {
            string sql = @"SELECT WHNO_MM1 FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetFlowIdN(string flowid)
        {
            string sql = @"SELECT FLOWNAME
                         FROM ME_FLOW
                         WHERE flowid =:flowid
                         AND ROWNUM = 1";

            return DBWork.Connection.ExecuteScalar<string>(sql, new { flowid = flowid }, DBWork.Transaction);
        }

        public string GetMAT_CLASS_N(string mat_class)
        {
            string sql = @"SELECT MAT_CLSNAME
                         FROM MI_MATCLASS
                         WHERE mat_class =:mat_class
                         AND ROWNUM = 1";

            return DBWork.Connection.ExecuteScalar<string>(sql, new { mat_class = mat_class }, DBWork.Transaction);
        }

        public string GetDailyDocno()
        {
            string sql = @"select GET_DAILY_DOCNO from DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public int CreateM(ME_DOCM ME_DOCM)
        {
            var sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME , USEID , USEDEPT , FRWH , TOWH , 
                        APPLY_KIND ,APPLY_NOTE ,MAT_CLASS, 
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :DOCTYPE, :FLOWID , :APPID , USER_INID(:APPID) , 
                        SYSDATE , :USEID , USER_INID(:APPID) , :FRWH , :TOWH , 
                        :APPLY_KIND , :APPLY_NOTE ,:MAT_CLASS, 
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int CreateE(ME_DOCEXP docexp)
        {
            var sql = @"INSERT INTO ME_DOCEXP (
                        DOCNO, SEQ, MMCODE , APVQTY , ITEM_NOTE, 
                        C_UP, C_AMT, EXP_DATE, LOT_NO,C_TYPE,
                        UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                        select :DOCNO, a.SEQ, a.MMCODE, a.APVQTY, a.ITEM_NOTE,
                                    b.M_CONTPRICE, (a.APVQTY*b.M_CONTPRICE), a.EXP_DATE, a.LOT_NO,null,
                                    SYSDATE, :UPDATE_USER, :UPDATE_IP
                            from ME_DOCEXP a, MI_MAST b
                         where a.MMCODE=b.MMCODE and DOCNO=:DOCNO_E";

            return DBWork.Connection.Execute(sql, docexp, DBWork.Transaction);
        }

        // 檢查明細內是否有批號效期管制品項
        public bool CheckExp(string docno)
        {
            string sql = @"
                                        SELECT 1
                                        FROM   MI_MAST a
                                        WHERE  EXISTS (SELECT 1
                                                       FROM   ME_DOCM b,
                                                              ME_DOCD c
                                                       WHERE  1 = 1
                                                              AND b.docno = :DOCNO
                                                              AND c.docno = b.docno
                                                              AND a.mmcode = c.mmcode)
                                               AND a.wexp_id = 'Y' 
                                        ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
        }

        public List<MI_WHID> GetWhnoById(string userId)
        {
            var sql = @"SELECT WH_NO FROM MI_WHID WHERE WH_USERID=:WH_USERID";
            return DBWork.Connection.Query<MI_WHID>(sql, new { WH_USERID = userId }, DBWork.Transaction).ToList();
        }

        public int UpdateMeDocd(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET APVQTY = :APVQTY, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int UpdateApvqty(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET APVQTY = APPQTY, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int UpdateStatus(ME_DOCM me_docm)
        {
            string sql = "";
            sql = @" DELETE FROM ME_DOCD 
                     WHERE DOCNO = :DOCNO";
            DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);

             sql = @"
                                    UPDATE me_docm
                                    SET    flowid = ( CASE
                                                        WHEN doctype = 'RN'
                                                             AND flowid = '0402' THEN '0401'
                                                        WHEN doctype = 'RN1'
                                                             AND flowid = '2' THEN '1'
                                                      END ),
                                           update_time = sysdate,
                                           update_user = :UPDATE_USER,
                                           update_ip = :UPDATE_IP
                                    WHERE  docno = :DOCNO 
                                 ";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
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
            public string USEDEPT;
            public string FLOWID;
            public string FRWH;
            public string TOWH;
            public string FROMDATE;
            public string TODATE;
            public string USERID;
            public string MAT_CLASS;
            public string APPTIME_S;
            public string APPTIME_E;
        }

        public class ME_DOCD_QUERY_PARAMS
        {
            public string DOCNO;
        }


        public IEnumerable<COMBO_MODEL> GetInidComboQ()
        {

            var sql = @" SELECT INID AS VALUE,INID_NAME AS TEXT,INID || ' ' ||INID_NAME AS COMBITEM
                        FROM UR_INID WHERE 1 = 1 ORDER BY INID ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetTowhComboQ(string userId)
        {

            var sql = @" SELECT WH_NO AS VALUE,WH_NAME(WH_NO)AS TEXT,WH_NO || ' ' ||WH_NAME(WH_NO) AS COMBITEM
                        FROM MI_WHID WHERE WH_USERID = :WH_USERID AND TASK_ID='1' ORDER BY WH_NO ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetFrwhComboQ()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT,
                        WH_NO || ' ' || WH_NAME as COMBITEM 
                        FROM MI_WHMAST 
                        WHERE 1=1 AND WH_KIND = '0'
                        AND WH_GRADE > '1'  
                        AND WH_GRADE < '5'  
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetFlowidCombo(string userId)
        {
            string sql = @"
                                        WITH temp_whkinds
                                             AS (SELECT b.wh_no,
                                                        b.wh_kind,
                                                        nvl(( CASE
                                                                WHEN a.task_id = '3' THEN '2'
                                                                ELSE a.task_id
                                                              END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  wh_userid = :userId
                                                        AND a.wh_no = b.wh_no
                                                        AND ( ( b.wh_grade IN ( '1', '2' )
                                                                AND a.task_id = '1' )
                                                               OR ( b.wh_grade = '1'
                                                                    AND a.task_id IN ( '2', '3' ) ) )),
                                             temp_mat_class
                                             AS (SELECT DISTINCT b.mat_class      AS value,
                                                                 b.mat_clsname    AS text,
                                                                 b.mat_class
                                                                 || ' '
                                                                 || b.mat_clsname AS COMBITEM
                                                 FROM   temp_whkinds a,
                                                        MI_MATCLASS b
                                                 WHERE  ( a.task_id = b.mat_clsid )
                                                 UNION
                                                 SELECT DISTINCT b.mat_class      AS value,
                                                                 b.mat_clsname    AS text,
                                                                 b.mat_class
                                                                 || ' '
                                                                 || b.mat_clsname AS COMBITEM
                                                 FROM   temp_whkinds a,
                                                        MI_MATCLASS b
                                                 WHERE  ( a.task_id = '2' )
                                                        AND b.mat_clsid = '3') SELECT b.flowid     AS value,
                                               '藥品'
                                               ||b.flowName AS text,
                                               flowId
                                               || ' 藥品'
                                               || flowName  AS COMBITEM
                                        FROM   temp_mat_class a,
                                               ME_FLOW b
                                        WHERE  a.value = '01'
                                               AND b.doctype IN ( 'RN' )
                                        UNION
                                        SELECT DISTINCT DATA_VALUE   AS VALUE,
                                                        '衛材'
                                                        ||DATA_DESC  AS TEXT,
                                                        DATA_VALUE
                                                        || ' 衛材'
                                                        || DATA_DESC AS COMBITEM
                                        FROM   temp_mat_class a,
                                               PARAM_D b
                                        WHERE  a.value IN ( '02', '03', '04', '05',
                                                            '06', '07', '08' )
                                               AND b.GRP_CODE = 'ME_DOCM'
                                               AND b.DATA_NAME = 'FLOWID_RN1' 
                                        ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { USERID = userId }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassQCombo(string userId)
        {
            string sql = @"
                                        WITH temp_whkinds
                                             AS (SELECT b.wh_no,
                                                        b.wh_kind,
                                                        nvl(( CASE
                                                                WHEN a.task_id = '3' THEN '2'
                                                                ELSE a.task_id
                                                              END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  wh_userid = :userId
                                                        AND a.wh_no = b.wh_no
                                                        AND ( ( b.wh_grade IN ( '1', '2' )
                                                                AND a.task_id = '1' )
                                                               OR ( b.wh_grade = '1'
                                                                    AND a.task_id IN ( '2', '3' ) ) ))
                                        SELECT DISTINCT b.mat_class      AS value,
                                                        '全部'||b.mat_clsname    AS text,
                                                        b.mat_class
                                                        || ' '
                                                        ||'全部'||b.mat_clsname AS COMBITEM
                                        FROM   temp_whkinds a,
                                               MI_MATCLASS b
                                        WHERE  ( a.task_id = b.mat_clsid )
                                        union
                                        select 'SUB_' || b.data_value as value, b.data_desc as text,
                                        b.data_value || ' ' || b.data_desc as COMBITEM 
                                        from temp_whkinds a, PARAM_D b
	                                     where b.grp_code ='MI_MAST' 
	                                       and b.data_name = 'MAT_CLASS_SUB'
	                                       and b.data_value = '1'
	                                       and trim(b.data_desc) is not null
                                           and (a.task_id = '1')
                                        union
                                        select 'SUB_' || b.data_value as value, b.data_desc as text,
                                        b.data_value || ' ' || b.data_desc as COMBITEM 
                                        from temp_whkinds a, PARAM_D b
	                                     where b.grp_code ='MI_MAST' 
	                                       and b.data_name = 'MAT_CLASS_SUB'
	                                       and b.data_value <> '1'
	                                       and trim(b.data_desc) is not null
                                           and (a.task_id = '2')
                                        ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { USERID = userId }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo(string userId)
        {
            string sql = @"
                                        WITH temp_whkinds
                                             AS (SELECT b.wh_no,
                                                        b.wh_kind,
                                                        nvl(( CASE
                                                                WHEN a.task_id = '3' THEN '2'
                                                                ELSE a.task_id
                                                              END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  wh_userid = :userId
                                                        AND a.wh_no = b.wh_no
                                                        AND ( ( b.wh_grade IN ( '1', '2' )
                                                                AND a.task_id = '1' )
                                                               OR ( b.wh_grade = '1'
                                                                    AND a.task_id IN ( '2', '3' ) ) ))
                                        SELECT DISTINCT b.mat_class      AS value,
                                                        b.mat_clsname    AS text,
                                                        b.mat_class
                                                        || ' '
                                                        || b.mat_clsname AS COMBITEM
                                        FROM   temp_whkinds a,
                                               MI_MATCLASS b
                                        WHERE  ( a.task_id = b.mat_clsid )
                                        union
                                        SELECT DISTINCT b.mat_class      AS value,
                                                        b.mat_clsname    AS text,
                                                        b.mat_class
                                                        || ' '
                                                        || b.mat_clsname AS COMBITEM
                                        FROM   temp_whkinds a,
                                               MI_MATCLASS b
                                        WHERE  ( a.task_id = '2' )
                                               AND b.mat_clsid = '3' 
                                        ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { USERID = userId }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetMeDocExpWexpidNs(string docno)
        {
            string sql = @"
               select NVL((select STORE_LOC from MI_WLOCINV where mmcode = a.mmcode and wh_no =  b.frwh and ROWNUM = 1),'TMPLOC')  STORE_LOC, 
                           a.docno, a.mmcode, a.lot_no, twn_date(a.exp_date) as EXPDATE, a.apvqty as APPQTY,b.FRWH,b.TOWH
                  from ME_DOCEXP a, ME_DOCM b
                 where a.docno = :docno and a.docno = b.docno
                   and exists(select 1 from MI_MAST where mmcode = a.mmcode and nvl(trim(wexp_id), 'N') = 'N') 
            ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno }, DBWork.Transaction);
        }
    }
}