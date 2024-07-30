using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System;

namespace WebApp.Controllers.AB
{
    public class AB0080Controller : SiteBase.BaseApiController
    {

        [HttpPost]
        public ApiResponse GetWH_NoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1= form.Get("p1");

            //var wh_no = form.Get("WH_NO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0080Repository repo = new AB0080Repository(DBWork);
                    session.Result.etts = repo.GetWH_NoCombo(p0,p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetWhGCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0080Repository repo = new AB0080Repository(DBWork);
                    session.Result.etts = repo.GetWhGCombo();
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