using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.GB;
using WebApp.Models;

namespace WebApp.Controllers.GB
{
    public class GB0006Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse QueryM(FormDataCollection form)
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
                    ParamRepository repo = new ParamRepository(DBWork);
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
        public ApiResponse GetCombo(FormDataCollection form)
        {
            var grp_code = form.Get("GRP_CODE");
            var data_name = form.Get("DATA_NAME");
            /*
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            */

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    ParamRepository repo = new ParamRepository(DBWork);
                    session.Result.etts = repo.GetCombo(grp_code, data_name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CreateM(PARAM_M param_m)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ParamRepository repo = new ParamRepository(DBWork);
                    if (!repo.CheckExists(param_m))
                    {
                        session.Result.afrs = repo.Create(param_m);
                        session.Result.etts = repo.Get(param_m.GRP_CODE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>參數類別ID</span> 重複，請重新輸入。";
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
        public ApiResponse UpdateM(PARAM_M param_m)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ParamRepository repo = new ParamRepository(DBWork);
                    session.Result.afrs = repo.Update(param_m);
                    session.Result.etts = repo.Get(param_m.GRP_CODE);

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
        public ApiResponse DeleteM(PARAM_M param_m)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ParamRepository repo = new ParamRepository(DBWork);
                    session.Result.afrs += repo.DeleteD(param_m.GRP_CODE);
                    session.Result.afrs = repo.Delete(param_m.GRP_CODE);

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
                    ParamRepository repo = new ParamRepository(DBWork);
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
        public ApiResponse CreateD(PARAM_D param_d)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ParamRepository repo = new ParamRepository(DBWork);
                    if (!repo.CheckExists(param_d))
                    {
                        session.Result.afrs = repo.CreateD(param_d);
                        session.Result.etts = repo.GetD(param_d);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>參數ID</span> 重複，請重新輸入。";
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
        public ApiResponse UpdateD(PARAM_D param_d)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ParamRepository repo = new ParamRepository(DBWork);
                    if (param_d.DATA_SEQ == param_d.DATA_SEQ_O)
                    {
                        session.Result.afrs = repo.UpdateD(param_d);
                        session.Result.etts = repo.GetD(param_d);
                    }
                    else
                    {
                        if (!repo.CheckExists(param_d))
                        {
                            session.Result.afrs = repo.UpdateD(param_d);
                            session.Result.etts = repo.GetD(param_d);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>參數ID</span> 重複，請重新輸入。";
                        }
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
        public ApiResponse DeleteD(PARAM_D param_d)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ParamRepository repo = new ParamRepository(DBWork);
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
