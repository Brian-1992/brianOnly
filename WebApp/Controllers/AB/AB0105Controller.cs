using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AB
{
    public class AB0105Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form) {
            string data_ym_s = form.Get("data_ym_s") == null ? string.Empty : form.Get("data_ym_s");
            string data_ym_e = form.Get("data_ym_e") == null ? string.Empty : form.Get("data_ym_e");
            string towh = form.Get("towh") == null ? string.Empty : form.Get("towh");
            string mmcode = form.Get("mmcode") == null ? string.Empty : form.Get("mmcode");

            using (WorkSession session = new WorkSession(this)) {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0105Repository repo = new AB0105Repository(DBWork);
                    session.Result.etts = repo.GetAll(data_ym_s, data_ym_e, towh, mmcode);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }



        #region combo

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var wh_no = form.Get("wh_no");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0105Repository repo = new AB0105Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetTowhCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0105Repository repo = new AB0105Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #endregion

    }
}