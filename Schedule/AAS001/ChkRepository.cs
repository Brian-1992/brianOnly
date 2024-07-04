using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;
using Dapper;
using JCLib.DB;

namespace AAS001
{
    public class ChkRepository : JCLib.Mvc.BaseRepository
    {
        public ChkRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        
        #region master
        public MI_WHMAST GetMiwhmast(string wh_no) {
            string sql = @"select * from MI_WHMAST where wh_no = :wh_no";
            return DBWork.Connection.QueryFirst<MI_WHMAST>(sql, new { wh_no = wh_no}, DBWork.Transaction);
        }

        public IEnumerable<LIS_CHK_MAST> GetLisChkmasts() {
            string sql = @"select * from LIS_CHK_MAST 
                            where rdtime is null";
            return DBWork.Connection.Query<LIS_CHK_MAST>(sql, DBWork.Transaction);
        }

        public int InsertChkmast(CHK_MAST mast) {
            string sql = @"insert into CHK_MAST 
                                  (chk_no, chk_ym, chk_wh_no, chk_wh_grade, chk_wh_kind, 
                                   chk_class, chk_period, chk_type, chk_level, chk_num,
                                   chk_total, chk_status, chk_no1,
                                   create_date, create_user, update_time, update_user, update_ip)
                          values (:chk_no, :chk_ym, :chk_wh_no, :chk_wh_grade, :chk_wh_kind,
                                  :chk_class, :chk_period, :chk_type, :chk_level, :chk_num,
                                  :chk_total, :chk_status, :chk_no1,
                                  sysdate, 'BATCH', sysdate, 'BATCH', '')";
            return DBWork.Connection.Execute(sql, mast, DBWork.Transaction);
        }

        public int UpdateChkmastCount(string chk_no) {
            string sql = @"update CHK_MAST 
                              set chk_num = (select count(*) from CHK_DETAILTOT where chk_no = :chk_no), 
                                  chk_total = (select count(*) from CHK_DETAILTOT where chk_no = :chk_no)
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no}, DBWork.Transaction);
        }
        #endregion

        #region detail
        public IEnumerable<LIS_CHK_DETAILTOT> GetLischkDetailtots(string chk_no) {
            string sql = @"select * from LIS_CHK_DETAILTOT
                            where chk_no = :chk_no";
            return DBWork.Connection.Query<LIS_CHK_DETAILTOT>(sql, new { chk_no = chk_no}, DBWork.Transaction);
        }

        public MI_MAST GetMimast(string mmcode) {
            string sql = @"select * from MI_MAST
                            where mmcode = :mmcode";
            return DBWork.Connection.QueryFirst<MI_MAST>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }

        public string GetAPL_OUTQTY(string mmcode, string wh_no)
        {
            var sql = @"select APL_OUTQTY from MI_WHINV where wh_no=:WH_NO and mmcode=:MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { MMCODE = mmcode, WH_NO = wh_no }, DBWork.Transaction);
        }
        public string GetSTORE_QTY(string wh_no, string mmcode) {
            string sql = @"select inv_qty 
                             from MI_WHINV
                            where wh_no = :wh_no 
                              and mmcode = :mmcode";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode = mmcode, wh_no = wh_no }, DBWork.Transaction);
        }
        public string GetLAST_QTYC(string chk_ym, string mmcode, string wh_no)
        {
            //var sql = @"SELECT NVL((select sum(USE_QTY) from MI_WINVMON
            //                where DATA_YM >=  to_char(add_months(to_date(:CHK_YM,'yyymm'),-3),'yyymm') and wh_no=:WH_NO and mmcode=:MMCODE
            //                group by WH_NO, MMCODE), 0) LAST_QTYC FROM DUAL";
            var sql = @"SELECT NVL((select sum(consume_qty) from LIS_CONSUME
                               where data_date >=  to_char(add_months(to_date(:CHK_YM,'yyymm'),-3),'yyymm') and CHK_WH_NO=:WH_NO and mmcode=:MMCODE
                               group by CHK_WH_NO, MMCODE), 0) LAST_QTYC FROM DUAL";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_YM = chk_ym, MMCODE = mmcode, WH_NO = wh_no }, DBWork.Transaction);
        }

        public int InsertDetiltot(CHK_DETAILTOT tot) {
            string sql = @"insert into CHK_DETAILTOT 
                                  (chk_no, mmcode, mmname_c, mmname_e, base_unit,
                                   m_contprice, wh_no, store_loc, mat_class, m_storeid,
                                   store_qty, store_qtyc, store_qtym, store_qtys,
                                   last_qty, last_qtyc, last_qtym, last_qtys,
                                   gap_t, gap_c, gap_m, gap_s, pro_los_qty, pro_los_amount,
                                   miss_per, miss_perc, miss_perm, miss_pers,
                                   apl_outqty, chk_remark, chk_qty1, status_tot, consume_qty, consume_amount,
                                   create_date, create_user, update_time, update_user)
                           values (:chk_no, :mmcode, :mmname_c, :mmname_e, :base_unit,
                                   :m_contprice, :wh_no, :store_loc, :mat_class, :m_storeid,
                                   :store_qty, :store_qtyc, :store_qtym, :store_qtys,
                                   :last_qty, :last_qtyc, :last_qtym, :last_qtys,
                                   :gap_t, :gap_c, :gap_m, :gap_s, :pro_los_qty, :pro_los_amount,
                                   :miss_per, :miss_perc, :miss_perm, :miss_pers,
                                   :apl_outqty, :chk_remark, :chk_qty1, :status_tot, :consume_qty, :consume_amount,
                                   :create_date, :create_user, :update_time, :update_user)";
            return DBWork.Connection.Execute(sql, tot, DBWork.Transaction);
        }
        #endregion

        #region update LIS
        public int UpdateLisChkMast(string chk_no) {
            string sql = @"update LIS_CHK_MAST
                              set rdtime = sysdate
                            where chk_no = :chk_no";

            return DBWork.Connection.Execute(sql, new { chk_no = chk_no}, DBWork.Transaction);
        }

        public int UpdateLisChkDetailtot(string chk_no)
        {
            string sql = @"update LIS_CHK_DETAILTOT
                              set rdtime = sysdate
                            where chk_no = :chk_no";

            return DBWork.Connection.Execute(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        #endregion
    }
}
