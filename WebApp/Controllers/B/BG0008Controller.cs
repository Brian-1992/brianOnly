using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System;

namespace WebApp.Controllers.BG
{
    public class BG0008Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var po_time_b = form.Get("po_time_b");
            var po_time_e = form.Get("po_time_e");
            var mat_class = form.Get("mat_class");
            var agen_no = form.Get("agen_no");
            var m_storeid = form.Get("m_storeid");
            var m_contid = form.Get("m_contid");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0008Repository(DBWork);
                    session.Result.etts = repo.GetAll(po_time_b,  po_time_e,  mat_class,  agen_no,  m_storeid,  m_contid, page, limit, sorters);

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
                    //AA0092Repository repo = new AA0092Repository(DBWork);
                    //session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
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
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0008Repository(DBWork);
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
        public ApiResponse Excel(FormDataCollection form)
        {
            var po_time_b = form.Get("po_time_b");
            var po_time_e = form.Get("po_time_e");
            var mat_class = form.Get("mat_class");
            var agen_no = form.Get("agen_no");
            var m_storeid = form.Get("m_storeid");
            var m_contid = form.Get("m_contid");


            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0008Repository repo = new BG0008Repository(DBWork);
                    string m_storeidStr = m_storeid == "1" ? "(庫備)" : "(非庫備)";
                    JCLib.Excel.Export("進貨缺交報表"+ m_storeidStr + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(po_time_b, po_time_e, mat_class, agen_no, m_storeid, m_contid));
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