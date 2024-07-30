using System.Net.Http.Formatting;
using System.Web.Http;
using System.Net.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Diagnostics;
namespace WebApp.Controllers.AA

{
    public class AA0065Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {

            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = "";// form.Get("p4");
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
                    var repo = new AA0065Repository(DBWork);
                    session.Result.etts = repo.GetAll(p1, p2, p3, p4, p5, User.Identity.Name, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public void GetExcel(FormDataCollection form)
        {
            var p1 = form.Get("p1"); // 日期
            var p2 = form.Get("p2"); // 日期
            var p3 = form.Get("p3"); // 庫房代碼
            var p4 = "";// form.Get("p4"); // 庫房級別
            var p5 = form.Get("p5"); // 藥品類別
            var hospinfo = "";
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0065Repository repo = new AA0065Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p1, p2, p3, p4, p5, User.Identity.Name));

                    //JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4, p5));
                    //session.Result.etts = repo.GetExcel(p0, p1, p2, p3, p4,p5);
                }
                catch
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0065Repository repo = new AA0065Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, p1, page, limit, "");
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