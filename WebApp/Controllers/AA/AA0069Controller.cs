using System.Net.Http.Formatting;
using System.Web.Http;
using System.Net.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Diagnostics;


namespace WebApp.Controllers.AA
{
    public class AA0069Controller : SiteBase.BaseApiController
    {
        

        [HttpGet]
        public ApiResponse GetWhnoCombo(string category, string level)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0069Repository repo = new AA0069Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(category,level);
                }
                catch when (!Debugger.IsAttached)
                {

                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p9 = form.Get("p9");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0069Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0,p1, p2,p3, p4, p5, p6, p7, p9, p10, p11, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
            var mat_class = form.Get("mat_class");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0069Repository repo = new AA0069Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo(p0, mat_class, page, limit, "");
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
                    bool isAuth = DBWork.UserInfo.VIEWALL;
                    AA0069Repository repo = new AA0069Repository(DBWork);
                    var taskid = repo.GetTaskid(User.Identity.Name);
                    if (isAuth)
                    { 
                        session.Result.etts = repo.GetWhComboA();
                    }
                    else
                    {
                        if (taskid == "1")
                        {
                            session.Result.etts = repo.GetWhCombo1();
                        }
                        else
                        {
                            session.Result.etts = repo.GetWhCombo2();
                        }
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
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    //AA0069Repository repo = new AA0069Repository(DBWork);
                    //session.Result.etts = repo.GetMatclassCombo(User.Identity.Name);
                    bool isAuth = DBWork.UserInfo.VIEWALL;
                    AA0069Repository repo = new AA0069Repository(DBWork);
                    var taskid = repo.GetTaskid(User.Identity.Name);
                    if (isAuth)
                    {
                        session.Result.etts = repo.GetMatclassComboA();
                    }
                    else
                    {
                        if (taskid == "1")
                        {
                            session.Result.etts = repo.GetMatclassCombo1();
                        }
                        else
                        {
                            session.Result.etts = repo.GetMatclassCombo2();
                        }
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
        public string GetUsertask()
        {
            string user_task = "";
            using (WorkSession session = new WorkSession())
            {

                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0069Repository repo = new AA0069Repository(DBWork);
                    user_task = repo.GetTaskid(User.Identity.Name);
                    return user_task;
                }
                catch
                {

                }

            }
            return user_task;
        }
        [HttpPost]
        public ApiResponse GetDoctypeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0069Repository repo = new AA0069Repository(DBWork);
                    session.Result.etts = repo.GetDoctypeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMcodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0069Repository repo = new AA0069Repository(DBWork);
                    session.Result.etts = repo.GetMcodeCombo();
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