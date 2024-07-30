using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
namespace WebApp.Controllers.AA
{
    public class AA0165Controller : SiteBase.BaseApiController
    {
        // AA0165 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0165Repository(DBWork);
                    AA0165_MODEL v = new AA0165_MODEL();
                    v.APPTIME_START = form.Get("APPTIME_START");
                    v.APPTIME_END = form.Get("APPTIME_END");
                    v.FRWH = form.Get("FRWH");
                    v.MAT_CLASS = form.Get("MAT_CLASS");
                    session.Result.etts = repo.GetAll(v, page, limit, sorters); //撈出object
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFrwhCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0165Repository repo = new AA0165Repository(DBWork);
                    session.Result.etts = repo.GetFrwh(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        } // 


        [HttpPost]
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0165Repository repo = new AA0165Repository(DBWork);
                    AA0165Repository repo2 = new AA0165Repository(DBWork);
                    session.Result.etts = repo2.GetMatClassOrigin();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        } // 

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0165Repository repo = new AA0165Repository(DBWork);

                    AA0165_MODEL v = new AA0165_MODEL();
                    v.APPTIME_START = form.Get("APPTIME_START");
                    v.APPTIME_END = form.Get("APPTIME_END");
                    v.FRWH = form.Get("FRWH");
                    v.MAT_CLASS = form.Get("MAT_CLASS");
                    v.USERID = User.Identity.Name;
                    JCLib.Excel.Export(
                        "中央庫房(衛材)繳回入帳明細表_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", 
                        repo.GetExcel(v));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // AB0056 查詢
        [HttpPost]
        public ApiResponse AllByDept(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var showopt = form.Get("showopt");
            var showdata = form.Get("showdata");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0165Repository(DBWork);
                    session.Result.etts = repo.GetAllByDept(User.Identity.Name, d0, d1, p0, p1, showopt, showdata, page, limit, sorters); //撈出object
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        

        [HttpPost]
        public ApiResponse GetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
            var mat_class = form.Get("mat_class");
            var store_id = form.Get("store_id");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0165Repository repo = new AA0165Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo(p0, mat_class, store_id, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse ExcelByDept(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var showopt = form.Get("p2");
            var showdata = form.Get("p3");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0165Repository repo = new AA0165Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN") + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcelByDept(User.Identity.Name, d0, d1, p0, p1, showopt, showdata));
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