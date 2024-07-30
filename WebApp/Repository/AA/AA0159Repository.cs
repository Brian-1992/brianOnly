using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0159Repository : JCLib.Mvc.BaseRepository
    {
        public AA0159Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_MAST_HISTORY> GetAll(string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select MMCODE, EFFSTARTDATE, EFFENDDATE, TWN_DATE(EFFSTARTDATE) AS EFFSTARTDATE_T, TWN_DATE(EFFENDDATE) AS EFFENDDATE_T, CANCEL_ID, 
                               M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, 
                               MMNAME_C, M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, 
                               BASE_UNIT, M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, 
                               WARBAK, ONECOST, HEALTHPAY, COSTKIND, WASTKIND, 
                               SPXFEE, ORDERKIND, CASEDOCT, DRUGKIND, M_AGENNO, 
                               M_AGENLAB, CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, 
                               NHI_PRICE, DISC_CPRICE, M_CONTPRICE, E_CODATE, TWN_DATE(E_CODATE) AS E_CODATE_T, CONTRACTAMT, 
                               CONTRACTSUM, TOUCHCASE, BEGINDATE_14, ISSPRICEDATE, SPDRUG, 
                               FASTDRUG, TWN_DATE(CREATE_TIME) AS CREATE_TIME, CREATE_USER, UPDATE_IP, COMMON, 
                               SPMMCODE, UNITRATE,
                               (select EASYNAME from ph_vender where AGEN_NO=A.M_AGENNO)EASYNAME, 
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_SOURCECODE'
                                   and DATA_VALUE = a.E_SOURCECODE) as E_SOURCECODE_DESC,
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_CONTID'
                                   and DATA_VALUE = a.M_CONTID) as M_CONTID_DESC,
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'TOUCHCASE'
                                   and DATA_VALUE = a.TOUCHCASE) as TOUCHCASE_DESC,
                               TWN_DATE(BEGINDATE_14) AS BEGINDATE_14_T,
                               TWN_DATE(ISSPRICEDATE) AS ISSPRICEDATE_T, 
                               MIMASTHIS_SEQ, a.DISCOUNT_QTY, a.APPQTY_TIMES,
                               A.ISIV, A.DISC_COST_UPRICE, A.M_STOREID,    
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_STOREID'
                                   and DATA_VALUE = a.M_STOREID) as M_STOREID_DESC  
                          from MI_MAST_HISTORY a
                         where MMCODE = :P0
                         order by MIMASTHIS_SEQ DESC";

            p.Add(":P0", mmcode);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST_HISTORY>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST_HISTORY> Get(string id)
        {
            var sql = @" select MMCODE, EFFSTARTDATE, EFFENDDATE, TWN_DATE(EFFSTARTDATE) AS EFFSTARTDATE_T, TWN_DATE(EFFENDDATE) AS EFFENDDATE_T, CANCEL_ID, 
                               M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, 
                               MMNAME_C, M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, 
                               BASE_UNIT, M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, 
                               WARBAK, ONECOST, HEALTHPAY, COSTKIND, WASTKIND, 
                               SPXFEE, ORDERKIND, CASEDOCT, DRUGKIND, M_AGENNO, 
                               M_AGENLAB, CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, 
                               NHI_PRICE, DISC_CPRICE, M_CONTPRICE, E_CODATE, TWN_DATE(E_CODATE) AS E_CODATE_T, CONTRACTAMT, 
                               CONTRACTSUM, TOUCHCASE, BEGINDATE_14, ISSPRICEDATE, SPDRUG, 
                               FASTDRUG, TWN_DATE(CREATE_TIME) AS CREATE_TIME, CREATE_USER, UPDATE_IP, COMMON, 
                               SPMMCODE, UNITRATE,
                               (select EASYNAME from ph_vender where AGEN_NO=A.M_AGENNO)EASYNAME, 
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_SOURCECODE'
                                   and DATA_VALUE = a.E_SOURCECODE) as E_SOURCECODE_DESC,
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_CONTID'
                                   and DATA_VALUE = a.M_CONTID) as M_CONTID_DESC,
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'TOUCHCASE'
                                   and DATA_VALUE = a.TOUCHCASE) as TOUCHCASE_DESC,
                               TWN_DATE(BEGINDATE_14) AS BEGINDATE_14_T,
                               TWN_DATE(ISSPRICEDATE) AS ISSPRICEDATE_T, 
                               MIMASTHIS_SEQ , a.DISCOUNT_QTY, a.APPQTY_TIMES,
                               A.ISIV, A.DISC_COST_UPRICE, A.M_STOREID,    
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_STOREID'
                                   and DATA_VALUE = a.M_STOREID) as M_STOREID_DESC      
                          from MI_MAST_HISTORY a
                         where MMCODE = :MMCODE
                         order by MIMASTHIS_SEQ  DESC ";
            return DBWork.Connection.Query<MI_MAST_HISTORY>(sql, new { MMCODE = id }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST_HISTORY> GetD(string seq)
        {
            var sql = @" select MMCODE, EFFSTARTDATE, EFFENDDATE, TWN_DATE(EFFSTARTDATE) AS EFFSTARTDATE_T, TWN_DATE(EFFENDDATE) AS EFFENDDATE_T, CANCEL_ID, 
                               M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, 
                               MMNAME_C, M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, 
                               BASE_UNIT, M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, 
                               WARBAK, ONECOST, HEALTHPAY, COSTKIND, WASTKIND, 
                               SPXFEE, ORDERKIND, CASEDOCT, DRUGKIND, M_AGENNO, 
                               M_AGENLAB, CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, 
                               NHI_PRICE, DISC_CPRICE, M_CONTPRICE, E_CODATE, TWN_DATE(E_CODATE) AS E_CODATE_T, CONTRACTAMT, 
                               CONTRACTSUM, TOUCHCASE, BEGINDATE_14, ISSPRICEDATE, SPDRUG, 
                               FASTDRUG, TWN_DATE(CREATE_TIME) AS CREATE_TIME, CREATE_USER, UPDATE_IP, COMMON, 
                               SPMMCODE, UNITRATE,
                               (select EASYNAME from ph_vender where AGEN_NO=A.M_AGENNO)EASYNAME, 
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_SOURCECODE'
                                   and DATA_VALUE = a.E_SOURCECODE) as E_SOURCECODE_DESC,
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_CONTID'
                                   and DATA_VALUE = a.M_CONTID) as M_CONTID_DESC,
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'TOUCHCASE'
                                   and DATA_VALUE = a.TOUCHCASE) as TOUCHCASE_DESC,
                               TWN_DATE(BEGINDATE_14) AS BEGINDATE_14_T,
                               TWN_DATE(ISSPRICEDATE) AS ISSPRICEDATE_T, 
                               MIMASTHIS_SEQ , a.DISCOUNT_QTY, a.APPQTY_TIMES,
                               A.ISIV, A.DISC_COST_UPRICE, A.M_STOREID,    
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_STOREID'
                                   and DATA_VALUE = a.M_STOREID) as M_STOREID_DESC      
                          from MI_MAST_HISTORY a
                         where MIMASTHIS_SEQ = :MIMASTHIS_SEQ  ";
            return DBWork.Connection.Query<MI_MAST_HISTORY>(sql, new { MIMASTHIS_SEQ = seq }, DBWork.Transaction);
        }
        public DataTable GetExcelExample()
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @" SELECT TWN_DATE(SYSDATE+1) 生效起始時間,
                                   '' 院內碼,
                                   '' 學名,
                                   '' 英文品名,
                                   '必填' 中文品名,
                                   '' 健保代碼,
                                   '' 健保自費碼,
                                   '' 許可證號,
                                   '' 許可證效期,
                                   '' 申請廠商,
                                   '' 製造商,
                                   'VIAL' 藥材單位,
                                   'BX' 出貨包裝單位,
                                   '50' 每包裝出貨量,
                                   '1' 物料子類別,
                                   '1' 是否常用品項,
                                   '1' 與HIS單位換算比值,
                                   '0' 是否可單一計價,
                                   '0' 是否健保給付,
                                   '0' 費用分類,
                                   'N' 管制級數,
                                   '0' 是否戰備,
                                   '0' 是否正向消耗,
                                   '0' 是否為特材,
                                   '0' 採購類別,
                                   '' 小採需求醫師,
                                   'N' 是否作廢,
                                   '0' 庫備識別碼,
                                   '0' 中西藥類別,
                                   'N' 是否點滴,
                                   '0' 特殊品項,
                                   '' 特材號碼,
                                   '0' 急救品項,
                                   '1' 申請倍數,                                
                                   '97160544' 廠商代碼,
                                   '' 合約案號,
                                   '' 廠牌,
                                   '0' 合約類別,
                                   TO_CHAR(SYSDATE+1,'YYYY/MM/DD') 合約到期日,
                                   'N' 付款方式,
                                   '0' 合約方式,
                                   '' 成本價,
                                   '' 決標價,
                                   '0' 二次折讓數量,
                                   '0' 二次優惠單價,
                                   '' 聯標項次,
                                   '' 健保價,
                                   '0' 聯標契約總數量,
                                   '0' 聯標項次契約總價
                            FROM DUAL";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public int Create(MI_MAST_HISTORY mi_mast)
        {
            var sql = @"INSERT INTO MI_MAST_HISTORY (
                                MIMASTHIS_SEQ, MMCODE, EFFSTARTDATE, CANCEL_ID,
                                M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, MMNAME_C, 
                                M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, BASE_UNIT, 
                                M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, WARBAK, 
                                ONECOST, HEALTHPAY, COSTKIND, WASTKIND, SPXFEE, 
                                ORDERKIND, CASEDOCT, DRUGKIND, COMMON, M_AGENNO, 
                                M_AGENLAB, CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, 
                                NHI_PRICE, DISC_CPRICE, M_CONTPRICE, CONTRACTAMT, CONTRACTSUM, 
                                TOUCHCASE,  SPDRUG, FASTDRUG, SPMMCODE, UNITRATE,
                                CREATE_TIME, CREATE_USER, UPDATE_IP, DISCOUNT_QTY, APPQTY_TIMES,
                                E_CODATE, DISC_COST_UPRICE, ISIV, M_STOREID  
                                )  
                                VALUES (
                                :MIMASTHIS_SEQ, :MMCODE, TWN_TODATE(:EFFSTARTDATE), :CANCEL_ID,
                                :M_NHIKEY, :HEALTHOWNEXP, :DRUGSNAME, :MMNAME_E, :MMNAME_C,
                                :M_PHCTNCO, :M_ENVDT, :ISSUESUPPLY, :E_MANUFACT, :BASE_UNIT,
                                :M_PURUN, :TRUTRATE, :MAT_CLASS_SUB, :E_RESTRICTCODE, :WARBAK,
                                :ONECOST, :HEALTHPAY, :COSTKIND, :WASTKIND, :SPXFEE,
                                :ORDERKIND, :CASEDOCT, :DRUGKIND, :COMMON, :M_AGENNO,
                                :M_AGENLAB, :CASENO, :E_SOURCECODE, :M_CONTID, :E_ITEMARMYNO, 
                                nvl(:NHI_PRICE,0), nvl(:DISC_CPRICE,0), nvl(:M_CONTPRICE,0), nvl(:CONTRACTAMT,0), :CONTRACTSUM,
                                :TOUCHCASE,  :SPDRUG, :FASTDRUG, :SPMMCODE, :UNITRATE,
                                SYSDATE, :CREATE_USER, :UPDATE_IP , :DISCOUNT_QTY, :APPQTY_TIMES ,
                                TWN_TODATE(:E_CODATE), :DISC_COST_UPRICE, :ISIV, :M_STOREID  
                                )";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int InsertHisFromMast(string seq, string id)
        {
            var sql = @"INSERT INTO MI_MAST_HISTORY (
                                MIMASTHIS_SEQ, MMCODE, EFFSTARTDATE, CANCEL_ID, 
                                M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, MMNAME_C, 
                                M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, BASE_UNIT, 
                                M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, WARBAK, 
                                ONECOST, HEALTHPAY, COSTKIND, WASTKIND, SPXFEE, 
                                ORDERKIND, CASEDOCT, DRUGKIND, M_AGENNO, M_AGENLAB, 
                                CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, NHI_PRICE, 
                                DISC_CPRICE, M_CONTPRICE, E_CODATE, CONTRACTAMT, CONTRACTSUM, 
                                TOUCHCASE, BEGINDATE_14, SPDRUG, FASTDRUG, 
                                CREATE_TIME,
                                COMMON, SPMMCODE, UNITRATE ,DISCOUNT_QTY, APPQTY_TIMES ,
                                DISC_COST_UPRICE, ISIV , M_STOREID 
                                )  
                                SELECT  
                                    :MIMASTHIS_SEQ, MMCODE, SYSDATE+1/(24*60*60), CANCEL_ID,
                                    M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, MMNAME_C, 
                                    M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, BASE_UNIT, 
                                    M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, WARBAK, 
                                    ONECOST, HEALTHPAY, COSTKIND, WASTKIND, SPXFEE, 
                                    ORDERKIND, CASEDOCT, DRUGKIND, M_AGENNO, M_AGENLAB, 
                                    CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, NHI_PRICE, 
                                    DISC_CPRICE, M_CONTPRICE, E_CODATE, CONTRACTAMT, CONTRACTSUM, 
                                    TOUCHCASE, BEGINDATE_14, SPDRUG, FASTDRUG, 
                                    SYSDATE, 
                                    COMMON, SPMMCODE, UNITRATE ,DISCOUNT_QTY, APPQTY_TIMES ,
                                    DISC_COST_UPRICE, ISIV, M_STOREID  
                                FROM MI_MAST 
                                WHERE MMCODE = :MMCODE
                                ";
            return DBWork.Connection.Execute(sql, new { MIMASTHIS_SEQ = seq, MMCODE = id }, DBWork.Transaction);
        }
        public int InsertFromXLS(MI_MAST_HISTORY mi_mast)
        {
            var sql = @"INSERT INTO MI_MAST_HISTORY (
                                MIMASTHIS_SEQ, MMCODE, EFFSTARTDATE, CANCEL_ID,
                                M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, MMNAME_C, 
                                M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, BASE_UNIT, 
                                M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, WARBAK, 
                                ONECOST, HEALTHPAY, COSTKIND, WASTKIND, SPXFEE, 
                                ORDERKIND, CASEDOCT, DRUGKIND, COMMON, M_AGENNO, 
                                M_AGENLAB, CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, 
                                NHI_PRICE, DISC_CPRICE, M_CONTPRICE, CONTRACTAMT, CONTRACTSUM, 
                                TOUCHCASE,  SPDRUG, FASTDRUG, SPMMCODE, UNITRATE,
                                E_CODATE, DISCOUNT_QTY, APPQTY_TIMES,
                                DISC_COST_UPRICE, ISIV , M_STOREID ,  
                                CREATE_TIME, CREATE_USER, UPDATE_IP 
                                )  
                                VALUES (
                                :MIMASTHIS_SEQ, :MMCODE, :EFFSTARTDATE, :CANCEL_ID,
                                :M_NHIKEY, :HEALTHOWNEXP, :DRUGSNAME, :MMNAME_E, :MMNAME_C,
                                :M_PHCTNCO, :M_ENVDT, :ISSUESUPPLY, :E_MANUFACT, :BASE_UNIT,
                                :M_PURUN, :TRUTRATE, :MAT_CLASS_SUB, :E_RESTRICTCODE, :WARBAK,
                                :ONECOST, :HEALTHPAY, :COSTKIND, :WASTKIND, :SPXFEE,
                                :ORDERKIND, :CASEDOCT, :DRUGKIND, :COMMON, :M_AGENNO,
                                :M_AGENLAB, :CASENO, :E_SOURCECODE, :M_CONTID, :E_ITEMARMYNO, 
                                :NHI_PRICE, :DISC_CPRICE, :M_CONTPRICE, :CONTRACTAMT, :CONTRACTSUM,
                                :TOUCHCASE, :SPDRUG, :FASTDRUG, :SPMMCODE, :UNITRATE,
                                :E_CODATE, :DISCOUNT_QTY, :APPQTY_TIMES ,
                                :DISC_COST_UPRICE, :ISIV , :M_STOREID , 
                                SYSDATE, :CREATE_USER, :UPDATE_IP 
                                )";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int Update(MI_MAST_HISTORY mi_mast)
        {
            var sql = @"UPDATE MI_MAST_HISTORY SET
                            EFFSTARTDATE = TWN_TODATE(:EFFSTARTDATE),
                            CANCEL_ID = :CANCEL_ID,
                            M_NHIKEY = :M_NHIKEY,
                            HEALTHOWNEXP = :HEALTHOWNEXP,
                            DRUGSNAME = :DRUGSNAME, 
                            MMNAME_E = :MMNAME_E, 
                            MMNAME_C = :MMNAME_C, 
                            M_PHCTNCO = :M_PHCTNCO, 
                            M_ENVDT = :M_ENVDT, 
                            ISSUESUPPLY = :ISSUESUPPLY, 
                            E_MANUFACT = :E_MANUFACT, 
                            BASE_UNIT = :BASE_UNIT, 
                            M_PURUN = :M_PURUN, 
                            UNITRATE = :UNITRATE, 
                            TRUTRATE = :TRUTRATE, 
                            MAT_CLASS_SUB = :MAT_CLASS_SUB, 
                            E_RESTRICTCODE = :E_RESTRICTCODE, 
                            WARBAK = :WARBAK, 
                            ONECOST = :ONECOST, 
                            HEALTHPAY = :HEALTHPAY, 
                            COSTKIND = :COSTKIND, 
                            WASTKIND = :WASTKIND, 
                            SPXFEE = :SPXFEE, 
                            ORDERKIND = :ORDERKIND, 
                            CASEDOCT = :CASEDOCT, 
                            DRUGKIND = :DRUGKIND, 
                            COMMON = :COMMON, 
                            SPMMCODE = :SPMMCODE, 
                            M_AGENNO = :M_AGENNO, 
                            M_AGENLAB = :M_AGENLAB, 
                            CASENO = :CASENO, 
                            E_SOURCECODE = :E_SOURCECODE, 
                            M_CONTID = :M_CONTID, 
                            E_ITEMARMYNO = :E_ITEMARMYNO, 
                            NHI_PRICE = :NHI_PRICE,  
                            DISC_CPRICE = :DISC_CPRICE, 
                            M_CONTPRICE = :M_CONTPRICE, 
                            CONTRACTAMT = :CONTRACTAMT, 
                            CONTRACTSUM = :CONTRACTSUM, 
                            TOUCHCASE = :TOUCHCASE, 
                            SPDRUG = :SPDRUG, 
                            FASTDRUG = :FASTDRUG,
                            DISCOUNT_QTY = :DISCOUNT_QTY,
                            APPQTY_TIMES = :APPQTY_TIMES,
                            DISC_COST_UPRICE = :DISC_COST_UPRICE, 
                            ISIV = :ISIV , 
                            M_STOREID = :M_STOREID,
                            E_CODATE = TWN_TODATE(:E_CODATE),
                            CREATE_TIME = SYSDATE, 
                            CREATE_USER = :CREATE_USER, 
                            UPDATE_IP = :UPDATE_IP 
                        WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ ";

            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int Delete(string seq)
        {
            var sql = @"DELETE MI_MAST_HISTORY WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ ";
            return DBWork.Connection.Execute(sql, new { MIMASTHIS_SEQ = seq }, DBWork.Transaction);
        }

        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST_HISTORY WHERE MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsMast(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }
        public bool CheckEffstratdate(string id)
        {
            string sql = @" SELECT 1 FROM DUAL WHERE TWN_TODATE(:PDATE) > SYSDATE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PDATE = id }, DBWork.Transaction) == null);
        }

        public IEnumerable<MI_MAST> GetMMCodeFromMastHisCombo(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT Distinct a.mmcode,
                                {0}
                                a.mmname_c,
                                a.mmname_e
                            FROM  MI_MAST_HISTORY a, MI_MAST b
                            WHERE a.mmcode = b.mmcode  
                              
                            ";
            if (!string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND b.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", p1);
            }
            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql = string.Format("SELECT * FROM ({0}) TMP WHERE 1=1 ", sql);

                sql += " AND (UPPER(MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

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

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        // 物料分類代碼
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @" SELECT Trim(MAT_CLASS) as VALUE, Trim(MAT_CLSNAME) as TEXT, 
                        Trim(MAT_CLASS) || ' ' || Trim(MAT_CLSNAME) as COMBITEM
                        FROM MI_MATCLASS WHERE 1=1 AND Trim(MAT_CLSID) in ('1', '2')   ORDER BY MAT_CLASS ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        // 物料子類別
        public IEnumerable<COMBO_MODEL> GetMatclassSubCombo()
        {
            string sql = @" SELECT DATA_VALUE as VALUE, DATA_DESC as TEXT, 
                        Trim(DATA_VALUE) || ' ' || Trim(DATA_DESC) as COMBITEM,DATA_SEQ
                        FROM PARAM_D WHERE 1=1 
                        AND GRP_CODE = 'MI_MAST' AND DATA_NAME = 'MAT_CLASS_SUB' ";

            sql += " ORDER BY DATA_SEQ ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        // 藥材單位
        public IEnumerable<COMBO_MODEL> GetBaseunitCombo()
        {
            string sql = @" SELECT Trim(UNIT_CODE) as VALUE, case when Trim(UI_CHANAME) is not null then Trim(UI_CHANAME) else Trim(UI_ENGNAME) end as TEXT, 
                        Trim(UNIT_CODE) || ' ' || (case when Trim(UI_CHANAME) is not null then Trim(UI_CHANAME) else Trim(UI_ENGNAME) end) as COMBITEM
                        FROM MI_UNITCODE
                        ORDER BY UNIT_CODE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetYnCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_VALUE as TEXT ,
                        DATA_VALUE as COMBITEM,DATA_SEQ 
                        FROM PARAM_D
                        WHERE GRP_CODE='Y_OR_N' AND DATA_NAME='YN' 
                        ORDER BY DATA_SEQ ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetRestriCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='E_RESTRICTCODE' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetESourceCodeCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='E_SOURCECODE' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetERestrictCodeCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='E_RESTRICTCODE' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWarbakCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='WARBAK' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetOnecostCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='ONECOST' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetHealthPayCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='HEALTHPAY' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetCostKindCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='COSTKIND' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWastKindCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='WASTKIND' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetSpXfeeCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='SPXFEE' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetOrderKindCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='ORDERKIND' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetDrugKindCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='DRUGKIND' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetSpDrugCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='SPDRUG' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetFastDrugCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='FASTDRUG' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMContidCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='M_CONTID' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetTouchCaseCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='TOUCHCASE' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetCommonCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='COMMON' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> MStoreidComboGet()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='M_STOREID' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<PH_VENDER> GetAgennoCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} AGEN_NO, AGEN_NAMEC, AGEN_NAMEE
                        FROM PH_VENDER WHERE (REC_STATUS<>'X' OR REC_STATUS is null)";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(AGEN_NO, :AGEN_NO_I), 1000) + NVL(INSTR(AGEN_NAMEC, :AGEN_NAMEC_I), 100) * 10 + NVL(INSTR(AGEN_NAMEE, :AGEN_NAMEE_I), 100) * 10) IDX,");
                p.Add(":AGEN_NO_I", p0);
                p.Add(":AGEN_NAMEC_I", p0);
                p.Add(":AGEN_NAMEE_I", p0);

                sql += " AND (AGEN_NO LIKE :AGEN_NO ";
                p.Add(":AGEN_NO", string.Format("{0}%", p0));

                sql += " OR AGEN_NAMEC LIKE :AGEN_NAMEC ";
                p.Add(":AGEN_NAMEC", string.Format("%{0}%", p0));

                sql += " OR AGEN_NAMEE LIKE :AGEN_NAMEE) ";
                p.Add(":AGEN_NAMEE", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY AGEN_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string GetMaxSeq(string mmcode)
        {
            var sql = " SELECT NVL(MAX(MIMASTHIS_SEQ),0) as SEQ FROM MI_MAST_HISTORY WHERE MMCODE = :MMCODE";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction).ToString();
            return rtn;
        }
        public int UpdateEffenddate(string seq, string effenddate)
        {
            var sql = @"UPDATE MI_MAST_HISTORY SET EFFENDDATE = TWN_TODATE(:EFFENDDATE)-1
                                WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ";
            return DBWork.Connection.Execute(sql, new { MIMASTHIS_SEQ = seq, EFFENDDATE = effenddate }, DBWork.Transaction);
        }
        public int UpdateEffenddateN(string seq)
        {
            var sql = @"UPDATE MI_MAST_HISTORY SET EFFENDDATE = NULL
                                WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ";
            return DBWork.Connection.Execute(sql, new { MIMASTHIS_SEQ = seq }, DBWork.Transaction);
        }
        public int UpdateHisECodate(MI_MAST_HISTORY mi_mast)
        {
            var sql = @"UPDATE MI_MAST_HISTORY SET E_CODATE = TWN_TODATE(:E_CODATE)
                        WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ AND MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }
        public int UpdateHisBegindate(MI_MAST_HISTORY mi_mast)
        {
            var sql = @"UPDATE MI_MAST_HISTORY SET BEGINDATE_14 = TWN_TODATE(:BEGINDATE_14)
                        WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ AND MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }
        public int UpdateHisStartdate(MI_MAST_HISTORY mi_mast)
        {
            var sql = @"UPDATE MI_MAST_HISTORY SET EFFSTARTDATE = :EFFSTARTDATE 
                        WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ AND MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }
        public string Getdatetime(string dt)
        {
            string sql = @"SELECT TWN_DATE(:pDate) TWNDATE FROM DUAL ";

            return DBWork.Connection.ExecuteScalar<string>(sql, new { pDate = dt });
        }

        public string GetHisSeq()
        {
            string sql = " SELECT MIMASTHISTORY_SEQ.nextval FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        public bool CheckExistsMMCODE(string id)
        {
            string sql = @" SELECT 1 FROM MI_MAST_HISTORY WHERE 1=1 
                          AND MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsPARAM_D(string name, string value)
        {
            string sql = @" SELECT 1 FROM PARAM_D 
                             WHERE GRP_CODE   = 'MI_MAST' 
                               AND DATA_NAME  = :NAME 
                               AND DATA_VALUE = :VALUE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { NAME = name, VALUE = value }, DBWork.Transaction) == null);
        }
        public bool CheckExistsBaseUint(string id)
        {
            string sql = @" SELECT 1 FROM MI_UNITCODE 
                             WHERE UNIT_CODE = :UNIT  ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { UNIT = id }, DBWork.Transaction) == null);
        }

        public IEnumerable<MI_MAST> GetMiMast(string mmcode) {
            string sql = @"
                select a.*, b.agen_namec from MI_MAST a, PH_VENDER b 
                 where a.mmcode=:mmcode
                   and a.m_agenno= b.agen_no
            ";
            return DBWork.Connection.Query<MI_MAST>(sql, new { mmcode }, DBWork.Transaction);
        }
    }
}