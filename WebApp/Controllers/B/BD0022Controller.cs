using JCLib.DB;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web;
using WebApp.Models;
using WebApp.Repository.B;

namespace WebApp.Controllers.B
{
    public class BD0022Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");    // 訂單號碼起
            var p0_1 = form.Get("p0_1");    // 訂單號碼訖
            var p1 = form.Get("p1");    // 物料分類
            var p2 = form.Get("p2");    // 合約識別碼
            var p3 = form.Get("p3");    // 訂單日期起
            var p3_1 = form.Get("p3_1");    // 訂單日期訖
            var p4 = form.Get("p4");    // 廠商代碼

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort") == null ? "" : form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0022Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p0_1, p1, p2, p3, p3_1, p4,page, limit, sorters); //撈出object
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse AllD(FormDataCollection form)
        {
            string po_no = form.Get("po_no");    // 訂單號碼
            string chk_deli_y = Convert.ToBoolean(form.Get("chk_deli_y")) == true ? "Y" : "N"; ;

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort") == null ? "" : form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0022Repository(DBWork);
                    session.Result.etts = repo.GetAllD(po_no, chk_deli_y, page, limit, sorters); //撈出object
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0022Repository repo = new BD0022Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMcontidCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0022Repository repo = new BD0022Repository(DBWork);
                    session.Result.etts = repo.GetMcontidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetPoTime()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0022Repository repo = new BD0022Repository(DBWork);
                    session.Result.etts = repo.GetPoTime();
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");    // 訂單號碼
            var p0_1 = form.Get("p0_1");
            var p1 = form.Get("p1");    // 物料分類
            var p2 = form.Get("p2");    // 合約識別碼
            var p3 = form.Get("p3");    // 訂單日期起
            var p3_1 = form.Get("p3_1");    // 訂單日期訖
            var p4 = form.Get("P4");  //廠商代碼

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0022Repository repo = new BD0022Repository(DBWork);
                    string filename = form.Get("FN") + '_' + DateTime.Now.ToString("yyyyMMddHHmmss");
                    JCLib.Excel.Export(string.Format("{0}.xlsx", filename), repo.GetExcel(p0, p0_1, p1, p2, p3, p3_1,p4));
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        //廠商代碼
        [HttpPost]
        public ApiResponse GetAgennoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0007Repository repo = new BE0007Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
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