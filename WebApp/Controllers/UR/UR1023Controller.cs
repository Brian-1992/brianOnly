using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;
using Newtonsoft.Json;

namespace WebApp.Controllers.UR
{
    public class UR1023Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetUsers(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //TACL_CREATE_BY Limit
            var p1 = form.Get("p1"); //TUSER
            var p2 = form.Get("p2"); //UNA
            var p3 = form.Get("p3"); //INID
            var p4 = form.Get("p4"); //INID Limit
            var p5 = form.Get("p5"); // ADUSER

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_TACL2Repository(DBWork);
                    session.Result.etts = repo.GetUsers(p0, p1, p2, p3, p4, p5);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet, HttpPost]
        public ApiResponse GetMenu(FormDataCollection f)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var pg = f.Get("PG");
                    var tuser = f.Get("p0");
                    var manager = f.Get("p1");

                    var repo = new UR_MENURepository(DBWork);
                    session.Result.etts = repo.GetMenuByUser2(pg, tuser, manager);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Update(FormDataCollection f)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var tuser = f.Get("TUSER");
                    var manager = f.Get("MGR");

                    var repo = new UR_TACL2Repository(DBWork);
                    dynamic obj = JsonConvert.DeserializeObject(f.Get("JSON"));
                    session.Result.afrs = repo.Update(tuser, manager, JsonConvert.DeserializeObject<UR_TACL2[]>(obj.T1.ToString()));

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
    }
}
