using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System.Data;
using System;
using System.Text;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Linq;


namespace WebApp.Controllers.F
{
    public class FA0079Controller : SiteBase.BaseApiController
    {

        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9") == null ? "" : form.Get("p9");
            var isab = form.Get("isab");
            var user = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            var YYYMM = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0079Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, YYYMM, isab, user, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9") == null ? "" : form.Get("p9");
            var isab = form.Get("isab");
            var user = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            var YYYMM = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0079Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, YYYMM, isab, user, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWhmastCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var isab = form.Get("isab");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            var user = User.Identity.Name;

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0079Repository repo = new FA0079Repository(DBWork);
                    session.Result.etts = repo
                        .GetWhmastCombo(p0, isab, user, page, limit, "")
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_GRADE = w.WH_GRADE });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
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
                    FA0079Repository repo = new FA0079Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMatClassSubCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0079Repository(DBWork);
                    session.Result.etts = repo.GetMatClassSubCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse GetESourceCodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0079Repository(DBWork);
                    session.Result.etts = repo.GetESourceCodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse GetMContidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0079Repository(DBWork);
                    session.Result.etts = repo.GetMContidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetOrderCodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0079Repository(DBWork);
                    session.Result.etts = repo.GetOrderCodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetTouchCaseCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0079Repository(DBWork);
                    session.Result.etts = repo.GetTouchCaseCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetERestrictcodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0079Repository(DBWork);
                    session.Result.etts = repo.GetERestrictcodeCombo();
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
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9") == null ? "" : form.Get("p9");
            var isab = form.Get("isab");
            var user = User.Identity.Name;
            var YYYMM = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0079Repository repo = new FA0079Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    JCLib.Excel.Export("存量查詢報表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, YYYMM, isab, user));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetExtraDiscAmout(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            var YYYMM = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0079Repository repo = new FA0079Repository(DBWork);
                    session.Result.msg = repo.GetExtraDiscAmout(p0, YYYMM);
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