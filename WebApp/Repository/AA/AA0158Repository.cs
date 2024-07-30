using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebApp.Repository.AA
{
    public class AA0158Repository : JCLib.Mvc.BaseRepository
    {
        public AA0158Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public MI_MAST GetMI_MAST(string strMmcode)
        {
            string strSql = "select * from mi_mast where mmcode =:mmcode and rownum=1";
            return DBWork.Connection.QueryFirstOrDefault<MI_MAST>(strSql, new { mmcode = strMmcode }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetAll(
            string common, string mat_class_sub, string mmcode,
            string agen_namec, string uni_no, string m_phctnco, 
            string case_no, string drug_kind, 
            int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select    A.MMCODE,
                                  A.CANCEL_ID,
                                  (select data_value || ' ' || data_desc
                                    from param_d where grp_code='Y_OR_N' 
                                   and data_name='YN' and data_value=A.CANCEL_ID) as CANCEL_ID_DESC,
                                  A.E_ORDERDCFLAG,
                                  (select data_value || ' ' || data_desc
                                    from param_d where grp_code='Y_OR_N'
                                   and data_name='YN' and data_value=A.E_ORDERDCFLAG) as E_ORDERDCFLAG_DESC,
                                  A.M_NHIKEY, A.HEALTHOWNEXP,
                                  A.DRUGSNAME, A.MMNAME_C, A.MMNAME_E, 
                                  A.M_PHCTNCO, A.M_ENVDT, A.ISSUESUPPLY, A.E_MANUFACT,
                                  A.BASE_UNIT,
                                  (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end)
                                     from MI_UNITCODE where UNIT_CODE=A.BASE_UNIT) as BASE_UNIT_DESC,
                                  A.M_PURUN,   
                                  (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end)
                                     from MI_UNITCODE where UNIT_CODE=A.M_PURUN) as M_PURUN_DESC,
                                  1 as TRUTRATE,
                                  A.MAT_CLASS,
                                  A.MAT_CLASS_SUB, 
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='MAT_CLASS_SUB' and data_value=A.MAT_CLASS_SUB) as MAT_CLASS_SUB_DESC,
                                  A.E_RESTRICTCODE,  
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='E_RESTRICTCODE' and data_value=A.E_RESTRICTCODE) as E_RESTRICTCODE_DESC, 
                                  A.WarBak,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='WARBAK' and data_value=WarBak) as WarBak_desc,
                                  A.OneCost,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='WASTKIND' and data_value=A.OneCost) as OneCost_desc, 
                                  A.HealthPay,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='HEALTHPAY' and data_value=A.HealthPay) as HealthPay_desc,
                                  A.CostKind,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='COSTKIND' and data_value=A.CostKind) as CostKind_desc,
                                  A.WastKind,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='WASTKIND' and data_value=A.WastKind) as WastKind_desc,
                                  A.SpXfee,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='SPXFEE' and data_value=A.SpXfee) as SpXfee_desc,
                                  A.OrderKind,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='ORDERKIND' and data_value=A.OrderKind) as OrderKind_desc,  
                                  A.DrugKind,
                                  (select data_value|| ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='DRUGKIND' and data_value=A.DrugKind) as DrugKind_desc,
                                  A.SPDRUG,
                                  (select data_value|| ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='SPDRUG' and data_value=A.SPDRUG) as SPDRUG_DESC,
                                  A.FASTDRUG,
                                  (select data_value|| ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='FASTDRUG' and data_value=A.FASTDRUG) as FASTDRUG_DESC,
                                  A.MIMASTHIS_SEQ,
                                  A.M_AGENNO,
                                  B.AGEN_NAMEC,
                                  A.M_AGENLAB, A.CaseNo, A.CASEDOCT,
                                  A.E_SOURCECODE,
                                  A.M_CONTID,
                                  (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                    where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_CONTID' and DATA_VALUE=A.M_CONTID) as M_CONTID_desc, 
                                  A.E_ITEMARMYNO, A.NHI_PRICE, A.DISC_CPRICE,
                                  A.M_CONTPRICE, A.E_CODATE, TWN_DATE(A.E_CODATE) E_CODATE_T,
                                  A.ContractAmt, A.ContractSum,
                                  A.TOUCHCASE,
                                  (select data_value|| ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='TOUCHCASE' and data_value=A.TOUCHCASE) as TOUCHCASE_desc,
                                  TWN_DATE(A.BEGINDATE_14) AS BEGINDATE_14_T,A.BEGINDATE_14,
                                  (select EFFSTARTDATE from MI_MAST_HISTORY where MIMASTHIS_SEQ=A.MIMASTHIS_SEQ) as EFFSTARTDATE,
                                  TWN_DATE((select EFFSTARTDATE from MI_MAST_HISTORY where MIMASTHIS_SEQ=A.MIMASTHIS_SEQ)) as EFFSTARTDATE_T,
                                  (select barcode from BC_BARCODE where mmcode <> barcode and mmcode=a.mmcode and rownum = 1) as barcode, 
                                  A.COMMON, A.SPMMCODE, A.UNITRATE, A.DISCOUNT_QTY, A.APPQTY_TIMES,
                                  A.DISC_COST_UPRICE, A.ISIV, A.M_STOREID, A.E_RETURNDRUGFLAG, A.E_VACCINE, 
                                  (SELECT jbid_rcrate 
                                   FROM rcrate 
                                   WHERE data_ym = TWN_YYYMM(a.update_time) AND caseno = A.caseno) JBID_RCRATE
                                from MI_MAST A
                                left join PH_VENDER B on a.m_agenno = b.agen_no
                                where 1=1
                                and a.mat_class in ('01','02')";

            if (common != "" && common != null)
            {
                sql += " AND Trim(A.COMMON) = :p0 ";
                p.Add(":p0", common);
            }
            if (mat_class_sub != "" && mat_class_sub != null)
            {
                if (mat_class_sub == "01" || mat_class_sub == "02")
                {
                    sql += " AND A.MAT_CLASS = :p1";
                }
                else
                {
                    sql += " AND Trim(A.MAT_CLASS_SUB) = :p1 ";
                }
                p.Add(":p1", mat_class_sub);
            }
            if (mmcode != "" && mmcode != null)
            {
                sql += " AND A.MMCODE LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmcode));
            }
            if (agen_namec != "" && agen_namec != null)
            {
                sql += " AND B.AGEN_NAMEC LIKE :p3 ";
                p.Add(":p3", string.Format("%{0}%", agen_namec));
            }
            if (uni_no != "" && uni_no != null)
            {
                sql += " AND B.UNI_NO LIKE :p4 ";
                p.Add(":p4", string.Format("%{0}%", uni_no));
            }
            if (m_phctnco != "" && m_phctnco != null)
            {
                sql += " AND A.M_PHCTNCO LIKE :p5 ";
                p.Add(":p5", string.Format("%{0}%", m_phctnco));
            }
            if (case_no != "" && case_no != null)
            {
                sql += " AND A.CASENO LIKE :p6 ";
                p.Add(":p6", string.Format("%{0}%", case_no));
            }
            if (!string.IsNullOrEmpty(drug_kind))
            {
                sql += " AND A.DRUGKIND = :p7 ";
                p.Add(":p7", drug_kind);
            }

            sql += " ORDER BY MMCODE ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> Get(string id)
        {
            var sql = @" SELECT A.MMCODE,
                                  A.CANCEL_ID,
                                  (select data_value || ' ' || data_desc
                                    from param_d where grp_code='Y_OR_N' 
                                   and data_name='YN' and data_value=A.CANCEL_ID) as CANCEL_ID_desc,
                                  A.E_ORDERDCFLAG,
                                  (select data_value || ' ' || data_desc
                                    from param_d where grp_code='Y_OR_N' 
                                   and data_name='YN' and data_value=A.E_ORDERDCFLAG) as E_ORDERDCFLAG_desc,
                                  A.M_NHIKEY, A.HEALTHOWNEXP,
                                  A.DRUGSNAME, A.MMNAME_C, A.MMNAME_E, 
                                  A.M_PHCTNCO, A.M_ENVDT, A.ISSUESUPPLY, A.E_MANUFACT,
                                  A.BASE_UNIT,
                                  (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end)
                                     from MI_UNITCODE where UNIT_CODE=A.BASE_UNIT) as BASE_UNIT_DESC,
                                  A.M_PURUN,   
                                  (select UNIT_CODE || ' ' || (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end)
                                     from MI_UNITCODE where UNIT_CODE=A.M_PURUN) as M_PURUN_DESC,
                                  1 as TRUTRATE,
                                  A.MAT_CLASS,
                                  A.MAT_CLASS_SUB,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='MAT_CLASS_SUB' and data_value=A.MAT_CLASS_SUB) as MAT_CLASS_SUB_desc,
                                  A.E_RESTRICTCODE,  
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='E_RESTRICTCODE' and data_value=A.E_RESTRICTCODE) as E_RESTRICTCODE_DESC, 
                                  A.WarBak,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='WARBAK' and data_value=WarBak) as WarBak_desc,
                                  A.OneCost,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='WASTKIND' and data_value=A.OneCost) as OneCost_desc, 
                                  A.HealthPay,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='HEALTHPAY' and data_value=A.HealthPay) as HealthPay_desc,
                                  A.CostKind,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='COSTKIND' and data_value=A.CostKind) as CostKind_desc,
                                  A.WastKind,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='WASTKIND' and data_value=A.WastKind) as WastKind_desc,
                                  A.SpXfee,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='SPXFEE' and data_value=A.SpXfee) as SpXfee_desc,
                                  A.OrderKind,
                                  (select data_value || ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='ORDERKIND' and data_value=A.OrderKind) as OrderKind_desc,  
                                  A.DrugKind,
                                  (select data_value|| ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='DRUGKIND' and data_value=A.DrugKind) as DrugKind_desc,
                                  A.MIMASTHIS_SEQ,
                                  A.M_AGENNO,
                                  (select AGEN_NAMEC from ph_vender where AGEN_NO=A.M_AGENNO) as AGEN_NAMEC,
                                  A.M_AGENLAB, A.CaseNo, A.CASEDOCT,
                                  A.E_SOURCECODE,
                                  A.M_CONTID,
                                  (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                    where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_CONTID' and DATA_VALUE=A.M_CONTID) as M_CONTID_desc, 
                                  A.E_ITEMARMYNO, A.NHI_PRICE, A.DISC_CPRICE,
                                  A.M_CONTPRICE, A.E_CODATE, TWN_DATE(A.E_CODATE)  E_CODATE_T,
                                  A.ContractAmt, A.ContractSum,
                                  A.TOUCHCASE,
                                  (select data_value|| ' ' || data_desc
                                     from param_d where grp_code='MI_MAST' 
                                      and data_name='TOUCHCASE' and data_value=A.TOUCHCASE) as TOUCHCASE_desc,
                                  TWN_DATE(A.BEGINDATE_14) AS BEGINDATE_14_T, A.BEGINDATE_14,
                                  (select EFFSTARTDATE from MI_MAST_HISTORY where MIMASTHIS_SEQ=A.MIMASTHIS_SEQ) as EFFSTARTDATE,
                                  TWN_DATE((select EFFSTARTDATE from MI_MAST_HISTORY where MIMASTHIS_SEQ=A.MIMASTHIS_SEQ)) as EFFSTARTDATE_T,
                                    (select barcode from BC_BARCODE where mmcode <> barcode and mmcode=a.mmcode and rownum = 1) as barcode, 
                                  A.COMMON, A.SPMMCODE, A.UNITRATE, A.DISCOUNT_QTY, A.APPQTY_TIMES,
                                  A.DISC_COST_UPRICE, A.ISIV, E_DRUGCLASSIFY, AUTO_APLID, M_MATID, 
                                  M_PAYID, M_DISCPERC, SPDRUG, FASTDRUG, A.M_STOREID, E_RETURNDRUGFLAG, E_VACCINE 
                        FROM MI_MAST A WHERE MMCODE=:MMCODE ";

            //先將查詢結果暫存在tmp_MI_MAST，接著產生BarCode的資料
            IEnumerable<MI_MAST> tmp_MI_MAST = DBWork.Connection.Query<MI_MAST>(sql, new { MMCODE = id });
            //================================產生BarCode的資料=======================================
            Barcode tmp_BarCode = new Barcode();

            foreach (MI_MAST tmp_MI_MASTData in tmp_MI_MAST)
            {
                TYPE type = TYPE.CODE128;

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    if (tmp_MI_MASTData.BARCODE != null)
                    {
                        try
                        {
                            Bitmap img_BarCode = (Bitmap)tmp_BarCode.Encode(type, tmp_MI_MASTData.BARCODE, 550, 45);

                            img_BarCode.Save(ms, ImageFormat.Jpeg);
                            byte[] byteImage = new Byte[ms.Length];
                            byteImage = ms.ToArray();
                            string strB64 = Convert.ToBase64String(byteImage);
                            tmp_MI_MASTData.MMCODE_BARCODE = strB64;
                        }
                        catch (FormatException ex)
                        {
                            tmp_MI_MASTData.MMCODE_BARCODE = null;
                        }
                    }
                    else
                        tmp_MI_MASTData.MMCODE_BARCODE = null;
                }
            }
            //return DBWork.Connection.Query<MI_MAST>(sql, new { MMCODE = id }, DBWork.Transaction);
            return tmp_MI_MAST;
        }

        public IEnumerable<MI_MAST_HISTORY> GetAllD(string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select MMCODE, EFFSTARTDATE, TWN_DATE(EFFSTARTDATE) AS EFFSTARTDATE_T, 
                               EFFENDDATE, TWN_DATE(EFFENDDATE) AS EFFENDDATE_T, M_AGENNO,
                               (select EASYNAME from ph_vender where AGEN_NO=A.M_AGENNO)EASYNAME, 
                               M_AGENLAB, CASENO, E_ITEMARMYNO, NHI_PRICE,
                               M_CONTPRICE, DISC_CPRICE,
                               a.E_SOURCECODE,
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_SOURCECODE'
                                   and DATA_VALUE = a.E_SOURCECODE) as E_SOURCECODE_DESC,
                               a.M_CONTID,
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_CONTID'
                                   and DATA_VALUE = a.M_CONTID) as M_CONTID_DESC,
                               E_CODATE, TWN_DATE(E_CODATE) AS E_CODATE_T,
                               a.TOUCHCASE,
                               (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'MI_MAST' and DATA_NAME = 'TOUCHCASE'
                                   and DATA_VALUE = a.TOUCHCASE) as TOUCHCASE_desc,
                               TWN_DATE(A.BEGINDATE_14) AS BEGINDATE_14_T, A.BEGINDATE_14,
                               TWN_DATE(A.ISSPRICEDATE) AS ISSPRICEDATE_T,A.ISSPRICEDATE,
                               ContractAmt, ContractSum,
                               MIMASTHIS_SEQ,  TWN_DATE(CREATE_TIME) AS CREATE_TIME, CREATE_USER,
                               COMMON, SPMMCODE, UNITRATE,
                               A.DISC_COST_UPRICE, A.ISIV, A.M_STOREID  
                          from MI_MAST_HISTORY a
                         where MMCODE = :P0
                           and to_char(EFFSTARTDATE, 'yyyyMMdd') <= to_char(sysdate, 'yyyyMMdd')
                           and MONEYCHANGE= 'Y'
                         order by MIMASTHIS_SEQ  ";
            p.Add(":P0", mmcode);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST_HISTORY>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetD(string id)
        {
            var sql = @" select MMCODE, EFFSTARTDATE, EFFENDDATE, TWN_DATE(EFFSTARTDATE) AS EFFSTARTDATE_T, TWN_DATE(EFFENDDATE) AS EFFENDDATE_T, CANCEL_ID, 
                               E_ORDERDCFLAG, M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, 
                               MMNAME_C, M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, 
                               BASE_UNIT, M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, 
                               WARBAK, ONECOST, HEALTHPAY, COSTKIND, WASTKIND, 
                               SPXFEE, ORDERKIND, CASEDOCT, DRUGKIND, M_AGENNO, 
                               M_AGENLAB, CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, 
                               NHI_PRICE, DISC_CPRICE, M_CONTPRICE, E_CODATE, CONTRACTAMT, 
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
                               A.ISIV, A.DISC_COST_UPRICE, A.M_STOREID     
                          from MI_MAST_HISTORY a
                         where MIMASTHIS_SEQ = :MIMASTHIS_SEQ  ";
            return DBWork.Connection.Query<MI_MAST>(sql, new { MIMASTHIS_SEQ = id }, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST_HISTORY> GetHisD(string seq)
        {
            var sql = @" select MMCODE, EFFSTARTDATE, EFFENDDATE, TWN_DATE(EFFSTARTDATE) AS EFFSTARTDATE_T, TWN_DATE(EFFENDDATE) AS EFFENDDATE_T, CANCEL_ID, 
                               E_ORDERDCFLAG, M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, 
                               MMNAME_C, M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, 
                               BASE_UNIT, M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, 
                               WARBAK, ONECOST, HEALTHPAY, COSTKIND, WASTKIND, 
                               SPXFEE, ORDERKIND, CASEDOCT, DRUGKIND, M_AGENNO, 
                               M_AGENLAB, CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, 
                               NHI_PRICE, DISC_CPRICE, M_CONTPRICE, E_CODATE, CONTRACTAMT, 
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

        public DataTable GetExcel(
            string common, string mat_class_sub, string mmcode, 
            string agen_namec, string uni_no, string m_phctnco, 
            string case_no, string drug_kind)
        {
            DynamicParameters p = new DynamicParameters();
            //20230823 「藥衛材基本檔修改歷程序號」欄位名稱太長，資料庫11版無法匯出
            var sql = @"select  A.MMCODE 院內碼,
                                A.DRUGSNAME 學名,
                                A.MMNAME_E 英文品名, 
                                A.MMNAME_C 中文品名,
                                A.M_NHIKEY 健保代碼, 
                                A.HEALTHOWNEXP 健保自費碼,
                                A.M_PHCTNCO 許可證號, 
                                A.M_ENVDT 許可證效期,
                                A.ISSUESUPPLY 申請廠商, 
                                A.E_MANUFACT 製造商,
                                A.BASE_UNIT as 藥材單位,
                                (select (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end) from MI_UNITCODE where UNIT_CODE=A.BASE_UNIT) as 藥材單位說明,
                                A.M_PURUN as 出貨包裝單位,
                                (select (case when UI_CHANAME is null then UI_ENGNAME else UI_CHANAME end) from MI_UNITCODE where UNIT_CODE=A.M_PURUN) as 出貨包裝單位說明,
                                A.UNITRATE as 每包裝出貨量,
                              --  A.MAT_CLASS as 物料分類,
                               -- (select MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as 物料分類說明, 
                                A.MAT_CLASS_SUB as 物料子類別,
                                (select data_desc from param_d where grp_code='MI_MAST' and data_name='MAT_CLASS_SUB' and data_value=A.MAT_CLASS_SUB) as 物料子類別說明, 
                                A.COMMON 是否常用品項,
                                (select DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'COMMON' and DATA_VALUE=A.COMMON) as 是否常用品項說明,
                                A.TRUTRATE as 與HIS單位換算比值,
                                A.OneCost as 是否可單一計價,
                                (select data_desc from param_d where grp_code='MI_MAST' and data_name='WASTKIND' and data_value=A.OneCost) as 是否可單一計價說明,
                                A.HealthPay as 是否健保給付,
                                (select data_desc from param_d where grp_code='MI_MAST' and data_name='HEALTHPAY' and data_value=A.HealthPay) as 是否健保給付說明,
                                A.CostKind as 費用分類,
                                (select data_desc from param_d where grp_code='MI_MAST' and data_name='COSTKIND' and data_value=A.CostKind) as 費用分類說明,
                                A.E_RESTRICTCODE as 管制級數,
                                (select data_desc from param_d where grp_code='MI_MAST' and data_name='E_RESTRICTCODE' and data_value=A.E_RESTRICTCODE) as 管制級數說明,
                                A.WarBak as 是否戰備,
                                (select data_desc from param_d where grp_code='MI_MAST' and data_name='WARBAK' and data_value=WarBak) as 是否戰備說明,
                                A.WastKind  as 是否正向消耗,
                                (select data_desc from param_d where grp_code='MI_MAST' and data_name='WASTKIND' and data_value=A.WastKind) as 是否正向消耗說明,
                                A.SpXfee as 是否為特材,
                                (select data_desc from param_d where grp_code='MI_MAST' and data_name='SPXFEE' and data_value=A.SpXfee) as 是否為特材說明,
                                A.OrderKind as 採購類別,
                                (select data_desc from param_d where grp_code='MI_MAST' and data_name='ORDERKIND' and data_value=A.OrderKind) as 採購類別說明,
                                A.CASEDOCT as 小採需求醫師,
                                A.CANCEL_ID as 是否作廢,
                                (select data_desc from param_d where grp_code='Y_OR_N' and data_name='YN' and data_value=A.CANCEL_ID) as 是否作廢說明,
                                A.M_STOREID as 庫備識別碼,
                                A.DrugKind as 中西藥類別,
                                (select data_desc from param_d where grp_code='MI_MAST'and data_name='DRUGKIND' and data_value=A.DrugKind) as 中西藥類別說明,
                                A.ISIV as 是否點滴,
                                A.SPDRUG as 特殊品項,
                                (select data_desc from param_d where grp_code='MI_MAST'and data_name='SPDRUG' and data_value=A.SPDRUG) as 特殊品項說明,
                                A.SPMMCODE as 特材號碼, 
                                A.FASTDRUG as 急救品項,
                                (select data_desc from param_d where grp_code='MI_MAST'and data_name='FASTDRUG' and data_value=A.FASTDRUG) as 急救品項說明,
                                A.APPQTY_TIMES as 申請倍數,
                                A.MIMASTHIS_SEQ as 基本檔修改歷程序號,  
                                A.E_RETURNDRUGFLAG 合理回流藥, 
                                A.E_VACCINE 疫苗, 
                                A.M_AGENNO 廠商代碼, 
                                B.AGEN_NAMEC 廠商名稱,
                                B.EASYNAME as 廠商簡稱, 
                                A.CaseNo 合約案號, 
                                A.M_AGENLAB 廠牌, 
                                A.TOUCHCASE as 合約類別,
                                (select data_desc from param_d where grp_code='MI_MAST'and data_name='TOUCHCASE' and data_value=A.TOUCHCASE) as 合約類別說明,
                                TWN_DATE(A.E_CODATE) 合約到期日,
                                A.E_SOURCECODE as 付款方式,
                                (select DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'E_SOURCECODE' and DATA_VALUE=A.E_SOURCECODE) as 付款方式說明,
                                A.M_CONTID as 合約方式,
                                (select DATA_DESC from PARAM_D where GRP_CODE = 'MI_MAST' and DATA_NAME = 'M_CONTID' and DATA_VALUE=A.M_CONTID) as 合約方式說明,
                                A.DISC_CPRICE 成本價,
                                A.M_CONTPRICE 決標價, 
                                A.DISCOUNT_QTY as 二次折讓數量,
                                A.DISC_COST_UPRICE as 二次優惠單價,
                                A.E_ITEMARMYNO 聯標項次, 
                                A.NHI_PRICE 健保價, 
                                A.ContractAmt 聯標契約總數量, 
                                A.ContractSum 聯標項次契約總價,
                                TWN_DATE(A.BEGINDATE_14) 建立日期,
                                (select TWN_DATE(EFFSTARTDATE) from MI_MAST_HISTORY where MIMASTHIS_SEQ=A.MIMASTHIS_SEQ) as 生效日期,
                                decode(A.OneCost, '0', 'N否', 'Y是') as 是否計價
                                from MI_MAST A
                                left join PH_VENDER B on a.m_agenno = b.agen_no
                                where 1=1
                                and a.mat_class in ('01','02')";

            if (common != "" && common != null)
            {
                sql += " AND Trim(A.COMMON) = :p0 ";
                p.Add(":p0", common);
            }
            if (mat_class_sub != "" && mat_class_sub != null)
            {
                if (mat_class_sub == "01" || mat_class_sub == "02")
                {
                    sql += " AND A.MAT_CLASS= :p1";
                }
                else
                {
                    sql += " AND Trim(A.MAT_CLASS_SUB) = :p1 ";
                }

                p.Add(":p1", mat_class_sub);
            }
            if (mmcode != "" && mmcode != null)
            {
                sql += " AND A.MMCODE LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmcode));
            }
            if (agen_namec != "" && agen_namec != null)
            {
                sql += " AND B.AGEN_NAMEC LIKE :p3 ";
                p.Add(":p3", string.Format("%{0}%", agen_namec));
            }
            if (uni_no != "" && uni_no != null)
            {
                sql += " AND B.UNI_NO LIKE :p4 ";
                p.Add(":p4", string.Format("%{0}%", uni_no));
            }
            if (m_phctnco != "" && m_phctnco != null)
            {
                sql += " AND A.M_PHCTNCO LIKE :p5 ";
                p.Add(":p5", string.Format("%{0}%", m_phctnco));
            }
            if (case_no != "" && case_no != null)
            {
                sql += " AND A.CASENO LIKE :p6 ";
                p.Add(":p6", string.Format("%{0}%", case_no));
            }
            if (!string.IsNullOrEmpty(drug_kind))
            {
                sql += " AND A.DRUGKIND = :p7 ";
                p.Add(":p7", drug_kind);
            }

            sql += " order by MMCODE asc";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public int CreateHis(MI_MAST mi_mast)
        {
            var sql = @"INSERT INTO MI_MAST_HISTORY (
                                MIMASTHIS_SEQ, MMCODE, EFFSTARTDATE, CANCEL_ID, 
                                M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, MMNAME_C, 
                                M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, BASE_UNIT, 
                                M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, WARBAK, 
                                ONECOST, HEALTHPAY, COSTKIND, WASTKIND, SPXFEE, 
                                ORDERKIND, CASEDOCT, DRUGKIND, M_AGENNO, M_AGENLAB, 
                                CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, NHI_PRICE, 
                                DISC_CPRICE, M_CONTPRICE, CONTRACTAMT, CONTRACTSUM, 
                                TOUCHCASE,  ISSPRICEDATE, SPDRUG, FASTDRUG,
                                CREATE_TIME, CREATE_USER, UPDATE_IP ,
                                COMMON, SPMMCODE, UNITRATE, DISCOUNT_QTY, APPQTY_TIMES,
                                DISC_COST_UPRICE, ISIV, M_STOREID, MONEYCHANGE
                                )  
                                VALUES (
                                :MIMASTHIS_SEQ, :MMCODE, SYSDATE, :CANCEL_ID,
                                :M_NHIKEY, :HEALTHOWNEXP, :DRUGSNAME, :MMNAME_E, :MMNAME_C,
                                :M_PHCTNCO, :M_ENVDT, :ISSUESUPPLY, :E_MANUFACT, :BASE_UNIT,
                                :M_PURUN, :TRUTRATE, :MAT_CLASS_SUB, :E_RESTRICTCODE, :WARBAK,
                                :ONECOST, :HEALTHPAY, :COSTKIND, :WASTKIND, :SPXFEE,
                                :ORDERKIND, :CASEDOCT, :DRUGKIND, :M_AGENNO, :M_AGENLAB,
                                :CASENO, :E_SOURCECODE, :M_CONTID, :E_ITEMARMYNO, :NHI_PRICE,
                                :DISC_CPRICE, :M_CONTPRICE,  :CONTRACTAMT, :CONTRACTSUM,
                                :TOUCHCASE,  SYSDATE, :SPDRUG, :FASTDRUG,
                                SYSDATE, :CREATE_USER, :UPDATE_IP,
                                :COMMON, :SPMMCODE, :UNITRATE, :DISCOUNT_QTY, :APPQTY_TIMES,
                                :DISC_COST_UPRICE, :ISIV , :M_STOREID, :MONEYCHANGE
                                )";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int Create(MI_MAST mi_mast)
        {
            var sql = @"INSERT INTO MI_MAST (
                                MMCODE, CANCEL_ID, E_ORDERDCFLAG, M_NHIKEY, HEALTHOWNEXP,
                                DRUGSNAME, MMNAME_E, MMNAME_C, M_PHCTNCO, M_ENVDT,
                                ISSUESUPPLY, E_MANUFACT, BASE_UNIT, M_PURUN, TRUTRATE,
                                MAT_CLASS_SUB, E_RESTRICTCODE, WARBAK, ONECOST, HEALTHPAY,
                                COSTKIND, WASTKIND, SPXFEE, ORDERKIND, CASEDOCT,
                                DRUGKIND, SPDRUG, FASTDRUG, MIMASTHIS_SEQ,
                                M_AGENNO, M_AGENLAB, CASENO, E_SOURCECODE, M_CONTID,
                                E_ITEMARMYNO, NHI_PRICE, DISC_CPRICE, M_CONTPRICE, 
                                CONTRACTAMT, CONTRACTSUM, TOUCHCASE, MAT_CLASS, M_STOREID,
                                COMMON, SPMMCODE, UNITRATE, DISCOUNT_QTY, APPQTY_TIMES,
                                DISC_COST_UPRICE, ISIV, E_DRUGCLASSIFY, AUTO_APLID, M_MATID, 
                                E_CODATE,
                                M_PAYID, M_DISCPERC, CREATE_TIME, CREATE_USER, UPDATE_IP, E_RETURNDRUGFLAG, E_VACCINE
                                )  
                                VALUES (
                                :MMCODE, :CANCEL_ID, :CANCEL_ID, :M_NHIKEY, :HEALTHOWNEXP,
                                :DRUGSNAME, :MMNAME_E, :MMNAME_C, :M_PHCTNCO, :M_ENVDT,
                                :ISSUESUPPLY, :E_MANUFACT, :BASE_UNIT, :M_PURUN, :TRUTRATE,
                                :MAT_CLASS_SUB, :E_RESTRICTCODE, :WARBAK, :ONECOST, :HEALTHPAY,
                                :COSTKIND, :WASTKIND, :SPXFEE, :ORDERKIND, :CASEDOCT,
                                :DRUGKIND, :SPDRUG, :FASTDRUG, :MIMASTHIS_SEQ,
                                :M_AGENNO, :M_AGENLAB, :CASENO, :E_SOURCECODE, :M_CONTID,
                                :E_ITEMARMYNO, :NHI_PRICE, :DISC_CPRICE, :M_CONTPRICE, 
                                :CONTRACTAMT, :CONTRACTSUM, :TOUCHCASE,  :MAT_CLASS, :M_STOREID,
                                :COMMON, :SPMMCODE, :UNITRATE, :DISCOUNT_QTY, NVL(:APPQTY_TIMES, 1) ,
                                :DISC_COST_UPRICE, :ISIV, :E_DRUGCLASSIFY, :AUTO_APLID, :M_MATID,
                                :E_CODATE,
                                :M_PAYID, :M_DISCPERC, SYSDATE, :CREATE_USER, :UPDATE_IP, :E_RETURNDRUGFLAG, :E_VACCINE 
                                )";
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

        public bool CheckExistsUnit(string id)
        {
            string sql = @"SELECT 1 FROM MI_UNITCODE WHERE UNIT_CODE = :BASE_UNIT ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { BASE_UNIT = id }, DBWork.Transaction) == null);
        }

        public int InsertUnit(MI_MAST mi_mast)
        {
            var sql = @"INSERT INTO MI_UNITCODE 
                                (UNIT_CODE, UI_CHANAME, UI_ENGNAME, UI_SNAME, CREATE_TIME, CREATE_USER)  
                        VALUES  (:BASE_UNIT, :UI_CHANAME, :UI_CHANAME, :UI_CHANAME, SYSDATE, '新增院內碼') ";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int Update(MI_MAST mi_mast)
        {
            var sql = @"UPDATE MI_MAST SET 
                                CANCEL_ID = :CANCEL_ID, E_ORDERDCFLAG = :CANCEL_ID,  M_NHIKEY = :M_NHIKEY, 
                                HEALTHOWNEXP = :HEALTHOWNEXP, DRUGSNAME = :DRUGSNAME, MMNAME_E = :MMNAME_E, 
                                MMNAME_C = :MMNAME_C, M_PHCTNCO = :M_PHCTNCO, M_ENVDT = :M_ENVDT,
                                ISSUESUPPLY = :ISSUESUPPLY, E_MANUFACT = :E_MANUFACT, BASE_UNIT = :BASE_UNIT, 
                                M_PURUN = :M_PURUN, TRUTRATE = :TRUTRATE, MAT_CLASS_SUB = :MAT_CLASS_SUB, 
                                E_RESTRICTCODE = :E_RESTRICTCODE, WARBAK = :WARBAK, ONECOST = :ONECOST, 
                                HEALTHPAY = :HEALTHPAY, COSTKIND = :COSTKIND, WASTKIND = :WASTKIND, 
                                SPXFEE = :SPXFEE, ORDERKIND = :ORDERKIND, CASEDOCT = :CASEDOCT,
                                DRUGKIND = :DRUGKIND, SPDRUG = :SPDRUG, FASTDRUG = :FASTDRUG, 
                                MIMASTHIS_SEQ = :MIMASTHIS_SEQ_NEW, M_AGENNO = :M_AGENNO, M_AGENLAB = :M_AGENLAB,
                                CASENO = :CASENO, E_SOURCECODE = :E_SOURCECODE, M_CONTID = :M_CONTID, 
                                E_ITEMARMYNO = :E_ITEMARMYNO, CONTRACTAMT = :CONTRACTAMT, 
                                CONTRACTSUM = :CONTRACTSUM, TOUCHCASE = :TOUCHCASE,
                                COMMON = :COMMON, SPMMCODE=:SPMMCODE, UNITRATE = :UNITRATE,
                                DISCOUNT_QTY = :DISCOUNT_QTY, APPQTY_TIMES = :APPQTY_TIMES,
                                DISC_COST_UPRICE = :DISC_COST_UPRICE, ISIV = :ISIV,
                                NHI_PRICE = :NHI_PRICE,DISC_CPRICE = :DISC_CPRICE , M_CONTPRICE = :M_CONTPRICE,
                                MAT_CLASS = (case :MAT_CLASS_SUB when '1' then '01' else '02' end) , 
                                M_STOREID = :M_STOREID, E_CODATE = :E_CODATE, E_RETURNDRUGFLAG = :E_RETURNDRUGFLAG, E_VACCINE = :E_VACCINE
                        WHERE MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }

        public int UpdateMastBegindate(MI_MAST mi_mast)
        {
            var sql = "";
            if (mi_mast.BEGINDATE_14_T == null)  //前端欄位是BEGINDATE_14_T
            {
                sql = @"UPDATE MI_MAST SET BEGINDATE_14 = null 
                                 WHERE MMCODE = :MMCODE ";
            }
            else
            {
                sql = @"UPDATE MI_MAST SET BEGINDATE_14 = :BEGINDATE_14 
                                 WHERE MMCODE = :MMCODE ";
            }
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }
        public int UpdateMastECodate(MI_MAST mi_mast)
        {
            var sql = "";
            if (mi_mast.E_CODATE_T == null) //前端欄位是E_CODATE_T
            {
                sql = @"UPDATE MI_MAST SET E_CODATE =null
                                 WHERE MMCODE = :MMCODE ";
            }
            else
            {
                sql = @"UPDATE MI_MAST SET E_CODATE = :E_CODATE 
                                 WHERE MMCODE = :MMCODE ";
            }
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }
        public int UpdateHisEffEndDate(string seq)
        {
            var sql = @"UPDATE MI_MAST_HISTORY SET EFFENDDATE = SYSDATE 
                                WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ";
            return DBWork.Connection.Execute(sql, new { MIMASTHIS_SEQ = seq }, DBWork.Transaction);
        }
        public int UpdateHis(MI_MAST_HISTORY mi_mast)
        {
            var sql = @"UPDATE MI_MAST_HISTORY SET
                            EFFSTARTDATE = :EFFSTARTDATE,
                            CANCEL_ID = :CANCEL_ID,
                            E_ORDERDCFLAG = :E_ORDERDCFLAG,
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
                            M_STOREID = :M_STOREID  ,
                            CREATE_TIME = SYSDATE, 
                            CREATE_USER = :CREATE_USER, 
                            UPDATE_IP = :UPDATE_IP 
                        WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ ";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }
        public int InsertHis(MI_MAST mi_mast)
        {
            var sql = @"INSERT INTO MI_MAST_HISTORY (
                                MIMASTHIS_SEQ, MMCODE, EFFSTARTDATE, CANCEL_ID, 
                                M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, MMNAME_C, 
                                M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, BASE_UNIT, 
                                M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, WARBAK, 
                                ONECOST, HEALTHPAY, COSTKIND, WASTKIND, SPXFEE, 
                                ORDERKIND, CASEDOCT, DRUGKIND, M_AGENNO, M_AGENLAB, 
                                CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, NHI_PRICE, 
                                DISC_CPRICE, M_CONTPRICE, CONTRACTAMT, CONTRACTSUM, 
                                TOUCHCASE,  SPDRUG, FASTDRUG, 
                                CREATE_TIME, CREATE_USER, UPDATE_IP,
                                COMMON, SPMMCODE, UNITRATE ,DISCOUNT_QTY, APPQTY_TIMES,
                                DISC_COST_UPRICE, ISIV, M_STOREID, MONEYCHANGE,
                                E_CODATE
                                )  
                                VALUES (
                                :MIMASTHIS_SEQ_NEW, :MMCODE, SYSDATE+1/(24*60*60), :CANCEL_ID,
                                :M_NHIKEY, :HEALTHOWNEXP, :DRUGSNAME, :MMNAME_E, :MMNAME_C,
                                :M_PHCTNCO, :M_ENVDT, :ISSUESUPPLY, :E_MANUFACT, :BASE_UNIT,
                                :M_PURUN, :TRUTRATE, :MAT_CLASS_SUB, :E_RESTRICTCODE, :WARBAK,
                                :ONECOST, :HEALTHPAY, :COSTKIND, :WASTKIND, :SPXFEE,
                                :ORDERKIND, :CASEDOCT, :DRUGKIND, :M_AGENNO, :M_AGENLAB,
                                :CASENO, :E_SOURCECODE, :M_CONTID, :E_ITEMARMYNO, :NHI_PRICE,
                                :DISC_CPRICE, :M_CONTPRICE, :CONTRACTAMT, :CONTRACTSUM,
                                :TOUCHCASE,  :SPDRUG, :FASTDRUG,
                                SYSDATE, :CREATE_USER, :UPDATE_IP,
                                :COMMON, :SPMMCODE, :UNITRATE, :DISCOUNT_QTY, NVL(:APPQTY_TIMES, 1),
                                :DISC_COST_UPRICE, :ISIV , :M_STOREID , :MONEYCHANGE,
                                :E_CODATE
                                )";
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }
        public int UpdateHisECodate(MI_MAST mi_mast)
        {
            var sql = "";
            if (mi_mast.E_CODATE_T == null)
            {
                sql = @"UPDATE MI_MAST_HISTORY SET E_CODATE =null 
                                  WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ  ";
            }
            else
            {
                sql = @"UPDATE MI_MAST_HISTORY SET E_CODATE = :E_CODATE 
                                  WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ  ";
            }
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }
        public int UpdateHisBegindate(MI_MAST mi_mast)
        {
            var sql = "";
            if (mi_mast.BEGINDATE_14_T == null)
            {
                sql = @"UPDATE MI_MAST_HISTORY SET BEGINDATE_14 = null 
                                 WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ  ";
            }
            else
            {
                sql = @"UPDATE MI_MAST_HISTORY SET BEGINDATE_14 = :BEGINDATE_14 
                                  WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ  ";
            }
            return DBWork.Connection.Execute(sql, mi_mast, DBWork.Transaction);
        }
        public int InsertHisPrice(MI_MAST mi_mast)
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
                                TOUCHCASE, BEGINDATE_14, ISSPRICEDATE, SPDRUG, FASTDRUG, 
                                CREATE_TIME, CREATE_USER, UPDATE_IP,
                                COMMON, SPMMCODE, UNITRATE ,DISCOUNT_QTY, APPQTY_TIMES ,
                                DISC_COST_UPRICE, ISIV , M_STOREID , MONEYCHANGE
                                )  
                                SELECT  
                                :MIMASTHIS_SEQ_NEW, MMCODE, SYSDATE+1/(24*60*60), CANCEL_ID, 
                                M_NHIKEY, HEALTHOWNEXP, DRUGSNAME, MMNAME_E, MMNAME_C, 
                                M_PHCTNCO, M_ENVDT, ISSUESUPPLY, E_MANUFACT, BASE_UNIT, 
                                M_PURUN, TRUTRATE, MAT_CLASS_SUB, E_RESTRICTCODE, WARBAK, 
                                ONECOST, HEALTHPAY, COSTKIND, WASTKIND, SPXFEE, 
                                ORDERKIND, CASEDOCT, DRUGKIND, M_AGENNO, M_AGENLAB, 
                                CASENO, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, NHI_PRICE, 
                                DISC_CPRICE, M_CONTPRICE, E_CODATE, CONTRACTAMT, CONTRACTSUM, 
                                TOUCHCASE, BEGINDATE_14, SYSDATE, SPDRUG, FASTDRUG, 
                                SYSDATE, :CREATE_USER, :UPDATE_IP,
                                COMMON, SPMMCODE, UNITRATE ,DISCOUNT_QTY, APPQTY_TIMES ,
                                DISC_COST_UPRICE, ISIV, M_STOREID  , :MONEYCHANGE
                                FROM MI_MAST_HISTORY 
                                WHERE MIMASTHIS_SEQ = :MIMASTHIS_SEQ
                                ";
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

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.mmcode,
                                           a.mmname_c,
                                           a.mmname_e
                                     FROM  MI_MAST a
                                    WHERE  1 = 1 
                                ";

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
            string sql = @" select '01' value, '全部藥品' text, '01 全部藥品' combitem, -1 data_seq from dual
                        union
                        select '02' value, '全部衛材' text, '02 全部衛材' combitem, 0 data_seq from dual
                        union 
                        SELECT DATA_VALUE as VALUE, DATA_DESC as TEXT, 
                        Trim(DATA_VALUE) || ' ' || Trim(DATA_DESC) as COMBITEM,DATA_SEQ
                        FROM PARAM_D WHERE 1=1 
                        AND GRP_CODE = 'MI_MAST' AND DATA_NAME = 'MAT_CLASS_SUB' ";

            sql += " ORDER BY DATA_SEQ ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetMatclassSubFCombo()
        {
            string sql = @"  
                        SELECT DATA_VALUE as VALUE, DATA_DESC as TEXT, 
                        Trim(DATA_VALUE) || ' ' || Trim(DATA_DESC) as COMBITEM,DATA_SEQ
                        FROM PARAM_D WHERE 1=1 
                          AND GRP_CODE = 'MI_MAST' AND DATA_NAME = 'MAT_CLASS_SUB' 
                        ORDER BY DATA_SEQ ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
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

        public string GetMatClass(string mat_class_sub)
        {
            var p = new DynamicParameters();
            var sql = @" select MAT_CLSID from MI_MATCLASS  where MAT_CLASS = :p0 ";

            p.Add(":p0", mat_class_sub);

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }
        public string GetHisSeq()
        {
            string sql = " SELECT MIMASTHISTORY_SEQ.nextval FROM DUAL ";
            //sql = " SELECT NVL(MAX(MIMASTHIS_SEQ),0)+1 as SEQ FROM MI_MAST_HISTORY ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
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
        public IEnumerable<COMBO_MODEL> GetCaseNoCombo(string strCaseNo)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT
                        caseno AS value, 
                        caseno AS text,
                        jbid_rcrate as EXTRA1
                    FROM
                        rcrate
                    WHERE 1=1";

            if (!String.IsNullOrEmpty(strCaseNo))
            {
                sql += @" AND caseno like :p0 ";
                p.Add(":p0", string.Format("{0}%", strCaseNo));
            }

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        public IEnumerable<COMBO_MODEL> GetCurrCaseNoCombo(string strCaseNo)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT
                        caseno AS value, 
                        caseno AS text,
                        jbid_rcrate as EXTRA1
                    FROM
                        rcrate
                    WHERE
                        data_ym = (SELECT set_ym FROM mi_mnset WHERE set_status = 'N')";

            if (!String.IsNullOrEmpty(strCaseNo))
            {
                sql += @" AND caseno like: p0 ";
                p.Add(":p0", string.Format("{0}%", strCaseNo));
            }

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        public IEnumerable<COMBO_MODEL> GetAgenNamecCombo(string strAgenNo)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT DISTINCT
                            agen_no AS value,
                            agen_namec AS text,
                            agen_no || ' ' || agen_namec AS combitem
                        FROM
                            ph_vender
                        WHERE 1=1";

            if (!String.IsNullOrEmpty(strAgenNo))
            {
                sql += @" AND agen_no like :p0 ";
                p.Add(":p0", string.Format("{0}%", strAgenNo));
            }
            sql += " ORDER BY agen_no ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        public IEnumerable<COMBO_MODEL> GetUniNoCombo(string strUniNo)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT DISTINCT
                        uni_no AS value,
                        uni_no AS text
                    FROM
                        ph_vender
                    WHERE 1=1";

            if (!String.IsNullOrEmpty(strUniNo))
            {
                sql += @" AND uni_no like :p0 ";
                p.Add(":p0", string.Format("{0}%", strUniNo));
            }
            sql += " ORDER BY uni_no ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        public IEnumerable<COMBO_MODEL> GetMPhctncoCombo(string strMPhctnco)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT DISTINCT
                        M_PHCTNCO AS value, 
                        M_PHCTNCO AS text
                    FROM
                        MI_MAST
                    WHERE 1=1
                        AND M_PHCTNCO <> ' '";

            if (!String.IsNullOrEmpty(strMPhctnco))
            {
                sql += @" AND M_PHCTNCO like :p0 ";
                p.Add(":p0", string.Format("%{0}%", strMPhctnco));
            }
            sql += " ORDER BY M_PHCTNCO";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
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
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public bool CheckExists803()
        {
            string sql = @"SELECT 1 FROM PARAM_D 
                            WHERE GRP_CODE = 'HOSP_INFO'
                              AND DATA_NAME = 'HospCode' 
                              AND DATA_VALUE = '803' ";
            return !(DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction) == null);
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

        public IEnumerable<MI_MAST> GetHisdata(string id)
        {
            var sql = @" SELECT  DA_CHNAME as MMNAME_C, DA_EGNAME as MMNAME_E,
                                 DA_UNIT as BASE_UNIT, DA_INSCODE as M_NHIKEY,
                                 DA_CUNIT as UI_CHANAME
                         FROM TOPDIA WHERE DA_SKDIACODE = :MMCODE ";

            return DBWork.Connection.Query<MI_MAST>(sql, new { MMCODE = id }, DBWork.Transaction);
        }

        public DateTime Getdatetime(string dt)
        {
            string sql = @"SELECT TO_DATE(:pDate || '00:00:00','YYYY/MM/DD HH24:MI:SS') TWNDATE FROM DUAL ";
            DateTime rtn = Convert.ToDateTime(DBWork.Connection.ExecuteScalar(sql, new { pDate = dt }, DBWork.Transaction));
            return rtn;
        }

        public bool CheckEffstratdate(string id)
        {
            string sql = @" SELECT 1 FROM DUAL WHERE TO_DATE(:PDATE || '00:00:00','YYYY/MM/DD HH24:MI:SS') > SYSDATE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PDATE = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsBaseUint(string id)
        {
            string sql = @" SELECT 1 FROM MI_UNITCODE 
                             WHERE UNIT_CODE = :UNIT  ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { UNIT = id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsMast(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
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

        public int InsertHisFromMast(string seq, string id)
        {
            var sql = @"INSERT INTO MI_MAST_HISTORY (
                                MIMASTHIS_SEQ, MMCODE, EFFSTARTDATE, CANCEL_ID, E_ORDERDCFLAG, 
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
                                    :MIMASTHIS_SEQ, MMCODE, SYSDATE+1/(24*60*60), CANCEL_ID, E_ORDERDCFLAG, 
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
                                MIMASTHIS_SEQ, MMCODE, EFFSTARTDATE, CANCEL_ID, E_ORDERDCFLAG, 
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
                                :MIMASTHIS_SEQ, :MMCODE, :EFFSTARTDATE, :CANCEL_ID, :E_ORDERDCFLAG,
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

        public DataTable GetExcelExample()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>{
                { "院內碼", "" },
                { "學名", "" },
                { "英文品名", "" },
                { "中文品名", "必填" },
                { "健保代碼", ""  },
                { "健保自費碼", ""  },
                { "許可證號", "" },
                { "許可證效期", "" },
                { "申請廠商", "" },
                { "製造商", "" },
                { "藥材單位", "VIAL" },
                { "出貨包裝單位", "BX" },
                { "每包裝出貨量", "50" },
                { "物料子類別", "1" },
                { "是否常用品項", "1"  },
                { "與HIS單位換算比值", "1" },
                { "是否可單一計價", "0" },
                { "是否健保給付", "0" },
                { "費用分類", "0" },
                { "管制級數", "N" },
                { "是否戰備", "0" },
                { "是否正向消耗", "0" },
                { "是否為特材", "0" },
                { "採購類別", "0" },
                { "小採需求醫師", "" },
                { "是否作廢", "N" },
                { "庫備識別碼", "0" },
                { "中西藥類別", "0" },
                { "是否點滴", "N" },
                { "特殊品項", "0" },
                { "特材號碼", "" },
                { "急救品項", "0" },
                { "申請倍數", "1" },
                { "廠商代碼", "97160544" },
                { "合約案號", "" },
                { "廠牌", "" },
                { "合約類別", "0" },
                { "合約到期日", DateTime.Now.AddDays(1).ToString("yyyy/MM/dd") },
                { "付款方式", "N" },
                { "合約方式", "0" },
                { "成本價", "" },
                { "決標價", "" },
                { "二次折讓數量", "0" },
                { "二次優惠單價", "0" },
                { "聯標項次", "" },
                { "健保價", "" },
                { "聯標契約總數量", "0" },
                { "聯標項次契約總價", "0" },
            };

            return ConvertDict2DataTable(dict);
        }
        private DataTable ConvertDict2DataTable(Dictionary<string, string> dict)
        {
            DataTable dt = new DataTable();
            List<string> values = new List<string> { };

            foreach (KeyValuePair<string, string> kp in dict)
            {
                dt.Columns.Add(kp.Key, typeof(string));
                values.Add(kp.Value);
            }
            dt.Rows.Add(values.ToArray());

            return dt;
        }
    }
}