using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
using System.Collections.Generic;

namespace WebApp.Controllers.AA
{
    public class AA0061Controller : SiteBase.BaseApiController
    {
        // AA0061 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
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

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0061Repository(DBWork);
                    session.Result.etts = repo.GetAll(User.Identity.Name, d0, d1,p0, p1, showopt,showdata, page, limit, sorters); //撈出object
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //AA0061
        [HttpPost]
        public ApiResponse GetMatclassCombo_61(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0061Repository repo = new AA0061Repository(DBWork);
                    string task_id = repo.CheckMiWhid(User.Identity.Name);

                    session.Result.etts = repo.GetMatClassOrigin(task_id);
                    //session.Result.msg = repo.CheckMiWhid(User.Identity.Name);
                    //if (session.Result.msg == "2")
                    //{
                    //    session.Result.etts = repo.GetMatClassMedic("2");
                    //}
                    //else
                    //{
                    //    session.Result.etts = repo.GetMatClassOrigin();   //直接回傳3~8
                    //}



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

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0061Repository(DBWork);

                    IEnumerable<string> depts = repo.GetUserWhnos(DBWork.UserInfo.UserId);
                    string deptString = GetDeptStrings(depts);

                    session.Result.etts = repo.GetAllByDept(User.Identity.Name, d0, d1, p0, p1, showopt, showdata, deptString); //撈出object
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public string GetDeptStrings(IEnumerable<string> depts) {
            string result = string.Empty;
            foreach (string dept in depts) {
                if (result != string.Empty) {
                    result += ", ";
                }
                result += string.Format("'{0}'", dept);
            }
            return result;
        }

        //AB0056
        [HttpPost]
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {

                    AA0061Repository repo2 = new AA0061Repository(DBWork);
                    session.Result.etts = repo2.GetMatClassCombo_S();

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
                    AA0061Repository repo = new AA0061Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo(p0, mat_class, store_id, page, limit, "");
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
                    AA0061Repository repo = new AA0061Repository(DBWork);

                    using (var dataTable1 = repo.GetExcel(User.Identity.Name, d0, d1, p0, p1, showopt, showdata))
                    {
                        JCLib.Excel.Export(form.Get("FN") + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", dataTable1,
                            (dt) => {
                                //headerHandler: 返回Excel表首要顯示的文字
                                return string.Format(repo.GetExcelTitle(User.Identity.Name,d0,d1, showdata));
                            });
                    }
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

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0061Repository repo = new AA0061Repository(DBWork);

                    IEnumerable<string> depts = repo.GetUserWhnos(DBWork.UserInfo.UserId);
                    string deptString = GetDeptStrings(depts);

                    using (var dataTable1 = repo.GetExcelByDept(User.Identity.Name, d0, d1, p0, p1, showopt, showdata, deptString))
                    {
                        JCLib.Excel.Export(form.Get("FN") + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", dataTable1,
                            (dt) => {
                                //headerHandler: 返回Excel表首要顯示的文字
                                return string.Format(repo.GetExcelTitle(User.Identity.Name, d0, d1, showdata));
                            });
                    }
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