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
    public class CE0011Controller : SiteBase.BaseApiController
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
                    var repo = new CE0011Repository(DBWork);
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

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0011Repository(DBWork);

                    session.Result.etts = repo.GetIncludeDetails(chk_no, chk_level, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

    }
}