using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System;

namespace WebApp.Controllers.F
{
    public class FA0075Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var MAT_CLASS = form.Get("P0");
            var APVTIME_B = form.Get("P1");
            var APVTIME_E = form.Get("P2");
            var MMCODE = form.Get("P3");
            bool isFlowid6only = form.Get("flowid6only") == "Y";
            bool isApvqtynot0 = form.Get("apvqtynot0") == "Y";

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0075Repository(DBWork);
                    session.Result.etts = repo.GetAll(MAT_CLASS, APVTIME_B, APVTIME_E, MMCODE, isFlowid6only, isApvqtynot0, page, limit, sorters);

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
                    var repo = new FA0075Repository(DBWork);
                    session.Result.etts = repo.GetMATCombo(p0);
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
                    FA0075Repository repo = new FA0075Repository(DBWork);
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
            var MAT_CLASS = form.Get("P0");
            var APVTIME_B = form.Get("P1");
            var APVTIME_E = form.Get("P2");
            var MMCODE = form.Get("P3");
            var MAT_CLASS_N = form.Get("P4");
            bool isFlowid6only = form.Get("flowid6only") == "Y";
            bool isApvqtynot0 = form.Get("apvqtynot0") == "Y";

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0075Repository repo = new FA0075Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    JCLib.Excel.Export("核撥明細品項報表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(MAT_CLASS, APVTIME_B, APVTIME_E, MMCODE, isFlowid6only, isApvqtynot0),
                         (dt) =>
                         {
                             return string.Format("{0}{1}{2}至{3}核撥明細品項報表", DBWork.UserInfo.InidName, MAT_CLASS_N, APVTIME_B == "" ? " ~ " : APVTIME_B, APVTIME_E == "" ? "~" : APVTIME_E);
                         });
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