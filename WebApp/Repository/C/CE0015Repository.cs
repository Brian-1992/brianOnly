using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CE0015Repository : JCLib.Mvc.BaseRepository
    {
        public CE0015Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public class OrderByItem {
            public string property { get; set; }
            public string direction { get; set; }
        }

        public IEnumerable<CHK_GRADE2_P> GetInclude(string chk_ym)
        {
            string sql = @"select a.mmcode, b.mmname_c, b.mmname_e, b.base_unit,
                                  b.m_purun, b.m_contprice, b.e_takekind
                             from CHK_GRADE2_P a, MI_MAST b
                            where a.chk_ym = :chkym
                              and b.mmcode = a.mmcode";
            //return DBWork.Connection.Query<CHK_GRADE2_P>(sql, new { CHKYM = chk_ym }, DBWork.Transaction);
            return DBWork.PagingQuery<CHK_GRADE2_P>(sql, new { CHKYM = chk_ym }, DBWork.Transaction);
        }

        private string GetFilter(float gap_t, float gap_price, float gap_p, float m_contprice, string e_orderdcflag, string drug_type) {
            string filter = string.Empty;

            if (gap_t > 0) {
                filter += @"     and (a.gap_t ) > :gap_t";
            }
            if (gap_price > 0)
            {
                filter += @"     and (a.gap_t * a.m_contprice) > :gap_price";
            }
            if (gap_p> 0)
            {
                filter += @"     and (a.gap_t / a.store_qty * 100) > :gap_p";
            }
            if (m_contprice > 0)
            {
                filter += @"     and z.m_contprice > :m_contprice";
            }
            if (e_orderdcflag != string.Empty)
            {
                filter += @"     and z.e_orderdcflag = :e_orderdcflag";
            }
            if (drug_type != string.Empty)
            {
                switch (drug_type) {
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
                        filter += @"    and y.ordercode = z.mmcode and y.raredisorderflag = 'Y'";
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

        public IEnumerable<CHK_GRADE2_P> GetAll(float m_contprice, string e_orderdcflag, string drug_type)
        {
            var p = new DynamicParameters();

            string sql = string.Format(@"select distinct a.mmcode, z.mmname_c, z.mmname_e, z.base_unit,
                                                z.m_purun, z.m_contprice, z.e_takekind
                                           from MI_WINVCTL a, MI_MAST z {0}
                                          where a.mmcode = z.mmcode
                                            and z.e_invflag = 'Y'
                                            and z.mat_class = '01'", drug_type == "4" ? ",HIS_BASORDM y" : string.Empty);
            sql += GetFilter(0, 0, 0, m_contprice, e_orderdcflag, drug_type);

            p.Add(":m_contprice", string.Format("{0}", m_contprice));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":drug_type", string.Format("{0}", drug_type));

            //return DBWork.Connection.Query<CHK_GRADE2_P>(sql,p, DBWork.Transaction);
            return DBWork.PagingQuery<CHK_GRADE2_P>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CHK_GRADE2_P> GetPreviousP(string chk_ym, float gap_t, float gap_price, float gap_p, float m_contprice, string e_orderdcflag, string drug_type, string wh_no)
        {
            var p = new DynamicParameters();
            string sql = string.Empty;
            if (wh_no != string.Empty)
            {
                sql = string.Format(@"with pre_chk_p as (
                                            select chk_ym 
                                              from (select distinct chk_ym from CHK_MAST
                                                    where chk_wh_kind = '0'
                                                      and chk_wh_grade = '2'
                                                      and chk_ym < :chk_ym
                                                      and chk_period = 'P'
                                                    order by chk_ym desc
                                                   )
                                            where rownum = 1
                                        )
                                         select a.wh_no, a.mmcode, z.mmname_c, z.mmname_e,
                                                a.base_unit,  a.store_qtyc,
                                                a.chk_qty1, a.chk_qty2, a.chk_qty3, 
                                                (CASE
                                                  WHEN a.status_tot = '1' THEN a.chk_qty1
                                                   WHEN a.status_tot = '2' THEN a.chk_qty2
                                                     WHEN a.status_tot = '3' THEN a.chk_qty3
                                                  ELSE 0
                                                 END) as chk_qty,
                                                a.status_tot, a.gap_t,
                                                (a.gap_t * e.avg_price ) as diff_amount,
                                                c.wh_name,
                                                e.avg_price as m_contprice
                                           from CHK_DETAILTOT a, CHK_MAST b, MI_WHMAST c, pre_chk_p d, MI_WHCOST e, MI_MAST z {0}
                                          where b.chk_wh_grade = '2'
                                            and b.chk_wh_kind = '0'
                                            and b.chk_wh_no = :wh_no
                                            and b.chk_ym = d.chk_ym
                                            and a.chk_no = b.chk_no
                                            and c.wh_no = b.chk_wh_no
                                            and e.data_ym = d.chk_ym
                                            and e.mmcode = a.mmcode
                                            and z.mmcode = a.mmcode"
                                      , drug_type == "4" ? ",HIS_BASORDM y" : string.Empty); // and a.gap_t > 0
            }
            else {
                sql = string.Format(@"with pre_chk_p as (
                                            select chk_ym 
                                              from (select distinct chk_ym from CHK_MAST
                                                     where chk_wh_kind = '0'
                                                       and chk_wh_grade = '2'
                                                       and chk_ym < :chk_ym
                                                       and chk_period = 'P'
                                                     order by chk_ym desc
                                                    )
                                            where rownum = 1
                                        )
                                      select a.mmcode, sum(a.store_qty) as store_qtyc, 
                                             sum((CASE
                                                  WHEN a.status_tot = '1' THEN a.chk_qty1
                                                   WHEN a.status_tot = '2' THEN a.chk_qty2
                                                     WHEN a.status_tot = '3' THEN a.chk_qty3
                                                  ELSE 0
                                                 END))    as chk_qty,
                                             sum(a.gap_t)  as gap_t,
                                             (sum(a.gap_t) * e.avg_price) as diff_amount,
                                             e.avg_price as m_contprice,
                                             a.base_unit, a.mmname_e, a.mmname_c
                                        from CHK_DETAILTOT a, CHK_MAST b, MI_WHMAST c, pre_chk_p d, MI_WHCOST e, MI_MAST z {0}
                                       where b.chk_wh_grade = '2'
                                         and b.chk_wh_kind = '0'
                                         and b.chk_ym = d.chk_ym
                                         and a.chk_no = b.chk_no
                                         and c.wh_no = b.chk_wh_no
                                         and e.data_ym = d.chk_ym
                                         and e.mmcode = a.mmcode
                                         and z.mmcode = a.mmcode
                                        ", drug_type == "4" ? ",HIS_BASORDM y" : string.Empty);
            }

            sql += GetFilter(gap_t, gap_price, gap_p, m_contprice, e_orderdcflag, drug_type);

            if (wh_no == string.Empty) {
                sql += "    group by a.mmcode, a.base_unit, a.mmname_e, a.mmname_c, e.avg_price";
            }

            sql = string.Format(@"
                    select * from (
                        {0}
                    )
                  ", sql);

            p.Add(":chk_ym", string.Format("{0}", chk_ym));
            p.Add(":gap_t", string.Format("{0}", gap_t));
            p.Add(":gap_price", string.Format("{0}", gap_price));
            p.Add(":gap_p", string.Format("{0}", gap_p));
            p.Add(":m_contprice", string.Format("{0}", m_contprice));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":drug_type", string.Format("{0}", drug_type));
            p.Add(":wh_no", string.Format("{0}", wh_no));

            return DBWork.PagingQuery<CHK_GRADE2_P>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CHK_GRADE2_P> GetPreviousS(string chk_ym, float gap_t, float gap_price, float gap_p, float m_contprice, string e_orderdcflag, string drug_type, string wh_no)
        {
            var p = new DynamicParameters();

            string sql = string.Empty;

            if (wh_no != string.Empty)
            {
                sql = string.Format(@"with pre_chk_p as (
                                            select chk_ym 
                                              from (select distinct chk_ym from CHK_MAST
                                                     where chk_wh_kind = '0'
                                                       and chk_wh_grade = '2'
                                                       and chk_ym < :chk_ym
                                                       and chk_period = 'S'
                                                     order by chk_ym desc
                                                    )
                                             where rownum = 1
                                         )
                                         select a.wh_no, a.mmcode, z.mmname_c, z.mmname_e,
                                                a.base_unit,  a.store_qtyc,
                                                a.chk_qty1, a.chk_qty2, a.chk_qty3, 
                                                (CASE
                                                  WHEN a.status_tot = '1' THEN a.chk_qty1
                                                   WHEN a.status_tot = '2' THEN a.chk_qty2
                                                     WHEN a.status_tot = '3' THEN a.chk_qty3
                                                  ELSE 0
                                                 END) as chk_qty,
                                                a.status_tot, a.gap_t,
                                                (a.gap_t * (select avg_price from MI_WHCOST where data_ym = d.chk_ym and mmcode = a.mmcode) ) as diff_amount,
                                                c.wh_name,
                                                (select avg_price from MI_WHCOST where data_ym = d.chk_ym and mmcode = a.mmcode) as m_contprice
                                           from CHK_DETAILTOT a, CHK_MAST b, MI_WHMAST c, pre_chk_p d, MI_MAST z {0}
                                          where b.chk_wh_grade = '2'
                                            and b.chk_wh_kind = '0'
                                            and b.chk_wh_no = :wh_no
                                            and b.chk_ym = d.chk_ym
                                            and a.chk_no = b.chk_no
                                            and c.wh_no = b.chk_wh_no
                                            and z.mmcode = a.mmcode", drug_type == "4" ? ",HIS_BASORDM y" : string.Empty);   // and a.gap_t > 0
            }
            else {
                sql = string.Format(@"with pre_chk_p as (
                                            select chk_ym 
                                              from (select distinct chk_ym from CHK_MAST
                                                     where chk_wh_kind = '0'
                                                       and chk_wh_grade = '2'
                                                       and chk_ym < :chk_ym
                                                       and chk_period = 'S'
                                                     order by chk_ym desc
                                                    )
                                             where rownum = 1
                                         )
                                         select a.wh_no, a.mmcode, z.mmname_c, z.mmname_e,
                                                a.base_unit,  a.store_qtyc,
                                                a.chk_qty1, a.chk_qty2, a.chk_qty3, 
                                                (CASE
                                                  WHEN a.status_tot = '1' THEN a.chk_qty1
                                                   WHEN a.status_tot = '2' THEN a.chk_qty2
                                                     WHEN a.status_tot = '3' THEN a.chk_qty3
                                                  ELSE 0
                                                 END) as chk_qty,
                                                a.status_tot, a.gap_t,
                                                (a.gap_t * (select avg_price from MI_WHCOST where data_ym = d.chk_ym and mmcode = a.mmcode) ) as diff_amount,
                                                c.wh_name,
                                                (select avg_price from MI_WHCOST where data_ym = d.chk_ym and mmcode = a.mmcode) as m_contprice
                                           from CHK_DETAILTOT a, CHK_MAST b, MI_WHMAST c, pre_chk_p d, MI_MAST z {0}
                                          where b.chk_wh_grade = '2'
                                            and b.chk_wh_kind = '0'
                                            and b.chk_ym = d.chk_ym
                                            and a.chk_no = b.chk_no
                                            and c.wh_no = b.chk_wh_no
                                            and z.mmcode = a.mmcode", drug_type == "4" ? ",HIS_BASORDM y" : string.Empty);   // and a.gap_t > 0
            }

            

            sql += GetFilter(gap_t, gap_price, gap_p, m_contprice, e_orderdcflag, drug_type);

            sql = string.Format(@"
                    select * from (
                        {0}
                    )
                  ", sql);

            p.Add(":chk_ym", string.Format("{0}", chk_ym));
            p.Add(":gap_t", string.Format("{0}", gap_t));
            p.Add(":gap_price", string.Format("{0}", gap_price));
            p.Add(":gap_p", string.Format("{0}", gap_p));
            p.Add(":m_contprice", string.Format("{0}", m_contprice));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":drug_type", string.Format("{0}", drug_type));
            p.Add(":wh_no", string.Format("{0}", wh_no));

            return DBWork.PagingQuery<CHK_GRADE2_P>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<string> GetExistChknos(string chk_ym)
        {
            string sql = @"select chk_no from CHK_MAST
                            where chk_wh_grade = '2'
                              and chk_wh_kind = '0'
                              and chk_ym = :chkym";
            return DBWork.Connection.Query<string>(sql, new { CHKYM = chk_ym }, DBWork.Transaction);
        }

        public int Insert(CHK_GRADE2_P item)
        {
            string sql = @"insert into CHK_GRADE2_P
                           values (:chk_ym, :mmcode, sysdate, :create_user, sysdate, :update_user, :update_ip)";
            return DBWork.Connection.Execute(sql, new
            {
                chk_ym = item.CHK_YM,
                mmcode = item.MMCODE,
                create_user = item.CREATE_USER,
                update_user = item.UPDATE_USER,
                update_ip = item.UPDATE_IP
            }, DBWork.Transaction);
        }

        public int Delete(CHK_GRADE2_P item)
        {
            string sql = @"delete from CHK_GRADE2_P
                            where chk_ym = :chk_ym and mmcode = :mmcode";
            return DBWork.Connection.Execute(sql, new
            {
                CHK_YM = item.CHK_YM,
                MMCODE = item.MMCODE
            }, DBWork.Transaction);
        }

        public bool Exists(CHK_GRADE2_P item)
        {
            string sql = @"select 1 from CHK_GRADE2_P
                            where chk_ym = :CHK_YM
                              and mmcode = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, item, DBWork.Transaction) == null);
        }


        public IEnumerable<ComboItemModel> GetWhnos() {
            string sql = @"select wh_no as value, (wh_no || ' ' || wh_name) as text
                             from MI_WHMAST
                            where wh_grade = '2'
                              and wh_kind = '0'
                            order by wh_no";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<CHK_MAST> GetChkMasts(string chk_ym, string chk_period) {
            string sql = string.Format(@"select wh_no as chk_wh_no, wh_kind as chk_wh_kind,  
                                                wh_grade as chk_wh_grade, '{0}' as chk_ym, 
                                                '{1}' as chk_period, '01' as chk_class, 'X' as chk_type, '1' as chk_level,
                                                '0' as chk_status, 'CE0015' as create_user
                                           from MI_WHMAST
                                          where wh_grade = '2'
                                            and wh_kind = '0'
                                            and wh_no <> 'TRIALPH'
                                          order by wh_no", chk_ym, chk_period);
            return DBWork.Connection.Query<CHK_MAST>(sql, DBWork.Transaction);
        }
        public IEnumerable<CHK_DETAIL> GetMedItems(string wh_no)
        {
            string sql = @"select a.wh_no, a.mmcode, c.mmname_c, c.mmname_e,
                                  c.base_unit,
                                  (select inv_qty from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode) as inv_qty, 
                                  0 as chk_qty,
                                  '' as store_loc, '' as chk_uid, '0 開立' as status
                             from MI_WINVCTL a, MI_MAST c
                            where a.wh_no = :wh_no
                              and c.mmcode = a.mmcode
                              and c.e_invflag = 'Y'
                              and c.e_orderdcflag = 'N'
                              and a.ctdmdccode <> '1'
                            order by mmcode";

            return DBWork.Connection.Query<CHK_DETAIL>(sql, new { wh_no = wh_no }, DBWork.Transaction);
        }

        public IEnumerable<string> GetExistChknosS()
        {
            string sql = @"select chk_no from CHK_MAST
                            where chk_wh_grade = '2'
                              and chk_wh_kind = '0'
                              and chk_ym = (select set_ym from MI_MNSET where set_status = 'N')";
            return DBWork.Connection.Query<string>(sql, DBWork.Transaction);
        }

        #region
        public int InsertChkG2Whinv(CHK_G2_WHINV whinv)
        {
            string sql = @"insert into CHK_G2_WHINV(chk_no, wh_no, mmcode, store_qty, chk_pre_date,
                                                    create_date, create_user, update_time, update_user, update_ip,
                                                    seq) 
                           values (
                                :chk_no, :wh_no, :mmcode, '', to_date(:chk_pre_date, 'YYYY-MM-DD'), 
                                sysdate, :create_user, sysdate, :update_user, :update_ip, 
                                :seq
                           )";
            return DBWork.Connection.Execute(sql, whinv, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetChkGrade2Ps(string wh_no, string chk_ym)
        {
            string sql = @"select a.wh_no, a.mmcode, c.mmname_c, c.mmname_e,
                                  c.base_unit, 
                                  (select inv_qty from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode) as inv_qty, 
                                  0 as chk_qty,
                                  '' as store_loc, '' as chk_uid, '0 開立' as status
                             from MI_WINVCTL a, CHK_GRADE2_P b, MI_MAST c
                            where a.wh_no = :wh_no
                              and a.ctdmdccode <> '1'
                              and b.mmcode = a.mmcode
                              and b.chk_ym = :chk_ym
                              and c.mmcode = a.mmcode
                            order by a.mmcode";
            return DBWork.Connection.Query<CHK_DETAIL>(sql, new { wh_no = wh_no, chk_ym = chk_ym }, DBWork.Transaction);
        }
        #endregion

        #region 2021-12-12
        public class MNSET {
            public string PYM { get; set; }
            public string SET_YM { get; set; }
            public string NYM { get; set; }
        }

        public IEnumerable<MNSET> GetMnSet() {
            string sql = @"
                  select twn_pym(a.set_ym) as pym, a.set_ym,
                        SUBSTR(TWN_DATE(ADD_MONTHS(TWN_TODATE(a.set_ym||'01'),1)),1,5) as nym
                   from (
                         select * from MI_MNSET
                          where set_status = 'N'
                          order by set_ym desc
                        ) a
            ";
            return DBWork.Connection.Query<MNSET>(sql, DBWork.Transaction);
        }

        #endregion
    }
}