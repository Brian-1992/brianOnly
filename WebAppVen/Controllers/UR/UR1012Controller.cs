using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebAppVen.Repository.UR;
using WebAppVen.Models;

namespace WebAppVen.Controllers.UR
{
    public class UR1012Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse Show(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_BulletinRepository repo = new UR_BulletinRepository(DBWork);
                    session.Result.etts = repo.Show(page, limit, sorters, p0, p1, User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Query(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_BulletinRepository repo = new UR_BulletinRepository(DBWork);
                    session.Result.etts = repo.Query(page, limit, sorters, p0, p1);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Create(UR_BULLETIN bulletin)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    bulletin.CREATE_BY = User.Identity.Name;
                    bulletin.CREATE_DT = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    UR_BulletinRepository repo = new UR_BulletinRepository(DBWork);
                    session.Result.afrs = repo.Create(bulletin);
                    session.Result.etts = repo.Get(bulletin.ID);

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
        public ApiResponse Update(UR_BULLETIN bulletin)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    bulletin.UPDATE_BY = User.Identity.Name;
                    bulletin.UPDATE_DT = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    UR_BulletinRepository repo = new UR_BulletinRepository(DBWork);
                    session.Result.afrs = repo.Update(bulletin);
                    session.Result.etts = repo.Get(bulletin.ID);

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
        public ApiResponse Delete(UR_BULLETIN bulletin)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    UR_BulletinRepository repo = new UR_BulletinRepository(DBWork);
                    session.Result.afrs = repo.Delete(bulletin.ID);

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
