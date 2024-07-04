using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BE;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WebApp.Controllers.BE
{
    public class BG0007Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0007Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        
        
        [HttpPost]
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0007Repository repo = new BG0007Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetStroeidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0007Repository repo = new BG0007Repository(DBWork);
                    session.Result.etts = repo.GetStroeidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetContidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0007Repository repo = new BG0007Repository(DBWork);
                    session.Result.etts = repo.GetContidCombo();
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