using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System.Data;
using System;
using System.Text;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Linq;


namespace WebApp.Controllers.F
{
    public class FA0098Controller : SiteBase.BaseApiController
    {
        //查詢Form
        [HttpPost]
        public ApiResponse AllForm(FormDataCollection form)
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
                    var repo = new FA0098Repository(DBWork);
                    session.Result.etts = repo.GetAllForm(p0, page, limit, sorters);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        // 查詢Grid
        [HttpPost]
        public ApiResponse AllGrid(FormDataCollection form)
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
                    var repo = new FA0098Repository(DBWork);
                    session.Result.etts = repo.GetAllGrid(p0, page, limit, sorters);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        //Gird的合計Form
        [HttpPost]
        public ApiResponse AmountForm(FormDataCollection form)
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
                    var repo = new FA0098Repository(DBWork);
                    session.Result.etts = repo.GetAmountForm(p0, page, limit, sorters);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
    }
}