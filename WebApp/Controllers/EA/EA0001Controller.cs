using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.EA;

namespace WebApp.Controllers.EA
{
    public class EA0001Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetChartData(FormDataCollection form)
        {
            var p0 = form.Get("WH_NO");
            var p1 = int.Parse(form.Get("PERG"));

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new EA0001Repository(DBWork);
                    session.Result.etts = repo.GetChartData(p0, p1);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTableData(FormDataCollection form)
        {
            var p0 = form.Get("WH_NO");
            var p1 = int.Parse(form.Get("MAT_CLSID"));
            var p2 = int.Parse(form.Get("PERG"));
            var p3 = int.Parse(form.Get("PERG_TYPE"));

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new EA0001Repository(DBWork);
                    session.Result.etts = repo.GetTableData(p0, p1, p2, p3);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWhnos() {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new EA0001Repository(DBWork);
                    session.Result.etts = repo.GetWhnos();

                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }
    }
}