using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BG;
using WebApp.Models;

namespace WebApp.Controllers.BG
{
    public class BG0003Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0").Substring(0, 10).Replace("-", "/");
            var p1 = form.Get("p1").Substring(0, 10).Replace("-", "/");
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
                    var repo = new BG0003Repository(DBWork);
                    if ((p0 != "") && (p1 != "") && (p2 != ""))
                    {
                        session.Result.etts = repo.GetAll(p0, p1, p2, page, limit, sorters);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>查詢日期及分類不可為空</span>，請重新輸入。";
                    }

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0003Repository repo = new BG0003Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2));
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