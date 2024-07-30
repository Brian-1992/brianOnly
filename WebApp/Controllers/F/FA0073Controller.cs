using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
namespace WebApp.Controllers.F
{
    public class FA0073Controller : SiteBase.BaseApiController
    {
        // FA0073 查詢

        public ApiResponse All(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var showopt = form.Get("showopt");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0073Repository(DBWork);
                    session.Result.etts = repo.GetAll(d0, d1, p0, p1, showopt, page, limit, sorters); //撈出object
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0073Repository repo = new FA0073Repository(DBWork);
                    session.Result.etts = repo.GetMatClass();

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTowhCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0073Repository repo = new FA0073Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo();
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