using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.G;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Controllers.G
{
    public class GA0007Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0007Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        
        // 修改
        [HttpPost]
        public ApiResponse Update(GA0007 ga0007)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0007Repository(DBWork);
                    ga0007.UPDATE_USER = User.Identity.Name;
                    ga0007.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(ga0007);
                    session.Result.etts = repo.Get(ga0007.MMCODE);

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
        public ApiResponse GetYN()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    GA0007Repository repo = new GA0007Repository(DBWork);
                    session.Result.etts = repo.GetYN();
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