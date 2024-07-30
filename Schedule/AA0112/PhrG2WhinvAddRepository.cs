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
    public class PhrG2WhinvAddRepository : JCLib.Mvc.BaseRepository
    {
        public PhrG2WhinvAddRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public string GetMnSet()
        {
            string sql = @"select set_ym from MI_MNSET 
                            where set_status = 'N'";
            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        public IEnumerable<CHK_MAST> GetChknos(string chk_ym) {
            string sql = @"
                select a.chk_no, a.chk_wh_no, a.chk_period,
                       (select count(*) from CHK_G2_WHINV where chk_no = a.chk_no) chk_total
                  from CHK_MAST a
                 where a.chk_wh_kind = '0'
                   and a.chk_wh_grade = '2'
                   and a.chk_level = '1'
                   and a.chk_status = '0'
                   and a.chk_ym = :chk_ym
            ";
            return DBWork.Connection.Query<CHK_MAST>(sql, new { chk_ym },DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetSItems(string wh_no, string chk_no)
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
                              and not exists (select 1 from CHK_G2_WHINV where chk_no = :chk_no and mmcode = a.mmcode)
                            order by mmcode";

            return DBWork.Connection.Query<CHK_DETAIL>(sql, new { wh_no, chk_no }, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetPItems(string chk_ym, string wh_no, string chk_no) {
            string sql = @"
                select a.wh_no, a.mmcode, c.mmname_c, c.mmname_e,
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
                   and not exists (select 1 from CHK_G2_WHINV where chk_no = :chk_no and mmcode = a.mmcode)
                 order by a.mmcode
            ";
            return DBWork.Connection.Query<CHK_DETAIL>(sql, new { chk_ym, wh_no, chk_no }, DBWork.Transaction);
        }
        public string GetChkPreDate(string chk_no) {
            string sql = @"
                select to_char(min(chk_pre_date), 'yyyy-mm-dd')
                  from CHK_G2_WHINV
                 where chk_no = :chk_no
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no }, DBWork.Transaction);
        }

        public string GetSeq(string chk_no) {
            string sql = @"
                select max(seq)+1 from CHK_G2_WHINV
                 where chk_no = :chk_no
            ";
            return DBWork.Connection.QueryFirst<string>(sql, new { chk_no }, DBWork.Transaction);
        }

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
    }
}
