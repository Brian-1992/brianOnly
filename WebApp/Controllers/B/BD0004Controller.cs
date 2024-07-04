using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System;

namespace WebApp.Controllers.B
{
    public class BD0004Controller  : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {
          
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0004Repository(DBWork);
                    session.Result.etts = repo.GetMasterAll(p1, p2, p3, p4, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse DetailAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            //var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0004Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse MasterUpdate(MM_PO_M mm_po_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0004Repository(DBWork);
                    mm_po_m.UPDATE_USER = User.Identity.Name;
                    mm_po_m.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterUpdate(mm_po_m);
                    session.Result.etts = repo.MasterGet(mm_po_m.PO_NO);

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
        public ApiResponse MasterUpdateMail(FormDataCollection form)
        {
            string pono = form.Get("pono");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0004Repository(DBWork);
                    string upuser = User.Identity.Name;
                    string upip = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterUpdateMAIL(pono, upuser, upip);
                    //session.Result.etts = repo.MasterGet(mm_po_m.PO_NO);

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
    }
}