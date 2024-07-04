using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;
using Newtonsoft.Json;

namespace WebApp.Controllers.UR
{
    public class UR1019Controller : SiteBase.BaseApiController
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
                    session.Result.etts = repo.GetByUser(p0, p1, DBWork.UserInfo.UserId);
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
            var pg = f.Get("PG");
            var p0 = f.Get("p0");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_MENURepository(DBWork);
                    session.Result.etts = repo.GetMenuByRoleAndUser(pg, p0, DBWork.UserInfo.UserId);
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
