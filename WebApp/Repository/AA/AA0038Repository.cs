using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0038Repository : JCLib.Mvc.BaseRepository
    {
        public AA0038Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_MAST> GetAll(string mmcode, string mmname_c, string mmname_e, string mat_class, string fid, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, 
                        (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as MAT_CLASS, 
                        (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end) from MI_UNITCODE where UNIT_CODE = trim(A.BASE_UNIT)) as BASE_UNIT, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_STOREID' and DATA_VALUE = A.M_STOREID) as M_STOREID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_CONTID' and DATA_VALUE = A.M_CONTID) as M_CONTID, 
                        A.M_IDKEY, A.M_INVKEY, A.M_NHIKEY, A.M_GOVKEY, 
                        A.M_VOLL, A.M_VOLW, A.M_VOLH, A.M_VOLC, A.M_SWAP, A.M_MATID, A.M_SUPPLYID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_CONSUMID' and DATA_VALUE = A.M_CONSUMID) as M_CONSUMID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_PAYKIND' and DATA_VALUE = A.M_PAYKIND) as M_PAYKIND, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_PAYID' and DATA_VALUE = A.M_PAYID) as M_PAYID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_TRNID' and DATA_VALUE = A.M_TRNID) as M_TRNID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_APPLYID' and DATA_VALUE = A.M_APPLYID) as M_APPLYID, 
                        A.M_PHCTNCO, A.M_ENVDT, A.M_AGENNO, A.M_AGENLAB, 
                        (select case when AGEN_NAMEC is null then AGEN_NAMEE else AGEN_NAMEC end from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                        (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end) from MI_UNITCODE where UNIT_CODE = trim(A.M_PURUN)) as M_PURUN, 
                        A.M_CONTPRICE, A.M_DISCPERC, A.CANCEL_ID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'WEXP_ID' and DATA_VALUE = A.WEXP_ID) as WEXP_ID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'WLOC_ID' and DATA_VALUE = A.WLOC_ID) as WLOC_ID,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CONTRACNO' and DATA_VALUE = A.CONTRACNO) as CONTRACNO,
                        A.UPRICE, A.DISC_CPRICE, A.DISC_UPRICE, A.PFILE_ID, A.EASYNAME, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CANCEL_NOTE' and DATA_VALUE = A.CANCEL_NOTE) as CANCEL_NOTE, A.NHI_PRICE,
                        A.E_ITEMARMYNO, A.E_YRARMYNO, A.E_CLFARMYNO, A.HOSP_PRICE, 
                        (select MIN_ORDQTY from MI_WINVCTL where WH_NO='560000' AND MMCODE=A.MMCODE) as MIN_ORDQTY,
                        (select EXCH_RATIO from MI_UNITEXCH where MMCODE=A.MMCODE and UNIT_CODE = A.M_PURUN and AGEN_NO = A.M_AGENNO) as EXCH_RATIO,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_SOURCECODE' and DATA_VALUE = A.E_SOURCECODE) as E_SOURCECODE,
                        A.BEGINDATE, A.ENDDATE
                        FROM MI_MAST A WHERE 1=1 ";

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

            if (fid == "E")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '1' ";
            if (fid == "M")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) in ('2', '3') ";
            else if (fid == "ED")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '4' ";
            else if (fid == "CN")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '5' ";
            else if (fid == "AR")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '6' ";
            else if (fid == "M+AR")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) in ('2', '3', '6') ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> Get(string id)
        {
            var sql = @" SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, 
                        A.BASE_UNIT, A.M_STOREID, A.M_CONTID, A.M_IDKEY, A.M_INVKEY, A.M_NHIKEY, A.M_GOVKEY, 
                        A.M_VOLL, A.M_VOLW, A.M_VOLH, A.M_VOLC, A.M_SWAP, A.M_MATID, A.M_SUPPLYID, A.M_CONSUMID, A.M_PAYKIND, A.M_PAYID,
                        A.M_TRNID, A.M_APPLYID, A.M_PHCTNCO, A.M_ENVDT, A.M_AGENNO, A.M_AGENLAB, A.M_PURUN, A.M_CONTPRICE, A.M_DISCPERC, A.CANCEL_ID, A.WEXP_ID, A.WLOC_ID,
                        A.CONTRACNO, A.UPRICE, A.DISC_CPRICE, A.DISC_UPRICE, A.PFILE_ID, A.EASYNAME, A.CANCEL_NOTE, A.NHI_PRICE, A.E_ITEMARMYNO, A.E_YRARMYNO, A.E_CLFARMYNO, A.HOSP_PRICE, 
                        (select MIN_ORDQTY from MI_WINVCTL where WH_NO='560000' AND MMCODE=A.MMCODE) as MIN_ORDQTY,
                        (select EXCH_RATIO from MI_UNITEXCH where MMCODE=A.MMCODE and UNIT_CODE = A.M_PURUN and AGEN_NO = A.M_AGENNO) as EXCH_RATIO,
                        (select case when AGEN_NAMEC is null then AGEN_NAMEE else AGEN_NAMEC end from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                        (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) as MAT_CLSID,
                        A.E_SOURCECODE,
                        A.BEGINDATE, A.ENDDATE
                        FROM MI_MAST A WHERE MMCODE=:MMCODE ";
            return DBWork.Connection.Query<MI_MAST>(sql, new { MMCODE = id }, DBWork.Transaction);
        }

        public DataTable GetExcel(string mmcode, string mmname_c, string mmname_e, string mat_class, string fid)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @" SELECT A.MMCODE as 院內碼, A.MMNAME_C as 中文品名, A.MMNAME_E as 英文品名, A.EASYNAME as 院內簡稱,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CANCEL_NOTE' and DATA_VALUE = A.CANCEL_NOTE) as 作廢備註,
                        (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as 物料分類代碼, 
                        (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end) from MI_UNITCODE where UNIT_CODE = trim(A.BASE_UNIT)) as 計量單位, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_STOREID' and DATA_VALUE = A.M_STOREID) as 庫備識別碼, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_CONTID' and DATA_VALUE = A.M_CONTID) as 合約識別碼, 
                        A.M_IDKEY as ID碼, A.M_INVKEY as 衛材料號碼, A.M_NHIKEY as 健保碼, A.M_GOVKEY as 行政院碼, 
                        A.M_VOLL as 長度CM, A.M_VOLW as 寬度CM, A.M_VOLH as 高度CM, A.M_VOLC as 圓周, A.M_SWAP as 材積轉換率, A.M_MATID as 是否聯標, A.M_SUPPLYID as 是否供應契約, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_CONSUMID' and DATA_VALUE = A.M_CONSUMID) as 消耗屬性, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_PAYKIND' and DATA_VALUE = A.M_PAYKIND) as 給付類別, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_PAYID' and DATA_VALUE = A.M_PAYID) as 計費方式, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_TRNID' and DATA_VALUE = A.M_TRNID) as 扣庫方式, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_APPLYID' and DATA_VALUE = A.M_APPLYID) as 申請申購識別碼, 
                        A.M_PHCTNCO as 環保或衛署許可證, A.M_ENVDT as 環保證號效期, A.M_AGENNO as 廠商代碼, A.M_AGENLAB as 廠牌, 
                        (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end) from MI_UNITCODE where UNIT_CODE = trim(A.M_PURUN)) as 申購計量單位, 
                        A.M_CONTPRICE as 合約單價, A.M_DISCPERC as 折讓比, A.CANCEL_ID as 是否作廢, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'WEXP_ID' and DATA_VALUE = A.WEXP_ID) as 批號效期註記, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'WLOC_ID' and DATA_VALUE = A.WLOC_ID) as 儲位記錄註記,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CONTRACNO' and DATA_VALUE = A.CONTRACNO) as 合約碼,
                        A.UPRICE as 最小單價, A.DISC_CPRICE as 優惠合約單價, A.DISC_UPRICE as 優惠最小單價, A.NHI_PRICE as 健保給付價,
                        A.E_ITEMARMYNO as 聯標項次院辦案號, A.E_YRARMYNO as 合約年度, A.E_CLFARMYNO as 競標組別, 
                        (select MIN_ORDQTY from MI_WINVCTL where WH_NO='560000' AND MMCODE=A.MMCODE) as 最小撥補量,
                        (select EXCH_RATIO from MI_UNITEXCH where MMCODE=A.MMCODE and UNIT_CODE = A.M_PURUN and AGEN_NO = A.M_AGENNO) as 廠商包裝轉換率, A.HOSP_PRICE as 自費價,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_SOURCECODE' and DATA_VALUE = A.E_SOURCECODE) as 來源代碼,
                        A.BEGINDATE as 生效起日, A.ENDDATE as 生效迄日
                        FROM MI_MAST A WHERE 1=1 ";

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

            if (fid == "E")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '1' ";
            if (fid == "M")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) in ('2', '3') ";
            else if (fid == "ED")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '4' ";
            else if (fid == "CN")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '5' ";
            else if (fid == "AR")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '6' ";
            else if (fid == "M+AR")
                sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) in ('2', '3', '6') ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public int Create(MI_MAST mi_mast)
        {
            var sql = @"INSERT INTO MI_MAST (MMCODE, MMNAME_C, MMNAME_E, MAT_CLASS, BASE_UNIT, M_STOREID, M_CONTID, M_IDKEY, M_INVKEY, M_NHIKEY, M_GOVKEY, M_VOLL, M_VOLW, M_VOLH, M_VOLC, M_SWAP,
                                M_MATID, M_SUPPLYID, M_CONSUMID, M_PAYKIND, M_PAYID, M_TRNID, M_APPLYID, M_PHCTNCO, M_ENVDT, M_AGENNO, M_AGENLAB, M_PURUN, M_CONTPRICE, M_DISCPERC, 
                                WEXP_ID, WLOC_ID, CANCEL_ID, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, E_INVFLAG, E_PURTYPE, E_SOURCECODE, E_PARCODE, E_SONTRANSQTY, CONTRACNO, UPRICE, DISC_CPRICE, DISC_UPRICE, PFILE_ID, EASYNAME, CANCEL_NOTE, NHI_PRICE)  
                                VALUES (:MMCODE, :MMNAME_C, :MMNAME_E, :MAT_CLASS, :BASE_UNIT, :M_STOREID, :M_CONTID, :M_IDKEY, :M_INVKEY, :M_NHIKEY, :M_GOVKEY, :M_VOLL, :M_VOLW, :M_VOLH, :M_VOLC, :M_SWAP,
                                :M_MATID, :M_SUPPLYID, :M_CONSUMID, :M_PAYKIND, :M_PAYID, :M_TRNID, :M_APPLYID, :M_PHCTNCO, :M_ENVDT, :M_AGENNO, :M_AGENLAB, :M_PURUN, :M_CONTPRICE, :M_DISCPERC, 
                                :WEXP_ID, :WLOC_ID, :CANCEL_ID, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, '0', '0', '0', '0', 0, :CONTRACNO, :UPRICE, :DISC_CPRICE, :DISC_UPRICE, :PFILE_ID, :EASYNAME, :CANCEL_NOTE, :NHI_PRICE)";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int InsertMinOrdQty(MI_MAST mi_mast)
        {
            var sql = @"INSERT INTO MI_WINVCTL (WH_NO, MMCODE, SAFE_DAY, OPER_DAY, SHIP_DAY, HIGH_QTY, MIN_ORDQTY)  
                                select WH_NO, :MMCODE, '15', '15', decode(WH_GRADE, '1', 15, 0), '0', :MIN_ORDQTY
                                from MI_WHMAST A
                                where WH_KIND = '1' and WH_GRADE in ('1', '2') and nvl(CANCEL_ID, 'N') = 'N'
                                and not exists (select 1 from MI_WINVCTL where WH_NO = A.WH_NO and MMCODE = :MMCODE) ";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int InsertExchRatio(MI_MAST mi_mast)
        {
            var sql = @"INSERT INTO MI_UNITEXCH (MMCODE,UNIT_CODE,AGEN_NO,EXCH_RATIO)  
                                VALUES (:MMCODE, :M_PURUN, :M_AGENNO, :EXCH_RATIO) ";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int Update(MI_MAST mi_mast)
        {
            var sql = @"UPDATE MI_MAST SET MMNAME_C = :MMNAME_C, MMNAME_E = :MMNAME_E, MAT_CLASS = :MAT_CLASS, BASE_UNIT = :BASE_UNIT, M_STOREID = :M_STOREID, M_CONTID = :M_CONTID, M_IDKEY = :M_IDKEY, M_INVKEY = :M_INVKEY, 
                                M_NHIKEY = :M_NHIKEY, M_GOVKEY = :M_GOVKEY, M_VOLL = :M_VOLL, M_VOLW = :M_VOLW, M_VOLH = :M_VOLH, M_VOLC = :M_VOLC, M_SWAP = :M_SWAP,
                                M_MATID = :M_MATID, M_SUPPLYID = :M_SUPPLYID, M_CONSUMID = :M_CONSUMID, M_PAYKIND = :M_PAYKIND, M_PAYID = :M_PAYID, M_TRNID = :M_TRNID, M_APPLYID = :M_APPLYID, M_PHCTNCO = :M_PHCTNCO, M_ENVDT = :M_ENVDT,
                                M_AGENNO = :M_AGENNO, M_AGENLAB = :M_AGENLAB, M_PURUN = :M_PURUN, M_CONTPRICE = :M_CONTPRICE, M_DISCPERC = :M_DISCPERC, 
                                WEXP_ID = :WEXP_ID, WLOC_ID = :WLOC_ID, CANCEL_ID = :CANCEL_ID, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP, CONTRACNO = :CONTRACNO, UPRICE = :UPRICE, DISC_CPRICE = :DISC_CPRICE, DISC_UPRICE = :DISC_UPRICE, PFILE_ID = :PFILE_ID, EASYNAME = :EASYNAME, CANCEL_NOTE = :CANCEL_NOTE, NHI_PRICE = :NHI_PRICE
                                WHERE MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int UpdateMinOrdQty(MI_MAST mi_mast)
        {
            var sql = @"UPDATE MI_WINVCTL A SET MIN_ORDQTY = :MIN_ORDQTY
                                WHERE MMCODE = :MMCODE
                                and EXISTS (SELECT 1 FROM MI_WHMAST WHERE WH_NO=A.WH_NO and WH_KIND='1' and WH_GRADE in ('1', '2')) ";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int UpdateExchRatio(MI_MAST mi_mast)
        {
            var sql = @"UPDATE MI_UNITEXCH SET EXCH_RATIO = :EXCH_RATIO
                                WHERE MMCODE = :MMCODE and UNIT_CODE = :M_PURUN and AGEN_NO = :M_AGENNO ";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        // 院內碼(以中文名稱為優先,若為null則使用英文名稱)
        public IEnumerable<COMBO_MODEL> GetMmcodeCombo()
        {
            string sql = @" SELECT Trim(MMCODE) as VALUE, case when Trim(MMNAME_C) is not null then Trim(MMNAME_C) else Trim(MMNAME_E) end as TEXT, 
                        Trim(MMCODE) || ' ' || (case when Trim(MMNAME_C) is not null then Trim(MMNAME_C) else Trim(MMNAME_E) end) as COMBITEM
                        FROM MI_MAST
                        ORDER BY MMCODE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        // 物料分類代碼
        public IEnumerable<COMBO_MODEL> GetMatclassCombo(string mat_classTyp, string vtype)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT Trim(MAT_CLASS) as VALUE, Trim(MAT_CLSNAME) as TEXT, 
                        Trim(MAT_CLASS) || ' ' || Trim(MAT_CLSNAME) as COMBITEM
                        FROM MI_MATCLASS WHERE 1=1 ";
                        
            if (mat_classTyp != "" && mat_classTyp != null)
            {
                if (mat_classTyp == "E")
                    sql += " AND Trim(MAT_CLSID) = '1' ";
                else if (mat_classTyp == "M")
                    sql += " AND Trim(MAT_CLSID) in ('2', '3') ";
                else if (mat_classTyp == "ED")
                    sql += " AND Trim(MAT_CLSID) = '4' ";
                else if (mat_classTyp == "CN")
                    sql += " AND Trim(MAT_CLSID) = '5' ";
                else if (mat_classTyp == "AR")
                    sql += " AND Trim(MAT_CLSID) = '6' ";
                else if (mat_classTyp == "M+AR")
                    sql += " AND Trim(MAT_CLSID) in ('2', '3', '6') ";
            }
            if (vtype != "" && vtype != null)
            {
                if (vtype == "I")
                    sql += " AND Trim(MAT_CLASS) <> '02' ";
            } 

            sql += " ORDER BY MAT_CLASS ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }

        // 計量單位代碼
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
                        DATA_VALUE as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='Y_OR_N' AND DATA_NAME='YN' 
                        ORDER BY DATA_VALUE DESC ";

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

        public string GetMatClsid(string mat_class)
        {
            var p = new DynamicParameters();
            var sql = @" select MAT_CLSID from MI_MATCLASS 
                        where MAT_CLASS = :p0 ";

            p.Add(":p0", mat_class);

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
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

        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public bool CheckMmcodeRef(string id)
        {
            string sql = @"SELECT 1 FROM dual 
                        WHERE (select count(*) from MI_WHINV where MMCODE=:MMCODE and INV_QTY > 0) > 0
                        or (select count(*) from MI_WLOCINV where MMCODE=:MMCODE and INV_QTY > 0) > 0
                        or (select count(*) from MI_WEXPINV where MMCODE=:MMCODE and INV_QTY > 0) > 0 ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public int ChkIsAdmg(string tuser)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from UR_UIR 
                        where TUSER = :p0
                        and RLNO = 'ADMG' ";

            p.Add(":p0", tuser);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public int ChkMinOrdQty(string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from MI_WINVCTL 
                        where MMCODE = :mmcode ";

            p.Add(":mmcode", mmcode);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public int ChkExchRatio(string mmcode, string m_purun, string m_agenno)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from MI_UNITEXCH 
                        where MMCODE = :mmcode and UNIT_CODE = :m_purun and AGEN_NO = :m_agenno ";

            p.Add(":mmcode", mmcode);
            p.Add(":m_purun", m_purun);
            p.Add(":m_agenno", m_agenno);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetFormEditable(string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT DATA_VALUE as VALUE
                        FROM PARAM_D where GRP_CODE = 'MI_MAST_MODCOL'
                        and (select count(*) from HIS_BASORDM where ORDERCODE = :MMCODE) > 0 ";

            p.Add(":MMCODE", mmcode);
            
            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
    }
}