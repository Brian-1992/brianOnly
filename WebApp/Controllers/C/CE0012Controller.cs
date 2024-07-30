using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.C;

namespace WebApp.Controllers.C
{
    public class CE0012Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {

            var wh_no = form.Get("p0");
            var chk_ym = form.Get("p1");
            var keeper = form.Get("p2");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0012Repository(DBWork);
                    IEnumerable<CHK_MAST> masters = repo.GetMasterAll(wh_no, chk_ym, keeper, page, limit, sorters);
                    foreach (CHK_MAST master in masters)
                    {
                        master.CHK_TYPE_NAME = repo.GetChkWhkindName(master.CHK_WH_KIND, master.CHK_TYPE);
                    }
                    session.Result.etts = masters;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDetails(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            var chk_level = form.Get("chk_level");
            var condition = form.Get("condition");
            var wh_kind = form.Get("wh_kind");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0012Repository(DBWork);

                    session.Result.etts = repo.GetDetails(chk_no, chk_level, wh_kind, condition, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public IEnumerable<CE0013Count> GetCountItem(CE0013Count all, CE0013Count p, CE0013Count n)
        {
            List<CE0013Count> list = new List<CE0013Count>();
            CE0013Count item = new CE0013Count()
            {
                TOT1 = all.TOT1,
                TOT2 = all.TOT2,
                TOT3 = all.TOT3,
                TOT4 = all.TOT4,
                TOT5 = all.TOT5,
                P_TOT1 = p.P_TOT1,
                P_TOT2 = p.P_TOT2,
                P_TOT3 = p.P_TOT3,
                P_TOT4 = p.P_TOT4,
                P_TOT5 = p.P_TOT5,
                N_TOT1 = n.N_TOT1,
                N_TOT2 = n.N_TOT2,
                N_TOT3 = n.N_TOT3,
                N_TOT4 = n.N_TOT4,
                N_TOT5 = n.N_TOT5
            };
            list.Add(item);
            return list;
        }
    }
}