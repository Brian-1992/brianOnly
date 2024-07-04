using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;

namespace WebApp.Controllers.UR
{
    public class UR1004Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetRoles(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_UIRRepository repo = new UR_UIRRepository(DBWork);
                    session.Result.etts = repo.GetRoles(p0, p1, User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetUsersInRole(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_UIRRepository(DBWork);
                    session.Result.etts = repo.GetUsersInRole(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetRolesInUser(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_UIRRepository(DBWork);
                    session.Result.etts = repo.GetRolesInUser(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetRolesNotInUser(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_UIRRepository(DBWork);
                    session.Result.etts = repo.GetRolesNotInUser(p0, p1, p2);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetUsersNotInRole(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 群組編號
            var p1 = form.Get("p1"); // 帳號
            var p2 = form.Get("p2"); // 姓名
            var p3 = form.Get("p3"); // 責任中心
            var p4 = form.Get("p4"); // AD帳號

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_UIRRepository(DBWork);
                    session.Result.etts = repo.GetUsersNotInRole(p0, p1, p2, p3, p4);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Create(UR_UIR ur_uir)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_UIRRepository(DBWork);
                    ur_uir.UIR_CREATE_BY = User.Identity.Name;
                    session.Result.afrs = repo.Create(ur_uir);

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
        public ApiResponse Delete(UR_UIR ur_uir)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_UIRRepository(DBWork);
                    session.Result.afrs = repo.Delete(ur_uir);

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
