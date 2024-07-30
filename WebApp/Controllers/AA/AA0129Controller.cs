using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WebApp.Controllers.AA
{
    public class AA0129Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var ctdmdccodes = form.Get("ctdmdccodes");
            var user_kind = form.Get("user_kind");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0129Repository(DBWork);
                    bool isAuth = DBWork.UserInfo.VIEWALL;
                    if (user_kind == "" & isAuth == false)
                        user_kind = repo.GetUserKind(User.Identity.Name);
                    session.Result.etts = repo.GetAll(isAuth,user_kind, User.Identity.Name, p0, p1, p2, p3, p4, ctdmdccodes, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // AA0137的showDetail會直接引用這邊
        [HttpPost]
        public ApiResponse AllD1(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0129Repository(DBWork);
                    session.Result.etts = repo.GetAllD1(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse AllD2(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0129Repository(DBWork);
                    session.Result.etts = repo.GetAllD2(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse AllD3(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0129Repository(DBWork);
                    session.Result.etts = repo.GetAllD3(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMAT_CLASSCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var user_kind = form.Get("user_kind");
            using (WorkSession session = new WorkSession(this))
            {

                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0129Repository repo = new AA0129Repository(DBWork);
                    bool isAuth = DBWork.UserInfo.VIEWALL;

                    if (user_kind == "" & isAuth == false)
                        user_kind = repo.GetUserKind(User.Identity.Name);
                    session.Result.etts = repo.GetMAT_CLASSCombo(isAuth,p0, user_kind);
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
        [HttpPost]
        public string GetUserkind()
        {
            string user_kind = "";
            using (WorkSession session = new WorkSession())
            {

                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0129Repository repo = new AA0129Repository(DBWork);
                    user_kind = repo.GetUserKind(User.Identity.Name);
                    return user_kind;
                    //session.Result.etts = repo.GetWhCombo(User.Identity.Name, user_kind);
                }
                catch
                {
                    //throw;
                }

            }
            return user_kind;
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
                    bool isAuth = DBWork.UserInfo.VIEWALL;
                    AA0129Repository repo = new AA0129Repository(DBWork);
                    if (user_kind == "")
                        user_kind = repo.GetUserKind(User.Identity.Name);
                    session.Result.etts = repo.GetWhCombo(isAuth,User.Identity.Name, user_kind);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpGet]
        public ApiResponse GetCtdmdccodeCombo() {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0129Repository(DBWork);
                    session.Result.etts = repo.GetCtdmdccodes();
                }
                catch {
                    throw;
                }
                return session.Result;
            }
        }
    }
}