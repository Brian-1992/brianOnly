using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0126Repository : JCLib.Mvc.BaseRepository
    {
        public AA0126Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_MAST> GetAll(string mmcode, string mmname_c, string mmname_e, string mat_class, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, 
                        (select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = (select MAT_CLASS from MI_MAST where MMCODE=A.MMCODE)) as MAT_CLASS, 
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
                        A.E_PARORDCODE, A.E_SONTRANSQTY, 
                        A.M_AGENNO, A.M_AGENLAB, 
                        (select case when AGEN_NAMEC is null then AGEN_NAMEE else AGEN_NAMEC end from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                        (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end) from MI_UNITCODE where UNIT_CODE = A.M_PURUN) as M_PURUN, 
                        A.M_CONTPRICE, A.M_DISCPERC, A.CANCEL_ID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_RESTRICTCODE' and DATA_VALUE = A.E_RESTRICTCODE) as E_RESTRICTCODE, 
                        A.E_ORDERDCFLAG, A.E_HIGHPRICEFLAG, A.E_RETURNDRUGFLAG, A.E_RESEARCHDRUGFLAG, A.E_VACCINE, A.E_TAKEKIND,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'WEXP_ID' and DATA_VALUE = A.WEXP_ID) as WEXP_ID, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'WLOC_ID' and DATA_VALUE = A.WLOC_ID) as WLOC_ID,
                        A.E_PATHNO, A.E_ORDERUNIT, A.E_FREQNOO, A.E_FREQNOI,
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CONTRACNO' and DATA_VALUE = A.CONTRACNO) as CONTRACNO,
                        A.UPRICE, A.DISC_CPRICE, A.DISC_UPRICE, A.EASYNAME, 
                        (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'CANCEL_NOTE' and DATA_VALUE = A.CANCEL_NOTE) as CANCEL_NOTE, A.NHI_PRICE,
                        (select HOSP_PRICE from MI_MAST where MMCODE = A.MMCODE) as HOSP_PRICE,
                        A.BEGINDATE, A.ENDDATE, EXCH_RATIO, MIN_ORDQTY, E_STOCKTRANSQTYI, E_SCIENTIFICNAME,
                        a.self_bid_source, a.self_contract_no, a.self_pur_upper_limit, a.self_cont_bdate, a.self_cont_edate
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
            //if (mat_class != "" && mat_class != null)
            //{
            //    sql += " AND Trim(A.MAT_CLASS) = :p3 ";
            //    p.Add(":p3", mat_class);
            //}
            // if () 可能需要加上權限判斷可顯示全部者
            //sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '1' "; 
            sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim((select MAT_CLASS from MI_MAST where MMCODE = A.MMCODE))) = '1' ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> Get(string id, string loadType)
        {
            string qTable = "MI_MAST_N";
            if (loadType == "O")
                qTable = "MI_MAST";
            var sql = @" SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, (select MAT_CLASS from MI_MAST where MMCODE=A.MMCODE) as MAT_CLASS, 
                        A.BASE_UNIT, A.E_SUPSTATUS, A.E_MANUFACT, A.E_IFPUBLIC, A.E_STOCKTYPE, A.E_SPECNUNIT, A.E_COMPUNIT, 
                        A.E_YRARMYNO, A.E_ITEMARMYNO, A.E_GPARMYNO, A.E_CLFARMYNO, E_CODATE, A.E_PRESCRIPTYPE, A.E_DRUGCLASS, A.E_DRUGCLASSIFY, A.E_DRUGFORM, A.E_COMITMEMO,
                        A.E_COMITCODE, A.E_INVFLAG, A.E_PURTYPE, A.E_SOURCECODE, A.E_DRUGAPLTYPE, A.E_ARMYORDCODE, A.E_PARCODE, A.E_PARORDCODE, A.E_SONTRANSQTY, A.CANCEL_ID, 
                        A.E_RESTRICTCODE, A.E_ORDERDCFLAG, A.E_HIGHPRICEFLAG, A.E_RETURNDRUGFLAG, A.E_RESEARCHDRUGFLAG, A.E_VACCINE, A.E_TAKEKIND, A.WEXP_ID, A.WLOC_ID,
                        A.E_PATHNO, A.E_ORDERUNIT, A.E_FREQNOO, A.E_FREQNOI, A.CONTRACNO, A.UPRICE, A.DISC_CPRICE, A.DISC_UPRICE, A.EASYNAME, A.CANCEL_NOTE, A.NHI_PRICE, A.M_CONTPRICE, A.M_DISCPERC, A.M_PURUN,
                        A.M_AGENNO, (select case when AGEN_NAMEC is null then AGEN_NAMEE else AGEN_NAMEC end from PH_VENDER where AGEN_NO = A.M_AGENNO) as AGEN_NAMEC,
                        (select HOSP_PRICE from MI_MAST where MMCODE = A.MMCODE) as HOSP_PRICE,
                        A.BEGINDATE, A.ENDDATE, EXCH_RATIO, MIN_ORDQTY, E_STOCKTRANSQTYI, E_SCIENTIFICNAME,
                        a.self_bid_source, a.self_contract_no, a.self_pur_upper_limit, a.self_cont_bdate, a.self_cont_edate
                        FROM " + qTable + @" A WHERE MMCODE=:MMCODE ";
            return DBWork.Connection.Query<MI_MAST>(sql, new { MMCODE = id }, DBWork.Transaction);
        }

        public int Create(MI_MAST mi_mast)
        {
            var sql = @"INSERT INTO MI_MAST_N (MMCODE, MMNAME_C, MMNAME_E, MAT_CLASS, BASE_UNIT, E_SUPSTATUS, E_MANUFACT, E_IFPUBLIC, E_STOCKTYPE, E_SPECNUNIT, E_COMPUNIT, E_YRARMYNO, E_ITEMARMYNO, E_GPARMYNO, E_CLFARMYNO, E_CODATE,
                                E_PRESCRIPTYPE, E_DRUGCLASS, E_DRUGCLASSIFY, E_DRUGFORM, E_COMITMEMO, E_COMITCODE, E_INVFLAG, E_PURTYPE, E_SOURCECODE, E_DRUGAPLTYPE, E_ARMYORDCODE, E_PARCODE, E_PARORDCODE, E_SONTRANSQTY, 
                                WEXP_ID, WLOC_ID, CANCEL_ID, E_RESTRICTCODE, E_ORDERDCFLAG, E_HIGHPRICEFLAG, E_RETURNDRUGFLAG, E_RESEARCHDRUGFLAG, E_VACCINE, E_TAKEKIND, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, M_STOREID, M_CONTID, M_AGENNO, 
                                E_PATHNO, E_ORDERUNIT, E_FREQNOO, E_FREQNOI, CONTRACNO, UPRICE, DISC_CPRICE, DISC_UPRICE, EASYNAME, CANCEL_NOTE, NHI_PRICE, M_PURUN, M_CONTPRICE, M_DISCPERC, EXCH_RATIO, MIN_ORDQTY, E_SCIENTIFICNAME,
                                SELF_BID_SOURCE, SELF_CONTRACT_NO, SELF_PUR_UPPER_LIMIT, SELF_CONT_BDATE, SELF_CONT_EDATE)  
                                VALUES (:MMCODE, :MMNAME_C, :MMNAME_E, :MAT_CLASS, :BASE_UNIT, :E_SUPSTATUS, :E_MANUFACT, :E_IFPUBLIC, :E_STOCKTYPE, :E_SPECNUNIT, :E_COMPUNIT, :E_YRARMYNO, :E_ITEMARMYNO, :E_GPARMYNO, :E_CLFARMYNO, to_date(to_char(:E_CODATE, 'yyyy/mm/dd'), 'yyyy/mm/dd'),
                                :E_PRESCRIPTYPE, :E_DRUGCLASS, :E_DRUGCLASSIFY, :E_DRUGFORM, :E_COMITMEMO, :E_COMITCODE, :E_INVFLAG, :E_PURTYPE, :E_SOURCECODE, :E_DRUGAPLTYPE, :E_ARMYORDCODE, :E_PARCODE, :E_PARORDCODE, :E_SONTRANSQTY,
                                :WEXP_ID, :WLOC_ID, :CANCEL_ID, :E_RESTRICTCODE, :E_ORDERDCFLAG, :E_HIGHPRICEFLAG, :E_RETURNDRUGFLAG, :E_RESEARCHDRUGFLAG, :E_VACCINE, :E_TAKEKIND, 
                                SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, '1', '0', '0', :E_PATHNO, :E_ORDERUNIT, :E_FREQNOO, :E_FREQNOI, :CONTRACNO, :UPRICE, :DISC_CPRICE, :DISC_UPRICE, :EASYNAME, :CANCEL_NOTE, :NHI_PRICE, :M_PURUN, :M_CONTPRICE, :M_DISCPERC, :EXCH_RATIO, :MIN_ORDQTY, :E_SCIENTIFICNAME,
                                :SELF_BID_SOURCE, :SELF_CONTRACT_NO, :SELF_PUR_UPPER_LIMIT, :SELF_CONT_BDATE, :SELF_CONT_EDATE)";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        /* public int Create(MI_MAST mi_mast)
        {
            var sql = @"INSERT INTO MI_MAST_N (MMCODE, MMNAME_C, MMNAME_E, MAT_CLASS, BASE_UNIT, E_SUPSTATUS, E_MANUFACT, E_IFPUBLIC, E_STOCKTYPE, E_SPECNUNIT, E_COMPUNIT, E_YRARMYNO, E_ITEMARMYNO, E_GPARMYNO, E_CLFARMYNO, E_CODATE,
                                E_PRESCRIPTYPE, E_DRUGCLASS, E_DRUGCLASSIFY, E_DRUGFORM, E_COMITMEMO, E_COMITCODE, E_INVFLAG, E_PURTYPE, E_SOURCECODE, E_DRUGAPLTYPE, E_ARMYORDCODE, E_PARCODE, E_PARORDCODE, E_SONTRANSQTY, 
                                WEXP_ID, WLOC_ID, CANCEL_ID, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, M_STOREID, M_CONTID, M_AGENNO)  
                                VALUES (:MMCODE, :MMNAME_C, :MMNAME_E, :MAT_CLASS, :BASE_UNIT, :E_SUPSTATUS, :E_MANUFACT, :E_IFPUBLIC, :E_STOCKTYPE, :E_SPECNUNIT, :E_COMPUNIT, :E_YRARMYNO, :E_ITEMARMYNO, :E_GPARMYNO, :E_CLFARMYNO, to_date(:E_CODATE, 'yyyy/mm/dd'),
                                :E_PRESCRIPTYPE, :E_DRUGCLASS, :E_DRUGCLASSIFY, :E_DRUGFORM, :E_COMITMEMO, :E_COMITCODE, :E_INVFLAG, :E_PURTYPE, :E_SOURCECODE, :E_DRUGAPLTYPE, :E_ARMYORDCODE, :E_PARCODE, :E_PARORDCODE, :E_SONTRANSQTY,
                                :WEXP_ID, :WLOC_ID, :CANCEL_ID, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, '1', '0', '0')";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        } */

        public int Update(MI_MAST mi_mast)
        {
            var sql = @"UPDATE MI_MAST_N SET MMNAME_C = :MMNAME_C, MMNAME_E = :MMNAME_E, MAT_CLASS = :MAT_CLASS, BASE_UNIT = :BASE_UNIT, E_SUPSTATUS = :E_SUPSTATUS, E_MANUFACT = :E_MANUFACT, E_IFPUBLIC = :E_IFPUBLIC, E_STOCKTYPE = :E_STOCKTYPE, 
                                E_SPECNUNIT = :E_SPECNUNIT, E_COMPUNIT = :E_COMPUNIT, E_YRARMYNO = :E_YRARMYNO, E_ITEMARMYNO = :E_ITEMARMYNO, E_GPARMYNO = :E_GPARMYNO, E_CLFARMYNO = :E_CLFARMYNO, E_CODATE = to_date(to_char(:E_CODATE, 'yyyy/mm/dd'), 'yyyy/mm/dd'),
                                E_PRESCRIPTYPE = :E_PRESCRIPTYPE, E_DRUGCLASS = :E_DRUGCLASS, E_DRUGCLASSIFY = :E_DRUGCLASSIFY, E_DRUGFORM = :E_DRUGFORM, E_COMITMEMO = :E_COMITMEMO, E_COMITCODE = :E_COMITCODE, E_INVFLAG = :E_INVFLAG, 
                                E_PURTYPE = :E_PURTYPE, E_SOURCECODE = :E_SOURCECODE, E_DRUGAPLTYPE = :E_DRUGAPLTYPE, E_ARMYORDCODE = :E_ARMYORDCODE, E_PARCODE = :E_PARCODE, E_PARORDCODE = :E_PARORDCODE, E_SONTRANSQTY = :E_SONTRANSQTY, 
                                WEXP_ID = :WEXP_ID, WLOC_ID = :WLOC_ID, CANCEL_ID = :CANCEL_ID, E_RESTRICTCODE = :E_RESTRICTCODE, E_ORDERDCFLAG = :E_ORDERDCFLAG, E_HIGHPRICEFLAG = :E_HIGHPRICEFLAG, E_RETURNDRUGFLAG = :E_RETURNDRUGFLAG, E_RESEARCHDRUGFLAG = :E_RESEARCHDRUGFLAG, E_VACCINE = :E_VACCINE, E_TAKEKIND = :E_TAKEKIND, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP, M_STOREID = :M_STOREID, M_CONTID = :M_CONTID, M_AGENNO = :M_AGENNO, E_PATHNO = :E_PATHNO, E_ORDERUNIT = :E_ORDERUNIT, E_FREQNOO = :E_FREQNOO, E_FREQNOI = :E_FREQNOI, CONTRACNO = :CONTRACNO, UPRICE = :UPRICE, DISC_CPRICE = :DISC_CPRICE, DISC_UPRICE = :DISC_UPRICE, EASYNAME = :EASYNAME, CANCEL_NOTE = :CANCEL_NOTE, NHI_PRICE = :NHI_PRICE,
                                M_PURUN = :M_PURUN, M_CONTPRICE = :M_CONTPRICE, M_DISCPERC = :M_DISCPERC, EXCH_RATIO = :EXCH_RATIO, MIN_ORDQTY = :MIN_ORDQTY,
                                SELF_BID_SOURCE = :SELF_BID_SOURCE,
                                SELF_CONTRACT_NO = :SELF_CONTRACT_NO,
                                SELF_PUR_UPPER_LIMIT = :SELF_PUR_UPPER_LIMIT,
                                SELF_CONT_BDATE = :SELF_CONT_BDATE,
                                SELF_CONT_EDATE = :SELF_CONT_EDATE
                                WHERE MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        /* public int Update(MI_MAST mi_mast)
        {
            var sql = @"UPDATE MI_MAST_N SET MMNAME_C = :MMNAME_C, MMNAME_E = :MMNAME_E, MAT_CLASS = :MAT_CLASS, BASE_UNIT = :BASE_UNIT, E_SUPSTATUS = :E_SUPSTATUS, E_MANUFACT = :E_MANUFACT, E_IFPUBLIC = :E_IFPUBLIC, E_STOCKTYPE = :E_STOCKTYPE, 
                                E_SPECNUNIT = :E_SPECNUNIT, E_COMPUNIT = :E_COMPUNIT, E_YRARMYNO = :E_YRARMYNO, E_ITEMARMYNO = :E_ITEMARMYNO, E_GPARMYNO = :E_GPARMYNO, E_CLFARMYNO = :E_CLFARMYNO, E_CODATE = to_date(:E_CODATE, 'yyyy/mm/dd'),
                                E_PRESCRIPTYPE = :E_PRESCRIPTYPE, E_DRUGCLASS = :E_DRUGCLASS, E_DRUGCLASSIFY = :E_DRUGCLASSIFY, E_DRUGFORM = :E_DRUGFORM, E_COMITMEMO = :E_COMITMEMO, E_COMITCODE = :E_COMITCODE, E_INVFLAG = :E_INVFLAG, 
                                E_PURTYPE = :E_PURTYPE, E_SOURCECODE = :E_SOURCECODE, E_DRUGAPLTYPE = :E_DRUGAPLTYPE, E_ARMYORDCODE = :E_ARMYORDCODE, E_PARCODE = :E_PARCODE, E_PARORDCODE = :E_PARORDCODE, E_SONTRANSQTY = :E_SONTRANSQTY, 
                                WEXP_ID = :WEXP_ID, WLOC_ID = :WLOC_ID, CANCEL_ID = :CANCEL_ID, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        } */

        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST_N WHERE MMCODE=:MMCODE";
            //string sql = @"SELECT 1 FROM MI_MAST_N WHERE MMCODE=:MMCODE AND PROC_TIME IS NOT NULL";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.M_CONTPRICE, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";


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

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";

            // if () 可能需要加上權限判斷可顯示全部者
            //sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim(A.MAT_CLASS)) = '1' ";
            sql += " AND (select MAT_CLSID from MI_MATCLASS where Trim(MAT_CLASS) = Trim((select MAT_CLASS from MI_MAST where MMCODE = A.MMCODE))) = '1' ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR (A.MMNAME_E LIKE :MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR (A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
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

    }
}