using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
namespace WebApp.Controllers.F
{
    public class FA0010Controller : SiteBase.BaseApiController
    {

        // FA0010 查詢
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            //var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {

                    var repo = new FA0010Repository(DBWork);
                    session.Result.afrs = repo.Delete(); //先刪除TMP_INVCTL_YR_RP table

                    var mysysdate = repo.GetSYSDATE(); //查詢目前系統時間
                    var rawsysdate = repo.GetRawSYSDATE(); //查詢目前系統時間(Raw Data)

                    session.Result.afrs = repo.Create(mysysdate, p0);         //新增到TMP_INVCTL_YR_RP table
                    session.Result.etts = repo.GetAll(mysysdate, page, limit); //撈出object
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetSt_YEAR(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0010Repository repo = new FA0010Repository(DBWork);
                    session.Result.etts = repo.GetSt_YEAR();
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