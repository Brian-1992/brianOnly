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
    public class AA0044Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            p1 = "";
            var p2 = form.Get("p2");
            p2 = "";
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var menuLink = form.Get("menuLink");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0044Repository(DBWork);
                    var v_st = repo.GetMnset(p0);
                    if(v_st == "N")
                    {
                        session.Result.etts = repo.GetAllMN(User.Identity.Name, p0, p1, p2, p3, p4, p5, p6, menuLink, page, limit, sorters);
                    }
                    else {
                        session.Result.etts = repo.GetAllML(User.Identity.Name, p0, p1, p2, p3, p4, p5, p6, menuLink, page, limit, sorters);
                    }
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0044Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        
        [HttpPost]
        public ApiResponse GetSetymCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0044Repository repo = new AA0044Repository(DBWork);
                    
                    session.Result.etts = repo.GetSetymCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWhkindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0044Repository repo = new AA0044Repository(DBWork);

                    session.Result.etts = repo.GetWhkindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWhgradeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0044Repository repo = new AA0044Repository(DBWork);
                    if (p0 == "0")
                    {
                        session.Result.etts = repo.GetWhgradeCombo();
                    }
                    else
                    {
                        session.Result.etts = repo.GetWhgrade1Combo();
                    }
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
            var wh_kind = form.Get("wh_kind");
            var wh_grade = form.Get("wh_grade");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0044Repository repo = new AA0044Repository(DBWork);
                    bool isAuth = DBWork.UserInfo.VIEWALL;
                    session.Result.etts = repo.GetWhnoCombo(wh_kind, wh_grade, p0, isAuth, User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetStoreidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0044Repository repo = new AA0044Repository(DBWork);
                    session.Result.etts = repo.GetStoreidCombo();
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
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0044Repository repo = new AA0044Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, p2, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeDocd(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var mat_class = form.Get("mat_class");
            var m_stroeid = form.Get("m_stroeid");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0044Repository repo = new AA0044Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeDocd(p0, mat_class, m_stroeid, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0044Repository repo = new AA0044Repository(DBWork);
                    var taskid = repo.GetTaskid(User.Identity.Name);
                    if (taskid == "2")
                    {
                        session.Result.etts = repo.GetMatclass2Combo();
                    }
                    else if (taskid == "3")
                    {
                        session.Result.etts = repo.GetMatclass3Combo();
                    }
                    else
                    {
                        session.Result.etts = repo.GetMatclass1Combo();
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        /*
        [HttpPost]
        public ApiResponse GetYN()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0044Repository repo = new AA0044Repository(DBWork);
                    session.Result.etts = repo.GetYN();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWhGrade()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0044Repository repo = new AA0044Repository(DBWork);
                    session.Result.etts = repo.GetWhGrade();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWhKind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0044Repository repo = new AA0044Repository(DBWork);
                    session.Result.etts = repo.GetWhKind();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        */

        [HttpPost]
        public ApiResponse GetWhCombo(FormDataCollection form)
        {
            var user_kind = form.Get("p0");
            var menuLink = form.Get("menuLink");
            using (WorkSession session = new WorkSession(this))
            {

                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    bool isAuth = DBWork.UserInfo.VIEWALL;
                    AA0044Repository repo = new AA0044Repository(DBWork);
                    if (user_kind == "")
                        user_kind = repo.GetUserKind(User.Identity.Name);
                    session.Result.etts = repo.GetWhCombo(isAuth, User.Identity.Name, user_kind, menuLink);
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