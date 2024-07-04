using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CE0013Repository : JCLib.Mvc.BaseRepository
    {
        public CE0013Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CHK_MAST> GetMasterAll(string wh_no, string chk_ym, string keeper, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select a.CHK_NO,
                                  a.CHK_YM,
                                  a.CHK_WH_NO, 
                                  a.CHK_WH_GRADE,
                                  a.CHK_WH_KIND,
                                  a.CHK_PERIOD,
                                  a.CHK_TYPE,
                                  a.CHK_LEVEL,
                                  a.CHK_NUM,
                                  a.CHK_TOTAL,
                                  a.CHK_STATUS,
                                  a.CHK_KEEPER,
                                  a.CHK_CLASS,
                                  (select g.una from UR_ID g where g.tuser = a.CHK_KEEPER) as CHK_KEEPER_NAME,
                                  a.CHK_NO1,
                                  (select b.WH_NO || ' ' ||b.WH_NAME from MI_WHMAST b where b.WH_NO = a.CHK_WH_NO) as WH_NAME,
                                  (select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                     from PARAM_D c
                                    where c.GRP_CODE = 'CHK_MAST' 
                                      and c.DATA_NAME = 'CHK_WH_KIND'
                                      and c.DATA_VALUE = a.CHK_WH_KIND) as WH_KIND_NAME,
                                  (select d.DATA_VALUE || ' ' || d.DATA_DESC 
                                     from PARAM_D d
                                    where d.GRP_CODE = 'CHK_MAST' 
                                      and d.DATA_NAME = 'CHK_PERIOD'
                                      and d.DATA_VALUE = a.CHK_PERIOD) as CHK_PERIOD_NAME,
                                  (select e.DATA_VALUE || ' ' || e.DATA_DESC 
                                     from PARAM_D e
                                    where e.GRP_CODE = 'CHK_MAST' 
                                      and e.DATA_NAME = 'CHK_LEVEL'
                                      and e.DATA_VALUE = a.CHK_LEVEL) as CHK_LEVEL_NAME,
                                  (select f.DATA_VALUE || ' ' || f.DATA_DESC 
                                     from PARAM_D f
                                    where f.GRP_CODE = 'CHK_MAST' 
                                      and f.DATA_NAME = 'CHK_STATUS'
                                      and f.DATA_VALUE = a.CHK_STATUS) as CHK_STATUS_NAME,
                                  (select f.DATA_VALUE || ' ' || f.DATA_DESC 
                                     from PARAM_D f
                                    where f.GRP_CODE = 'CHK_MAST' 
                                      and f.DATA_NAME = 'CHK_CLASS'
                                      and f.DATA_VALUE = a.CHK_CLASS) as CHK_CLASS_NAME,
                                  (select h.chk_level from CHK_MAST h 
                                    where h.chk_no1 = a.chk_no 
                                      and h.chk_level = (select max(chk_level) from  CHK_MAST h2 
                                                          where h2.chk_no1 = a.chk_no 
                                                            and h2.chk_status in ('3', 'P', 'C'))  
                                  ) as FINAL_LEVEL,
                                  (select h.chk_level from CHK_MAST h 
                                    where h.chk_no1 = a.chk_no 
                                      and h.chk_level = (select max(chk_level) from  CHK_MAST h2 
                                                          where h2.chk_no1 = a.chk_no 
                                                            and h2.chk_status not in ('3', 'P', 'C'))  
                                  ) as ING_LEVEL  
                             from CHK_MAST a
                            where a.chk_level = '1'
                              and chk_status in ('3', 'P', 'C')";

            if (wh_no != string.Empty)
            {
                sql += "      and a.CHK_WH_NO = :wh_no";
            }

            if (chk_ym != string.Empty)
            {
                sql += "      and a.CHK_YM like :chk_ym";
            }

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_ym", string.Format("{0}%", chk_ym));
            p.Add(":keeper", string.Format("{0}", keeper));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CHK_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public string GetChkWhkindName(string kind, string value)
        {

            string sql = string.Format(@"select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                           from PARAM_D c
                                          where c.GRP_CODE = 'CHK_MAST' 
                                            and c.DATA_NAME = 'CHK_WH_KIND_{0}'
                                            and c.DATA_VALUE = '{1}'", kind, value);

            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        public IEnumerable<CE0013Data> GetDetails(string chk_no, string chk_level, string wh_kind, string condition, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = string.Format(@"
                with temp_set_ym as (
                        select set_ym from MI_MNSET
                         where (select create_date as temp from CHK_MAST where chk_no = :chk_no)
                                 between set_btime and set_ctime
                           and rownum = 1
                         order by set_ym desc
                ),
                current_set_ym as (
                        select set_ym from MI_MNSET
                         where set_status = 'N'
                           and rownum = 1
                         order by set_ym desc
                ), 
                cost_data as (
                        select a.mmcode, a.chk_no,
                               (TRIM(NVL(mmname_c, ' ')) || '<br>' || TRIM(NVL(mmname_e, ' '))) as mmname,
                               (a.store_qty) as store_qty,
                               b.set_ym, b.avg_price,
                                (select uprice from MI_MAST where mmcode = a.mmcode) as m_contprice,
                                (CASE
                                      WHEN STATUS_TOT = '1' THEN CHK_QTY1
                                      WHEN STATUS_TOT = '2' THEN CHK_QTY2
                                      WHEN STATUS_TOT = '3' THEN CHK_QTY3
                                      ELSE 0
                                 END)    AS CHK_QTY,
                               a.pro_los_qty , 
                               c.set_ym as chk_set_ym,
                               nvl(d.apl_outqty, a.apl_outqty) as apl_outqty,
                                a.consume_qty ,
                               (select listagg(chk_remark, '<br>') within group (order by store_loc)                     
                                  from chk_detail
                                 where chk_no = (
                                                select CHK_NO
                                                  from chk_mast
                                                 where chk_level = a.STATUS_TOT
                                                   and chk_no1 = :chk_no)
                                   and mmcode = a.mmcode
                                   and chk_remark is not null) as diff_remark ,
                               (case
                                    when c.set_ym = e.set_ym
                                        then 'Y' else 'N'
                                end) as is_current_ym,
                               f.wh_grade,
                               nvl(d.use_qty, 0) as use_qty
                          from CHK_DETAILTOT a, MI_WHCOST{0} b, temp_set_ym c, MI_WINVMON d, current_set_ym e, MI_WHMAST f
                         where a.chk_no = :chk_no
                           and a.mmcode = b.mmcode(+)
                           and b.data_ym(+) = c.set_ym
                           and d.data_ym(+) = c.set_ym
                           and d.wh_no(+) = a.wh_no
                           and d.mmcode(+) = a.mmcode
                           and f.wh_no = a.wh_no
                           {1}
                    )
                select a.mmcode, a.chk_no, a.mmname,a.store_qty, a.CHK_QTY,  
                       (pro_los_qty) as diff_qty,
                        (case when is_current_ym = 'Y' then m_contprice else avg_price end) as m_contprice,
                       round((a.pro_los_qty) *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 2) as diff_amount,
                       (case when
                               decode(apl_outqty, 0 ,   null,apl_outqty) is null
                               then a.pro_los_qty * 100
                               else 
                               TRUNC ((pro_los_qty / apl_outqty * 100), 5)
                       end ) as diff_p,
                       a.diff_remark  ,
                       (case 
                         when a.wh_grade > '1' then a.use_qty
                            else nvl(a.apl_outqty, a.consume_qty)
                         end) as apl_outqty,
                       a.is_current_ym
                 from cost_data a 
                where 1=1
            ", wh_kind == "E" || wh_kind == "C" ? "_EC": string.Empty
            , wh_kind == "E" || wh_kind == "C" ? "   and b.mc_id = '1'" : string.Empty);

            if (condition != string.Empty) {
                sql += string.Format("     and (pro_los_qty) {0} 0", condition);
            }

            p.Add(":chk_no", string.Format("{0}", chk_no));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0013Data>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public CE0013Count GetCountAll(string chk_no, string chk_level, string wh_kind) {
            var p = new DynamicParameters();

            string sql = string.Format(@"
                with temp_set_ym as (
                        select set_ym from MI_MNSET
                         where (select create_date as temp from CHK_MAST where chk_no = :chk_no)
                                 between set_btime and nvl(set_etime, sysdate)
                ),
                current_set_ym as (
                        select set_ym from MI_MNSET
                         where set_status = 'N'
                           and rownum = 1
                         order by set_ym desc
                ), 
                cost_data as (
                    select a.mmcode, a.last_qty, b.set_ym, b.avg_price,
                           (a.store_qty) as store_qty,
                           (select uprice from MI_MAST where mmcode = a.mmcode) as m_contprice,
                           (case
                              when status_tot = '1' then chk_qty1
                              when status_tot = '2' then chk_qty2
                              when status_tot = '3' then chk_qty3
                              else 0
                            end)    as chk_qty,
                           a.pro_los_qty , 
                           c.set_ym as chk_set_ym,
                           nvl(d.apl_outqty, a.apl_outqty) as apl_outqty,
                           (case
                                when c.set_ym = e.set_ym
                                    then 'Y' else 'N'
                            end) as is_current_ym
                      from CHK_DETAILTOT a, MI_WHCOST{0} b, temp_set_ym c, MI_WINVMON d, current_set_ym e
                     where a.chk_no = :chk_no
                       and a.mmcode = b.mmcode(+)
                       and b.data_ym(+) = c.set_ym
                       and d.data_ym(+) = c.set_ym
                       and d.wh_no(+) = a.wh_no
                       and d.mmcode(+) = a.mmcode
                        {1}
                )
                select round(sum(chk_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end) ), 2) as tot1,
                       round(sum(store_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end)), 2) as tot2,
                       TRUNC ( SUM (nvl(pro_los_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 0)) / 
                               decode (SUM (nvl(apl_outqty  *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 1)),
                                        0, 1 ,
                                        SUM (nvl(apl_outqty  *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 1)))
                                * 100, 5) AS tot3,
                       round(SUM (nvl(pro_los_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 0)), 2) AS tot4,
                       round(SUM (nvl(apl_outqty  *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 0)), 2) AS tot5
                  from cost_data
            ", wh_kind == "E" || wh_kind == "C" ? "_EC" : string.Empty
            , wh_kind == "E" || wh_kind == "C" ? "   and b.mc_id = '1'" : string.Empty);

            p.Add(":chk_no", string.Format("{0}", chk_no));

            return DBWork.Connection.QueryFirst<CE0013Count>(sql, p, DBWork.Transaction);
        }

        public CE0013Count GetCountP(string chk_no, string chk_level, string wh_kind)
        {
            var p = new DynamicParameters();

            string sql = string.Format(@"
                with temp_set_ym as (
                        select set_ym from MI_MNSET
                         where (select create_date as temp from CHK_MAST where chk_no = :chk_no)
                                 between set_btime and nvl(set_etime, sysdate)
                ),
                current_set_ym as (
                        select set_ym from MI_MNSET
                         where set_status = 'N'
                           and rownum = 1
                         order by set_ym desc
                ), 
                cost_data as (
                    select a.mmcode, a.last_qty, b.set_ym, b.avg_price,
                           (a.store_qty - 
                             (case
                                  WHEN a.STATUS_TOT = '1' THEN nvl(a.his_consume_qty_t1, 0)
                                     WHEN a.STATUS_TOT = '2' THEN nvl(a.his_consume_qty_t2, 0)
                                     WHEN a.STATUS_TOT = '3' THEN nvl(a.his_consume_qty_t3, 0)
                                     ELSE 0
                               end)   
                           ) as store_qty,
                           (select uprice from MI_MAST where mmcode = a.mmcode) as m_contprice,
                           (case
                              when status_tot = '1' then chk_qty1
                              when status_tot = '2' then chk_qty2
                              when status_tot = '3' then chk_qty3
                              else 0
                            end)    as chk_qty,
                           a.pro_los_qty , 
                           c.set_ym as chk_set_ym,
                           nvl(d.apl_outqty, a.apl_outqty) as apl_outqty,
                           (case
                                when c.set_ym = e.set_ym
                                    then 'Y' else 'N'
                            end) as is_current_ym
                      from CHK_DETAILTOT a, MI_WHCOST{0} b, temp_set_ym c, MI_WINVMON d, current_set_ym e
                     where a.chk_no = :chk_no
                       and a.mmcode = b.mmcode(+)
                       and b.data_ym(+) = c.set_ym
                       and d.data_ym(+) = c.set_ym
                       and d.wh_no(+) = a.wh_no
                       and d.mmcode(+) = a.mmcode
                       and a.pro_los_qty > 0
                        {1}
                )
                select round(sum(chk_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end) ), 2) as p_tot1,
                       round(sum(store_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end)), 2) as p_tot2,
                       TRUNC ( SUM (nvl(pro_los_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 0)) / 
                               decode (SUM (nvl(apl_outqty  *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 1)),
                                        0, 1 ,
                                        SUM (nvl(apl_outqty  *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 1)))
                                * 100, 5) AS p_tot3,
                       round(SUM (nvl(pro_los_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 0)), 2) AS p_tot4,
                       round(SUM (nvl(apl_outqty  *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 0)), 2) AS p_tot5
                  from cost_data
            ", wh_kind == "E" || wh_kind == "C" ? "_EC": string.Empty
            , wh_kind == "E" || wh_kind == "C" ? "   and b.mc_id = '1'" : string.Empty);

            p.Add(":chk_no", string.Format("{0}", chk_no));

            IEnumerable<CE0013Count> results = DBWork.Connection.Query<CE0013Count>(sql, p, DBWork.Transaction);
            if (results.Any() == false) {
                return new CE0013Count();
            }

            return results.First();
        }
        public CE0013Count GetCountN(string chk_no, string chk_level, string wh_kind)
        {
            var p = new DynamicParameters();

            string sql = string.Format(@"
                with temp_set_ym as (
                        select set_ym from MI_MNSET
                         where (select create_date as temp from CHK_MAST where chk_no = :chk_no)
                                 between set_btime and nvl(set_etime, sysdate)
                ),
                current_set_ym as (
                        select set_ym from MI_MNSET
                         where set_status = 'N'
                           and rownum = 1
                         order by set_ym desc
                ), 
                cost_data as (
                    select a.mmcode, a.last_qty, b.set_ym, b.avg_price,
                           (a.store_qty - 
                             (case
                                  WHEN a.STATUS_TOT = '1' THEN nvl(a.his_consume_qty_t1, 0)
                                     WHEN a.STATUS_TOT = '2' THEN nvl(a.his_consume_qty_t2, 0)
                                     WHEN a.STATUS_TOT = '3' THEN nvl(a.his_consume_qty_t3, 0)
                                     ELSE 0
                               end)   
                           ) as store_qty,
                           (select uprice from MI_MAST where mmcode = a.mmcode) as m_contprice,
                           (case
                              when status_tot = '1' then chk_qty1
                              when status_tot = '2' then chk_qty2
                              when status_tot = '3' then chk_qty3
                              else 0
                            end)    as chk_qty,
                           a.pro_los_qty , 
                           c.set_ym as chk_set_ym,
                           nvl(d.apl_outqty, a.apl_outqty) as apl_outqty,
                           (case
                                when c.set_ym = e.set_ym
                                    then 'Y' else 'N'
                            end) as is_current_ym
                      from CHK_DETAILTOT a, MI_WHCOST{0} b, temp_set_ym c, MI_WINVMON d, current_set_ym e
                     where a.chk_no = :chk_no
                       and a.mmcode = b.mmcode(+)
                       and b.data_ym(+) = c.set_ym
                       and d.data_ym(+) = c.set_ym
                       and d.wh_no(+) = a.wh_no
                       and d.mmcode(+) = a.mmcode
                       and a.pro_los_qty < 0
                        {1}
                )
                select round(sum(chk_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end) ), 2) as n_tot1,
                       round(sum(store_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end)), 2) as n_tot2,
                       TRUNC ( SUM (nvl(pro_los_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 0)) / 
                               decode (SUM (nvl(apl_outqty  *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 1)),
                                        0, 1 ,
                                        SUM (nvl(apl_outqty  *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 1)))
                                * 100, 5) AS n_tot3,
                       round(SUM (nvl(pro_los_qty *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 0)), 2) AS n_tot4,
                       round(SUM (nvl(apl_outqty  *  (case when is_current_ym = 'Y' then m_contprice else avg_price end), 0)), 2) AS n_tot5
                  from cost_data
            ", wh_kind == "E" || wh_kind == "C" ? "_EC" :string.Empty
            , wh_kind == "E" || wh_kind == "C" ? "   and b.mc_id = '1'" : string.Empty);

            p.Add(":chk_no", string.Format("{0}", chk_no));

            IEnumerable<CE0013Count> results = DBWork.Connection.Query<CE0013Count>(sql, p, DBWork.Transaction);
            if (results.Any() == false)
            {
                return new CE0013Count();
            }

            return results.First();
        }

        public IEnumerable<CE0013Data> PrintData(string chk_no, string chk_level, string wh_kind, string condition) {
            var p = new DynamicParameters();

            string sql = string.Format(@"
                with temp_set_ym as (
                        select set_ym from MI_MNSET
                         where (select create_date as temp from CHK_MAST where chk_no = :chk_no)
                                 between set_btime and set_ctime
                ),
                current_set_ym as (
                        select set_ym from MI_MNSET
                         where set_status = 'N'
                           and rownum = 1
                         order by set_ym desc
                ), 
                cost_data as (
                        select a.mmcode, a.chk_no,
                              (TRIM(NVL(mmname_c, ' ')) || '<br>' || TRIM(NVL(mmname_e, ' '))) as mmname,
                              (a.store_qty - 
                                (case
                                     WHEN a.STATUS_TOT = '1' THEN nvl(a.his_consume_qty_t1, 0)
                                        WHEN a.STATUS_TOT = '2' THEN nvl(a.his_consume_qty_t2, 0)
                                        WHEN a.STATUS_TOT = '3' THEN nvl(a.his_consume_qty_t3, 0)
                                        ELSE 0
                                  end)   
                              ) as store_qty,
                              b.set_ym, b.avg_price,
                               (select uprice from MI_MAST where mmcode = a.mmcode) as m_contprice,
                               (CASE
                                     WHEN STATUS_TOT = '1' THEN CHK_QTY1
                                     WHEN STATUS_TOT = '2' THEN CHK_QTY2
                                     WHEN STATUS_TOT = '3' THEN CHK_QTY3
                                     ELSE 0
                                END)    AS CHK_QTY,
                              a.pro_los_qty , --as diff_qty,
                              c.set_ym as chk_set_ym,
                              nvl(d.apl_outqty, a.apl_outqty) as apl_outqty,
                               a.consume_qty ,
                              (select listagg(chk_remark, '<br>') within group (order by store_loc)                     
                                 from chk_detail
                                where chk_no = (
                                               select CHK_NO
                                                 from chk_mast
                                                where chk_level = a.STATUS_TOT
                                                  and chk_no1 = :chk_no)
                                                  and mmcode = a. mmcode
                                                  and chk_remark is not null) as diff_remark ,
                              (case
                                   when c.set_ym = e.set_ym
                                       then 'Y' else 'N'
                               end) as is_current_ym,
                              f.wh_grade,
                              nvl(d.use_qty, 0) as use_qty
                         from CHK_DETAILTOT a, MI_WHCOST{0} b, temp_set_ym c, MI_WINVMON d, current_set_ym e, MI_WHMAST f
                        where a.chk_no = :chk_no
                          and a.mmcode = b.mmcode(+)
                          and b.data_ym(+) = c.set_ym
                          and d.data_ym(+) = c.set_ym
                          and d.wh_no(+) = a.wh_no
                          and d.mmcode(+) = a.mmcode
                          and f.wh_no = a.wh_no
                          {1}
                    )
                select a.mmcode, a.chk_no, a.mmname,a.store_qty, a.CHK_QTY,  
                       (pro_los_qty) as diff_qty,
                        (case when is_current_ym = 'Y' then m_contprice else avg_price end) as m_contprice,
                       round((a.pro_los_qty) *  (case when is_current_ym = 'Y' then m_contprice else avg_price end)) as diff_amount,
                       (case when
                               decode(apl_outqty, 0 ,   null,apl_outqty) is null
                               then a.pro_los_qty
                               else 
                               TRUNC ((pro_los_qty / apl_outqty * 100), 5)
                       end ) as diff_p,
                       a.diff_remark  ,
                       a.is_current_ym,
                       (case 
                         when a.wh_grade > '1' then a.use_qty
                            else nvl(a.apl_outqty, a.consume_qty)
                         end) as apl_outqty,
                       b.CHK_NO,
                       b.CHK_YM,
                       b.CHK_WH_NO, 
                       b.CHK_WH_GRADE,
                       b.CHK_WH_KIND,
                       b.CHK_PERIOD,
                       b.CHK_TYPE,
                       b.CHK_LEVEL,
                       b.CHK_NUM,
                       b.CHK_TOTAL,
                       b.CHK_STATUS,
                       b.CHK_KEEPER,
                       b.CHK_CLASS,
                       (select una from UR_ID where tuser = b.CHK_KEEPER) as CHK_KEEPER_NAME,
                       (select WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO = b.CHK_WH_NO) as WH_NAME,
                       (select DATA_VALUE || ' ' || DATA_DESC 
                          from PARAM_D
                         where GRP_CODE = 'CHK_MAST' 
                           and DATA_NAME = 'CHK_WH_KIND'
                           and DATA_VALUE = b.CHK_WH_KIND) as WH_KIND_NAME,
                       (select DATA_VALUE || ' ' || DATA_DESC 
                          from PARAM_D
                         where GRP_CODE = 'CHK_MAST' 
                           and DATA_NAME = 'CHK_PERIOD'
                           and DATA_VALUE = b.CHK_PERIOD) as CHK_PERIOD_NAME,
                       (select DATA_VALUE || ' ' || DATA_DESC 
                          from PARAM_D
                         where GRP_CODE = 'CHK_MAST' 
                           and DATA_NAME = 'CHK_LEVEL'
                           and DATA_VALUE = b.CHK_LEVEL) as CHK_LEVEL_NAME,
                       (select DATA_VALUE || ' ' || DATA_DESC 
                          from PARAM_D f
                         where GRP_CODE = 'CHK_MAST' 
                           and DATA_NAME = 'CHK_STATUS'
                           and DATA_VALUE = b.CHK_STATUS) as CHK_STATUS_NAME,
                       (select DATA_VALUE || ' ' || DATA_DESC 
                          from PARAM_D f
                         where GRP_CODE = 'CHK_MAST' 
                           and DATA_NAME = 'CHK_CLASS'
                           and DATA_VALUE = b.CHK_CLASS) as CHK_CLASS_NAME
                  from cost_data a , CHK_MAST b
                 where 1=1
                   and b.chk_no = a.chk_no
            ", wh_kind == "E" || wh_kind == "C" ? "_EC": string.Empty
            , wh_kind == "E" || wh_kind == "C" ? "   and b.mc_id = '1'" : string.Empty);

            if (condition != string.Empty)
            {
                sql += string.Format("     and (pro_los_qty) {0} 0", condition);
            }

            sql += "                     order by mmcode";

            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":condition", string.Format("{0}", condition));

            return DBWork.Connection.Query<CE0013Data>(sql, p, DBWork.Transaction);
        }
        
    }
}