using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using WebApp.Models.AA;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AA
{
    public class AA0138ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public int F6 { get; set; }
        public int F7 { get; set; }
        public int F8 { get; set; }
        public int F9 { get; set; }
        public string F10 { get; set; }

        public string STORE_QTY { get; set; }
        public string STORE_QTY_TIME { get; set; }

        public string F11 { get; set; }
    }

    public class AA0138ReportWH {
        public string WH_NO { get; set; }           // 庫房代碼
        public string WH_NAME { get; set; }         // 庫房名稱
        public string MMCODE_CNT { get; set; }      // 清點項數
        public string IS_CHK_BATCH { get; set; }    // 是否開單
    }
    public class AA0138Repository : JCLib.Mvc.BaseRepository
    {
        public AA0138Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0133> GetAll(string drugType, bool isHospCode0)
        {
            var p = new DynamicParameters();

            var sql = string.Empty;
            if (isHospCode0)
            {
                sql = @"select mmcode,
                               mmcode_namec(mmcode) as mmname_c,
                               mmcode_name(mmcode) as mmname_e,
                               base_unit(mmcode) as base_unit,
                               sum(pmn_invqty) as pmn_invqty,
                               sum(mn_inqty) as mn_inqty,
                               sum(use_qty) as use_qty,
                               sum(inv_qty) as inv_qty,
                               sum(chk_qty) as chk_qty,
                               sum(store_qty) as store_qty,
                               sum(order_total) as order_total
                          from (
                                 with chk_nos as (
                                      select chk_no, chk_wh_no from CHK_MAST 
                                       where substr(chk_ym, 1,5) = cur_setym
                                         and ( (chk_wh_grade='3' and chk_wh_kind='0') or
                                                chk_wh_no in('ER1','ERC') )
                                         and create_user = 'BATCH'
                                         and chk_type = :drugtype
                                 ),
                                 mnset_date as (
                                      select set_ctime from MI_MNSET
                                       where set_ym < cur_setym
                                         and rownum = 1
                                       order by set_ym desc
                                 ),
                                 chk_datas as (
                                      select a.wh_no, a.mmcode,
                                             (case
                                                when status_tot = '1' then a.chk_qty1
                                                when status_tot = '2' then a.chk_qty2
                                                when status_tot = '3' then a.chk_qty3
                                              end) as chk_qty,
                                             a.store_qty as store_qty,
                                             (case
                                                when status_tot = '1' then a.store_qty_time1
                                                when status_tot = '2' then a.store_qty_time2
                                                when status_tot = '3' then a.store_qty_time3
                                              end) as store_qty_time,
                                             decode(c.his_consume_datatime, 
                                                    null, twn_date((case
                                                                      when status_tot = '1' then a.store_qty_time1
                                                                      when status_tot = '2' then a.store_qty_time2
                                                                      when status_tot = '3' then a.store_qty_time3
                                                                    end)
                                                                 )||'055959',
                                                    c.his_consume_datatime) as his_consume_datatime_e,
                                             decode(c.his_consume_datatime, 
                                                    null, twn_date((case
                                                                      when status_tot = '1' then a.store_qty_time1
                                                                      when status_tot = '2' then a.store_qty_time2
                                                                      when status_tot = '3' then a.store_qty_time3
                                                                    end)
                                                                 )||'000000',
                                                    substr(c.his_consume_datatime, 1,7) ||'000000') as his_consume_datatime_s,
                                             c.his_consume_qty_t
                                        from CHK_DETAILTOT a, chk_nos b, chk_detail c
                                       where a.chk_no = b.chk_no
                                         and c.chk_no = a.chk_no
                                         and c.mmcode = a.mmcode
                                 ),
                                 mi_consume_date_datas as (
                                       select a.wh_no, a.parent_ordercode as mmcode, 
                                              sum(parent_consume_qty) as consume_date_sum
                                         from MI_CONSUME_DATE a, chk_datas b, mnset_date c
                                        where a.wh_no=  b.wh_no
                                          and a.mmcode = b.mmcode
                                          and a.data_date < twn_date(b.store_qty_time)
                                          and a.data_date > twn_date(c.set_ctime)
                                        group by a.wh_no, a.parent_ordercode
                                 ),
                                 his_consume_d_datas as (
                                       select a.stockcode as wh_no, 
                                              a.parent_ordercode as mmcode,
                                              sum(parent_consume_qty) as consume_d_sum
                                         from HIS_CONSUME_D a, chk_datas b
                                        where a.STOCKCODE = b.wh_no
                                          and a.parent_ordercode = b.mmcode
                                          and (a.workdate||a.worktime) between b.HIS_CONSUME_DATATIME_s and b.HIS_CONSUME_DATATIME_e
                                        group by a.stockcode, a.parent_ordercode
                                 )
                                 select a.wh_no, a.mmcode, 
                                        (select inv_qty from MI_WINVMON
                                          where data_ym = twn_yyymm(add_months(sysdate, -2))
                                            and wh_no = a.wh_no
                                            and mmcode = a.mmcode) as pmn_invqty,
                                        b.apl_inqty as mn_inqty,
                                        (
                                            b.APL_OUTQTY + b.TRN_OUTQTY + b.ADJ_OUTQTY + b.BAK_OUTQTY + b.REJ_OUTQTY
                                          + b.DIS_OUTQTY + b.EXG_OUTQTY + b.MIL_OUTQTY + b.INVENTORYQTY + b.USE_QTY
                                          - b.TRN_INQTY - b.ADJ_INQTY - b.BAK_INQTY - b.EXG_INQTY - b.MIL_INQTY
                                        ) as use_qty,
                                        b.INV_QTY as inv_qty,
                                        a.chk_qty,
                                        a.store_qty as store_qty,
                                        a.store_qty_time,
                                        c.consume_date_sum,
                                        d.consume_d_sum,
                                        (nvl(c.consume_date_sum, 0) + nvl(d.consume_d_sum, 0) + nvl(his_consume_qty_t, 0)) as order_total
                                   from chk_datas a, MI_WINVMON b, mi_consume_date_datas c, his_consume_d_datas d
                                  where b.wh_no = a.wh_no
                                    and b.mmcode = a.mmcode
                                    and b.data_ym = twn_pym(cur_setym)
                                    and a.wh_no = c.wh_no(+)
                                    and a.mmcode = c.mmcode(+)
                                    and a.wh_no = d.wh_no(+)
                                    and a.mmcode = d.mmcode(+)
                                  order by a.mmcode
                               )
                         group by mmcode
                         order by mmcode";
            }
            else {
                string drugType_string = string.Empty;
                if (drugType == "3")
                {
                    drugType_string = @" and exists (select 1 from MI_MAST where mmcode=a.mmcode and e_restrictcode in ('1','2','3'))";
                }
                if (drugType == "4")
                {
                    drugType_string = @" and exists (select 1 from MI_MAST where mmcode=a.mmcode and e_restrictcode in ('4'))";
                }

                sql = string.Format(@" 
                        select mmcode, 
                                    mmcode_namec(mmcode) as mmname_c,
                                    mmcode_name(mmcode) as mmname_e,      
                                    base_unit(mmcode) as base_unit,
                               sum(inv_qty) as inv_qty,
                               sum(chk_qty) as chk_qty
                        from (
                            select a.inid, a.mmcode,
                                    --mmcode_namec(a.mmcode) as mmname_c,
                                    --mmcode_name(a.mmcode) as mmname_e,      
                                    --base_unit(a.mmcode) as base_unit,
                                    a.inv_qty, a.chk_qty
                               from DGMISS_CHK a
                              where a.data_ym=twn_pym(cur_setym)
                                {0}
                              order by a.mmcode
                        ) a
                    group by mmcode
                    order by mmcode
                     ", !string.IsNullOrEmpty(drugType) ? drugType_string : string.Empty
                     );
            }

            p.Add(":drugtype", string.Format("{0}", drugType));

            return DBWork.PagingQuery<AA0133>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string drugType, bool showStoreQty, bool isHospCode0)
        {
            var p = new DynamicParameters();

            string showStoreQtyString = @", sum(store_qty) as 盤點電腦量 ";

            var sql = string.Empty;

            if (isHospCode0) {
                sql = string.Format(@"select mmcode as 院內碼,
                                             mmcode_namec(mmcode) as 中文品名,
                                             mmcode_name(mmcode) as 英文品名,
                                             base_unit(mmcode) as 單位,
                                             sum(pmn_invqty) as 上期結存,
                                             sum(mn_inqty) as 本月進貨,
                                             sum(use_qty) as 本月消耗,
                                             sum(inv_qty) as 本月結存,
                                             sum(chk_qty) as 實際清點,
                                             sum(order_total) as 期初至清點時間醫令扣庫量
                                             {0}
                                        from (
                                               with chk_nos as (
                                                    select chk_no, chk_wh_no from CHK_MAST 
                                                     where substr(chk_ym, 1,5) = cur_setym 
                                                       and ( (chk_wh_grade='3' and chk_wh_kind='0') or
                                                              chk_wh_no in('ER1','ERC') )
                                                       and create_user = 'BATCH'
                                                       and chk_type = :drugtype
                                               ),
                                               mnset_date as (
                                                    select set_ctime from MI_MNSET
                                                     where set_ym < cur_setym
                                                       and rownum = 1
                                                     order by set_ym desc
                                               ),
                                               chk_datas as (
                                                    select a.wh_no, a.mmcode,
                                                           (case
                                                              when status_tot = '1' then a.chk_qty1
                                                              when status_tot = '2' then a.chk_qty2
                                                              when status_tot = '3' then a.chk_qty3
                                                            end) as chk_qty,
                                                           a.store_qty as store_qty,
                                                           (case
                                                              when status_tot = '1' then a.store_qty_time1
                                                              when status_tot = '2' then a.store_qty_time2
                                                              when status_tot = '3' then a.store_qty_time3
                                                            end) as store_qty_time,
                                                           decode(c.his_consume_datatime, 
                                                                  null, twn_date((case
                                                                                    when status_tot = '1' then a.store_qty_time1
                                                                                    when status_tot = '2' then a.store_qty_time2
                                                                                    when status_tot = '3' then a.store_qty_time3
                                                                                  end)
                                                                               )||'055959',
                                                                  c.his_consume_datatime) as his_consume_datatime_e,
                                                           decode(c.his_consume_datatime, 
                                                                  null, twn_date((case
                                                                                    when status_tot = '1' then a.store_qty_time1
                                                                                    when status_tot = '2' then a.store_qty_time2
                                                                                    when status_tot = '3' then a.store_qty_time3
                                                                                  end)
                                                                               )||'000000',
                                                                  substr(c.his_consume_datatime, 1,7) ||'000000') as his_consume_datatime_s,
                                                           c.his_consume_qty_t
                                                      from CHK_DETAILTOT a, chk_nos b, chk_detail c
                                                     where a.chk_no = b.chk_no
                                                       and c.chk_no = a.chk_no
                                                       and c.mmcode = a.mmcode
                                               ),
                                               mi_consume_date_datas as (
                                                     select a.wh_no, a.parent_ordercode as mmcode, 
                                                            sum(parent_consume_qty) as consume_date_sum
                                                       from MI_CONSUME_DATE a, chk_datas b, mnset_date c
                                                      where a.wh_no=  b.wh_no
                                                        and a.mmcode = b.mmcode
                                                        and a.data_date < twn_date(b.store_qty_time)
                                                        and a.data_date > twn_date(c.set_ctime)
                                                      group by a.wh_no, a.parent_ordercode
                                               ),
                                               his_consume_d_datas as (
                                                     select a.stockcode as wh_no, 
                                                            a.parent_ordercode as mmcode,
                                                            sum(parent_consume_qty) as consume_d_sum
                                                       from HIS_CONSUME_D a, chk_datas b
                                                      where a.STOCKCODE = b.wh_no
                                                        and a.parent_ordercode = b.mmcode
                                                        and (a.workdate||a.worktime) between b.HIS_CONSUME_DATATIME_s and b.HIS_CONSUME_DATATIME_e
                                                      group by a.stockcode, a.parent_ordercode
                                               )
                                               select a.wh_no, a.mmcode, 
                                                      (select inv_qty from MI_WINVMON
                                                        where data_ym = twn_yyymm(add_months(sysdate, -2))
                                                          and wh_no = a.wh_no
                                                          and mmcode = a.mmcode) as pmn_invqty,
                                                      b.apl_inqty as mn_inqty,
                                                      (
                                                          b.APL_OUTQTY + b.TRN_OUTQTY + b.ADJ_OUTQTY + b.BAK_OUTQTY + b.REJ_OUTQTY
                                                        + b.DIS_OUTQTY + b.EXG_OUTQTY + b.MIL_OUTQTY + b.INVENTORYQTY + b.USE_QTY
                                                        - b.TRN_INQTY - b.ADJ_INQTY - b.BAK_INQTY - b.EXG_INQTY - b.MIL_INQTY
                                                      ) as use_qty,
                                                      b.INV_QTY as inv_qty,
                                                      a.chk_qty,
                                                      a.store_qty as store_qty,
                                                      a.store_qty_time,
                                                      c.consume_date_sum,
                                                      d.consume_d_sum,
                                                      (nvl(c.consume_date_sum, 0) + nvl(d.consume_d_sum, 0) + nvl(a.his_consume_qty_t, 0)) as order_total
                                                 from chk_datas a, MI_WINVMON b, mi_consume_date_datas c, his_consume_d_datas d
                                                where b.wh_no = a.wh_no
                                                  and b.mmcode = a.mmcode
                                                  and b.data_ym = twn_pym(cur_setym)
                                                  and a.wh_no = c.wh_no(+)
                                                  and a.mmcode = c.mmcode(+)
                                                  and a.wh_no = d.wh_no(+)
                                                  and a.mmcode = d.mmcode(+)
                                                order by a.mmcode 
                                             )
                                       group by mmcode
                                       order by mmcode",
                                       showStoreQty ? showStoreQtyString : string.Empty);
            }
            else {
                string drugType_string = string.Empty;
                if (drugType == "3")
                {
                    drugType_string = @" and exists (select 1 from MI_MAST where mmcode=a.mmcode and e_restrictcode in ('1','2','3'))";
                }
                if (drugType == "4")
                {
                    drugType_string = @" and exists (select 1 from MI_MAST where mmcode=a.mmcode and e_restrictcode in ('4'))";
                }

                sql = string.Format(@" 
                        select mmcode 院內碼, 
                                    mmcode_namec(mmcode) as 中文品名,
                                    mmcode_name(mmcode) as 英文品名,      
                                    base_unit(mmcode) as 單位,
                               sum(inv_qty) as 本月結存,
                               sum(chk_qty) as 實際清點
                        from (
                            select a.inid, a.mmcode,
                                    --mmcode_namec(a.mmcode) as mmname_c,
                                    --mmcode_name(a.mmcode) as mmname_e,      
                                    --base_unit(a.mmcode) as base_unit,
                                    a.inv_qty, a.chk_qty
                               from DGMISS_CHK a
                              where a.data_ym=twn_pym(cur_setym)
                                {0}
                              order by a.mmcode
                        )
                    group by mmcode
                    order by mmcode
                     ", !string.IsNullOrEmpty(drugType) ? drugType_string : string.Empty
                     );
            }

            p.Add(":drugtype", string.Format("{0}", drugType));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<AA0138ReportMODEL> GetPrintData(string drugType, bool isHospCode0)
        {
            var p = new DynamicParameters();

            var sql = string.Empty;

            if (isHospCode0)
            {
                sql = @"select mmcode as F2,
                               mmcode_namec(mmcode) as F3,
                               mmcode_name(mmcode) as F4,
                               base_unit(mmcode) as F5,
                               sum(pmn_invqty) as F6,
                               sum(mn_inqty) as F7,
                               sum(use_qty) as F8,
                               sum(inv_qty) as F9,
                               sum(chk_qty) as F10,
                               sum(order_total) as F11,
                               sum(store_qty) as store_qty
                          from (
                                with chk_nos as (
                                     select chk_no, chk_wh_no from CHK_MAST 
                                      where substr(chk_ym, 1,5) = cur_setym 
                                        and ( (chk_wh_grade='3' and chk_wh_kind='0') or
                                               chk_wh_no in('ER1','ERC') )
                                        and create_user = 'BATCH'
                                        and chk_type = :drugtype
                                ),
                                mnset_date as (
                                     select set_ctime from MI_MNSET
                                      where set_ym < cur_setym
                                        and rownum = 1
                                      order by set_ym desc
                                ),
                                chk_datas as (
                                     select a.wh_no, a.mmcode,
                                            (case
                                               when status_tot = '1' then a.chk_qty1
                                               when status_tot = '2' then a.chk_qty2
                                               when status_tot = '3' then a.chk_qty3
                                             end) as chk_qty,
                                            a.store_qty as store_qty,
                                            (case
                                               when status_tot = '1' then a.store_qty_time1
                                               when status_tot = '2' then a.store_qty_time2
                                               when status_tot = '3' then a.store_qty_time3
                                             end) as store_qty_time,
                                            decode(c.his_consume_datatime, 
                                                   null, twn_date((case
                                                                     when status_tot = '1' then a.store_qty_time1
                                                                     when status_tot = '2' then a.store_qty_time2
                                                                     when status_tot = '3' then a.store_qty_time3
                                                                   end)
                                                                )||'055959',
                                                   c.his_consume_datatime) as his_consume_datatime_e,
                                            decode(c.his_consume_datatime, 
                                                   null, twn_date((case
                                                                     when status_tot = '1' then a.store_qty_time1
                                                                     when status_tot = '2' then a.store_qty_time2
                                                                     when status_tot = '3' then a.store_qty_time3
                                                                   end)
                                                                )||'000000',
                                                   substr(c.his_consume_datatime, 1,7) ||'000000') as his_consume_datatime_s,
                                            c.his_consume_qty_t
                                       from CHK_DETAILTOT a, chk_nos b, chk_detail c
                                      where a.chk_no = b.chk_no
                                        and c.chk_no = a.chk_no
                                        and c.mmcode = a.mmcode
                                ),
                                mi_consume_date_datas as (
                                      select a.wh_no, a.parent_ordercode as mmcode, 
                                             sum(parent_consume_qty) as consume_date_sum
                                        from MI_CONSUME_DATE a, chk_datas b, mnset_date c
                                       where a.wh_no=  b.wh_no
                                         and a.mmcode = b.mmcode
                                         and a.data_date < twn_date(b.store_qty_time)
                                         and a.data_date > twn_date(c.set_ctime)
                                       group by a.wh_no, a.parent_ordercode
                                ),
                                his_consume_d_datas as (
                                      select a.stockcode as wh_no, 
                                             a.parent_ordercode as mmcode,
                                             sum(parent_consume_qty) as consume_d_sum
                                        from HIS_CONSUME_D a, chk_datas b
                                       where a.STOCKCODE = b.wh_no
                                         and a.parent_ordercode = b.mmcode
                                         and (a.workdate||a.worktime) between b.HIS_CONSUME_DATATIME_s and b.HIS_CONSUME_DATATIME_e
                                       group by a.stockcode, a.parent_ordercode
                                )
                                select a.wh_no, a.mmcode, 
                                       (select inv_qty from MI_WINVMON
                                         where data_ym = twn_yyymm(add_months(sysdate, -2))
                                           and wh_no = a.wh_no
                                           and mmcode = a.mmcode) as pmn_invqty,
                                       b.apl_inqty as mn_inqty,
                                       (
                                           b.APL_OUTQTY + b.TRN_OUTQTY + b.ADJ_OUTQTY + b.BAK_OUTQTY + b.REJ_OUTQTY
                                         + b.DIS_OUTQTY + b.EXG_OUTQTY + b.MIL_OUTQTY + b.INVENTORYQTY + b.USE_QTY
                                         - b.TRN_INQTY - b.ADJ_INQTY - b.BAK_INQTY - b.EXG_INQTY - b.MIL_INQTY
                                       ) as use_qty,
                                       b.INV_QTY as inv_qty,
                                       a.chk_qty,
                                       a.store_qty as store_qty,
                                       a.store_qty_time,
                                       c.consume_date_sum,
                                       d.consume_d_sum,
                                       (nvl(c.consume_date_sum, 0) + nvl(d.consume_d_sum, 0) + nvl(a.his_consume_qty_t, 0)) as order_total
                                  from chk_datas a, MI_WINVMON b, mi_consume_date_datas c, his_consume_d_datas d
                                 where b.wh_no = a.wh_no
                                   and b.mmcode = a.mmcode
                                   and b.data_ym = twn_pym(cur_setym)
                                   and a.wh_no = c.wh_no(+)
                                   and a.mmcode = c.mmcode(+)
                                   and a.wh_no = d.wh_no(+)
                                   and a.mmcode = d.mmcode(+)
                                 order by a.mmcode
                               )
                         group by mmcode
                         order by mmcode";
            }
            else {
                string drugType_string = string.Empty;
                if (drugType == "3")
                {
                    drugType_string = @" and exists (select 1 from MI_MAST where mmcode=a.mmcode and e_restrictcode in ('1','2','3'))";
                }
                if (drugType == "4")
                {
                    drugType_string = @" and exists (select 1 from MI_MAST where mmcode=a.mmcode and e_restrictcode in ('4'))";
                }

                sql = string.Format(@" 
                        select mmcode F2, 
                                    mmcode_namec(mmcode) as F3,
                                    mmcode_name(mmcode) as F4,      
                                    base_unit(mmcode) as F5,
                               sum(inv_qty) as F9,
                               sum(chk_qty) as F10
                        from (
                            select a.inid, a.mmcode,
                                    --mmcode_namec(a.mmcode) as mmname_c,
                                    --mmcode_name(a.mmcode) as mmname_e,      
                                    --base_unit(a.mmcode) as base_unit,
                                    a.inv_qty, a.chk_qty
                               from DGMISS_CHK a
                              where a.data_ym=twn_pym(cur_setym)
                                {0}
                              order by a.mmcode
                        )
                    group by mmcode
                    order by mmcode
                     ", !string.IsNullOrEmpty(drugType) ? drugType_string : string.Empty
                     );
            }

            p.Add(":drugtype", string.Format("{0}", drugType));


            return DBWork.Connection.Query<AA0138ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public DataTable GetWhExcel(string drugType, bool isHospCode0){
            var p = new DynamicParameters();

            string sql = string.Empty;

            if (isHospCode0) {
                sql = @"
                   select A.WH_NO as 庫房代碼,
                          A.WH_NAME as 庫房名稱,
                          B.mmcode_cnt as 清點項數, 
                          (case
                              when ((select 1 from CHK_MAST
                                      where chk_wh_no = a.wh_no
                                        and chk_ym = (twn_yyymm(sysdate)||'01') 
                                        and create_user = 'BATCH'
                                        and ( (chk_wh_grade='3' and chk_wh_kind='0') or
                                            chk_wh_no in('ER1','ERC') )
                                        and chk_type = '3') = 1) 
                                then '是' else '否'
                          end) as 是否開單
                     from
                          (select WH_NO,WH_NAME 
                             from MI_WHMAST
                            where ( (wh_grade='3' and wh_kind='0') or
                                     wh_no in('ER1','ERC') ) 
                          ) A
                          left join
                            (select b.chk_wh_no,count(*) as mmcode_cnt
                               from CHK_DETAILTOT a,
                                    (select chk_no, chk_wh_no 
                                       from CHK_MAST 
                                      where chk_ym = (twn_yyymm(sysdate)||'01') 
                                        and ( (chk_wh_grade='3' and chk_wh_kind='0') or
                                            chk_wh_no in('ER1','ERC') )
                                        and create_user = 'BATCH'
                                        and chk_type = '3'
                                    ) b
                              where a.chk_no=b.chk_no
                              group by b.chk_wh_no, b.chk_no
                            ) B
                            on A.WH_NO=B.chk_wh_no
                    order by A.WH_NO";

            }
            else {
                sql = @"
                    select b.責任中心, b.責任中心名稱, b.清點項數,
                           (select count(*) from DGMISS_CHK c
                                where data_ym=b.data_ym and inid = b.責任中心 and chk_qty is not null
                                  and exists (select 1 from MI_MAST where mmcode = c.mmcode and e_restrictcode in ('1','2','3','4') ) ) 已清點數
                           from(
                           select a.inid 責任中心, inid_name(a.inid) 責任中心名稱, a.data_ym,
                           count(*) 清點項數
                      from DGMISS_CHK a
                     where a.data_ym=twn_pym(cur_setym)
                       and exists (select 1 from MI_MAST where mmcode = a.mmcode and e_restrictcode in ('1','2','3','4') )
                     group by a.data_ym, a.inid
                           ) b
                     order by b.責任中心
                ";
            }

            p.Add(":drugtype", string.Format("{0}", drugType));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<AA0138ReportWH> GetWhData(string drugType, bool isHospCode0) {
            var p = new DynamicParameters();

            string sql = string.Empty;

            if (isHospCode0)
            {
                sql = @"
                   select A.WH_NO,A.WH_NAME,B.mmcode_cnt, 
                          (case
                              when ((select 1 from CHK_MAST
                                      where chk_wh_no = a.wh_no
                                        and chk_ym = (twn_yyymm(sysdate)||'01') 
                                        and create_user = 'BATCH'
                                        and ( (chk_wh_grade='3' and chk_wh_kind='0') or
                                            chk_wh_no in('ER1','ERC') )
                                        and chk_type = :drugtype) = 1) 
                                 then '是' else '否'
                          end) as is_chk_batch
                     from
                          (select WH_NO,WH_NAME 
                             from MI_WHMAST
                            where ( (wh_grade='3' and wh_kind='0') or
                                            wh_no in('ER1','ERC') )
                          ) A
                          left join
                            (select b.chk_wh_no,count(*) as mmcode_cnt
                               from CHK_DETAILTOT a,
                                    (select chk_no, chk_wh_no 
                                       from CHK_MAST 
                                      where chk_ym = (twn_yyymm(sysdate)||'01') 
                                        and ( (chk_wh_grade='3' and chk_wh_kind='0') or
                                            chk_wh_no in('ER1','ERC') )
                                        and create_user = 'BATCH'
                                        and chk_type = :drugtype
                                    ) b
                              where a.chk_no=b.chk_no
                              group by b.chk_wh_no, b.chk_no
                            ) B
                            on A.WH_NO=B.chk_wh_no
                    order by A.WH_NO";
            }
            else {
                sql = @"
                    select b.責任中心 as WH_NO, b.責任中心名稱 as wh_name, b.清點項數 as mmcode_cnt,
                           '是' as is_chk_batch
                           from(
                           select a.inid 責任中心, inid_name(a.inid) 責任中心名稱, a.data_ym,
                           count(*) 清點項數
                      from DGMISS_CHK a
                     where a.data_ym=twn_pym(cur_setym)
                       and exists (select 1 from MI_MAST where mmcode = a.mmcode and e_restrictcode in ('1','2','3','4') )
                     group by a.data_ym, a.inid
                           ) b
                     order by b.責任中心
                ";
            }            
            p.Add(":drugtype", string.Format("{0}", drugType));


            return DBWork.Connection.Query<AA0138ReportWH>(sql, p, DBWork.Transaction);
        }

        public string GetHospCode() {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }

}