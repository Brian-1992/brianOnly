using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;

namespace WebApp.Controllers.AB
{
    public class AB0065Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var limit = int.Parse(form.Get("limit"));

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0065Repository repo = new AB0065Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse TRCODEGet(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0065Repository repo = new AB0065Repository(DBWork);
                    session.Result.etts = repo.TRCODEGet();
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