using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AB0063Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetWH(FormDataCollection form)
        {
            var v_tpe = "N";
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0063Repository repo = new AB0063Repository(DBWork);
                    var v_inid = repo.GetUridInid(User.Identity.Name);
                    if (repo.Checkwhno(v_inid))
                    {
                        v_tpe = "Y";
                    }
                    session.Result.etts = repo.GetWH(v_tpe).Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMatClass(FormDataCollection form)
        {
            var vWhNo = form.Get("whNo");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0063Repository repo = new AB0063Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo(vWhNo);
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢
        [HttpPost]
        public ApiResponse GetQueryData(FormDataCollection form)
        {
            var matClass = form.Get("matClass");
            var fromYM = form.Get("dataYMFrom");
            var toYM = form.Get("dataYMTo");
            var whNo = form.Get("whNo");
            var mmCode = form.Get("mmCode");
            var docType = form.Get("docType");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0072Repository(DBWork);
                    session.Result.etts = repo.GetQueryData("js", matClass, fromYM, toYM, whNo, mmCode, docType, page, limit, sorters, "AB0063"); //撈出object
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
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
                    var repo = new AB0063Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
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