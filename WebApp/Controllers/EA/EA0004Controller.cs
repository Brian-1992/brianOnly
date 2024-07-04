using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.EA;

namespace WebApp.Controllers.EA
{
    public class EA0004Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetSeriesData(FormDataCollection form)
        {
            var p0 = form.Get("INID");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new EA0004Repository(DBWork);
                    session.Result.etts = repo.GetSeriesData(p0);
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