using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BE;
using WebApp.Models;
using System;

namespace WebApp.Controllers.BH
{
    public class BH0005Controller : SiteBase.BaseApiController
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
                    var repo = new BH0005Repository(DBWork);
                    if (bool.Parse(p5) == true)
                    {
                        session.Result.etts = repo.GetNow(p0, p1, p2, p3, p4, page, limit, sorters);
                    }
                    else {
                        session.Result.etts = repo.GetHis(p0, p1, p2, p3, p4, page, limit, sorters);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public string GetDataTime()
        {
            string str = "";
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BH0005Repository(DBWork);
                    str = repo.GetDataTime();
                }
                catch
                {
                    throw;
                }
                return str;
            }
        }


        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");



            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0005Repository repo = new BH0005Repository(DBWork);
                    if (bool.Parse(p5) == true)
                    {
                        JCLib.Excel.Export("院內氣體鋼瓶現況" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel_Now(p0, p1, p2, p3, p4));
                    }
                    else
                    {
                        JCLib.Excel.Export("院內氣體鋼瓶歷史資料" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel_His(p0, p1, p2, p3, p4));

                    }
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