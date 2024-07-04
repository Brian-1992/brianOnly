using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;

namespace WebApp.Controllers.AA
{   // 資料自1090801起
    public class AA0068Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var WH_NO = form.Get("P0");
            var YYYMMDD_B = form.Get("P1");
            var YYYMMDD_E = form.Get("P2");
            var P3 = form.Get("P3");
            //var MMCODE = form.Get("P5");
            //var P6 = form.Get("P6");
            var AGEN_NO = form.Get("P4");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");


            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0068Repository(DBWork);
                    // 每次先更新價格資料
                    //session.Result.etts = repo.GetAll(WH_NO, YYYMM, P2, P3, P4,  page, limit, sorters);
                    //repo.UpdatePrice(WH_NO, YYYMMDD_B, YYYMMDD_E, User.Identity.Name, DBWork.ProcIP);
                    string hospCode= repo.GetHospCode();
                    session.Result.etts = repo.GetAll(WH_NO, YYYMMDD_B, YYYMMDD_E, P3, AGEN_NO, page, limit, sorters, hospCode == "0");


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
                    var repo = new AA0068Repository(DBWork);
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
                    AA0068Repository repo = new AA0068Repository(DBWork);
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
            var YYYMMDD_B = form.Get("P1");
            var YYYMMDD_E = form.Get("P2");
            var P3 = form.Get("P3");
            var AGEN_NO = form.Get("P4");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0068Repository repo = new AA0068Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    JCLib.Excel.Export("藥品進貨日報表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(WH_NO, YYYMMDD_B, YYYMMDD_E, P3, AGEN_NO, hospCode=="0"));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH_NoCombo(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0068Repository repo = new AA0068Repository(DBWork);
                    session.Result.etts = repo.GetWH_NoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAgen_NoCombo(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0068Repository repo = new AA0068Repository(DBWork);
                    session.Result.etts = repo.GetAgen_NoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetHospCode() {
            using (WorkSession session = new WorkSession(this)) {
                UnitOfWork DBWork = session.UnitOfWork;
                try {
                    var repo = new AA0068Repository(DBWork);
                    session.Result.success = true;
                    session.Result.msg = repo.GetHospCode();
                } catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }

    }
}