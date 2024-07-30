using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0058Repository : JCLib.Mvc.BaseRepository
    {
        public AA0058Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_MAST> GetAll(string mmcode, string mmname_c, string mmname_e, string mat_class, string p4, string p5, string p6, string p7, string p8, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, 
                        (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as MAT_CLASS, 
                        (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end) from MI_UNITCODE where UNIT_CODE = trim(A.BASE_UNIT)) as BASE_UNIT, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_SUPSTATUS' and DATA_VALUE = A.E_SUPSTATUS) as E_SUPSTATUS, 
                        A.E_MANUFACT, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_IFPUBLIC' and DATA_VALUE = A.E_IFPUBLIC) as E_IFPUBLIC, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_STOCKTYPE' and DATA_VALUE = A.E_STOCKTYPE) as E_STOCKTYPE, 
                        A.E_SPECNUNIT, A.E_COMPUNIT, A.E_YRARMYNO, A.E_ITEMARMYNO, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_GPARMYNO' and DATA_VALUE = A.E_GPARMYNO) as E_GPARMYNO, 
                        A.E_CLFARMYNO, A.E_CODATE, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_PRESCRIPTYPE' and DATA_VALUE = A.E_PRESCRIPTYPE) as E_PRESCRIPTYPE, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_DRUGCLASS' and DATA_VALUE = A.E_DRUGCLASS) as E_DRUGCLASS, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_DRUGCLASSIFY' and DATA_VALUE = A.E_DRUGCLASSIFY) as E_DRUGCLASSIFY, 
                        A.E_DRUGFORM, A.E_COMITMEMO,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_COMITCODE' and DATA_VALUE = A.E_COMITCODE) as E_COMITCODE, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_INVFLAG' and DATA_VALUE = A.E_INVFLAG) as E_INVFLAG, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_PURTYPE' and DATA_VALUE = A.E_PURTYPE) as E_PURTYPE, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_SOURCECODE' and DATA_VALUE = A.E_SOURCECODE) as E_SOURCECODE, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_DRUGAPLTYPE' and DATA_VALUE = A.E_DRUGAPLTYPE) as E_DRUGAPLTYPE, 
                        A.E_ARMYORDCODE, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_PARCODE' and DATA_VALUE = A.E_PARCODE) as E_PARCODE, 
                        A.E_PARORDCODE, A.E_SONTRANSQTY, A.CANCEL_ID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_RESTRICTCODE' and DATA_VALUE = A.E_RESTRICTCODE) as E_RESTRICTCODE, 
                        A.E_ORDERDCFLAG, A.E_HIGHPRICEFLAG, A.E_RETURNDRUGFLAG, A.E_RESEARCHDRUGFLAG, A.E_VACCINE, A.E_TAKEKIND,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'WEXP_ID' and DATA_VALUE = A.WEXP_ID) as WEXP_ID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'WLOC_ID' and DATA_VALUE = A.WLOC_ID) as WLOC_ID,
                        A.E_PATHNO, A.E_ORDERUNIT, A.E_FREQNOO, A.E_FREQNOI,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CONTRACNO' and DATA_VALUE = A.CONTRACNO) as CONTRACNO,
                        A.UPRICE, A.DISC_CPRICE, A.DISC_UPRICE, A.PFILE_ID, A.EASYNAME, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CANCEL_NOTE' and DATA_VALUE = A.CANCEL_NOTE) as CANCEL_NOTE, A.NHI_PRICE,
                        (select EXCH_RATIO from MI_UNITEXCH where MMCODE=A.MMCODE and UNIT_CODE = A.M_PURUN and AGEN_NO = A.M_AGENNO) as EXCH_RATIO,
                        A.M_CONTPRICE, A.M_DISCPERC, A.M_PURUN, A.HOSP_PRICE, 
                        A.M_AGENNO, (select case when AGEN_NAMEC is null then AGEN_NAMEE else AGEN_NAMEC end from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                        A.E_STOCKTRANSQTYI, A.E_SCIENTIFICNAME, A.M_NHIKEY,
                        c.ISWILLING, c.DISCOUNT_QTY, c.DISC_COST_UPRICE,
                        d.SELF_CONTRACT_NO, d.SELF_PUR_UPPER_LIMIT, d.SELF_CONT_BDATE, d.SELF_CONT_EDATE,
                        A.BEGINDATE, A.ENDDATE
                        FROM MI_MAST A 
                        LEFT OUTER JOIN HIS_BASORDM B ON B.ORDERCODE=A.MMCODE 
                        LEFT OUTER JOIN MILMED_JBID_LIST c
                        on substr(a.E_YRARMYNO, 1, 3) = c.JBID_STYR and a.E_ITEMARMYNO = c.BID_NO
                        left outer join MED_SELFPUR_DEF d on (d.mmcode = a.mmcode and (twn_date(sysdate) between d.self_cont_bdate and d.self_cont_edate))
                        WHERE 1=1 ";

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
            sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '1' ";

            if (p4 != "" && p4 != null)
            {
                sql += " AND A.E_ORDERDCFLAG = :p4 ";
                p.Add(":p4", p4);
            }
            if (p5 != "" && p5 != null)
            {
                sql += " AND A.E_RESTRICTCODE = :p5 ";
                p.Add(":p5", p5);
                //if(p5 == "1")
                //{
                //    sql += " AND A.E_RESTRICTCODE IN ('1','2','3') ";
                //}
                //else
                //{

                //}
            }

            if (p6 != "" && p6 != null)
            {
                sql += " AND A.E_RETURNDRUGFLAG = :p6 ";
                p.Add(":p6", p6);
            }
            if (p7 != "" && p7 != null)
            {
                sql += " AND A.E_VACCINE = :p7 ";
                p.Add(":p7", p7);
            }
            if (p8 != "" && p8 != null)
            {
                sql += " AND B.RAREDISORDERFLAG = :p8 ";
                p.Add(":p8", p8);
            }


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> Get(string id)
        {
            var sql = @" SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, 
                        A.BASE_UNIT, A.E_SUPSTATUS, A.E_MANUFACT, A.E_IFPUBLIC, A.E_STOCKTYPE, A.E_SPECNUNIT, A.E_COMPUNIT, 
                        A.E_YRARMYNO, A.E_ITEMARMYNO, A.E_GPARMYNO, A.E_CLFARMYNO, E_CODATE, A.E_PRESCRIPTYPE, A.E_DRUGCLASS, A.E_DRUGCLASSIFY, A.E_DRUGFORM, A.E_COMITMEMO,
                        A.E_COMITCODE, A.E_INVFLAG, A.E_PURTYPE, A.E_SOURCECODE, A.E_DRUGAPLTYPE, A.E_ARMYORDCODE, A.E_PARCODE, A.E_PARORDCODE, A.E_SONTRANSQTY, A.CANCEL_ID, 
                        A.E_RESTRICTCODE, A.E_ORDERDCFLAG, A.E_HIGHPRICEFLAG, A.E_RETURNDRUGFLAG, A.E_RESEARCHDRUGFLAG, A.E_VACCINE, A.E_TAKEKIND, A.WEXP_ID, A.WLOC_ID,
                        A.E_PATHNO, A.E_ORDERUNIT, A.E_FREQNOO, A.E_FREQNOI, A.CONTRACNO, A.UPRICE, A.DISC_CPRICE, A.DISC_UPRICE, A.PFILE_ID, A.EASYNAME, A.CANCEL_NOTE, A.NHI_PRICE, A.M_CONTPRICE, A.M_DISCPERC, A.M_PURUN,
                        (select EXCH_RATIO from MI_UNITEXCH where MMCODE=A.MMCODE and UNIT_CODE = A.M_PURUN and AGEN_NO = A.M_AGENNO) as EXCH_RATIO, A.HOSP_PRICE, 
                        A.M_AGENNO, (select case when AGEN_NAMEC is null then AGEN_NAMEE else AGEN_NAMEC end from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                        A.E_STOCKTRANSQTYI, A.E_SCIENTIFICNAME, A.M_NHIKEY,
                        B.ISWILLING, B.DISCOUNT_QTY, B.DISC_COST_UPRICE,
                        c.SELF_CONTRACT_NO, c.SELF_PUR_UPPER_LIMIT, c.SELF_CONT_BDATE, c.SELF_CONT_EDATE,
                        A.BEGINDATE, A.ENDDATE
                        FROM MI_MAST A 
                        LEFT OUTER JOIN MILMED_JBID_LIST B
                        on substr(a.E_YRARMYNO, 1, 3) = B.JBID_STYR and a.E_ITEMARMYNO = B.BID_NO 
                        left outer join MED_SELFPUR_DEF c on (c.mmcode = a.mmcode and (twn_date(sysdate) between c.self_cont_bdate and c.self_cont_edate))
                        WHERE a.MMCODE=:MMCODE ";
            return DBWork.Connection.Query<MI_MAST>(sql, new { MMCODE = id }, DBWork.Transaction);
        }

        public DataTable GetExcel(string mmcode, string mmname_c, string mmname_e, string mat_class)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @" SELECT A.MMCODE as 院內碼, A.MMNAME_C as 中文品名, A.MMNAME_E as 英文品名, A.EASYNAME as 院內簡稱,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CANCEL_NOTE' and DATA_VALUE = A.CANCEL_NOTE) as 作廢備註,
                        (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as 物料分類代碼, 
                        (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end) from MI_UNITCODE where UNIT_CODE = trim(A.BASE_UNIT)) as 計量單位代碼, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_SUPSTATUS' and DATA_VALUE = A.E_SUPSTATUS) as 廠商供應狀態, 
                        A.E_MANUFACT as 原製造商, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_IFPUBLIC' and DATA_VALUE = A.E_IFPUBLIC) as 是否公藥, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_STOCKTYPE' and DATA_VALUE = A.E_STOCKTYPE) as 扣庫規則分類, 
                        A.E_SPECNUNIT as 劑量, A.E_COMPUNIT as 成份, A.E_YRARMYNO as 軍聯項次年號, A.E_ITEMARMYNO as 軍聯項次號, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_GPARMYNO' and DATA_VALUE = A.E_GPARMYNO) as 軍聯項次分類, 
                        A.E_CLFARMYNO as 軍聯項次組別, A.E_CODATE as 合約效期, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_PRESCRIPTYPE' and DATA_VALUE = A.E_PRESCRIPTYPE) as 藥品單複方, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_DRUGCLASS' and DATA_VALUE = A.E_DRUGCLASS) as 用藥類別, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_DRUGCLASSIFY' and DATA_VALUE = A.E_DRUGCLASSIFY) as 藥品性質, 
                        A.E_DRUGFORM as 藥品劑型, A.E_COMITMEMO as 藥委會註記,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_COMITCODE' and DATA_VALUE = A.E_COMITCODE) as 藥委會品項, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_INVFLAG' and DATA_VALUE = A.E_INVFLAG) as 是否盤點品項, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_PURTYPE' and DATA_VALUE = A.E_PURTYPE) as 藥品採購案別, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_SOURCECODE' and DATA_VALUE = A.E_SOURCECODE) as 來源代碼, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_DRUGAPLTYPE' and DATA_VALUE = A.E_DRUGAPLTYPE) as 藥品請領類別, 
                        A.E_ARMYORDCODE as 軍品院內碼, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_PARCODE' and DATA_VALUE = A.E_PARCODE) as 母藥註記, 
                        A.E_PARORDCODE as 母藥院內碼, A.E_SONTRANSQTY as 子藥轉換量, A.CANCEL_ID as 是否作廢, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_RESTRICTCODE' and DATA_VALUE = A.E_RESTRICTCODE) as 管制用藥, 
                        A.E_ORDERDCFLAG as 停用碼, A.E_HIGHPRICEFLAG as 高價用藥, A.E_RETURNDRUGFLAG as 合理回流藥, A.E_RESEARCHDRUGFLAG as 研究用藥, A.E_VACCINE as 疫苗, A.E_TAKEKIND as 服用藥別,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'WEXP_ID' and DATA_VALUE = A.WEXP_ID) as 批號效期註記, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'WLOC_ID' and DATA_VALUE = A.WLOC_ID) as 儲位記錄註記,
                        A.E_PATHNO as 院內給藥途徑部位代碼, A.E_ORDERUNIT as 醫囑單位, A.E_FREQNOO as 門診給藥頻率, A.E_FREQNOI as 住院給藥頻率,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CONTRACNO' and DATA_VALUE = A.CONTRACNO) as 合約碼,
                        M_AGENNO as 廠商代碼, (select case when AGEN_NAMEC is null then AGEN_NAMEE else AGEN_NAMEC end from PH_VENDER where AGEN_NO = A.M_AGENNO) as 廠商名稱,
                        A.M_CONTPRICE as 合約單價, A.M_AGENNO as 廠商代碼, A.UPRICE as 最小單價, A.DISC_CPRICE as 優惠合約單價, A.M_DISCPERC as 折讓比, A.DISC_UPRICE as 優惠最小單價, 
                        (select EXCH_RATIO from MI_UNITEXCH where MMCODE=A.MMCODE and UNIT_CODE = A.M_PURUN and AGEN_NO = A.M_AGENNO) as 廠商包裝轉換率,
                        A.NHI_PRICE as 健保給付價, A.HOSP_PRICE as 自費價, 
                        A.BEGINDATE as 生效起日, A.ENDDATE as 生效迄日, A.E_STOCKTRANSQTYI as 住院扣庫轉換量, A.E_SCIENTIFICNAME as 成份名稱, A.M_NHIKEY as 健保碼,
                        B.ISWILLING as 單次訂購達優惠數量折讓意願, B.DISCOUNT_QTY as 單次採購優惠數量, B.DISC_COST_UPRICE as 單次訂購達優惠數量成本價
                        FROM MI_MAST A  
                        LEFT OUTER JOIN MILMED_JBID_LIST B
                        on substr(a.E_YRARMYNO, 1, 3) = B.JBID_STYR and a.E_ITEMARMYNO = B.BID_NO 
                        WHERE 1=1 ";

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
            sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '1' ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public int Create(MI_MAST mi_mast)
        {
            var sql = @"INSERT INTO MI_MAST (MMCODE, MMNAME_C, MMNAME_E, MAT_CLASS, BASE_UNIT, E_SUPSTATUS, E_MANUFACT, E_IFPUBLIC, E_STOCKTYPE, E_SPECNUNIT, E_COMPUNIT, E_YRARMYNO, E_ITEMARMYNO, E_GPARMYNO, E_CLFARMYNO, E_CODATE,
                                E_PRESCRIPTYPE, E_DRUGCLASS, E_DRUGCLASSIFY, E_DRUGFORM, E_COMITMEMO, E_COMITCODE, E_INVFLAG, E_PURTYPE, E_SOURCECODE, E_DRUGAPLTYPE, E_ARMYORDCODE, E_PARCODE, E_PARORDCODE, E_SONTRANSQTY, 
                                WEXP_ID, WLOC_ID, CANCEL_ID, E_RESTRICTCODE, E_ORDERDCFLAG, E_HIGHPRICEFLAG, E_RETURNDRUGFLAG, E_RESEARCHDRUGFLAG, E_VACCINE, E_TAKEKIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, M_STOREID, M_CONTID, M_AGENNO, 
                                E_PATHNO, E_ORDERUNIT, E_FREQNOO, E_FREQNOI, CONTRACNO, UPRICE, DISC_CPRICE, DISC_UPRICE, PFILE_ID, EASYNAME, CANCEL_NOTE, NHI_PRICE, M_CONTPRICE, M_DISCPERC, M_PURUN, M_NHIKEY, 
                                SELF_BID_SOURCE, SELF_CONTRACT_NO, SELF_PUR_UPPER_LIMIT, SELF_CONT_BDATE, SELF_CONT_EDATE)  
                                VALUES (:MMCODE, :MMNAME_C, :MMNAME_E, :MAT_CLASS, :BASE_UNIT, :E_SUPSTATUS, :E_MANUFACT, :E_IFPUBLIC, :E_STOCKTYPE, :E_SPECNUNIT, :E_COMPUNIT, :E_YRARMYNO, :E_ITEMARMYNO, :E_GPARMYNO, :E_CLFARMYNO, to_date(to_char(:E_CODATE, 'yyyy/mm/dd'), 'yyyy/mm/dd'),
                                :E_PRESCRIPTYPE, :E_DRUGCLASS, :E_DRUGCLASSIFY, :E_DRUGFORM, :E_COMITMEMO, :E_COMITCODE, :E_INVFLAG, :E_PURTYPE, :E_SOURCECODE, :E_DRUGAPLTYPE, :E_ARMYORDCODE, :E_PARCODE, :E_PARORDCODE, :E_SONTRANSQTY,
                                :WEXP_ID, :WLOC_ID, :CANCEL_ID, :E_RESTRICTCODE, :E_ORDERDCFLAG, :E_HIGHPRICEFLAG, :E_RETURNDRUGFLAG, :E_RESEARCHDRUGFLAG, :E_VACCINE, :E_TAKEKIND, 
                                SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, '1', '0', '0', :E_PATHNO, :E_ORDERUNIT, :E_FREQNOO, :E_FREQNOI, :CONTRACNO, :UPRICE, :DISC_CPRICE, :DISC_UPRICE, :PFILE_ID, :EASYNAME, :CANCEL_NOTE, :NHI_PRICE, :M_CONTPRICE, :M_DISCPERC, :M_PURUN, :M_NHIKEY,
                                )";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int Update(MI_MAST mi_mast)
        {
            var sql = @"UPDATE MI_MAST SET MMNAME_C = :MMNAME_C, MMNAME_E = :MMNAME_E, MAT_CLASS = :MAT_CLASS, BASE_UNIT = :BASE_UNIT, E_SUPSTATUS = :E_SUPSTATUS, E_MANUFACT = :E_MANUFACT, E_IFPUBLIC = :E_IFPUBLIC, E_STOCKTYPE = :E_STOCKTYPE, 
                                E_SPECNUNIT = :E_SPECNUNIT, E_COMPUNIT = :E_COMPUNIT, E_YRARMYNO = :E_YRARMYNO, E_ITEMARMYNO = :E_ITEMARMYNO, E_GPARMYNO = :E_GPARMYNO, E_CLFARMYNO = :E_CLFARMYNO, E_CODATE = to_date(to_char(:E_CODATE, 'yyyy/mm/dd'), 'yyyy/mm/dd'),
                                E_PRESCRIPTYPE = :E_PRESCRIPTYPE, E_DRUGCLASS = :E_DRUGCLASS, E_DRUGCLASSIFY = :E_DRUGCLASSIFY, E_DRUGFORM = :E_DRUGFORM, E_COMITMEMO = :E_COMITMEMO, E_COMITCODE = :E_COMITCODE, E_INVFLAG = :E_INVFLAG, 
                                E_PURTYPE = :E_PURTYPE, E_SOURCECODE = :E_SOURCECODE, E_DRUGAPLTYPE = :E_DRUGAPLTYPE, E_ARMYORDCODE = :E_ARMYORDCODE, E_PARCODE = :E_PARCODE, E_PARORDCODE = :E_PARORDCODE, E_SONTRANSQTY = :E_SONTRANSQTY, 
                                WEXP_ID = :WEXP_ID, WLOC_ID = :WLOC_ID, CANCEL_ID = :CANCEL_ID, E_RESTRICTCODE = :E_RESTRICTCODE, E_ORDERDCFLAG = :E_ORDERDCFLAG, E_HIGHPRICEFLAG = :E_HIGHPRICEFLAG, E_RETURNDRUGFLAG = :E_RETURNDRUGFLAG, E_RESEARCHDRUGFLAG = :E_RESEARCHDRUGFLAG, E_VACCINE = :E_VACCINE, E_TAKEKIND = :E_TAKEKIND, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP, E_PATHNO = :E_PATHNO, E_ORDERUNIT = :E_ORDERUNIT, E_FREQNOO = :E_FREQNOO, E_FREQNOI = :E_FREQNOI, CONTRACNO = :CONTRACNO, UPRICE = :UPRICE, DISC_CPRICE = :DISC_CPRICE, DISC_UPRICE = :DISC_UPRICE, PFILE_ID = :PFILE_ID, EASYNAME = :EASYNAME, CANCEL_NOTE = :CANCEL_NOTE, NHI_PRICE = :NHI_PRICE,
                                M_CONTPRICE = :M_CONTPRICE, M_DISCPERC = :M_DISCPERC, M_AGENNO = :M_AGENNO, M_PURUN = :M_PURUN, M_NHIKEY = :M_NHIKEY
                                WHERE MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }
    }
}