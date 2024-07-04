using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;

namespace WebApp.Repository.AA
{
    public class AA0048Repository : JCLib.Mvc.BaseRepository
    {
        public AA0048Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0048> GetAll(string wh_no, string mmcode, string MatClass,string[] str_ctdm, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.*,
                               a.wh_no as wh_no_display,
                               a.mmcode as mmcode_display,
                               a.safe_day as safe_day_display,
                               a.oper_day as oper_day_display,
                               a.ship_day as ship_day_display,
                               a.high_qty as high_qty_display,
                               a.low_qty as low_qty_display,
                               a.min_ordqty as min_ordqty_display,
                               a.CTDMDCCODE_N as ctdmdccode_display,
                               a.is_auto as is_auto_display,
                               a.isSplit as issplit_display
                          from (
                               SELECT A.WH_NO, A.MMCODE, A.SAFE_DAY, A.OPER_DAY, A.SHIP_DAY,
                                      A.DAVG_USEQTY, 
                                      A.HIGH_QTY, A.LOW_QTY, A.MIN_ORDQTY, A.SUPPLY_WHNO,
                                      A.SAFE_QTY,
                                      A.OPER_QTY,
                                      A.SHIP_QTY,
                                      a.IS_AUTO,
                                      (SELECT INV_QTY FROM MI_WHINV WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) INV_QTY , 
                                      STORE_LOC(A.WH_NO,A.MMCODE) STORE_LOC,STOCKUNIT(A.MMCODE) STOCKUNIT, A.CTDMDCCODE,
                                      (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WINVCTL' AND DATA_NAME='CTDMDCCODE' AND DATA_VALUE=A.CTDMDCCODE) CTDMDCCODE_N,
                                      b.MMNAME_E || ' ' || b.MMNAME_C AS MMNAME,
                                      b.E_RESTRICTCODE,
                                      (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_MAST' AND DATA_NAME='E_RESTRICTCODE' AND DATA_VALUE=b.E_RESTRICTCODE) E_RESTRICTCODE_N,
                                      c.DANGERDRUGFLAG,
                                      (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='HIS_BASORDM' AND DATA_NAME='DANGERDRUGFLAG' AND DATA_VALUE=c.DANGERDRUGFLAG) DANGERDRUGFLAG_N, 
                                      (SELECT TWN_DATE(MAX(EXP_DATE)) FROM MI_WEXPINV WHERE WH_NO = a.WH_NO AND MMCODE = A.MMCODE) maxEXP_DATE,
                                      (SELECT data_value || ' ' || data_desc FROM  PARAM_D a
                                           WHERE grp_code = 'MI_WINVCTL' 
                                             and data_name = 'USEADJ_CLASS'
                                             and data_value = (select USEADJ_CLASS from MI_WINVCTL 
                                                                where wh_no = a.wh_no and mmcode = a.mmcode)
                                      ) as  USEADJ_CLASS_DISPLAY,
                                      a.USEADJ_CLASS,
                                      a.NOWCONSUMEFLAG ,
                                      (select wh_kind from MI_WHMAST where wh_no = a.wh_no) as wh_kind,
                                      (select wh_grade from MI_WHMAST where wh_no = a.wh_no) as wh_grade,
                                      a.isSplit,TWN_DATE(a.FSTACKDATE) as FSTACKDATE ,a.DAVG_USEQTY_90,
                                      a.SAFE_QTY_90,a.OPER_QTY_90,a.SHIP_QTY_90,a.HIGH_QTY_90
                                 FROM MI_WINVCTL a 
                                INNER JOIN MI_MAST b ON b.MMCODE = a.MMCODE 
                                 LEFT OUTER JOIN HIS_BASORDM c ON c.ORDERCODE = a.MMCODE 
                                WHERE 1=1
                               ";

            if (wh_no != "" && wh_no != null)
            {
                sql += " AND a.WH_NO in :p0 ";
                string[] tmpWh_no = wh_no.Split(',');
                p.Add(":p0", tmpWh_no);
            }
            if (mmcode != "")
            {
                sql += " AND a.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            if (MatClass != "" && MatClass != null)
            {
                sql += " AND EXISTS (SELECT 1 FROM MI_MAST WHERE MMCODE = A.MMCODE AND MAT_CLASS = :MatClass) ";
                p.Add(":MatClass", string.Format("{0}", MatClass));
            }

            //if (ctdm != "")
            //{
            //    sql += " AND a.CTDMDCCODE = :p3 ";
            //    p.Add(":p3", string.Format("{0}", ctdm));
            //}
            if (str_ctdm.Length > 0)
            {
                string sql_ctdm = "";
                sql += @"AND (";
                foreach (string tmp_ctdm in str_ctdm)
                {
                    if (string.IsNullOrEmpty(sql_ctdm))
                    {
                        sql_ctdm = @"A.CTDMDCCODE = '" + tmp_ctdm + "'";
                    }
                    else
                    {
                        sql_ctdm += @" OR A.CTDMDCCODE = '" + tmp_ctdm + "'";
                    }
                }
                sql += sql_ctdm + ") ";
            }
            sql += " ) a";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0048>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0048> Get(string wh_no, string mmcode)
        {
            var sql = @"SELECT A.WH_NO, A.MMCODE, A.SAFE_DAY, A.OPER_DAY, A.SHIP_DAY,
                     A.DAVG_USEQTY, 
                    A.HIGH_QTY, A.LOW_QTY, A.MIN_ORDQTY, A.SUPPLY_WHNO,
                    A.SAFE_QTY,
                    A.OPER_QTY,
                    A.SHIP_QTY,
                     a.IS_AUTO, 
                     (SELECT INV_QTY FROM MI_WHINV WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) INV_QTY , 
                     STORE_LOC(A.WH_NO,A.MMCODE) STORE_LOC,STOCKUNIT(A.MMCODE) STOCKUNIT, A.CTDMDCCODE,
                    (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WINVCTL' AND DATA_NAME='CTDMDCCODE' AND DATA_VALUE=A.CTDMDCCODE) CTDMDCCODE_N,
                    b.MMNAME_E || ' ' || b.MMNAME_C AS MMNAME, 
                    b.E_RESTRICTCODE, 
                    (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_MAST' AND DATA_NAME='E_RESTRICTCODE' AND DATA_VALUE=b.E_RESTRICTCODE) E_RESTRICTCODE_N, 
                    c.DANGERDRUGFLAG, 
                    (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='HIS_BASORDM' AND DATA_NAME='DANGERDRUGFLAG' AND DATA_VALUE=c.DANGERDRUGFLAG) DANGERDRUGFLAG_N, 
                    (SELECT TWN_DATE(MAX(EXP_DATE)) FROM MI_WEXPINV WHERE WH_NO = a.WH_NO AND MMCODE = a.MMCODE) maxEXP_DATE ,
                    (SELECT data_value || ' ' || data_desc FROM  PARAM_D a
                         WHERE grp_code = 'MI_WINVCTL' 
                           and data_name = 'USEADJ_CLASS'
                           and data_value = (select USEADJ_CLASS from MI_WINVCTL 
                                              where wh_no = a.wh_no and mmcode = a.mmcode)
                    ) as  USEADJ_CLASS_NAME,
                    a.USEADJ_CLASS,
                    a.isSplit
                   FROM MI_WINVCTL a 
                    INNER JOIN MI_MAST b ON b.MMCODE = a.MMCODE 
                    LEFT OUTER JOIN HIS_BASORDM c ON c.ORDERCODE = a.MMCODE 
                    WHERE 1=1
                    AND A.WH_NO = :WH_NO and A.MMCODE = :MMCODE";
            return DBWork.Connection.Query<AA0048>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public int Create(AA0048 mi_winvctl)
        {
            var sql = @"INSERT INTO MI_WINVCTL (WH_NO, MMCODE, SAFE_DAY, OPER_DAY, SHIP_DAY, HIGH_QTY, LOW_QTY, MIN_ORDQTY, SUPPLY_WHNO,IS_AUTO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, USEADJ_CLASS, ISSPLIT)  
                                VALUES (:WH_NO,:MMCODE,:SAFE_DAY,:OPER_DAY,:SHIP_DAY,:HIGH_QTY,:LOW_QTY,:MIN_ORDQTY,:SUPPLY_WHNO,:IS_AUTO, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :USEADJ_CLASS, :ISSPLIT)";
            return DBWork.Connection.Execute(sql, mi_winvctl, DBWork.Transaction);
        }

        public int Update(AA0048 mi_winvctl)
        {
            var sql = @"UPDATE MI_WINVCTL 
                           SET SAFE_DAY = :SAFE_DAY, OPER_DAY = :OPER_DAY, 
                               SHIP_DAY = :SHIP_DAY, HIGH_QTY = :HIGH_QTY, LOW_QTY = :LOW_QTY, 
                               MIN_ORDQTY = :MIN_ORDQTY, IS_AUTO = :IS_AUTO, 
                               UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                               USEADJ_CLASS = :USEADJ_CLASS, ISSPLIT = :ISSPLIT
                                WHERE WH_NO = :WH_NO and MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, mi_winvctl, DBWork.Transaction);
        }

        public int Delete(string wh_no, string mmcode)
        {
            // 刪除資料
            var sql = @"DELETE MI_WINVCTL WHERE WH_NO = :WH_NO and MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public IEnumerable<MI_WHMAST> GetWH_NOComboOne(string userId)      //AA0048
        {
            string sql = @"SELECT a.WH_NO,b.WH_NAME FROM MI_WHID a JOIN MI_WHMAST b ON (a.WH_NO = b.WH_NO) WHERE a.WH_USERID= :WH_USERID";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }
        public IEnumerable<MI_WHMAST> GetWH_NOComboNotOne(string userId) //AB0036
        {
            string sql = @"SELECT DISTINCT WH_NO, WH_NAME from MI_WHMAST WHERE INID=USER_INID(:INID) AND WH_GRADE<>'1' AND WH_KIND IN ('0','1')";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { INID = userId }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCODEComboWhMM(string wh_no)
        {
            string sql = @"SELECT DISTINCT b.MMCODE , b.MMNAME_C, b.MMNAME_E " +
                "from MI_WINVCTL a JOIN MI_MAST b ON (a.MMCODE = b.MMCODE) WHERE a.WH_NO = :WH_NO";

            return DBWork.Connection.Query<MI_MAST>(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMMCODEComboMast()
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT A.MMCODE , A.MMNAME_C, A.MMNAME_E FROM MI_MAST A, MI_MATCLASS B 
                WHERE A.MAT_CLASS=B.MAT_CLASS AND ";

            string[] tmp = "2,3".Split(',');
            sql += "B.MAT_CLSID IN :mat_class ";
            p.Add(":mat_class", tmp);

            return DBWork.Connection.Query<MI_MAST>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMMCODEComboMastGeneral(string task_id)
        {
            string sql = @"SELECT A.MMCODE , A.MMNAME_C, A.MMNAME_E FROM MI_MAST A, MI_MATCLASS B 
                           WHERE A.MAT_CLASS=B.MAT_CLASS AND B.MAT_CLSID= :MAT_CLSID";

            return DBWork.Connection.Query<MI_MAST>(sql, new { MAT_CLSID = task_id }, DBWork.Transaction);
        }
        public IEnumerable<ComboModel> GetSUPPLY_WHNOCombo()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, " +
                "WH_NO || ' ' || WH_NAME as COMBITEM  from MI_WHMAST";

            return DBWork.Connection.Query<ComboModel>(sql, DBWork.Transaction);
        }
        public bool CheckExists(string WH_NO, string MMCODE)
        {
            string sql = @"SELECT 1 FROM MI_WINVCTL WHERE WH_NO=:WH_NO and MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, MMCODE = MMCODE }, DBWork.Transaction) == null);
        }

        public string CheckWhKIND(string wh_no)
        {
            string sql = @"SELECT DISTINCT WH_KIND from MI_WHMAST WHERE WH_NO=:WH_NO";

            return DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no }, DBWork.Transaction).ToString();
        }
        public string CheckMiWhid(string userId) //確定是否為什麼屬性，如果是2則就要3~8
        {
            string sql = @"select WHM1_TASK(:USERID) as TASK_ID FROM DUAL";
            return DBWork.Connection.ExecuteScalar(sql, new { USERID = userId }, DBWork.Transaction).ToString();
        }

        public string GetWH_KIND(string wh_no)
        {
            var p = new DynamicParameters();


            var sql = @"SELECT listagg(WH_KIND, ',') within group (order by WH_KIND) as WH_KIND 
                        FROM (select distinct WH_KIND from MI_WHMAST WHERE WH_NO in :wh_no)";

            string[] tmpWh_no = null;
            if (wh_no != null)
                tmpWh_no = wh_no.Split(',');

            p.Add(":wh_no", tmpWh_no);

            if (DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction) == null)
            {
                return "";
            }
            else
            {
                return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
            }
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo_1()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS WHERE MAT_CLSID = '1'";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo_A()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS WHERE MAT_CLSID in ('1', '2', '3')";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo_else()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS WHERE MAT_CLSID  IN ('2','3') ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        
        public IEnumerable<COMBO_MODEL> GetCtdmdCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_WINVCTL' AND DATA_NAME='CTDMDCCODE' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public string SelectMI_WHMAST(string wh_no)
        {
            var sql = @"Select count(*) as count FROM MI_WHMAST 
                         WHERE WH_NO = :WH_NO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }
        public string SelectMI_WHMAST_check_kind(string wh_no, bool isMat)
        {
            string wh_kind = isMat ? "1" : "0";
            var sql = @"Select count(*) as count FROM MI_WHMAST 
                         WHERE WH_NO = :WH_NO
                           and wh_kind = :wh_kind";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, wh_kind = wh_kind }, DBWork.Transaction);
        }
        public string SelectMI_WHMAST_check_grade(string wh_no, bool isMat) {
            if (isMat == true) {
                return "1";
            }
            string sql = @"select count(*) as count from MI_WHMAST
                            where wh_no = :wh_no
                              and wh_kind = '0'
                              and wh_grade <= 2";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { wh_no = wh_no }, DBWork.Transaction);
        }
        public string SelectMI_MAST(string mmcode)
        {
            var sql = @"Select count(*) as count FROM MI_MAST 
                         WHERE MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectMI_WINVCTL(string wh_no, string mmcode)
        {
            var sql = @"Select count(*) as count FROM MI_WINVCTL 
                         WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.ExecuteScalar(sql, new { WH_NO =wh_no, MMCODE = mmcode }, DBWork.Transaction).ToString();
        }

        public string SelectDANGERDRUGFLAG_N(string mmcode)
        {
            var sql = @"SELECT NVL(DATA_DESC, ' ') FROM PARAM_D a, HIS_BASORDM b WHERE 
GRP_CODE='HIS_BASORDM' AND DATA_NAME='DANGERDRUGFLAG' AND DATA_VALUE=b.DANGERDRUGFLAG AND b.ORDERCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectE_RESTRICTCODE_N(string mmcode)
        {
            var sql = @"SELECT DATA_DESC FROM PARAM_D a,MI_MAST b WHERE GRP_CODE='MI_MAST' AND DATA_NAME='E_RESTRICTCODE' 
AND DATA_VALUE=b.E_RESTRICTCODE AND b.MMCODE =  :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectSUPPLY_WHNO(string wh_no, string mmcode) {
            string sql = @"select supply_whno from MI_WINVCTL where wh_no = :wh_no and mmcode = :mmcode";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode = mmcode, wh_no = wh_no }, DBWork.Transaction);
        }
        public string SelectCTDMDCCODE(string wh_no, string mmcode)
        {
            string sql = @"select CTDMDCCODE from MI_WINVCTL where wh_no = :wh_no and mmcode = :mmcode";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode = mmcode, wh_no = wh_no }, DBWork.Transaction);
        }
        public string SelectCTDMDCCODE_N(string code)
        {
            string sql = @"select data_value || ' '|| data_desc from PARAM_D where grp_code = 'MI_WINVCTL' and data_name = 'CTDMDCCODE' and data_value = :code";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { code = code}, DBWork.Transaction);
        }


        public string SelectMMNAME(string mmcode)
        {
            var sql = @"SELECT MMNAME_E || ' ' || MMNAME_C AS MMNAME FROM MI_MAST WHERE MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        }

        public int UpdateMI_WINVCTL(bool isMat, string wh_no, string mmcode, string safe_day, string oper_day, string ship_day, string high_qty, string low_qty, string min_ordqty, string is_auto, string useadj_class, string issplit, string update_user, string update_ip)
        {
            string default_sql = "update MI_WINVCTL set ";
            string sql = "update MI_WINVCTL set ";
            if (isMat) {
                if (safe_day != string.Empty)
                {
                    if (sql != default_sql)
                    {
                        sql += ", ";
                    }
                    sql += "safe_day = :SAFE_DAY";
                }
                if (oper_day != string.Empty)
                {
                    if (sql != default_sql)
                    {
                        sql += ", ";
                    }
                    sql += "OPER_DAY = :OPER_DAY";
                }
                if (ship_day != string.Empty)
                {
                    if (sql != default_sql)
                    {
                        sql += ", ";
                    }
                    sql += "SHIP_DAY = :SHIP_DAY";
                }
                if (high_qty != string.Empty)
                {
                    if (sql != default_sql)
                    {
                        sql += ", ";
                    }
                    sql += "HIGH_QTY = :HIGH_QTY";
                }
                if (low_qty != string.Empty)
                {
                    if (sql != default_sql)
                    {
                        sql += ", ";
                    }
                    sql += "LOW_QTY = :LOW_QTY";
                }
                if (min_ordqty != string.Empty)
                {
                    if (sql != default_sql)
                    {
                        sql += ", ";
                    }
                    sql += "MIN_ORDQTY = :MIN_ORDQTY";
                }
            }
            if (is_auto != string.Empty)
            {
                if (sql != default_sql)
                {
                    sql += ", ";
                }
                sql += "IS_AUTO = :IS_AUTO";
            }
            if (isMat == false) {
                if (useadj_class != string.Empty) {
                    if (sql != default_sql)
                    {
                        sql += ", ";
                    }
                    sql += "useadj_class = :USEADJ_CLASS";
                }
            }
            if (is_auto != string.Empty)
            {
                if (sql != default_sql)
                {
                    sql += ", ";
                }
                sql += "ISSPLIT = :ISSPLIT";
            }
            if (sql != default_sql)
            {
                sql += ", ";
            }
            sql += @"       UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                     where wh_no = :WH_NO and mmcode = :MMCODE";
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,MMCODE = mmcode,
                SAFE_DAY = safe_day,OPER_DAY = oper_day,SHIP_DAY = ship_day,HIGH_QTY = high_qty,LOW_QTY = low_qty,MIN_ORDQTY = min_ordqty,
                IS_AUTO = is_auto,
                USEADJ_CLASS = useadj_class,
                UPDATE_USER = update_user,
                UPDATE_IP = update_ip,
                ISSPLIT = issplit
            }, DBWork.Transaction);

        }

        public int InsertMI_WINVCTL(bool isMat, string wh_no, string mmcode, string safe_day, string oper_day, string ship_day, string high_qty, string low_qty, string min_ordqty, string is_auto, string ctdmdccode, string user, string update_ip, string useadj_class, string issplit)
        {
            var sql = string.Format(@"INSERT INTO MI_WINVCTL 
                                (WH_NO, MMCODE, {0} IS_AUTO, CTDMDCCODE, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, USEADJ_CLASS, ISSPLIT)  
                        VALUES (:WH_NO,:MMCODE, {1} :IS_AUTO,:CTDMDCCODE, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :USEADJ_CLASS, :ISSPLIT)",
                        isMat ? "SAFE_DAY, OPER_DAY, SHIP_DAY, HIGH_QTY, LOW_QTY, MIN_ORDQTY," : string.Empty,
                        isMat ? ":SAFE_DAY,:OPER_DAY,:SHIP_DAY,:HIGH_QTY,:LOW_QTY,:MIN_ORDQTY," : string.Empty);
            return DBWork.Connection.Execute(sql, new
            {
                WH_NO = wh_no,
                MMCODE = mmcode,
                SAFE_DAY = safe_day,
                OPER_DAY = oper_day,
                SHIP_DAY = ship_day,
                HIGH_QTY = high_qty,
                LOW_QTY = low_qty,
                MIN_ORDQTY = min_ordqty,
                IS_AUTO = is_auto,
                CTDMDCCODE = ctdmdccode,
                CREATE_USER = user,
                UPDATE_USER = user,
                UPDATE_IP = update_ip,
                USEADJ_CLASS = useadj_class,
                ISSPLIT = issplit
            }, DBWork.Transaction);
        }

        public string SelectAVG_USEQTYL(string wh_no, string mmcode)
        {
            var sql = @"SELECT AVG_USEQTY FROM V_MM_AVGUSE WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string> (sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string SelectSAFE_QTY(string wh_no, string mmcode)
        {
            var sql = @"SELECT SAFE_QTY FROM V_MM_WHINVCTL WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string SelectOPER_QTY(string wh_no, string mmcode)
        {
            var sql = @"SELECT OPER_QTY FROM V_MM_WHINVCTL WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectSAFE_DAY(string wh_no, string mmcode)
        {
            var sql = @"SELECT SAFE_DAY FROM V_MM_WHINVCTL WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectOPER_DAY(string wh_no, string mmcode)
        {
            var sql = @"SELECT OPER_DAY FROM V_MM_WHINVCTL WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectSHIP_DAY(string wh_no, string mmcode)
        {
            var sql = @"SELECT SHIP_DAY FROM V_MM_WHINVCTL WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectMIN_ORDQTY(string wh_no, string mmcode)
        {
            var sql = @"SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectIS_AUTO(string wh_no, string mmcode)
        {
            var sql = @"SELECT IS_AUTO FROM MI_WINVCTL WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectHIGH_QTY(string wh_no, string mmcode)
        {
            var sql = @"SELECT HIGH_QTY FROM MI_WINVCTL WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectLOW_QTY(string wh_no, string mmcode)
        {
            var sql = @"SELECT LOW_QTY FROM MI_WINVCTL WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectNOWCONSUMEFLAG(string wh_no, string mmcode)
        {
            var sql = @"SELECT NOWCONSUMEFLAG FROM MI_WINVCTL WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectUSEADJ_CLASS(string wh_no, string mmcode)
        {
            var sql = @"SELECT data_value || ' ' || data_desc FROM  PARAM_D a
                         WHERE grp_code = 'MI_WINVCTL' 
                           and data_name = 'USEADJ_CLASS'
                           and data_value = (select USEADJ_CLASS from MI_WINVCTL where wh_no = :wh_no and mmcode = :mmcode)";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { wh_no = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string SelectISSPLIT(string wh_no, string mmcode)
        {
            var sql = @"SELECT ISSPLIT FROM MI_WINVCTL WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        //匯出
        public DataTable GetExcel(string wh_no, string mmcode, string MatClass, string[] str_ctdm)
        {
            var p = new DynamicParameters();

            var sql90D = @", a.FSTACKDATE as 第一次接收日期,
                             a.DAVG_USEQTY_90 as ""90天日平均消耗量"",
                             a.SAFE_QTY_90 as 安全量90,
                             a.OPER_QTY_90 as 作業量90,
                             a.SHIP_QTY_90 as 運補量90,
                             a.HIGH_QTY_90 as 基準量90
                         ";
            

            var sql = @"SELECT 
                    (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='HIS_BASORDM' AND DATA_NAME='DANGERDRUGFLAG' AND DATA_VALUE=c.DANGERDRUGFLAG) 高警訊,
                    (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_MAST' AND DATA_NAME='E_RESTRICTCODE' AND DATA_VALUE=b.E_RESTRICTCODE) 管制用藥,
                    '' 檢核結果,A.WH_NO 庫房代碼, A.MMCODE 院內碼, b.MMNAME_E || ' ' || b.MMNAME_C AS 名稱,
                    (SELECT TWN_DATE(MAX(EXP_DATE)) FROM MI_WEXPINV WHERE WH_NO = A.WH_NO AND MMCODE = A.MMCODE) 最新效期,
                     (SELECT INV_QTY FROM MI_WHINV WHERE WH_NO=A.WH_NO AND MMCODE=A.MMCODE) 現有庫存 , 
                     A.DAVG_USEQTY 日平均消耗量, 
                    A.SAFE_DAY 安全日, A.SAFE_QTY 安全量, 
                    A.OPER_DAY 作業日, A.OPER_QTY 作業量,
                    A.SHIP_DAY 運補日,A.HIGH_QTY 基準量, A.LOW_QTY 最低庫存量,STORE_LOC(A.WH_NO,A.MMCODE) 儲位碼,
                     a.IS_AUTO ""是否自動撥補(Y/N)"",
                    A.MIN_ORDQTY 最小撥補量,STOCKUNIT(A.MMCODE) 扣庫單位,
                    (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WINVCTL' AND DATA_NAME='CTDMDCCODE' AND DATA_VALUE=A.CTDMDCCODE) 各庫停用碼, 
                    (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WINVCTL' AND DATA_NAME='USEADJ_CLASS' AND DATA_VALUE=A.useadj_class) 醫令扣庫歸整,
                    a.isSPLIT ""自動申領是否拆單(Y/N)"" ";

            p.Add(":MMCODE", mmcode);

            if (MatClass=="01") //物料分類=01藥品
            {
                sql = sql + sql90D;
            }

            sql += @"FROM MI_WINVCTL a 
                    INNER JOIN MI_MAST b ON b.MMCODE = a.MMCODE 
                    LEFT OUTER JOIN HIS_BASORDM c ON c.ORDERCODE = a.MMCODE 
                    WHERE 1=1 ";

            if (wh_no != "" && wh_no != null)
            {
                string[] tmpWh_no = wh_no.Split(',');
                sql += " AND a.WH_NO in :p0 ";
                p.Add(":p0", tmpWh_no);
            }
            if (mmcode != "")
            {
                sql += " AND a.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            if (MatClass != "" && MatClass != null)
            {
                sql += " AND EXISTS (SELECT 1 FROM MI_MAST WHERE MMCODE = A.MMCODE AND MAT_CLASS = :MatClass) ";
                p.Add(":MatClass", string.Format("{0}", MatClass));
            }
            if (str_ctdm.Length > 0)
            {
                string sql_ctdm = "";
                sql += @"AND (";
                foreach (string tmp_ctdm in str_ctdm)
                {
                    if (string.IsNullOrEmpty(sql_ctdm))
                    {
                        sql_ctdm = @"A.CTDMDCCODE = '" + tmp_ctdm + "'";
                    }
                    else
                    {
                        sql_ctdm += @" OR A.CTDMDCCODE = '" + tmp_ctdm + "'";
                    }
                }
                sql += sql_ctdm + ") ";
            }
            sql += "   order by a.mmcode";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        #region 2020-05-13 新增: 檢查是否有庫房權限

        public bool CheckInWhid(string userid, string wh_no) {
            string sql = @"select 1 from MI_WHID 
                            where wh_no in :wh_no and wh_userid = :userid";

            string[] tmpWh_no = null;
            if (wh_no != null)
                tmpWh_no = wh_no.Split(',');

            return !(DBWork.Connection.ExecuteScalar(sql, new { wh_no = tmpWh_no, userid = userid }, DBWork.Transaction) == null);
        }

        #endregion

        #region 2020-08-14 新增: 新增USEADJ_CLASS欄位
        public IEnumerable<COMBO_MODEL> GetUseadjClassCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_WINVCTL' AND DATA_NAME='USEADJ_CLASS' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        #endregion
    }

}