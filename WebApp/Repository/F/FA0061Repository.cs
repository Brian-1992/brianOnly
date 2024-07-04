using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;

namespace WebApp.Repository.F
{
    public class FA0061Repository : JCLib.Mvc.BaseRepository
    {
        public FA0061Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0061> GetAll(string inqym, string sum_PAY_AMT, string is_SELF_PUR_UPPER_LIMIT, string is_SELF_CONT_BDATE, string SELF_CONT_BDATE)
        {
            var p = new DynamicParameters();
            int isum_PAY_AMT = Convert.ToInt16(sum_PAY_AMT);

            string sql = @"select A.*,:inqym0 as inqym,(select MMNAME_E from  MI_MAST where mmcode=a.mmcode) as MMNAME_E
                             from (select a.mmcode,b.SELF_CONTRACT_NO, b.SELF_PUR_UPPER_LIMIT,
                                          b.SELF_CONT_BDATE, b.SELF_CONT_EDATE,
                                          sum(round(a.DISC_CPRICE * a.DELI_QTY)) sum_PAY_AMT
                                     from MM_PO_INREC a, MED_SELFPUR_DEF b  
                                    Where a.wh_no=WHNO_ME1
                                      and a.DELI_QTY <> 0
                                      and twn_date(a.accountdate) >= b.SELF_CONT_BDATE
                                      and a.accountdate <= twn_todate(:inqym1||'01')-1  
                                      and a.mmcode=b.mmcode
                                      and b.SELF_CONT_BDATE <= twn_date(twn_todate(:inqym2||'01')-1)
                                      and b.SELF_CONT_EDATE >= twn_date(twn_todate(:inqym3||'01')-1)
                                    group by a.mmcode,b.SELF_CONTRACT_NO, b.SELF_PUR_UPPER_LIMIT,
                                             b.SELF_CONT_BDATE, b.SELF_CONT_EDATE
                                    ) A
                             where 1=1 ";                              
            
            p.Add(":inqym0", string.Format("{0}", inqym));
            p.Add(":inqym1", string.Format("{0}", inqym));
            p.Add(":inqym2", string.Format("{0}", inqym));
            p.Add(":inqym3", string.Format("{0}", inqym));            

            if (isum_PAY_AMT>0)
            {
                sql += @" and sum_PAY_AMT >= :sum_PAY_AMT ";
                p.Add("sum_PAY_AMT", isum_PAY_AMT*10000);
            }

            if (is_SELF_PUR_UPPER_LIMIT == "true") 
            {
                sql += @" and sum_PAY_AMT>=SELF_PUR_UPPER_LIMIT ";
            }

            if (is_SELF_CONT_BDATE == "true")
            {
                sql += @"  and twn_yyymm(add_months(twn_todate(SELF_CONT_EDATE),-:SELF_CONT_BDATE)) < :inqym4 ";
                p.Add(":SELF_CONT_BDATE", SELF_CONT_BDATE);
                p.Add(":inqym4", string.Format("{0}", inqym));
            }

            sql += " order by MMCODE";

            return DBWork.PagingQuery<FA0061>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string inqym, string sum_PAY_AMT, string is_SELF_PUR_UPPER_LIMIT, string is_SELF_CONT_BDATE, string SELF_CONT_BDATE)
        {
            var p = new DynamicParameters();
            int isum_PAY_AMT = Convert.ToInt16(sum_PAY_AMT);

            string sql = @"select A.mmcode as 院內碼,
                                  (select MMNAME_E from  MI_MAST where mmcode=a.mmcode) as 藥品英文名稱,
                                  A.SELF_CONTRACT_NO as 合約案號,
                                  A.SELF_PUR_UPPER_LIMIT as 採購上限金額,
                                  A.SELF_CONT_BDATE as 合約生效起日, 
                                  A.SELF_CONT_EDATE as 合約生效迄日,
                                  sum_PAY_AMT as 累計結報金額,
                                  :inqym0 as 查詢年月
                             from (select a.mmcode,b.SELF_CONTRACT_NO, b.SELF_PUR_UPPER_LIMIT,
                                          b.SELF_CONT_BDATE, b.SELF_CONT_EDATE,
                                          sum(round(a.DISC_CPRICE * a.DELI_QTY)) sum_PAY_AMT
                                     from MM_PO_INREC a, MED_SELFPUR_DEF b  
                                    Where a.wh_no=WHNO_ME1
                                      and a.DELI_QTY <> 0
                                      and twn_date(a.accountdate) >= b.SELF_CONT_BDATE
                                      and a.accountdate <= twn_todate(:inqym1||'01')-1  
                                      and a.mmcode=b.mmcode
                                      and b.SELF_CONT_BDATE <= twn_date(twn_todate(:inqym2||'01')-1)
                                      and b.SELF_CONT_EDATE >= twn_date(twn_todate(:inqym3||'01')-1)
                                    group by a.mmcode,b.SELF_CONTRACT_NO, b.SELF_PUR_UPPER_LIMIT,
                                             b.SELF_CONT_BDATE, b.SELF_CONT_EDATE
                                    ) A
                             where 1=1 ";

            p.Add(":inqym0", string.Format("{0}", inqym));
            p.Add(":inqym1", string.Format("{0}", inqym));
            p.Add(":inqym2", string.Format("{0}", inqym));
            p.Add(":inqym3", string.Format("{0}", inqym));            

            if (isum_PAY_AMT > 0)
            {
                sql += @" and sum_PAY_AMT >= :sum_PAY_AMT ";
                p.Add("sum_PAY_AMT", isum_PAY_AMT * 10000);
            }

            if (is_SELF_PUR_UPPER_LIMIT == "true")
            {
                sql += @" and sum_PAY_AMT>=SELF_PUR_UPPER_LIMIT ";
            }

            if (is_SELF_CONT_BDATE == "true")
            {
                sql += @"  and twn_yyymm(add_months(twn_todate(SELF_CONT_EDATE),-:SELF_CONT_BDATE)) < :inqym4 ";
                p.Add(":SELF_CONT_BDATE", SELF_CONT_BDATE);
                p.Add(":inqym4", string.Format("{0}", inqym));
            }

            sql += " order by MMCODE";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public DataTable GetExcel_Detail(string inqym)
        {
            var p = new DynamicParameters();

            string sql = @"with mast as (
                                         select A.*,:inqym0 as inqym,(select MMNAME_E from  MI_MAST where mmcode=a.mmcode) as MMNAME_E
                                           from (select a.mmcode,b.SELF_CONTRACT_NO, b.SELF_PUR_UPPER_LIMIT,
                                                        b.SELF_CONT_BDATE, b.SELF_CONT_EDATE,
                                                        sum(round(a.DISC_CPRICE * a.DELI_QTY)) sum_PAY_AMT
                                                   from MM_PO_INREC a, MED_SELFPUR_DEF b  
                                                  Where a.wh_no=WHNO_ME1
                                                    and a.DELI_QTY <> 0
                                                    and twn_date(a.accountdate) >= b.SELF_CONT_BDATE
                                                    and a.accountdate <= twn_todate(:inqym1||'01')-1  
                                                    and a.mmcode=b.mmcode
                                                    and b.SELF_CONT_BDATE <= twn_date(twn_todate(:inqym2||'01')-1)
                                                    and b.SELF_CONT_EDATE >= twn_date(twn_todate(:inqym3||'01')-1)
                                                  group by a.mmcode,b.SELF_CONTRACT_NO, b.SELF_PUR_UPPER_LIMIT,
                                                        b.SELF_CONT_BDATE, b.SELF_CONT_EDATE
                                                  ) A
                                          where 1=1
                                         ),
                          detail as (
                                     select twn_date(a.accountdate) accountdate,
                                            a.agen_no||' '||p.agen_namec as agen_no, a.mmcode, y.MMNAME_E, a.M_PURUN,
                                            a.PO_PRICE,
                                            (case when a.TRANSKIND='111' then 'A進貨'
                                                  when a.TRANSKIND='120' then 'D退貨'
                                             else ''
                                             end) as flag,
                                            a.DELI_QTY, round(a.PO_PRICE * a.DELI_QTY) PO_AMT,
                                            (case when a.CONTRACNO='2' then '2:自辨合約'
                                                  when a.CONTRACNO='3' then '3:正式採購一年期自辦合約品項'
                                            else a.CONTRACNO
                                            end) memo,
                                            (round(a.PO_PRICE * a.DELI_QTY) - round(a.DISC_CPRICE * a.DELI_QTY)) DISC_AMT,
                                            a.DISC_CPRICE,
                                            round(a.DISC_CPRICE * a.DELI_QTY) PAY_AMT
                                       from MM_PO_INREC a, PH_VENDER p, MI_MAST y, MED_SELFPUR_DEF b, mast c
                                      Where wh_no=WHNO_ME1
                                        and a.mmcode=y.mmcode
                                        and a.mmcode = c.mmcode
                                        and a.agen_no=p.agen_no
                                        and a.mmcode=b.mmcode
                                        and a.DELI_QTY <> 0
                                        and twn_date(a.accountdate) >= b.SELF_CONT_BDATE
                                        and twn_date(a.accountdate) <= twn_date(twn_todate(c.inqym||'01')-1)
                                      order by a.agen_no, a.accountdate, MMNAME_E, a.mmcode
                                    )
                       select  b.accountdate as 進貨日期,
                               b.agen_no as 廠商代碼, 
                               b.mmcode as 院內碼, 
                               b.MMNAME_E as 藥品名稱, 
                               b.M_PURUN as 單位,
                               b.PO_PRICE as 發票單價,
                               b.flag as 類別,
                               b.DELI_QTY as 進貨量, 
                               b.PO_AMT as 發票金額,
                               b.memo as 說明,
                               b.DISC_AMT as 折讓金額,
                               b.DISC_CPRICE as 優惠價,
                               b.PAY_AMT as 優惠金額
                          from mast a, detail b
                         where a.mmcode = b.mmcode
                        order by a.mmcode, b.agen_no, b.accountdate, b.MMNAME_E ";

            p.Add(":inqym0", string.Format("{0}", inqym));
            p.Add(":inqym1", string.Format("{0}", inqym));
            p.Add(":inqym2", string.Format("{0}", inqym));
            p.Add(":inqym3", string.Format("{0}", inqym));          
        
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<FA0061> GetMMCODE_Detail(string inqym, string mmcode)
        {
            var p = new DynamicParameters();

            string sql = @"select twn_date(a.accountdate) accountdate,
                                  a.agen_no||' '||p.agen_namec as agen_no, a.mmcode, y.MMNAME_E, a.M_PURUN,
                                  a.PO_PRICE,
                                  (case when a.TRANSKIND='111' then 'A進貨'
                                        when a.TRANSKIND='120' then 'D退貨'
                                   else ''
                                   end) as flag,
                                  a.DELI_QTY, round(a.PO_PRICE * a.DELI_QTY) PO_AMT,
                                  (case when a.CONTRACNO='2' then '2:自辨合約'
                                        when a.CONTRACNO='3' then '3:正式採購一年期自辦合約品項'
                                   else a.CONTRACNO
                                   end) memo,
                                  (round(a.PO_PRICE * a.DELI_QTY) - round(a.DISC_CPRICE * a.DELI_QTY)) DISC_AMT,
                                  a.DISC_CPRICE,
                                  round(a.DISC_CPRICE * a.DELI_QTY) PAY_AMT
                             from MM_PO_INREC a, PH_VENDER p, MI_MAST y, MED_SELFPUR_DEF b
                            Where wh_no=WHNO_ME1
                              and a.mmcode=y.mmcode
                              and a.agen_no=p.agen_no
                              and a.mmcode=b.mmcode
                              and a.DELI_QTY <> 0
                              and twn_date(a.accountdate) >= b.SELF_CONT_BDATE
                              and twn_date(a.accountdate) <= twn_date(twn_todate(:inqym0||'01')-1)
                              and a.MMCODE=:mmcode
                            order by a.agen_no, a.accountdate, MMNAME_E, a.mmcode";

            p.Add(":inqym0", string.Format("{0}", inqym));
            p.Add(":mmcode", string.Format("{0}", mmcode));          

            return DBWork.PagingQuery<FA0061>(sql, p, DBWork.Transaction);
        }

        public DataTable GetMmcodeExcel(string inqym, string mmcode)
        {
            var p = new DynamicParameters();

            string sql = @"select twn_date(a.accountdate) 進貨日期,
                                  a.agen_no||' '||p.agen_namec as 廠商代碼, 
                                  a.mmcode as 院內碼, 
                                  y.MMNAME_E as 藥品名稱, 
                                  a.M_PURUN as 單位,
                                  a.PO_PRICE as 發票單價,
                                  (case when a.TRANSKIND='111' then 'A進貨'
                                        when a.TRANSKIND='120' then 'D退貨'
                                   else ''
                                   end) as 類別,
                                  a.DELI_QTY as 進貨量, 
                                  round(a.PO_PRICE * a.DELI_QTY) as 發票金額,
                                  (case when a.CONTRACNO='2' then '2:自辨合約'
                                        when a.CONTRACNO='3' then '3:正式採購一年期自辦合約品項'
                                   else a.CONTRACNO
                                   end) as 說明,
                                  (round(a.PO_PRICE * a.DELI_QTY) - round(a.DISC_CPRICE * a.DELI_QTY)) as 折讓金額,
                                  a.DISC_CPRICE as 優惠價,
                                  round(a.DISC_CPRICE * a.DELI_QTY) as 優惠金額
                             from MM_PO_INREC a, PH_VENDER p, MI_MAST y, MED_SELFPUR_DEF b
                            Where wh_no=WHNO_ME1
                              and a.mmcode=y.mmcode
                              and a.agen_no=p.agen_no
                              and a.mmcode=b.mmcode
                              and a.DELI_QTY <> 0
                              and twn_date(a.accountdate) >= b.SELF_CONT_BDATE
                              and twn_date(a.accountdate) <= twn_date(twn_todate(:inqym0||'01')-1)
                              and a.MMCODE=:mmcode
                            order by a.agen_no, a.accountdate, MMNAME_E, a.mmcode";

            p.Add(":inqym0", string.Format("{0}", inqym));
            p.Add(":mmcode", string.Format("{0}", mmcode));


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

    }
}