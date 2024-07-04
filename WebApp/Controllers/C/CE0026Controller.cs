using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Repository.F;
using WebApp.Models;
using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace WebApp.Controllers.C
{
    public class CE0026Controller  : SiteBase.BaseApiController
    {
        // GET api/<controller>
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("P0");
            var p1 = form.Get("P1");
          

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");




            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0026Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, page, limit, sorters);
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