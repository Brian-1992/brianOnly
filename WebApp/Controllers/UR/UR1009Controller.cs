using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;
using Newtonsoft.Json;

namespace WebApp.Controllers.UR
{
    public class UR1009Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_RoleRepository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1);
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
                    var repo = new UR_MENURepository(DBWork);
                    session.Result.etts = repo.GetMenuByRole(f.Get("PG"), f.Get("p0"));
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
                    var repo = new UR_TACLRepository(DBWork);
                    dynamic obj = JsonConvert.DeserializeObject(f.Get("JSON"));
                    session.Result.afrs = repo.Update(f.Get("RLNO"), JsonConvert.DeserializeObject<UR_TACL[]>(obj.T1.ToString()));

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
