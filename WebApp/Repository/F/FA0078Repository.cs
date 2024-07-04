using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using JCLib.DB;
using System.Data;
using WebApp.Models;
using WebApp.Models.F;

namespace WebApp.Repository.F
{
    public class FA0078Repository : JCLib.Mvc.BaseRepository
    {
        public FA0078Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0078> GetAll(string MAT_CLASS, string ExportType, string YYYMM, string IsTCB, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select T.AGEN_NO, T.AGEN_NAMEC, T.AGEN_ACC, 
                        nvl(sum(T.TOT_AMT), 0) as TOT_AMT, nvl(sum(T.TOT_AMT_1), 0) as TOT_AMT_1, sum(T.TOT_AMT_2) as TOT_AMT_2, sum(T.TOT_AMT_3) as TOT_AMT_3,
                        nvl(sum(T.TOT_AMT), 0) - nvl(sum(T.TOT_AMT_1), 0) - sum(T.TOT_AMT_2) - sum(T.TOT_AMT_3) as TOT_AMT_4
                        from (
                            select A.AGEN_NO, C.AGEN_NAMEC, C.AGEN_ACC,
                            sum(floor(A.ACC_PO_PRICE * A.TX_QTY_T )) as TOT_AMT,
                            round(sum(A.ACC_PO_PRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) as TOT_AMT_1,
                            (case when A.ACC_ISWILLING='是' and A.PO_QTY >= A.ACC_DISCOUNT_QTY
						       then round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_COST_UPRICE * A.TX_QTY_T))
						       else 0
					        end) as TOT_AMT_2,
                            sum(EXTRA_DISC_AMOUNT) as TOT_AMT_3
                            from BC_CS_ACC_LOG A, MI_MAST B, PH_VENDER C
                            where A.MMCODE=B.MMCODE and A.AGEN_NO=C.AGEN_NO 
                            and substr(A.PO_NO,1,3) NOT IN ('TXT') 
                            and A.AGEN_NO <> '999'
                            and A.STATUS='P'
                            and A.MAT_CLASS = :MAT_CLASS
                            and TWN_YYYMM(A.ACC_TIME) = :YYYMM  ";

            if (ExportType == "N")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('1','2','3','01','02','03')";
            else if(ExportType == "Y")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('0Y', '0N', 'X')";

            if (IsTCB == "0")  //合庫
                sql += " and C.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and C.AGEN_BANK <> '006' ";

            sql += @"       group by A.AGEN_NO, C.AGEN_NAMEC, C.AGEN_ACC, A.ACC_ISWILLING, A.PO_QTY, A.ACC_DISCOUNT_QTY
                        ) T
                        group by T.AGEN_NO, T.AGEN_NAMEC, T.AGEN_ACC
                        order by T.AGEN_NO ";

            p.Add(":MAT_CLASS", MAT_CLASS);
            p.Add(":YYYMM", YYYMM);
            p.Add(":ExportType", ExportType);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0078>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string MAT_CLASS, string ExportType, string YYYMM, string IsTCB)
        {
            var p = new DynamicParameters();

            var sql = @"select T.AGEN_NO as 廠商編號, T.AGEN_NAMEC as 廠商名稱, T.AGEN_ACC as 銀行帳號, 
                        nvl(sum(T.TOT_AMT), 0) as 應付總價, nvl(sum(T.TOT_AMT_1), 0) as 折讓金額, sum(T.TOT_AMT_2) as 聯標契約優惠, sum(T.TOT_AMT_3) as 額外折讓金額,
                        nvl(sum(T.TOT_AMT), 0) - nvl(sum(T.TOT_AMT_1), 0) - sum(T.TOT_AMT_2) - sum(T.TOT_AMT_3) as 實付總價
                        from (
                            select A.AGEN_NO, C.AGEN_NAMEC, C.AGEN_ACC,
                            sum(floor(A.ACC_PO_PRICE * A.TX_QTY_T )) as TOT_AMT,
                            round(sum(A.ACC_PO_PRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) as TOT_AMT_1,
                            (case when A.ACC_ISWILLING='是' and A.PO_QTY >= A.ACC_DISCOUNT_QTY
						       then round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_COST_UPRICE * A.TX_QTY_T))
						       else 0
					        end) as TOT_AMT_2,
                            sum(EXTRA_DISC_AMOUNT) as TOT_AMT_3
                            from BC_CS_ACC_LOG A, MI_MAST B, PH_VENDER C
                            where A.MMCODE=B.MMCODE and A.AGEN_NO=C.AGEN_NO 
                            and substr(A.PO_NO,1,3) NOT IN ('TXT') 
                            and A.AGEN_NO <> '999'
                            and A.STATUS='P'
                            and A.MAT_CLASS = :MAT_CLASS
                            and TWN_YYYMM(A.ACC_TIME) = :YYYMM  ";

            if (ExportType == "N")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('1','2','3','01','02','03')";
            else if (ExportType == "Y")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('0Y', '0N', 'X')";

            if (IsTCB == "0")  //合庫
                sql += " and C.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and C.AGEN_BANK <> '006' ";

            sql += @"       group by A.AGEN_NO, C.AGEN_NAMEC, C.AGEN_ACC, A.ACC_ISWILLING, A.PO_QTY, A.ACC_DISCOUNT_QTY
                        ) T
                        group by T.AGEN_NO, T.AGEN_NAMEC, T.AGEN_ACC
                        order by T.AGEN_NO ";

            p.Add(":MAT_CLASS", MAT_CLASS);
            p.Add(":YYYMM", YYYMM);
            p.Add(":ExportType", ExportType);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ComboItemModel> Mat_ClassComboGet(string userid) 
        {
            var sql = @"with temp_whkinds as (
                            select b.wh_no, b.wh_kind, 
                            nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as task_id
                            from MI_WHID a, MI_WHMAST b
                            where wh_userid = :userId
                            and a.wh_no = b.wh_no
                            and b.wh_grade = '1'
                            and a.task_id in ('1','2','3')
                        )
                        select distinct b.mat_class as VALUE, b.mat_clsname as TEXT, b.mat_class || ' ' ||  b.mat_clsname as COMBITEM 
                        from temp_whkinds a, MI_MATCLASS b
                        where (a.task_id = b.mat_clsid)
                        union
                        select distinct b.mat_class as VALUE, b.mat_clsname as TEXT, b.mat_class || ' ' ||  b.mat_clsname as COMBITEM  
                        from temp_whkinds a, MI_MATCLASS b
                        where (a.task_id = '2')
                        and b.mat_clsid = '3'
                        ";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { userId = userid });

        }

        public string GetClsname(string mat_class)
        {
            var sql = @"select MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = :MAT_CLASS ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { MAT_CLASS = mat_class }, DBWork.Transaction);
        }
        public DataTable GetTxtData(string MATCLS, string ContractType, string YearMonth, string IsTCB)
        {
            var p = new DynamicParameters();

            var sql = @"select T.AGEN_ACC as 銀行帳號, 
            nvl(sum(T.TOT_AMT), 0) - nvl(sum(T.TOT_AMT_1), 0) - sum(T.TOT_AMT_2) - sum(T.TOT_AMT_3) as 匯款金額,
            T.AGEN_BANK as 銀行代碼, T.AGEN_SUB as 分行代碼, T.UNI_NO as 統一編號, T.AGEN_NAMEC as 廠商名稱,
            T.AGEN_NO
			from (
				select A.AGEN_NO, C.AGEN_NAMEC, C.AGEN_BANK, C.AGEN_SUB, C.UNI_NO, C.AGEN_ACC, 
				sum(floor(A.ACC_PO_PRICE * A.TX_QTY_T )) as TOT_AMT,
				round(sum(A.ACC_PO_PRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) as TOT_AMT_1,
				(case when A.ACC_ISWILLING='是' and A.PO_QTY >= A.ACC_DISCOUNT_QTY
				   then round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_COST_UPRICE * A.TX_QTY_T))
				   else 0
				end) as TOT_AMT_2,
				sum(EXTRA_DISC_AMOUNT) as TOT_AMT_3
				from BC_CS_ACC_LOG A, MI_MAST B, PH_VENDER C
				where A.MMCODE=B.MMCODE and A.AGEN_NO=C.AGEN_NO 
				and substr(A.PO_NO,1,3) NOT IN ('TXT') 
				and A.AGEN_NO <> '999'
				and A.STATUS='P'
				and A.MAT_CLASS = :MAT_CLASS
				and TWN_YYYMM(A.ACC_TIME) = :YYYMM  ";

            if (ContractType == "N")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('1','2','3','01','02','03')";
            else if (ContractType == "Y")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('0Y', '0N', 'X')";

            if (IsTCB == "0")  //合庫
                sql += " and C.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and C.AGEN_BANK <> '006' ";

            sql += @"       group by A.AGEN_NO, C.AGEN_NAMEC, C.AGEN_BANK, C.AGEN_SUB, C.UNI_NO, C.AGEN_ACC, A.ACC_ISWILLING, A.PO_QTY, A.ACC_DISCOUNT_QTY
			) T
			group by T.AGEN_NO, T.AGEN_NAMEC, T.AGEN_ACC, T.AGEN_BANK, T.AGEN_SUB, T.UNI_NO
			order by T.AGEN_NO ";

            p.Add(":MAT_CLASS", MATCLS);
            p.Add(":YYYMM", YearMonth);

            sql = string.Format(@"select * 
                                    from ( {0} )
                                   where 匯款金額 <> 0", sql);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<PH_BANK_FEE> GetPhBankFee()
        {
            string sql = @"select * from PH_BANK_FEE
                            order by cashfrom";

            return DBWork.Connection.Query<PH_BANK_FEE>(sql, DBWork.Transaction);
        }

        public IEnumerable<FA0078> Report(string MAT_CLASS, string ExportType, string YYYMM, string IsTCB)
        {
            var p = new DynamicParameters();

            var sql = @"select T.AGEN_NO, T.AGEN_NAMEC, T.AGEN_ISLOCAL, T.AGEN_ACC, 
			nvl(sum(T.TOT_AMT), 0) as TOT_AMT, nvl(sum(T.TOT_AMT_1), 0) as TOT_AMT_1, sum(T.TOT_AMT_2) as TOT_AMT_2, sum(T.TOT_AMT_3) as TOT_AMT_3,
			nvl(sum(T.TOT_AMT), 0) - nvl(sum(T.TOT_AMT_1), 0) - sum(T.TOT_AMT_2) - sum(T.TOT_AMT_3) as TOT_AMT_4
			from (
				select A.AGEN_NO, C.AGEN_NAMEC, (case when C.AGEN_BANK = '006' then '是' else '否' end) as AGEN_ISLOCAL, C.AGEN_ACC, 
				sum(floor(A.ACC_PO_PRICE * A.TX_QTY_T )) as TOT_AMT,
				round(sum(A.ACC_PO_PRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) as TOT_AMT_1,
				(case when A.ACC_ISWILLING='是' and A.PO_QTY >= A.ACC_DISCOUNT_QTY
				   then round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_COST_UPRICE * A.TX_QTY_T))
				   else 0
				end) as TOT_AMT_2,
				sum(EXTRA_DISC_AMOUNT) as TOT_AMT_3
				from BC_CS_ACC_LOG A, MI_MAST B, PH_VENDER C
				where A.MMCODE=B.MMCODE and A.AGEN_NO=C.AGEN_NO 
				and substr(A.PO_NO,1,3) NOT IN ('TXT') 
				and A.AGEN_NO <> '999'
				and A.STATUS='P'
				and A.MAT_CLASS = :MAT_CLASS
				and TWN_YYYMM(A.ACC_TIME) = :YYYMM  ";

            if (ExportType == "N")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('1','2','3','01','02','03')";
            else if (ExportType == "Y")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('0Y', '0N', 'X')";

            if (IsTCB == "0")  //合庫
                sql += " and C.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and C.AGEN_BANK <> '006' ";

            sql += @"       group by A.AGEN_NO, C.AGEN_NAMEC, C.AGEN_BANK, C.AGEN_ACC, A.ACC_ISWILLING, A.PO_QTY, A.ACC_DISCOUNT_QTY
			) T
			group by T.AGEN_NO, T.AGEN_NAMEC, T.AGEN_ISLOCAL, T.AGEN_ACC
			order by T.AGEN_NO ";

            p.Add(":MAT_CLASS", MAT_CLASS);
            p.Add(":YYYMM", YYYMM);
            p.Add(":ExportType", ExportType);

            return DBWork.Connection.Query<FA0078>(sql, p, DBWork.Transaction);
        }

        public DataTable GetReportSumValue(string MAT_CLASS, string ExportType, string YYYMM, string IsTCB)
        {
            var p = new DynamicParameters();

            var sql = @"select 
			nvl(sum(T.TOT_AMT), 0) as TOT_AMT, nvl(sum(T.TOT_AMT_1), 0) as TOT_AMT_1, sum(T.TOT_AMT_2) as TOT_AMT_2, sum(T.TOT_AMT_3) as TOT_AMT_3,
			nvl(sum(T.TOT_AMT), 0) - nvl(sum(T.TOT_AMT_1), 0) - sum(T.TOT_AMT_2) - sum(T.TOT_AMT_3) as TOT_AMT_4,
            sum(T.CNT) as CNT
			from (
				select 1 as CNT, A.AGEN_NO, C.AGEN_NAMEC, C.AGEN_ACC,
				sum(floor(A.ACC_PO_PRICE * A.TX_QTY_T )) as TOT_AMT,
				round(sum(A.ACC_PO_PRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) as TOT_AMT_1,
				(case when A.ACC_ISWILLING='是' and A.PO_QTY >= A.ACC_DISCOUNT_QTY
				   then round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_COST_UPRICE * A.TX_QTY_T))
				   else 0
				end) as TOT_AMT_2,
				sum(EXTRA_DISC_AMOUNT) as TOT_AMT_3
				from BC_CS_ACC_LOG A, MI_MAST B, PH_VENDER C
				where A.MMCODE=B.MMCODE and A.AGEN_NO=C.AGEN_NO 
				and substr(A.PO_NO,1,3) NOT IN ('TXT') 
				and A.AGEN_NO <> '999'
				and A.STATUS='P'
				and A.MAT_CLASS = :MAT_CLASS
				and TWN_YYYMM(A.ACC_TIME) = :YYYMM  ";

            if (ExportType == "N")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('1','2','3','01','02','03')";
            else if (ExportType == "Y")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('0Y', '0N', 'X')";

            if (IsTCB == "0")  //合庫
                sql += " and C.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and C.AGEN_BANK <> '006' ";

            sql += @"       group by A.AGEN_NO, C.AGEN_NAMEC, C.AGEN_ACC, A.ACC_ISWILLING, A.PO_QTY, A.ACC_DISCOUNT_QTY
			) T ";

            p.Add(":MAT_CLASS", MAT_CLASS);
            p.Add(":YYYMM", YYYMM);
            p.Add(":ExportType", ExportType);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable ReportDetail(string MAT_CLASS, string ExportType, string YYYMM, string AGEN_NO, string IsTCB)
        {
            var p = new DynamicParameters();

            var sql = @"
				select A.PO_NO, A.AGEN_NO, A.MMCODE, B.MMNAME_C, C.AGEN_NAMEC, C.AGEN_ACC, A.ACC_PURUN as M_PURUN,
                A.ACC_PO_PRICE as PO_PRICE,
                sum(floor(A.TX_QTY_T)) as DELI_QTY,
				nvl(sum(floor(A.ACC_PO_PRICE * A.TX_QTY_T )), 0) as PO_AMT,
				nvl(round(sum(A.ACC_PO_PRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)), 0) as DISC_AMT,
				(case when A.ACC_ISWILLING='是' and A.PO_QTY >= A.ACC_DISCOUNT_QTY
				   then round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_COST_UPRICE * A.TX_QTY_T))
				   else 0
				end) as WILL_DISC_AMT,
				sum(EXTRA_DISC_AMOUNT) as EXTRA_DISC_AMOUNT
				from BC_CS_ACC_LOG A, MI_MAST B, PH_VENDER C
				where A.MMCODE=B.MMCODE and A.AGEN_NO=C.AGEN_NO 
				and substr(A.PO_NO,1,3) NOT IN ('TXT') 
				and A.AGEN_NO <> '999'
				and A.STATUS='P'
				and A.MAT_CLASS = :MAT_CLASS
				and TWN_YYYMM(A.ACC_TIME) = :YYYMM  ";

            if (ExportType == "N")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('1','2','3','01','02','03')";
            else if (ExportType == "Y")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('0Y', '0N', 'X')";

            if (AGEN_NO != "")
                sql += " and A.AGEN_NO=:AGEN_NO ";

            if (IsTCB == "0")  //合庫
                sql += " and C.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and C.AGEN_BANK <> '006' ";

            sql += @"       group by A.PO_NO, A.AGEN_NO, A.MMCODE, B.MMNAME_C,C.AGEN_NAMEC, C.AGEN_ACC, A.ACC_PURUN, A.ACC_PO_PRICE, A.ACC_ISWILLING, A.PO_QTY, A.ACC_DISCOUNT_QTY ";

            p.Add(":MAT_CLASS", MAT_CLASS);
            p.Add(":YYYMM", YYYMM);
            p.Add(":AGEN_NO", AGEN_NO);
            p.Add(":ExportType", ExportType);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable ReportInvoice(string MAT_CLASS, string ExportType, string YYYMM, string IsTCB)
        {
            var p = new DynamicParameters();

            var sql = @"select T.AGEN_NO, T.AGEN_NAMEC, T.UNI_NO, T.INVOICE, T.INVOICE_DT, T.MMCODE, T.MMNAME_C, T.ACC_PURUN as M_PURUN, T.ACC_PO_PRICE as PO_PRICE, sum(T.DELI_QTY) as DELI_QTY,
			nvl(sum(T.TOT_AMT), 0) as TOT_AMT, nvl(sum(T.TOT_AMT_1), 0) as TOT_AMT_1, sum(T.TOT_AMT_2) as TOT_AMT_2, sum(T.TOT_AMT_3) as TOT_AMT_3,
			nvl(sum(T.TOT_AMT), 0) - nvl(sum(T.TOT_AMT_1), 0) - sum(T.TOT_AMT_2) - sum(T.TOT_AMT_3) as TOT_AMT_4
			from (
				select A.AGEN_NO, C.AGEN_NAMEC, C.UNI_NO,
                A.INVOICE, twn_date(A.INVOICE_DT) as INVOICE_DT, A.MMCODE, B.MMNAME_C, A.ACC_PURUN, A.ACC_PO_PRICE, 
                sum(TX_QTY_T) as DELI_QTY,
				(case when sum(floor(A.ACC_PO_PRICE * A.TX_QTY_T )) < 0 then 0 else sum(floor(A.ACC_PO_PRICE * A.TX_QTY_T )) end) as TOT_AMT,
				round(sum(A.ACC_PO_PRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) as TOT_AMT_1,
				(case when A.ACC_ISWILLING='是' and A.PO_QTY >= A.ACC_DISCOUNT_QTY
				   then round(sum(A.ACC_DISC_CPRICE * A.TX_QTY_T)) - round(sum(A.ACC_DISC_COST_UPRICE * A.TX_QTY_T))
				   else 0
				end) as TOT_AMT_2,
				sum(EXTRA_DISC_AMOUNT) as TOT_AMT_3
				from BC_CS_ACC_LOG A, MI_MAST B, PH_VENDER C
				where A.MMCODE=B.MMCODE and A.AGEN_NO=C.AGEN_NO 
				and substr(A.PO_NO,1,3) NOT IN ('TXT') 
				and A.AGEN_NO <> '999'
				and A.STATUS='P'
				and A.MAT_CLASS = :MAT_CLASS
				and TWN_YYYMM(A.ACC_TIME) = :YYYMM  ";

            if (ExportType == "N")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('1','2','3','01','02','03')";
            else if (ExportType == "Y")
                sql += " and (select CONTRACNO from MM_PO_M where PO_NO = A.PO_NO) in ('0Y', '0N', 'X')";

            if (IsTCB == "0")  //合庫
                sql += " and C.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and C.AGEN_BANK <> '006' ";

            sql += @"       group by A.AGEN_NO, C.AGEN_NAMEC, C.AGEN_BANK, C.AGEN_ACC, C.UNI_NO, A.INVOICE, A.INVOICE_DT, 
                                A.MMCODE, B.MMNAME_C, A.ACC_PURUN, A.ACC_PO_PRICE, A.ACC_ISWILLING, A.PO_QTY, A.ACC_DISCOUNT_QTY
			) T
			group by T.AGEN_NO, T.AGEN_NAMEC, T.UNI_NO, T.INVOICE, T.INVOICE_DT, T.MMCODE, T.MMNAME_C, T.ACC_PURUN, T.ACC_PO_PRICE
			order by T.AGEN_NO, T.INVOICE ";

            p.Add(":MAT_CLASS", MAT_CLASS);
            p.Add(":YYYMM", YYYMM);
            p.Add(":ExportType", ExportType);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}