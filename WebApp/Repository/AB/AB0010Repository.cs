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
    public class AB0010Repository : JCLib.Mvc.BaseRepository
    {
        public AB0010Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(ME_DOCM_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DOCNO, TWN_DATE(APPTIME) AS APPTIME
                    ,  APPID || ' ' || USER_NAME(APPID) AS APP_NAME
                    , USER_NAME(APPID) AS APPID 
                    , (SELECT FLOWID || ' ' || FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID AND DOCTYPE='MR') FLOWID
                    , APPDEPT || ' ' || INID_NAME(APPDEPT) AS APPDEPT_NAME
                    , TOWH || ' ' || WH_NAME(TOWH) AS TOWH_NAME
                    , FRWH || ' ' || WH_NAME(FRWH) AS FRWH_NAME
                    , TOWH, FRWH 
                    , RETURN_NOTE
                    FROM ME_DOCM A WHERE 1=1 ";

            if (query.DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :DOCNO ";
                p.Add(":DOCNO", string.Format("%{0}%", query.DOCNO));
            }

            //if (query.APPID != "")
            //{
            //    sql += " AND A.APPID LIKE :APPID ";
            //    p.Add(":APPID", string.Format("%{0}%", query.APPID));
            //}

            //if (query.APPDEPT != "")
            //{
            //    sql += " AND A.APPDEPT = :APPDEPT ";
            //    p.Add(":APPDEPT", string.Format("{0}", query.APPDEPT));
            //}
            sql += " AND WH_NO_AUTH(A.TOWH,A.APPID) = 'Y' ";
            if (query.DOCTYPE != "")
            {
                sql += " AND A.DOCTYPE = :DOCTYPE ";
                p.Add(":DOCTYPE", string.Format("{0}", query.DOCTYPE));
            }

            if (query.FLOWID != "")
            {
                string[] tmp = query.FLOWID.Split(',');
                sql += " AND A.FLOWID IN :FLOWID";
                p.Add(":FLOWID", tmp);
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

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetAllMeDocd(ME_DOCD_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.DOCNO, A.MMCODE, A.SEQ, A.APPQTY, A.APVQTY, A.ACKQTY, A.S_INV_QTY, A.INV_QTY, A.SAFE_QTY, A.OPER_QTY 
                        , A.PACK_QTY, A.PACK_UNIT, A.E_ORDERDCFLAG, a.PACK_TIMES
                        , C.TOWH AS WH_NO
                        , (SELECT NVL(APL_QTY_90(C.TOWH, a.mmcode), 0) FROM DUAL) AS SUGGEST_QTY
                        , (SELECT STORE_LOC('PH1S', a.MMCODE) FROM DUAL) as STORE_LOC
                        , TWN_DATE(A.APVTIME) AS APVTIME
                        , TWN_DATE(A.ACKTIME) AS ACKTIME
                        , b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT 
                        , a.APLYITEM_NOTE 
                        ,GET_PARAM('MI_WINVCTL','CTDMDCCODE',CTDMDCCODE(c.TOWH,a.MMCODE)) CTDMDCCODE
                        ,DRUGCHANGEMEMO(a.MMCODE) DRUGMEMO
                        , (select low_qty from MI_WINVCTL where wh_no = c.towh and mmcode = a.mmcode) as low_qty
                        , a.ISSPLIT, a.M_AGENNO
                        , (select nvl(APL_QTY_90(c.TOWH,a.MMCODE),0) from dual) as APLY_QTY_90
                        , (select nvl((select SAFE_QTY_90 
                                         from MI_WINVCTL 
                                        where WH_NO = c.TOWH and MMCODE = a.MMCODE),0) from dual) as SAFE_QTY_90
                        , (select nvl((select round(HIGH_QTY_90)
                                         from MI_WINVCTL 
                                        where WH_NO = c.TOWH and MMCODE = a.MMCODE),0) from dual) as HIGH_QTY_90                      
                        FROM ME_DOCD a 
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

        public List<UR_ID> GetUserInfo(string tuser)
        {
            var sql = @"SELECT TUSER, INID, UNA, UENA, EMAIL FROM UR_ID WHERE TUSER = :TUSER";
            return DBWork.Connection.Query<UR_ID>(sql, new { TUSER = tuser }, DBWork.Transaction).ToList();
        }

        // 取得使用部門
        //public IEnumerable<MI_WHMAST> GetSupplyInid(string inid)
        public IEnumerable<MI_WHMAST> GetSupplyInid(string userId)
        {
            //var sql = @"SELECT WH_NO, WH_NO || ' ' || WH_NAME AS WH_NAME, SUPPLY_INID FROM MI_WHMAST WHERE wh_kind = 0 and SUPPLY_INID IN
            //            (SELECT SUPPLY_INID FROM MI_WHMAST WHERE wh_kind = 0 and INID = :INID)";
            //return DBWork.Connection.Query<MI_WHMAST>(sql, new { INID = inid }, DBWork.Transaction);
            //var sql = @"SELECT A.WH_NO, A.WH_NO || ' ' || A.WH_NAME AS WH_NAME FROM MI_WHMAST A INNER JOIN MI_WHID B ON A.WH_NO=B.WH_NO WHERE B.WH_USERID = :WH_UESRID AND A.WH_GRADE>'1'";

            // 若有藥品一級庫權限，可選全部藥品庫房
            var sql = @"SELECT WH_NO, WH_NO||' '||WH_NAME(WH_NO) as WH_NAME
                    FROM MI_WHID
                    WHERE WH_USERID = :WH_UESRID
                    AND TASK_ID = '1'
                    UNION ALL
                    SELECT WH_NO, WH_NO||' '||WH_NAME(WH_NO) as WH_NAME
                    FROM MI_WHMAST
                    WHERE WH_KIND = '0'
                    AND WH_GRADE <> '1'
                    AND WH_GRADE <> '5'
                     AND AUTH_1ST(:WH_UESRID)='Y'";

            return DBWork.Connection.Query<MI_WHMAST>(sql, new { WH_UESRID = userId }, DBWork.Transaction);
        }


        // 取得核撥庫房, 只可選上級庫
        public IEnumerable<MI_WHMAST> GetFrwh(string inid, string wh_grade)
        {
            //var sql = @"SELECT WH_NO, WH_NO || ' ' || WH_NAME AS WH_NAME, SUPPLY_INID FROM MI_WHMAST WHERE wh_kind = 0 and WH_GRADE < :WH_GRADE and SUPPLY_INID IN
            //            (SELECT SUPPLY_INID FROM MI_WHMAST WHERE wh_kind = 0 and INID = :INID)";
            //var sql = @"SELECT WH_NO, WH_GRADE || ' ' || WH_NO || ' ' || WH_NAME AS WH_NAME, SUPPLY_INID FROM MI_WHMAST WHERE wh_kind = '0' and WH_GRADE < :WH_GRADE ORDER BY WH_GRADE";
            var sql = @"SELECT WH_NO, WH_NO || ' ' || WH_NAME AS WH_NAME, SUPPLY_INID FROM MI_WHMAST WHERE wh_kind = '0' and WH_GRADE < :WH_GRADE ORDER BY WH_GRADE";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { INID = inid, WH_GRADE = wh_grade }, DBWork.Transaction);
        }

        public IEnumerable<MI_WINVCTL> GetSuggestQty(string wh_no, string mmcode)
        {
            var sql = @"SELECT mi_winvctl.safe_day,   
                                 mi_winvctl.oper_day,   
                                 mi_winvctl.ship_day,   
                                 mi_winvctl.safe_qty,   
                                 mi_winvctl.oper_qty,   
                                 mi_winvctl.ship_qty,   
                                 mi_winvctl.high_qty,   
                                 mi_winvctl.low_qty,   
                                 mi_winvctl.davg_useqty,   
                                 mi_winvctl.min_ordqty,   
                                 mi_whinv.inv_qty,
                                 apl_qty_90(:WH_NO, :MMCODE) as suggest_qty
                            FROM mi_winvctl,   
                                 mi_whinv  
                           WHERE ( mi_winvctl.mmcode = mi_whinv.mmcode ) and  
                                 ( mi_winvctl.wh_no = mi_whinv.wh_no )
                                 and mi_winvctl.wh_no=:WH_NO and mi_winvctl.mmcode=:MMCODE";
            return DBWork.Connection.Query<MI_WINVCTL>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string GetGrade(string wh_no)
        {
            var sql = @"select WH_GRADE from MI_WHMAST where WH_NO = :WH_NO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.M_CONTPRICE, A.BASE_UNIT 
                            FROM MI_MAST A INNER JOIN MI_WINVCTL B ON A.MMCODE=B.MMCODE 
                            WHERE 1=1 AND A.E_ORDERDCFLAG = 'N' "; // 撈可存放庫房的院內碼

            if (query.WH_NO != "")
            {
                sql += " AND B.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", query.WH_NO));
            }

            /* 
             * 如果要加速UPPER查詢，可以針對UPPER建立INDEX
             * CREATE INDEX upper_index_name ON table(upper(name))
             * 或
             * 不使用UPPER，改用ALTER SESSION SET NLS_COMP=LINGUISTIC;
             * 將設定比對為忽略大小寫
             */
            if (query.MMCODE != "")
            {
                sql += " AND UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT 
                        FROM MI_MAST A INNER JOIN MI_WINVCTL B ON A.MMCODE=B.MMCODE 
                        WHERE A.E_ORDERDCFLAG  = 'N' 
                          AND B.CTDMDCCODE in ('0', '3') ";
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

        public string GetDocno()
        {
            //OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString); 
            //conn.Open();
            //OracleCommand cmd = new OracleCommand("GET_DOCNO", conn);
            //cmd.CommandType = CommandType.StoredProcedure;
            //OracleParameter output = new OracleParameter("OUTPUT1", OracleDbType.Varchar2, 12);    
            //output.Direction = ParameterDirection.Output;
            //cmd.Parameters.Add(output);
            //cmd.ExecuteNonQuery();
            //rtn = output.Value.ToString();
            //conn.Close();
            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12); // 務必要填上size,不然取值都會是null

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_DOCNO").Value;
        }

        public string GetMaxSeq(string docno)
        {
            var sql = @"SELECT MAX(SEQ) + 1 AS MAXSEQ FROM ME_DOCD WHERE DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string GetMaxSeqForDocexp(string docno)
        {
            var sql = @"SELECT MAX(SEQ) + 1 AS MAXSEQ FROM ME_DOCEXP WHERE DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
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
                    , APPID || ' ' || USER_NAME(APPID) AS APP_NAME
                    , (SELECT FLOWID || ' ' || FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID AND DOCTYPE='MR') FLOWID
                    , APPDEPT || ' ' || INID_NAME(APPDEPT) AS APPDEPT_NAME
                    , TOWH || ' ' || WH_NAME(TOWH) AS TOWH_NAME
                    , FRWH || ' ' || WH_NAME(FRWH) AS FRWH_NAME
                    , TOWH, FRWH  
                    FROM ME_DOCM A WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);
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

        public int MasterUpdate(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FRWH=:FRWH, TOWH=:TOWH, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int DetailCreate(ME_DOCD me_docd)
        {
            var sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, S_INV_QTY, INV_QTY, SAFE_QTY, OPER_QTY, PACK_QTY, PACK_UNIT, E_ORDERDCFLAG, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP,APLYITEM_NOTE, ISSPLIT, M_AGENNO, PACK_TIMES)  
                                    VALUES (:DOCNO, :SEQ, :MMCODE, :APPQTY, :S_INV_QTY, :INV_QTY, :SAFE_QTY, :OPER_QTY, :PACK_QTY, :PACK_UNIT, :E_ORDERDCFLAG, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :APLYITEM_NOTE, :ISSPLIT, :M_AGENNO, :PACK_TIMES)";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> DetailGet(ME_DOCD me_docd)
        {
            var sql = @"SELECT a.DOCNO, A.MMCODE, A.SEQ, A.APPQTY, A.APVQTY, A.ACKQTY, C.TOWH AS WH_NO
                        , APL_QTY_90(C.TOWH, a.mmcode) AS SUGGEST_QTY
                        , TWN_DATE(A.APVTIME) AS APVTIME
                        , TWN_DATE(A.ACKTIME) AS ACKTIME
                        , b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT 
                        , a.APLYITEM_NOTE 
                        FROM ME_DOCD a 
                        LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE
                        INNER JOIN ME_DOCM C ON A.DOCNO=C.DOCNO WHERE A.DOCNO=:DOCNO AND A.SEQ=:SEQ";
            return DBWork.Connection.Query<ME_DOCD>(sql, me_docd, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> LoadLowItem(string docno, string is_auto, string mmcode_prefix, string restrictcode123, string is_vaccine)
        {
            var p = new DynamicParameters();
            var sql = string.Empty;
            if (string.IsNullOrEmpty(is_auto) == false)
            {
                sql = @"select * from (
                            select a.docno,b.mmcode,nvl(apl_qty_90(a.towh,b.mmcode),0) apl_qty
                            from me_docm a, MI_WINVCTL b, mi_mast c
                            where a.docno = :DOCNO
                              AND B.CTDMDCCODE = '0' 
                            and a.towh = b.wh_no
                            and b.mmcode = c.mmcode
                            and c.e_orderdcflag = 'N'
                            and a.towh = b.wh_no
                            and b.is_auto = :IS_AUTO
                            and c.mmcode like :MMCODE
                            and nvl(c.e_vaccine, 'N') = 'N'
                            and c.e_restrictcode not in ('1','2','3')
                            )
                            where apl_qty > 0";
            }
            else if (string.IsNullOrEmpty(restrictcode123) == false)
            {
                sql = string.Format(@"
                    select * from (
                            select a.docno,b.mmcode,nvl(apl_qty_90(a.towh,b.mmcode),0) apl_qty
                              from me_docm a, MI_WINVCTL b, mi_mast c
                             where a.docno = :DOCNO
                               AND B.CTDMDCCODE = '0' 
                               and a.towh = b.wh_no
                               and b.mmcode = c.mmcode
                               and c.e_orderdcflag = 'N'
                               and nvl(c.e_vaccine, 'N')  = 'N'
                               and a.towh = b.wh_no
                               and c.e_restrictcode in ( '1','2','3' )
                            )
                     where apl_qty > 0
                ");
            }
            else if (string.IsNullOrEmpty(is_vaccine) == false)
            {
                sql = string.Format(@"
                    select * from (
                            select a.docno,b.mmcode,nvl(apl_qty_90(a.towh,b.mmcode),0) apl_qty
                              from me_docm a, MI_WINVCTL b, mi_mast c
                             where a.docno = :DOCNO
                               AND B.CTDMDCCODE = '0' 
                               and a.towh = b.wh_no
                               and b.mmcode = c.mmcode
                               and c.e_orderdcflag = 'N'
                               and a.towh = b.wh_no
                               and nvl(c.e_vaccine, 'N')  = 'Y'
                               and c.e_restrictcode not in ('1','2','3')
                            )
                     where apl_qty > 0
                ");
            }

            p.Add(":DOCNO", docno);
            p.Add(":IS_AUTO", is_auto);
            p.Add(":MMCODE", string.Format("{0}%", mmcode_prefix));
            return DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);
        }

        public int DetailUpdate(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET MMCODE=:MMCODE, APPQTY=:APPQTY, S_INV_QTY=:S_INV_QTY, INV_QTY=:INV_QTY, SAFE_QTY=:SAFE_QTY, OPER_QTY=:OPER_QTY, PACK_QTY=:PACK_QTY, PACK_UNIT=:PACK_UNIT
                        , E_ORDERDCFLAG=:E_ORDERDCFLAG, UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP, APLYITEM_NOTE=:APLYITEM_NOTE 
                        WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailDelete(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public int UpdateStatus(string docno, string flowId, string userId, string ip)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = :FLOWID, 
                                apptime = sysdate,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :userId, UPDATE_IP = :ip
                                WHERE DOCNO = :docno";
            return DBWork.Connection.Execute(sql, new { docno, flowId, userId, ip }, DBWork.Transaction);
        }

        // 檢查申請單號是否存在
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        // 核撥庫房允許存放的院內碼
        public bool CheckWhmmExists(string wh_no, string mmcode)
        {
            string sql = @"SELECT 1 FROM MI_WINVCTL WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        // 檢查申請單是否有院內碼項次,要送申請時,必須一定要有院內碼項次
        public bool CheckMeDocdExists(string docno)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
        }

        public bool CheckMeDocexpExists(string docno)
        {
            string sql = @"SELECT 1 FROM ME_DOCEXP WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
        }

        // 檢查申請單院內碼項次的申請數量不得<=0,有的話則不能提出申請
        public bool CheckMeDocdAppqty(string docno)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO and APPQTY<=0";
            return (DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
        }

        // 檢查申請單是否有院內碼項次
        public bool CheckMeDocdExists_1(ME_DOCD me_docd)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE AND SEQ!=:SEQ";
            return !(DBWork.Connection.ExecuteScalar(sql, me_docd, DBWork.Transaction) == null);
        }

        public string GetFrwh(string docno)
        {
            string sql = @"SELECT FRWH FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string Get_S_INV_QTY(string towh, string mmcode)
        {
            string sql = @"SELECT SUPERIOR_STOCK_QTY(:TOWH, :MMCODE) FROM DUAL";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string Get_SUGGEST_QTY(string towh, string mmcode)
        {
            string sql = @"SELECT NVL(APL_QTY_90(:TOWH, :MMCODE), 0) FROM DUAL";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string Get_INV_QTY(string towh, string mmcode)
        {
            string sql = @"SELECT INV_QTY FROM MI_WHINV WHERE WH_NO=:TOWH AND MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string Get_SAFE_OPER_QTY(string towh, string mmcode)
        {
            //string sql = @"select SAFE_QTY || ',' || round(high_qty) ||',' || SAFE_QTY_90 ||',' || HIGH_QTY_90 ||',' || APL_QTY_90(a.WH_NO,a.MMCODE) as APLY_QTY_90 
            //                 from MI_WINVCTL a
            //                where WH_NO=:TOWH AND MMCODE=:MMCODE";
            //string sql = @"select SAFE_QTY || ',' || round(high_qty) 
            //                 from MI_WINVCTL 
            //                where WH_NO=:TOWH AND MMCODE=:MMCODE";
            string sql = @"select SAFE_QTY_90  || ',' ||
                                  round(HIGH_QTY_90) 
                             from MI_WINVCTL a
                            where wh_no = :TOWH
                              and mmcode = :MMCODE
            ";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string Get_PACK(string towh, string mmcode)
        {
            string sql = @"SELECT PACK_QTY || ',' || PACK_UNIT FROM ME_UIMAST WHERE WH_NO=:TOWH AND MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string Get_ORDERDCFLAG(string mmcode)
        {
            string sql = @"SELECT E_ORDERDCFLAG FROM MI_MAST WHERE MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        }

        public string GetTowh(string docno)
        {
            string sql = @"SELECT TOWH FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string GetStoreLoc(string frwh, string mmcode)
        {
            //string sql = @"SELECT STORE_LOC('PH1S', :MMCODE) FROM DUAL";
            string sql = "SELECT STORE_LOC FROM MI_WLOCINV WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { WH_NO = frwh, MMCODE = mmcode }, DBWork.Transaction);
        }

        public int Copy(string newDocno, string oldDocno, string user, string ip)
        {
            // copy master
            var sql = @"INSERT INTO ME_DOCM (DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT, APPTIME, USEID, USEDEPT, FRWH, TOWH, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                                    SELECT :NEWDOCNO, DOCTYPE, '0101', :CREATE_USER, APPDEPT, SYSDATE, :CREATE_USER, USEDEPT, FRWH, TOWH, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP FROM ME_DOCM where DOCNO=:OLDDOCNO";
            DBWork.Connection.Execute(sql, new { NEWDOCNO = newDocno, OLDDOCNO = oldDocno, CREATE_USER = user, UPDATE_USER = user, UPDATE_IP = ip }, DBWork.Transaction);

            // copy detail
            sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                               SELECT :NEWDOCNO, SEQ, MMCODE, '0', SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP FROM ME_DOCD where DOCNO=:OLDDOCNO";

            return DBWork.Connection.Execute(sql, new { NEWDOCNO = newDocno, OLDDOCNO = oldDocno, CREATE_USER = user, UPDATE_USER = user, UPDATE_IP = ip }, DBWork.Transaction);
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

        public class ME_DOCD_QUERY_PARAMS
        {
            public string DOCNO;
            public string SEQ;
        }
        public IEnumerable<COMBO_MODEL> GetAppdeptCombo()
        {
            string sql = @"SELECT DISTINCT A.APPDEPT as VALUE, B.INID_NAME as TEXT ,
                        A.APPDEPT || ' ' || B.INID_NAME as COMBITEM 
                        FROM ME_DOCM A ,UR_INID B
                        WHERE A.APPDEPT=B.INID AND A.DOCTYPE IN ('MR')  
                        ORDER BY A.APPDEPT ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetUsedeptCombo()
        {
            string sql = @"SELECT DISTINCT A.USEDEPT as VALUE, B.INID_NAME as TEXT ,
                        A.USEDEPT || ' ' || B.INID_NAME as COMBITEM 
                        FROM ME_DOCM A ,UR_INID B
                        WHERE A.USEDEPT=B.INID AND A.DOCTYPE IN ('MR')  
                        ORDER BY A.USEDEPT ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetFrwhCombo()
        {
            string sql = @"SELECT DISTINCT A.FRWH as VALUE, B.WH_NAME as TEXT ,
                        A.FRWH || ' ' || B.WH_NAME as COMBITEM 
                        FROM ME_DOCM A ,MI_WHMAST B
                        WHERE A.FRWH=B.WH_NO AND A.DOCTYPE IN ('MR')  
                        ORDER BY A.FRWH ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetTowhCombo()
        {
            string sql = @"SELECT DISTINCT A.WH_NO as VALUE, A.WH_NAME as TEXT ,
                        A.WH_NO || ' ' || A.WH_NAME as COMBITEM, A.WH_GRADE EXTRA1
                        FROM MI_WHMAST A
                        WHERE A.WH_KIND = '0' AND A.WH_GRADE > '1'
                          and a.cancel_id = 'N'
                        ORDER BY A.WH_GRADE, A.WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetDocnoCombo(string user)
        {
            string sql = @"SELECT DISTINCT DOCNO as VALUE, DOCNO as TEXT,
                        DOCNO || '(' || TOWH || ')' as COMBITEM,APPTIME EXTRA1 
                        FROM ME_DOCM 
                        WHERE 1=1 AND DOCTYPE = 'MR' 
                        AND FLOWID = '0102' AND APPID = :TUSER
                        ORDER BY  EXTRA1 DESC ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = user });
        }

        public DataTable GetExcel(string docno)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                          A.DOCNO 單據號碼,
                          MMCODE 院內碼,
                          MMCODE_NAME(MMCODE) 藥品名稱,
                          SAFE_QTY 安全量,
                          OPER_QTY 基準量,
                          INV_QTY 庫存量,
                          MIN_ORDQTY(B.TOWH,MMCODE) 最低庫存量,
                          B.FRWH||'_'||WH_NAME(B.FRWH) 上級庫,
                          S_INV_QTY 上級庫庫存量,
                          APPQTY 申請量,
                          PACK_QTY 申請包裝量,
                          PACK_TIMES as 包裝申領量倍數,
                          APLYITEM_NOTE 備註,
                          UPRICE(MMCODE) 單價 
                        FROM ME_DOCD A, ME_DOCM B
                        WHERE A.DOCNO = B.DOCNO AND A.DOCNO = :DOCNO
                        
                        ";

            p.Add(":DOCNO", docno);


            sql += " ORDER BY A.MMCODE";



            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<COMBO_MODEL> GetPackUnitCombo(string mmcode)
        {
            string sql = @"SELECT DISTINCT PACK_UNIT as VALUE, PACK_UNIT as TEXT ,
                        PACK_UNIT as COMBITEM, PACK_QTY as EXTRA1 
                        FROM V_UIMAST
                        WHERE MMCODE=:MMCODE 
                        ORDER BY PACK_UNIT";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { MMCODE = mmcode });
        }

        #region 2020-06-23 送核撥更新me_docd.apl_contime
        public int UpdateDocdAplcontime(string docno, string update_user, string update_ip)
        {
            string sql = @"update ME_DOCD
                              set apl_contime = sysdate,
                                  APVQTY=APPQTY,
                                  update_time = sysdate,
                                  update_ip = :update_ip,
                                  update_user = :update_user
                            where docno = :docno";
            return DBWork.Connection.Execute(sql, new { docno = docno, update_user = update_user, update_ip = update_ip }, DBWork.Transaction);
        }
        #endregion

        public string GetFlowid(string docno) {
            string sql = @"select flowid from ME_DOCM where docno = :docno";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { docno = docno }, DBWork.Transaction);
        }

        #region 2020-12-28 載入全庫品項
        public IEnumerable<ME_DOCD> LoadAllItem(string docno)
        {
            var p = new DynamicParameters();
            var sql = string.Format(@"
                   select * 
                     from (select a.docno,b.mmcode,nvl(apl_qty_90(a.towh,b.mmcode),0) apl_qty
                              from me_docm a, MI_WINVCTL b, mi_mast c
                             where a.docno = :DOCNO
                               AND B.CTDMDCCODE = '0' 
                               and a.towh = b.wh_no
                               and b.mmcode = c.mmcode
                               and c.e_orderdcflag = 'N'
                               and a.towh = b.wh_no
                               and substr(c.mmcode,1,3) = '005'
                               and (c.m_agenno <> '000'
                                        or exists (select 1 from MI_WHINV 
                                            where wh_no = a.frwh
                                              and mmcode = c.mmcode
                                              and inv_qty > 0) 
                                   )  
                          ) a
                    order by mmcode");

            p.Add(":DOCNO", docno);            
            
            return DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);
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

        public bool CheckIsTowhCancelByDocno(string docno)
        {
            string sql = @"
               select 1 from ME_DOCM  a
                where a.docno = :docno
                  and exists (select 1 from MI_WHMAST 
                               where wh_no = a.towh
                                 and cancel_id = 'N')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) == null;
        }
        #endregion

        #region 2021-10-04 
        public string GetISSPLIT(string docno, string mmcode) {
            string sql = @"
                select issplit from MI_WINVCTL 
                 where WH_NO = (select towh from ME_DOCM where docno = :docno) 
                   and MMCODE = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { docno, mmcode }, DBWork.Transaction);
        }
        public string GetM_AGENNO(string mmcode) {
            string sql = @"
                select m_agenno from MI_MAST where mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetSplitData(string docno) {
            string sql = @"
                select distinct ISSPLIT, null as M_AGENNO from ME_DOCD
                 where ISSPLIT='N' and DOCNO = :DOCNO  
                 union
                select distinct ISSPLIT,M_AGENNO from ME_DOCD
                 where ISSPLIT='Y' and DOCNO = :DOCNO  
                 order by ISSPLIT,M_AGENNO
            ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno }, DBWork.Transaction);
        }

        public int CreateNewDocm(string docno, string new_docno, string agen_no,string flowId, string userId, string ip) {
            string sql = @"
                insert into ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT,
                        APPTIME, USEID, USEDEPT, FRWH, TOWH,
                        LIST_ID, APPLY_KIND, MAT_CLASS, JCN, APPLY_NOTE, POST_TIME, SET_YM, 
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP,
                        AGEN_NO, STKTRANSKIND, RETURN_NOTE
                        )
                select :new_docno as docno, doctype, :flowId as flowId, appId, appDept,
                       appTime, useId, useDept, frwh, towh,
                       list_id, apply_kind, mat_class, jcn, apply_note, post_time, set_ym,
                       sysdate as create_time, create_user, sysdate as update_time, :userId as update_user, :ip as update_ip,
                       :agen_no as agaen_no, STKTRANSKIND, RETURN_NOTE
                  from ME_DOCM
                 where docno = :docno
            ";
            return DBWork.Connection.Execute(sql, new { docno, new_docno, agen_no, flowId, userId, ip }, DBWork.Transaction);
        }

        public int CreateNewDocd(string docno, string new_docno,string issplit, string m_agenno, string userId, string ip)
        {
            string sql = string.Format(@"
                insert into ME_DOCD (
                    DOCNO, SEQ, MMCODE, APPQTY, APVQTY, APVTIME, APVID,
                    ACKQTY, ACKID, ACKTIME, STAT, RDOCNO, RSEQ,
                    EXPT_DISTQTY, DIS_USER, DIS_TIME, BW_MQTY, BW_SQTY,
                    PICK_QTY, PICK_USER, PICK_TIME, ONWAY_QTY, APL_CONTIME, AMT,
                    UP, RV_MQTY, GTAPL_RESON, APLYITEM_NOTE,
                    CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP,
                    EXP_STATUS, MEDNO, NRCODE, BEDNO, FRWH_D, ORDERDATE,
                    CHINNAME, VISITSEQ, E_DRUGCLASSIFY, S_INV_QTY, INV_QTY,
                    SAFE_QTY, OPER_QTY, PACK_QTY, PACK_UNIT,
                    E_ORDERDCFLAG, CONFIRMSWITCH, TRANSKIND, POSTID,
                    AVG_PRICE, ACKQTYT, PMMCODE, PQTY, TRNAB_RESON,
                    TRNAB_QTY,PACK_TIMES,ISSPLIT,M_AGENNO
                )
                select :new_docno as docno, SEQ, MMCODE, APPQTY, APVQTY, APVTIME, APVID,
                       ACKQTY, ACKID, ACKTIME, STAT, RDOCNO, RSEQ,
                       EXPT_DISTQTY, DIS_USER, DIS_TIME, BW_MQTY, BW_SQTY,
                       PICK_QTY, PICK_USER, PICK_TIME, ONWAY_QTY, APL_CONTIME, AMT,
                       UP, RV_MQTY, GTAPL_RESON, APLYITEM_NOTE,
                       sysdate as CREATE_TIME, create_user, sysdate as UPDATE_TIME, :userId as update_user, :ip as update_ip,
                       EXP_STATUS, MEDNO, NRCODE, BEDNO, FRWH_D, ORDERDATE,
                       CHINNAME, VISITSEQ, E_DRUGCLASSIFY, S_INV_QTY, INV_QTY,
                       SAFE_QTY, OPER_QTY, PACK_QTY, PACK_UNIT,
                       E_ORDERDCFLAG, CONFIRMSWITCH, TRANSKIND, POSTID,
                       AVG_PRICE, ACKQTYT, PMMCODE, PQTY, TRNAB_RESON,
                       TRNAB_QTY,PACK_TIMES,ISSPLIT,M_AGENNO
                  from ME_DOCD
                 where docno = :docno
                   and ISSPLIT = :issplit
                   {0}
            ", issplit == "Y" ? " and m_agenno = :m_agenno" : string.Empty);
            return DBWork.Connection.Execute(sql, new { docno, new_docno, issplit, m_agenno, userId, ip }, DBWork.Transaction);
        }

        public int DeleteAllDocd(string docno) {
            string sql = @"
                delete from ME_DOCD where docno = :docno
            ";
            return DBWork.Connection.Execute(sql, new { docno }, DBWork.Transaction);
        }
        #endregion

        public string GetPackTimes(string wh_no, string mmcode) {
            string sql = @"
                select pack_times from ME_UIMAST
                 where wh_no = :wh_no
                   and mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { wh_no, mmcode }, DBWork.Transaction);
        }
    }

}