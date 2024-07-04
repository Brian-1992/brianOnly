using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Reflection;

namespace WebApp.Repository.AA
{
    public class AA0125Repository : JCLib.Mvc.BaseRepository
    {
        public AA0125Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        #region 可修改欄位參數設定
        // 次月衛材
        List<string> canEditFields02 = new List<string>() {
            "M_APPLYID",    // 申請申購識別碼
            "MIN_ORDQTY",   // 最小撥補量
            "M_PURUN",      // 申購計量單位
            "EXCH_RATIO",   // 廠商包裝轉換率
            "BEGINDATE",    // 生效起日
            "ENDDATE"       // 生效迄日
        };
        // 次月一般物品
        List<string> canEditFields03 = new List<string>() {
            "M_APPLYID",    // 申請申購識別碼
            "MIN_ORDQTY",   // 最小撥補量
            "M_PURUN",      // 申購計量單位
            "EXCH_RATIO",   // 廠商包裝轉換率
            "M_AGENNO",     // 廠商代碼    
            "M_CONTPRICE",  // 合約單價
            "MMNAME_C",     // 中文品名
            "M_STOREID",    // 庫備識別碼
            "M_CONTID",     // 合約識別碼
            "BEGINDATE",    // 生效起日
            "ENDDATE",      // 生效迄日
            "M_PHCTNCO",    // 環保或衛署許可證
            "M_ENVDT"       // 環保證號效期
        };

        // 年度衛材
        List<string> canEditFieldsY02 = new List<string>() {
            "M_APPLYID",    // 申請申購識別碼
            "MIN_ORDQTY",   // 最小撥補量
            "M_PURUN",      // 申購計量單位
            "EXCH_RATIO",   // 廠商包裝轉換率
            "M_AGENNO",     // 廠商代碼  
            "MMNAME_C",     // 中文品名
            "MMNAME_E",     // 英文品名
            "M_VOLL",       // 長
            "M_VOLW",       // 寬
            "M_VOLH",       // 高
            "M_VOLC",       // 圓周
            "M_SWAP",       // 材積轉換率  
            "M_CONTPRICE",  // 合約單價
            "M_DISCPERC",   // 折讓比
            "M_CONTID",     // 合約識別碼
            "BEGINDATE",    // 生效起日
            "ENDDATE"       // 生效迄日
        };
        // 年度一般物品
        List<string> canEditFieldsY03 = new List<string>() {
            "M_APPLYID",    // 申請申購識別碼
            "MIN_ORDQTY",   // 最小撥補量
            "M_PURUN",      // 申購計量單位
            "EXCH_RATIO",   // 廠商包裝轉換率
            "M_AGENNO",     // 廠商代碼    
            "M_CONTPRICE",  // 合約單價
            "MMNAME_C",     // 中文品名
            "M_STOREID",    // 庫備識別碼
            "M_CONTID",     // 合約識別碼
            "BEGINDATE",    // 生效起日
            "ENDDATE",      // 生效迄日
            "M_PHCTNCO",    // 環保或衛署許可證
            "M_ENVDT",      // 環保證號效期
            "M_SUPPLYID",   // 是否供應契約
            "M_VOLL",       // 長
            "M_VOLW",       // 寬
            "M_VOLH",       // 高
            "M_VOLC",       // 圓周
            "M_SWAP"        // 材積轉換率  
        };

        #endregion

        public IEnumerable<MI_MAST> GetAll(string mmcode, string mmname_c, string mmname_e, string mat_class, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, 
                        (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as MAT_CLASS, 
                        (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end) from MI_UNITCODE where UNIT_CODE = trim(A.BASE_UNIT)) as BASE_UNIT, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST_N' and DATA_NAME = 'M_STOREID' and DATA_VALUE = A.M_STOREID) as M_STOREID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST_N' and DATA_NAME = 'M_CONTID' and DATA_VALUE = A.M_CONTID) as M_CONTID, 
                        A.M_IDKEY, A.M_INVKEY, A.M_NHIKEY, A.M_GOVKEY, 
                        A.M_VOLL, A.M_VOLW, A.M_VOLH, A.M_VOLC, A.M_SWAP, A.M_MATID, A.M_SUPPLYID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST_N' and DATA_NAME = 'M_CONSUMID' and DATA_VALUE = A.M_CONSUMID) as M_CONSUMID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST_N' and DATA_NAME = 'M_PAYKIND' and DATA_VALUE = A.M_PAYKIND) as M_PAYKIND, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST_N' and DATA_NAME = 'M_PAYID' and DATA_VALUE = A.M_PAYID) as M_PAYID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST_N' and DATA_NAME = 'M_TRNID' and DATA_VALUE = A.M_TRNID) as M_TRNID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST_N' and DATA_NAME = 'M_APPLYID' and DATA_VALUE = A.M_APPLYID) as M_APPLYID, 
                        A.M_PHCTNCO, A.M_ENVDT, A.M_AGENNO, A.M_AGENLAB, 
                        (select case when AGEN_NAMEC is null then AGEN_NAMEE else AGEN_NAMEC end from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                        (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end) from MI_UNITCODE where UNIT_CODE = A.M_PURUN) as M_PURUN, 
                        A.M_CONTPRICE, A.M_DISCPERC, A.CANCEL_ID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST_N' and DATA_NAME = 'WEXP_ID' and DATA_VALUE = A.WEXP_ID) as WEXP_ID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST_N' and DATA_NAME = 'WLOC_ID' and DATA_VALUE = A.WLOC_ID) as WLOC_ID,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CONTRACNO' and DATA_VALUE = A.CONTRACNO) as CONTRACNO,
                        A.UPRICE, A.DISC_CPRICE, A.DISC_UPRICE, A.EASYNAME, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CANCEL_NOTE' and DATA_VALUE = A.CANCEL_NOTE) as CANCEL_NOTE, A.NHI_PRICE,
                        A.E_ITEMARMYNO, A.E_YRARMYNO, A.E_CLFARMYNO,
                        (select HOSP_PRICE from MI_MAST where MMCODE = A.MMCODE) as HOSP_PRICE,
                        A.BEGINDATE, A.ENDDATE, EXCH_RATIO, MIN_ORDQTY,
                        TO_CHAR(TWN_TODATE(A.BEGINDATE), 'YYYY-MM-DD') as BEGINDATE_DATE,
                        TO_CHAR(TWN_TODATE(A.ENDDATE), 'YYYY-MM-DD') as ENDDATE_DATE
                        FROM MI_MAST_N A WHERE 1=1 ";

            if (mmcode != "" && mmcode != null)
            {
                sql += " AND A.MMCODE LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", mmcode));
            }
            if (mmname_c != "" && mmname_c != null)
            {
                sql += " AND A.MMNAME_C LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmname_c));
            }
            if (mmname_e != "" && mmname_e != null)
            {
                sql += " AND A.MMNAME_E LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmname_e));
            }
            if (mat_class != "" && mat_class != null)
            {
                sql += " AND Trim(A.MAT_CLASS) = :p3 ";
                p.Add(":p3", mat_class);
            }

            // if () 可能需要加上權限判斷可顯示全部者
            sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) in ('2', '3') ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> Get(string id, string loadType)
        {
            string qTable = "MI_MAST_N";
            if (loadType == "O")
                qTable = "MI_MAST";
            var sql = @" SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, 
                        A.BASE_UNIT, A.M_STOREID, A.M_CONTID, A.M_IDKEY, A.M_INVKEY, A.M_NHIKEY, A.M_GOVKEY, 
                        A.M_VOLL, A.M_VOLW, A.M_VOLH, A.M_VOLC, A.M_SWAP, A.M_MATID, A.M_SUPPLYID, A.M_CONSUMID, A.M_PAYKIND, A.M_PAYID,
                        A.M_TRNID, A.M_APPLYID, A.M_PHCTNCO, A.M_ENVDT, A.M_AGENNO, A.M_AGENLAB, A.M_PURUN, A.M_CONTPRICE, A.M_DISCPERC, A.CANCEL_ID, A.WEXP_ID, A.WLOC_ID,
                        A.CONTRACNO, A.UPRICE, A.DISC_CPRICE, A.DISC_UPRICE, A.EASYNAME, A.CANCEL_NOTE, A.NHI_PRICE, A.E_ITEMARMYNO, A.E_YRARMYNO, A.E_CLFARMYNO,
                        (select case when AGEN_NAMEC is null then AGEN_NAMEE else AGEN_NAMEC end from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                        (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) as MAT_CLSID,
                        (select HOSP_PRICE from MI_MAST where MMCODE = A.MMCODE) as HOSP_PRICE,
                        A.BEGINDATE, A.ENDDATE, EXCH_RATIO, MIN_ORDQTY,
                        TO_CHAR(TWN_TODATE(A.BEGINDATE), 'YYYY-MM-DD') as BEGINDATE_DATE,
                        TO_CHAR(TWN_TODATE(A.ENDDATE), 'YYYY-MM-DD') as ENDDATE_DATE
                        FROM " + qTable + @" A WHERE MMCODE=:MMCODE ";
            return DBWork.Connection.Query<MI_MAST>(sql, new { MMCODE = id }, DBWork.Transaction);
        }

        public int Create(MI_MAST mi_mast_n)
        {
            var sql = @"INSERT INTO MI_MAST_N (MMCODE, MMNAME_C, MMNAME_E, MAT_CLASS, BASE_UNIT, M_STOREID, M_CONTID, M_IDKEY, M_INVKEY, M_NHIKEY, M_GOVKEY, M_VOLL, M_VOLW, M_VOLH, M_VOLC, M_SWAP,
                                M_MATID, M_SUPPLYID, M_CONSUMID, M_PAYKIND, M_PAYID, M_TRNID, M_APPLYID, M_PHCTNCO, M_ENVDT, M_AGENNO, M_AGENLAB, M_PURUN, M_CONTPRICE, M_DISCPERC, 
                                WEXP_ID, WLOC_ID, CANCEL_ID, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, E_INVFLAG, E_PURTYPE, E_SOURCECODE, E_PARCODE, E_SONTRANSQTY, CONTRACNO, UPRICE, DISC_CPRICE, DISC_UPRICE, EASYNAME, CANCEL_NOTE, NHI_PRICE, E_ITEMARMYNO, E_YRARMYNO, E_CLFARMYNO, EXCH_RATIO, MIN_ORDQTY,
                                BEGINDATE, ENDDATE)  
                                VALUES (:MMCODE, :MMNAME_C, :MMNAME_E, :MAT_CLASS, :BASE_UNIT, :M_STOREID, :M_CONTID, :M_IDKEY, :M_INVKEY, :M_NHIKEY, :M_GOVKEY, :M_VOLL, :M_VOLW, :M_VOLH, :M_VOLC, :M_SWAP,
                                :M_MATID, :M_SUPPLYID, :M_CONSUMID, :M_PAYKIND, :M_PAYID, :M_TRNID, :M_APPLYID, :M_PHCTNCO, :M_ENVDT, :M_AGENNO, :M_AGENLAB, :M_PURUN, :M_CONTPRICE, :M_DISCPERC, 
                                :WEXP_ID, :WLOC_ID, :CANCEL_ID, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, '0', '0', '0', '0', 0, :CONTRACNO, :UPRICE, :DISC_CPRICE, :DISC_UPRICE, :EASYNAME, :CANCEL_NOTE, :NHI_PRICE, :E_ITEMARMYNO, :E_YRARMYNO, :E_CLFARMYNO, :EXCH_RATIO, :MIN_ORDQTY,
                                :BEGINDATE, :ENDDATE)";
            return DBWork.Connection.Execute(sql, mi_mast_n, DBWork.Transaction);
        }

        public int Update(MI_MAST mi_mast_n)
        {
            var sql = @"UPDATE MI_MAST_N SET MMNAME_C = :MMNAME_C, MMNAME_E = :MMNAME_E, MAT_CLASS = :MAT_CLASS, BASE_UNIT = :BASE_UNIT, M_STOREID = :M_STOREID, M_CONTID = :M_CONTID, M_IDKEY = :M_IDKEY, M_INVKEY = :M_INVKEY, 
                                M_NHIKEY = :M_NHIKEY, M_GOVKEY = :M_GOVKEY, M_VOLL = :M_VOLL, M_VOLW = :M_VOLW, M_VOLH = :M_VOLH, M_VOLC = :M_VOLC, M_SWAP = :M_SWAP,
                                M_MATID = :M_MATID, M_SUPPLYID = :M_SUPPLYID, M_CONSUMID = :M_CONSUMID, M_PAYKIND = :M_PAYKIND, M_PAYID = :M_PAYID, M_TRNID = :M_TRNID, M_APPLYID = :M_APPLYID, M_PHCTNCO = :M_PHCTNCO, M_ENVDT = :M_ENVDT,
                                M_AGENNO = :M_AGENNO, M_AGENLAB = :M_AGENLAB, M_PURUN = :M_PURUN, M_CONTPRICE = :M_CONTPRICE, M_DISCPERC = :M_DISCPERC, 
                                WEXP_ID = :WEXP_ID, WLOC_ID = :WLOC_ID, CANCEL_ID = :CANCEL_ID, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP, CONTRACNO = :CONTRACNO, UPRICE = :UPRICE, DISC_CPRICE = :DISC_CPRICE, DISC_UPRICE = :DISC_UPRICE,
                                EASYNAME = :EASYNAME, CANCEL_NOTE = :CANCEL_NOTE, NHI_PRICE = :NHI_PRICE, E_ITEMARMYNO = :E_ITEMARMYNO, E_YRARMYNO = :E_YRARMYNO, E_CLFARMYNO = :E_CLFARMYNO, EXCH_RATIO = :EXCH_RATIO, MIN_ORDQTY = :MIN_ORDQTY,
                                BEGINDATE = :BEGINDATE, ENDDATE = :ENDDATE
                                WHERE MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, mi_mast_n, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E
                            from MI_MAST A where (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) in ( '3') ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10 + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMNAME_E_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

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

        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST_N WHERE MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public bool CheckMmcodeRef(string id)
        {
            string sql = @"SELECT 1 FROM dual 
                        WHERE (select count(*) from MI_WHINV where MMCODE=:MMCODE) > 0
                        or (select count(*) from MI_WLOCINV where MMCODE=:MMCODE) > 0
                        or (select count(*) from MI_WEXPINV where MMCODE=:MMCODE) > 0 ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public IEnumerable<COMBO_MODEL> GetDefaultBeginDate()
        {
            string sql = @"select to_char(add_months(sysdate, 1), 'YYYY-MM')||'-01' as value,
                                  to_char(twn_todate('9991231'), 'YYYY-MM-DD') as text
                             from dual";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public DataTable GetImportSampleExcel()
        {
            string sql = @"select '' as ""院內碼"",
                                  '' as ""中文品名"",
                                  '' as ""申請申購識別碼"",
                                  '' as ""最小撥補量"",
                                  '' as ""申購計量單位"",
                                  '' as ""廠商包裝轉換率"",
                                  '' as ""廠商代碼"",
                                  '' as ""合約單價"",
                                  '' as ""折讓比"",
                                  '' as ""庫備識別碼"",
                                  '' as ""合約識別碼"",
                                  '' as ""環保或衛署許可證"",
                                  '' as ""環保證號效期"",
                                  twn_date(sysdate+1) as ""生效起日(民國年月日)"",
                                  '9991231' as ""生效迄日(民國年月日)""        
                             from dual ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetImportSampleYExcel()
        {
            string sql = @"select '' as ""院內碼"",
                                  '' as ""中文品名"",
                                  '' as ""英文品名"",
                                  '' as ""申請申購識別碼"",
                                  '' as ""最小撥補量"",
                                  '' as ""申購計量單位"",
                                  '' as ""廠商包裝轉換率"",
                                  '' as ""廠商代碼"",
                                  '' as ""合約單價"",
                                  '' as ""折讓比"",
                                  '' as ""庫備識別碼"",
                                  '' as ""合約識別碼"",
                                  '' as ""環保或衛署許可證"",
                                  '' as ""環保證號效期"",
                                  '' as ""是否供應契約"",
                                  '' as ""長"",
                                  '' as ""寬"",
                                  '' as ""高"",
                                  '' as ""圓周"",
                                  '' as ""材積轉換率"",
                                  twn_date(sysdate+1) as ""生效起日(民國年月日)"",
                                  '9991231' as ""生效迄日(民國年月日)""        
                             from dual ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        #region 上傳

        #region 上傳檢核

        public bool CheckMmcodeExists(string mmcode)
        {
            string sql = @"select 1 from MI_MAST where mmcode = :mmcode";
            return !(DBWork.Connection.ExecuteScalar(sql, new { mmcode = mmcode }, DBWork.Transaction) == null);
        }

        public MI_MAST GetMiMastNByMmcode(string mmcode)
        {
            string sql = @"select * from MI_MAST_N where mmcode = :mmcode";
            return DBWork.Connection.QueryFirst<MI_MAST>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }

        public MI_MAST GetMiMastByMmcode(string mmcode)
        {
            string sql = @"select a.*,
                                  (select exch_ratio from MI_UNITEXCH 
                                    where mmcode = a.mmcode
                                      and unit_code = a.m_purun 
                                      and agen_no = a.m_agenno) as exch_ratio
                             from MI_MAST a where mmcode = :mmcode";
            return DBWork.Connection.QueryFirst<MI_MAST>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }

        public bool CheckMatClsid(string mat_class)
        {
            string sql = @"select 1 from MI_MATCLASS 
                            where mat_class = :mat_class
                              and mat_clsid in  (2, 3)";
            return !(DBWork.Connection.ExecuteScalar(sql, new { mat_class = mat_class }, DBWork.Transaction) == null);
        }
        public bool CheckUnitCodeExists(string unit_code) {
            string sql = @"select 1 from MI_UNITCODE
                            where unit_code = :unit_code";
            return !(DBWork.Connection.ExecuteScalar(sql, new { unit_code = unit_code}, DBWork.Transaction) == null);
        }

        public bool CheckParamExists(string grp_code, string data_name, string data_value)
        {
            string sql = @"select 1 from PARAM_D 
                            where grp_code = :grp_code
                              and data_name = :data_name
                              and data_value = :data_value";
            return !(DBWork.Connection.ExecuteScalar(sql, new { grp_code = grp_code, data_name = data_name, data_value = data_value }, DBWork.Transaction) == null);
        }

        public bool CheckAgennoExists(string agen_no)
        {
            string sql = @"select 1 from PH_VENDER 
                            where agen_no = :agen_no";
            return !(DBWork.Connection.ExecuteScalar(sql, new { agen_no = agen_no }, DBWork.Transaction) == null);
        }

        public int UpdateMiMastN(MI_MAST mast, bool isNew, bool isYear)
        {
            List<string> editableColumns = new List<string>();

            if (mast.MAT_CLASS == "02" && isYear)
            {
                editableColumns = canEditFieldsY02;
            }
            if (mast.MAT_CLASS != "02" && isYear)
            {
                editableColumns = canEditFieldsY03;
            }
            if (mast.MAT_CLASS == "02" && isYear == false)
            {
                editableColumns = canEditFields02;
            }
            if (mast.MAT_CLASS != "02" && isYear == false)
            {
                editableColumns = canEditFields03;
            }

            string sql = string.Format(@"
                           update MI_MAST_N
                              set update_user = :UPDATE_USER,
                                  update_ip = :UPDATE_IP,
                                  update_time = sysdate,
                                  {1}
                                  {2}
                                  {0}
                            where mmcode = :mmcode"
                    , GetUpdateColumns(mast, editableColumns)
                    , isNew ? "  create_user = :CREATE_USER ," : string.Empty
                    , isNew ? "  create_time = sysdate ," : string.Empty);
            return DBWork.Connection.Execute(sql, mast, DBWork.Transaction);
        }

        private string GetUpdateColumns(MI_MAST mast, List<string> editableColumns)
        {
            string update_string = string.Empty;

            List<string> properties = GetPropertiesFromType(mast);

            foreach (string property in properties)
            {
                if (editableColumns.Contains(property))
                {
                    if (string.IsNullOrEmpty(mast.GetType().GetProperty(property).GetValue(mast, null).ToString()) == false)
                    {
                        if (property == "M_CONTPRICE")
                        {
                            update_string += string.Format(@" M_CONTPRICE = :M_CONTPRICE, 
                                                              DISC_CPRICE = :DISC_CPRICE,
                                                              UPRICE = :UPRICE,
                                                              DISC_UPRICE = :DISC_UPRICE,
                                                ");
                        }
                        else
                        {
                            update_string += string.Format("{0} = :{0},", property.ToUpper());
                        }

                    }
                }
            }

            update_string = update_string.Substring(0, update_string.Length - 1);

            return update_string;
        }

        private List<string> GetPropertiesFromType(object atype)
        {
            if (atype == null) return new List<string>();
            Type t = atype.GetType();
            PropertyInfo[] props = t.GetProperties();
            List<string> propNames = new List<string>();
            foreach (PropertyInfo prp in props)
            {
                propNames.Add(prp.Name);
            }
            return propNames;
        }

        public int InsertMiMastN(MI_MAST mast)
        {
            string sql = @"INSERT INTO MI_MAST_N 
                                  (MMCODE, MMNAME_C, MMNAME_E, MAT_CLASS, BASE_UNIT, M_STOREID, M_CONTID, M_IDKEY, M_INVKEY, M_NHIKEY, M_GOVKEY, M_VOLL, M_VOLW, M_VOLH, M_VOLC, M_SWAP,
                                M_MATID, M_SUPPLYID, M_CONSUMID, M_PAYKIND, M_PAYID, M_TRNID, M_APPLYID, M_PHCTNCO, M_ENVDT, M_AGENNO, M_AGENLAB, M_PURUN, M_CONTPRICE, M_DISCPERC, 
                                WEXP_ID, WLOC_ID, CANCEL_ID, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, E_INVFLAG, E_PURTYPE, E_SOURCECODE, E_PARCODE, E_SONTRANSQTY, CONTRACNO, UPRICE, DISC_CPRICE, DISC_UPRICE, EASYNAME, CANCEL_NOTE, NHI_PRICE, E_ITEMARMYNO, E_YRARMYNO, E_CLFARMYNO)  
                            select MMCODE, MMNAME_C, MMNAME_E, MAT_CLASS, BASE_UNIT, M_STOREID, M_CONTID, M_IDKEY, M_INVKEY, M_NHIKEY, M_GOVKEY, M_VOLL, M_VOLW, M_VOLH, M_VOLC, M_SWAP,
                                M_MATID, M_SUPPLYID, M_CONSUMID, M_PAYKIND, M_PAYID, M_TRNID, M_APPLYID, M_PHCTNCO, M_ENVDT, M_AGENNO, M_AGENLAB, M_PURUN, M_CONTPRICE, M_DISCPERC, 
                                WEXP_ID, WLOC_ID, CANCEL_ID, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, E_INVFLAG, E_PURTYPE, E_SOURCECODE, E_PARCODE, E_SONTRANSQTY, CONTRACNO, UPRICE, DISC_CPRICE, DISC_UPRICE, EASYNAME, CANCEL_NOTE, NHI_PRICE, E_ITEMARMYNO, E_YRARMYNO, E_CLFARMYNO
                             from MI_MAST
                            where mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, new { mmcode = mast.MMCODE }, DBWork.Transaction);
        }
        #endregion

        #endregion

        #region 2021-02-22 取得MI_WINVCTL資料
        public MI_WINVCTL GetWinvctl(string mmcode)
        {
            string sql = @"select * from MI_WINVCTL
                            where mmcode = :mmcode
                              and wh_no = WHNO_MM1";
            return DBWork.Connection.QueryFirstOrDefault<MI_WINVCTL>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }

        #endregion

        #region 2021-02-22 檢查是否MI_UNITEXCH有資料，無資料新增，有資料更新
        public bool CheckUnitExchExists(string mmcode, string agen_no, string unit_code)
        {
            string sql = @"select 1 from MI_UNITEXCH
                            where mmcode = :mmcode
                              and agen_no = :agen_no
                              and unit_code = :unit_code";
            return !(DBWork.Connection.ExecuteScalar(sql, new { mmcode = mmcode, agen_no = agen_no, unit_code = unit_code }, DBWork.Transaction) == null);
        }

        public string GetExchRatio(string mmcode, string agen_no, string unit_code)
        {
            string sql = @"select 1 from MI_UNITEXCH
                            where mmcode = :mmcode
                              and agen_no = :agen_no
                              and unit_code = :unit_code";
            string result = DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode = mmcode, agen_no = agen_no, unit_code = unit_code }, DBWork.Transaction);
            if (result == null) {
                return "1";
            }

            sql = @"select exch_ratio from MI_UNITEXCH
                     where mmcode = :mmcode
                       and agen_no = :agen_no
                       and unit_code = :unit_code";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode = mmcode, agen_no = agen_no, unit_code = unit_code }, DBWork.Transaction);
        }

        public int InsertUnitExch(string mmcode, string agen_no, string unit_code, string exch_ratio, string update_user, string update_ip)
        {
            string sql = @"insert into MI_UNITEXCH (mmcode, unit_code, agen_no, exch_ratio,
                                                    create_time, create_user, update_time, update_user, update_ip)
                           values (:mmcode, :unit_code, :agen_no, :exch_ratio,
                                   sysdate, :update_user, sysdate, :update_user, :update_ip)";
            return DBWork.Connection.Execute(sql, new
            {
                mmcode = mmcode,
                agen_no = agen_no,
                unit_code = unit_code,
                exch_ratio = exch_ratio,
                update_user = update_user,
                update_ip = update_ip
            }, DBWork.Transaction);
        }

        public int UpdateUnitExch(string mmcode, string agen_no, string unit_code, string exch_ratio, string update_user, string update_ip)
        {
            string sql = @"update MI_UNITEXCH
                              set exch_ratio = :exch_ratio,
                                  update_time = sysdate,
                                  update_user = :update_user,
                                  update_ip = :update_ip
                            where mmcode = :mmcode
                              and agen_no = :agen_no
                              and unit_code = :unit_code";
            return DBWork.Connection.Execute(sql, new
            {
                mmcode = mmcode,
                agen_no = agen_no,
                unit_code = unit_code,
                exch_ratio = exch_ratio,
                update_user = update_user,
                update_ip = update_ip
            }, DBWork.Transaction);
        }

        #endregion

        #region 2021-04-07 生效日不可小於明日
        public bool CheckBeginDateValid(string begin_date)
        {
            string sql = @"select (case when twn_date(sysdate+1)  <= :begin_date then 'Y' else 'N' end) from dual";
            return (DBWork.Connection.ExecuteScalar<string>(sql, new { begin_date = begin_date }, DBWork.Transaction) == "Y");
        }
        #endregion

        #region 檢查院內碼物料類別
        public string CheckMmcodeMatClass(string mmcode) {
            string sql = @"
                select mat_class from MI_MAST
                 where mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode }, DBWork.Transaction);
        }

        #endregion
    }
}