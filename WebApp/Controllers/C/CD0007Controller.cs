using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using Newtonsoft.Json;

namespace WebApp.Controllers.C
{
    public class CD0007Controller : SiteBase.BaseApiController
    {


        public ApiResponse QueryD(FormDataCollection form)   //SELECT * FROM UR_ID where INID tab1  
        {
            var p0 = "";
            var p1 = "";
            var p2 = "";
            var p3 = "";

            p0 = form.Get("p0");
            p1 = form.Get("p1");
            p2 = form.Get("p2");
            p3 = form.Get("p3");

            //p2 = form.Get("p2");

            //if (p2 == "userpopwindow")
            //{
            //    p0 = form.Get("p0");
            //    p3 = form.Get("p3");
            //}
            //else
            //{
            //    p0 = form.Get("p0");
            //    p1 = form.Get("p1");
            //}

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0007Repository(DBWork);

                    session.Result.etts = repo.GetPickDetail(p0, p1, p2, p3, page, limit, sorters);
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

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0007Repository repo = new CD0007Repository(DBWork);
                    session.Result.etts = repo.GetWH_NoCombo(p0, page, limit, "");
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
