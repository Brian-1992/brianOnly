using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;

namespace WebApp.Controllers.F
{
    public class FA0072Controller : ApiController
    {
        
        [HttpPost]
        public ApiResponse GetWhmastCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0072Repository repo = new FA0072Repository(DBWork);
                    session.Result.etts = repo.GetWhmastCombo();
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