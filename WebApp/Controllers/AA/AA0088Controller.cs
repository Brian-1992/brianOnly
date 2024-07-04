using System.Net.Http.Formatting;
using System.Web.Http;
using System.Net.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Diagnostics;
namespace WebApp.Controllers.AA
{
    public class AA0088Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {

            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0088Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetAll(p1, p2, page, limit, sorters, User.Identity.Name, hospCode=="0");
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