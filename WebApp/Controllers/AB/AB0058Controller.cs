using System.Net.Http.Formatting;
using System.Web.Http;
using System.Net.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System.Diagnostics;

namespace WebApp.Controllers.AB
{
    public class AB0058Controller : SiteBase.BaseApiController
    {
        // GET api/<controller>
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {

            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0058Repository(DBWork);
                    session.Result.etts = repo.GetAll(p1, p2, p3, p4, p5, page, limit, sorters, User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetExcel(FormDataCollection form)
        {
            var d0 = form.Get("d0"); //1080605
            var d1 = form.Get("d1");
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            var d0Year = int.Parse(d0.Substring(0, 3)) + 1911;
            var newd0 = d0Year + "-" + d0.Substring(3, 2) + "-" + d0.Substring(5, 2); //yyyy-mm-dd

            var d1Year = int.Parse(d1.Substring(0, 3)) + 1911;
            var newd1 = d1Year + "-" + d1.Substring(3, 2) + "-" + d1.Substring(5, 2); //yyyy-mm-dd
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0058Repository repo = new AB0058Repository(DBWork);

                    JCLib.Excel.Export("出貨明細表" + ".xls", repo.GetExcel(newd0, newd1, p0, p1, p2));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0058Repository repo = new AB0058Repository(DBWork);
                    session.Result.etts = repo.GetMatClass(User.Identity.Name);
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