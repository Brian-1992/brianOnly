using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.EA;

namespace WebApp.Controllers.EA
{
    public class EA0003Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetSeriesData(FormDataCollection form)
        {
            var p0 = form.Get("INID");
            var p1 = form.Get("MMCODE");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new EA0003Repository(DBWork);
                    session.Result.etts = repo.GetSeriesData(p0, p1);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWhnos()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new EA0003Repository(DBWork);
                    session.Result.etts = repo.GetWhnos();

                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
    }
}