using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;

namespace WebApp.Controllers.UR
{
    public class UR1022Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse Query(UR_LOGIN ur_login)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_LOGINRepository repo = new UR_LOGINRepository(DBWork);
                    //session.Result.etts = repo.Query(p0, p1, p2);
                    session.Result.etts = repo.Query(ur_login.UNA, ur_login.LOGIN_DATE_B, ur_login.LOGIN_DATE_E, ur_login.USER_IP);
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
