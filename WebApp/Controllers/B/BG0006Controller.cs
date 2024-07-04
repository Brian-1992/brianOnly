using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BG;
using WebApp.Models;
using WebApp.Repository.BC;

namespace WebApp.Controllers.BG
{
    public class BG0006Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var P0 = form.Get("P0");    //物料分類
            var P1 = form.Get("P1");    //進貨日期起
            var P2 = form.Get("P2");    //合約種類
            //var P3 = form.Get("P3");    //合約碼
            var P4 = form.Get("P4");    //進貨日期訖
            var P5 = form.Get("P5");    //院內碼
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0006Repository(DBWork);
                    session.Result.etts = repo.GetAll(P0, P1, P2, P4, P5, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse All_2(FormDataCollection form)
        {   // 匯總
            var P0 = form.Get("P20");    //物料分類
            var P1 = form.Get("P21");    //進貨日期起
            var P2 = form.Get("P22");    //合約種類
            //var P3 = form.Get("P3");    //合約碼
            var P4 = form.Get("P24");    //進貨日期訖
            var P5 = form.Get("P25");    //院內碼
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0006Repository(DBWork);
                    session.Result.etts = repo.GetAll_2(P0, P1, P2, P4, P5, page, limit, sorters);
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
            var P0 = form.Get("P0");    //物料分類
            var P1 = form.Get("P1");    //訂單年月
            var P2 = form.Get("P2");    //合約種類
            //var P3 = form.Get("P3");    //合約碼
            var P4 = form.Get("P4");    //進貨日期訖
            var P5 = form.Get("P5");    //院內碼

            if (!string.IsNullOrWhiteSpace(P1))
            {
                P1 = (int.Parse(P1.Substring(0, P1.Length < 7 ? 2 : 3)) + 1911).ToString() + "-" + P1.Substring(P1.Length < 7 ? 2 : 3, 2) + "-" + P1.Substring(P1.Length < 7 ? 4 : 5, 2);
            };


            if (!string.IsNullOrWhiteSpace(P4))
            {
                P4 = (int.Parse(P4.Substring(0, P4.Length < 7 ? 2 : 3)) + 1911).ToString() + "-" + P4.Substring(P4.Length < 7 ? 2 : 3, 2) + "-" + P4.Substring(P4.Length < 7 ? 4 : 5, 2);
            };


            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0006Repository repo = new BG0006Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(P0, P1, P2, P4, P5));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse Excel_2(FormDataCollection form)
        {
            var P0 = form.Get("P20");    //物料分類
            var P1 = form.Get("P21");    //訂單年月
            var P2 = form.Get("P22");    //合約種類
            //var P3 = form.Get("P3");    //合約碼
            var P4 = form.Get("P24");    //進貨日期訖
            var P5 = form.Get("P25");    //院內碼

            if (!string.IsNullOrWhiteSpace(P1))
            {
                P1 = (int.Parse(P1.Substring(0, P1.Length < 7 ? 2 : 3)) + 1911).ToString() + "-" + P1.Substring(P1.Length < 7 ? 2 : 3, 2) + "-" + P1.Substring(P1.Length < 7 ? 4 : 5, 2);
            };


            if (!string.IsNullOrWhiteSpace(P4))
            {
                P4 = (int.Parse(P4.Substring(0, P4.Length < 7 ? 2 : 3)) + 1911).ToString() + "-" + P4.Substring(P4.Length < 7 ? 2 : 3, 2) + "-" + P4.Substring(P4.Length < 7 ? 4 : 5, 2);
            };


            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0006Repository repo = new BG0006Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel_2(P0, P1, P2, P4, P5));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse GetMatCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0006Repository(DBWork);
                    //session.Result.etts = repo.GetMatCombo(User.Identity.Name);
                    session.Result.etts = repo.GetMatCombo();
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