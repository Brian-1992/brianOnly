using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data.OracleClient;

namespace WebApp.Repository.AA
{
    public class AA0190Repository : JCLib.Mvc.BaseRepository
    {
        public AA0190Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0190_MODEL> GetAll(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, string p11, string p12, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT T.* FROM ( SELECT 
                            B.MMCODE AS F1, 	
                            B.MMNAME_C AS F2, 	
                            C.SAFE_QTY AS F3, 	
                            C.NORMAL_QTY AS F4, 	
                            A.INV_QTY AS F5, ";

            if (!string.IsNullOrWhiteSpace(p10))
            {

                if (p12 == "Y")
                {
                    sql += @" FLOOR( 
                              ( NVL(C.SAFE_QTY,0)-NVL(A.INV_QTY,0)-NVL(E.DUE_QTY,0) ) 
                                / B.UNITRATE +  ( 100 - :p10 ) / 100 ) *  B.UNITRATE
                             AS F6,";
                }
                else
                {
                    sql += @" FLOOR( 
                              ( NVL(C.SAFE_QTY,0)-NVL(A.INV_QTY,0) ) 
                                / B.UNITRATE +  ( 100 - :p10 ) / 100 ) *  B.UNITRATE
                             AS F6,";
                }
                p.Add(":p10", p10);
            }
            else
            {
                if (p12 == "Y")
                {
                    sql += "    NVL(C.SAFE_QTY,0)-NVL(A.INV_QTY,0)-NVL(E.DUE_QTY,0) AS F6,";
                }
                else
                {
                    sql += "    NVL(C.SAFE_QTY,0)-NVL(A.INV_QTY,0) AS F6,";

                }
            }
            sql += @"	    NVL(c.WAR_QTY,0) AS F7, 	
                            E.PO_NO AS F8, 	
                            NVL(E.PO_QTY,0) AS F9, 	
                            B.BASE_UNIT AS F10, 	
                            B.UNITRATE AS F11, 	
                            B.M_NHIKEY AS F12, 	
                            B.TRUTRATE AS F13 
                        FROM MI_WHINV A, MI_MAST B, MI_BASERO_14 C, MI_WHMAST D,
                            ( SELECT PO_NO,MMCODE,PO_QTY,DELI_QTY,(PO_QTY - DELI_QTY) DUE_QTY FROM MM_PO_D WHERE DELI_STATUS <> 'C'  ) E
                        WHERE A.WH_NO = D.WH_NO
                          AND D.WH_GRADE = '1'
                          AND D.WH_KIND IN ('0','1')
                          AND A.MMCODE = B.MMCODE
                          AND A.WH_NO = C.WH_NO
                          AND A.MMCODE = C.MMCODE
                          AND A.MMCODE = E.MMCODE(+)
                          AND A.INV_QTY < C.SAFE_QTY  ";

            
            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql += " AND A.MMCODE = :p0 ";
                p.Add(":p0", p0);
            }

            if (!string.IsNullOrWhiteSpace(p1))
            {
                if (p1 == "all01" || p1 == "all02")
                {
                    sql += " AND B.MAT_CLASS = :p1 ";
                    p.Add(":p1", p1.Replace("all", ""));
                }
                else
                {
                    sql += " AND B.MAT_CLASS_SUB = :p1 ";
                    p.Add(":p1", p1);
                }
            }
            if (!string.IsNullOrWhiteSpace(p2))
            {
                sql += " AND B.M_CONTID = :p2 ";
                p.Add(":p2", p2);
            }
            if (!string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND B.E_SOURCECODE = :p3 ";
                p.Add(":p3", p3);
            }
            if (!string.IsNullOrWhiteSpace(p4))
            {
                sql += " AND B.ORDERKIND = :p4 ";
                p.Add(":p4", p4);
            }
            if (p5 == "Y")
            {
                sql += " AND B.CANCEL_ID <> 'Y' ";  //Y:作廢
            }
            if (p6 == "Y")
            {
                sql += " AND B.SPDRUG <> '1' ";
            }
            if (p7 == "Y")
            {
                sql += " AND B.FASTDRUG <> '1' ";
            }
            if (p8 == "Y")
            {
                sql += " AND B.E_RESTRICTCODE = 'N' ";  //N:非管制用藥
            }
            if (p9 == "Y")
            {
                sql += " AND NVL(E.DUE_QTY,0) > 0 ";
            }
            sql += " ) T WHERE 1 = 1 ";
            if (p11 == "Y")
            {
                sql += " AND T.F6 > 0 ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0190_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
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

        public IEnumerable<COMBO_MODEL> GetMatClassSubCombo()
        {
            string sql = @"SELECT 
                    data_value AS value, data_desc AS text, 99 as sort_num 
                FROM
                    param_d
                WHERE
                    grp_code = 'MI_MAST'
                    AND   data_name = 'MAT_CLASS_SUB'
                    AND   TRIM(data_desc) IS NOT NULL
                UNION
                     SELECT ' ' AS value, '全部' AS text, 1 as sort_num FROM dual
                UNION 
                     SELECT 'all01' AS value, '全部藥品' AS text, 2 as sort_num FROM dual
                UNION
                     SELECT 'all02' AS value, '全部衛材' AS text, 3 as sort_num FROM dual
                    order by sort_num";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetESourceCodeCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT 'P' VALUE, '買斷' TEXT FROM DUAL
                            UNION
                            SELECT 'C' VALUE, '寄售' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetMContidCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '合約品項' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '非合約' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        

        public IEnumerable<COMBO_MODEL> GetOrderkindCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '無' TEXT FROM DUAL
                            UNION
                            SELECT '1' VALUE, '常備品項' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '小額採購' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public string GetMatClass(string id)
        {
            string sql = @"select mat_class from mi_mast where mmcode=:MMCODE ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetPrno(string id)
        {
            string sql = @"select whno_mm1||twn_systime||:MAT_CLASS from dual ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetPrno01()
        {
            string sql = @"select twn_systime from dual ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, null, DBWork.Transaction).ToString();
            return rtn;
        }

        public bool CheckExistsMMPRM(string id)
        {
            string sql = @"select 1 from MM_PR_M where pr_no = :PR_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PR_NO = id }, DBWork.Transaction) == null);
        }

        public int InsertMMPRM(MM_PR_M mm_pr_m)
        {
            var sql = @"insert into MM_PR_M (
                            pr_no, pr_dept, pr_time, pr_user, mat_class, 
                            pr_status, create_time, create_user, update_ip, xaction, isFromDocm)
                        values (
                            :PR_NO, user_inid(:UPDATE_USER), sysdate, :UPDATE_USER, :MAT_CLASS,
                            '35', sysdate, :UPDATE_USER, :UPDATE_IP, '0', 'N')";

            return DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);
        }

        public int InsertMMPRD(MM_PR_D mm_pr_d)
        {
            var sql = @"insert into MM_PR_D (
                            pr_no, mmcode, m_contid, m_contprice, m_purun, 
                            pr_price,  pr_qty, req_qty_t, unit_swap, agen_fax,
                            agen_name, agen_no,  disc, is_email, rec_status, 
                            m_nhikey, mmname_c, mmname_e, wexp_id, base_unit, 
                            orderkind, caseno, e_sourcecode, unitrate, e_codate, 
                            seq,
                            create_time, create_user, update_time, update_user, update_ip
                            )
                            select :PR_NO, a.mmcode, a.m_contid, a.m_contprice, a.m_purun,
                                (case when a.m_contid='0' then a.m_contprice else a.disc_cprice end ) as pr_price,:PR_QTY as pr_qty, :PR_QTY as req_qty_t, 1 as unit_swap,b.agen_fax,
                                b.agen_namec, b.agen_no, a.m_discperc as disc, 'N' as is_email, '35' as rec_status, 
                                a.m_nhikey, a.mmname_c, a.mmname_e, a.wexp_id, a.base_unit, 
                                a.orderkind, a.caseno, a.e_sourcecode, a.unitrate, a.e_codate, 
                                MM_PR_D_SEQ.nextval,
                                sysdate as create_time, :UPDATE_USER as create_user, sysdate as update_time, :UPDATE_USER as update_user, :UPDATE_IP as update_ip
                            from MI_MAST a , PH_VENDER b 
                            where a.m_agenno = b.agen_no and a.MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, mm_pr_d, DBWork.Transaction);
        }
        public int InsertMMPRM01(MM_PR_M mm_pr_m)
        {
            var sql = @"insert into MM_PR01_M (
                            pr_no, pr_dept, pr_time, pr_user, mat_class, 
                            create_time, create_user, update_ip, isFromDocm)
                        values (
                            :PR_NO, user_inid(:UPDATE_USER), sysdate, :UPDATE_USER, '01',
                            sysdate, :UPDATE_USER, :UPDATE_IP, 'N')";
            return DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);
        }

        public int InsertMMPRD01(MM_PR_D mm_pr_d)
        {
            var sql = @"insert into MM_PR01_D (    
                            pr_no, mmcode, pr_qty, pr_price, m_purun, 
                            base_unit, m_contprice, m_discperc, unit_swap, req_qty_t, 
                            agen_no, m_contid, agen_name, email, agen_fax, 
                            pr_amt,  min_ordqty, disc_cprice, disc_uprice, uprice, 
                            isWilling, discount_qty, disc_cost_uprice, safe_qty, oper_qty, 
                            ship_qty, mmname_c, mmname_e, wexp_id, orderkind, 
                            caseno, e_sourcecode, unitrate, m_nhikey, e_codate,
                            create_time, create_user, update_time, update_user, update_ip
                            )
                            select
                                 :PR_NO as pr_no, a.mmcode, :PR_QTY as pr_qty, (case when a.m_contid='0' then a.m_contprice else a.disc_cprice end ) as pr_price,a.m_purun,
                                 a.base_unit, a.m_contprice, a.m_discperc, 1 as unit_swap, :PR_QTY as req_qty_t,
                                 b.agen_no, a.m_contid, b.agen_namec, b.email, b.agen_fax,
                                 (:PR_QTY * a.m_contprice) as pr_amt, nvl(c.min_ordqty,1) as min_ordqty, a.disc_cprice, a.disc_uprice, a.uprice,
                                 (select isWilling from MILMED_JBID_LIST where substr(a.E_YRARMYNO,1,3)= JBID_STYR and a.E_ITEMARMYNO=BID_NO) as isWilling, 
                                 (select discount_qty from MILMED_JBID_LIST where substr(a.E_YRARMYNO,1,3)= JBID_STYR and a.E_ITEMARMYNO=BID_NO) as discount_qty, 
                                 (select disc_cost_uprice from MILMED_JBID_LIST where substr(a.E_YRARMYNO,1,3)= JBID_STYR and a.E_ITEMARMYNO=BID_NO) as disc_cost_uprice,
                                 nvl(c.safe_qty,0) as safe_qty, nvl(c.oper_qty,0) as oper_qty, 
                                 nvl(c.ship_qty, 0) as ship_qty, a.mmname_c, a.mmname_e, a.wexp_id, a.orderkind, 
                                 a.caseno, a.e_sourcecode, a.unitrate,a.m_nhikey, a.e_codate,
                                 sysdate as create_time, :UPDATE_USER as create_user, sysdate as update_time, :UPDATE_USER as update_user, :UPDATE_IP as update_ip
                            from MI_MAST a
                              left join PH_VENDER b on (a.m_agenno = b.agen_no)
                              left join MI_WINVCTL c on (c.wh_no = WHNO_ME1 and c.mmcode = a.mmcode)
                            where a.mmcode = :MMCODE
                              and nvl(a.e_orderdcflag,'N') = 'N'
                         ";
            return DBWork.Connection.Execute(sql, mm_pr_d, DBWork.Transaction);

        }

    }



    public class AA0190_MODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public string F6 { get; set; }
        public string F7 { get; set; }
        public string F8 { get; set; }
        public string F9 { get; set; }
        public string F10 { get; set; }
        public string F11 { get; set; }
        public string F12 { get; set; }
        public string F13 { get; set; }
    }
    
}