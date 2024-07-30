using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;

namespace WebApp.Controllers.C
{
    public class CE0025Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse GetQueryData(FormDataCollection form)
        {
            var chkYM = form.Get("chkYM");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0025Repository(DBWork);
                    session.Result.etts = repo.GetQueryData("js", chkYM, page, limit, sorters); //撈出object
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