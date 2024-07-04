using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;
using WebApp.Models.C;

namespace WebApp.Repository.C
{
    public class CE0033Repository : JCLib.Mvc.BaseRepository
    {
        public CE0033Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CHK_DETAIL> GetUserChkList(string chk_no, string chk_uid) {
            string sql = @"select chk_no, mmcode, mmname_c, mmname_e, base_unit,
                                  m_contprice, wh_no, store_loc, loc_name,
                                  m_storeid, store_qtyc, store_qtym, store_qtys,
                                  chk_qty, chk_remark, chk_uid, status_ini, 
                                  chk_time,
                                  store_qty_n, store_qty_update_time,
                                  create_date, create_user, update_time, update_user
                             from CHK_DETAIL
                            where chk_no = :chk_no and chk_uid = :chk_uid
                            order by COALESCE(TO_NUMBER(REGEXP_SUBSTR(store_loc, '^\d+')), 9999999999), mmcode";
            return DBWork.Connection.Query<CHK_DETAIL>(sql, new { chk_no = chk_no, chk_uid = chk_uid}, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetItemDetail(string chk_no, string store_loc, string barcode, string mmcode, string chk_uid) {
            string sql = @"select chk_no, mmcode, mmname_c, mmname_e, base_unit,
                                  m_contprice, wh_no, store_loc, loc_name,
                                  m_storeid, store_qtyc, store_qtym, store_qtys,
                                  chk_qty, chk_remark, chk_uid, status_ini, 
                                  chk_time,
                                  store_qty_n, store_qty_update_time,
                                  create_date, create_user, update_time, update_user 
                             from CHK_DETAIL 
                            where chk_no = :chk_no and chk_uid = :chk_uid
                              and store_loc = :store_loc
                              and mmcode = (select mmcode from BC_BARCODE where barcode = :barcode)";
            if (mmcode != string.Empty) {
                sql += "      and mmcode = :mmcode";
            }

            return DBWork.Connection.Query<CHK_DETAIL>(sql, new { chk_no = chk_no, chk_uid = chk_uid , store_loc = store_loc, barcode = barcode, mmcode = mmcode}, DBWork.Transaction);
        }

        public int updateChkqty(string chk_no, string store_loc, string mmcode, string chk_uid, string chk_qty, string update_ip) {
            string sql = @"update CHK_DETAIL
                              set chk_qty = :chk_qty, chk_time = sysdate, update_user = :chk_uid, update_ip = :update_ip
                            where chk_no = :chk_no
                              and store_loc = :store_loc
                              and mmcode = :mmcode
                              and chk_uid = :chk_uid";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, store_loc = store_loc, mmcode = mmcode, chk_uid=chk_uid, chk_qty = chk_qty, update_ip = update_ip}, DBWork.Transaction);
        }

        public CHK_MAST GetChkMast(string chk_no) {
            string sql = @"select * from CHK_MAST where chk_no = :chk_no";

            return DBWork.Connection.QueryFirstOrDefault<CHK_MAST>(sql, new { chk_no = chk_no}, DBWork.Transaction);
        }

        #region combos
        public IEnumerable<CE0003> GetChknoCombo(string chk_level, string userId)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.chk_no, a.chk_ym, a.chk_status,
                               (select data_desc from PARAM_D where grp_code = 'CHK_MAST' and data_name = 'CHK_WH_KIND_1'  and data_value = a.chk_type) as chk_type_name, 
                               (select data_desc from PARAM_D where grp_code = 'CHK_MAST' and data_name = 'CHK_CLASS'  and data_value = a.chk_class) as chk_class_name,
                               (select data_desc from PARAM_D where grp_code = 'CHK_MAST' and data_name = 'CHK_STATUS'  and data_value = a.chk_status) as chk_status_name
                          from CHK_MAST a
                         where a.chk_level = :chk_level
                           and a.chk_wh_no = WHNO_MM1
                           and a.chk_status > '0'
                           and a.chk_ym = TWN_YYYMM(sysdate)
                           and chk_no  in  (select DISTINCT chk_no from CHK_DETAIL where CHK_UID = :chk_uid ) 
                        order by a.chk_no";

            p.Add(":chk_level", chk_level);
            p.Add(":chk_uid", userId);

            return DBWork.Connection.Query<CE0003>(sql, p, DBWork.Transaction);
        }
        #endregion
    }
}