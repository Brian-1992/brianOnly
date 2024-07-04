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
using WebApp.Models.AB;

namespace WebApp.Repository.AB
{
    public class AB0112Repository : JCLib.Mvc.BaseRepository
    {
        public AB0112Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(ME_DOCM_QUERY_PARAMS query, string[] str_FLOWID, string[] str_matclass, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" 
                                    SELECT docno, --單據號碼,
                                           (SELECT mat_class || ' ' || mat_clsname FROM mi_matclass WHERE mat_class = a.mat_class) mat_class, --物料分類,
                                           TWN_DATE(apptime) AS apptime, --繳回申請時間,
                                           towh || ' ' || WH_NAME(towh) AS towh, --申請庫房,
                                           frwh || ' ' || WH_NAME(frwh) AS frwh, --繳回庫房, 
                                           (CASE
                                                  WHEN mat_class = '01' THEN
                                                         (
                                                                SELECT flowid
                                                                              || ' 藥品'
                                                                              || flowname
                                                                FROM   me_flow
                                                                WHERE  doctype IN ('RN')
                                                                AND    flowid = a.flowid)
                                                  ELSE
                                                        (
                                                        SELECT data_value
                                                                      || ' 衛材'
                                                                      || data_desc
                                                        FROM   param_d
                                                        WHERE  grp_code = 'ME_DOCM'
                                                        AND    data_name = 'FLOWID_RN1'
                                                        AND    a.flowid = data_value)
                                           END) flowid, --單據狀態,
                                           apply_note, --備註
                                           appid || ' ' || USER_NAME(appid) AS app_name,
                                           user_name(appid) AS appid,
                                           appdept || ' ' || INID_NAME(appdept) AS appdept_name
                                    FROM   me_docm a
                                    WHERE  1=1
                                    AND    doctype IN ('RN','RN1')
                                    AND    (a.frwh IN ( SELECT wh_no FROM   mi_whid WHERE  wh_userid = :userId) or
                                                 (select count(*) from UR_UIR where RLNO in ('MAT_14','MED_14') and TUSER = :userId) > 0)";

            if (!string.IsNullOrWhiteSpace(query.USERID))
            {
                p.Add(":userId", query.USERID);
            }

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

        public IEnumerable<COMBO_MODEL> GetFlowidCombo(string userId)
        {
            string sql = @"
                                        WITH temp_whkinds
                                             AS (SELECT b.wh_no, b.wh_kind,
                                                        nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  wh_userid = :userId
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '1', '2', '3' )
                                                 union
                                                     SELECT b.wh_no,  b.wh_kind,
                                                        nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  (select count(*) from UR_UIR where RLNO = 'MAT_14' and TUSER = :userId) > 0
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '2', '3' )
                                                  union
                                                     SELECT b.wh_no,  b.wh_kind,
                                                        nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  (select count(*) from UR_UIR where RLNO = 'MED_14' and TUSER = :userId) > 0
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '1' )
                                             ), 
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
                                        WHERE  a.value IN ( '02', '03', '04', '05','06', '07', '08' )
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
                                                        nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  wh_userid = :userId
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '1', '2', '3' )
                                                 union
                                                     SELECT b.wh_no,  b.wh_kind,
                                                        nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  (select count(*) from UR_UIR where RLNO = 'MAT_14' and TUSER = :userId) > 0
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '2', '3' )
                                                  union
                                                     SELECT b.wh_no,  b.wh_kind,
                                                        nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  (select count(*) from UR_UIR where RLNO = 'MED_14' and TUSER = :userId) > 0
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '1' )
                                                    ) 
                                        SELECT DISTINCT b.mat_class AS value,
                                                        b.mat_clsname AS text,
                                                        b.mat_class
                                                        || ' ' || b.mat_clsname AS COMBITEM
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
                                        UNION
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
                                                        nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  wh_userid = :userId
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '1', '2', '3' )
                                                 union
                                                     SELECT b.wh_no,  b.wh_kind,
                                                        nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  (select count(*) from UR_UIR where RLNO = 'MAT_14' and TUSER = :userId) > 0
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '2', '3' )
                                                  union
                                                     SELECT b.wh_no,  b.wh_kind,
                                                        nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  (select count(*) from UR_UIR where RLNO = 'MED_14' and TUSER = :userId) > 0
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '1' )
                                        )
                                       SELECT DISTINCT b.mat_class  AS value,
                                                        b.mat_clsname    AS text,
                                                        b.mat_class|| ' ' || b.mat_clsname AS COMBITEM
                                        FROM   temp_whkinds a,
                                               MI_MATCLASS b
                                        WHERE  ( a.task_id = b.mat_clsid )
                                        ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { USERID = userId }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassComboByFrwh(string userId, string frwh_no)
        {
            string sql = @"
                                          WITH temp_whkinds
                                             AS (SELECT b.wh_no,
                                                        b.wh_kind,
                                                        Nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  wh_userid = :userId
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '1', '2', '3' )
                                                        AND b.wh_kind = (SELECT wh_kind FROM   mi_whmast  WHERE  wh_no = :wh_no)
                                               union
                                                     SELECT b.wh_no,  b.wh_kind,
                                                        nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  (select count(*) from UR_UIR where RLNO = 'MAT_14' and TUSER = :userId) > 0
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '2', '3' )
                                                        AND b.wh_kind = (SELECT wh_kind FROM   mi_whmast  WHERE  wh_no = :wh_no)
                                               union
                                                     SELECT b.wh_no,  b.wh_kind,
                                                        nvl(( CASE WHEN a.task_id = '3' THEN '2' ELSE a.task_id END ), '2') AS task_id
                                                 FROM   MI_WHID a,
                                                        MI_WHMAST b
                                                 WHERE  (select count(*) from UR_UIR where RLNO = 'MED_14' and TUSER = :userId) > 0
                                                        AND a.wh_no = b.wh_no
                                                        AND b.wh_grade NOT IN ( '1', '5' )
                                                        AND a.task_id IN ( '1' )
                                                        AND b.wh_kind = (SELECT wh_kind FROM   mi_whmast  WHERE  wh_no = :wh_no)
                                                 )
                                        SELECT DISTINCT b.mat_class      AS value,
                                                        b.mat_clsname    AS text,
                                                        b.mat_class || ' '|| b.mat_clsname AS COMBITEM
                                        FROM   temp_whkinds a,
                                               MI_MATCLASS b
                                        WHERE  ( a.task_id = b.mat_clsid )
                                        UNION
                                        SELECT DISTINCT b.mat_class      AS value,
                                                        b.mat_clsname    AS text,
                                                        b.mat_class|| ' ' || b.mat_clsname AS COMBITEM
                                        FROM   temp_whkinds a,
                                               MI_MATCLASS b
                                        WHERE  ( a.task_id = '2' )
                                               AND b.mat_clsid = '3' 
                                      ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { USERID = userId, wh_no = frwh_no }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCEXP> GetMeDocexp(ME_DOCEMP_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"     SELECT A.DOCNO, --單號,
                                       A.SEQ, --項次,
                                       A.MMCODE, --院內碼,
                                       A.APVQTY, --繳回數量,
                                       A.ITEM_NOTE, --備註
                                       B.MMNAME_C, --中文品名,
                                       B.MMNAME_E, --英文品名,
                                       A.LOT_NO,--批號
                                       TWN_DATE(A.EXP_DATE) EXP_DATE,--效期
                                       B.BASE_UNIT, --計量單位,
                                       B.DISC_UPRICE, --優惠最小單價,
                                       (t.INV_QTY - A.APVQTY) LAST_QTY, -- 繳回後剩餘庫存量
                                       t.INV_QTY, --出庫庫房存量,
                                       C.FLOWID --狀態
                                FROM   ME_DOCEXP A
                                       INNER JOIN MI_MAST B ON B.MMCODE = A.MMCODE
                                       INNER JOIN ME_DOCM C ON C.DOCNO = A.DOCNO
                                       LEFT JOIN mi_whinv t on t.mmcode = a.mmcode and t.wh_no = c.frwh
                                WHERE  1 = 1 ";

            if (query.DOCNO != "")
            {
                sql += " AND a.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCEXP>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
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

        //public string GetMaxSeq(string docno)
        //{
        //    var sql = @"SELECT MAX(SEQ) + 1 AS MAXSEQ FROM ME_DOCD WHERE DOCNO=:DOCNO";
        //    return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        //}

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

        public IEnumerable<MI_WHID> GetWhTaskId(string userId)
        {
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
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,A.DISC_UPRICE, 
                               ( CASE WHEN A.M_STOREID = '1'
                                 AND A.M_CONTID <> '3'
                                 AND A.M_APPLYID <> 'E' THEN 'Y' ELSE 'X' END) AS M_PAYID,
                               (SELECT DATA_VALUE||' '||DATA_DESC SPE 
                               FROM PARAM_D WHERE GRP_CODE='MI_MAST' 
                               AND DATA_NAME='M_STOREID' AND DATA_VALUE=A.M_STOREID) M_STOREID,
                               (SELECT DATA_VALUE||' '||DATA_DESC SPE 
                               FROM PARAM_D WHERE GRP_CODE='MI_MAST' 
                               AND DATA_NAME='M_CONTID' AND DATA_VALUE=A.M_CONTID) M_CONTID,
                               (SELECT DATA_VALUE||' '||DATA_DESC SPE 
                               FROM PARAM_D WHERE GRP_CODE='MI_MAST' 
                               AND DATA_NAME='M_APPLYID' AND DATA_VALUE=A.M_APPLYID) M_APPLYID,
                                INV_QTY(WHNO_MM1, A.MMCODE) as S_INV_QTY,
                                A.M_AGENNO,
                                (select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                                (select AGEN_NAMEE from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEE
                        FROM MI_MAST A
                        LEFT JOIN 
                        (select * from MI_WHINV b where wh_no = 
                            (SELECT FRWH
                            FROM   ME_DOCM
                            WHERE  DOCNO = :DOCNO)
                        ) B ON A.mmcode = B.mmcode
                        WHERE nvl(B.INV_QTY,0)>0 
                        AND A.MAT_CLASS = (SELECT MAT_CLASS from ME_DOCM WHERE DOCNO = :DOCNO) ";

            p.Add(":DOCNO", string.Format("{0}", query.DOCNO));

            if (query.MMCODE != "")
            {
                sql += " AND UPPER(A.MMCODE) LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND UPPER(A.MMNAME_E) LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            if (query.M_AGENNO != "")
            {
                sql += " AND A.M_AGENNO LIKE :M_AGENNO ";
                p.Add(":M_AGENNO", string.Format("%{0}%", query.M_AGENNO));
            }
            if (query.AGEN_NAME != "")
            {
                sql += " AND ((select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.M_AGENNO) LIKE :AGEN_NAME ";
                sql += "  OR (select AGEN_NAMEE from PH_VENDER where AGEN_NO = A.M_AGENNO) LIKE :AGEN_NAME) ";
                p.Add(":AGEN_NAME", string.Format("%{0}%", query.AGEN_NAME));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string docno, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                        SELECT {0} 
                                               a.mmcode,
                                               a.mmname_c,
                                               a.mmname_e,
                                               a.mat_class,
                                               a.base_unit,
                                               NVL(b.inv_qty, 0) AS inv_qty
                                        FROM   mi_mast a
                                               LEFT JOIN 
                                               (select * from MI_WHINV b where wh_no = (SELECT frwh
                                                                        FROM   me_docm
                                                                        WHERE  docno = :docno)) b ON a.mmcode = b.mmcode

                                        WHERE  1 = 1
                                               AND a.mat_class IN ( '02', '03', '04', '05',
                                                                    '06', '07', '08' )
                                               AND a.mat_class = (SELECT mat_class
                                                                  FROM   me_docm
                                                                  WHERE  docno = :docno)
                                               AND NVL(b.inv_qty, 0) > 0
                                        UNION
                                        SELECT {0}
                                               a.mmcode,
                                               a.mmname_c,
                                               a.mmname_e,
                                               a.mat_class,
                                               a.base_unit,
                                               NVL(b.inv_qty, 0) AS inv_qty
                                        FROM   mi_mast a
                                              LEFT JOIN 
                                               (select * from MI_WHINV b where wh_no = (SELECT frwh
                                                                        FROM   me_docm
                                                                        WHERE  docno = :docno)) b ON a.mmcode = b.mmcode

                                        WHERE  1 = 1
                                               AND a.mat_class = '01'
                                               AND a.mat_class = (SELECT mat_class
                                                                  FROM   me_docm
                                                                  WHERE  docno = :docno)
                                               AND NVL(b.inv_qty, 0) > 0 
                                    ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql = string.Format("SELECT * FROM ({0}) TMP WHERE 1=1 ", sql);

                sql += " AND (UPPER(MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add(":docno", docno);
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
        public bool CheckMmcodeExist(string docno, string mmcode)
        {
            string sql = @"
                                        SELECT 1
                                        FROM   MI_WHINV
                                        WHERE  wh_no = (SELECT frwh
                                                        FROM   ME_DOCM
                                                        WHERE  docno = :DOCNO)
                                               AND mmcode = :MMCODE 
                                        ";

            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        // 檢查申請單是否有院內碼項次
        public bool CheckMeDocexpExists_1(ME_DOCEXP me_docexp)
        {
            string sql = @" SELECT 1 
                            FROM ME_DOCEXP 
                            WHERE DOCNO = :DOCNO 
                            AND MMCODE = :MMCODE 
                            AND SEQ != :SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, me_docexp, DBWork.Transaction) == null);
        }

        // 檢查申請單是否有院內碼項次,要送申請時,必須一定要有院內碼項次
        public bool CheckMeDocexpExists(string docno)
        {
            string sql = @" SELECT 1 
                            FROM ME_DOCEXP 
                            WHERE DOCNO = :DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
        }

        // 檢查申請單院內碼項次的申請數量不得<=0,有的話則不能提出申請
        public bool CheckMeDocexpAppqty(string docno)
        {
            string sql = @" SELECT 1 
                            FROM ME_DOCEXP 
                            WHERE DOCNO = :DOCNO
                            AND APVQTY <= 0";
            return (DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
        }

        // 檢查申請單院內碼項次的繳回數量不可>現有庫存量,有的話則不可繳回
        public bool CheckAppqtyInvqty(string docno)
        {
            string sql = @"
                                        SELECT 1
                                        FROM   (SELECT A.APVQTY,--繳回數量,
                                                       (SELECT INV_QTY
                                                        FROM   MI_WHINV
                                                        WHERE  MMCODE = A.MMCODE
                                                               AND WH_NO = C.FRWH) INV_QTY --出庫庫房存量,
                                                FROM   ME_DOCEXP A
                                                       inner join MI_MAST B
                                                               ON B.MMCODE = A.MMCODE
                                                       inner join ME_DOCM C
                                                               ON C.DOCNO = A.DOCNO
                                                WHERE  A.DOCNO = :DOCNO) D
                                        WHERE  D.APVQTY > D.INV_QTY 
                                      ";
            return (DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
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
            var sql = @" DELETE FROM ME_DOCEXP 
                         WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DetailCreate(ME_DOCEXP me_docexp)
        {
            var sql = @" INSERT INTO ME_DOCEXP (DOCNO, SEQ, MMCODE, LOT_NO, EXP_DATE, APVQTY, ITEM_NOTE,  UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                         VALUES (:DOCNO, :SEQ, :MMCODE, :LOT_NO, TO_DATE(:EXP_DATE,'YYYY/MM/DD'), :APVQTY, :ITEM_NOTE, SYSDATE, :UPDATE_USER, :UPDATE_IP)";

            return DBWork.Connection.Execute(sql, me_docexp, DBWork.Transaction);
        }

        public int DetailUpdate(ME_DOCEXP me_docexp)
        {
            var sql = @" UPDATE ME_DOCEXP 
                         SET APVQTY = :APVQTY, 
                             LOT_NO = :LOT_NO,
                             EXP_DATE = TO_DATE(:EXP_DATE,'YYYY/MM/DD'),
                             ITEM_NOTE = :ITEM_NOTE,
                             UPDATE_TIME = SYSDATE, 
                             UPDATE_USER = :UPDATE_USER,
                             UPDATE_IP = :UPDATE_IP 
                         WHERE DOCNO = :DOCNO
                         AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, me_docexp, DBWork.Transaction);
        }

        public int DetailDelete(string docno, string seq)
        {
            var sql = @" DELETE FROM ME_DOCEXP 
                         WHERE DOCNO = :DOCNO
                         AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public int UpdateStatus(ME_DOCM me_docm)
        {
            var sql = "";
            sql = @" INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, APLYITEM_NOTE, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                         SELECT DOCNO, SEQ, MMCODE, APVQTY, ITEM_NOTE, SYSDATE, :UPDATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP
                         FROM  ME_DOCEXP
                         WHERE DOCNO = :DOCNO";
            //var sql = @"UPDATE ME_DOCD SET APL_CONTIME = SYSDATE WHERE DOCNO = :DOCNO";
            DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
            sql = @"
                                UPDATE ME_DOCM
                                SET    FLOWID = ( CASE
                                                    WHEN mat_class = '01' THEN '0402'
                                                    ELSE '2'
                                                  END ),
                                       UPDATE_TIME = SYSDATE,
                                       UPDATE_USER = :UPDATE_USER,
                                       UPDATE_IP = :UPDATE_IP
                                WHERE  DOCNO = :DOCNO 
                            ";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public string GetDocexpSeq(string docno)
        {
            string sql = @"SELECT NVL(MAX(SEQ),0) + 1 as SEQ 
                           FROM ME_DOCEXP 
                           WHERE DOCNO = :DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction).ToString();
            return rtn;
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
            public string FLOWID;
            public string MAT_CLASS;
            public string APPTIME_S;
            public string APPTIME_E;
            public string USERID;
        }

        public class ME_DOCEMP_QUERY_PARAMS
        {
            public string DOCNO;
            public string FRWH;
            public string TOWH;
        }

        public class MI_MAST_QUERY_PARAMS
        {
            public string DOCNO;
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥

            public string M_AGENNO;
            public string AGEN_NAME;
        }

        public IEnumerable<COMBO_MODEL> GetFrwhCombo(string id)
        {
            string sql = @"
                                        SELECT b.wh_no      AS value,
                                               b.wh_name    AS text,
                                               b.wh_no ||' ' || b.wh_name AS combitem,
                                               b.wh_kind,
                                               b.wh_grade
                                        FROM   MI_WHID a,
                                               MI_WHMAST b
                                        WHERE  wh_userid = :userId
                                               AND a.wh_no = b.wh_no
                                               AND b.wh_grade NOT IN ( '1', '5' )
                                               AND a.task_id IN ( '1', '2', '3' )
                                               AND nvl(b.cancel_id, 'N') = 'N' 
                                         union
                                        SELECT b.wh_no      AS value,
                                               b.wh_name    AS text,
                                               b.wh_no ||' ' || b.wh_name AS combitem,
                                               b.wh_kind,
                                               b.wh_grade
                                        FROM   MI_WHID a,
                                               MI_WHMAST b
                                        WHERE  (select count(*) from UR_UIR where RLNO = 'MAT_14' and TUSER = :userId) > 0
                                               AND a.wh_no = b.wh_no
                                               AND b.wh_grade NOT IN ( '1', '5' )
                                               AND a.task_id IN ( '2', '3' )
                                               AND nvl(b.cancel_id, 'N') = 'N' 
                                        union
                                        SELECT b.wh_no      AS value,
                                               b.wh_name    AS text,
                                               b.wh_no ||' ' || b.wh_name AS combitem,
                                               b.wh_kind,
                                               b.wh_grade
                                        FROM   MI_WHID a,
                                               MI_WHMAST b
                                        WHERE  (select count(*) from UR_UIR where RLNO = 'MED_14' and TUSER = :userId) > 0
                                               AND a.wh_no = b.wh_no
                                               AND b.wh_grade NOT IN ( '1', '5' )
                                               AND a.task_id IN ( '1' )
                                               AND nvl(b.cancel_id, 'N') = 'N' 
                                        ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { userId = id });
        }

        public IEnumerable<COMBO_MODEL> GetTowhCombo(string Frwh_no)
        {
            var p = new OracleDynamicParameters();

            string sql = @"
                                        SELECT a.wh_no      AS value,
                                               a.wh_name    AS text,
                                               a.wh_no ||' ' || a.wh_name AS combitem,
                                               a.wh_kind,
                                               a.wh_grade
                                        FROM   MI_WHMAST a
                                        WHERE  a.wh_grade IN ( '1', '2' )
                                               AND a.wh_kind = (SELECT wh_kind FROM mi_whmast WHERE wh_no = :wh_no)
                                               AND a.wh_grade < (SELECT wh_grade FROM mi_whmast WHERE wh_no = :wh_no)
                                               AND nvl(a.cancel_id, 'N') = 'N' 
                                        ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { wh_no = Frwh_no });
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
        public bool CheckPastYear(string wh_no, string mmcode)
        {
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
            if (result)
            {
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

        public bool CheckIsFrwhCancelByDocno(string docno)
        {
            string sql = @"
                                        SELECT nvl(cancel_id, 'N')
                                        FROM   MI_WHMAST
                                        WHERE  wh_no = (SELECT frwh
                                                        FROM   ME_DOCM
                                                        WHERE  docno = :docno) 
                                        ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) == null;
        }

        public bool CheckIsTowhCancelByDocno(string docno)
        {
            string sql = @"
                                        SELECT nvl(cancel_id, 'N')
                                        FROM   MI_WHMAST
                                        WHERE  wh_no = (SELECT towh
                                                        FROM   ME_DOCM
                                                        WHERE  docno = :docno) 
                                        ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) == null;
        }
        #endregion

        // 檢查出庫庫房與入庫庫房是否WH_KIND相同
        public bool CheckIsSameWhKind(string frwh, string towh)
        {
            string sql = @"
                                        SELECT CASE
                                                 WHEN frwh = towh THEN frwh
                                                 ELSE ''
                                               END result
                                        FROM   (SELECT (SELECT wh_kind
                                                        FROM   mi_whmast
                                                        WHERE  wh_no = :FRWH) frwh,
                                                       (SELECT wh_kind
                                                        FROM   mi_whmast
                                                        WHERE  wh_no = :TOWH) towh
                                                FROM   DUAL) A 
                                        ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { FRWH = frwh, TOWH = towh }, DBWork.Transaction) == null);
        }

        // 檢查單號是否存在
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        //國軍DOCNO單號統一用GET_DAILY_DOCNO
        public string GetDailyDocno()
        {
            string sql = @"select GET_DAILY_DOCNO from DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public bool CheckExistsM1(string id)
        {
            string sql = @" SELECT 1 
                            FROM ME_DOCM 
                            WHERE DOCNO = :DOCNO 
                            AND SUBSTR(FLOWID, -1, 1)<>'2' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public int UpdateStatus1(ME_DOCM me_docm)
        {
            string sql = "";
            sql = @" DELETE FROM ME_DOCD 
                     WHERE DOCNO = :DOCNO";
            DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);

            sql = @" UPDATE ME_DOCM
                        SET FLOWID = ( CASE WHEN mat_class = '01' THEN '0401'
                                            ELSE '1'
                                       END ),
                            UPDATE_TIME = SYSDATE,
                            UPDATE_USER = :UPDATE_USER,
                            UPDATE_IP = :UPDATE_IP
                     WHERE  DOCNO = :DOCNO  ";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }
        // 檢查繳回數量是否大於庫存量，若<=0跳出錯誤訊息
        public bool CheckAppqtyLargerThanInvqty(string docno, string mmcode, string appqty) {
            string sql = @"
                select (case when :appqty > inv_qty then 'Y' else 'N' end)
                  from MI_WHINV
                 where wh_no = (select frwh from ME_DOCM where docno = :docno)
                   and mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { docno, mmcode, appqty }, DBWork.Transaction) == "Y";
        }

        public IEnumerable<ME_DOCD> GetPrintData(string p0)
        {
            var p = new DynamicParameters();
            var sql = @" select 
                A.SEQ, A.MMCODE, B.MMNAME_C, A.APPQTY, B.BASE_UNIT, B.UPRICE, 
                A.APPQTY * B.UPRICE as APP_AMT, 
                AGEN_NAME(B.M_AGENNO) as M_AGENNO, 
                (select twn_date(MAX(EXP_DATE)) FROM MI_WEXPINV where WH_NO=(select FRWH from ME_DOCM where DOCNO=A.DOCNO) and MMCODE=A.MMCODE) as EXPDATE, 
                A.APLYITEM_NOTE as DRUGMEMO 
                FROM ME_DOCD A, MI_MAST B 
                WHERE 1=1 
                and A.DOCNO = :P0 
                and A.MMCODE = B.MMCODE 
                ORDER BY A.SEQ ";

            p.Add(":P0", string.Format("{0}", p0));

            return DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);

        }

        public DataTable GetPrintTitleData(string docno)
        {
            var p = new DynamicParameters();
            var sql = @"select TWN_DAT_FORMAT(SYSDATE) as PrintTime, 
                TWN_DAT_FORMAT(A.APPTIME) || ' ' || TO_CHAR(A.APPTIME,'HH24') || '時' || TO_CHAR(A.APPTIME,'MI') || '分' as apptime,
                WH_NAME(A.TOWH) as supplyWhName,
                INID_NAME(A.APPDEPT) as appInidName,
                nvl((select round(SUM(C.APPQTY * (select UPRICE from MI_MAST where MMCODE=C.MMCODE))) from ME_DOCD C where C.DOCNO=A.DOCNO), 0) as sumAmt,
                A.APPLY_NOTE as memo,
                (select DATA_DESC from PARAM_D where GRP_CODE = 'ME_DOCM' and DATA_NAME='ISARMY' and DATA_VALUE = A.ISARMY) as isArmy,
                A.APPID || ' ' || USER_NAME(A.APPID) as appUser,
                (select wh_no || ' ' || wh_name from mi_whmast where wh_no = A.FRWH) as frwh
                from ME_DOCM A
                where A.DOCNO = :DOCNO ";

            p.Add(":DOCNO", string.Format("{0}", docno));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<MI_WEXPINV> GetLotno(string mmcode, string docno, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT DISTINCT A.MMCODE, A.LOT_NO , TWN_DATE(A.EXP_DATE) as EXP_DATE , A.INV_QTY  
                        FROM MI_WEXPINV  A 
                        WHERE A.WH_NO=(select frwh from ME_DOCM where docno = :DOCNO)
                        AND A.MMCODE=:MMCODE
                        ORDER BY A.LOT_NO   ";

            p.Add(":MMCODE", mmcode);
            p.Add(":DOCNO", docno);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WEXPINV>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        //批號+效期+效期數量combox
        public IEnumerable<MI_WEXPINV> GetLOT_NO(string FRWH, string MMCODE)
        {
            string sql = @"  SELECT MWV.LOT_NO LOT_NO,
                                    (TO_CHAR (MWV.EXP_DATE, 'YYYYMMDD') - 19110000) EXP_DATE,
                                    MWV.INV_QTY
                             FROM MI_WEXPINV MWV
                             WHERE MWV.WH_NO = :FRWH
                             AND MWV.MMCODE = :MMCODE";

            return DBWork.Connection.Query<MI_WEXPINV>(sql, new { FRWH = FRWH, MMCODE = MMCODE }, DBWork.Transaction);
        }
        //帶出效期
        public string GetEXP_DATE(string FRWH, string MMCODE, string LOT_NO)
        {
            string sql = @"  SELECT (TO_CHAR (MWV.EXP_DATE, 'YYYYMMDD') - 19110000) EXP_DATE
                             FROM MI_WEXPINV MWV
                             WHERE MWV.WH_NO = :FRWH
                             AND MWV.MMCODE = :MMCODE
                             AND MWV.LOT_NO = :LOT_NO";

            return DBWork.Connection.Query<string>(sql, new { FRWH = FRWH, MMCODE = MMCODE }, DBWork.Transaction).ToString();
        }
    }
}