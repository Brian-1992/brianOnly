using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0071Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
            var p1 = "650000";
            var p2 = form.Get("p1"); ;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0071Repository repo = new AA0071Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo(p0, p1, p2, page, limit, "");

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");
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
                    var repo = new AA0071Repository(DBWork);

                    session.Result.etts = repo.GetAll(d0, d1, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public void Excel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0071Repository(DBWork);

                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(form.Get("d0"), form.Get("d1"), form.Get("p1"), form.Get("p2")));
                }
                catch
                {
                    throw;
                }
            }
        }
        
    }
}