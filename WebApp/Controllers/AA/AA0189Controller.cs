using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.AA;

namespace WebApp.Controllers.AA
{
    public class AA0189Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0189Repository(DBWork);
                    CN_RECORD query = new CN_RECORD();

                    query.WH_NO = p0 == null ? "" : p0.Trim();
                    query.MMCODE = p1 == null ? "" : p1.Trim();
                    query.QTY_CHK = Convert.ToBoolean(p2) == true ? "Y" : "N"; // 顯示數量不符資料
                    session.Result.etts = repo.GetAll(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("WH_NO");
            var p1 = form.Get("MMCODE");
            var p2 = form.Get("QTY_CHK");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0189Repository repo = new AA0189Repository(DBWork);
                    p0 = p0 == null ? "" : p0.Trim();
                    p1 = p1 == null ? "" : p1.Trim();
                    p2 = Convert.ToBoolean(p2) == true ? "Y" : "N"; // 顯示數量不符資料
                    JCLib.Excel.Export("寄售數量回報查詢_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls", repo.GetExcel(p0, p1, p2));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
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
                    AA0189Repository repo = new AA0189Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0189Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo();
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