using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0085Repository : JCLib.Mvc.BaseRepository
    {
        public AA0085Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ComboItemModel> GetAgenCombo()
        {
            var p = new DynamicParameters();

            var sql = @"select agen_no||'_'||agen_namec TEXT,agen_no VALUE from PH_VENDER
                             Where rec_status <>'X' order by agen_no ";


            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);

        }
        public IEnumerable<ComboItemModel> Wh_NoComboGet()
        {
            var p = new DynamicParameters();

            var sql = @"select WH_NO VALUE,WH_NO ||' '||WH_NAME TEXT from  MI_WHMAST where WH_GRADE in ('1','5') and wh_kind='0'   order by wh_no
                              ";


            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);

        }

        public IEnumerable<AA0085> GetAll(string WH_NO, string ExportType, string YYYMM, string AGEN_NO, string IsTCB, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.AGEN_NO, AGEN_NAMEC, AGEN_ACC, 
                                   CASE WHEN AGEN_BANK = '006' THEN '＊' 
                                        ELSE ' ' END AS AGEN_ISLOCAL , 
                                   CASE :ExportType WHEN 'Y' THEN '零購' WHEN 'N' THEN '合約' END  as memo,
                                   sum(b.subtot) TOT_AMT , 0 TOT_AMT_1, 0 TOT_AMT_2
                              from PH_VENDER a, 
                              (SELECT agen_no, accountdate,  po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,  
                                      (case when TransKind='111' then round(sum(PO_PRICE * DELI_QTY) )
                                            else round( sum(PO_PRICE * DELI_QTY*(-1)) ) end) subtot
                                 FROM MM_PO_INREC  
                                Where wh_no=:WH_NO 
                                  and TWN_YYYMM(accountdate) =:YYYMM  
                                  and STATUS in  ('Y', 'E')
                                  AND AGEN_NO<>'999'
                                  AND PO_PRICE<>0
                              ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')";  //'1','2','01','02'
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";
            sql += @"  group by agen_no, accountdate,  po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO 
                        )  b ";
            sql += " Where a.agen_no=b.agen_no ";
            if (AGEN_NO != "")
            {
                sql += " and a.agen_no <> :AGEN_NO";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";
            sql += " group by  a.agen_no,AGEN_NAMEC, AGEN_ACC, AGEN_BANK ";

            sql += @" union 
                            select a.AGEN_NO, AGEN_NAMEC, AGEN_ACC, 
                                   CASE WHEN AGEN_BANK = '006' THEN '＊' 
                                        ELSE ' ' END AS AGEN_ISLOCAL , 
                                   CASE :ExportType WHEN 'Y' THEN '零購_折讓' WHEN 'N' THEN '合約_折讓' END  as memo,
                                   sum(b.subtot) TOT_AMT , sum(b.subtot_1) TOT_AMT_1, sum(b.subtot_2) TOT_AMT_2
                            from PH_VENDER a, 
                              (SELECT agen_no, accountdate,  po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                                      mmcode, TRANSKIND, CONTRACNO,
                                      (case when TransKind='111'
                                            then round(sum(PO_PRICE * DELI_QTY)) - round(sum(DISC_CPRICE * DELI_QTY))
                                            else (round(sum(PO_PRICE * DELI_QTY)) - round(sum(DISC_CPRICE * DELI_QTY)))*(-1)
                                       end) subtot_1, 
                                      (case when TransKind='111' 
                                            then (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                       then round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY))
                                                       else 0
                                                  end) 
                                            else (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                       then (round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY)))
                                                       else 0
                                                  end)*(-1)
                                       end) subtot_2,
                                      (case when TransKind='111' 
                                            then round(sum(PO_PRICE * DELI_QTY)) - round(sum(disc_cprice * DELI_QTY)) +
                                                   (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                         then round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY))
                                                         else 0
                                                    end) 
                                            else (round(sum(PO_PRICE * DELI_QTY)) - round(sum(disc_cprice * DELI_QTY)) +
                                                    (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                          then round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY))
                                                          else 0
                                                     end) 
                                                 )*(-1)
                                       end) subtot
                                 FROM MM_PO_INREC  
                                Where wh_no=:WH_NO 
                                  and TWN_YYYMM(accountdate) =:YYYMM  
                                  and STATUS in  ('Y', 'E')
                                  AND AGEN_NO<>'999'
                                  AND PO_PRICE<>0
                              ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')";
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";
            sql += @"  group by agen_no, accountdate,  po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                                mmcode, TRANSKIND, CONTRACNO,
                                DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING
                        )  b ";

            sql += " Where a.agen_no=b.agen_no   ";
            if (AGEN_NO != "")
            {
                sql += "and a.agen_no <> :AGEN_NO";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";
            sql += " group by  a.agen_no,AGEN_NAMEC, AGEN_ACC, AGEN_BANK ";

            sql += @" union 
                            select a.AGEN_NO, AGEN_NAMEC, AGEN_ACC, 
                                   CASE WHEN AGEN_BANK = '006' THEN '＊' 
                                        ELSE ' ' END AS AGEN_ISLOCAL , 
                                   CASE :ExportType WHEN 'Y' THEN '零購_實付' WHEN 'N' THEN '合約_實付' END  as memo ,
                                   sum(b.subtot) TOT_AMT , 0 TOT_AMT_1, 0 TOT_AMT_2
                            from PH_VENDER a, 
                            (SELECT agen_no,  accountdate, po_no, lot_no, exp_date,  M_PURUN, PO_PRICE, DISC_CPRICE,
                                    mmcode, TRANSKIND, CONTRACNO,  
                                    (case when TransKind='111' 
                                          then round(sum((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY 
                                                               then DISC_COST_UPRICE else DISC_CPRICE end) * DELI_QTY))
                                          else round(sum((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY 
                                                               then DISC_COST_UPRICE else DISC_CPRICE end) * DELI_QTY * (-1)))
                                     end) subtot
                               FROM MM_PO_INREC  
                              Where wh_no=:WH_NO 
                                and TWN_YYYMM(accountdate) =:YYYMM  
                                and STATUS in  ('Y', 'E')
                                AND AGEN_NO<>'999'
                                AND PO_PRICE<>0
                              ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')"; 
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";
            sql += @"  group by agen_no,  accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                                mmcode, TRANSKIND, CONTRACNO,
                                DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING 
                        )  b ";

            sql += " Where a.agen_no=b.agen_no ";
            if (AGEN_NO != "")
            {
                sql += " and a.agen_no <> :AGEN_NO";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";
            sql += " group by  a.agen_no,AGEN_NAMEC, AGEN_ACC, AGEN_BANK ";

            sql += " order by agen_no, memo ";

            p.Add(":WH_NO", WH_NO);
            p.Add(":YYYMM", YYYMM);
            p.Add(":AGEN_NO", AGEN_NO);
            p.Add(":ExportType", ExportType);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0085>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0085> Report(string WH_NO, string ExportType, string YYYMM, string AGEN_NO, string IsTCB)
        {
            var p = new DynamicParameters();

            var sql = @"select a.AGEN_NO, AGEN_NAMEC, AGEN_ACC, 
                                   CASE WHEN a.AGEN_BANK = '006' THEN '＊' 
                                        ELSE ' ' END AS AGEN_ISLOCAL , 
                                   CASE :ExportType WHEN 'Y' THEN '零購' WHEN 'N' THEN '合約' END  as memo,
                                   sum(b.subtot) TOT_AMT , 0 TOT_AMT_1, 0 TOT_AMT_2
                              from PH_VENDER a, 
                              (SELECT agen_no, accountdate,  po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,  
                                 (case when TransKind='111' then round(sum(PO_PRICE * DELI_QTY) )
                                 else round( sum(PO_PRICE * DELI_QTY*(-1)) ) end) subtot
                                 FROM MM_PO_INREC  
                                 Where wh_no=:WH_NO 
                                 and TWN_YYYMM(accountdate) =:YYYMM  
                                 and STATUS in  ('Y', 'E')
                                 AND AGEN_NO<>'999'
                                 AND PO_PRICE<>0
                              ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')"; 
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";
            sql += @"  group by  agen_no,  accountdate,  po_no, lot_no, exp_date,  M_PURUN, PO_PRICE, DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO
                        )  b ";

            sql += " Where a.agen_no=b.agen_no   ";
            if (AGEN_NO != "")
            {
                sql += " and a.agen_no <> :AGEN_NO";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";
            sql += " group by  a.agen_no,AGEN_NAMEC, AGEN_ACC, a.AGEN_BANK ";

            sql += @"union 
                            select a.AGEN_NO, AGEN_NAMEC, AGEN_ACC, 
                                   CASE WHEN a.AGEN_BANK = '006' THEN '＊' 
                                        ELSE ' ' END AS AGEN_ISLOCAL , 
                                   CASE :ExportType WHEN 'Y' THEN '零購_折讓' WHEN 'N' THEN '合約_折讓' END  as memo,
                                   sum(b.subtot) TOT_AMT , sum(b.subtot_1) TOT_AMT_1, sum(b.subtot_2) TOT_AMT_2  
                            from PH_VENDER a, 
                              (SELECT agen_no,  accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                                      mmcode, TRANSKIND, CONTRACNO,
                                      (case when TransKind='111'
                                            then round(sum(PO_PRICE * DELI_QTY)) - round(sum(DISC_CPRICE * DELI_QTY))
                                            else (round(sum(PO_PRICE * DELI_QTY)) - round(sum(DISC_CPRICE * DELI_QTY)))*(-1)
                                       end) subtot_1,
                                      (case when TransKind='111' 
                                            then (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                       then round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY))
                                                       else 0
                                                  end) 
                                            else (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                       then (round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY)))
                                                       else 0
                                                  end)*(-1)
                                       end) subtot_2,
                                      (case when TransKind='111' 
                                            then round(sum(PO_PRICE * DELI_QTY)) - round(sum(disc_cprice * DELI_QTY)) +
                                                   (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                         then round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY))
                                                         else 0
                                                    end) 
                                            else (round(sum(PO_PRICE * DELI_QTY)) - round(sum(disc_cprice * DELI_QTY)) +
                                                    (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                          then round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY))
                                                          else 0
                                                     end) 
                                                 )*(-1)
                                       end) subtot
                                 FROM MM_PO_INREC  
                                Where wh_no=:WH_NO 
                                  and TWN_YYYMM(accountdate) =:YYYMM  
                                  and STATUS in  ('Y', 'E')
                                  AND AGEN_NO<>'999'
                                  AND PO_PRICE<>0
                              ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')"; 
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";
            sql += @"  group by agen_no, accountdate,  po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                                mmcode, TRANSKIND, CONTRACNO,
                                DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING
                        )  b ";

            sql += " Where a.agen_no=b.agen_no   ";
            if (AGEN_NO != "")
            {
                sql += " and a.agen_no <> :AGEN_NO";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";
            sql += " group by  a.agen_no,AGEN_NAMEC, AGEN_ACC, a.AGEN_BANK  ";

            sql += @" union 
                            select a.AGEN_NO, AGEN_NAMEC, AGEN_ACC, 
                                   CASE WHEN a.AGEN_BANK = '006'  THEN '＊' 
                                        ELSE ' ' END AS AGEN_ISLOCAL , 
                                   CASE :ExportType WHEN 'Y' THEN '零購_實付' WHEN 'N' THEN '合約_實付' END  as memo ,
                                   sum(b.subtot) TOT_AMT , 0 TOT_AMT_1, 0 TOT_AMT_2  
                            from PH_VENDER a, 
                              (SELECT agen_no, accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                                      mmcode, TRANSKIND, CONTRACNO,
                                      (case when TransKind='111' 
                                            then round(sum((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY 
                                                                 then DISC_COST_UPRICE else DISC_CPRICE end) * DELI_QTY))
                                            else round(sum((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY 
                                                                 then DISC_COST_UPRICE else DISC_CPRICE end) * DELI_QTY * (-1)))
                                       end) subtot
                                 FROM MM_PO_INREC  
                                Where wh_no=:WH_NO 
                                  and TWN_YYYMM(accountdate) =:YYYMM  
                                  and STATUS in  ('Y', 'E')
                                  AND AGEN_NO<>'999'
                                  AND PO_PRICE<>0
                              ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')";
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";

            sql += @"  group by agen_no, accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                                mmcode, TRANSKIND, CONTRACNO,
                                DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING
                        )  b ";

            sql += " Where a.agen_no=b.agen_no ";
            if (AGEN_NO != "")
            {
                sql += " and a.agen_no <> :AGEN_NO";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";
            sql += " group by  a.agen_no,AGEN_NAMEC, AGEN_ACC, a.AGEN_BANK ";
            sql += "   order by agen_no, memo ";


            sql = string.Format(@"
                with ori_data as (
                    {0}
                ),
                zero_data as (
                   select agen_no from ori_data
                    where memo like '%實付%'
                      and tot_amt = 0
                )
                select a.*
                  from ori_data a
                 where not exists (select 1 from zero_data where agen_no = a.agen_no)
                 order by agen_no, memo
            ", sql);

            p.Add(":WH_NO", WH_NO);
            p.Add(":YYYMM", YYYMM);
            p.Add(":AGEN_NO", AGEN_NO);
            p.Add(":ExportType", ExportType);

            return DBWork.Connection.Query<AA0085>(sql, p, DBWork.Transaction);
        }

        public DataTable ReportDetail(string WH_NO, string ExportType, string YYYMM, string AGEN_NO, string AGEN_NO2, string IsTCB)
        {
            var p = new DynamicParameters();

            var sql = @"select agen_no, accountdate, MMNAME_E, M_PURUN, PO_PRICE, DISC_CPRICE,
                          DISC_COST_UPRICE, DISCOUNT_QTY, mmcode,
                          TRANSKIND, CONTRACNO, sum(DELI_QTY) DELI_QTY,
                          round(PO_PRICE * sum(DELI_QTY))- round(DISC_CPRICE * sum(DELI_QTY))  DISC_AMT,
                          (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                then round(DISC_CPRICE * sum(DELI_QTY)) - round(DISC_COST_UPRICE * sum(DELI_QTY)) 
                                else null
                           end) as WILL_DISC_AMT,
                          round(PO_PRICE * sum(DELI_QTY))   PO_AMT,
                          round(PO_PRICE * sum(DELI_QTY))   DISC_AMT,
                          round(PO_PRICE * sum(DELI_QTY))   PAY_AMT,
                          (case when TRANSKIND='111' then 'A進貨'  when TRANSKIND='120' then 'D已退貨' else '' end) as flag,
                          (case when CONTRACNO='0Y' then '零購' when CONTRACNO='0N' then '零購' when CONTRACNO='X' then '零購' else '合約' end)  memo,  
                          (case when TransKind='111' then round(PO_PRICE * sum(DELI_QTY))  else  round(PO_PRICE * sum(DELI_QTY))*(-1) end) RAW_PO_PRICE,
                          (case when TransKind='111' 
                                then round(PO_PRICE * sum(DELI_QTY)) - round(disc_cprice * sum(DELI_QTY)) +
                                       (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                             then round(DISC_CPRICE * sum(DELI_QTY)) - round(DISC_COST_UPRICE * sum(DELI_QTY))
                                             else 0
                                        end)
                                else (round(PO_PRICE * sum(DELI_QTY)) - round(disc_cprice * sum(DELI_QTY)) +
                                        (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                              then round(DISC_CPRICE * sum(DELI_QTY)) - round(DISC_COST_UPRICE * sum(DELI_QTY))
                                              else 0
                                         end)
                                     )*(-1)
                           end) RAW_DISC
                        from  
                        (select TWN_date(a.accountdate) accountdate, a.agen_no, a.mmcode||' | '||substr(b.MMNAME_E,1,30) MMNAME_E,  
                                po_no, lot_no, exp_date,  
                                a.M_PURUN, a.PO_PRICE,a.DISC_CPRICE, a.mmcode, a.CONTRACNO, a.TRANSKIND, a.DELI_QTY,
                                a.DISC_COST_UPRICE, a.DISCOUNT_QTY, a.PO_QTY, a.ISWILLING
                           from MM_PO_INREC a, mi_mast b
                          Where wh_no=:wh_no
                            and a.mmcode=b.mmcode 
                            and STATUS in  ('Y', 'E')
                            AND AGEN_NO<>'999'
                            AND PO_PRICE<>0
                            and TWN_YYYMM(accountdate) =:YYYMM ";
            if (AGEN_NO != "")
            {
                sql += " and a.agen_no <> :AGEN_NO ";
            }

            if (ExportType == "N")
            {
                sql += " and a.CONTRACNO in ('1','2','3','01','02','03') ";
            }
            else
            {
                sql += " and a.CONTRACNO in ('0Y', '0N', 'X') ";
            }
            if (AGEN_NO2 != "")
            {
                sql += " and a.agen_no=:AGEN_NO2 ";
            }
            if (IsTCB == "0")  //合庫
                sql += " and (select agen_bank from PH_VENDER where agen_no = a.agen_no) = '006' ";
            else if (IsTCB == "1")
                sql += " and (select agen_bank from PH_VENDER where agen_no = a.agen_no) <> '006' ";
            sql += @"  )
                     group by agen_no, accountdate, MMNAME_E, po_no, lot_no, exp_date,
                              M_PURUN, PO_PRICE, DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                              DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING
                     order by MMNAME_E, DELI_QTY, TRANSKIND ";

            p.Add(":WH_NO", WH_NO);
            p.Add(":YYYMM", YYYMM);
            p.Add(":AGEN_NO", AGEN_NO);
            p.Add(":ExportType", ExportType);
            p.Add(":AGEN_NO2", AGEN_NO2);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel(string WH_NO, string ExportType, string YYYMM, string AGEN_NO, string IsTCB)
        {
            var p = new DynamicParameters();

            var sql = @"select a.AGEN_NO 廠商編號, AGEN_NAMEC 公司名稱, 
                                   CASE WHEN a.AGEN_BANK = '006' THEN '*' 
                                        ELSE ' ' END AS 合庫 , 
                                   AGEN_ACC 銀行帳號, '' 發票張數,
                                   sum(b.subtot) 發票金額,
                                   CASE :ExportType WHEN 'Y' THEN '零購' WHEN 'N' THEN '合約' END  as 備註
                              from PH_VENDER a, 
                              (SELECT agen_no,  accountdate,  po_no, lot_no, exp_date,
                                      M_PURUN, PO_PRICE, DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                                      (case when TransKind='111'
                                            then round(sum(PO_PRICE * DELI_QTY) )
                                            else round( sum(PO_PRICE * DELI_QTY*(-1)) )
                                       end) subtot
                                 FROM MM_PO_INREC  
                                Where wh_no=:WH_NO 
                                  and TWN_YYYMM(accountdate) =:YYYMM  
                                  and STATUS in  ('Y', 'E')
                                  AND AGEN_NO<>'999'
                                  AND PO_PRICE<>0
                               ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')"; 
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";
            sql += @"  group by agen_no, accountdate,  po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                                mmcode, TRANSKIND, CONTRACNO
                        )  b ";

            sql += " Where a.agen_no=b.agen_no ";
            if (AGEN_NO != "")
            {
                sql += " and a.agen_no <> :AGEN_NO";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";
            sql += " group by  a.agen_no,AGEN_NAMEC, AGEN_ACC, a.AGEN_BANK ";

            sql += @" union 
                            select a.AGEN_NO 廠商編號, AGEN_NAMEC 公司名稱,  
                                   CASE WHEN a.AGEN_BANK = '006' THEN '＊' 
                                        ELSE ' ' END AS 合庫 , 
                                   AGEN_ACC 銀行帳號, '' 發票張數,
                                   sum(b.subtot) 發票金額,
                                    CASE :ExportType WHEN 'Y' THEN '零購_折讓' WHEN 'N' THEN '合約_折讓' END  as 備註
                             from PH_VENDER a, 
                              (SELECT agen_no, accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                                      mmcode, TRANSKIND, CONTRACNO,
                                      (case when TransKind='111' 
                                            then round(sum(PO_PRICE * DELI_QTY)) - round(sum(disc_cprice * DELI_QTY)) +
                                                   (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                         then round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY))
                                                         else 0
                                                    end)
                                            else (round(sum(PO_PRICE * DELI_QTY)) - round(sum(disc_cprice * DELI_QTY)) +
                                                   (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                         then round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY))
                                                         else 0
                                                    end)
                                                 )*(-1)
                                       end) subtot
                                 FROM MM_PO_INREC  
                                Where wh_no=:WH_NO 
                                  and TWN_YYYMM(accountdate) =:YYYMM  
                                  and STATUS in  ('Y', 'E')                                 
                                  AND AGEN_NO<>'999'
                                  AND PO_PRICE<>0
                              ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')"; 
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";
            sql += @"  group by agen_no, accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                             mmcode, TRANSKIND, CONTRACNO,
                             DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING
                        )  b ";

            sql += " Where a.agen_no=b.agen_no   ";
            if (AGEN_NO != "")
            {
                sql += " and a.agen_no <> :AGEN_NO";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";
            sql += " group by  a.agen_no,AGEN_NAMEC, AGEN_ACC, a.AGEN_BANK ";

            sql += @" union 
                            select a.AGEN_NO 廠商編號, AGEN_NAMEC 公司名稱,  
                                   CASE WHEN a.AGEN_BANK = '006' THEN '＊' 
                                        ELSE ' ' END AS 合庫 , 
                                   AGEN_ACC 銀行帳號,'' 發票張數,
                                   sum(b.subtot) 發票金額,
                                   CASE :ExportType WHEN 'Y' THEN '零購_實付' WHEN 'N' THEN '合約_實付' END  as 備註 
                              from PH_VENDER a, 
                                (SELECT agen_no, accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                                        mmcode, TRANSKIND, CONTRACNO,
                                        (case when TransKind='111' 
                                              then round(sum((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY 
                                                                   then DISC_COST_UPRICE else DISC_CPRICE end) * DELI_QTY))
                                              else round(sum((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY 
                                                                   then DISC_COST_UPRICE else DISC_CPRICE end) * DELI_QTY * (-1)))
                                         end) subtot
                                   FROM MM_PO_INREC  
                                  Where wh_no=:WH_NO 
                                    and TWN_YYYMM(accountdate) =:YYYMM  
                                    and STATUS in  ('Y', 'E')
                                    AND AGEN_NO<>'999'
                                    AND PO_PRICE<>0
                              ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')"; 
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";
            sql += @"  group by agen_no, accountdate,  po_no, lot_no, exp_date, M_PURUN, PO_PRICE, DISC_CPRICE,
                             mmcode, TRANSKIND, CONTRACNO,
                             DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING
                        )  b ";

            sql += " Where a.agen_no=b.agen_no   ";
            if (AGEN_NO != "")
            {
                sql += "and a.agen_no <> :AGEN_NO";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";
            sql += " group by  a.agen_no,AGEN_NAMEC, AGEN_ACC, a.AGEN_BANK ";

            sql += "  order by 廠商編號, 備註";
            p.Add(":WH_NO", WH_NO);
            p.Add(":YYYMM", YYYMM);
            p.Add(":AGEN_NO", AGEN_NO);
            p.Add(":ExportType", ExportType);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        public DataTable GetReportSumValue(string WH_NO, string ExportType, string YYYMM, string AGEN_NO, string IsTCB)
        {
            var p = new DynamicParameters();

            var sql = @"select sum(b.subtot), 0, 0,
                                   CASE :ExportType WHEN 'Y' THEN '零購' WHEN 'N' THEN '合約' END  as memo
                            , 1 num
                            from PH_VENDER a, 
                            (   SELECT agen_no,  accountdate,  po_no, lot_no, exp_date,  M_PURUN, PO_PRICE, DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO, 
                                 (case when TransKind='111' then round(sum(PO_PRICE * DELI_QTY) )
                                 else round( sum(PO_PRICE * DELI_QTY*(-1)) ) end) subtot
                                 FROM MM_PO_INREC  
                                 Where wh_no=:WH_NO 
                                 and TWN_YYYMM(accountdate) =:YYYMM  
                                 and STATUS in  ('Y', 'E')
                                 AND AGEN_NO<>'999'
                                 AND PO_PRICE<>0
                        ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')"; 
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";

            sql += @"  group by agen_no,  accountdate,  po_no, lot_no, exp_date,  M_PURUN, PO_PRICE, DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO
                        )  b ";
            sql += " Where a.agen_no=b.agen_no   ";

            if (AGEN_NO != "")
            {
                sql += " and a.agen_no <> :AGEN_NO ";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";

            sql += @"   union 
                            select sum(b.subtot), sum(b.subtot_1), sum(b.subtot_2), 
                                   CASE :ExportType WHEN 'Y' THEN '零購_折讓' WHEN 'N' THEN '合約_折讓' END  as memo
                            , 2 num
                            from PH_VENDER a, 
                                (SELECT agen_no, accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE,
                                        DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                                       (case when TransKind='111'
                                             then round(sum(PO_PRICE * DELI_QTY)) - round(sum(DISC_CPRICE * DELI_QTY))
                                             else (round(sum(PO_PRICE * DELI_QTY)) - round(sum(DISC_CPRICE * DELI_QTY)))*(-1)
                                        end) subtot_1,
                                       (case when TransKind='111' 
                                             then (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                        then round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY))
                                                        else 0
                                                   end) 
                                             else (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                        then (round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY)))
                                                        else 0
                                                   end)*(-1)
                                        end) subtot_2,
                                       (case when TransKind='111' 
                                             then round(sum(PO_PRICE * DELI_QTY)) - round(sum(disc_cprice * DELI_QTY)) +
                                                    (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                          then round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY))
                                                          else 0
                                                     end)
                                             else (round(sum(PO_PRICE * DELI_QTY)) - round(sum(disc_cprice * DELI_QTY)) +
                                                     (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                                           then round(sum(DISC_CPRICE * DELI_QTY)) - round(sum(DISC_COST_UPRICE * DELI_QTY))
                                                           else 0
                                                      end)
                                                  )*(-1)
                                        end) subtot
                                 FROM MM_PO_INREC  
                                Where wh_no=:WH_NO 
                                  and TWN_YYYMM(accountdate) =:YYYMM  
                                  and STATUS in  ('Y', 'E')
                                  AND AGEN_NO<>'999'
                                  AND PO_PRICE<>0
                      ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')"; 
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";

            sql += @"  group by agen_no, accountdate,  po_no, lot_no, exp_date, M_PURUN, PO_PRICE,
                                DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                                DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING
                        )  b ";
            sql += " Where a.agen_no=b.agen_no   ";

            if (AGEN_NO != "")
            {
                sql += " and a.agen_no <> :AGEN_NO ";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";

            sql += @" union 
                            select sum(b.subtot) , 0, 0,
                                   CASE :ExportType WHEN 'Y' THEN '零購_實付' WHEN 'N' THEN '合約_實付' END  as memo
                            , 3 num     
                            from PH_VENDER a, 
                              (SELECT agen_no,  accountdate,  po_no, lot_no, exp_date,  M_PURUN, PO_PRICE,
                                      DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                                      (case when TransKind='111' 
                                            then round(sum((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY 
                                                                 then DISC_COST_UPRICE else DISC_CPRICE end) * DELI_QTY))
                                            else round(sum((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY 
                                                                 then DISC_COST_UPRICE else DISC_CPRICE end) * DELI_QTY * (-1)))
                                       end) subtot
                                 FROM MM_PO_INREC  
                                Where wh_no=:WH_NO 
                                  and TWN_YYYMM(accountdate) =:YYYMM  
                                  and STATUS in  ('Y', 'E')
                                  AND AGEN_NO<>'999'
                                  AND PO_PRICE<>0
                     ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')"; 
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";

            sql += @"  group by agen_no, accountdate,  po_no, lot_no, exp_date, M_PURUN, PO_PRICE,
                                DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                                DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING
                        )  b ";
            sql += " Where a.agen_no=b.agen_no ";
            if (AGEN_NO != "")
            {
                sql += " and a.agen_no <> :AGEN_NO ";
            }
            if (IsTCB == "0")  //合庫
                sql += " and a.AGEN_BANK = '006' ";
            else if (IsTCB == "1")
                sql += " and a.AGEN_BANK <> '006' ";
            sql += @"             order by num";

            p.Add(":WH_NO", WH_NO);
            p.Add(":YYYMM", YYYMM);
            p.Add(":AGEN_NO", AGEN_NO);
            p.Add(":ExportType", ExportType);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public DataTable GetReportTOTValue(string WH_NO, string ExportType, string YYYMM, string AGEN_NO, string IsTCB)
        {
            var p = new DynamicParameters();
            //合約
            //合庫 AGEN_BANK = '006'
            var sql = @"SELECT count(distinct a.agen_no) ,
                          SUM(ROUND(DELI_QTY * PO_PRICE))  TOT,
                          SUM(ROUND(DELI_QTY * PO_PRICE) - ROUND(DELI_QTY * DISC_CPRICE) +
                              ROUND(DELI_QTY * DISC_CPRICE) - ROUND(DELI_QTY * DISC_COST_UPRICE_A)) TOT_DISC,
                          SUM(ROUND(DELI_QTY * PO_PRICE) - ROUND(DELI_QTY * DISC_CPRICE)) TOT_DISC_1,
                          SUM(ROUND(DELI_QTY * DISC_CPRICE) - ROUND(DELI_QTY * DISC_COST_UPRICE_A)) TOT_DISC_2,
                          SUM(ROUND((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                          then DISC_COST_UPRICE else DISC_CPRICE end) * DELI_QTY)
                             ) TOT_PAY,
                          1 num
                        FROM PH_VENDER a, 
                             (SELECT agen_no, accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE,
                                     DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                                     DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING,
                                     (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                           then DISC_COST_UPRICE
                                           else DISC_CPRICE
                                      end) DISC_COST_UPRICE_A,
                                     (case when TransKind='111' then sum(DELI_QTY)
                                           else sum(DELI_QTY)*(-1)  end) DELI_QTY
                                FROM MM_PO_INREC  
                               Where wh_no=:WH_NO 
                                 and TWN_YYYMM(accountdate) =:YYYMM  
                                 and STATUS in  ('Y', 'E') 
                                 AND AGEN_NO<>'999'
                                 AND PO_PRICE<>0
                        ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')"; 
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";
            sql += @"    group by agen_no, accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE,
                                  DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                                  DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING
                         ) b  
                         WHERE A.AGEN_NO=B.AGEN_NO 
                           AND a.AGEN_BANK = '006' ";
            if (AGEN_NO != "")
                sql += " and a.agen_no <> :AGEN_NO ";

            //非合庫  AGEN_BANK <> '006'
            sql += @"  UNION
                       SELECT  count(distinct a.agen_no) ,
                          SUM(ROUND(DELI_QTY * PO_PRICE))  TOT,
                          SUM(ROUND(DELI_QTY * PO_PRICE) - ROUND(DELI_QTY * DISC_CPRICE) +
                              ROUND(DELI_QTY * DISC_CPRICE) - ROUND(DELI_QTY * DISC_COST_UPRICE_A)) TOT_DISC,
                          SUM(ROUND(DELI_QTY * PO_PRICE) - ROUND(DELI_QTY * DISC_CPRICE)) TOT_DISC_1,
                          SUM(ROUND(DELI_QTY * DISC_CPRICE) - ROUND(DELI_QTY * DISC_COST_UPRICE_A)) TOT_DISC_2,
                          SUM(ROUND((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                          then DISC_COST_UPRICE else DISC_CPRICE end) * DELI_QTY)
                             ) TOT_PAY,
                          2 num
                        FROM PH_VENDER a, 
                             (SELECT agen_no, accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE,
                                     DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                                     DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING,
                                     (case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY
                                           then DISC_COST_UPRICE
                                           else DISC_CPRICE
                                      end) DISC_COST_UPRICE_A,
                                     (case when TransKind='111' then sum(DELI_QTY)
                                           else sum(DELI_QTY)*(-1)  end) DELI_QTY
                                FROM MM_PO_INREC  
                               Where wh_no=:WH_NO 
                                 and TWN_YYYMM(accountdate) =:YYYMM  
                                 and STATUS in ('Y', 'E') 
                                 AND AGEN_NO<>'999'
                                 AND PO_PRICE<>0 
                      ";

            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')"; 
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";
            sql += @"     group by agen_no, accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE,
                                   DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                                   DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING
                         ) b  
                         WHERE A.AGEN_NO=B.AGEN_NO 
                           AND a.AGEN_BANK <> '006' ";
            if (AGEN_NO != "")
                sql += " and a.agen_no <> :AGEN_NO ";

            sql += @"   order by num";
            p.Add(":WH_NO", WH_NO);
            p.Add(":YYYMM", YYYMM);
            p.Add(":AGEN_NO", AGEN_NO);
            p.Add(":ExportType", ExportType);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public DataTable GetTxtData(string WH_NO, string ExportType, string YYYMM, string AGEN_NO, string IsTCB)
        {
            //var sql = @"SELECT AGEN_ACC AS 銀行帳號,
            //            (PAYSUM - TXFEE) AS 匯款金額,
            //            AGEN_BANK AS 銀行代碼,
            //            AGEN_SUB AS 分行代碼,
            //            UNI_NO AS 統一編號,
            //            AGEN_NAMEC AS 廠商名稱
            //            FROM PH_PO_MONPAY 
            //            WHERE yyymm = :YearMonth 
            //             AND mat_class = :MATCLS ";

            var p = new DynamicParameters();
            //string sql = @"select a.AGEN_NO, agen_bank, AGEN_NAMEC, AGEN_ACC, 
            //                       CASE WHEN AGEN_BANK = '006' THEN '＊' 
            //                            ELSE ' ' END AS AGEN_ISLOCAL , 
            //                       CASE :ExportType WHEN 'Y' THEN '零購_實付' WHEN 'N' THEN '合約_實付' END  as memo ,
            //                       sum(b.tot) TOT_AMT  
            string sql = @"select agen_no, AGEN_ACC AS 銀行帳號,
                            tot AS 匯款金額, 
                            AGEN_BANK AS 銀行代碼,
                            AGEN_SUB AS 分行代碼,
                            UNI_NO AS 統一編號,
                            AGEN_NAMEC AS 廠商名稱
                          from                                   
                            (SELECT a.agen_no, AGEN_ACC, AGEN_BANK, AGEN_SUB,
                                    UNI_NO, AGEN_NAMEC, sum(b.tot) tot                          
                               from PH_VENDER a, 
                                    (SELECT agen_no, accountdate, po_no, lot_no, exp_date, M_PURUN, PO_PRICE,
                                            DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                                            (case when TransKind='111' 
                                                  then round((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY 
                                                                   then DISC_COST_UPRICE else DISC_CPRICE
                                                              end) * sum(DELI_QTY))  
                                                  else round((case when ISWILLING='是' and PO_QTY>=DISCOUNT_QTY 
                                                                   then DISC_COST_UPRICE else DISC_CPRICE
                                                              end) * sum(DELI_QTY))*(-1)
                                             end) tot
                                       FROM MM_PO_INREC  
                                      Where wh_no=:WH_NO 
                                        and TWN_YYYMM(accountdate) =:YYYMM  
                                        and STATUS in  ('Y', 'E')
                                        AND AGEN_NO<>'999'
                                        AND PO_PRICE<>0
                              ";
            if (ExportType == "N")
                sql += " and CONTRACNO in ('1','2','3','01','02','03')";
            else
                sql += " and CONTRACNO in ('0Y', '0N', 'X')";
            sql += @"   group by agen_no,  accountdate,  po_no, lot_no, exp_date,  M_PURUN, PO_PRICE,
                                 DISC_CPRICE, mmcode, TRANSKIND, CONTRACNO,
                                DISC_COST_UPRICE, DISCOUNT_QTY, PO_QTY, ISWILLING
                       ) b 
                     where a.agen_no=b.agen_no 
                        group by a.agen_no, agen_acc, agen_bank, agen_sub, uni_no, agen_namec
                    ) c
                      where 1=1 ";
            if (AGEN_NO != "")
            {
                sql += " and agen_no <> :AGEN_NO";
            }
            if (IsTCB == "0")
                sql += @" AND agen_bank = '006'";
            else if (IsTCB == "1")
                sql += @" AND agen_bank <> '006'";

            sql += " order by agen_no";

            sql = string.Format(@"select * from (
                                    {0}
                                )
                                 where 匯款金額 <> 0", sql);

            p.Add(":WH_NO", WH_NO);
            p.Add(":YYYMM", YYYMM);
            p.Add(":AGEN_NO", AGEN_NO);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public int UpdatePrice(string WH_NO, string YYYMM, string UPDATE_USER, string UPDATE_IP)
        {
            var p = new DynamicParameters();
            var sql = @" update MM_PO_INREC a
                           set PO_PRICE =(select CONT_PRICE from V_MMCODE_PRICE where mmcode=a.mmcode and  begindate <= twn_date(accountdate) and twn_date(accountdate) <= enddate and rownum=1 ), 
                              DISC_CPRICE=(select DISC_CPRICE from V_MMCODE_PRICE where mmcode=a.mmcode and  begindate <= twn_date(accountdate) and twn_date(accountdate) <= enddate and rownum=1 ),  
                              UPDATE_TIME =SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP,
                              PO_PRICE_OLD=(case when PO_PRICE_OLD is null then PO_PRICE else PO_PRICE_OLD end),
                              DISC_CPRICE_OLD=(case when DISC_CPRICE_OLD is null then DISC_CPRICE else DISC_CPRICE_OLD end) 
                        where wh_no=:WH_NO 
                              and TWN_YYYMM(accountdate) =:YYYMM 
                       ";
            p.Add(":WH_NO", WH_NO);
            p.Add(":YYYMM", YYYMM);
            p.Add(":UPDATE_USER", UPDATE_USER);
            p.Add(":UPDATE_IP", UPDATE_IP);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
        #region 暫存
        //public DataSet GetExcel(string ExportType, string YYYMM, string AGEN_NO)
        //{
        //    var p1 = new DynamicParameters();
        //    var p2 = new DynamicParameters();

        //    string sql = string.Format( @"SELECT AGEN_NO, 
        //                       AGEN_NAMEC, 
        //                       AGEN_ACC, 
        //                       AGEN_ISLOCAL,
        //                       AGEN_BANK, 
        //                       AGEN_SUB,
        //                       CASE WHEN AGEN_ISLOCAL = 'Y' 
        //                            THEN '＊' 
        //                            ELSE ' ' END AS BANKMARK ,
        //                       SUM(SumInOutAmount) SumInOutAmount ,
        //                       '' AS RecYM,
        //                       '' AS CountInvoice,
        //                       '' AS RateTi, 
        //                       MamageType,
        //                       '' AS Title,
        //                       '' CountGroup
        //                FROM ( 
        //                       SELECT SUPP.AGEN_NO, 
        //                              SUPP.AGEN_NAMEC, 
        //                              SUPP.AGEN_ACC,
        //                              SUPP.AGEN_ISLOCAL,
        //                              SUPP.AGEN_BANK, 
        //                              SUPP.AGEN_SUB,  
        //                              ROUND(STTN.TR_INV_QTY * ORDD.CONTRACTPRICE ,0) AS SUMINOUTAMOUNT ,  --合約價
        //                              'A' AS MamageType,                                                 --(合約)
        //                              ORDD.ContracNo
        //                FROM PH_VENDER SUPP, 
        //                     MI_WHTRNS STTN, 
        //                     ME_DOCM   DOCM, 
        //                     HIS_BASORDD ORDD 
        //                WHERE SUPP.AGEN_NO = DOCM.AGEN_NO 
        //                --AND DOCM.DOCNO = STTN.TR_DOCNO 
        //                AND STTN.MMCODE = ORDD.OrderCode 
        //                AND TWN_DATE(TR_DATE) BETWEEN ORDD.BEGINDATE AND ORDD.ENDDATE 
        //                AND SUBSTR(TWN_DATE(TR_DATE),1,5) = :YYYMM 
        //                UNION ALL 
        //                SELECT SUPP.AGEN_NO, 
        //                       SUPP.AGEN_NAMEC, 
        //                       SUPP.AGEN_ACC,
        //                       SUPP.AGEN_ISLOCAL,
        //                       SUPP.AGEN_BANK, 
        //                       SUPP.AGEN_SUB,  
        //                       ROUND(STTN.TR_INV_QTY * ORDD.COSTAMOUNT,0) AS SUMINOUTAMOUNT,   --進價
        //                       'B' AS MamageType,                                                  --(合約_實付)
        //                       ORDD.ContracNo
        //                FROM PH_VENDER SUPP, 
        //                     MI_WHTRNS STTN, 
        //                     ME_DOCM   DOCM, 
        //                     HIS_BASORDD ORDD 
        //                WHERE SUPP.AGEN_NO = DOCM.AGEN_NO 
        //                --AND DOCM.DOCNO = STTN.TR_DOCNO
        //                AND STTN.MMCODE = ORDD.OrderCode 
        //                AND TWN_DATE(TR_DATE) BETWEEN ORDD.BEGINDATE AND ORDD.ENDDATE 
        //                AND SUBSTR(TWN_DATE(TR_DATE),1,5) = :YYYMM 
        //                UNION ALL 
        //                SELECT SUPP.AGEN_NO, 
        //                       SUPP.AGEN_NAMEC, 
        //                       SUPP.AGEN_ACC, 
        //                       SUPP.AGEN_ISLOCAL,
        //                       SUPP.AGEN_BANK, 
        //                       SUPP.AGEN_SUB,
        //                       (ROUND(STTN.TR_INV_QTY * ORDD.CONTRACTPRICE,0) - ROUND(STTN.TR_INV_QTY * ORDD.COSTAMOUNT ,0)) AS SumInOutAmount, --合約價 - 進價
        //                       'C' AS  MamageType,                                                -- (合約_折讓)
        //                       ORDD.ContracNo
        //                FROM PH_VENDER SUPP, 
        //                     MI_WHTRNS STTN, 
        //                     ME_DOCM   DOCM, 
        //                     HIS_BASORDD ORDD 
        //                WHERE SUPP.AGEN_NO = DOCM.AGEN_NO 
        //                --AND DOCM.DOCNO = STTN.TR_DOCNO 
        //                AND STTN.MMCODE = ORDD.OrderCode 
        //                AND TWN_DATE(TR_DATE) BETWEEN ORDD.BEGINDATE AND ORDD.ENDDATE 
        //                AND SUBSTR(TWN_DATE(TR_DATE),1,5) = :YYYMM 
        //                )EXNDBACK 

        //                WHERE  ContracNo IN  :ContracNo

        //                {0}

        //                GROUP BY AGEN_NO, 
        //                      AGEN_NAMEC, 
        //                      AGEN_ACC,  
        //                      MamageType ,
        //                      AGEN_ISLOCAL,
        //                      AGEN_BANK, 
        //                      AGEN_SUB,
        //                      ContracNo
        //                ORDER BY MamageType,  
        //                      AGEN_NO, 
        //                      AGEN_NAMEC, 
        //                      AGEN_ACC
        //                ", AGEN_NO!=""?"AND AGEN_NO<>:AGEN_NO":"");

        //    p1.Add(":YYYMM", string.Format("{0}", YYYMM));
        //    p2.Add(":YYYMM", string.Format("{0}", YYYMM));

        //    if (AGEN_NO != "")
        //    {
        //        p1.Add(":AGEN_NO", string.Format("{0}", AGEN_NO));
        //        p2.Add(":AGEN_NO", string.Format("{0}", AGEN_NO));
        //    }

        //    DataSet ds = new DataSet();

        //    DataTable dt;

        //    DataTable dt1;
        //    DataTable dt2;
        //    DataTable dt3;
        //    DataRow[] dr1;
        //    DataRow[] dr2;
        //    DataRow[] dr3;

        //    //零購
        //    if (ExportType == "" || ExportType == "Y")
        //    {
        //        string[] ContracNoA = { "0Y", "0N" };
        //        p1.Add(":ContracNo", ContracNoA);

        //        dt = new DataTable();
        //        using (var rdr = DBWork.Connection.ExecuteReader(sql, p1, DBWork.Transaction))
        //            dt.Load(rdr);

        //        dt1 = new DataTable();
        //        dt2 = new DataTable();
        //        dt3 = new DataTable();
        //        dr1 = dt.Select("MamageType='A'");
        //        dr2 = dt.Select("MamageType='B'");
        //        dr3 = dt.Select("MamageType='C'");

        //        if (dr1.Length > 0)
        //        {
        //            dt1 = dr1.CopyToDataTable();
        //        }
        //        if (dr2.Length > 0)
        //        {
        //            dt2 = dr2.CopyToDataTable();
        //        }
        //        if (dr3.Length > 0)
        //        {
        //            dt3 = dr3.CopyToDataTable();
        //        }
        //        dt1.TableName = YYYMM.Substring(0, 3) + "年" + YYYMM.Substring(3, 2) + "月份藥品合約品項結報月報表(零購).csv";
        //        dt2.TableName = YYYMM.Substring(0, 3) + "年" + YYYMM.Substring(3, 2) + "月份藥品合約品項結報月報表(零購_實付).csv";
        //        dt3.TableName = YYYMM.Substring(0, 3) + "年" + YYYMM.Substring(3, 2) + "月份藥品合約品項結報月報表(零購_折讓).csv";

        //        ds.Tables.Add(dt1);
        //        ds.Tables.Add(dt2);
        //        ds.Tables.Add(dt3);
        //    }

        //    //合約99%
        //    if (ExportType == "" || ExportType == "N")
        //    {
        //        string[] ContracNoB = { "1", "2", "01", "02" };
        //        p2.Add(":ContracNo", ContracNoB);

        //        dt = new DataTable();
        //        using (var rdr = DBWork.Connection.ExecuteReader(sql, p2, DBWork.Transaction))
        //            dt.Load(rdr);

        //        dt1 = new DataTable();
        //        dt2 = new DataTable();
        //        dt3 = new DataTable();
        //        dr1 = dt.Select("MamageType='A'");
        //        dr2 = dt.Select("MamageType='B'");
        //        dr3 = dt.Select("MamageType='C'");

        //        if (dr1.Length > 0)
        //        {
        //            dt1 = dr1.CopyToDataTable();
        //        }
        //        if (dr2.Length > 0)
        //        {
        //            dt2 = dr2.CopyToDataTable();
        //        }
        //        if (dr3.Length > 0)
        //        {
        //            dt3 = dr3.CopyToDataTable();
        //        }
        //        dt1.TableName = YYYMM.Substring(0, 3) + "年" + YYYMM.Substring(3, 2) + "月份藥品合約品項結報月報表(合約).csv";
        //        dt2.TableName = YYYMM.Substring(0, 3) + "年" + YYYMM.Substring(3, 2) + "月份藥品合約品項結報月報表(合約_實付).csv";
        //        dt3.TableName = YYYMM.Substring(0, 3) + "年" + YYYMM.Substring(3, 2) + "月份藥品合約品項結報月報表(合約_折讓).csv";

        //        ds.Tables.Add(dt1);
        //        ds.Tables.Add(dt2);
        //        ds.Tables.Add(dt3);
        //    }

        //    return ds;
        //}
        #endregion

        public IEnumerable<PH_BANK_FEE> GetPhBankFee() {
            string sql = @"select * from PH_BANK_FEE
                            order by cashfrom";

            return DBWork.Connection.Query<PH_BANK_FEE>(sql, DBWork.Transaction);
        }

        public string ChkChtDateStr(string yyymm)
        {
            var sql = @"select twn_yyymm(twn_todate(:YYYMM || '01')) from dual ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { YYYMM = yyymm }, DBWork.Transaction);
        }
    }
}