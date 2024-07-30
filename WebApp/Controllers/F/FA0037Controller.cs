using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System;
using System.Data;
using System.Web;
using System.IO;
namespace WebApp.Controllers.F
{
    public class FA0037Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0037Repository repo = new FA0037Repository(DBWork);
                    session.Result.etts = repo.GetMatClass();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            //var p7 = form.Get("p7");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0037Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, p6, page, limit, sorters);
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
            var MAT_CLASS = form.Get("P0");
            var DIS_TIME_B = form.Get("D0");
            var DIS_TIME_E = form.Get("D1");
            var YM = form.Get("P1");
            var GB = form.Get("P2");
            var CONT = form.Get("P3");
            var XACTION = form.Get("P4");
            var title = "";
            string mclsname = "";
            //var P3 = bool.Parse(form.Get("P3"));
            if (GB == "0")
            {
                if (CONT == "0")
                {
                    title = "庫備合約品項";
                }
                if (CONT == "2")
                {
                    title = "庫備非合約品項";
                }
                if (CONT == "全部")
                {
                    title = "庫備";
                }
            }
            else if (GB == "1")
            {
                if (CONT == "0")
                {
                    title = "非庫備合約品項";
                }
                if (CONT == "2")
                {
                    title = "非庫備非合約品項";
                }
                if (CONT == "全部")
                {
                    title = "非庫備";
                }
            }
            else if (GB == "全部")
            {
                if (CONT == "0")
                {
                    title = "合約品項";
                }
                if (CONT == "2")
                {
                    title = "非合約品項";
                }
                if (CONT == "全部")
                {
                    title = "";
                }
            }


            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0037Repository(DBWork);

                    mclsname = repo.MCLSNAME(MAT_CLASS);
                    title = "中央庫房" + mclsname + title + YM + "申購報表.xls";
                    using (var dataTable1 = repo.GetExcel(MAT_CLASS, DIS_TIME_B, DIS_TIME_E, YM, GB, CONT, XACTION))
                    {
                        JCLib.Excel.Export(title, dataTable1);
                    }
                    //JCLib.Excel.Export
                    //string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    //JCLib.Excel.Export("中藥訂購單" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(MAT_CLASS, DIS_TIME_B, DIS_TIME_E));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //匯出非庫備申請單
        [HttpPost]
        public ApiResponse Excel2(FormDataCollection form)
        {
            var MAT_CLASS = form.Get("P0");
            var DIS_TIME_B = form.Get("D0");
            var DIS_TIME_E = form.Get("D1");
            var YM = form.Get("P1");
            var GB = form.Get("P2");   //庫備分類
            var CONT = form.Get("P3"); //合約分類
            var XACTION = form.Get("P4");
            var title = "非庫備申請單";
            string mclsname = "";
            if (CONT == "0")
            {
                title += "-合約";
            }
            if (CONT == "2")
            {
                title += "-非合約";
            }
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0037Repository(DBWork);

                    mclsname = repo.MCLSNAME(MAT_CLASS);
                    title = "中央庫房" + mclsname + YM + title +".xls";
                    using (var dataTable1 = repo.GetExcel2(MAT_CLASS, DIS_TIME_B, DIS_TIME_E, YM, GB, CONT, XACTION))
                    {
                        JCLib.Excel.Export(title, dataTable1);
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