using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models.C;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.C
{
    public class CE0026Repository : JCLib.Mvc.BaseRepository
    {
        public CE0026Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<CE0026> GetAll( string ym, string rb1,  int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"with chk_nos as (
                               select chk_no from CHK_MAST a
                                where substr(a.chk_ym, 1, 5) = :p0 
                                  and a.chk_wh_grade = '2' 
                                  and a.chk_wh_kind = '0' 
                                  and a.chk_level = '1' 
                           ),
                           current_set_ym as (
                                   select set_ym from MI_MNSET
                                    where set_status = 'N'
                                      and rownum = 1
                                    order by set_ym desc
                           ), 
                           memo_data as (
                               select a.wh_no, a.mmcode, a.memo
                                 from CHK_G2_DETAILTOT a, chk_nos b
                                where a.chk_no = b.chk_no
                                  and a.memo is not null
                           ),
                           cost_data as (
                                select b.wh_no, b.mmcode, 
                                       (select m_contprice from MI_MAST where mmcode = b.mmcode) as m_contprice,
                                       b.store_qty,
                                       c.avg_price, b.gap_t, 
                                       (CASE
                                             WHEN b.STATUS_TOT = '1' THEN b.CHK_QTY1
                                             WHEN b.STATUS_TOT = '2' THEN b.CHK_QTY2
                                             WHEN b.STATUS_TOT = '3' THEN b.CHK_QTY3
                                             ELSE 0
                                        END)    AS CHK_QTY,
                                       b.pro_los_qty , 
                                       nvl(b.apl_outqty, 0) as apl_outqty,
                                       (case
                                            when :p0 = f.set_ym
                                                then 'Y' else 'N'
                                        end) as is_current_ym
                                  from chk_nos A, CHK_DETAILTOT B, MI_WHCOST c, current_set_ym f
                                 where A.CHK_NO = B.CHK_NO
                                   and b.mmcode = c.mmcode(+)
                                   and c.data_ym(+) = :p0
                           )
                           select * 
                             from (
                                    select mmcode, m_contprice, is_current_ym, avg_price,
                                           (select mmname_c from MI_MAST where mmcode = a.mmcode) as mmname_c, 
                                           (select mmname_e from MI_MAST where mmcode = a.mmcode) as mmname_e,
                                           (select base_unit from MI_MAST where mmcode = a.mmcode) as base_unit,
                                           sum(store_qty) as store_qty,
                                           round(sum(store_qty) * avg_price, 2) as store_cost,
                                           sum(chk_qty) as chk_qty,
                                           round(sum(chk_qty) * avg_price, 2) as chk_cost,
                                           sum(gap_t) as gap_t,
                                           sum(apl_outqty) as aplq,
                                           trunc((sum(gap_t) / (case when sum(apl_outqty) = 0 then 1 else sum(apl_outqty) end)  * 100),2) as diff_p,
                                           round(sum(gap_t) * avg_price, 2) as diff_cost,
                                           (select listagg(wh_no || ':' || memo, '<br>') 
                                                             within group (order by mmcode)
                                                       from memo_data
                                                      where mmcode = a.mmcode) as memo
                                      from cost_data a
                                      group by mmcode, m_contprice, is_current_ym, avg_price
                                  ) a
                            where 1=1";

            if (rb1 == "1")
                {
                    sql += " AND  a.gap_t > 0";
                }
             
                if (rb1 == "2")
                {
                    sql += " AND a.gap_t < 0 ";
                }
         

            sql += " order by  a.mmcode ";
            p.Add(":p0", ym);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0026>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<CE0026> GetReport(string ym, string rb1)
        {
            var p = new DynamicParameters();

            var sql = @"with chk_nos as (
                               select chk_no from CHK_MAST a
                                where substr(a.chk_ym, 1, 5) = :p0 
                                  and a.chk_wh_grade = '2' 
                                  and a.chk_wh_kind = '0' 
                                  and a.chk_level = '1' 
                           ),
                           current_set_ym as (
                                   select set_ym from MI_MNSET
                                    where set_status = 'N'
                                      and rownum = 1
                                    order by set_ym desc
                           ), 
                           memo_data as (
                               select a.wh_no, a.mmcode, a.memo
                                 from CHK_G2_DETAILTOT a, chk_nos b
                                where a.chk_no = b.chk_no
                                  and a.memo is not null
                           ),
                           cost_data as (
                                select b.wh_no, b.mmcode, 
                                       (select m_contprice from MI_MAST where mmcode = b.mmcode) as m_contprice,
                                       b.store_qty,
                                       c.avg_price, b.gap_t, 
                                       (CASE
                                             WHEN b.STATUS_TOT = '1' THEN b.CHK_QTY1
                                             WHEN b.STATUS_TOT = '2' THEN b.CHK_QTY2
                                             WHEN b.STATUS_TOT = '3' THEN b.CHK_QTY3
                                             ELSE 0
                                        END)    AS CHK_QTY,
                                       b.pro_los_qty , 
                                       nvl(b.apl_outqty, 0) as apl_outqty,
                                       (case
                                            when :p0 = f.set_ym
                                                then 'Y' else 'N'
                                        end) as is_current_ym
                                  from chk_nos A, CHK_DETAILTOT B, MI_WHCOST c, current_set_ym f
                                 where A.CHK_NO = B.CHK_NO
                                   and b.mmcode = c.mmcode(+)
                                   and c.data_ym(+) = :p0
                           )
                           select * 
                             from (
                                    select mmcode, m_contprice, is_current_ym, avg_price,
                                           (select mmname_c from MI_MAST where mmcode = a.mmcode) as mmname_c, 
                                           (select mmname_e from MI_MAST where mmcode = a.mmcode) as mmname_e,
                                           (select base_unit from MI_MAST where mmcode = a.mmcode) as base_unit,
                                           sum(store_qty) as store_qty,
                                           round(sum(store_qty) * avg_price, 2) as store_cost,
                                           sum(chk_qty) as chk_qty,
                                           round(sum(chk_qty) * avg_price, 2) as chk_cost,
                                           sum(gap_t) as gap_t,
                                           sum(apl_outqty) as aplq,
                                           trunc((sum(gap_t) / (case when sum(apl_outqty) = 0 then 1 else sum(apl_outqty) end)  * 100),2) as diff_p,
                                           round(sum(gap_t) * avg_price, 2) as diff_cost,
                                           (select listagg(wh_no || ':' || memo, '<br>') 
                                                             within group (order by mmcode)
                                                       from memo_data
                                                      where mmcode = a.mmcode) as memo
                                      from cost_data a
                                      group by mmcode, m_contprice, is_current_ym, avg_price
                                  ) a
                            where 1=1";

            if (rb1 == "1")
            {
                sql += " AND  a.gap_t > 0";
            }

            if (rb1 == "2")
            {
                sql += " AND a.gap_t < 0 ";
            }


            sql += " order by  a.mmcode ";
            p.Add(":p0", ym);

            return DBWork.Connection.Query<CE0026>(sql, p, DBWork.Transaction);
        }
    }
}