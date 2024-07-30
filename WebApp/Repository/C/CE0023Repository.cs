using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;
using static WebApp.Repository.C.CE0020Repository;

namespace WebApp.Repository.C
{
    public class CE0023Repository : JCLib.Mvc.BaseRepository
    {
        public CE0023Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public class Sorter
        {
            public string property { get; set; }
            public string direction { get; set; }
        }

        public IEnumerable<CHK_MAST> GetMasterAll(string wh_no, string chk_ym, string keeper)
        {
            var p = new DynamicParameters();
            var sql = @"Select a.CHK_NO, a.CHK_NO1, a.CHK_YM, a.CHK_WH_NO
                        , USER_NAME(a.CHK_KEEPER) AS CHK_KEEPER
                        , WH_NAME(a.CHK_WH_NO) AS WH_NAME
                        , a.CHK_NUM || '/' || a.CHK_TOTAL AS QTY
                        , a.CHK_CLASS || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_CLASS' AND DATA_VALUE=a.CHK_CLASS) AS CHK_CLASS
                        , a.CHK_LEVEL || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_LEVEL' AND DATA_VALUE=a.CHK_LEVEL) AS CHK_LEVEL
                        , a.CHK_WH_GRADE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_GRADE' AND DATA_VALUE=a.CHK_WH_GRADE) AS CHK_WH_GRADE
                        , a.CHK_WH_KIND || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND' AND DATA_VALUE=a.CHK_WH_KIND) AS CHK_WH_KIND
                        , a.CHK_PERIOD || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_PERIOD' AND DATA_VALUE=a.CHK_PERIOD) AS CHK_PERIOD
                        , CASE WHEN a.CHK_WH_KIND='0' THEN a.CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_0' AND DATA_VALUE=a.CHK_TYPE)
                        ELSE a.CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_1' AND DATA_VALUE=a.CHK_TYPE) END AS CHK_TYPE
                        , a.CHK_STATUS || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_STATUS' AND DATA_VALUE=a.CHK_STATUS) AS CHK_STATUS_NAME
                        , a.CHK_STATUS
                        from CHK_MAST a, MI_WHID b
                        where 1=1 
                          and chk_status>='1' 
                          and CHK_LEVEL='2'
                          and b.WH_USERID = :CHK_KEEPER
                          and a.CHK_WH_NO = b.WH_NO 
                          AND a.chk_wh_grade = '2' 
                          AND a.chk_wh_kind = '0' ";
            p.Add(":CHK_KEEPER", keeper);

            //if (query.CHK_NO != "")
            //{
            //    sql += " AND CHK_NO=:CHK_NO";
            //    p.Add(":CHK_NO", query.CHK_NO);
            //}

            if (wh_no != "")
            {
                sql += " AND CHK_WH_NO=:CHK_WH_NO";
                p.Add(":CHK_WH_NO", wh_no);
            }

            if (chk_ym != "")
            {
                sql += " and CHK_YM LIKE :CHK_YM";
                p.Add(":CHK_YM", string.Format("{0}%", chk_ym));
            }

            return DBWork.PagingQuery<CHK_MAST>(sql, p, DBWork.Transaction);

        }

        public IEnumerable<CHK_DETAIL> GetDetailAll(string chk_no, FilterItem filter)
        {
            var p = new DynamicParameters();

            string sql = string.Format(@"with temp_data as (
                                            select a.chk_no, b.seq, 
                                                   a.mmcode, 
                                                   (select base_unit from MI_MAST where mmcode = a.mmcode) as base_unit,
                                                   (SELECT MMNAME_C FROM MI_MAST WHERE mmcode = a.mmcode) as MMNAME_C,
                                                   (SELECT MMNAME_E FROM MI_MAST WHERE mmcode = a.mmcode) as MMNAME_E,
                                                   sum(a.chk_qty) CHK_QTY, 
                                                   (case
                                                       when b.store_qty is null then 0
                                                       else sum(a.chk_qty) - b.store_qty
                                                    end) as qty_diff,
                                                   (select m_contprice from MI_MAST where mmcode = a.mmcode) as m_contprice,
                                                   b.store_qty,
                                                   TWN_DATE(b.CHK_PRE_DATE) CHK_TIME,
                                                   b.memo
                                              from CHK_G2_DETAIL a, CHK_G2_WHINV b {0} {1} 
                                             where a.chk_no = :chk_no 
                                               and a.chk_no = b.chk_no 
                                               and a.mmcode = b.mmcode ",
                                            (filter.e_orderdcflag != string.Empty ||
                                              filter.drug_type == "1" || filter.drug_type == "2" ||
                                              filter.drug_type == "3" || filter.drug_type == "5") ? ", MI_MAST z" : string.Empty,
                                            filter.drug_type == "4" ? ",HIS_BASORDM y" : string.Empty);
            sql += GetWithFilter(filter);
            sql += @"                        group by a.chk_no, b.seq, a.mmcode, b.store_qty, b.CHK_PRE_DATE, b.memo )
                                        select * from temp_data a
                                         where 1=1";
            sql += GetFilter(filter);


            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":gap_t", string.Format("{0}", filter.gap_t));
            p.Add(":gap_price", string.Format("{0}", filter.gap_price));
            p.Add(":gap_p", string.Format("{0}", filter.gap_p));
            p.Add(":m_contprice", string.Format("{0}", filter.m_contprice));
            p.Add(":e_orderdcflag", string.Format("{0}", filter.e_orderdcflag));
            p.Add(":drug_type", string.Format("{0}", filter.drug_type));
            p.Add(":seq1", string.Format("{0}", filter.seq1));
            p.Add(":seq2", string.Format("{0}", filter.seq2));
            p.Add(":mmcode1", string.Format("{0}", filter.mmcode1));
            p.Add(":mmcode2", string.Format("{0}", filter.mmcode2));
            p.Add(":mmname", string.Format("%{0}%", filter.mmname));

            return DBWork.PagingQuery<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }

        private string GetFilter(FilterItem filterItem)
        {
            string filter = string.Empty;

            if (filterItem.gap_t > 0)
            {
                filter += @"     and a.store_qty is not null 
                                 and abs(a.qty_diff ) > :gap_t";
            }
            if (filterItem.gap_price > 0)
            {
                filter += @"     and a.store_qty is not null 
                                 and abs(a.qty_diff * a.m_contprice) > :gap_price";
            }
            if (filterItem.gap_p > 0)
            {
                filter += @"     and a.store_qty is not null 
                                 and abs(a.qty_diff / a.store_qty * 100) > :gap_p";
            }

            return filter;
        }
        private string GetWithFilter(FilterItem filterItem)
        {
            string filter = string.Empty;
            if (filterItem.e_orderdcflag != string.Empty ||
                                             filterItem.drug_type == "1" || filterItem.drug_type == "2" ||
                                             filterItem.drug_type == "3" || filterItem.drug_type == "5")
            {
                filter += @"     and z.mmcode = a.mmcode";
            }
            if (filterItem.seq1 != string.Empty)
            {
                filter += @"     and b.seq >= :seq1";
            }
            if (filterItem.seq2 != string.Empty)
            {
                filter += @"     and b.seq <= :seq2";
            }
            if (filterItem.mmcode1 != string.Empty)
            {
                filter += @"     and a.mmcode >= :mmcode1";
            }
            if (filterItem.mmcode2 != string.Empty)
            {
                filter += @"     and a.mmcode <= :mmcode2";
            }
            if (filterItem.mmname != string.Empty)
            {
                if (filterItem.e_orderdcflag != string.Empty ||
                                             filterItem.drug_type == "1" || filterItem.drug_type == "2" ||
                                             filterItem.drug_type == "3" || filterItem.drug_type == "5")
                {
                    filter += @"  and (z.mmname_c like :mmname or z.mmname_e like :mmname)";
                }
                else
                {
                    filter += @"  and exists (select 1 from MI_MAST 
                                               where mmcode = a.mmcode 
                                                 and (z.mmname_c like :mmname or z.mmname_e like :mmname) )";
                }
            }
            if (filterItem.m_contprice > 0)
            {
                filter += @"     and z.m_contprice > :m_contprice";
            }
            if (filterItem.e_orderdcflag != string.Empty)
            {
                filter += @"     and z.e_orderdcflag = :e_orderdcflag ";
            }
            if (filterItem.drug_type != string.Empty)
            {
                switch (filterItem.drug_type)
                {
                    case "1":   // 管制藥

                        filter += @"    and z.e_restrictcode in ('1','2','3','4')";
                        break;
                    case "2":   // 高價藥
                        filter += @"    and z.e_highpriceflag = 'Y'";
                        break;
                    case "3":   // 研究用藥
                        filter += @"    and z.e_researchdrugflag = 'Y'";
                        break;
                    case "4":   // 罕見病藥
                        filter += @"    and y.ordercode = a.mmcode and y.raredisorderflag = 'Y'";
                        break;
                    case "5":   // 公費藥  借用MI_MAST 回流藥欄位()
                        filter += @"    and z.E_RETURNDRUGFLAG  = 'Y'";
                        break;
                    default:
                        filter += string.Empty;
                        break;
                }
            }


            return filter;
        }

        public IEnumerable<CHK_DETAIL> GetUserDetails(string chk_no, string mmcode)
        {
            var p = new DynamicParameters();

            string sql = @"Select (select una from UR_ID where tuser = a.chk_uid) as CHK_UID_NAME, 
                            a.chk_uid,
                            a.chk_qty, a.chk_no, a.mmcode
                            from CHK_G2_DETAIL a
                            where chk_no = :chk_no 
                            and mmcode = :mmcode";

            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":mmcode", string.Format("{0}", mmcode));

            return DBWork.PagingQuery<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }

        public int SaveEdit(string CHK_QTY, string chk_no, string mmcode, string chk_uid, string UPDATE_USER, string UPDATE_IP)
        {
            string sql = @"Update CHK_G2_DETAIL 
                            set CHK_QTY = :CHK_QTY, UPDATE_USER = :UPDATE_USER, 
                            UPDATE_TIME = SYSDATE, UPDATE_IP = :UPDATE_IP
                            where CHK_NO = :CHK_NO 
                            and mmcode = :mmcode 
                            and chk_uid = :chk_uid";

            return DBWork.Connection.Execute(sql,
                new { CHK_QTY = CHK_QTY, CHK_NO = chk_no, mmcode = mmcode, chk_uid = chk_uid, UPDATE_USER = UPDATE_USER, UPDATE_IP = UPDATE_IP },
                DBWork.Transaction);
        }

        public IEnumerable<CHK_G2_WHINV> GetWhinvs(string chk_no) {
            string sql = @"select * from CHK_G2_WHINV
                            where chk_no = :chk_no";
            return DBWork.Connection.Query<CHK_G2_WHINV>(sql, new { chk_no = chk_no}, DBWork.Transaction);
        }
        public int Update_CHK_G2_DETAIL_Status(string chk_no)
        {
            string sql = @"UPDATE CHK_G2_DETAIL SET STATUS_INI = '3' where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }

        public int Update_CHK_MAST_Status(string chk_no)
        {
            string sql = @"UPDATE CHK_MAST SET CHK_STATUS = '3' where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }

        public string GetStoreQty(string chk_no, string mmcode) {
            string sql = @"select store_qty from CHK_G2_WHINV where chk_no = :chk_no and mmcode = :mmcode";
            return DBWork.Connection.QueryFirst<string>(sql, new { chk_no = chk_no, mmcode = mmcode }, DBWork.Transaction);
        }

        public string GetChkqtySum(string chk_no, string mmcode)
        {
            string sql = @"select sum(chk_qty) from CHK_G2_DETAIL 
                            where chk_no = :chk_no and mmcode = :mmcode 
                            group by chk_no, mmcode";
            return DBWork.Connection.QueryFirst<string>(sql, new { chk_no = chk_no, mmcode = mmcode }, DBWork.Transaction);
        }

        public int Update_CHK_G2_DETAILTOT(string store_qty, string chk_qty, string chk_no1, string mmcode, string UserName, string UserIP, string chk_no, string apl_outqty)
        {
            var p = new DynamicParameters();

            string sql = @"update CHK_G2_DETAILTOT 
                              set store_qty = :store_qty, 
                                  chk_qty2 = :chk_qty, 
                                  status_tot = '2', 
                                  status = '',
                                  gap_t = (:chk_qty - :store_qty), 
                                  miss_per = (case
                                                  when :apl_outqty = '0'
                                                      then round((:chk_qty - :store_qty) * 100, 5)
                                                  else round((( :chk_qty - :store_qty) / :apl_outqty * 100),5)
                                              end),  
                                  update_user = :UserName,
                                  update_time = SYSDATE,
                                  update_ip = :UserIP,
                                  memo = (select memo from CHK_G2_WHINV where chk_no = :chk_no and mmcode = :mmcode)
                            where chk_no = :chk_no1
                              and mmcode = :mmcode";

            p.Add(":store_qty", string.Format("{0}", store_qty));
            p.Add(":chk_qty", string.Format("{0}", chk_qty));
            p.Add(":UserName", string.Format("{0}", UserName));
            p.Add(":UserIP", string.Format("{0}", UserIP));
            p.Add(":chk_no1", string.Format("{0}", chk_no1));
            p.Add(":mmcode", string.Format("{0}", mmcode));
            p.Add(":chk_no", string.Format("{0}", chk_no));
            p.Add(":apl_outqty", string.Format("{0}", apl_outqty));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }



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

        #region 2020-07-15  新增: 備註欄位(CHK_G2_WHINV.MEMO)
        public int UpdateMemo(CHK_G2_WHINV whinv)
        {
            string sql = @"update CHK_G2_WHINV
                              set memo = :memo,
                                  update_user = :update_user,
                                  update_time = sysdate,
                                  update_ip = :update_ip
                            where chk_no = :chk_no and mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, whinv, DBWork.Transaction);
        }
        #endregion

        #region 2020-09-11 藥局消耗量計算修改
        public string GetAploutqty(string start_date, string end_date, string wh_no, string mmcode) {
            string sql = @"select chk_phr_aploutqty(:start_date, :end_date, :wh_no, :mmcode) from dual";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { start_date = start_date, end_date = end_date, wh_no = wh_no, mmcode = mmcode }, DBWork.Transaction);
        }
        #endregion
    }
}