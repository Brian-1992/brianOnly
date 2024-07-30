using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebAppVen.Repository.UR;

namespace SiteBase.Controllers
{
    public class MenuController : ApiController
    {
        public ApiResponse Main(FormDataCollection f)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_MENURepository(DBWork);
                    var parent = (f.Get("PG") == "root") ? "" : f.Get("PG");
                    var expanded = (f.Get("PG") == "root") ? true : false;
                    session.Result.etts = repo.GetMenuIndex(parent, User.Identity.Name, "YS", expanded);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse Mobile(FormDataCollection f)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_MENURepository(DBWork);
                    var parent = (f.Get("PG") == "root") ? "" : f.Get("PG");
                    var expanded = (f.Get("PG") == "root") ? true : false;
                    session.Result.etts = repo.GetMenuIndex(parent, User.Identity.Name, "MOBILE", expanded);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse Query(FormDataCollection f)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_MENURepository(DBWork);
                    session.Result.etts = repo.GetMenuByQuery(f.Get("MenuName"), User.Identity.Name);
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
