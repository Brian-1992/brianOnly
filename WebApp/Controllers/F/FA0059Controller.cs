using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WebApp.Controllers.F
{
    public class FA0059Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var pt = form.Get("pt");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0059Repository(DBWork);
                    var v_rep_time = pt;
                    if (form.Get("first") is null)
                    {
                    }
                    else
                    {
                        v_rep_time = repo.GetSysdate();
                        session.Result.afrs = repo.DeleteTMP();
                        session.Result.afrs = repo.InsertTMP(p0, v_rep_time);
                    }
                    session.Result.etts = repo.GetAllM(v_rep_time, page, limit, sorters);

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
            var pt = form.Get("pt");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0059Repository repo = new FA0059Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(pt));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        //匯出EXCEL3
        [HttpPost]
        public ApiResponse Excel3(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0059Repository repo = new FA0059Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel3(d0, d1));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出EXCEL4
        [HttpPost]
        public ApiResponse Excel4(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0059Repository repo = new FA0059Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel4(d0, d1));
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