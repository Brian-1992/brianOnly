using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.F
{
    public class FA0004Repository : JCLib.Mvc.BaseRepository
    {
        public FA0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0004M> GetAll(string chk_ym, string m_storeid, string mat_class) {
            var p = new DynamicParameters();

            string sql = string.Format(@"select c.mmcode as mmcode,
                                  c.mmname_c as mmname_c,
                                  c.mmname_e as mmname_e,
                                  b.store_loc as store_loc,
                                  c.chk_remark as chk_remark,
                                  c.status_tot as status_tot,
                                  c.chk_qty1 as chk_qty1,
                                  c.chk_qty2 as chk_qty2,
                                  c.chk_qty3 as chk_qty3,
                                  c.apl_outqty as apl_outqty,
                                  c.store_qty as store_qty,
                                  c.m_contprice as m_contprice
                             from chk_mast a, chk_detail b, chk_detailtot c
                            where a.chk_wh_no = '560000'
                              and a.chk_status = '3'
                              and a.chk_ym = '{0}'
                              and b.chk_no = a.chk_no
                              and (c.chk_no = a.chk_no or c.chk_no = a.chk_no)
                              and c.mmcode = b.mmcode
                              and c.m_storeid = '{1}'", chk_ym, m_storeid);
            if (mat_class != string.Empty) {
                sql += string.Format("  and c.mat_class = {0}", mat_class);
            }

            sql += "   order by c.mmcode ASC, c.status_tot DESC";

            return DBWork.PagingQuery<FA0004M>(sql, DBWork.Transaction);
        }

        public IEnumerable<FA0004M> GetExcel(string chk_ym, string m_storeid, string mat_class) {
            string sql = string.Format(@"select c.mmcode as mmcode,
                                  c.mmname_c as mmname_c,
                                  c.mmname_e as mmname_e,
                                  b.store_loc as store_loc,
                                  c.chk_remark as chk_remark,
                                  c.status_tot as status_tot,
                                  c.chk_qty1 as chk_qty1,
                                  c.chk_qty2 as chk_qty2,
                                  c.chk_qty3 as chk_qty3,
                                  c.apl_outqty as apl_outqty,
                                  c.store_qty as store_qty,
                                  c.m_contprice as m_contprice
                             from chk_mast a, chk_detail b, chk_detailtot c
                            where a.chk_wh_no = '560000'
                              and a.chk_status = '3'
                              and a.chk_ym = '{0}'
                              and b.chk_no = a.chk_no
                              and (c.chk_no = a.chk_no or c.chk_no = a.chk_no)
                              and c.mmcode = b.mmcode
                              and c.m_storeid = '{1}'", chk_ym, m_storeid);
            if (mat_class != string.Empty)
            {
                sql += string.Format("  and c.mat_class = {0}", mat_class);
            }

            sql += "   order by c.mmcode ASC, c.status_tot DESC";

            return DBWork.Connection.Query<FA0004M>(sql, DBWork.Transaction);
        }

        public IEnumerable<FA0004M> Print(string chk_ym, string m_storeid, string mat_class, string userID) {
            string sql = string.Format(@"select c.mmcode as mmcode,
                                  c.mmname_c as mmname_c,
                                  c.mmname_e as mmname_e,
                                  b.store_loc as store_loc,
                                  c.chk_remark as chk_remark,
                                  c.status_tot as status_tot,
                                  c.chk_qty1 as chk_qty1,
                                  c.chk_qty2 as chk_qty2,
                                  c.chk_qty3 as chk_qty3,
                                  c.apl_outqty as apl_outqty,
                                  c.store_qty as store_qty,
                                  c.m_contprice as m_contprice,
                                  (select tuser || ' ' || una from UR_ID where tuser = '{2}') as printuser
                             from chk_mast a, chk_detail b, chk_detailtot c 
                            where a.chk_wh_no = '560000'
                              and a.chk_status = '3'
                              and a.chk_ym = '{0}'
                              and b.chk_no = a.chk_no
                              and (c.chk_no = a.chk_no or c.chk_no = a.chk_no)
                              and c.mmcode = b.mmcode
                              and c.m_storeid = '{1}'", chk_ym, m_storeid, userID);
            if (mat_class != string.Empty)
            {
                sql += string.Format("  and c.mat_class = {0}", mat_class);
            }

            sql += "   order by c.mmcode ASC, c.status_tot DESC";

            return DBWork.Connection.Query<FA0004M>(sql, DBWork.Transaction);

        }

        #region combo
        public IEnumerable<ComboItemModel> GetMatclassCombo()
        {
            string sql = @"select mat_class as value, mat_class || ' ' || mat_clsname as text
                             from MI_MATCLASS
                            where mat_class in ('02','03','04','05','06','07','08','09','13')
                            order by mat_class";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);

        }
        #endregion
    }
}