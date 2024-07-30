using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.G
{
    public class GA0001Repository : JCLib.Mvc.BaseRepository
    {
        public GA0001Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<TC_INVQMTR> GetAll(string inv_day,  string mmname, int page_index, int page_size, string sorters) {
            var p = new DynamicParameters();
            string sql = @"select data_ym, mmcode, mmname_c, base_unit,
                                  in_price, pmn_invqty, mn_useqty, mn_invqty,
                                  store_loc, m6avg_useqty, m3avg_useqty, 
                                  m6max_useqty, m3max_useqty, inv_day, exp_purqty,
                                  agen_namec, pur_unit, in_purprice,
                                  baseun_multi, purun_multi, rcm_purqty, rcm_purday, TWN_TIME_FORMAT(create_time) as create_time
                             from TC_INVQMTR
                            where 1=1";

            if (inv_day != string.Empty) {
                sql += "      and inv_day <= :inv_day";
                p.Add(":inv_day", inv_day);
            }
            if (mmname != string.Empty)
            {
                sql += "      and mmname_c like :mmname";
                p.Add(":mmname", string.Format("%{0}%", mmname));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<TC_INVQMTR>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        #region 計算平均月消耗量
        public IEnumerable<TC_USEQMTR> GetUseqmtrs(List<string> yms) {
            string ymString = GetYMString(yms);

            string sql = string.Format(@"select * from TC_USEQMTR
                                          where data_ym in ({0})", ymString);


            return DBWork.Connection.Query<TC_USEQMTR>(sql, DBWork.Transaction);
        }
        private string GetYMString(List<string> yms) {
            string item = string.Empty;
            int i = 0;
            foreach (string ym in yms) {
                if (i == 0)
                {
                    item = string.Format("'{0}'", ym);
                }
                else {
                    item = string.Format("{0},'{1}'", item, ym);
                }
                i++;
            }
            return item;
        }

        public int DeleteInvctl() {
            string sql = "delete from TC_INVCTL";

            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int InsertInvctl(TC_INVCTL invctl) {
            string sql = @" insert into TC_INVCTL (mmcode, mmname_c, m6avg_useqty, m3avg_useqty,
                                                   m6max_useqty, m3max_useqty, base_unit, 
                                                   create_time, create_user, update_time, update_user, update_ip)
                            values (:MMCODE, :MMNAME_C, :M6AVG_USEQTY, :M3AVG_USEQTY,
                                    :M6MAX_USEQTY, :M3MAX_USEQTY, :BASE_UNIT,
                                    SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";

            return DBWork.Connection.Execute(sql, new {
                MMCODE = invctl.MMCODE,
                MMNAME_C = invctl.MMNAME_C,
                M6AVG_USEQTY = invctl.M6AVG_USEQTY,
                M3AVG_USEQTY = invctl.M3AVG_USEQTY,
                M6MAX_USEQTY = invctl.M6MAX_USEQTY,
                M3MAX_USEQTY = invctl.M3MAX_USEQTY,
                BASE_UNIT = invctl.BASE_UNIT,
                CREATE_USER = invctl.CREATE_USER,
                UPDATE_USER = invctl.UPDATE_USER,
                UPDATE_IP = invctl.UPDATE_IP
            }, DBWork.Transaction);
        }

        #endregion

        #region 匯入

        public IEnumerable<TC_INVQMTR> GetInvqmtrs() {
            string sql = @"select * from TC_INVQMTR order by mmcode";
            return DBWork.Connection.Query<TC_INVQMTR>(sql,  DBWork.Transaction);
        }

        public TC_INVCTL GetInvctl(string data_ym, string mmcode) {
            string sql = @"select * from TC_INVCTL
                            where mmcode = :MMCODE ";
            //return DBWork.Connection.QueryFirst<TC_INVCTL>(sql,new { DATA_YM = data_ym, MMCODE = mmcode} ,DBWork.Transaction);

            IEnumerable<TC_INVCTL> result = DBWork.Connection.Query<TC_INVCTL>(sql, new { DATA_YM = data_ym, MMCODE = mmcode }, DBWork.Transaction);
            if (result.Any()) {
                return result.Take(1).FirstOrDefault();
            }
            return null;
        }

        public TC_MMAGEN GetMmagen(string mmcode) {
            string sql = @"select * from TC_MMAGEN
                            where mmcode = :MMCODE
                              and pur_seq = '1'";
            return DBWork.Connection.QueryFirstOrDefault<TC_MMAGEN>(sql, new { MMCODE = mmcode }, DBWork.Transaction);

            //IEnumerable<TC_MMAGEN> result = DBWork.Connection.Query<TC_MMAGEN>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
            //if (result.Any())
            //{
            //    return result.Take(1).FirstOrDefault();
            //}
            //return null;
        }

        public TC_PURUNCOV GetPuruncov(string base_unit, string pur_unit) {
            string sql = @"select * from TC_PURUNCOV
                            where base_unit = :BASE_UNIT
                              and pur_unit = :PUR_UNIT";
            IEnumerable<TC_PURUNCOV> result = DBWork.Connection.Query<TC_PURUNCOV>(sql, new { BASE_UNIT = base_unit, PUR_UNIT = pur_unit }, DBWork.Transaction);
            if (result.Any())
            {
                return result.Take(1).FirstOrDefault();
            }
            return null;
        }

        public int InsertInvqmtr(TC_INVQMTR invqmtr) {
            string sql = @" insert into TC_INVQMTR (data_ym, mmcode, mmname_c,base_unit, in_price, 
                                                    pmn_invqty, mn_inqty, mn_useqty, mn_invqty, store_loc,
                                                    m6avg_useqty, m3avg_useqty, m6max_useqty, m3max_useqty,  
                                                    inv_day, exp_purqty, agen_namec, pur_unit, in_purprice,
                                                    baseun_multi, purun_multi, rcm_purqty,
                                                    create_time, create_user, update_time, update_user, update_ip, rcm_purday)
                            values (:DATA_YM, :MMCODE, :MMNAME_C, :BASE_UNIT, :IN_PRICE,
                                    :PMN_INVQTY, :MN_INQTY, :MN_USEQTY, :MN_INVQTY, :STORE_LOC,
                                    :M6AVG_USEQTY, :M3AVG_USEQTY, :M6MAX_USEQTY, :M3MAX_USEQTY, 
                                    :INV_DAY, :EXP_PURQTY, :AGEN_NAMEC, :PUR_UNIT, :IN_PURPRICE,
                                    :BASEUN_MULTI, :PURUN_MULTI, :RCM_PURQTY,
                                    SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :RCM_PURDAY)";

            return DBWork.Connection.Execute(sql, invqmtr, DBWork.Transaction);
        }
        public int DeleteInvqmtr() {
            string sql = @"delete from TC_INVQMTR";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public TC_MAST GetTcMast(string mmcode) {
            string sql = @"select * from TC_MAST
                            where mmcode = :MMCODE";
            IEnumerable<TC_MAST> result = DBWork.Connection.Query<TC_MAST>(sql, new {MMCODE= mmcode }, DBWork.Transaction);
            if (result.Any())
            {
                return result.Take(1).FirstOrDefault();
            }
            return null;
        }
        public int InsertMast(TC_MAST mast)
        {
            string sql = @" insert into TC_MAST (mmcode, mmname_c,
                                                 create_time, create_user, update_time, update_user, update_ip)
                            values (:MMCODE, :MMNAME_C, 
                                    SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";

            return DBWork.Connection.Execute(sql, mast, DBWork.Transaction);
        }
        public int UpdateMast(TC_MAST mast)
        {
            string sql = @"update TC_MAST 
                              set mmname_c = :MMNAME_C, update_time = sysdate, update_user = :UPDATE_USER, update_ip = :UPDATE_IP
                            where mmcode = :MMCODE";
            return DBWork.Connection.Execute(sql, mast, DBWork.Transaction);
        }

        #endregion

        #region 訂購/取消訂購
        public int UpdateInvqmtr(TC_INVQMTR invqmtr) {
            string sql = @"update TC_INVQMTR 
                              set purch_st = :PURCH_ST, update_time = sysdate, update_user = :UPDATE_USER, update_ip = :UPDATE_IP
                            where data_ym = :DATA_YM
                              and mmcode = :MMCODE";
            return DBWork.Connection.Execute(sql, invqmtr, DBWork.Transaction);
        }
        #endregion

        #region combo
        public IEnumerable<COMBO_MODEL> GetStatusCombo() {
            string sql = @"select data_value as VALUE,
                                  (data_value ||' '|| data_desc) as TEXT
                             from PARAM_D
                            where grp_code = 'TC_INVQMTR'
                              and data_name = 'PURCH_ST'
                            order by data_value";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        #endregion
    }
}