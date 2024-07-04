using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CE0024Repository : JCLib.Mvc.BaseRepository
    {
        public CE0024Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CHK_G2_DETAILTOT> GetAll(string wh_no, string chk_ym, float f_u_price, float f_number, float f_amount, float miss_per, bool recheck_only, string start_date, string end_date) {

            string sql = @"select b.chk_no, b.wh_no, b.mmcode, c.mmname_c, c.mmname_e, c.base_unit,
                                  b.store_qty, b.gap_t, b.miss_per, b.chk_qty1, b.chk_qty2,
                                  b.chk_qty3, b.status_tot, b.status,
                                  ( case
                                      when (b.is_recheck = '1' and nvl(b.status, ' ') != 'R') then 'V'
                                        else ' '
                                    end) as is_recheck,
                                  (select seq from CHK_G2_WHINV where chk_no = b.chk_no and mmcode = b.mmcode) as seq,
                                  b.memo,
                                  chk_phr_aploutqty(:start_date, :end_date, b.wh_no, b.mmcode) as apl_outqty
                             from CHK_MAST a, CHK_G2_DETAILTOT b, MI_MAST c
                            where a.chk_wh_no = :wh_no
                              and a.chk_ym = :chk_ym
                              and b.chk_no = a.chk_no
                              and c.mmcode = b.mmcode";

            sql += GetFilter(f_u_price, f_number, f_amount, miss_per);

            if (recheck_only)
            {
                sql += "      and b.IS_RECHECK = '1'";
            }

            return DBWork.PagingQuery<CHK_G2_DETAILTOT>(sql, new { wh_no = wh_no, chk_ym = chk_ym, f_u_price = f_u_price, f_number = f_number, f_amount = f_amount, miss_per = miss_per,
                                                                   start_date = start_date, end_date = end_date }, DBWork.Transaction);
        }

        private string GetFilter(float f_u_price, float f_number, float f_amount, float miss_per)
        {
            string filter = string.Empty;

            if (f_u_price > 0)
            {
                filter += @"   and c.m_contprice > :f_u_price";
            }
            if (f_number > 0)
            {
                filter += @"   and ABS(b.gap_t ) > ABS(:f_number)";
            }
            if (f_amount > 0)
            {
                filter += @"   and ABS(b.gap_t * c.m_contprice) > ABS(:f_amount)";
            }
            if (miss_per > 0)
            {
                filter += @"   and ABS(b.miss_per) > ABS(:miss_per)";
            }

            return filter;
        }

        public IEnumerable<string> GetChkno(string chkno1) {
            string sql = @"select chk_no from chk_mast where chk_no1 = :chk_no and chk_level = '2'";
            return DBWork.Connection.Query<string>(sql, new { chk_no = chkno1 }, DBWork.Transaction);
        }

        public int UpdateG2DetailTotR(CHK_G2_DETAILTOT tot)
        {
            string sql = @"update CHK_G2_DETAILTOT
                              set status = 'R',
                                  is_recheck = '1'
                            where chk_no = :chk_no
                              and mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, new { chk_no = tot.CHK_NO, mmcode = tot.MMCODE }, DBWork.Transaction);
        }

        public int UpdateMast(string chk_no)
        {
            string sql = @"update CHK_MAST
                              set chk_status = '4', chk_num = '0', chk_total = '0'
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }

        public int DeleteG2Detail(string chk_no) {
            string sql = @"delete from CHK_G2_DETAIL
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }

        public int DeleteG2Whinv(string chk_no) {
            string sql = @"delete from CHK_G2_WHINV
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public int DeleteG2Updn(string chk_no)
        {
            string sql = @"delete from CHK_GRADE2_UPDN
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }

        public IEnumerable<CHK_G2_DETAILTOT> GetAllG2Details(string chk_ym) {
            string sql = @"select b.*
                             from CHK_MAST a, CHK_G2_DETAILTOT b
                            where a.chk_ym = :chk_ym
                              and a.chk_wh_grade = '2'
                              and a.chk_wh_kind = '0'
                              and a.chk_level = '1'
                              and b.chk_no = a.chk_no";
            return DBWork.Connection.Query<CHK_G2_DETAILTOT>(sql, new { chk_ym = chk_ym }, DBWork.Transaction);
        }

        public MI_MAST GetMimast(string mmcode) {
            string sql = @"select * from MI_MAST where mmcode = :mmcode";
            return DBWork.Connection.QueryFirst<MI_MAST>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }
        public int InsertChkDetailTot(CHK_DETAILTOT tot) {
            string sql = @"insert into CHK_DETAILTOT
                                  (chk_no, mmcode, mmname_c, mmname_e, base_unit,
                                   m_contprice, wh_no, store_loc, mat_class, m_storeid,
                                   store_qty, store_qtyc, store_qtym, store_qtys,
                                   last_qty, last_qtyc, last_qtym, last_qtys,
                                   gap_t, gap_c, gap_m, gap_s, pro_los_qty, pro_los_amount,
                                   miss_per, miss_perc, miss_perm, miss_pers, apl_outqty,
                                   chk_remark, chk_qty1, chk_qty2, chk_qty3, status_tot, 
                                   create_date, create_user, update_time, update_user, update_ip)
                           values (:chk_no, :mmcode, :mmname_c, :mmname_e, :base_unit, 
                                   :m_contprice, :wh_no, :store_loc, :mat_class, :m_storeid,
                                   :store_qty, :store_qtyc, :store_qtym, :store_qtys,
                                   :last_qty, :last_qtyc, :last_qtym, :last_qtys,
                                   :gap_t, :gap_c, :gap_m, :gap_s, :pro_los_qty, :pro_los_amount,
                                   :miss_per, :miss_perc, :miss_perm, :miss_pers, :apl_outqty,
                                   :chk_remark, :chk_qty1, :chk_qty2, :chk_qty3, :status_tot,
                                   sysdate, :create_user, sysdate, :update_user, :update_ip)";
            return DBWork.Connection.Execute(sql, tot, DBWork.Transaction);
        }

        public IEnumerable<string> GetDetailtotChkno(string wh_no, string chk_ym) {
            string sql = @"select distinct a.chk_no from CHK_DETAILTOT a, CHK_MAST b
                            where b.chk_wh_no = :wh_no
                              and b.chk_ym = :chk_ym
                              and b.chk_level = '1'
                              and a.chk_no = b.chk_no";
            return DBWork.Connection.Query<string>(sql, new { chk_ym = chk_ym, wh_no = wh_no }, DBWork.Transaction);
        }
        public int InsertMaster(CHK_MAST mast) {
            string sql = @"insert into CHK_MAST
                           values ( :chk_no, :chk_ym, :chk_wh_no, :chk_wh_grade,
                                    :chk_wh_kind, :chk_class, :chk_period, :chk_type, :chk_level,
                                    :chk_num, :chk_total, :chk_status, :chk_keeper, :chk_no1,
                                    sysdate, :create_user, sysdate, :update_user, :update_ip)";
            return DBWork.Connection.Execute(sql, mast, DBWork.Transaction);
        }
        public CHK_MAST GetChkMast(string chk_no1) {
            string sql = @"select * from CHK_MAST where chk_no = :chk_no1";
            return DBWork.Connection.QueryFirst<CHK_MAST>(sql, new { chk_no1 = chk_no1 }, DBWork.Transaction);
        }

        public IEnumerable<CHK_MAST> GetG2ChkStatus(string chk_ym)
        {
            string sql = @"select chk_no, chk_wh_no, chk_status, chk_ym, chk_level
                             from CHK_MAST
                            where chk_ym = :chk_ym
                              and chk_wh_grade = '2'
                              and chk_wh_kind = '0'
                            order by chk_level desc";
            return DBWork.Connection.Query<CHK_MAST>(sql, new { chk_ym = chk_ym }, DBWork.Transaction);
        }

        public int ClearRecheckMark(string chk_no1)
        {
            string sql = @"update CHK_G2_DETAILTOT
                              set IS_RECHECK = '0'
                            where chk_no = :chk_no1
                              and nvl(status, ' ') != 'R'";
            return DBWork.Connection.Execute(sql, new { chk_no1 = chk_no1 }, DBWork.Transaction);
        }


        public bool CheckRecheckItemExists(string chk_no1) {
            string sql = @"select 1 from CHK_G2_DETAILTOT
                            where chk_no = :chk_no1
                              and status = 'R'";
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         chk_no1 = chk_no1
                                                     },
                                                     DBWork.Transaction) == null);
        }

        public bool CheckDetailtotExists(string chk_ym) {
            string sql = @"select 1 from CHK_DETAILTOT
                            where chk_no in (select chk_no from CHK_MAST
                                              where chk_ym = :chk_ym 
                                                and chk_wh_grade = '2'
                                                and chk_wh_kind = '0')
                              and rownum = 1";
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         chk_ym = chk_ym
                                                     },
                                                     DBWork.Transaction) == null);
        }

        public int UpdateMemo(CHK_G2_DETAILTOT tot) {
            string sql = @"update CHK_G2_DETAILTOT
                              set memo = :memo
                            where chk_no = :chk_no
                              and mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, tot, DBWork.Transaction);
        }


        #region 2020-09-08 新增: 目前藥局盤盈虧結果查詢
        private string GetChkResultSQL(string wh_no, string content_type) {
            string sql = string.Format(@"
                            with chk_nos as (
                               select chk_no from CHK_MAST a
                                where substr(a.chk_ym, 1, 5) = :CHK_YM 
                                  and a.chk_wh_grade = '2' 
                                  and a.chk_wh_kind = '0' 
                                  and a.chk_level = '1' 
                                  {0}
                           ),
                            finish_chk_nos as (
                                select distinct a.chk_no 
                                  from chk_nos a, CHK_G2_DETAILTOT b
                                 where a.chk_no = b.chk_no
                            ),
                           current_set_ym as (
                                   select set_ym from MI_MNSET
                                    where set_status = 'N'
                                      and rownum = 1
                                    order by set_ym desc
                           ), 
                           memo_data as (
                               select a.wh_no, a.mmcode, a.memo
                                 from CHK_G2_DETAILTOT a, finish_chk_nos b
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
                                       chk_phr_aploutqty(:start_date, :end_date, b.wh_no, b.mmcode) as apl_outqty,
                                       (case
                                            when :CHK_YM = f.set_ym
                                                then 'Y' else 'N'
                                        end) as is_current_ym
                                  from finish_chk_nos A, CHK_G2_DETAILTOT B, MI_WHCOST c,  current_set_ym f
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
            ", wh_no == string.Empty ? string.Empty : " and a.chk_wh_no = :wh_no");

            if (content_type != string.Empty)
            {
                if (content_type == "1")
                {
                    sql += " and  a.GAP_T > 0";
                }
                else if (content_type == "2")
                {
                    sql += " and  a.GAP_T < 0";
                }
            }
            sql += @" 
                      order by mmcode";
            return sql;
        }
        public IEnumerable<CE0027> GetChkResult(string chk_ym, string wh_no, string content_type, string start_date, string end_date) {
            var p = new DynamicParameters();

            string sql = GetChkResultSQL(wh_no, content_type);

            p.Add(":chk_ym", string.Format("{0}", chk_ym));
            p.Add(":start_date", string.Format("{0}", start_date));
            p.Add(":end_date", string.Format("{0}", end_date));
            p.Add(":wh_no", string.Format("{0}", wh_no));

            return DBWork.PagingQuery<CE0027>(sql, p, DBWork.Transaction);
        }

        private string GetChkCountsSQl(string wh_no) {
            string sql = string.Format(@"
                           with chk_nos as (        -- 取得盤點單號
                               select chk_no from CHK_MAST a
                                where substr(a.chk_ym, 1, 5) = :CHK_YM 
                                  and a.chk_wh_grade = '2' 
                                  and a.chk_wh_kind = '0' 
                                  and a.chk_level = '1' 
                                    {0}
                           ),
                           finish_chk_nos as (
                                select distinct a.chk_no 
                                  from chk_nos a, CHK_G2_DETAILTOT b
                                 where a.chk_no = b.chk_no
                            ),
                           current_set_ym as (      -- 取得目前月結年月
                                   select set_ym from MI_MNSET
                                    where set_status = 'N'
                                      and rownum = 1
                                    order by set_ym desc
                           ), 
                           memo_data as (           -- 取得備註資料
                               select a.wh_no, a.mmcode, a.memo
                                 from CHK_G2_DETAILTOT a, finish_chk_nos b
                                where a.chk_no = b.chk_no
                                  and a.memo is not null
                           ),
                           cost_data as (           -- 取得所有藥局盤點資料
                                select b.wh_no, b.mmcode, 
                                       b.store_qty,
                                       c.avg_price, b.gap_t, 
                                       (CASE
                                             WHEN b.STATUS_TOT = '1' THEN b.CHK_QTY1
                                             WHEN b.STATUS_TOT = '2' THEN b.CHK_QTY2
                                             WHEN b.STATUS_TOT = '3' THEN b.CHK_QTY3
                                             ELSE 0
                                        END)    AS CHK_QTY,
                                       chk_phr_aploutqty(:start_date, :end_date, b.wh_no, b.mmcode) as apl_outqty,
                                       (case
                                            when :CHK_YM = f.set_ym
                                                then 'Y' else 'N'
                                        end) as is_current_ym
                                  from finish_chk_nos A, CHK_G2_DETAILTOT B, MI_WHCOST c , current_set_ym f
                                 where A.CHK_NO = B.CHK_NO
                                   and b.mmcode = c.mmcode(+)
                                   and c.data_ym(+) = :CHK_YM
                           ),                           
                           sum_data as (            -- 取得各院內碼加總資料
                            select mmcode, avg_price, is_current_ym, 
                                   sum(chk_qty) as chk_qty, 
                                   sum(store_qty) as store_qty,
                                   sum(gap_t) as gap_t, 
                                   sum(apl_outqty) as apl_outqty
                             from cost_data
                             group by mmcode, avg_price, is_current_ym
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
                            ",
                            wh_no == string.Empty ? string.Empty : " and a.chk_wh_no = :wh_no");
            return sql;
        }
        public IEnumerable<CE0027Count> GetChkCounts(string chk_ym, string wh_no, string start_date, string end_date)
        {
            var p = new DynamicParameters();

            string sql = GetChkCountsSQl(wh_no);

            p.Add(":chk_ym", string.Format("{0}", chk_ym));
            p.Add(":start_date", string.Format("{0}", start_date));
            p.Add(":end_date", string.Format("{0}", end_date));
            p.Add(":wh_no", string.Format("{0}", wh_no));

            return DBWork.Connection.Query<CE0027Count>(sql, p, DBWork.Transaction);
        }

        #endregion

        #region 2020-09-10 
        public string GetStartDate(string chk_ym) {
            if (chk_ym == "10909") {
                return "1090606";
            }
            if (chk_ym == "10908")
            {
                return "1090706";
            }
            string sql = @"select distinct chk_period 
                             from CHK_MAST 
                            where chk_ym = :chk_ym
                              and chk_wh_grade = '2'
                              and chk_wh_kind = '0'";
            string chk_period = DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_ym = chk_ym }, DBWork.Transaction);

            string start_date_sql = chk_period == "S" ? "twn_pym(twn_pym(twn_pym(:chk_ym)))" : "twn_pym(:chk_ym)";
            sql = string.Format(@"
                    select twn_date(min(chk_pre_date))
                      from CHK_G2_WHINV a
                     where chk_no in (select chk_no from CHK_MAST
                                       where chk_ym = {0}
                                         and chk_wh_grade = '2'
                                         and chk_wh_kind = '0'
                                         and chk_level = '1')",
                                         start_date_sql
                );
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_ym = chk_ym }, DBWork.Transaction);

        }
        public string GetEndDate(string chk_ym) {
            string sql = @"
                    select twn_date(min(chk_pre_date))
                      from CHK_G2_WHINV a
                     where chk_no in (select chk_no from CHK_MAST
                                       where chk_ym = :chk_ym
                                         and chk_wh_grade = '2'
                                         and chk_wh_kind = '0'
                                         and chk_level = '1')";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_ym = chk_ym }, DBWork.Transaction);
        }

        public string GetAploutqty(string start_date, string end_date, string wh_no, string mmcode)
        {
            string sql = @"select chk_phr_aploutqty(:start_date, :end_date, :wh_no, :mmcode) from dual";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { start_date = start_date, end_date = end_date, wh_no = wh_no, mmcode = mmcode }, DBWork.Transaction);
        }
        #endregion

        #region 2020-10-20
        public DataTable GetChkResultExcel(string chk_ym, string wh_no, string content_type, string start_date, string end_date) {
            var p = new DynamicParameters();
            string sql = GetChkResultSQL(wh_no, content_type);
            sql = string.Format(@"
                        select mmcode as 院內碼,
                               mmname_c as 中文名稱,
                               mmname_e as 英文名稱,
                               base_unit as 劑量單位,
                               store_qty as 電腦量,
                               chk_qty as 盤點量,
                               gap_t as 誤差量,
                               avg_price as 移動平均價,
                               diff_cost as 誤差金額,
                               diff_p as 誤差百分比,
                               apl_outqty as 消耗量
                          from (
                                {0}
                               )
                         order by mmcode      
                  ", sql);

            p.Add(":chk_ym", string.Format("{0}", chk_ym));
            p.Add(":start_date", string.Format("{0}", start_date));
            p.Add(":end_date", string.Format("{0}", end_date));
            p.Add(":wh_no", string.Format("{0}", wh_no));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        #endregion
    }
}