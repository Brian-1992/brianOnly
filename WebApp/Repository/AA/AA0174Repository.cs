using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0174Repository : JCLib.Mvc.BaseRepository
    {
        public AA0174Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0174_MODEL> GetAll(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                                                string p11, string p12, string p13, string p14, string p15, string p16, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = "";

            if (String.IsNullOrEmpty(p8) || p8 == "in")
            {
                sql += BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
            }

            if (String.IsNullOrEmpty(p11) && String.IsNullOrEmpty(p13)) //搜尋條件 無發票號碼和訂單號碼 可顯示out
            {
                if (String.IsNullOrEmpty(p8) || p8 == "out")
                {
                    if (!String.IsNullOrEmpty(sql))
                    {
                        sql += @"union";
                    }
                    sql += BuildUninSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                }
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0174_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string GetT2F1(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                              string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();
            var sql = "SELECT NVL(sum(f32), 0)FROM(" +
                BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16) +
                ") where f1 = '進貨'";

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }

        public string GetT2F2(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                              string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();
            var sql = "SELECT NVL(sum(f32), 0)FROM(" +
                BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16) +
                ") where f1 = '退貨'";

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }

        public string GetT2F3(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                              string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();
            var sql = "SELECT NVL(sum(f22), 0)FROM(" +
                BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16) +
                ") where f1 = '退貨'";

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }

        public string GetT2F4(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                              string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();
            var sql = "SELECT NVL(sum(f32), 0)FROM(" +
                BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16) +
                ") where f1 = '退貨'";

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }

        public string GetT2F5(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                              string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();
            var sql = "SELECT NVL(sum(f32), 0)FROM(" +
                BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16) +
                ") where f1='進貨' and f40= '合約'";

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }

        public string GetT2F6(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                              string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();
            var sql = "SELECT NVL(sum(f32), 0)FROM(" +
                BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16) +
                ") where f1='退貨' and f40= '合約'";

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }

        public string GetT2F7(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                              string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();
            var sql = "SELECT NVL(sum(f46), 0)FROM(" +
                BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16) +
                ")";

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }

        public string GetT2F8(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                              string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();
            var sql = "SELECT NVL(sum(f32), 0)FROM(" +
                BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16) +
                ") where f40= '合約'";

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }

        public string GetT2F9(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                              string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();
            var sql = "SELECT NVL(sum(f15), 0)FROM(" +
                BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16) +
                ")";

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }

        public string GetT2F10(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                               string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();
            var sql = "SELECT NVL(sum(f44), 0)FROM(" +
                BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16) +
                ")";

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
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

        public IEnumerable<COMBO_MODEL> GetWarBakCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '非戰備' TEXT FROM DUAL
                            UNION
                            SELECT '1' VALUE, '戰備' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

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

        public IEnumerable<COMBO_MODEL> GetERestrictcodeCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT ' ' VALUE, '全部' TEXT FROM DUAL
                            UNION
                            SELECT 'N' VALUE, '非管制用藥' TEXT FROM DUAL
                            UNION
                            SELECT '0' VALUE, '其它列管藥品' TEXT FROM DUAL
                            UNION
                            SELECT '1' VALUE, '第一級管制用藥' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '第二級管制用藥' TEXT FROM DUAL
                            UNION
                            SELECT '3' VALUE, '第三級管制用藥' TEXT FROM DUAL
                            UNION
                            SELECT '4' VALUE, '第四級管制用藥' TEXT FROM DUAL
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

        public IEnumerable<COMBO_MODEL> GetDrugKindCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '非藥品' TEXT FROM DUAL
                            UNION
                            SELECT '1' VALUE, '西藥' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '中藥' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
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
                                      AND  nvl(cancel_id, 'N') = 'N'
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

        public IEnumerable<PH_VENDER> GetPHVenderCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.AGEN_NO,
                                           a.EASYNAME,
                                           a.AGEN_NAMEC
                                     FROM  PH_VENDER a
                                    WHERE  1 = 1 
                                ";

            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql = string.Format(sql, "(NVL(INSTR(A.AGEN_NO, :AGEN_NO_I), 1000) + NVL(INSTR(A.AGEN_NAMEC, :AGEN_NAMEC_I), 100) * 10 + NVL(INSTR(A.EASYNAME, :EASYNAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":AGEN_NO_I", p0);
                p.Add(":AGEN_NAMEC_I", p0);
                p.Add(":EASYNAME_I", p0);

                sql = string.Format("SELECT * FROM ({0}) TMP WHERE 1=1 ", sql);

                sql += " AND (AGEN_NO LIKE :AGEN_NO ";
                p.Add(":AGEN_NO", string.Format("%{0}%", p0));

                sql += " OR AGEN_NAMEC LIKE :AGEN_NAMEC ";
                p.Add(":AGEN_NAMEC", string.Format("%{0}%", p0));

                sql += " OR EASYNAME LIKE :EASYNAME) ";
                p.Add(":EASYNAME", string.Format("%{0}%", p0));

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


        public DataTable GetExcel1(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                                   string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DISTINCT f41 AS 健保代碼, f6 AS 廠商統編, f7 AS 廠商名稱, f30 AS 廠商地址 FROM (" +
                BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p14, p15, p13, p16) +
                ")";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel2(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
                                   string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var p = new DynamicParameters();
            var sql = "";

            if (String.IsNullOrEmpty(p8) || p8 == "in")
            {
                sql += BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
            }

            if (String.IsNullOrEmpty(p8) || p8 == "out")
            {
                if (!String.IsNullOrEmpty(sql))
                {
                    sql += @"union";
                }
                sql += BuildUninSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
            }

            sql = string.Format(@"
                select f1 類別, f3 日期, f4 訂單號碼, f5 廠商代碼,
                       f6 廠商統編, f7 廠商名稱, f8 藥材代碼, f34 藥材名稱, f9 包裝, f10 單位,
                       f11 數量, f12 末效期, f13 單價, f14 贈品數量, f15 折讓金額,
                       f16 小計, f19 發票日期, f20 發票號碼, f21 備註, f22 贈品小計,
                       f23 交貨日期, f24 製造批號, f27 病患病歷號, f28 病患姓名,
                       f29  明細備註, f30 廠商地址, f31 合約優惠金額, f32 調整後小計金額,
                        f33 調整後優惠金額, f35 管制品, f36 買斷寄庫, f37 案號, f38 合約到期日,
                        f39 合約類別, f40  是否合約, f41 健保代碼, f42 健保價, f43 合約價,
                        f44 合約成本差額, f45 合約小計, f46 合約贈品小計, f47 許可證號,
                        f48 聯標項次, f49 中西藥類別, f50 優惠比, f51 優惠單價
                 from (
                    {0}
                )
                ", sql);

            var sql1 = @"with srcseqs AS(
                       SELECT a.src_seq FROM bc_cs_acc_log a WHERE a.src_seq IS NOT NULL AND twn_date(a.acc_time) BETWEEN :p0 AND :p1
                    )  SELECT
                        '進貨' 類別,
                        twn_date(a.acc_time) 日期,
                        a.po_no 訂單號碼,
                        a.agen_no 廠商代碼,
                        (SELECT uni_no FROM ph_vender WHERE agen_no = a.agen_no ) 廠商統編,
                        (SELECT easyname FROM ph_vender WHERE agen_no = a.agen_no ) 廠商名稱,
                        a.mmcode 藥材代碼,
                        b.mmname_c 藥材名稱,
                        a.acc_purun 包裝,
                        b.base_unit 單位,
                        a.acc_qty 數量,
                        twn_date(a.exp_date) 末效期,
                        a.acc_po_price 單價,
                        0 贈品數量,
                        a.extra_disc_amount 折讓金額,
                        ( a.acc_qty * a.acc_po_price ) 小計,
                        twn_date(d.invoice_dt) 發票日期,
                        d.invoice 發票號碼,
                        substr((SELECT memo FROM mm_po_m WHERE po_no = a.po_no ),0, 30) 備註,
                        0 贈品小計,
                        twn_date(d.deli_dt) 交貨日期,
                        a.lot_no 製造批號,
                        a.chartno AS 病患病歷號,
                        a.chinname AS 病患姓名,
                        (SELECT memo FROM mm_po_d WHERE po_no = a.po_no AND   mmcode = a.mmcode AND   chinname = a.chinname AND   chartno = a.chartno ) AS 明細備註,
                        (SELECT agen_add FROM ph_vender WHERE agen_no = a.agen_no ) 廠商地址,
                        a.acc_disc_cprice 合約優惠金額,
                        round(a.acc_qty * a.acc_po_price,0) 調整後小計金額,
                        round(a.acc_disc_cprice,0) 調整後優惠金額,
                        DECODE(b.e_restrictcode,'N','否','是') 管制品,
                        DECODE(b.e_sourcecode,'P','買斷','C','寄售','') 買斷寄庫,
                        b.caseno 案號,
                        twn_date(b.e_codate) 合約到期日,
                        DECODE(b.touchcase,'0','非合約','1','院內選項','2','非院內選項','3','院內自辦合約','') 合約類別,
                        DECODE(b.m_contid,'0','合約','2','非合約','') 是否合約,
                        b.m_nhikey 健保代碼,
                        b.nhi_price 健保價,
                        a.acc_po_price 合約價,
                        ( a.acc_qty * a.acc_po_price - a.po_qty * a.acc_disc_cprice ) 合約成本差額,
                        ( a.acc_qty * a.acc_po_price ) 合約小計,
                        0 合約贈品小計,
                        b.m_phctnco 許可證號,
                        b.e_itemarmyno 聯標項次,
                        DECODE(b.drugkind,'0','非藥品','1','西藥','2','中藥','') 中西藥類別,
                        a.acc_m_discperc 優惠比,
                        a.acc_disc_cprice 優惠單價
                      FROM
                        bc_cs_acc_log a,
                        mi_mast b,
                        ph_invoice d
                      WHERE
                        a.mmcode = b.mmcode
                        AND   a.po_no = d.po_no (+)
                        AND   a.mmcode = d.mmcode (+)
                        AND   a.invoice = d.invoice (+)
                        AND   NOT EXISTS (
                            SELECT
                                1
                            FROM
                                srcseqs
                            WHERE
                                a.seq = src_seq
                        )
                        AND   a.acc_qty > 0
                                ";



            sql1 += BuildWhereConditionsSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        private string BuildGridSql(
            ref DynamicParameters p,
            string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
            string p11, string p12, string p13, string p14, string p15, string p16)
        {
            var sql = @"with srcseqs AS(
                       SELECT a.src_seq FROM bc_cs_acc_log a WHERE a.src_seq IS NOT NULL AND twn_date(a.acc_time) BETWEEN :p0 AND :p1
                    )  SELECT
                        '進貨' f1, twn_date(a.acc_time) f3, a.po_no f4, a.agen_no f5,
                        (SELECT uni_no FROM ph_vender WHERE agen_no = a.agen_no ) f6,
                        (SELECT easyname FROM ph_vender WHERE agen_no = a.agen_no ) f7,
                        a.mmcode f8, a.acc_purun f9, b.base_unit f10, a.acc_qty f11,
                           twn_date(a.exp_date) f12,a.acc_po_price f13,0 f14,a.extra_disc_amount f15,
                           ( a.acc_qty * a.acc_po_price ) f16,twn_date(d.invoice_dt) f19,
                        d.invoice f20,a.memo f21,
                        0 f22,twn_date(d.deli_dt) f23,a.lot_no f24, a.chartno AS f27,a.chinname AS f28, 
                        (SELECT memo FROM mm_po_d WHERE po_no = a.po_no AND mmcode = a.mmcode AND chinname = a.chinname AND chartno = a.chartno ) AS f29,
                        (SELECT agen_add FROM ph_vender WHERE agen_no = a.agen_no ) f30,
                        a.acc_disc_cprice f31, round(a.acc_qty * a.acc_po_price, 0) f32,
                        round(a.acc_disc_cprice, 0) f33,b.mmname_c || ' ' || b.mmname_e f34,
                         DECODE(b.e_restrictcode, 'N', '否', '是') f35,
                        DECODE(b.e_sourcecode, 'P', '買斷', 'C', '寄售', '') f36,
                        b.caseno f37, twn_date(b.e_codate) f38,
                        DECODE(b.touchcase, '0', '非合約', '1', '院內選項', '2', '非院內選項', '3', '院內自辦合約', '') f39,
                        DECODE(b.m_contid, '0', '合約', '2', '非合約', '') f40,b.m_nhikey f41, b.nhi_price f42, a.acc_po_price f43,
                               a.cont_cost_diff f44,
                        (a.acc_qty * a.acc_po_price) f45,0 f46,b.m_phctnco f47, b.e_itemarmyno f48,
                       DECODE(b.drugkind, '0', '非藥品', '1', '西藥', '2', '中藥', '') f49,
                        a.acc_m_discperc f50, a.acc_disc_cprice f51
                      FROM
                        bc_cs_acc_log a,
                        mi_mast b,
                        ph_invoice d
                      WHERE
                        a.mmcode = b.mmcode
                        AND a.po_no = d.po_no(+)
                        AND a.mmcode = d.mmcode(+)
                        AND a.invoice = d.invoice(+)
                        AND NOT EXISTS(SELECT 1 FROM srcseqs WHERE a.seq = src_seq)
                        AND a.acc_qty > 0";
            sql += BuildWhereConditionsSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);

            return sql;
        }

        /// <summary>
        /// Build Union Sql 放的是 退貨的資料
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p7"></param>
        /// <param name="p8"></param>
        /// <param name="p10"></param>
        /// <param name="p11"></param>
        /// <param name="p12"></param>
        /// <param name="p13"></param>
        /// <param name="p14"></param>
        /// <param name="p15"></param>
        /// <param name="p16"></param>
        /// <returns></returns>
        private string BuildUninSql(
            ref DynamicParameters p,
            string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
            string p11, string p12, string p13, string p14, string p15, string p16)
        {
            string sql = @"   select c.inout_type f1, c.tr_date f3, '' f4, c.m_agenno f5,
            (select uni_no from PH_VENDER where agen_no = c.m_agenno) f6,
            (select easyname from PH_VENDER where agen_no = c.m_agenno) f7,
            c.mmcode f8, c.m_purun f9, c.base_unit f10, c.tr_inv_qty f11,
            c.exp_date f12, c.m_contprice f13, 0 f14, 0 f15, (c.tr_inv_qty * c.m_contprice) f16, '' f19, '' f20, 
            (select APPLY_NOTE from ME_DOCM where DOCNO = c.tr_docno) f21, 0 f22, '' f23, c.lot_no f24, '' f27, '' f28, c.memo AS f29,
            (select agen_add from PH_VENDER where agen_no = c.m_agenno) f30, 
            c.disc_cprice f31, round(c.tr_inv_qty * c.m_contprice, 0) f32, 
            round(c.disc_cprice, 0) f33, c.mmname_c || ' ' || c.mmname_e f34,
             c.e_restrictcode f35, c.e_sourcecode f36, c.caseno f37,
             c.e_codate f38, c.touchcase f39, c.m_contid f40, c.m_nhikey f41, c.nhi_price f42, c.m_contprice f43,
             (c.tr_inv_qty * c.m_contprice - c.tr_inv_qty * c.DISC_CPRICE) f44, 
            (c.tr_inv_qty * c.m_contprice) f45, 0 f46, c.m_phctnco f47,
              c.e_itemarmyno f48, c.drugkind f49, c.m_discperc f50,
              c.DISC_CPRICE f51
            from(
               select '退貨' inout_type, twn_date(a.tr_date) tr_date, '' po_no,
            b.M_AGENNO, a.mmcode, a.tr_docno, b.m_purun, b.base_unit,
            a.tr_inv_qty, twn_date(e.exp_date) exp_date, e.lot_no,
            e.item_note as memo, f.disc_cprice, f.mmname_c, f.mmname_e, 
            DECODE(f.E_RESTRICTCODE, 'N', '否', '是')  E_RESTRICTCODE,
            DECODE(f.E_SOURCECODE, 'P', '買斷', 'C', '寄售', '') E_SOURCECODE,
            b.CASENO, TWN_DATE(f.E_CODATE) E_CODATE,
            DECODE(f.TOUCHCASE, '0', '非合約', '1', '院內選項', '2', '非院內選項', '3', '院內自辦合約', '')  TOUCHCASE,
            DECODE(f.M_CONTID, '0', '合約', '2', '非合約', '')  M_CONTID,
            f.M_NHIKEY, f.NHI_PRICE, f.m_contprice, f.M_PHCTNCO,
            f.E_ITEMARMYNO, DECODE(b.DRUGKIND, '0', '非藥品', '1', '西藥', '2', '中藥', '')  DRUGKIND,
           (case when f.m_contprice=0 then 0 else  round(f.m_contprice - f.disc_cprice / f.m_contprice * 100, 2)  end) m_discperc
            from ME_DOCEXP e, MI_MAST b,  MI_WHTRNS a
            left join MI_MAST_HISTORY f on(a.mmcode = f.mmcode
            and a.tr_date between f.effstartdate and NVL(f.effenddate, a.tr_date))
            where a.tr_doctype in ('RJ', 'RJ1') and a.mmcode=b.mmcode
            and a.tr_docno = e.docno and a.mmcode = e.mmcode 
            ";

            if (!string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.tr_date) BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p0));
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p0) & string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.tr_date) >= :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }
            if (string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.tr_date) <= :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }

            if (!string.IsNullOrWhiteSpace(p2))
            {
                sql += " AND B.M_AGENNO = :p2 ";
                p.Add(":p2", p2);
            }
            if (!string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND A.MMCODE = :p3 ";
                p.Add(":p3", p3);
            }

            if (!string.IsNullOrWhiteSpace(p4))
            {
                if (p4 == "all01" || p4 == "all02")
                {
                    sql += " AND B.MAT_CLASS = :p4 ";
                    p.Add(":p4", p4.Replace("all", ""));
                }
                else
                {
                    sql += " AND B.MAT_CLASS_SUB = :p4 ";
                    p.Add(":p4", p4);
                }
            }

            if (!string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND B.WARBAK = :p5 ";
                p.Add(":p5", p5);
            }
            if (!string.IsNullOrWhiteSpace(p6))
            {
                sql += " AND B.E_SOURCECODE = :p6 ";
                p.Add(":p6", p6);
            }
            if (!string.IsNullOrWhiteSpace(p7))
            {
                sql += " AND B.E_RESTRICTCODE = :p7 ";
                p.Add(":p7", p7);
            }
            if (!string.IsNullOrWhiteSpace(p10))
            {
                sql += " AND B.M_CONTID = :p10 ";
                p.Add(":p10", p10);
            }
            if (!string.IsNullOrWhiteSpace(p12))
            {
                sql += " AND B.DRUGKIND = :p12 ";
                p.Add(":p12", p12);
            }

            if (!string.IsNullOrWhiteSpace(p14))
            {
                sql += " AND e.lot_no like :p14 ";
                p.Add(":p14", string.Format("%{0}%", p14));
            }

            if (!string.IsNullOrWhiteSpace(p15))
            {
                sql += " and TWN_DATE(e.EXP_DATE) >=:p15 ";
                p.Add(":p15", string.Format("{0}", p15));
            }

            if (!string.IsNullOrWhiteSpace(p16))
            {
                sql += " and TWN_DATE(e.EXP_DATE) <=:p16 ";
                p.Add(":p16", string.Format("{0}", p16));
            }
            sql += " ) c order by f3 ";

            return sql;
        }
        /// <summary>
        /// 封裝查詢條件的 Sql，後面可以考慮 optional attribute 
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="p"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <param name="p5"></param>
        /// <param name="p6"></param>
        /// <param name="p10"></param>
        /// <param name="p11"></param>
        /// <param name="p12"></param>
        /// <param name="p13"></param>
        /// <param name="p16"></param>
        private string BuildWhereConditionsSql(
            ref DynamicParameters p,
            string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
            string p11, string p12, string p13, string p14, string p15, string p16)
        {
            string sql = "";
            if (!string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.ACC_TIME) BETWEEN :p0 AND :p1 ";
                p.Add(":p0", string.Format("{0}", p0));
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p0) & string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.ACC_TIME) >= :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }
            if (string.IsNullOrWhiteSpace(p0) & !string.IsNullOrWhiteSpace(p1))
            {
                sql += " AND TWN_DATE(A.ACC_TIME) = :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p2))
            {
                sql += " AND A.AGEN_NO = :p2 ";
                p.Add(":p2", p2);
            }
            if (!string.IsNullOrWhiteSpace(p3))
            {
                sql += " AND A.MMCODE = :p3 ";
                p.Add(":p3", p3);
            }

            if (!string.IsNullOrWhiteSpace(p4))
            {
                if (p4 == "all01" || p4 == "all02")
                {
                    sql += " AND B.MAT_CLASS = :p4 ";
                    p.Add(":p4", p4.Replace("all", ""));
                }
                else
                {
                    sql += " AND B.MAT_CLASS_SUB = :p4 ";
                    p.Add(":p4", p4);
                }
            }

            if (!string.IsNullOrWhiteSpace(p5))
            {
                sql += " AND B.WARBAK = :p5 ";
                p.Add(":p5", p5);
            }
            if (!string.IsNullOrWhiteSpace(p6))
            {
                sql += " AND B.E_SOURCECODE = :p6 ";
                p.Add(":p6", p6);
            }
            if (!string.IsNullOrWhiteSpace(p7))
            {
                sql += " AND B.E_RESTRICTCODE = :p7 ";
                p.Add(":p7", p7);
            }
            if (!string.IsNullOrWhiteSpace(p10))
            {
                sql += " AND B.M_CONTID = :p10 ";
                p.Add(":p10", p10);
            }
            if (!string.IsNullOrWhiteSpace(p11))
            {
                sql += " AND A.PO_NO = :p11 ";
                p.Add(":p11", p11);
            }
            if (!string.IsNullOrWhiteSpace(p12))
            {
                sql += " AND B.DRUGKIND = :p12 ";
                p.Add(":p12", p12);
            }
            if (!string.IsNullOrWhiteSpace(p13))
            {
                sql += " AND D.INVOICE like :p13 ";
                p.Add(":p13", string.Format("%{0}%", p13));
            }

            if (!string.IsNullOrWhiteSpace(p14))
            {
                sql += " AND a.lot_no like :p14 ";
                p.Add(":p14", string.Format("%{0}%", p14));
            }

            if (!string.IsNullOrWhiteSpace(p15))
            {
                sql += " and TWN_DATE(A.EXP_DATE) >=:p15 ";
                p.Add(":p15", string.Format("{0}", p15));
            }

            if (!string.IsNullOrWhiteSpace(p16))
            {
                sql += " and TWN_DATE(A.EXP_DATE) <=:p16 ";
                p.Add(":p16", string.Format("{0}", p16));
            }

            return sql;
        }

        public IEnumerable<AA0174PrintModel> GetPrintData(
            string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p10,
            string p11, string p12, string p13, string p14, string p15, string p16
            )
        {
            //轉成需求格式 EX:10804
            string tmpTIME1 = "", tmpTIME2 = "";

            var p = new DynamicParameters();

            var sql = @"SELECT  f8 as MMCODE, f34 as MMNAME_C, f10 as BASE_UNIT, 
                    f13 as PRICE , f48 as E_ITEMARMYNO,
                    sum(case when f1='進貨' then f11 else 0 end) IN_QTY,
                    sum(case when f1='退貨' then f11 else 0 end) RE_QTY, 
                    sum(f14) GIVEAWAY_QTY, sum(f15) EXTRA_DISC_AMOUNT,
                    (f13* (sum(case when f1='進貨' then f11 else 0 end)- sum(case when f1='退貨' then f11 else 0 end))-sum(f14))-sum(f15) as amt
                    FROM (";
            if (String.IsNullOrEmpty(p8) || p8 == "in")
            {
                sql += BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
            }

            if (String.IsNullOrEmpty(p8) || p8 == "out")
            {
                if (!String.IsNullOrEmpty(sql))
                {
                    sql += @"union";
                }
                sql += BuildUninSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
            }
            //+ BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16) 
            //                + BuildUninSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16)
            sql += @")
                    group by f8 , f34 , f10 , f13 ,f48";


            return DBWork.Connection.Query<AA0174PrintModel>(sql, p, DBWork.Transaction);
        }

    }

    public class AA0174_MODEL : JCLib.Mvc.BaseModel
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
        public string F14 { get; set; }
        public string F15 { get; set; }
        public string F16 { get; set; }
        public string F17 { get; set; }
        public string F18 { get; set; }
        public string F19 { get; set; }
        public string F20 { get; set; }
        public string F21 { get; set; }
        public string F22 { get; set; }
        public string F23 { get; set; }
        public string F24 { get; set; }
        public string F25 { get; set; }
        public string F26 { get; set; }
        public string F27 { get; set; }
        public string F28 { get; set; }
        public string F29 { get; set; }
        public string F30 { get; set; }
        public string F31 { get; set; }
        public string F32 { get; set; }
        public string F33 { get; set; }
        public string F34 { get; set; }
        public string F35 { get; set; }
        public string F36 { get; set; }
        public string F37 { get; set; }
        public string F38 { get; set; }
        public string F39 { get; set; }
        public string F40 { get; set; }
        public string F41 { get; set; }
        public string F42 { get; set; }
        public string F43 { get; set; }
        public string F44 { get; set; }
        public string F45 { get; set; }
        public string F46 { get; set; }
        public string F47 { get; set; }
        public string F48 { get; set; }
        public string F49 { get; set; }
        public string F50 { get; set; }
        public string F51 { get; set; }
    }

    public class AA0174DetailMODEL : JCLib.Mvc.BaseModel
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

    }

    public class AA0174PrintModel : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string BASE_UNIT { get; set; }
        public string PRICE { get; set; }
        public string E_ITEMARMYNO { get; set; }
        public string IN_QTY { get; set; }
        public string RE_QTY { get; set; }
        public string GIVEAWAY_QTY { get; set; }
        public string EXTRA_DISC_AMOUNT { get; set; }
        public string AMT { get; set; }
    }
}