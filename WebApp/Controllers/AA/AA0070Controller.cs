using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;

namespace WebApp.Controllers.AA
{
    public class AA0070Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var DIS_TIME_B = form.Get("P1");
            var DIS_TIME_E = form.Get("P2");
            var P3 = bool.Parse(form.Get("P3"));
            string APPTIME_B = form.Get("P4");
            string APPTIME_E = form.Get("P5");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0070Repository(DBWork);
                        session.Result.etts = repo.GetAll(DIS_TIME_B, DIS_TIME_E, P3, APPTIME_B, APPTIME_E, page, limit, sorters);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var DIS_TIME_B = form.Get("P1");
            var DIS_TIME_E = form.Get("P2");
            var P3 = bool.Parse(form.Get("P3"));
            string APPTIME_B = form.Get("P4");
            string APPTIME_E = form.Get("P5");


            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0070Repository repo = new AA0070Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    JCLib.Excel.Export("戰備調撥明細表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel( DIS_TIME_B, DIS_TIME_E, P3, APPTIME_B, APPTIME_E));
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