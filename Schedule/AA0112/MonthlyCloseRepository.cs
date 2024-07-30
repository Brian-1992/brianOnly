using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace AA0112
{
    public class MonthlyCloseRepository : JCLib.Mvc.BaseRepository
    {
        public MonthlyCloseRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public string GetMnSetDate() {
            string sql = @"select to_char(set_ctime, 'YYYY-MM-DD') from MI_MNSET 
                            where set_status = 'N'";
            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        public IEnumerable<CHK_MAST> GetUndoneMasts(string chk_ym) {
            string sql = @"select * from CHK_MAST
                            where chk_ym = :chk_ym
                              and chk_period = 'M'
                            order by chk_no1, chk_level";
            return DBWork.Connection.Query<CHK_MAST>(sql, new { chk_ym = chk_ym},DBWork.Transaction);
        }
        public int DeleteChkDetail(string chk_no)
        {
            string sql = @"delete from CHK_DETAIL
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public int DeleteChkDetailTemp(string chk_no)
        {
            string sql = @"delete from CHK_DETAIL_TEMP
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public int DeleteChkMast(string chk_no) {
            string sql = @"delete from CHK_MAST
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no}, DBWork.Transaction);
        }
        public int UpdatePreMaster(string chk_no1, string preLevel)
        {
            string sql = @"update CHK_MAST 
                              set chk_status = '3', update_time = sysdate
                            where chk_no1 = :chk_no1 and chk_level = :preLevel";
            return DBWork.Connection.Execute(sql, new { chk_no1 = chk_no1, preLevel = preLevel}, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetDetails(string chk_no) {
            string sql = @"select * from CHK_DETAIL
                            where chk_no = :chk_no";
            return DBWork.Connection.Query<CHK_DETAIL>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public int UpdateChkDetail2(string chk_no, string mmcode)
        {
            string sql = @"update CHK_DETAIL
                              set update_time = sysdate,
                                  update_user = 'BATCH',
                                  update_ip = '',
                                  status_ini = '3'
                            where chk_no = :chk_no
                              and mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no,mmcode = mmcode }, DBWork.Transaction);
        }
        public int UpdateChkDetail1(string chk_no, string wh_no, string mmcode) {
            string sql = @"update CHK_DETAIL
                              set store_qty_n = (select inv_qty from MI_WHINV where wh_no = :wh_no and mmcode = :mmcode),
                                  store_qty_update_time = sysdate,
                                  chk_qty = (select inv_qty from MI_WHINV where wh_no = :wh_no and mmcode = :mmcode), 
                                  update_time = sysdate,
                                  update_user = 'BATCH',
                                  update_ip = '',
                                  status_ini = '3'
                            where chk_no = :chk_no
                              and mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, wh_no = wh_no, mmcode = mmcode }, DBWork.Transaction);
        }
        public int UpdateMaster(string chk_no) {
            string sql = @"update CHK_MAST
                              set chk_status = '3', update_time = sysdate,
                                  update_user = 'BATCH',
                                  update_ip = ''
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no}, DBWork.Transaction);
        }
        public MI_MAST GetMiMast(string mmcode) {
            string sql = @"select * from MI_MAST where mmcode = :mmcode";
            return DBWork.Connection.QueryFirst<MI_MAST>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }

        #region insert into CHK_DETAILTOT 初盤
        public string GetSTORE_QTY(string chk_no, string mmcode, string chk_wh_grade)
        {
            var sql = string.Format(@"SELECT store_qty
                                        from (select sum(NVL(
                                                            (case 
                                                                when store_qty_update_time is null
                                                                    then (select INV_QTY from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode)
                                                                else store_qty_n
                                                            end), 
                                                    0)) STORE_QTY 
                                        FROM CHK_DETAIL a
                                       WHERE a.CHK_NO=:CHK_NO and a.MMCODE=:MMCODE GROUP BY a.WH_NO, a.MMCODE)");
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string GetLAST_QTYC(string chk_ym, string mmcode, string wh_no)
        {
            var sql = @"SELECT NVL((select sum(USE_QTY) from MI_WINVMON
                            where DATA_YM >=  to_char(add_months(to_date(:CHK_YM,'yyymm'),-3),'yyymm') and wh_no=:WH_NO and mmcode=:MMCODE
                            group by WH_NO, MMCODE), 0) LAST_QTYC FROM DUAL";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_YM = chk_ym, MMCODE = mmcode, WH_NO = wh_no }, DBWork.Transaction);
        }
        public string GetCHK_QTY(string chk_no, string mmcode)
        {
            var sql = @"SELECT CHK_QTY FROM (SELECT SUM(NVL(CHK_QTY, 0)) CHK_QTY FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY MMCODE, WH_NO)";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string GetAPL_OUTQTY(string mmcode, string wh_no)
        {
            var sql = @"select APL_OUTQTY from MI_WHINV where wh_no=:WH_NO and mmcode=:MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { MMCODE = mmcode, WH_NO = wh_no }, DBWork.Transaction);
        }

        public int InsertDetailTot(CHK_DETAILTOT tot)
        {
            var sql = @"INSERT INTO CHK_DETAILTOT (chk_no, mmcode, mmname_c, mmname_e, base_unit, m_contprice, wh_no,
                                                   store_loc, mat_class, m_storeid, store_qty, store_qtyc, store_qtym, store_qtys,
                                                   last_qty, last_qtyc, last_qtym, last_qtys, gap_t, gap_c, gap_m, gap_s,
                                                   pro_los_qty, pro_los_amount, miss_per, miss_perc, miss_perm, miss_pers,
                                                   apl_outqty, chk_remark, chk_qty1, chk_qty2, chk_qty3, status_tot,
                                                   create_date, create_user,update_time, update_user, update_ip,
                                                   store_qty1, store_qty2, store_qty3, store_qty_time1, consume_qty, consume_amount)
                        (select distinct
                                a.CHK_NO ,
                                a.MMCODE  ,
                                b.MMNAME_C ,
                                b.MMNAME_E ,
                                b.BASE_UNIT ,
                                b.M_CONTPRICE ,
                                a.WH_NO  ,
                                '' STORE_LOC,
                                b.MAT_CLASS  ,
                                b.M_STOREID ,
                                :STORE_QTY  ,
                                :STORE_QTYC  ,
                                :STORE_QTYM  ,
                                :STORE_QTYS  ,
                                :LAST_QTY,
                                :LAST_QTYC,                  
                                :LAST_QTYM,
                                :LAST_QTYS   ,                  
                                :GAP_T,       
                                :GAP_C   ,
                                0 GAP_M   ,
                                0 GAP_S   ,
                                :PRO_LOS_QTY ,
                                :PRO_LOS_AMOUNT ,
                                :MISS_PER,
                                :MISS_PERC ,
                                0 MISS_PERM  ,
                                0 MISS_PERS  ,
                                :APL_OUTQTY,                         
                                0 CHK_REMARK ,
                                :CHK_QTY1 as chk_qty1 ,                          
                                0 CHK_QTY2  ,
                                0 CHK_QTY3  ,
                                '1' STATUS_TOT  , 
                                SYSDATE,
                                :UPDATE_USER,
                                SYSDATE,
                                :UPDATE_USER ,
                                :UPDATE_IP  ,
                                :STORE_QTY as store_qty1, 0 as store_qty2, 0 as store_qty3,
                                (select store_qty_update_time from CHK_DETAIL 
                                  where chk_no = a.chk_no and mmcode = b.mmcode and rownum = 1) as store_qty_time1,
                                :consume_qty, :consume_amount
                           from CHK_DETAIL a, MI_MAST b
                          where a.CHK_NO=:CHK_NO AND a.MMCODE=:MMCODE AND a.MMCODE = b.MMCODE)";
            return DBWork.Connection.Execute(sql, tot, DBWork.Transaction);
        }
        #endregion

        #region update CHK_DETAILTOT 複盤 三盤
        public int UpdateDetailTot(CHK_DETAILTOT tot, CHK_MAST mast)
        {
            var sql = string.Format(@"update CHK_DETAILTOT 
                                         set store_qty{0} = :store_qty,
                                             store_qty_time{0} = (select store_qty_update_time from CHK_DETAIL 
                                                                   where chk_no = '{1}' and mmcode = :mmcode and rownum = 1),
                                             store_qty = :store_qty,
                                             store_qtyc = :store_qtyc,
                                             last_qty = :last_qty,
                                             last_qtyc = :last_qtyc,
                                             gap_t = :gap_t,
                                             gap_c = :gap_c,
                                             pro_los_qty = :pro_los_qty,
                                             pro_los_amount = :pro_los_amount,
                                             miss_per = :miss_per,
                                             miss_perc = :miss_perc,
                                             update_user = :update_user,
                                             update_ip = :update_ip,
                                             consume_qty = :consume_qty,
                                             consume_amount = :consume_amount,
                                             status_tot = {0},
                                             chk_qty{0} = :chk_qty{0}
                                       where chk_no = :chk_no and mmcode = :mmcode", mast.CHK_LEVEL, mast.CHK_NO);

            return DBWork.Connection.Execute(sql, tot, DBWork.Transaction);
        }

        public bool IsExists(string chk_no, string mmcode)
        {
            string sql = @"select 1 from CHK_DETAILTOT where chk_no = :chk_no and mmcode = :mmcode";
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new { chk_no = chk_no, mmcode = mmcode },
                                                     DBWork.Transaction) == null);
        }

        public int InsertDetailTot23(CHK_DETAILTOT tot, string chk_no1, string chk_no)
        {
            var sql = string.Format(@"INSERT INTO CHK_DETAILTOT (chk_no, mmcode, mmname_c, mmname_e, base_unit, m_contprice, wh_no,
                                                   store_loc, mat_class, m_storeid, store_qty, store_qtyc, store_qtym, store_qtys,
                                                   last_qty, last_qtyc, last_qtym, last_qtys, gap_t, gap_c, gap_m, gap_s,
                                                   pro_los_qty, pro_los_amount, miss_per, miss_perc, miss_perm, miss_pers,
                                                   apl_outqty, chk_remark, chk_qty1, chk_qty2, chk_qty3, status_tot,
                                                   create_date, create_user,update_time, update_user, update_ip,
                                                   store_qty1, store_qty2, store_qty3, store_qty_time1, store_qty_time2, store_qty_time3, consume_qty, consume_amount)
                        (select 
                                :CHK_NO as CHK_NO ,
                                :MMCODE as MMCODE ,
                                (select mmname_c from MI_MAST where mmcode = :MMCODE) as mmname_c,
                                (select mmname_e from MI_MAST where mmcode = :MMCODE) as mmname_e,
                                (select base_unit from MI_MAST where mmcode = :MMCODE) as base_unit,
                                (select nvl(m_contprice, '0') from MI_MAST where mmcode = :MMCODE) as m_contprice,
                                :WH_NO as WH_NO  ,
                                '' STORE_LOC,
                                (select mat_class from MI_MAST where mmcode = :MMCODE) as mat_class,
                                (select m_storeid from MI_MAST where mmcode = :MMCODE) as m_storeid,
                                :STORE_QTY  ,
                                :STORE_QTYC  ,
                                :STORE_QTYM  ,
                                :STORE_QTYS  ,
                                :LAST_QTY,
                                :LAST_QTYC,                  
                                :LAST_QTYM,
                                :LAST_QTYS   ,                  
                                :GAP_T,       
                                :GAP_C   ,
                                0 GAP_M   ,
                                0 GAP_S   ,
                                :PRO_LOS_QTY ,
                                :PRO_LOS_AMOUNT ,
                                :MISS_PER,
                                :MISS_PERC ,
                                0 MISS_PERM  ,
                                0 MISS_PERS  ,
                                :APL_OUTQTY,                         
                                ' ' CHK_REMARK ,
                                :CHK_QTY1 as chk_qty1 ,                          
                                :CHK_QTY2 as CHK_QTY2  ,
                                :CHK_QTY3 as CHK_QTY3  ,
                                :STATUS_TOT STATUS_TOT  , 
                                SYSDATE,
                                :UPDATE_USER,
                                SYSDATE,
                                :UPDATE_USER ,
                                :UPDATE_IP  ,
                                '0' as store_qty1,
                                {0} as store_qty2,
                                {1} as store_qty3,
                                '' as store_qty_time1,
                                {2} as store_qty_time2,
                                {3} as store_qty_time3,
                                :consume_qty, :consume_amount
                            from dual )",
                           tot.STATUS_TOT == "2" ? ":STORE_QTY" : "'0'",
                           tot.STATUS_TOT == "3" ? ":STORE_QTY" : "'0'",
                           tot.STATUS_TOT == "2" ? string.Format("(select store_qty_update_time from CHK_DETAIL where chk_no = '{0}' and mmcode = '{1}' and rownum = 1)", chk_no, tot.MMCODE) : "''",
                           tot.STATUS_TOT == "3" ? string.Format("(select store_qty_update_time from CHK_DETAIL where chk_no = '{0}' and mmcode = '{1}' and rownum = 1)", chk_no, tot.MMCODE) : "''");
            return DBWork.Connection.Execute(sql, tot, DBWork.Transaction);
        }
        #endregion
    }
}
