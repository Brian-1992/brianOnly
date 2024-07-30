using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.G
{
    public class GA0002Repository : JCLib.Mvc.BaseRepository
    {
        public GA0002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        #region master
        public IEnumerable<TC_PURCH_M> GetMasters(string pur_no, string tc_type, string start_date, string end_date) {

            var p = new DynamicParameters();

            string sql = @"select a.pur_no, TWN_DATE(a.pur_date) as pur_date, TWN_DATE(a.pur_date) as pur_date_display, a.tc_type,
                                  (select una from UR_ID where tuser = a.pur_unm) as pur_unm_name, 
                                  (case
                                     WHEN tc_type = 'A' THEN 'A 科學中藥'
                                     WHEN tc_type = 'B' THEN 'B 飲片'
                                     ELSE ''
                                  end)    as tc_type_name,
                                  a.pur_note, a.pur_unm
                             from TC_PURCH_M a
                            where a.purch_st = 'A'";
            if (tc_type != string.Empty) {
                sql += @"     and a.tc_type = :tc_type";
                p.Add(":tc_type", tc_type);
            }
            if (pur_no != string.Empty) {
                sql += "     and a.pur_no like :pur_no";
                p.Add(":pur_no", string.Format("%{0}%", pur_no));
            }
            if (start_date != string.Empty) {
                sql += string.Format(@"     and a.pur_date >= TWN_TODATE(:start_date)");
                p.Add(":start_date", start_date);
            }
            if (end_date != string.Empty)
            {
                sql += string.Format(@"     and a.pur_date <= TWN_TODATE(:end_date)");
                p.Add(":end_date", end_date);
            }
            return DBWork.PagingQuery<TC_PURCH_M>(sql, p, DBWork.Transaction);
        }

        public int MasterInsert(TC_PURCH_M master) {
            string sql = @"insert into TC_PURCH_M
                           values (:pur_no, TWN_TODATE(:pur_date), :tc_type, :create_user, 'A',:pur_note, 
                                   sysdate, :create_user, sysdate, :update_user, :update_ip)";
            return DBWork.Connection.Execute(sql, master, DBWork.Transaction);

        }
        public string GetTwnSystime() {
            string sql = "select twn_systime from dual";
            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        public int MasterUpdate(TC_PURCH_M master) {
            string sql = @"update TC_PURCH_M
                              set pur_date = TWN_TODATE(:pur_date), pur_note = :pur_note,
                                  update_time = sysdate, update_user = :update_user, update_ip = :update_ip
                            where pur_no = :pur_no";
            return DBWork.Connection.Execute(sql, master, DBWork.Transaction);
        }

        public int MasterDelete(string pur_no) {
            string sql = @"delete from TC_PURCH_M where pur_no = :pur_no";
            return DBWork.Connection.Execute(sql, new {pur_no = pur_no }, DBWork.Transaction);
        }

        public int PlaceOrder(string pur_no)
        {
            string sql = @"update TC_PURCH_M
                              set purch_st = 'B'
                            where pur_no = :pur_no";
            return DBWork.Connection.Execute(sql, new { pur_no = pur_no }, DBWork.Transaction);
        }

        #endregion

        #region detail

        public IEnumerable<TC_PURCH_DL> GetDetails(string pur_no) {
            string sql = @"select a.pur_no, a.mmcode, a.agen_namec, a.pur_qty, a.pur_unit,
                                  a.in_purprice, a.pur_amount, a.mmname_c,
                                  (select base_unit from TC_INVQMTR where mmcode = a.mmcode) as base_unit,
                                  (select inv_day from TC_INVQMTR where mmcode = a.mmcode) as inv_day
                             from TC_PURCH_DL a
                            where pur_no = :pur_no";

            return DBWork.PagingQuery<TC_PURCH_DL>(sql, new { pur_no = pur_no}, DBWork.Transaction);
        }
        public int DetailDelete(TC_PURCH_DL detail) {
            string sql = @"delete from TC_PURCH_DL where pur_no = :pur_no and mmcode = :mmcode and agen_namec = :agen_namec";
            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }
        public int DetailAllDelete(string pur_no) {
            string sql = @"delete from TC_PURCH_DL where pur_no = :pur_no ";
            return DBWork.Connection.Execute(sql, new { pur_no = pur_no }, DBWork.Transaction);
        }


        #endregion

        #region detail insert
        public IEnumerable<TC_INVQMTR> GetInvqmtrs(string pur_no, string tc_type, string inv_day, string mmname)
        {
            var p = new DynamicParameters();
            string sql = @"select * from (
                                    (select a.data_ym, a.mmcode, a.mmname_c, a.base_unit,
                                            a.in_price, a.pmn_invqty, a.mn_useqty, a.mn_inqty,a.mn_invqty,
                                            a.store_loc, a.m6avg_useqty, a.m3avg_useqty, 
                                            a.m6max_useqty, a.m3max_useqty, a.inv_day, a.exp_purqty,
                                            a.agen_namec, a.pur_unit, a.in_purprice,
                                            (case when a.base_unit <> a.pur_unit then a.baseun_multi else 1 end ) as baseun_multi,
                                            (case when a.base_unit <> a.pur_unit then a.purun_multi else 1 end ) as purun_multi, 
                                            a.rcm_purqty, 
                                            TWN_TIME_FORMAT(a.create_time) as create_time,
                                           (case 
                                            when a.base_unit <> a.pur_unit then 
                                            (case
                                            when (select count(*) from TC_PURUNCOV 
                                                         where pur_unit = a.pur_unit 
                                                           and base_unit = a.base_unit) = 0
                                                       then 'N'
                                                       else 'Y'
                                             end)
                                                  else 'Y'
                                              end) as is_valid,
                                              1 as pur_seq --, a.rcm_purday
                                       from TC_INVQMTR a)
                                     UNION
                                     (select a.data_ym, a.mmcode, a.mmname_c, a.base_unit,
                                             a.in_price, a.pmn_invqty, a.mn_useqty, a.mn_inqty,a.mn_invqty,
                                             a.store_loc, a.m6avg_useqty, a.m3avg_useqty, 
                                             a.m6max_useqty, a.m3max_useqty, a.inv_day, a.exp_purqty,
                                             b.agen_namec, b.pur_unit, b.in_purprice,
                                             (case when a.base_unit <> b.pur_unit then c.baseun_multi else 1 end ) as baseun_multi,
                                             (case when a.base_unit <> b.pur_unit then c.purun_multi else 1 end ) as purun_multi, 
                                              null as rcm_purqty, 
                                             TWN_TIME_FORMAT(a.create_time) as create_time,
                                             (case 
                                            when a.base_unit <> b.pur_unit then 
                                            (case
                                            when (select count(*) from TC_PURUNCOV 
                                                         where pur_unit = b.pur_unit 
                                                           and base_unit = a.base_unit) = 0
                                                       then 'N'
                                                       else 'Y'
                                             end)
                                                  else 'Y'
                                              end) as is_valid,
                                               b.pur_seq --,a.rcm_purday
                                        from TC_INVQMTR a
                                               left join TC_MMAGEN b on a.mmcode = b.mmcode 
                                               left join TC_PURUNCOV c on a.base_unit = c.base_unit and c.pur_unit = b.pur_unit
                                       where 1=1
                                       and b.pur_seq > 1 )
                            ) A
                            where 1=1";

            if (inv_day != string.Empty)
            {
                sql += "      and A.inv_day <= :inv_day";
                p.Add(":inv_day", inv_day);
            }
            if (mmname != string.Empty)
            {
                sql += "      and A.mmname_c like :mmname_c";
                p.Add(":mmname_c", string.Format("%{0}%", mmname));
            }
            if (tc_type == "A")
            {
                sql += "      and substr(A.mmcode,1,1) in('C','B')";
            }
            else if (tc_type == "B")
            {
                sql += "      and substr(A.mmcode,1,1) in('D','E')";
            }

            sql += @"         and (select count(*) from TC_PURCH_DL 
                                    where pur_no = :pur_no 
                                      and mmcode = A.mmcode and agen_namec = A.agen_namec) = 0";

            p.Add(":pur_no", pur_no);

            return DBWork.PagingQuery<TC_INVQMTR>(sql, p, DBWork.Transaction);
        }
        public TC_INVQMTR GetInvqmtr(string mmcode) {
            string sql = @"select * from TC_INVQMTR where mmcode = :mmcode";
            return DBWork.Connection.QueryFirstOrDefault<TC_INVQMTR>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }

        public int DetailInsert(TC_PURCH_DL detail)
        {
            string sql = @"insert into TC_PURCH_DL
                           values (:pur_no, :mmcode, :agen_namec, :mmname_c,
                                   :pur_qty, :pur_unit, :in_purprice, :pur_amount,
                                   sysdate, :create_user, sysdate, :update_user, :update_ip)";

            return DBWork.Connection.Execute(sql, detail, DBWork.Transaction);
        }

        public IEnumerable<TC_PURUNCOV> GetMulti(string pur_unit, string base_unit)
        {
            string sql = @"select purun_multi, baseun_multi from TC_PURUNCOV
                            where pur_unit = :pur_unit and base_unit = :base_unit";
            return DBWork.Connection.Query<TC_PURUNCOV>(sql, new { pur_unit = pur_unit, base_unit = base_unit }, DBWork.Transaction);
        }

        
        public IEnumerable<AGEN_INFO> GetAgens(string mmcode, string base_unit) {
            string sql = @"select a.mmcode, a.agen_namec, a.pur_unit, a.in_purprice, '' as pur_qty, a.pur_seq,
                                  (case 
                                        when (select count(*) from TC_PURUNCOV 
                                               where pur_unit = a.pur_unit 
                                                 and base_unit = :base_unit) > 0
                                            then 'Y'
                                        else 'N'
                                    end) as is_valid,
                                  NVL((select purun_multi from TC_PURUNCOV
                                    where pur_unit = a.pur_unit 
                                      and base_unit = :base_unit), '') as purun_multi,
                                  NVL((select baseun_multi from TC_PURUNCOV
                                    where pur_unit = a.pur_unit 
                                      and base_unit = :base_unit), '') as baseun_multi
                             from TC_MMAGEN a
                            where a.mmcode = :mmcode";
            return DBWork.PagingQuery<AGEN_INFO>(sql, new { mmcode = mmcode, base_unit = base_unit }, DBWork.Transaction);
        }
        #endregion

        #region cancel order
        public IEnumerable<TC_PURCH_M> GetOrders(string pur_no, string tc_type, string start_date, string end_date)
        {

            var p = new DynamicParameters();

            string sql = @"select a.pur_no, TWN_DATE(a.pur_date) as pur_date, a.tc_type,
                                  (select una from UR_ID where tuser = a.pur_unm) as pur_unm_name, 
                                  (case
                                     WHEN tc_type = 'A' THEN 'A 科學中藥'
                                     WHEN tc_type = 'B' THEN 'B 飲片'
                                     ELSE ''
                                  end)    as tc_type_name,
                                  a.pur_note, a.pur_unm
                             from TC_PURCH_M a
                            where a.purch_st = 'B'";
            if (tc_type != string.Empty) {
                sql += @"     and a.tc_type = :tc_type";
                p.Add(":tc_type", tc_type);
            }
            if (pur_no != string.Empty)
            {
                sql += "     and a.pur_no like :pur_no";
                p.Add(":pur_no", string.Format("%{0}%", pur_no));
            }
            if (start_date != string.Empty)
            {
                sql += string.Format(@"     and a.pur_date >= TWN_TODATE(:start_date)");
                p.Add(":start_date", start_date);
            }
            if (end_date != string.Empty)
            {
                sql += string.Format(@"     and a.pur_date <= TWN_TODATE(:end_date)");
                p.Add(":end_date", end_date);
            }
            return DBWork.PagingQuery<TC_PURCH_M>(sql, p, DBWork.Transaction);
        }

        public int CancelOrder(string pur_no) {
            string sql = @"update TC_PURCH_M
                              set purch_st = 'A'
                            where pur_no = :pur_no";
            return DBWork.Connection.Execute(sql, new { pur_no = pur_no }, DBWork.Transaction);
        }
        #endregion
    }
}