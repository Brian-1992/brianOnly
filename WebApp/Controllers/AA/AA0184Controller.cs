using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebApp.Models;
using WebApp.Models.AB;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AB
{
    public class AA0184Controller : SiteBase.BaseApiController
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            string[] arr_p3 = { };
            if (!string.IsNullOrEmpty(p3))
            {
                arr_p3 = p3.Trim().Split(','); //用,分割
            }

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0184Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, arr_p3, p4, p5, p6, p7, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //匯出
        [HttpPost]
        public ApiResponse GetExcel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");

            string[] arr_p3 = { };
            if (!string.IsNullOrEmpty(p3))
            {
                arr_p3 = p3.Trim().Split(','); //用,分割
            }

            string fileName = string.Format("申請核撥查詢_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0184Repository repo = new AA0184Repository(DBWork);
                    JCLib.Excel.Export(fileName, repo.GetExcel(p0, p1, p2, arr_p3, p4, p5, p6, p7));
                }
                catch(Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        //物料代碼combo
        [HttpPost]
        public ApiResponse GetMatClassSubCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0184Repository repo = new AA0184Repository(DBWork);
                    session.Result.etts = repo.GetMatClassSubCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //申請日期區間
        [HttpPost]
        public ApiResponse GetStartApptime(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0184Repository repo = new AA0184Repository(DBWork);
                    session.Result.etts = repo.GetStartApptime();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //申請單狀態combo
        [HttpPost]
        public ApiResponse GetFlowIdCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0184Repository repo = new AA0184Repository(DBWork);
                    session.Result.etts = repo.GetFlowIdCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //入庫庫房combo
        [HttpPost]
        public ApiResponse GetWhNoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0184Repository repo = new AA0184Repository(DBWork);
                    session.Result.etts = repo.GetWhNoCombo(p0);
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
                    AA0184Repository repo = new AA0184Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
    }
}