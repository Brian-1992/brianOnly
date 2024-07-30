using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.F;

namespace WebApp.Controllers.F
{
    public class FA0041Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var chk_ym = form.Get("P0");
            var unitClass = form.Get("P1");
            var isChk = form.Get("P2");
            var chk_type = form.Get("P3") == null ? string.Empty : form.Get("P3").Trim();
            var chk_status = form.Get("P4") == null ? string.Empty : form.Get("P4").Trim();

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0041Repository(DBWork);
                    
                    session.Result.etts = repo.GetAll(chk_ym, unitClass, isChk, chk_type, chk_status);
                }
                catch(Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var chk_ym = form.Get("P0");
            var unitClass = form.Get("P1").Replace("&#x3D;","=").Replace("&#39;","'");
            var isChk = form.Get("P2");
            var chk_type = form.Get("P3") == null ? string.Empty : form.Get("P3").Trim();
            var chk_status = form.Get("P4") == null ? string.Empty : form.Get("P4").Trim();

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0041Repository(DBWork);
                    //session.Result.etts = repo.GetAll(chk_ym, storeId, matClass);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = repo.GetExcel(chk_ym, unitClass, isChk, chk_type, chk_status);

                    dtItems.Merge(result);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 已完成資料
        [HttpPost]
        public ApiResponse DoneDatas(FormDataCollection form) {
            var chk_ym = form.Get("P0");
            var unitClass = form.Get("P1");
            var isChk = form.Get("P2");
            var chk_type = form.Get("P3") == null ? string.Empty : form.Get("P3").Trim();
            var chk_status = form.Get("P4") == null ? string.Empty : form.Get("P4").Trim();
            var wh_no = form.Get("P5") == null ? string.Empty : form.Get("P5").Trim();
            var mmcode = form.Get("P6") == null ? string.Empty : form.Get("P6").Trim();
            var chk_no = form.Get("P7") == null ? string.Empty : form.Get("P7").Trim();

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0041Repository(DBWork);

                    session.Result.etts = repo.GetDoneDatas(chk_ym, unitClass, chk_type, chk_status, wh_no, mmcode, chk_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse DoneExcel(FormDataCollection form) {
            var chk_ym = form.Get("P0");
            var unitClass = form.Get("P1").Replace("&#x3D;", "=").Replace("&#39;", "'"); ;
            var isChk = form.Get("P2");
            var chk_type = form.Get("P3") == null ? string.Empty : form.Get("P3").Trim();
            var chk_status = form.Get("P4") == null ? string.Empty : form.Get("P4").Trim();
            var wh_no = form.Get("P5") == null ? string.Empty : form.Get("P5").Trim();
            var mmcode = form.Get("P6") == null ? string.Empty : form.Get("P6").Trim();
            var chk_no = form.Get("P7") == null ? string.Empty : form.Get("P7").Trim();

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0041Repository(DBWork);
                    //session.Result.etts = repo.GetAll(chk_ym, storeId, matClass);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = repo.GetDoneExcel(chk_ym, unitClass, chk_type, chk_status, wh_no, mmcode, chk_no);

                    dtItems.Merge(result);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region combo
        [HttpGet]
        public ApiResponse GetUnitClassConbo() {
            List<ComboItemModel> list = new List<ComboItemModel>();
           
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0041Repository(DBWork);

                    list.Add(new ComboItemModel { VALUE = " = 'A'", TEXT = string.Format("{0}({1})", "應盤點單位", repo.GetUnitCount("A")) });
                    list.Add(new ComboItemModel { VALUE = " = 'B'", TEXT = string.Format("{0}({1})", "行政科室", repo.GetUnitCount("B")) });
                    list.Add(new ComboItemModel { VALUE = " = 'C'", TEXT = string.Format("{0}({1})", "財務獨立單位", repo.GetUnitCount("C")) });
                    list.Add(new ComboItemModel { VALUE = " <> 'D'", TEXT = string.Format("{0}", "全院") });
                    
                    session.Result.etts = list;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetChkTypeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0041Repository(DBWork);
                    session.Result.etts = repo.GetChkType();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

        }
        [HttpGet]
        public ApiResponse GetChkStatusCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0041Repository(DBWork);
                    session.Result.etts = repo.GetChkStatus();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

        }

        [HttpPost]
        public ApiResponse GetWhnoCombo(FormDataCollection form)
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
                    FA0041Repository repo = new FA0041Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(p0, page, limit, "")
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_GRADE = w.WH_GRADE });
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
                    FA0041Repository repo = new FA0041Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
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