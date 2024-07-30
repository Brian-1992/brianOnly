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
    public class AA0133ReportMODEL : JCLib.Mvc.BaseModel
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
    public class AA0133Repository : JCLib.Mvc.BaseRepository
    {
        public AA0133Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0133> GetAll(string wh_no, string drugType, bool isHospCode0)
        {
            var p = new DynamicParameters();

            string wh_no_string = string.Empty;
            if (!string.IsNullOrEmpty(wh_no))
            {
                wh_no_string = @"   and chk_wh_no = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }

            var sql = string.Empty;
            if (isHospCode0)
            {
                sql = string.Format(@" 
                         with chk_nos as (
                            select chk_no, chk_wh_no from CHK_MAST 
                             where substr(chk_ym, 1,5) = cur_setym
                               and ( (chk_wh_grade='3' and chk_wh_kind='0') or
                                      chk_wh_no in('ER1','ERC') )
                               and create_user = 'BATCH'
                               and chk_type = :drugtype
                               {0}
                         ),
                         mnset_date as (
                            select set_ctime 
                              from MI_MNSET
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
                                   decode(c.his_consume_datatime, null, 
                                          twn_date((case
                                                      when status_tot = '1' then a.store_qty_time1
                                                      when status_tot = '2' then a.store_qty_time2
                                                      when status_tot = '3' then a.store_qty_time3
                                                    end)
                                                 )||'055959',
                                                 c.his_consume_datatime
                                         ) as his_consume_datatime_e,
                                   decode(c.his_consume_datatime, null,   
                                          twn_date((case
                                                      when status_tot = '1' then a.store_qty_time1
                                                      when status_tot = '2' then a.store_qty_time2
                                                      when status_tot = '3' then a.store_qty_time3
                                                    end)
                                                 )||'000000',
                                                 substr(c.his_consume_datatime, 1,7) ||'000000'
                                         ) as his_consume_datatime_s,
                                   c.his_consume_qty_t
                              from CHK_DETAILTOT a, chk_nos b, chk_detail c
                             where a.chk_no = b.chk_no
                               and c.chk_no = a.chk_no
                               and c.mmcode = a.mmcode
                         ),
                         mi_consume_date_datas as (
                            select a.wh_no, 
                                   a.parent_ordercode as mmcode, 
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
                               and (a.workdate||a.worktime) between b.his_consume_datatime_s and b.his_consume_datatime_e
                             group by a.stockcode, a.parent_ordercode
                         )
                         select a.wh_no, a.mmcode, 
                                mmcode_namec(a.mmcode) as mmname_c,
                                mmcode_name(a.mmcode) as mmname_e,
                                base_unit(a.mmcode) as base_unit,
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
                                b.inv_qty as inv_qty,
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
                          order by a.mmcode", wh_no_string);
            }
            else {
                string drugType_string = string.Empty;
                if (drugType == "3") {
                    drugType_string = @" and exists (select 1 from MI_MAST where mmcode=a.mmcode and e_restrictcode in ('1','2','3'))";
                }
                if (drugType == "4")
                {
                    drugType_string = @" and exists (select 1 from MI_MAST where mmcode=a.mmcode and e_restrictcode in ('4'))";
                }

                sql = string.Format(@" 
                        select a.inid, a.mmcode,
                                mmcode_namec(a.mmcode) as mmname_c,
                                mmcode_name(a.mmcode) as mmname_e,      
                                base_unit(a.mmcode) as base_unit,
                                a.inv_qty, a.chk_qty
                           from DGMISS_CHK a
                          where a.data_ym=twn_pym(cur_setym)
                            {0}
                            {1}
                          order by a.mmcode
                     ", !string.IsNullOrEmpty(wh_no) ? "and a.inid=:wh_no" : string.Empty
                      , !string.IsNullOrEmpty(drugType) ? drugType_string : string.Empty
                     );
                p.Add(":wh_no", string.Format("{0}", wh_no));
            }

            

            p.Add(":drugtype", string.Format("{0}", drugType));

            return DBWork.PagingQuery<AA0133>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<MI_WHMAST> GetWH(string userid, string userinid, bool isAll, bool isHospCode0)
        {
            string sql = string.Empty;
            if (isHospCode0)
            {
                if (isAll)
                {
                    sql = @"select wh_no, wh_name(wh_no) as wh_name
                          from MI_WHMAST
                         where wh_grade = '3' and wh_kind = '0'
                           and cancel_id = 'N'
                         order by wh_no";
                }
                else
                {
                    sql = @"select wh_no,
                                  wh_name(wh_no) as wh_name
                             from (
                                    select b.wh_no 
                                      from MI_WHID b
                                     where b.task_id = '1'
                                       and b.wh_userid = :userid
                                       and (exists (select 1 from MI_WHMAST 
                                                    where wh_no = b.wh_no
                                                      and wh_grade = '3' and wh_kind = '0'
                                                   )
                                            or wh_no in ('ER1', 'ERC')
                                            )
                                    union
                                    select wh_no
                                      from MI_WHMAST 
                                     where ((wh_grade = '3' and wh_kind = '0') 
                                            or wh_no in ('ER1', 'ERC'))
                                       and inid = :userinid
                                  ) a
                            order by wh_no
                          ";
                }
            }
            else {
                sql = @"select inid as wh_no, inid_name(inid) as wh_name
                          from UR_INID a
                         where exists (select 1 from DGMISS where app_inid=a.inid)
                         order by inid";
            }

            

            
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { userid = userid, userinid = userinid }, DBWork.Transaction);
        }

        public DataTable GetExcel(string wh_no, string drugType, bool showStoreQty, bool isHospCode0)
        {
            var p = new DynamicParameters();

            string showStoreQtyString = @",a.store_qty as 盤點電腦量,
                                           a.store_qty_time as 盤點時間";

            string wh_no_string = string.Empty;
            if (!string.IsNullOrEmpty(wh_no))
            {
                wh_no_string = @"   and chk_wh_no = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }

            var sql = string.Empty;

            if (isHospCode0)
            {
                sql = string.Format(@" 
                         with chk_nos as (
                            select chk_no, chk_wh_no from CHK_MAST 
                             where substr(chk_ym, 1,5) = cur_setym 
                               and ( (chk_wh_grade='3' and chk_wh_kind='0') or
                                      chk_wh_no in('ER1','ERC') )
                               and create_user = 'BATCH'
                               and chk_type = :drugtype
                               {0}
                         ),
                         mnset_date as (
                            select set_ctime 
                              from MI_MNSET
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
                                   decode(c.his_consume_datatime, null, 
                                          twn_date((case
                                                      when status_tot = '1' then a.store_qty_time1
                                                      when status_tot = '2' then a.store_qty_time2
                                                      when status_tot = '3' then a.store_qty_time3
                                                    end)
                                                 )||'055959',
                                                 c.his_consume_datatime
                                         ) as his_consume_datatime_e,
                                   decode(c.his_consume_datatime, null,   
                                          twn_date((case
                                                      when status_tot = '1' then a.store_qty_time1
                                                      when status_tot = '2' then a.store_qty_time2
                                                      when status_tot = '3' then a.store_qty_time3
                                                    end)
                                                 )||'000000',
                                                 substr(c.his_consume_datatime, 1,7) ||'000000'
                                         ) as his_consume_datatime_s,
                                   c.his_consume_qty_t
                              from CHK_DETAILTOT a, chk_nos b, chk_detail c
                             where a.chk_no = b.chk_no
                               and c.chk_no = a.chk_no
                               and c.mmcode = a.mmcode
                         ),
                         mi_consume_date_datas as (
                            select a.wh_no, 
                                   a.parent_ordercode as mmcode, 
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
                               and (a.workdate||a.worktime) between b.his_consume_datatime_s and b.his_consume_datatime_e
                             group by a.stockcode, a.parent_ordercode
                         )
                         select a.mmcode  as 院內碼, 
                                mmcode_namec(a.mmcode) as 中文品名,
                                mmcode_name(a.mmcode) as 英文品名,
                                base_unit(a.mmcode) as 單位,
                                (select inv_qty from MI_WINVMON
                                  where data_ym = twn_yyymm(add_months(sysdate, -2))
                                    and wh_no = a.wh_no
                                    and mmcode = a.mmcode) as 上期結存,
                                b.apl_inqty as 本月進貨,
                                (
                                    b.APL_OUTQTY + b.TRN_OUTQTY + b.ADJ_OUTQTY + b.BAK_OUTQTY + b.REJ_OUTQTY
                                  + b.DIS_OUTQTY + b.EXG_OUTQTY + b.MIL_OUTQTY + b.INVENTORYQTY + b.USE_QTY
                                  - b.TRN_INQTY - b.ADJ_INQTY - b.BAK_INQTY - b.EXG_INQTY - b.MIL_INQTY
                                ) as 本月消耗,
                                b.inv_qty as 本月結存,
                                a.chk_qty as 實際清點,
                                (nvl(c.consume_date_sum, 0) + nvl(d.consume_d_sum, 0) + nvl(a.his_consume_qty_t, 0)) as 期初至清點時間醫令扣庫量
                                {1}
                           from chk_datas a, MI_WINVMON b, mi_consume_date_datas c, his_consume_d_datas d
                          where b.wh_no = a.wh_no
                            and b.mmcode = a.mmcode
                            and b.data_ym = twn_pym(cur_setym)
                            and a.wh_no = c.wh_no(+)
                            and a.mmcode = c.mmcode(+)
                            and a.wh_no = d.wh_no(+)
                            and a.mmcode = d.mmcode(+)
                          order by a.mmcode",
                            wh_no_string,
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

                sql=string.Format(@" 
                        select a.mmcode 院內碼,
                                mmcode_namec(a.mmcode) as 中文品名,
                                mmcode_name(a.mmcode) as 英文品名,      
                                base_unit(a.mmcode) as 單位,
                                a.inv_qty 本月結存, a.chk_qty 實際清點
                           from DGMISS_CHK a
                          where a.data_ym=twn_pym(cur_setym)
                            {0}
                            {1}
                          order by a.mmcode
                     ", !string.IsNullOrEmpty(wh_no) ? "and a.inid=:wh_no" : string.Empty
                      , !string.IsNullOrEmpty(drugType) ? drugType_string : string.Empty
                      );
            }
            
            p.Add(":drugtype", string.Format("{0}", drugType));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<AA0133ReportMODEL> GetPrintData(string wh_no, string drugType, string userid, string userinid, bool isAll,bool isHospCode0)
        {
            var p = new DynamicParameters();

            string wh_no_string = string.Empty;
            if (!string.IsNullOrEmpty(wh_no))
            {
                wh_no_string = @"   and chk_wh_no = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }
            else {
                if (isHospCode0)
                {
                    if (isAll)
                    {
                        wh_no_string = @"   and chk_wh_no in (
                                              select wh_no 
                                                from MI_WHMAST
                                               where ( (wh_grade='3' and wh_kind='0') or
                                                        wh_no in('ER1','ERC') )
                                                 and cancel_id = 'N'
                                            ) ";
                    }
                    else
                    {
                        wh_no_string = @"   and chk_wh_no in (
                                              select b.wh_no 
                                                from MI_WHID b
                                               where b.task_id = '1'
                                                 and b.wh_userid = :userid
                                                 and exists (select 1 from MI_WHMAST 
                                                              where wh_no = b.wh_no
                                                                and ( (wh_grade='3' and wh_kind='0') or
                                                                       wh_no in('ER1','ERC') )
                                                            )
                                              union
                                              select wh_no
                                                from MI_WHMAST 
                                               where ( (wh_grade='3' and wh_kind='0') or
                                                        wh_no in('ER1','ERC') )
                                                 and inid = :userinid
                                            ) ";
                    }
                }

                               
                p.Add(":userid", string.Format("{0}", userid));
                p.Add(":userinid", string.Format("{0}", userinid));
            }
            var sql = string.Empty;

            if (isHospCode0)
            {
                sql = string.Format(@" 
                         with chk_nos as (
                            select chk_no, chk_wh_no from CHK_MAST 
                             where substr(chk_ym, 1,5) = cur_setym 
                               and ( (chk_wh_grade='3' and chk_wh_kind='0') or
                                      chk_wh_no in('ER1','ERC') )
                               and create_user = 'BATCH'
                               and chk_type = :drugtype
                               {0}
                         ),
                         mnset_date as (
                            select set_ctime 
                              from MI_MNSET
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
                                   decode(c.his_consume_datatime, null, 
                                          twn_date((case
                                                      when status_tot = '1' then a.store_qty_time1
                                                      when status_tot = '2' then a.store_qty_time2
                                                      when status_tot = '3' then a.store_qty_time3
                                                    end)
                                                 )||'055959',
                                                 c.his_consume_datatime
                                         ) as his_consume_datatime_e,
                                   decode(c.his_consume_datatime, null,   
                                          twn_date((case
                                                      when status_tot = '1' then a.store_qty_time1
                                                      when status_tot = '2' then a.store_qty_time2
                                                      when status_tot = '3' then a.store_qty_time3
                                                    end)
                                                 )||'000000',
                                                 substr(c.his_consume_datatime, 1,7) ||'000000'
                                         ) as his_consume_datatime_s,
                                   c.his_consume_qty_t
                              from CHK_DETAILTOT a, chk_nos b, chk_detail c
                             where a.chk_no = b.chk_no
                               and c.chk_no = a.chk_no
                               and c.mmcode = a.mmcode
                         ),
                         mi_consume_date_datas as (
                            select a.wh_no, 
                                   a.parent_ordercode as mmcode, 
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
                               and (a.workdate||a.worktime) between b.his_consume_datatime_s and b.his_consume_datatime_e
                             group by a.stockcode, a.parent_ordercode
                         )
                         select wh_name(a.wh_no) as F1, 
                                a.mmcode as F2, 
                                mmcode_namec(a.mmcode) as F3,
                                mmcode_name(a.mmcode) as F4,
                                base_unit(a.mmcode) as F5,
                                (select inv_qty from MI_WINVMON
                                  where data_ym = twn_yyymm(add_months(sysdate, -2))
                                    and wh_no = a.wh_no
                                    and mmcode = a.mmcode) as F6,
                                b.apl_inqty as F7,
                                (
                                    b.APL_OUTQTY + b.TRN_OUTQTY + b.ADJ_OUTQTY + b.BAK_OUTQTY + b.REJ_OUTQTY
                                  + b.DIS_OUTQTY + b.EXG_OUTQTY + b.MIL_OUTQTY + b.INVENTORYQTY + b.USE_QTY
                                  - b.TRN_INQTY - b.ADJ_INQTY - b.BAK_INQTY - b.EXG_INQTY - b.MIL_INQTY
                                ) as F8,
                                b.inv_qty as F9,
                                a.chk_qty as F10,
                                a.store_qty as store_qty,
                                a.store_qty_time,
                                c.consume_date_sum,
                                d.consume_d_sum,
                                (nvl(c.consume_date_sum, 0) + nvl(d.consume_d_sum, 0) + nvl(a.his_consume_qty_t, 0)) as F11
                           from chk_datas a, MI_WINVMON b, mi_consume_date_datas c, his_consume_d_datas d
                          where b.wh_no = a.wh_no
                            and b.mmcode = a.mmcode
                            and b.data_ym = twn_pym(cur_setym)
                            and a.wh_no = c.wh_no(+)
                            and a.mmcode = c.mmcode(+)
                            and a.wh_no = d.wh_no(+)
                            and a.mmcode = d.mmcode(+)
                          order by a.mmcode", wh_no_string);
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
                        select a.inid F1, a.mmcode F2,
                                mmcode_namec(a.mmcode) as F3,
                                mmcode_name(a.mmcode) as F4,      
                                base_unit(a.mmcode) as F5,
                                a.inv_qty F9, a.chk_qty F10
                           from DGMISS_CHK a
                          where a.data_ym=twn_pym(cur_setym)
                            {0}
                            {1}
                          order by a.mmcode
                     ", !string.IsNullOrEmpty(wh_no) ? "and a.inid=:wh_no" : string.Empty
                      , drugType_string
                     );
            }

            

            p.Add(":drugtype", string.Format("{0}", drugType));

            return DBWork.Connection.Query<AA0133ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public string GetUserInid(string tuser) {
            string sql = @"select inid from UR_ID where tuser = :tuser ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { tuser = tuser }, DBWork.Transaction);
            
        }

        public string GetHospCode() {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }

}