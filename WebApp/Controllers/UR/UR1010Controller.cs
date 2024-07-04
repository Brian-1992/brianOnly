using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using TSGH.Repository.UR;

namespace WebApp.Controllers.UR
{
    public class UR1010Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new TAB_LOGRepository(DBWork);
                    session.Result.etts = repo.Query(page, limit, sorters, p0, p1,p2);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTabLogRefCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new TAB_LOGRepository(DBWork);
                    session.Result.etts = repo.GetTabLogRefCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFieldNameCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); 
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new TAB_LOGRepository(DBWork);
                    session.Result.etts = repo.GetFieldNameCombo(p0);
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