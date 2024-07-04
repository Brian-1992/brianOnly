using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.B
{
    public class BG0009Report_MODEL : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string BASE_UNIT { get; set; }
        public string P_INV_QTY { get; set; }
        public string IN_QTY { get; set; }
        public string REJ_OUTQTY { get; set; }
        public string MIL_QTY { get; set; }
        public string OUT_QTY { get; set; }
        public string T_OUT_QTY { get; set; }
        public string INV_QTY { get; set; }
        public string WRESQTY { get; set; }
        public string M_CONTPRICE { get; set; }
        public string DISC_CPRICE { get; set; }
        public string EXTRA_DISC_AMOUNT { get; set; }
        public string TOT_AMT_1 { get; set; }
        public string TOT_AMT_2 { get; set; }
        public string E_SOURCECODE { get; set; }
        public string M_AGENNO { get; set; }
        public string TOT_AMT_3 { get; set; }
        public string AGEN_NAME { get; set; }
        public string AGEN_TEL { get; set; }
        public string AGEN_FAX { get; set; }
        public string PO_NO { get; set; }
    }
    public class BG0009Repository : JCLib.Mvc.BaseRepository
    {
        public BG0009Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BG0009> GetAll(string pDATA_YM, string[] arr_p0, string p1, string p2, string p2_1, string p3, string p4, string p5, string p6, string p7, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string mat_class = "''";
            string mat_class_sub = "''";
            for (int i = 0; i < arr_p0.Length; i++)
            {
                if (arr_p0[i].Contains("SUB_"))
                {
                    if (i == 0)
                        mat_class_sub = @" '" + arr_p0[i].Replace("SUB_", "") + "'";
                    else
                        mat_class_sub += @",'" + arr_p0[i].Replace("SUB_", "") + "'";
                }
                else
                {
                    if (i == 0)
                        mat_class = @" '" + arr_p0[i] + "'";
                    else
                        mat_class += @",'" + arr_p0[i] + "'";
                }
            }

            var sql = @"with temp_qty as (
	                select B.M_AGENNO as M_AGENNO, AGEN_NAME(B.M_AGENNO) as AGEN_NAME, A.MMCODE, sum(C.ACC_QTY) as ACC_QTY,
	                   (sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as EXTRA_DISC_AMOUNT,
	                   (sum(A.DELI_QTY * A.PO_PRICE)) as TOT_AMT_1,
	                   (sum(A.DELI_QTY * A.PO_PRICE)) - (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE))) + sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as TOT_AMT_2,
	                   (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE)))) as TOT_AMT_3
	                from PH_INVOICE A, MI_MAST B, BC_CS_ACC_LOG C
	                where A.MMCODE=B.MMCODE
	                and C.SEQ=A.ACC_LOG_SEQ
	                and C.ACC_TIME between (select SET_BTIME from MI_MNSET where SET_YM=:pDATA_YM) 
	                 and (select SET_CTIME from MI_MNSET where SET_YM=:pDATA_YM) 
	                group by B.M_AGENNO, A.MMCODE
                )
                select A.MMCODE,A.MMNAME_C,A.BASE_UNIT,A.M_CONTPRICE,A.DISC_CPRICE,A.P_INV_QTY, nvl(temp_qty.ACC_QTY, 0) as IN_QTY, 
                       nvl((select sum(PO_QTY) from PH_INVOICE where INVOICE_TYPE = '2' and DELI_STATUS = 'N' and MMCODE = A.MMCODE and DATA_YM = :pDATA_YM), 0) as REJ_OUTQTY,
	                   A.MIL_QTY,A.OUT_QTY,(A.MIL_QTY+A.OUT_QTY) as T_OUT_QTY,A.INV_QTY,nvl(A.WRESQTY,0) as WRESQTY,
	                   round(nvl(temp_qty.EXTRA_DISC_AMOUNT,0)) as EXTRA_DISC_AMOUNT,
	                   round(nvl(temp_qty.TOT_AMT_1, 0)) as TOT_AMT_1,
	                   round(nvl(temp_qty.TOT_AMT_2, 0)) as TOT_AMT_2,
	                   round(nvl(temp_qty.TOT_AMT_3, 0)) as TOT_AMT_3,
	                   DECODE(A.E_SOURCECODE,'P','買斷','C','寄售','') as E_SOURCECODE,A.M_AGENNO,
	                   D.UNI_NO, b.SPXFEE, b.M_NHIKEY, b.NHI_PRICE
                  from V_COST_MILTORY A, temp_qty, 
                  MI_MAST B, PH_VENDER D
                 where A.MMCODE=B.MMCODE and B.M_AGENNO = D.AGEN_NO
                 and A.M_AGENNO = temp_qty.M_AGENNO and A.MMCODE = temp_qty.MMCODE
                 and A.E_SOURCECODE IN ('C','P') and B.M_CONTID in ('0','2') "
;
            if (pDATA_YM != "")
            {
                sql += @" and A.DATA_YM=:pDATA_YM";
                p.Add(":pDATA_YM", string.Format("{0}", pDATA_YM));
            }
            if (arr_p0.Length > 0)
            {
                sql += @" and (B.MAT_CLASS in (" + mat_class + ")" +
                    "           or B.MAT_CLASS_SUB in (" + mat_class_sub + ") )";
            }
            if (!string.IsNullOrWhiteSpace(p1))
            {
                sql += " and A.E_SOURCECODE=:p1";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p2))
            {
                if (!string.IsNullOrWhiteSpace(p2_1))
                {
                    sql += " and A.M_AGENNO between :p2 and :p2_1";
                    p.Add(":p2", string.Format("{0}", p2));
                    p.Add(":p2_1", string.Format("{0}", p2_1));
                }
                else
                {
                    sql += " and A.M_AGENNO=:p2";
                    p.Add(":p2", string.Format("{0}", p2));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(p2_1))
                {
                    sql += " and A.M_AGENNO=:p2_1";
                    p.Add(":p2_1", string.Format("{0}", p2_1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p3))
            {
                sql += " and B.M_CONTID=:p3";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (!string.IsNullOrWhiteSpace(p4))
            {
                sql += " and B.CASENO=:p4";
                p.Add(":p4", string.Format("{0}", p4));
            }
            if (!string.IsNullOrWhiteSpace(p5))
            {
                sql += " and AGEN_NAME(A.M_AGENNO) LIKE :p5";
                p.Add(":p5", string.Format("%{0}%", p5));
            }
            if (p6 == "true") //隱藏零庫存、未進出、無消耗之品項
            {
                sql += "  and not (INV_QTY = 0 and IN_QTY=0 and OUT_QTY=0)";
            }
            if (!String.IsNullOrEmpty(p7))
            {
                sql += " and D.UNI_NO=:p7";
                p.Add(":p7", string.Format("{0}", p7));
            }
            sql += " order by A.MMCODE,A.M_AGENNO";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BG0009>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        //印明細
        public IEnumerable<BG0009> ReportDL(string pDATA_YM, string[] arr_p0, string p1, string p2, string p2_1, string p3, string p4, string p5, string p6)
        {
            var p = new DynamicParameters();
            string mat_class = "''";
            string mat_class_sub = "''";
            for (int i = 0; i < arr_p0.Length; i++)
            {
                if (arr_p0[i].Contains("SUB_"))
                {
                    if (i == 0)
                        mat_class_sub = @" '" + arr_p0[i].Replace("SUB_", "") + "'";
                    else
                        mat_class_sub += @",'" + arr_p0[i].Replace("SUB_", "") + "'";
                }
                else
                {
                    if (i == 0)
                        mat_class = @" '" + arr_p0[i] + "'";
                    else
                        mat_class += @",'" + arr_p0[i] + "'";
                }
            }

            var sql = @"
                        with temp_qty as (
	                        select B.M_AGENNO as M_AGENNO, AGEN_NAME(B.M_AGENNO) as AGEN_NAME, A.MMCODE, sum(C.ACC_QTY) as ACC_QTY,
	                           (sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as EXTRA_DISC_AMOUNT,
	                           (sum(A.DELI_QTY * A.PO_PRICE)) as TOT_AMT_1,
	                           (sum(A.DELI_QTY * A.PO_PRICE)) - (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE))) + sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as TOT_AMT_2,
	                           (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE)))) as TOT_AMT_3
	                        from PH_INVOICE A, MI_MAST B, BC_CS_ACC_LOG C
	                        where A.MMCODE=B.MMCODE
	                        and C.SEQ=A.ACC_LOG_SEQ
	                        and C.ACC_TIME between (select SET_BTIME from MI_MNSET where SET_YM=:pDATA_YM) 
	                         and (select SET_CTIME from MI_MNSET where SET_YM=:pDATA_YM) 
	                        group by B.M_AGENNO, A.MMCODE
                        )
                        select A.MMCODE,A.MMNAME_C,A.BASE_UNIT,A.M_CONTPRICE,A.DISC_CPRICE,A.P_INV_QTY,nvl(temp_qty.ACC_QTY, 0) as IN_QTY,
                               nvl((select sum(PO_QTY) from PH_INVOICE where INVOICE_TYPE = '2' and DELI_STATUS = 'N' and MMCODE = A.MMCODE and DATA_YM = :pDATA_YM), 0) as REJ_OUTQTY,
                               A.MIL_QTY,A.OUT_QTY,(A.MIL_QTY+A.OUT_QTY) as T_OUT_QTY,A.INV_QTY,nvl(A.WRESQTY,0) as WRESQTY,
                               round(nvl(temp_qty.EXTRA_DISC_AMOUNT,0)) as EXTRA_DISC_AMOUNT,
                               round(nvl(temp_qty.TOT_AMT_1, 0)) as TOT_AMT_1,
	                           round(nvl(temp_qty.TOT_AMT_2, 0)) as TOT_AMT_2,
	                           round(nvl(temp_qty.TOT_AMT_3, 0)) as TOT_AMT_3,
                               DECODE(A.E_SOURCECODE,'P','買斷','C','寄售','') as E_SOURCECODE,A.M_AGENNO
                         from V_COST_MILTORY A, temp_qty, MI_MAST B
                         where A.MMCODE=B.MMCODE and A.E_SOURCECODE IN ('C','P') and B.M_CONTID in ('0','2')
                         and A.M_AGENNO = temp_qty.M_AGENNO and A.MMCODE = temp_qty.MMCODE ";

            if (pDATA_YM != "")
            {
                sql += @" and A.DATA_YM=:pDATA_YM";
                p.Add(":pDATA_YM", string.Format("{0}", pDATA_YM));
            }
            if (arr_p0.Length > 0)
            {
                sql += @" and (B.MAT_CLASS in (" + mat_class + ")" +
                    "           or B.MAT_CLASS_SUB in (" + mat_class_sub + ") )";
            }
            if (!string.IsNullOrWhiteSpace(p1))
            {
                sql += " and A.E_SOURCECODE=:p1";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p2))
            {
                if (!string.IsNullOrWhiteSpace(p2_1))
                {
                    sql += " and A.M_AGENNO between :p2 and :p2_1";
                    p.Add(":p2", string.Format("{0}", p2));
                    p.Add(":p2_1", string.Format("{0}", p2_1));
                }
                else
                {
                    sql += " and A.M_AGENNO=:p2";
                    p.Add(":p2", string.Format("{0}", p2));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(p2_1))
                {
                    sql += " and A.M_AGENNO=:p2_1";
                    p.Add(":p2_1", string.Format("{0}", p2_1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p3))
            {
                sql += " and B.M_CONTID=:p3";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (!string.IsNullOrWhiteSpace(p4))
            {
                sql += " and B.CASENO=:p4";
                p.Add(":p4", string.Format("{0}", p4));
            }
            if (!string.IsNullOrWhiteSpace(p5))
            {
                sql += " and AGEN_NAME(A.M_AGENNO) LIKE :p5";
                p.Add(":p5", string.Format("%{0}%", p5));
            }
            if (p6 == "true") //隱藏零庫存、未進出、無消耗之品項
            {
                sql += " and not (INV_QTY = 0 and IN_QTY=0 and OUT_QTY=0)";
            }
            sql += " order by A.MMCODE,A.M_AGENNO";

            return DBWork.Connection.Query<BG0009>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<BG0009> ReportG(string pDATA_YM, string[] arr_p0, string p1, string p2, string p2_1, string p3, string p4, string p5, string p6)
        {
            var p = new DynamicParameters();
            string mat_class = "''";
            string mat_class_sub = "''";
            for (int i = 0; i < arr_p0.Length; i++)
            {
                if (arr_p0[i].Contains("SUB_"))
                {
                    if (i == 0)
                        mat_class_sub = @" '" + arr_p0[i].Replace("SUB_", "") + "'";
                    else
                        mat_class_sub += @",'" + arr_p0[i].Replace("SUB_", "") + "'";
                }
                else
                {
                    if (i == 0)
                        mat_class = @" '" + arr_p0[i] + "'";
                    else
                        mat_class += @",'" + arr_p0[i] + "'";
                }
            }

            var sql = @"
                        with temp_qty as (
	                        select B.M_AGENNO as M_AGENNO, AGEN_NAME(B.M_AGENNO) as AGEN_NAME, A.MMCODE, sum(C.ACC_QTY) as ACC_QTY,
	                           (sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as EXTRA_DISC_AMOUNT,
	                           (sum(A.DELI_QTY * A.PO_PRICE)) as TOT_AMT_1,
	                           (sum(A.DELI_QTY * A.PO_PRICE)) - (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE))) + sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as TOT_AMT_2,
	                           (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE)))) as TOT_AMT_3
	                        from PH_INVOICE A, MI_MAST B, BC_CS_ACC_LOG C
	                        where A.MMCODE=B.MMCODE
	                        and C.SEQ=A.ACC_LOG_SEQ
	                        and C.ACC_TIME between (select SET_BTIME from MI_MNSET where SET_YM=:pDATA_YM) 
	                         and (select SET_CTIME from MI_MNSET where SET_YM=:pDATA_YM) 
	                        group by B.M_AGENNO, A.MMCODE
                        )
                        select A.MMCODE,A.MMNAME_C,A.BASE_UNIT,A.M_CONTPRICE,A.DISC_CPRICE,A.P_INV_QTY,nvl(temp_qty.ACC_QTY, 0) as IN_QTY,
                               nvl((select sum(PO_QTY) from PH_INVOICE where INVOICE_TYPE = '2' and DELI_STATUS = 'N' and MMCODE = A.MMCODE and DATA_YM = :pDATA_YM), 0) as REJ_OUTQTY,
                               A.MIL_QTY,A.OUT_QTY,(A.MIL_QTY+A.OUT_QTY) as T_OUT_QTY,A.INV_QTY,nvl(A.WRESQTY,0) as WRESQTY,
                               round(nvl(temp_qty.EXTRA_DISC_AMOUNT,0)) as EXTRA_DISC_AMOUNT,
	                           round(nvl(temp_qty.TOT_AMT_1, 0)) as TOT_AMT_1,
	                           round(nvl(temp_qty.TOT_AMT_2, 0)) as TOT_AMT_2,
	                           round(nvl(temp_qty.TOT_AMT_3, 0)) as TOT_AMT_3,
                               DECODE(A.E_SOURCECODE,'P','買斷','C','寄售','') as E_SOURCECODE,A.M_AGENNO,
                               (select EASYNAME from PH_VENDER where AGEN_NO=A.M_AGENNO)as EASYNAME
                         from V_COST_MILTORY A, temp_qty, MI_MAST B
                         where A.MMCODE=B.MMCODE and A.E_SOURCECODE IN ('C','P') and B.M_CONTID in ('0','2')
                         and A.M_AGENNO = temp_qty.M_AGENNO and A.MMCODE = temp_qty.MMCODE ";
            if (pDATA_YM != "")
            {
                sql += @" and A.DATA_YM=:pDATA_YM";
                p.Add(":pDATA_YM", string.Format("{0}", pDATA_YM));
            }
            if (arr_p0.Length > 0)
            {
                sql += @" and (B.MAT_CLASS in (" + mat_class + ")" +
                    "           or B.MAT_CLASS_SUB in (" + mat_class_sub + ") )";
            }
            if (!string.IsNullOrWhiteSpace(p1))
            {
                sql += " and A.E_SOURCECODE=:p1";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p2))
            {
                if (!string.IsNullOrWhiteSpace(p2_1))
                {
                    sql += " and A.M_AGENNO between :p2 and :p2_1";
                    p.Add(":p2", string.Format("{0}", p2));
                    p.Add(":p2_1", string.Format("{0}", p2_1));
                }
                else
                {
                    sql += " and A.M_AGENNO=:p2";
                    p.Add(":p2", string.Format("{0}", p2));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(p2_1))
                {
                    sql += " and A.M_AGENNO=:p2_1";
                    p.Add(":p2_1", string.Format("{0}", p2_1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p3))
            {
                sql += " and B.M_CONTID=:p3";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (!string.IsNullOrWhiteSpace(p4))
            {
                sql += " and B.CASENO=:p4";
                p.Add(":p4", string.Format("{0}", p4));
            }
            if (!string.IsNullOrWhiteSpace(p5))
            {
                sql += " and AGEN_NAME(A.M_AGENNO) LIKE :p5";
                p.Add(":p5", string.Format("%{0}%", p5));
            }
            if (p6 == "true") //隱藏零庫存、未進出、無消耗之品項
            {
                sql += " and not (INV_QTY = 0 and IN_QTY=0 and OUT_QTY=0)";
            }
            sql += " order by A.M_AGENNO,A.MMCODE";

            return DBWork.Connection.Query<BG0009>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<BG0009> ReportAGEN(string pDATA_YM, string[] arr_p0, string p1, string p2, string p2_1, string p3, string p4, string p5, string p6)
        {
            var p = new DynamicParameters();
            string mat_class = "''";
            string mat_class_sub = "''";
            for (int i = 0; i < arr_p0.Length; i++)
            {
                if (arr_p0[i].Contains("SUB_"))
                {
                    if (i == 0)
                        mat_class_sub = @" '" + arr_p0[i].Replace("SUB_", "") + "'";
                    else
                        mat_class_sub += @",'" + arr_p0[i].Replace("SUB_", "") + "'";
                }
                else
                {
                    if (i == 0)
                        mat_class = @" '" + arr_p0[i] + "'";
                    else
                        mat_class += @",'" + arr_p0[i] + "'";
                }
            }

            // 原本直接group by廠商可能會與其他報表group by院內碼再加總的結果有落差,這邊也統一為先group by院內碼再group by廠商加總
            var sql = @"
                select T.M_AGENNO, T.AGEN_NAME, round(sum(T.EXTRA_DISC_AMOUNT)) as EXTRA_DISC_AMOUNT, 
                        round(sum(TOT_AMT_1)) as TOT_AMT_1, round(sum(TOT_AMT_2)) as TOT_AMT_2, round(sum(TOT_AMT_3)) as TOT_AMT_3
                    from (
                        select B.M_AGENNO as M_AGENNO, AGEN_NAME(B.M_AGENNO) as AGEN_NAME, A.MMCODE, sum(C.ACC_QTY) as ACC_QTY,
                           (sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as EXTRA_DISC_AMOUNT,
                           (sum(A.DELI_QTY * A.PO_PRICE)) as TOT_AMT_1,
                           (sum(A.DELI_QTY * A.PO_PRICE)) - (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE))) + sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as TOT_AMT_2,
                           (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE)))) as TOT_AMT_3
                        from PH_INVOICE A, MI_MAST B, BC_CS_ACC_LOG C
                        where A.MMCODE=B.MMCODE
                        and C.SEQ=A.ACC_LOG_SEQ
                        and C.ACC_TIME between (select SET_BTIME from MI_MNSET where SET_YM=:pDATA_YM) 
                         and (select SET_CTIME from MI_MNSET where SET_YM=:pDATA_YM) 
                        and B.E_SOURCECODE IN ('C','P') and B.M_CONTID in ('0','2') 
            ";

            p.Add(":pDATA_YM", string.Format("{0}", pDATA_YM));

            if (arr_p0.Length > 0)
            {
                sql += @" and (B.MAT_CLASS in (" + mat_class + ")" +
                    "           or B.MAT_CLASS_SUB in (" + mat_class_sub + ") )";
            }
            if (!string.IsNullOrWhiteSpace(p1))
            {
                sql += " and B.E_SOURCECODE=:p1";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p2))
            {
                if (!string.IsNullOrWhiteSpace(p2_1))
                {
                    sql += " and B.M_AGENNO between :p2 and :p2_1";
                    p.Add(":p2", string.Format("{0}", p2));
                    p.Add(":p2_1", string.Format("{0}", p2_1));
                }
                else
                {
                    sql += " and B.M_AGENNO=:p2";
                    p.Add(":p2", string.Format("{0}", p2));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(p2_1))
                {
                    sql += " and B.M_AGENNO=:p2_1";
                    p.Add(":p2_1", string.Format("{0}", p2_1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p3))
            {
                sql += " and B.M_CONTID=:p3";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (!string.IsNullOrWhiteSpace(p4))
            {
                sql += " and B.CASENO=:p4";
                p.Add(":p4", string.Format("{0}", p4));
            }
            if (!string.IsNullOrWhiteSpace(p5))
            {
                sql += " and AGEN_NAME(B.M_AGENNO) LIKE :p5";
                p.Add(":p5", string.Format("%{0}%", p5));
            }
            //if (p6 == "true") //隱藏零庫存、未進出、無消耗之品項
            //{
            //    sql += "  and not (INV_QTY = 0 and IN_QTY=0 and OUT_QTY=0)";
            //}

            sql += @"
                        group by B.M_AGENNO, A.MMCODE
                    ) T
                    group by T.M_AGENNO, T.AGEN_NAME
                    order by T.M_AGENNO
            ";
            return DBWork.Connection.Query<BG0009>(sql, p, DBWork.Transaction);
        }

        public DataTable GetAgenExcel(string pDATA_YM, string[] arr_p0, string p1, string p2, string p2_1, string p3, string p4, string p5, string p6)
        {
            var p = new DynamicParameters();
            string mat_class = "''";
            string mat_class_sub = "''";
            for (int i = 0; i < arr_p0.Length; i++)
            {
                if (arr_p0[i].Contains("SUB_"))
                {
                    if (i == 0)
                        mat_class_sub = @" '" + arr_p0[i].Replace("SUB_", "") + "'";
                    else
                        mat_class_sub += @",'" + arr_p0[i].Replace("SUB_", "") + "'";
                }
                else
                {
                    if (i == 0)
                        mat_class = @" '" + arr_p0[i] + "'";
                    else
                        mat_class += @",'" + arr_p0[i] + "'";
                }
            }

            // 原本直接group by廠商可能會與其他報表group by院內碼再加總的結果有落差,這邊也統一為先group by院內碼再group by廠商加總
            var sql = @"
                select ROW_NUMBER() OVER(ORDER BY T.M_AGENNO) as 項次, T.M_AGENNO as 廠商代碼, T.AGEN_NAME as 廠商名稱, round(sum(TOT_AMT_1)) as 應付總額, 
                         round(sum(T.EXTRA_DISC_AMOUNT)) as 折讓金額, round(sum(TOT_AMT_3)) as 聯標契約優惠, round(sum(TOT_AMT_2)) as 實付總價
                    from (
                        select B.M_AGENNO as M_AGENNO, AGEN_NAME(B.M_AGENNO) as AGEN_NAME, A.MMCODE, sum(C.ACC_QTY) as ACC_QTY,
                           (sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as EXTRA_DISC_AMOUNT,
                           (sum(A.DELI_QTY * A.PO_PRICE)) as TOT_AMT_1,
                           (sum(A.DELI_QTY * A.PO_PRICE)) - (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE))) + sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as TOT_AMT_2,
                           (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE)))) as TOT_AMT_3
                        from PH_INVOICE A, MI_MAST B, BC_CS_ACC_LOG C
                        where A.MMCODE=B.MMCODE
                        and C.SEQ=A.ACC_LOG_SEQ
                        and C.ACC_TIME between (select SET_BTIME from MI_MNSET where SET_YM=:pDATA_YM) 
                         and (select SET_CTIME from MI_MNSET where SET_YM=:pDATA_YM) 
                        and B.E_SOURCECODE IN ('C','P') and B.M_CONTID in ('0','2') 
            ";

            p.Add(":pDATA_YM", string.Format("{0}", pDATA_YM));

            if (arr_p0.Length > 0)
            {
                sql += @" and (B.MAT_CLASS in (" + mat_class + ")" +
                    "           or B.MAT_CLASS_SUB in (" + mat_class_sub + ") )";
            }
            if (!string.IsNullOrWhiteSpace(p1))
            {
                sql += " and B.E_SOURCECODE=:p1";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (!string.IsNullOrWhiteSpace(p2))
            {
                if (!string.IsNullOrWhiteSpace(p2_1))
                {
                    sql += " and B.M_AGENNO between :p2 and :p2_1";
                    p.Add(":p2", string.Format("{0}", p2));
                    p.Add(":p2_1", string.Format("{0}", p2_1));
                }
                else
                {
                    sql += " and B.M_AGENNO=:p2";
                    p.Add(":p2", string.Format("{0}", p2));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(p2_1))
                {
                    sql += " and B.M_AGENNO=:p2_1";
                    p.Add(":p2_1", string.Format("{0}", p2_1));
                }
            }
            if (!string.IsNullOrWhiteSpace(p3))
            {
                sql += " and B.M_CONTID=:p3";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (!string.IsNullOrWhiteSpace(p4))
            {
                sql += " and B.CASENO=:p4";
                p.Add(":p4", string.Format("{0}", p4));
            }
            if (!string.IsNullOrWhiteSpace(p5))
            {
                sql += " and AGEN_NAME(B.M_AGENNO) LIKE :p5";
                p.Add(":p5", string.Format("%{0}%", p5));
            }
            //if (p6 == "true") //隱藏零庫存、未進出、無消耗之品項
            //{
            //    sql += "  and not (INV_QTY = 0 and IN_QTY=0 and OUT_QTY=0)";
            //}

            sql += @"
                        group by B.M_AGENNO, A.MMCODE
                    ) T
                    group by T.M_AGENNO, T.AGEN_NAME
                    order by T.M_AGENNO
            ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public int ChkPrintData(string pDATA_YM, string agenno)
        {
            var p = new DynamicParameters();
            var sql = @"
                        with temp_qty as (
	                        select B.M_AGENNO, A.MMCODE
	                        from PH_INVOICE A, MI_MAST B, BC_CS_ACC_LOG C
	                        where A.MMCODE=B.MMCODE
	                        and C.SEQ=A.ACC_LOG_SEQ
	                        and C.ACC_TIME between (select SET_BTIME from MI_MNSET where SET_YM=:pDATA_YM) 
	                         and (select SET_CTIME from MI_MNSET where SET_YM=:pDATA_YM) 
                        )
                        select count(*) from V_COST_MILTORY A, MI_MAST B, temp_qty
                        where A.MMCODE=B.MMCODE and A.E_SOURCECODE IN ('C','P') and B.M_CONTID in ('0','2') and IN_QTY>0
                        and A.M_AGENNO = temp_qty.M_AGENNO and A.MMCODE = temp_qty.MMCODE ";

            if (pDATA_YM != "")
            {
                sql += @" and A.DATA_YM=:pDATA_YM";
                p.Add(":pDATA_YM", string.Format("{0}", pDATA_YM));
            }

            if (!string.IsNullOrWhiteSpace(agenno))
            {
                sql += " and A.M_AGENNO=:agenno";
                p.Add(":agenno", string.Format("{0}", agenno));
            }
            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }
        public DataTable GetReportMain(string agenno)
        {
            var p = new DynamicParameters();
            var sql = @"select AGEN_NO as M_AGENNO,AGEN_NAMEC as AGEN_NAME,AGEN_TEL,AGEN_FAX from PH_VENDER where AGEN_NO=:agenno";
            p.Add(":agenno", agenno);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<BG0009> GetPrintData(string pDATA_YM, string agenno)
        {
            var p = new DynamicParameters();

            var sql = @"
                        with temp_qty as (
	                        select B.M_AGENNO as M_AGENNO, AGEN_NAME(B.M_AGENNO) as AGEN_NAME, A.MMCODE, sum(C.ACC_QTY) as ACC_QTY,
	                           (sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as EXTRA_DISC_AMOUNT,
	                           (sum(A.DELI_QTY * A.PO_PRICE)) as TOT_AMT_1,
	                           (sum(A.DELI_QTY * A.PO_PRICE)) - (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE))) + sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as TOT_AMT_2,
	                           (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE)))) as TOT_AMT_3
	                        from PH_INVOICE A, MI_MAST B, BC_CS_ACC_LOG C
	                        where A.MMCODE=B.MMCODE
	                        and C.SEQ=A.ACC_LOG_SEQ
	                        and C.ACC_TIME between (select SET_BTIME from MI_MNSET where SET_YM=:pDATA_YM) 
	                         and (select SET_CTIME from MI_MNSET where SET_YM=:pDATA_YM) 
	                        group by B.M_AGENNO, A.MMCODE
                        )
                        select A.MMCODE,A.MMNAME_C,A.BASE_UNIT,A.M_CONTPRICE,A.DISC_CPRICE,A.P_INV_QTY,nvl(temp_qty.ACC_QTY, 0) as IN_QTY,
                               nvl((select sum(PO_QTY) from PH_INVOICE where INVOICE_TYPE = '2' and DELI_STATUS = 'N' and MMCODE = A.MMCODE and DATA_YM = :pDATA_YM), 0) as REJ_OUTQTY,
                               A.MIL_QTY,A.OUT_QTY,(A.MIL_QTY+A.OUT_QTY) as T_OUT_QTY,A.INV_QTY,nvl(A.WRESQTY,0) as WRESQTY,
                               round(nvl(temp_qty.EXTRA_DISC_AMOUNT,0)) as EXTRA_DISC_AMOUNT,
	                           round(nvl(temp_qty.TOT_AMT_1, 0)) as TOT_AMT_1,
	                           round(nvl(temp_qty.TOT_AMT_2, 0)) as TOT_AMT_2,
	                           round(nvl(temp_qty.TOT_AMT_3, 0)) as TOT_AMT_3,
                               DECODE(A.E_SOURCECODE,'P','買斷','C','寄售','') as E_SOURCECODE,
                               (select listagg(PO_NO,',') within group (order by D.MMCODE) from BC_CS_ACC_LOG D
                                 where D.MMCODE=A.MMCODE and D.AGEN_NO=A.M_AGENNO and TWN_YYYMM(D.ACC_TIME)=A.DATA_YM) as PO_NO
                         from V_COST_MILTORY A, temp_qty, MI_MAST B
                         where A.MMCODE=B.MMCODE and A.E_SOURCECODE IN ('C','P') and B.M_CONTID in ('0','2') and temp_qty.ACC_QTY > 0 
                         and A.M_AGENNO = temp_qty.M_AGENNO and A.MMCODE = temp_qty.MMCODE ";

            if (pDATA_YM != "")
            {
                sql += @" and A.DATA_YM=:pDATA_YM";
                p.Add(":pDATA_YM", string.Format("{0}", pDATA_YM));
            }

            if (!string.IsNullOrWhiteSpace(agenno))
            {
                sql += " and A.M_AGENNO=:agenno";
                p.Add(":agenno", string.Format("{0}", agenno));
            }

            sql += " order by A.M_AGENNO";

            return DBWork.Connection.Query<BG0009>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<BG0009Count> GetPrintDetails(string pDATA_YM, string agenno)
        {
            var p = new DynamicParameters();

            var sql = @"
                        select 0 as M_TOT1, 0 as P_TOT1, 0 as M_TOT2, 
                                round(sum(T.EXTRA_DISC_AMOUNT)) as EXTRA_DISC_AMOUNT, 
                                round(sum(T.TOT_AMT_1)) as P_TOT2, round(sum(T.TOT_AMT_3)) as TOT_AMT_3, 
                                round(sum(T.TOT_AMT_2)) as P_TOT3, round(sum(T.TOT_AMT_2)) as P_TOT4,
                                (select data_value from PARAM_D where grp_code='BG0009' and data_name='MEMO') as memo
                            from (
                                select B.M_AGENNO as M_AGENNO, AGEN_NAME(B.M_AGENNO) as AGEN_NAME, A.MMCODE, sum(C.ACC_QTY) as ACC_QTY,
                                   (sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as EXTRA_DISC_AMOUNT,
                                   (sum(A.DELI_QTY * A.PO_PRICE)) as TOT_AMT_1,
                                   (sum(A.DELI_QTY * A.PO_PRICE)) - (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE))) + sum(nvl(A.EXTRA_DISC_AMOUNT, 0))) as TOT_AMT_2,
                                   (sum(A.DELI_QTY * (A.PO_PRICE - nvl(A.DISC_CPRICE,A.PO_PRICE)))) as TOT_AMT_3
                                from PH_INVOICE A, MI_MAST B, BC_CS_ACC_LOG C
                                where A.MMCODE=B.MMCODE
                                and C.SEQ=A.ACC_LOG_SEQ
                                and C.ACC_TIME between (select SET_BTIME from MI_MNSET where SET_YM=:pDATA_YM) 
                                 and (select SET_CTIME from MI_MNSET where SET_YM=:pDATA_YM) 
                                and B.E_SOURCECODE IN ('C','P') and B.M_CONTID in ('0','2') 
	                         ";

            p.Add(":pDATA_YM", string.Format("{0}", pDATA_YM));

            if (!string.IsNullOrWhiteSpace(agenno))
            {
                sql += " and B.M_AGENNO=:agenno";
                p.Add(":agenno", string.Format("{0}", agenno));
            }

            sql += @"        group by B.M_AGENNO, A.MMCODE
                        ) T
                        group by T.M_AGENNO, T.AGEN_NAME
                        order by T.M_AGENNO ";

            return DBWork.Connection.Query<BG0009Count>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @" SELECT MAT_CLASS AS VALUE, '全部' || MAT_CLSNAME AS TEXT, 
                                   MAT_CLASS || ' ' || '全部' || MAT_CLSNAME AS COMBITEM
                            FROM MI_MATCLASS WHERE  mat_clsid in ('1','2')
                            union
                            select 'SUB_' || DATA_VALUE as VALUE, DATA_DESC as TEXT,
                            DATA_VALUE || ' ' || DATA_DESC AS COMBITEM
                            from PARAM_D
	                        where GRP_CODE ='MI_MAST' 
	                        and DATA_NAME = 'MAT_CLASS_SUB'
	                        and trim(DATA_DESC) is not null
                            ORDER BY VALUE";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, null);
        }
        public IEnumerable<COMBO_MODEL> GetSourcecodeCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                                  DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                             FROM PARAM_D
                            WHERE GRP_CODE='MI_MAST' AND DATA_NAME='E_SOURCECODE' and DATA_VALUE in ('C','P')
                            ORDER BY DATA_VALUE";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, null);
        }
        public IEnumerable<COMBO_MODEL> GetMcontidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                                  DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                             FROM PARAM_D
                            WHERE GRP_CODE='MM_PO_M' AND DATA_NAME='M_CONTID' and DATA_VALUE in ('0','2')
                            ORDER BY DATA_VALUE";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, null);
        }
        public IEnumerable<PH_VENDER> GetAgennoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} AGEN_NO, AGEN_NAMEC, AGEN_NAMEE, AGEN_NO||' '||AGEN_NAMEC as EASYNAME
                            from PH_VENDER where (REC_STATUS <> 'X' or REC_STATUS is null) ";

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

        public IEnumerable<PH_VENDER> GetPhVenderUniNoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} UNI_NO, AGEN_NAMEC, AGEN_NAMEE, AGEN_NO||' '||AGEN_NAMEC as EASYNAME
                            from PH_VENDER where (REC_STATUS <> 'X' or REC_STATUS is null) ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UNI_NO, :UNI_NO_I), 1000) + NVL(INSTR(AGEN_NAMEC, :AGEN_NAMEC_I), 100) * 10 + NVL(INSTR(AGEN_NAMEE, :AGEN_NAMEE_I), 100) * 10) IDX,");
                p.Add(":UNI_NO_I", p0);
                p.Add(":AGEN_NAMEC_I", p0);
                p.Add(":AGEN_NAMEE_I", p0);

                sql += " AND (UNI_NO LIKE :UNI_NO ";
                p.Add(":UNI_NO", string.Format("{0}%", p0));

                sql += " OR AGEN_NAMEC LIKE :AGEN_NAMEC ";
                p.Add(":AGEN_NAMEC", string.Format("%{0}%", p0));

                sql += " OR AGEN_NAMEE LIKE :AGEN_NAMEE) ";
                p.Add(":AGEN_NAMEE", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY UNI_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int ChkDATA_YM(string data_ym)
        {
            var p = new DynamicParameters();
            string sql = @"select 1 as CNT from MI_WINVMON
                            where DATA_YM=:DATA_YM and rownum=1";
            p.Add(":DATA_YM", data_ym);
            int rtn = DBWork.Connection.ExecuteScalar<int>(sql, p, DBWork.Transaction);
            return rtn;
        }

        public string GetHospName()
        {
            string sql = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospName' ";
            return DBWork.Connection.ExecuteScalar<string>(sql, DBWork.Transaction);
        }
        public string GetPH_VENDER_MEMO(string agenno, string agenno1)
        {
            var p = new DynamicParameters();
            string sql = @" SELECT MEMO FROM PH_VENDER where trim(MEMO) is not null and rownum=1";
            if (agenno != "")
            {
                if (agenno1 != "")
                {
                    sql += " and AGEN_NO between :agenno and :agenno1";
                    p.Add(":agenno", agenno);
                    p.Add(":agenno1", agenno1);
                }
                else
                {
                    sql += " and AGEN_NO=:agenno";
                    p.Add(":agenno", agenno);
                }
            }
            else
            {
                if (agenno1 != "")
                {
                    sql += " and AGEN_NO=:agenno1";
                    p.Add(":agenno1", agenno1);
                }
            }
            sql += " order by AGEN_NO";
            return DBWork.Connection.ExecuteScalar<string>(sql, p, DBWork.Transaction);
        }
        public int SavePH_VENDER_MEMO(string agenno, string agenno1, string note)
        {
            var p = new DynamicParameters();
            var sql = @"update PH_VENDER set MEMO=:note where 1=1";
            p.Add(":note", note);
            if (agenno != "")
            {
                if (agenno1 != "")
                {
                    sql += " and AGEN_NO between :agenno and :agenno1";
                    p.Add(":agenno", agenno);
                    p.Add(":agenno1", agenno1);
                }
                else
                {
                    sql += " and AGEN_NO=:agenno";
                    p.Add(":agenno", agenno);
                }
            }
            else
            {
                if (agenno1 != "")
                {
                    sql += " and AGEN_NO=:agenno1";
                    p.Add(":agenno1", agenno1);
                }
            }
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
        public DataTable GetMailPhvenderData(string agenno, string agenno1)
        {
            var p = new DynamicParameters();
            string sql = @"select AGEN_NO,EMAIL,MEMO from PH_VENDER where 1=1";
            if (agenno != "")
            {
                if (agenno1 != "")
                {
                    sql += " and AGEN_NO between :agenno and :agenno1";
                    p.Add(":agenno", agenno);
                    p.Add(":agenno1", agenno1);
                }
                else
                {
                    sql += " and AGEN_NO=:agenno";
                    p.Add(":agenno", agenno);
                }
            }
            else
            {
                if (agenno1 != "")
                {
                    sql += " and AGEN_NO=:agenno1";
                    p.Add(":agenno1", agenno1);
                }
            }
            sql += " order by AGEN_NO,MEMO ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string chkMail(string strMail)
        {
            string sql = @" select EMAIL from PH_VENDER where EMAIL = :MAIL ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { MAIL = strMail }, DBWork.Transaction);
        }

        public string chkSubject(string strSubject)
        {
            string sql = @" SELECT :SUBJECT FROM dual ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { SUBJECT = strSubject }, DBWork.Transaction);
        }

        public string chkContent(string strContent)
        {
            string sql = @" SELECT :CONTENT FROM dual ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CONTENT = strContent }, DBWork.Transaction);
        }

        public bool CheckParamDMemoExists()
        {
            string sql = @"
                select 1 from PARAM_D where grp_code='BG0009' and data_name='MEMO'
            ";
            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction) != null;
        }
        public bool CheckParamMBG0009Exists()
        {
            string sql = @"
                select 1 from PARAM_M where grp_code='BG0009'
            ";
            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction) != null;
        }

        public int InsertParamDMEMO()
        {
            string sql = @"
                insert into PARAM_D (grp_code, data_seq, data_name, data_value)
                values ('BG0009',(select nvl(max(data_seq), 1) from PARAM_D where grp_code='BG0009'),'MEMO','')
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
        public int InsertParamMBG0009()
        {
            string sql = @"
                insert into PARAM_M (grp_code, grp_desc)
                values ('BG0009', '貨款對帳單')
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
        public string GetParamDMemo()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code = 'BG0009' and data_name='MEMO'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public int UpdateParamDMemo(string memo)
        {
            string sql = @"
                update PARAM_D set data_value = :memo
                where grp_code='BG0009' and data_name='MEMO'
            ";
            return DBWork.Connection.Execute(sql, new { memo }, DBWork.Transaction);
        }
    }
}