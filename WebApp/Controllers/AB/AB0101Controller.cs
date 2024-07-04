using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AB
{
    public class AB0101Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form) {
            string date1 = form.Get("p0");
            string date2 = form.Get("p1");
            string frwh = form.Get("p2");
            string towh = form.Get("p3");
            string mmcode = form.Get("p4");
            string posttype = form.Get("p5");
            string diff = form.Get("p6");

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    AB0101Repository repo = new AB0101Repository(DBWork);
                    session.Result.etts = repo.GetAll(date1, date2, frwh, towh, mmcode, posttype, Convert.ToBoolean(diff));

                }
                catch (Exception ex) {
                    throw;
                }

                return session.Result;
            }

        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            string date1 = form.Get("p0");
            string date2 = form.Get("p1");
            string frwh = form.Get("p2");
            string towh = form.Get("p3");
            string mmcode = form.Get("p4");
            string posttype = form.Get("p5");
            string diff = form.Get("p6");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0101Repository repo = new AB0101Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;

                    result = repo.GetExcel(date1, date2, frwh, towh, mmcode, posttype, Convert.ToBoolean(diff));


                    dtItems.Merge(result);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        #region combo
        public ApiResponse TowhNos(FormDataCollection form)
        {

            string query = form.Get("query");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0101Repository(DBWork);
                    session.Result.etts = repo.GetTowhNos(query, DBWork.UserInfo.UserId).Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse FrwhNos(FormDataCollection form)
        {

            string query = form.Get("query");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0101Repository(DBWork);
                    session.Result.etts = repo
                        .GetFrwhNos(query)
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0101Repository repo = new AB0101Repository(DBWork);
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
        public ApiResponse GetOrdersortCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0101Repository repo = new AB0101Repository(DBWork);
                    session.Result.etts = repo.GetOrdersortCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion
    }
}