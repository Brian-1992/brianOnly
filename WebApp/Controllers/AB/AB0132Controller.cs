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
    public class AB0132Controller : SiteBase.BaseApiController
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
            var p8 = form.Get("p8");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            var menuLink = form.Get("menuLink");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0132Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, p6, p7,p8, page, limit, sorters, menuLink);
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
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var menuLink = form.Get("menuLink");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0132Repository repo = new AB0132Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7,p8, menuLink));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel2(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var menuLink = form.Get("menuLink");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0132Repository repo = new AB0132Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel2(p0, p1, p2, p3, menuLink));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //藥品代碼combo
        [HttpPost]
        public ApiResponse GetOrdercodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //院內碼smartquery
            var page = int.Parse(form.Get("page"));
            var limit = int.Parse(form.Get("limit"));
            var menuLink = form.Get("menuLink") ==null? "": form.Get("menuLink"); //院內碼smartquery


            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0132Repository repo = new AB0132Repository(DBWork);
                    session.Result.etts = repo.GetOrdercodeCombo(p0, page, limit, "", menuLink);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //科別代碼combo
        [HttpPost]
        public ApiResponse GetSectionNoCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0132Repository repo = new AB0132Repository(DBWork);
                    session.Result.etts = repo.GetSectionNoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetSetBTime(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0132Repository(DBWork);
                    session.Result.msg = repo.GetSetBTime();

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //科別代碼combo
        [HttpPost]
        public ApiResponse GetWhnoCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0132Repository repo = new AB0132Repository(DBWork);
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