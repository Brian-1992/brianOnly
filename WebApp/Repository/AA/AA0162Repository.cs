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
    public class AA0162Repository : JCLib.Mvc.BaseRepository
    {
        public AA0162Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string docno, string apptime1, string apptime2, string[] flowid, string mat_class, string tuser, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            string sql = string.Format(@"SELECT DOCNO, TWN_DATE(APPTIME) as APPTIME, 
                    APPID || ' ' || USER_NAME(APPID) as APP_NAME, 
                    USER_NAME(APPID) as APPID , 
                    APPDEPT || ' ' || INID_NAME(APPDEPT) as APPDEPT_NAME, 
                    (SELECT FLOWID || ' ' || FLOWNAME FROM ME_FLOW WHERE DOCTYPE in ('SP','SP1') and FLOWID=A.FLOWID) as FLOWID, 
                    (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) as MAT_CLASS, 
                    FRWH || ' ' || WH_NAME(FRWH) FRWH_N, 
                    FRWH, 
                    APPLY_NOTE,UPDATE_TIME,
                    (select count(*) from V_ME_DOCD_E B where B.DOCNO = A.DOCNO 
                        and (EXISTS(SELECT 'X' FROM MI_WHID C WHERE C.WH_NO IN (B.TOWH,B.FRWH) AND WH_USERID=:WH_USERID)
                        or (EXISTS(SELECT 'X' FROM UR_ID D,MI_WHMAST E WHERE D.INID=E.INID AND D.TUSER=:WH_USERID)))) as TOTAL
                    FROM ME_DOCM A
                    WHERE A.FLOWID in ('0501','0599','1','3','X')
                    AND A.DOCTYPE in ('SP','SP1')
                    AND A.MAT_CLASS in (select MAT_CLASS from MI_MATCLASS where MAT_CLSID in ('1','2'))
            ");

            if (docno != string.Empty)
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", docno));
            }
            if (apptime1 != string.Empty)
            {
                sql += " and TWN_DATE(A.APPTIME) >= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime1));
            }
            if (apptime2 != string.Empty)
            {
                sql += " and TWN_DATE(A.APPTIME) <= :p2 ";
                p.Add(":p2", string.Format("{0}", apptime2));
            }
            if (flowid.Length > 0)
            {
                sql += " and A.FLOWID in :p3 ";
                p.Add(":p3", flowid);
            }
            if (mat_class != string.Empty)
            {
                if (mat_class.Contains("SUB_"))
                {
                    sql += " and (select count(*) from ME_DOCD C left join MI_MAST D on C.MMCODE = D.MMCODE where A.DOCNO = C.DOCNO and D.MAT_CLASS_SUB = :p4) > 0";
                    p.Add(":p4", mat_class.Replace("SUB_", ""));
                }
                else
                {
                    sql += " and A.MAT_CLASS = :p4 ";
                    p.Add(":p4", mat_class);
                }    
            }
            p.Add(":WH_USERID", tuser);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCEXP> GetAllD(string docno, string wh_no, int page_index, int page_size, string sorters)
        {
            //C_AMT改寫死方式(新增或更新的時候寫入)
            string sql = @"SELECT A.DOCNO, A.SEQ, A.EXP_DATE, A.LOT_NO, A.LOT_NO LOT_NO_N, 
                                        TWN_DATE(A.EXP_DATE) EXP_DATE_T, A.MMCODE, A.APVQTY, A.C_UP, B.MMNAME_C, B.MMNAME_E, 
                                        B.BASE_UNIT, A.C_AMT,
                                                A.ITEM_NOTE, C.INV_QTY AS INV_QTY 
                                            FROM
                                                ME_DOCEXP A 

                                            INNER JOIN
                                                MI_MAST B 
                                                    ON B.MMCODE = A.MMCODE 
                                            INNER JOIN
                                                MI_WHINV C 
                                                    ON (
                                                        C.WH_NO = :WH_NO  
                                                        AND C.MMCODE = B.MMCODE 
                                                    ) 

                                            WHERE
                                                A.DOCNO = :DOCNO
                            ";

            var p = new DynamicParameters();
            p.Add(":DOCNO", string.Format("{0}", docno));
            p.Add(":WH_NO", string.Format("{0}", wh_no));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCEXP>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> GetReport(string docno, string apptime_bg, string apptime_ed, string mclass, string[] flowid, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT B.MMCODE,(select MAT_CLSNAME from MI_MATCLASS where MAT_CLASS=A.MAT_CLASS) as MAT_CLASS,
                               C.MMNAME_C,C.MMNAME_E,C.BASE_UNIT,B.APPQTY,C.DISC_CPRICE,
                               TWN_DATE(A.APPTIME) APPTIME ,
                               A.DOCNO,C.M_AGENNO ,A.APPLY_NOTE
                          FROM ME_DOCM A, ME_DOCD B, MI_MAST C
                         WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                               AND A.FLOWID in ('0501','0599','1','3','X')
                               AND A.DOCTYPE in ('SP','SP1')
                               AND A.MAT_CLASS in (select MAT_CLASS from MI_MATCLASS where MAT_CLSID in ('1','2'))
            ";

            if (docno != string.Empty)
            {
                sql += " AND A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", docno));
            }
            if (apptime_bg != "")
            {
                sql += " and TWN_DATE(A.APPTIME) >= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime_bg));
            }
            if (apptime_ed != "")
            {
                sql += " and TWN_DATE(A.APPTIME) <= :p2 ";
                p.Add(":p2", string.Format("{0}", apptime_ed));
            }
            if (flowid.Length > 0)
            {
                sql += " and A.FLOWID in :p3 ";
                p.Add(":p3", flowid);
            }
            if (mclass != string.Empty)
            {
                if (mclass.Contains("SUB_"))
                {
                    sql += " and (select count(*) from ME_DOCD D left join MI_MAST E on D.MMCODE = E.MMCODE where A.DOCNO = D.DOCNO and E.MAT_CLASS_SUB = :p4) > 0";
                    p.Add(":p4", mclass.Replace("SUB_", ""));
                }
                else
                {
                    sql += " and A.MAT_CLASS = :p4 ";
                    p.Add(":p4", mclass);
                }
            }
            if (mmcode != "")
            {
                sql += " and B.MMCODE = :p5 ";
                p.Add(":p5", mmcode);
            }

            sql += " order by A.DOCNO, B.MMCODE";

            return DBWork.Connection.Query<ME_DOCM>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,A.DISC_UPRICE, A.DISC_CPRICE, 
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
                        WHERE A.MAT_CLASS = (select mat_class from ME_DOCM where docno = :DOCNO) 
                           AND EXISTS ( SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO =(select frwh from ME_DOCM where docno = :DOCNO) ) ";

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

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,
                               A.M_CONTPRICE, A.DISC_CPRICE
                          FROM MI_MAST A
                         where 1=1
                           AND A.MAT_CLASS = (select mat_class from ME_DOCM where docno = :DOCNO) 
                           AND EXISTS ( SELECT 1 FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO =(select frwh from ME_DOCM where docno = :DOCNO) ) ";
            p.Add(":DOCNO", docno);

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, UPPER(:MMCODE_I)), 1000) + NVL(INSTR(A.MMNAME_E, UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE UPPER(:MMCODE) ";
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

        public IEnumerable<ComboItemModel> GetMclassQCombo(string tuser)
        {
            var sql = @"with temp_whkinds as (
                        select b.WH_NO, b.WH_KIND, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as TASK_ID
                        from MI_WHID a, MI_WHMAST b
                        where WH_USERID = :TUSER
                        and a.WH_NO = b.wh_no
                        and b.WH_GRADE = '1'
                        and a.TASK_ID in ('1','2','3')
                    )
                    select distinct b.MAT_CLASS as VALUE, '全部'||b.MAT_CLSNAME as TEXT,b.MAT_CLASS || ' ' || '全部' || b.MAT_CLSNAME as COMBITEM, a.WH_KIND as EXTRA1,
                    a.WH_NO || ' ' || WH_NAME(a.WH_NO) as EXTRA2
                    from temp_whkinds a, MI_MATCLASS b
                    where (a.TASK_ID = b.MAT_CLSID)
                    union
                    select 'SUB_' || b.data_value as value, b.data_desc as text,
                    b.data_value || ' ' || b.data_desc as COMBITEM, '' as EXTRA1, '' as EXTRA2 
                    from temp_whkinds a, PARAM_D b
	                    where b.grp_code ='MI_MAST' 
	                    and b.data_name = 'MAT_CLASS_SUB'
	                    and b.data_value = '1'
	                    and trim(b.data_desc) is not null
                        and (a.task_id = '1')
                    union
                    select 'SUB_' || b.data_value as value, b.data_desc as text,
                    b.data_value || ' ' || b.data_desc as COMBITEM, '' as EXTRA1, '' as EXTRA2
                    from temp_whkinds a, PARAM_D b
	                    where b.grp_code ='MI_MAST' 
	                    and b.data_name = 'MAT_CLASS_SUB'
	                    and b.data_value <> '1'
	                    and trim(b.data_desc) is not null
                        and (a.task_id = '2')
                    ";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMclassCombo(string tuser)
        {
            var sql = @"with temp_whkinds as (
                        select b.WH_NO, b.WH_KIND, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as TASK_ID
                        from MI_WHID a, MI_WHMAST b
                        where WH_USERID = :TUSER
                        and a.WH_NO = b.wh_no
                        and b.WH_GRADE = '1'
                        and a.TASK_ID in ('1','2','3')
                    )
                    select distinct b.MAT_CLASS as VALUE,b.MAT_CLSNAME as TEXT,b.MAT_CLASS || ' ' ||  b.MAT_CLSNAME as COMBITEM, a.WH_KIND as EXTRA1,
                    a.WH_NO || ' ' || WH_NAME(a.WH_NO) as EXTRA2
                    from temp_whkinds a, MI_MATCLASS b
                    where (a.TASK_ID = b.MAT_CLSID)
                    ";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetFlowidCombo()
        {
            var sql = @"select distinct flowid as VALUE, flowName as TEXT 
                    from ME_FLOW
                    where doctype in ('SP','SP1')
                    union
                    select '' as VALUE, '全部' as TEXT from dual
                    ";

            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }

        public int MasterCreate(ME_DOCM me_docm)
        {
            var sql = @"INSERT INTO ME_DOCM (DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT, APPTIME, MAT_CLASS, APPLY_NOTE, FRWH, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                    VALUES (:DOCNO, :DOCTYPE, :FLOWID, :APPID, :APPDEPT, SYSDATE, :MAT_CLASS, :APPLY_NOTE, :FRWH, SYSDATE
                                        , :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> MasterGet(string docno)
        {
            var sql = @" SELECT DOCNO FROM ME_DOCM WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public bool ChceckFlowId01(string docno)
        {
            string sql = @"
                select 1 from ME_DOCM
                 where docno = :docno
                   and (flowId = '0501'
                       or flowId = '1')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) != null;
        }

        public int MasterUpdate(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET MAT_CLASS=:MAT_CLASS, APPLY_NOTE=:APPLY_NOTE, APPID=:UPDATE_USER, APPDEPT = USER_INID(:UPDATE_USER), APPTIME = SYSDATE, USEID = :UPDATE_USER, USEDEPT = USER_INID(:UPDATE_USER),
                FRWH=:FRWH, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO";
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

        public int DetailCreate(ME_DOCEXP me_docexp)
        {
            var sql = @"INSERT INTO ME_DOCEXP (
                        DOCNO, SEQ, MMCODE , APVQTY , ITEM_NOTE, 
                        C_UP, C_AMT, EXP_DATE, LOT_NO,
                        UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APVQTY , :ITEM_NOTE, 
                        :C_UP, :C_UP*:APVQTY, TO_DATE(:EXP_DATE_T,'YYYY/MM/DD'), :LOT_NO, 
                        SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            
            return DBWork.Connection.Execute(sql, me_docexp, DBWork.Transaction);
        }

        public int DetailUpdate(ME_DOCEXP me_docd)
        {
            var sql = @"UPDATE ME_DOCEXP SET MMCODE=:MMCODE, APVQTY=:APVQTY, 
            EXP_DATE=TO_DATE(:EXP_DATE_T,'YYYY/MM/DD'), LOT_NO=:LOT_NO,C_AMT=C_UP*:APVQTY, ITEM_NOTE=:ITEM_NOTE, 
            UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailDelete(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCEXP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
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

        public IEnumerable<ME_DOCD> GetMeDocExpWexpidNs(string docno)
        {
            string sql = @"
               select NVL((select STORE_LOC from MI_WLOCINV where mmcode = a.mmcode and wh_no =  b.frwh and ROWNUM = 1),'TMPLOC')  STORE_LOC, 
                           a.docno, a.mmcode, a.lot_no,  twn_date(a.exp_date) as EXPDATE, a.apvqty as APPQTY,b.FRWH,b.TOWH
                  from ME_DOCEXP a, ME_DOCM b
                 where a.docno = :docno and a.docno = b.docno
                   and exists(select 1 from MI_MAST where mmcode = a.mmcode and nvl(trim(wexp_id), 'N') = 'N') 
            ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno }, DBWork.Transaction);
        }

        // 檢查申請單號是否存在
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

        //檢查是否存在重複的報廢項目
        public bool CheckExpExisted(ME_DOCEXP me_docd, bool isUpdate)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE AND LOT_NO=:LOT_NO 
            AND EXP_DATE = TO_DATE(:EXP_DATE_T,'YYYY/MM/DD') ";
            if (isUpdate)
            {
                sql += "AND SEQ!=:SEQ "; //如果是更新就增加條件
            }
            return !(DBWork.Connection.ExecuteScalar(sql, me_docd, DBWork.Transaction) == null);
        }
        public bool CheckExistsE(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public IEnumerable<ME_DOCM> GetDocEMmcode_All(string doc)
        {
            string sql = @"SELECT MMCODE FROM ME_DOCEXP WHERE DOCNO=:DOCNO ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = doc }, DBWork.Transaction);
        }
        public bool CheckExistsDMmcode(string doc, string mmcode)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc, MMCODE = mmcode }, DBWork.Transaction) == null);
        }
        public string GetDocEMmcodeApvqty(string doc, string mmcode)
        {
            string sql = @"SELECT SUM(APVQTY) FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND MMCODE = :MMCODE";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc, MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetDocEMmcodeNote(string doc, string mmcode)
        {
            string sql = @" SELECT LISTAGG(NVL(ITEM_NOTE,' '), ' ') WITHIN GROUP (ORDER BY mmcode) AS note FROM ME_DOCEXP WHERE DOCNO=:DOCNO AND MMCODE = :MMCODE GROUP BY mmcode";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = doc, MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
        }
        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public int CreateD(ME_DOCD docd)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE ,APPQTY, APLYITEM_NOTE,
                         CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY, :APLYITEM_NOTE,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, docd, DBWork.Transaction);
        }
        public class MI_MAST_QUERY_PARAMS
        {
            public string DOCNO;
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;
            public string WH_NO;

            public string M_AGENNO;
            public string AGEN_NAME;
        }
    }
}