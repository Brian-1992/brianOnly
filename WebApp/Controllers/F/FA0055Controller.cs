using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;

namespace WebApp.Controllers.F
{
    public class FA0055Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
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
                    var repo = new FA0055Repository(DBWork);
                    
                    IEnumerable<FA0055> items = repo.GetAllM(p0, page, limit, sorters);
                    session.Result.etts = items;

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Apply(FormDataCollection form)
        {
            var p0 = form.Get("P0");
            var p1 = form.Get("P1");
            var p2 = form.Get("P2");
            var p3 = form.Get("P3");
            var p4 = form.Get("P4");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    FA0055Repository repo = new FA0055Repository(DBWork);

                    var rtn = repo.CallProc(p0, p1, p2, p3, p4);
                    if (rtn != "Y")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "發生執行錯誤，" + rtn + "。";
                    }

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
        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0055Repository repo = new FA0055Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetYMCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0055Repository(DBWork);
                    session.Result.etts = repo.GetYMCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTpeoOth(FormDataCollection form) {

            string set_ym = form.Get("P0");
            

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (set_ym == string.Empty)
                    {
                        return session.Result;
            }

                    var repo = new FA0055Repository(DBWork);
                    session.Result.etts = repo.GetTpeoOth(set_ym);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region 明細
        [HttpPost]
        public ApiResponse GetDetails(FormDataCollection form) {
            string data_ym = form.Get("data_ym");
            string status = form.Get("status");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0055Repository(DBWork);
                    session.Result.etts = repo.GetDetails(data_ym, status);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDetailsExcel(FormDataCollection form)
        {
            string data_ym = form.Get("data_ym");
            string status = form.Get("status");
            string fn = form.Get("fn");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0055Repository(DBWork);

                    DataTable result = null;

                    result = repo.GetDetailsExcel(data_ym, status);

                    JCLib.Excel.Export(fn, result);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        #region 其他消耗明細
        [HttpPost]
        public ApiResponse GetOtherDetailsExcel(FormDataCollection form)
        {
            string data_ym = form.Get("data_ym");
            string fn = form.Get("fn");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0055Repository(DBWork);

                    DataTable result = null;

                    result = repo.GetOtherDetailsExcel(data_ym);

                    JCLib.Excel.Export(fn, result);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        #region 2-4級庫調整金額明細
        [HttpPost]
        public ApiResponse GetAdjCost24Excel(FormDataCollection form)
        {
            string data_ym = form.Get("data_ym");
            string fn = form.Get("fn");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0055Repository(DBWork);

                    DataTable result = null;

                    result = repo.GetAdjCost24Excel(data_ym);

                    JCLib.Excel.Export(fn, result);

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