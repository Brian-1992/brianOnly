using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;


namespace WebApp.Controllers.AA
{
    public class AA0056Controller : SiteBase.BaseApiController
    {
        // 查詢Master
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0056Repository(DBWork);
                    session.Result.etts = repo.GetAllM(page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢Detail
        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0056Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //修改Master
        [HttpPost]
        public ApiResponse UpdateM(AA0056M AA0056M)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0056Repository(DBWork);
                    AA0056M.UPDATE_USER = User.Identity.Name;
                    AA0056M.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(AA0056M);

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

        //修改Detail
        [HttpPost]
        public ApiResponse UpdateD(AA0056D AA0056D)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0056Repository(DBWork);
                    AA0056D.UPDATE_USER = User.Identity.Name;
                    AA0056D.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(AA0056D);

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