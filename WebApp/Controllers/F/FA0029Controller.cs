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
    public class FA0029Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            
            var p1 = form.Get("p1");
            
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0029Repository(DBWork);
                    session.Result.etts = repo.GetAll(p1, page, limit, sorters);
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
            var YM = form.Get("P1");
          
            var title = "";
          
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0029Repository(DBWork);

                    string str_UserDept = DBWork.UserInfo.InidName;
                    title = str_UserDept + YM + "申領品項總材積明細.xls";
                    //using (var dataTable1 = repo.GetExcel(YM))
                    //{
                    //    JCLib.Excel.Export(title, dataTable1);
                    //}
                    JCLib.Excel.Export("中央庫房" + "" +YM + "申領品項總材積明細.xls", repo.GetExcel(YM),
                         (dt) =>
                         {
                             return string.Format("中央庫房" + "" +YM + "申領品項總材積明細", DBWork.UserInfo.InidName, "");
                         });
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

    }
}