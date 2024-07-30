using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CE0021Repository : JCLib.Mvc.BaseRepository
    {
        public CE0021Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CHK_MAST> GetChkMasts(string wh_no, string chk_ym, string keeper, string chk_status, string chk_level, int page_index, int page_size, string sorters)
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
                                  a.CHK_CLASS,
                                  a.CHK_NUM,
                                  a.CHK_TOTAL,
                                  a.CHK_STATUS,
                                  a.CHK_KEEPER,
                                  (select g.una from UR_ID g where g.tuser = a.CHK_KEEPER) as CHK_KEEPER_NAME,
                                  a.CHK_NO1,
                                  (select b.WH_NO || ' ' ||b.WH_NAME from MI_WHMAST b where b.WH_NO = a.CHK_WH_NO) as WH_NAME,
                                  (select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                     from PARAM_D c
                                    where c.GRP_CODE = 'CHK_MAST' 
                                      and c.DATA_NAME = 'CHK_WH_KIND'
                                      and c.DATA_VALUE = a.CHK_WH_KIND) as WH_KIND_NAME,
                                  (select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                     from PARAM_D c
                                    where c.GRP_CODE = 'CHK_MAST' 
                                      and c.DATA_NAME = 'CHK_CLASS'
                                      and c.DATA_VALUE = a.CHK_CLASS) as CHK_CLASS_NAME,
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
                                  (case
                                    when a.chk_status = '0' then ''
                                    else (select listagg(una, '、') within group (order by una)
                                            from ( 
                                                  (select una 
                                                    from CHK_NOUID c, UR_ID d
                                                   where c.chk_no = a.chk_no
                                                     and c.chk_uid = d.tuser)
                                                  union
                                                  (
                                                  select una from CHK_DETAIL c, UR_ID d
                                                   where c.chk_no = a.chk_no 
                                                     and c.chk_uid = d.tuser
                                                  )
                                                  union
                                                  (
                                                  select una from CHK_GRADE2_UPDN c, UR_ID d
                                                   where c.chk_no = a.chk_no 
                                                     and c.chk_uid = d.tuser
                                                  )
                                             order by una))
                                  end) as chk_uid_names
                             from CHK_MAST a, MI_WHID b
                            where b.WH_USERID = :keeper
                              and a.CHK_WH_NO = b.WH_NO
                              and a.chk_level = :chk_level
                              and a.chk_wh_kind = '0'
                              and a.chk_wh_grade = '2' ";
            if (wh_no != string.Empty)
            {
                sql += "  and a.CHK_WH_NO = :wh_no";
            }
            if (chk_ym != string.Empty)
            {
                sql += string.Format("  and a.CHK_YM like '{0}%'", chk_ym);
            }
            if (chk_status != string.Empty)
            {
                sql += @"     and a.CHK_STATUS = :chk_status";
            }

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_ym", string.Format("{0}", chk_ym));
            p.Add(":keeper", string.Format("{0}", keeper));
            p.Add(":chk_level", string.Format("{0}", chk_level));
            p.Add(":chk_status", string.Format("{0}", chk_status));
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

        #region 新增

        // 取得新增來源清單
        public IEnumerable<CHK_MAST> GetMasterInsertList(string wh_no, string chk_ym, string keeper, int page_index, int page_size, string sorters)
        {
            string chk_ym_datetime = GetDateTime(chk_ym);
            var p = new DynamicParameters();

            string sql = @"select a.CHK_NO,
                                  a.CHK_YM,
                                  a.CHK_WH_NO, 
                                  a.CHK_WH_GRADE,
                                  a.CHK_WH_KIND,
                                  a.CHK_PERIOD,
                                  a.CHK_TYPE,
                                  a.CHK_LEVEL,
                                  a.CHK_CLASS,
                                  a.CHK_NUM,
                                  a.CHK_TOTAL,
                                  a.CHK_STATUS,
                                  a.CHK_KEEPER,
                                  (select g.una from UR_ID g where g.tuser = a.CHK_KEEPER) as CHK_KEEPER_NAME,
                                  a.CHK_NO1,
                                  (select b.WH_NO || ' ' ||b.WH_NAME from MI_WHMAST b where b.WH_NO = a.CHK_WH_NO) as WH_NAME,
                                  (select c.DATA_VALUE || ' ' || c.DATA_DESC 
                                     from PARAM_D c
                                    where c.GRP_CODE = 'CHK_MAST' 
                                      and c.DATA_NAME = 'CHK_CLASS'
                                      and c.DATA_VALUE = a.CHK_CLASS) as CHK_CLASS_NAME,
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
                                      and f.DATA_VALUE = a.CHK_STATUS) as CHK_STATUS_NAME
                             from CHK_MAST a, MI_WHID b
                            where b.WH_USERID = :keeper
                              and a.CHK_WH_NO = b.WH_NO
                              and SUBSTR(a.CHK_YM, 0, 5) = :chk_ym
                              and a.CHK_WH_NO = :wh_no
                              and a.chk_level = '1'
                              and a.chk_status = '3'
                              and a.chk_wh_kind = '0'
                              and a.chk_wh_grade = '2'
                              and a.chk_no not in (
                                    select NVL(chk_no1, ' ')
                                      from CHK_MAST
                                     where CHK_WH_NO = :wh_no
                                       and chk_level = '2'
                                       and SUBSTR(chk_ym, 0, 5) = :chk_ym
                                    )";

            p.Add(":wh_no", string.Format("{0}", wh_no));
            p.Add(":chk_ym", string.Format("{0}", chk_ym));
            p.Add(":keeper", string.Format("{0}", keeper));
            p.Add(":chk_ym_datetime", chk_ym_datetime);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CHK_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }
        private string GetDateTime(string yyymm)
        {
            int yyyy = int.Parse(yyymm.Substring(0, 3)) + 1911;
            string mm = yyymm.Substring(3, 2);
            return string.Format("{0}-{1}-01", yyyy, mm);
        }

        public int InsertChkMast(CHK_MAST mast)
        {
            string sql = @"insert into CHK_MAST
                                  (CHK_NO, CHK_YM, CHK_WH_NO, CHK_WH_GRADE, CHK_WH_KIND,
                                  CHK_PERIOD, CHK_TYPE, CHK_LEVEL, CHK_NUM, CHK_TOTAL, CHK_STATUS, CHK_KEEPER, CHK_NO1,
                                  CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                           values (:chk_no, :chk_ym, :chk_wh_no, :chk_wh_grade, :chk_wh_kind,
                                   :chk_period, :chk_type, '2', 0, 0, 0, :chk_keeper, : chk_no1,
                                   sysdate, :create_user, sysdate, :update_user, :update_ip
                                  )";

            return DBWork.Connection.Execute(sql, mast, DBWork.Transaction);
        }

        public bool CheckExists(string chk_no)
        {
            string sql = @"select 1 from CHK_MAST
                            WHERE chk_no1 = :chk_no "; // TRUNC(a.EXP_DATE, 'DD') TWN_TODATE(:exp_date)
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new { chk_no = chk_no, },
                                                     DBWork.Transaction) == null);
        }
        public string GetCurrentSeq(string wh_no, string ym)
        {
            string sql = @"select NVL(max(substr(CHK_NO ,15,2))+1, 1)
                             from CHK_MAST 
                            where substr(chk_no,1,11) = :chk_no";

            string result = DBWork.Connection.QueryFirst<string>(sql,
                                                         new
                                                         {
                                                             chk_no = string.Format("{0}{1}", wh_no, ym)
                                                         }, DBWork.Transaction);
            return result.PadLeft(2, '0');

        }
        #endregion

        #region 刪除
        public int DeleteChkDetail(string chk_no)
        {
            string sql = @"delete from CHK_DETAIL
                            where chk_no = :chk_no";

            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }

        public int DeleteChkMast(string chk_no)
        {
            string sql = @"delete from CHK_MAST
                            where chk_no = :chk_no";

            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }
        #endregion

        #region 盤點明細主畫面

        public IEnumerable<CHK_G2_WHINV> GetIncludeDetails(string chk_no)
        {
            var p = new DynamicParameters();

            string sql = @"select a.chk_no, a.wh_no, a.mmcode, a.store_qty, twn_date(a.chk_pre_date) as chk_pre_date,
                                  b.mmname_c, b.mmname_e, b.base_unit, a.seq
                             from CHK_G2_WHINV a, MI_MAST b
                            where a.chk_no = :chk_no
                              and b.mmcode = a.mmcode";

            p.Add(":chk_no", string.Format("{0}", chk_no));

            return DBWork.PagingQuery<CHK_G2_WHINV>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CHK_G2_DETAILTOT> GetExcludeDetails(string chk_no, string chk_no1, float f_u_price, float f_number, float f_amount, float miss_per)
        {
            var p = new DynamicParameters();

            string sql = @"select a.chk_no, a.wh_no, a.mmcode, a.store_qty, a.gap_t, a.miss_per, a.chk_qty1,
                                  a.status_tot, b.mmname_c, b.mmname_e, b.base_unit,
                                  (select seq from CHK_G2_WHINV where chk_no = a.chk_no and mmcode = a.mmcode) as seq
                             from CHK_G2_DETAILTOT a, MI_MAST b
                            where a.chk_no = :chk_no1
                              and a.status_tot = '1'
                              and b.mmcode = a.mmcode";

            sql += GetFilter(f_u_price, f_number, f_amount, miss_per);
            sql += @"         and not exists ( select 'x' from CHK_G2_WHINV 
                                                where chk_no = :chk_no 
                                                  and mmcode = a.mmcode)";


            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":chk_no1", string.Format("{0}", chk_no1));
            p.Add(":f_u_price", string.Format("{0}", f_u_price));
            p.Add(":f_number", string.Format("{0}", f_number));
            p.Add(":f_amount", string.Format("{0}", f_amount));
            p.Add(":miss_per", string.Format("{0}", miss_per));

            return DBWork.PagingQuery<CHK_G2_DETAILTOT>(sql, p, DBWork.Transaction);
        }

        private string GetFilter(float f_u_price, float f_number, float f_amount, float miss_per)
        {
            string filter = string.Empty;

            if (f_u_price > 0)
            {
                filter += @"   and b.m_contprice >= :f_u_price";
            }
            if (f_number > 0)
            {
                filter += @"   and ABS(a.gap_t ) >= ABS(:f_number)";
            }
            if (f_amount > 0)
            {
                filter += @"   and ABS(a.gap_t * b.m_contprice) >= ABS(:f_amount)";
            }
            if (miss_per > 0)
            {
                filter += @"   and ABS(a.miss_per) >= ABS(:miss_per)";
            }

            return filter;
        }

        public int InsertG2Whinv(CHK_G2_WHINV whinv) {
            string sql = @"insert into CHK_G2_WHINV (chk_no, wh_no, mmcode,
                                                     create_date, create_user, update_time, update_user, update_ip, seq)
                           values (:chk_no, :wh_no, :mmcode, sysdate, :create_user, sysdate, :update_user, :update_ip, :seq)"; ;
            return DBWork.Connection.Execute(sql, whinv, DBWork.Transaction);
        }

        public int DeleteG2Whinv(string chk_no, string mmcode) {
            string sql = @"delete from CHK_G2_WHINV where chk_no = :chk_no and mmcode = :mmcode";

            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, mmcode = mmcode }, DBWork.Transaction);
        }
        #endregion

        #region 重盤
        public CHK_MAST GetMast(string chk_no) {
            string sql = @"select * from CHK_MAST where chk_no = :chk_no";

            return DBWork.Connection.QueryFirst<CHK_MAST>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public bool ChkG2WhinvExist(string chk_no)
        {
            string sql = @"select 1 from CHK_G2_WHINV where chk_no = :chk_no";

            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         chk_no = chk_no
                                                     },
                                                     DBWork.Transaction) == null);
        }

        public IEnumerable<CHK_DETAILTOT> GetDetialTotsR(string chk_no1) {
            string sql = @"select * from CHK_G2_DETAILTOT
                            where chk_no = :chk_no1
                              and status = 'R'";
            return DBWork.Connection.Query<CHK_DETAILTOT>(sql, new { chk_no1 = chk_no1 }, DBWork.Transaction);
        }

        public int InsertChkG2Whinv(CHK_G2_WHINV whinv)
        {
            string sql = @"insert into CHK_G2_WHINV values (
                                :chk_no, :wh_no, :mmcode, '', '', sysdate, :create_user, sysdate, :update_user, :update_ip
                           )";
            return DBWork.Connection.Execute(sql, whinv, DBWork.Transaction);
        }

        public IEnumerable<CHK_G2_WHINV> GetChkG2Whinvs(string chk_no, string seq1, string seq2, string mmcode1, string mmcode2, bool chkpredateNullonly)
        {
            string sql = @"select a.chk_no, a.wh_no, a.mmcode, a.store_qty, twn_date(a.chk_pre_date) as chk_pre_date,
                                  (select mmname_c from MI_MAST where mmcode = a.mmcode) as mmname_c,
                                  (select mmname_e from MI_MAST where mmcode = a.mmcode) as mmname_e,
                                  (select base_unit from MI_MAST where mmcode = a.mmcode) as base_unit,
                                  a.seq
                             from CHK_G2_WHINV a
                            where a.chk_no = :chk_no";
            if (seq1 != string.Empty)
            {
                sql += "      and a.seq >= :seq1";
            }
            if (seq2 != string.Empty)
            {
                sql += "      and a.seq <= :seq2";
            }
            if (mmcode1 != string.Empty)
            {
                sql += "      and a.mmcode >= :mmcode1";
            }
            if (mmcode2 != string.Empty)
            {
                sql += "      and a.mmcode <= :mmcode2";
            }
            if (chkpredateNullonly)
            {
                sql += "      and a.chk_pre_date is null";
            }

            return DBWork.PagingQuery<CHK_G2_WHINV>(sql, new
            {
                chk_no = chk_no,
                seq1 = seq1,
                seq2 = seq2,
                mmcode1 = mmcode1,
                mmcode2 = mmcode2
            }, DBWork.Transaction);

        }
        #endregion

        #region 分派盤點人元
        public IEnumerable<BC_WHCHKID> GetPickUsers(string wh_no)
        {
            string sql = @"select a.WH_CHKUID, 
                                  b.una as WH_CHKUID_NAME
                             from BC_WHCHKID a, UR_ID b
                            where a.WH_NO = :wh_no
                              and b.tuser = a.WH_CHKUID";
            return DBWork.Connection.Query<BC_WHCHKID>(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }

        public int UpdateMaster(string chk_no)
        {
            string sql = @"update CHK_MAST set chk_status = '1',
                                               chk_total = (select count(*) from chk_detail where chk_no = :chk_no)
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }
        #endregion

        #region combo

        public class WhnoComboItem
        {
            public string WH_NO { get; set; }
            public string WH_NAME { get; set; }
            public string WH_KIND { get; set; }
            public string WH_GRADE { get; set; }
        }

        public IEnumerable<WhnoComboItem> GetWhnoCombo(string wh_userId)
        {
            string sql = @"select b.WH_NO, 
                                  (b.WH_NO || ' ' || b.WH_NAME) as WH_NAME, 
                                  b.WH_KIND, 
                                  b.WH_GRADE
                             from MI_WHID a, MI_WHMAST b
                            where a.WH_USERID = :WH_USERID
                              and b.WH_NO = a.WH_NO
                               and b.wh_grade = '2'
                              and b.wh_kind = '0'
                            order by b.WH_NO";
            return DBWork.Connection.Query<WhnoComboItem>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }

        #endregion

        #region 2020-06-11 新增項次欄位
        public string GetMedSeq(string chk_no, string mmcode)
        {
            string sql = @"select seq
                             from CHK_G2_WHINV 
                            where chk_no = (select chk_no1 from CHK_MAST where chk_no = :chk_no)
                              and mmcode = :mmcode";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new
            {
                chk_no = chk_no,
                mmcode = mmcode,
            }, DBWork.Transaction);
        }
        
        #endregion
    }
}