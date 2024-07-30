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
    public class FA0080Controller : SiteBase.BaseApiController
    {

        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var menulink = form.Get("menulink");
            var user = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0080Repository(DBWork);
                    if (menulink == "AB0148" || menulink == "AB0150" || menulink == "AB0154")
                    {
                        if (repo.CheckIs01MmCode(p5))
                        {
                            session.Result.success = false;
                            session.Result.msg = string.Format("此院內碼非藥材，請重新查詢!");
                            return session.Result;
                        }
                    }
                    if (menulink == "AB0147") {
                        if (repo.CheckIsRestrictcode(p5)) {
                            session.Result.success = false;
                            session.Result.msg = "此院內碼非管制藥品，請重新查詢";
                            return session.Result;
                        }
                    }

                    session.Result.etts = repo.GetAllM(p0, p1, p4, p5, p6, Convert.ToBoolean(p7) == true ? "Y" : "N", menulink, user, page, limit, sorters);
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
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var menulink = form.Get("menulink");
            var user = User.Identity.Name;
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0080Repository repo = new FA0080Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    JCLib.Excel.Export("存量查詢報表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(p0, p1, p4, p5, p6, Convert.ToBoolean(p7) == true ? "Y" : "N", menulink, user));
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
                    var repo = new FA0080Repository(DBWork);
                    session.Result.etts = repo.GetMatClassSubCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetCommonCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0080Repository(DBWork);
                    session.Result.etts = repo.GetCommonCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWhnoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var menulink = form.Get("menulink");
            var user = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0080Repository repo = new FA0080Repository(DBWork);
                    session.Result.etts = repo
                        .GetWhnoCombo(p0, menulink, user, page, limit, "")
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
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p6 = form.Get("p6");
            var menulink = form.Get("menulink");
            var user = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0080Repository repo = new FA0080Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, p2, p3, p4, p6, menulink, user, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetERestrictCodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0080Repository repo = new FA0080Repository(DBWork);
                    session.Result.etts = repo.GetERestrictCodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetCursetym() {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;

                try
                {
                    var repo = new FA0080Repository(DBWork);
                    session.Result.msg = repo.GetCursetym();

                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }
        }
    }
}