using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace WebApp.Controllers.C
{
    public class CD0012Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse Query(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 責任中心代碼

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0012Repository(DBWork);

                    session.Result.etts = repo.GetQuery(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Create(MM_ITEMS_COMP mm_items_comp)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0012Repository(DBWork);
                    if (!repo.CheckExists(mm_items_comp))
                    {

                        mm_items_comp.CREATE_USER = User.Identity.Name;
                        mm_items_comp.UPDATE_USER = User.Identity.Name;
                        mm_items_comp.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.Create(mm_items_comp);

                        session.Result.etts = repo.Get(mm_items_comp.INID);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "此責任中心已建檔，請重新輸入。";
                    }

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Update(MM_ITEMS_COMP mm_items_comp)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0012Repository(DBWork);
                    mm_items_comp.UPDATE_USER = User.Identity.Name;
                    mm_items_comp.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(mm_items_comp);
                    session.Result.etts = repo.Get(mm_items_comp.INID);

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Delete(MM_ITEMS_COMP mm_items_comp)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0012Repository(DBWork);
                    session.Result.afrs += repo.Delete(mm_items_comp.INID);
                    
                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetQueryInidCombo(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0012Repository repo = new CD0012Repository(DBWork);
                    session.Result.etts = repo.GetQueryInidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFormInidCombo(FormDataCollection form)
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
                    CD0012Repository repo = new CD0012Repository(DBWork);
                    session.Result.etts = repo.GetFormInidCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetInidNameByInid(FormDataCollection form)
        {
            string inid = form.Get("INID");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0012Repository repo = new CD0012Repository(DBWork);
                    session.Result.etts = repo.GetInidNameByInid(inid);
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