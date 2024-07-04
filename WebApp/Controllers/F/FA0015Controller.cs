using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System;

namespace WebApp.Controllers.F
{
    public class FA0015Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var MAT_CLASS = "02";   //form.Get("P0");
            var DIS_TIME_B = form.Get("P1");
            var DIS_TIME_E = form.Get("P2");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0015Repository(DBWork);
                        session.Result.etts = repo.GetAll(MAT_CLASS, DIS_TIME_B, DIS_TIME_E, page, limit, sorters);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMATCombo(bool p0)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0015Repository(DBWork);
                    session.Result.etts = repo.GetMATCombo(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse getDeptName()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0015Repository(DBWork);
                    session.Result.etts = repo.getDeptName();
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
            var MAT_CLASS = form.Get("P0");
            var DIS_TIME_B = form.Get("P1");
            var DIS_TIME_E = form.Get("P2");
            var DIS_TIME_B_raw = form.Get("P3");
            var DIS_TIME_E_raw = form.Get("P4");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    
                    FA0015Repository repo = new FA0015Repository(DBWork);
                    var DeptName = repo.getDeptName().ToString();
                    var MatName = repo.getMatName(MAT_CLASS).ToString();
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    using (var dataTable1 = repo.GetExcel(MAT_CLASS, DIS_TIME_B, DIS_TIME_E))
                    {
                        JCLib.Excel.Export("戰備調撥統計表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", dataTable1,
                            (dt) =>
                            {
                                return string.Format("{0}{1}{2}至{3}戰備調撥統計表", DeptName, MatName, DIS_TIME_B_raw, DIS_TIME_E_raw);
                            });
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