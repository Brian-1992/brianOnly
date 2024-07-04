using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;

namespace WebApp.Controllers.C
{
    public class CB0007Controller : ApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0006Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, p6, page, limit, sorters, User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}