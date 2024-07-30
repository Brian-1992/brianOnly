using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;

namespace WebApp.Controllers.UR
{
    public class UR1011Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse QueryM(FormDataCollection form)
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
                    UR_ParamRepository repo = new UR_ParamRepository(DBWork);
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
        public ApiResponse CreateM(UR_PARAM_M param_m)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    UR_ParamRepository repo = new UR_ParamRepository(DBWork);
                    session.Result.afrs = repo.Create(param_m);
                    session.Result.etts = repo.Get(param_m.ID);

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
        public ApiResponse UpdateM(UR_PARAM_M param_m)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    UR_ParamRepository repo = new UR_ParamRepository(DBWork);
                    session.Result.afrs = repo.Update(param_m);
                    session.Result.etts = repo.Get(param_m.ID);

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
        public ApiResponse DeleteM(UR_PARAM_M param_m)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    UR_ParamRepository repo = new UR_ParamRepository(DBWork);
                    session.Result.afrs = repo.Delete(param_m.ID);

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
        public ApiResponse QueryD(FormDataCollection form)
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
                    UR_ParamRepository repo = new UR_ParamRepository(DBWork);
                    session.Result.etts = repo.QueryD(page, limit, sorters, p0, p1, p2);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CreateD(UR_PARAM_D param_d)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    UR_ParamRepository repo = new UR_ParamRepository(DBWork);
                    session.Result.afrs = repo.CreateD(param_d);
                    session.Result.etts = repo.GetD(param_d);

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
        public ApiResponse UpdateD(UR_PARAM_D param_d)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    UR_ParamRepository repo = new UR_ParamRepository(DBWork);
                    session.Result.afrs = repo.UpdateD(param_d);
                    session.Result.etts = repo.GetD(param_d);

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
        public ApiResponse DeleteD(UR_PARAM_D param_d)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    UR_ParamRepository repo = new UR_ParamRepository(DBWork);
                    session.Result.afrs = repo.DeleteD(param_d);

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
