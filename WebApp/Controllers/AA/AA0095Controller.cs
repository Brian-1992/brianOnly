using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;

namespace WebApp.Controllers.AA
{
    public class AA0095Controller : SiteBase.BaseApiController
    {
        /// <summary>
        /// 讀取庫別代碼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse GetWHNO(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0095Repository repo = new AA0095Repository(DBWork);
                    session.Result.etts = repo.GetWHNO();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        /// <summary>
        /// 取得報表資料
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse SearchReportData(FormDataCollection form)
        {
            var p0 = form.Get("p0");    //庫別代碼
            var p1 = form.Get("p1");    //月份別
            var p2 = form.Get("p2");    //藥品代碼_FROM
            var p3 = form.Get("p3");    //藥品代碼_TO
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0095Repository repo = new AA0095Repository(DBWork);
                    session.Result.etts = repo.SearchReportData(p0, p1, p2, p3, page, limit, sorters);
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