using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.B
{
    public class BD0023Repository : JCLib.Mvc.BaseRepository
    {
        public BD0023Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BD0023> GetAll(string p0, string p1, string p2, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"select A.PO_NO, TWN_DATE(b.acc_time) as invoice_dt, B.AGEN_NO, C.AGEN_NAMEC,
                               sum(IN_AMOUNT) as po_amt
                          FROM PH_INVOICE A, BC_CS_ACC_LOG B, PH_VENDER C, MI_MAST D 
                         WHERE A.PO_NO = B.PO_NO
                           AND B.AGEN_NO = C.AGEN_NO
                           AND A.MMCODE = D.MMCODE
                           and a.deli_status='C'
                           AND a.invoice is null
                            ";

            if (!string.IsNullOrWhiteSpace(p0))  //訂單號碼
            {
                sql += " and A.PO_NO=:p0";
                p.Add(":p0", string.Format("{0}", p0));
            }

            if (!string.IsNullOrWhiteSpace(p1))
            {
                if (p1 == "all01" || p1 == "all02")
                {
                    sql += " AND D.MAT_CLASS = :p1 ";
                    p.Add(":p1", p1.Replace("all", ""));
                }
                else
                {
                    sql += " AND D.MAT_CLASS_SUB = :p1 ";
                    p.Add(":p1", p1);
                }
            }

            if (!string.IsNullOrWhiteSpace(p2))  //合約識別碼
            {
                sql += " and exists (select 1 from MM_PO_M where po_no = a.po_no and M_CONTID = :p2)";
                p.Add(":p2", string.Format("{0}", p2));
            }

            sql += @"
                group by A.PO_NO, TWN_DATE(b.acc_time), B.AGEN_NO, C.AGEN_NAMEC
            ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0023>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BD0023> GetAllD(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"select A.PO_NO, A.MMCODE, (select mmname_c from MM_PO_D where po_no=a.po_no and mmcode=a.mmcode and rownum=1) as mmname_c, 
        A.M_PURUN, (select BASE_UNIT from MM_PO_D where po_no=a.po_no and mmcode=a.mmcode and rownum=1) BASE_UNIT,
                               TWN_DATE(B.EXP_DATE) as EXP_DATE, A.deli_qty as PO_QTY, A.PO_PRICE, a.IN_AMOUNT as PO_AMT, 
                              TWN_DATE(A.INVOICE_DT) as INVOICE_DT,
                               A.INVOICE, B.MEMO
                        FROM PH_INVOICE A, BC_CS_ACC_LOG B
                        WHERE A.MMCODE = B.MMCODE
                          AND A.PO_NO = B.PO_NO
                          and a.deli_status='C'
                          and a.acc_log_seq = b.seq
                          and twn_date(a.deli_dt) = twn_date(b.acc_time)
                        ";

            if (!string.IsNullOrWhiteSpace(p0))  //訂單號碼
            {
                sql += " and A.PO_NO=:p0";
                p.Add(":p0", string.Format("{0}", p0));
            }
            else
            {
                sql += " and 1 = 2 ";
            }
            if (!string.IsNullOrWhiteSpace(p1))  //進貨日期
            {
                sql += " and twn_date(a.deli_dt) = :p1";
                p.Add(":p1", string.Format("{0}", p1));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0023>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
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

        public IEnumerable<COMBO_MODEL> GetMContidCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '合約品項' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '非合約' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }


        public string GetHospName()
        {
            string sql = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospName' ";
            return DBWork.Connection.ExecuteScalar<string>(sql, DBWork.Transaction);
        }

        public IEnumerable<BD0023_PRINT_MODEL> GetReport(string PO_NO, string MAT_CLASS, string M_CONTID)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT A.PO_NO, D.AGEN_NO || ' ' || E.AGEN_NAMEC AS AGEN_NAMEC, 
                                TWN_DATE(A.INVOICE_DT) as INVOICE_DT, 
                                b.memo REMARK, 
                                ROW_NUMBER() OVER (PARTITION BY A.PO_NO ORDER BY A.PO_NO) AS ITEM_NO, 
                                A.MMCODE, 
                                (select mmname_e from MM_PO_D where po_no = a.po_no and mmcode = a.mmcode and rownum=1)|| ' ' || 
                                    (select mmname_c from MM_PO_D where po_no = a.po_no and mmcode = a.mmcode and rownum=1) AS MMNAME_C, 
                                (select BASE_UNIT from MM_PO_D where po_no = a.po_no and mmcode = a.mmcode and rownum=1) BASE_UNIT,
                                A.M_PURUN, 
                                A.DELI_QTY as po_QTY, 
                                A.PO_PRICE, 
                                a.in_amount as PO_AMT, 
                                A.INVOICE, 
                                TWN_DATE(B.EXP_DATE) as EXP_DATE, 
                                B.MEMO,
                                sum(IN_AMOUNT) as PO_SUM 
                        FROM PH_INVOICE A, BC_CS_ACC_LOG B, MM_PO_M D, PH_VENDER E
                        WHERE A.MMCODE = B.MMCODE
                          AND A.PO_NO = B.PO_NO
                          and a.acc_log_seq = b.seq
                          AND A.PO_NO = D.PO_NO
                          and a.invoice is null
                          and a.deli_status='C'
                          AND D.AGEN_NO = E.AGEN_NO
                        ";
            if (!string.IsNullOrWhiteSpace(PO_NO))  
            {
                sql += " and A.PO_NO=:PO_NO";
                p.Add(":PO_NO", string.Format("{0}", PO_NO));
            }

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (MAT_CLASS == "all01" || MAT_CLASS == "all02")
                {
                    sql += " AND C.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", MAT_CLASS.Replace("all", ""));
                }
                else
                {
                    sql += " AND C.MAT_CLASS_SUB = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", MAT_CLASS);
                }
            }

            if (!string.IsNullOrWhiteSpace(M_CONTID))  //合約識別碼
            {
                sql += " and D.M_CONTID=:M_CONTID";
                p.Add(":M_CONTID", string.Format("{0}", M_CONTID));
            }
            sql += @" group by A.PO_NO, D.AGEN_NO, E.AGEN_NAMEC, TWN_DATE(A.INVOICE_DT),b.memo,
                                A.MMCODE, A.M_PURUN, A.DELI_QTY, A.PO_PRICE,a.in_amount , A.INVOICE, TWN_DATE(B.EXP_DATE)
                                ORDER BY A.PO_NO, a.mmcode ";
            return DBWork.Connection.Query<BD0023_PRINT_MODEL>(sql, p, DBWork.Transaction);
        }
    }


    public class BD0023_PRINT_MODEL : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string TRANSNO { get; set; }
        public string INVOICE_DT { get; set; }
        public string AGEN_NO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public decimal PO_AMT { get; set; }
        public decimal PO_SUM { get; set; }
        public string MEMO { get; set; }
        public decimal PO_AMT_SUM { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string M_PURUN { get; set; }
        public string BASE_UNIT { get; set; }
        public string EXP_DATE { get; set; }
        public decimal PO_QTY { get; set; }
        public decimal PO_PRICE { get; set; }
        public string INVOICE { get; set; }
        public string ITEM_NO { get; set; }
        public string REMARK { get; set; }
    }
}