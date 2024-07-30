using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System;
using System.Data;

namespace WebApp.Controllers.AB
{
    public class AB0070Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetMMCODEComboOne(FormDataCollection form)
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
                    AB0070Repository repo = new AB0070Repository(DBWork);
                    session.Result.etts = repo.GetMMCODEComboOne(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH_NOComboOne()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0070Repository repo = new AB0070Repository(DBWork);
                    session.Result.etts = repo.GetWH_NOComboOne();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDiffClsComboOne(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0070Repository repo = new AB0070Repository(DBWork);
                    session.Result.etts = repo.GetDiffClsComboOne();
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