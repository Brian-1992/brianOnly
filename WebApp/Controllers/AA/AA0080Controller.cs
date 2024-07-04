using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0080Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var MMCODE = form.Get("MMCODE");
            var WH_NO = form.Get("WH_NO");
            var p2 = form.Get("p2");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0092Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetNow(WH_NO, MMCODE, p2, page, limit, sorters, hospCode=="0");

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
                    AA0080Repository repo = new AA0080Repository(DBWork);
                    session.Result.etts = repo.GetWH_NoCombo(p0, page, limit, "")
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_GRADE = w.WH_GRADE });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //[HttpPost]
        //public ApiResponse GetMMCodeCombo(FormDataCollection form)
        //{
        //    var p0 = form.Get("p0");

        //    var page = int.Parse(form.Get("page"));
        //    var start = int.Parse(form.Get("start"));
        //    var limit = int.Parse(form.Get("limit"));
        //    var sorters = form.Get("sort");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AA0092Repository repo = new AA0092Repository(DBWork);
        //            session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var MMCODE = form.Get("MMCODE");
            var WH_NO = form.Get("WH_NO");
            var p2 = form.Get("p2");


            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0092Repository repo = new AA0092Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    string hospCode = repo.GetHospCode();
                    JCLib.Excel.Export("疫管署撥發藥品庫存報表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(WH_NO, MMCODE, p2, hospCode=="0"));
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