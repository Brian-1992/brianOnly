using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CE0027Repository : JCLib.Mvc.BaseRepository
    {
        public CE0027Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CE0027> GetMasterAll(string chk_ym, string ContentType)
        {
            var p = new DynamicParameters();

            string sql = @"with chk_nos as (
                               select chk_no from CHK_MAST a
                                where substr(a.chk_ym, 1, 5) = :CHK_YM 
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
                                            when :CHK_YM = f.set_ym
                                                then 'Y' else 'N'
                                        end) as is_current_ym
                                  from chk_nos A, CHK_DETAILTOT B, MI_WHCOST c,  current_set_ym f
                                 where A.CHK_NO = B.CHK_NO
                                   and b.mmcode = c.mmcode(+)
                                   and c.data_ym(+) = :CHK_YM
                           )
                           select * 
                             from (
                                    select mmcode, m_contprice, is_current_ym, avg_price,
                                           (select mmname_c from MI_MAST where mmcode = a.mmcode) as mmname_c, 
                                           (select mmname_e from MI_MAST where mmcode = a.mmcode) as mmname_e,
                                           (select base_unit from MI_MAST where mmcode = a.mmcode) as base_unit,
                                           round(sum(gap_t) * avg_price, 2) as diff_cost,
                                           sum(store_qty) as store_qty,
                                           sum(chk_qty) as chk_qty,
                                           sum(gap_t) as gap_t,
                                           trunc((sum(gap_t) / (case when sum(apl_outqty) = 0 then 1 else sum(apl_outqty) end)  * 100),5) as diff_p,
                                           sum(APL_OUTQTY) as apl_outqty ,
                                           (select listagg(wh_no || ':' || memo, '<br>') 
                                                             within group (order by mmcode)
                                                       from memo_data
                                                      where mmcode = a.mmcode) as memo
                                      from cost_data a
                                      group by mmcode, m_contprice, is_current_ym, avg_price
                                  ) a
                            where 1=1
                            ";

            //0=全部  1=盤盈  2=盤虧
            if (ContentType != string.Empty)
            {
                if (ContentType == "1")
                {
                    sql += " and  a.GAP_T > 0";
                }
                else if (ContentType == "2")
                {
                    sql += " and  a.GAP_T < 0";
                }
            }
            sql += @" 
                      order by mmcode";

            p.Add(":chk_ym", string.Format("{0}", chk_ym));

            return DBWork.PagingQuery<CE0027>(sql, p, DBWork.Transaction);

        }

        public IEnumerable<CE0027Count> GetDetails(string chk_ym)
        {
            var p = new DynamicParameters();

            string sql = @"with chk_nos as (        -- 取得盤點單號
                               select chk_no from CHK_MAST a
                                where substr(a.chk_ym, 1, 5) = :CHK_YM 
                                  and a.chk_wh_grade = '2' 
                                  and a.chk_wh_kind = '0' 
                                  and a.chk_level = '1' 
                           ),
                           current_set_ym as (      -- 取得目前月結年月
                                   select set_ym from MI_MNSET
                                    where set_status = 'N'
                                      and rownum = 1
                                    order by set_ym desc
                           ), 
                           memo_data as (           -- 取得備註資料
                               select a.wh_no, a.mmcode, a.memo
                                 from CHK_G2_DETAILTOT a, chk_nos b
                                where a.chk_no = b.chk_no
                                  and a.memo is not null
                           ),
                           cost_data as (           -- 取得所有藥局盤點資料
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
                                       nvl( b.apl_outqty, 0) as apl_outqty,
                                       (case
                                            when :CHK_YM = f.set_ym
                                                then 'Y' else 'N'
                                        end) as is_current_ym
                                  from chk_nos A, CHK_DETAILTOT B, MI_WHCOST c , current_set_ym f
                                 where A.CHK_NO = B.CHK_NO
                                   and b.mmcode = c.mmcode(+)
                                   and c.data_ym(+) = :CHK_YM
                           ),                           
                           sum_data as (            -- 取得各院內碼加總資料
                            select mmcode, m_contprice,avg_price, is_current_ym, 
                                   sum(chk_qty) as chk_qty, 
                                   sum(store_qty) as store_qty,
                                   sum(gap_t) as gap_t, 
                                   sum(apl_outqty) as apl_outqty
                             from cost_data
                             group by mmcode, m_contprice,avg_price, is_current_ym
                           )
                           select a.* , b.*, c.*        -- a:盤總 b:盤盈 c:盤虧
                             from (select nvl(round(sum(chk_qty * avg_price), 2), 0) as tot1,
                                          nvl(round(sum(store_qty * avg_price), 2), 0) as tot2,
                                          nvl(Trunc(( sum(GAP_T   * avg_price )  / 
                                                  decode(sum(APL_OUTQTY  * avg_price ),
                                                           0 , 1 ,
                                                           sum(APL_OUTQTY  * avg_price ) ) 
                                                   * 100),5), 0) as tot3,             
                                           nvl(round(sum(GAP_T   * avg_price ), 2), 0) as tot4,
                                           nvl(round(sum(APL_OUTQTY  * avg_price ), 2), 0) as tot5
                                    from sum_data
                                    ) a,
                                   (select nvl(round(sum(chk_qty * avg_price), 2), 0) as p_tot1,
                                           nvl(round(sum(store_qty * avg_price), 2), 0) as p_tot2,
                                           nvl(Trunc(( sum(GAP_T   * avg_price )  / 
                                                   decode(sum(APL_OUTQTY  * avg_price ),
                                                            0 , 1 ,
                                                            sum(APL_OUTQTY  * avg_price ) ) 
                                                    * 100),5), 0) as p_tot3,             
                                            nvl(round(sum(GAP_T   * avg_price ), 2), 0) as p_tot4,
                                            nvl(round(sum(APL_OUTQTY  * avg_price ), 2), 0) as p_tot5
                                       from sum_data
                                      where gap_t > 0
                                   ) b,
                                   (select nvl(round(sum(chk_qty * avg_price), 2), 0) as n_tot1,
                                           nvl(round(sum(store_qty * avg_price), 2), 0) as n_tot2,
                                           nvl(Trunc(( sum(GAP_T   * avg_price )  / 
                                                   decode(sum(APL_OUTQTY  * avg_price ),
                                                            0 , 1 ,
                                                            sum(APL_OUTQTY  * avg_price ) ) 
                                                    * 100),5), 0) as n_tot3,             
                                            nvl(round(sum(GAP_T   * avg_price ), 2), 0) as n_tot4,
                                            nvl(round(sum(APL_OUTQTY  * avg_price ), 2), 0) as n_tot5
                                       from sum_data
                                      where gap_t < 0
                                   ) c
                            ";

            p.Add(":chk_ym", string.Format("{0}", chk_ym));

            return DBWork.Connection.Query<CE0027Count>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CE0027> PrintData(string chk_ym, string ContentType)
        {
            var p = new DynamicParameters();

            string sql = @"with chk_nos as (
                               select chk_no from CHK_MAST a
                                where substr(a.chk_ym, 1, 5) = :chk_ym 
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
                                            when :CHK_YM = f.set_ym
                                                then 'Y' else 'N'
                                        end) as is_current_ym
                                  from chk_nos A, CHK_DETAILTOT B, MI_WHCOST c,  current_set_ym f
                                 where A.CHK_NO = B.CHK_NO
                                   and b.mmcode = c.mmcode(+)
                                   and c.data_ym(+) = :chk_ym
                           )
                           select * 
                             from (
                                    select mmcode, m_contprice, is_current_ym, avg_price,
                                           (select mmname_c from MI_MAST where mmcode = a.mmcode) as mmname_c, 
                                           (select mmname_e from MI_MAST where mmcode = a.mmcode) as mmname_e,
                                           (select base_unit from MI_MAST where mmcode = a.mmcode) as base_unit,
                                           round(sum(gap_t) * avg_price, 2) as diff_cost,
                                           sum(store_qty) as store_qty,
                                           sum(chk_qty) as chk_qty,
                                           sum(gap_t) as gap_t,
                                           trunc((sum(gap_t) / (case when sum(apl_outqty) = 0 then 1 else sum(apl_outqty) end)  * 100),5) as diff_p,
                                           sum(APL_OUTQTY) as apl_outqty ,
                                           (select listagg(wh_no || ':' || memo, '<br>') 
                                                             within group (order by mmcode)
                                                       from memo_data
                                                      where mmcode = a.mmcode) as memo
                                      from cost_data a
                                      group by mmcode, m_contprice, is_current_ym, avg_price
                                  ) a
                            where 1=1
                            ";

            //0=全部  1=盤盈  2=盤虧
            if (ContentType != string.Empty)
            {
                if (ContentType == "1")
                {
                    sql += " and  a.GAP_T > 0";
                }
                else if (ContentType == "2")
                {
                    sql += " and  a.GAP_T < 0";
                }
            }
            sql += " order by mmcode";

            p.Add(":chk_ym", string.Format("{0}", chk_ym));

            return DBWork.Connection.Query<CE0027>(sql, p, DBWork.Transaction);
        }

        public string GetChkPeriod(string chk_ym) {
            string sql = @"select distinct (select data_value || ' ' || data_desc 
                                              from PARAM_D 
                                              where grp_code = 'CHK_MAST' 
                                                and data_name = 'CHK_PERIOD' 
                                                and data_value = a.chk_period) 
                             from CHK_MAST a 
                            where a.chk_ym = :chk_ym
                              and a.chk_wh_kind = '0'
                              and a.chk_wh_grade = '2'
                              and a.chk_level = '1'";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_ym = chk_ym }, DBWork.Transaction);
        }
    }
}