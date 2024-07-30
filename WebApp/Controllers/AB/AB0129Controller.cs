using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System;
using System.Data;
using System.Linq;

namespace WebApp.Controllers.AB
{
    public class AB0129Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0129Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }

        //庫房代碼下拉選單
        [HttpPost]
        public ApiResponse GetWH_NOComboOne(FormDataCollection form) //AB0129
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0129Repository repo = new AB0129Repository(DBWork);
                    session.Result.etts = repo.GetWH_NOComboOne(page, limit, "").Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //院內碼下拉選單
        [HttpPost]
        public ApiResponse GetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0129Repository repo = new AB0129Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo(p0, p1, page, limit, "");

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //月份
        [HttpPost]
        public ApiResponse GetNowMonth(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0129Repository repo = new AB0129Repository(DBWork);
                    session.Result.etts = repo.GetNowMonth();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //最後更新日期
        [HttpPost]
        public ApiResponse GetLastUpdateDate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0129Repository repo = new AB0129Repository(DBWork);
                    session.Result.etts = repo.GetLastUpdateDate();
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