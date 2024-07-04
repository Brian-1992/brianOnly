using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System;
using System.Data;

namespace WebApp.Controllers.AB
{
    public class AB0077Controller : SiteBase.BaseApiController
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
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9") == null ? string.Empty : form.Get("p9");
            var p10 = form.Get("p10") == null ? string.Empty : form.Get("p10");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0077Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var p9 = form.Get("p9") == null ? string.Empty : form.Get("p9");
            var p10 = form.Get("p10") == null ? string.Empty : form.Get("p10");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0077Repository repo = new AB0077Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);

                    dtItems.Merge(result);
                    dtItems.Columns["MMCODE"].ColumnName = "院內碼";
                    dtItems.Columns["TR_INV_QTY"].ColumnName = "進出數量";

                    JCLib.Excel.Export(form.Get("FN"), dtItems);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCODEComboOne(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0077Repository repo = new AB0077Repository(DBWork);
                    session.Result.etts = repo.GetMMCODEComboOne(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH_NOComboOne()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0077Repository repo = new AB0077Repository(DBWork);
                    session.Result.etts = repo.GetWH_NOComboOne();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDiffClsComboOne(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0077Repository repo = new AB0077Repository(DBWork);
                    session.Result.etts = repo.GetDiffClsComboOne();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetMiMcodes() {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0077Repository repo = new AB0077Repository(DBWork);
                    session.Result.etts = repo.GetMiMcodes();
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