using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;

namespace WebApp.Controllers.AA
{
    public class AA0137Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 庫房
            var p1 = form.Get("p1"); // 院內碼
            var p2 = form.Get("p2"); // 中文品名
            var p3 = form.Get("p3"); // 英文品名
            var p4 = form.Get("p4"); // 管制用藥
            var ctdmdccodes = form.Get("ctdmdccodes"); //各庫停用碼
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0137Repository(DBWork);
                    session.Result.etts = repo.GetAll(User.Identity.Name, p0, p1, p2, p3, p4, ctdmdccodes, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0137Repository repo = new AA0137Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(User.Identity.Name, p0, p1, p2, p3, p4));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWhCombo(FormDataCollection form)
        {
            var user_kind = form.Get("p0");
            using (WorkSession session = new WorkSession(this))
            {

                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0137Repository repo = new AA0137Repository(DBWork);
                    session.Result.etts = repo.GetWhCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmcodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0137Repository repo = new AA0137Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo(p0, page, limit, "");
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