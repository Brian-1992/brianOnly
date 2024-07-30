using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository
{
    public class WebApiRepository : JCLib.Mvc.BaseRepository
    {
        public WebApiRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public WhMmInvqty GetInvqty(MI_WINVCTL item) {

            DynamicParameters p = new DynamicParameters();
            string sql = @"select a.wh_no, a.mmcode, a.inv_qty as total_inv_qty,
                                  b.wh_name as wh_name
                             from MI_WHINV a, MI_WHMAST b
                            where 1=1
                              and a.wh_no = :wh_no
                              and a.mmcode = :mmcode
                              and b.wh_no = a.wh_no";

            p.Add("wh_no", item.WH_NO);
            p.Add("mmcode", item.MMCODE);

            return DBWork.Connection.QueryFirstOrDefault<WhMmInvqty>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<LotExpInv> GetWexpInv(WhMmInvqty item) {
            DynamicParameters p = new DynamicParameters();
            string sql = @"select lot_no, exp_date, inv_qty
                             from MI_WEXPINV
                            where wh_no = :wh_no
                              and mmcode = :mmcode";

            p.Add("wh_no", item.WH_NO);
            p.Add("mmcode", item.MMCODE);

            return DBWork.Connection.Query<LotExpInv>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MmcodeInvqty> GetMmcodeInvqty(string mmcode) {
            string sql = @"select mmcode, wh_no, inv_qty
                             from MI_WHINV
                            where mmcode = :mmcode
                              and inv_qty <> 0";
            return DBWork.Connection.Query<MmcodeInvqty>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }
    }
}