using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System;

namespace WebApp.Controllers.F
{
    public class FA0046Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var WH_NO = form.Get("P0");
            var YYYMM = form.Get("P1");
            var P2 = form.Get("P2");
            var P3 = form.Get("P3");
            var P4 = form.Get("P4");
            //var MMCODE = form.Get("P5");
            //var P6 = form.Get("P6");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");


            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0046Repository(DBWork);
                    //session.Result.etts = repo.GetAll(WH_NO, YYYMM, P2, P3, P4,  page, limit, sorters);

                    if (P2 == "01")
                    {
                        session.Result.etts = repo.GetAll(WH_NO, YYYMM, P2,  P4, page, limit, sorters);
                    }
                    else
                    {
                        session.Result.etts = repo.GetAll(WH_NO, YYYMM, P2, P3, page, limit, sorters);

                    }

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMATCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0046Repository(DBWork);
                    session.Result.etts = repo.GetMATCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            //var wh_no = form.Get("WH_NO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0046Repository repo = new FA0046Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
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
            var WH_NO = form.Get("P0");
            var YYYMM = form.Get("P1");
            var P2 = form.Get("P2");
            var P3 = form.Get("P3");
            var P4 = form.Get("P4");





            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0046Repository repo = new FA0046Repository(DBWork);

                    if (P2 == "01")
                    {
                        JCLib.Excel.Export("各衛星庫房盤盈虧品項報表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(WH_NO, YYYMM, P2, P4));

                    }
                    else
                    {
                        JCLib.Excel.Export("各衛星庫房盤盈虧品項報表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(WH_NO, YYYMM, P2, P3));
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