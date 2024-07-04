using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;

namespace WebApp.Controllers.AB
{
    public class AB0083Controller : ApiController
    {

        
        [HttpPost]
        public ApiResponse GetKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0083Repository repo = new AB0083Repository(DBWork);
                    session.Result.etts = repo.GetKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWhmastCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0083Repository repo = new AB0083Repository(DBWork);
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