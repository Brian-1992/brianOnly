using JCLib.DB;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web;
using WebApp.Models;
using WebApp.Repository.B;

namespace WebApp.Controllers.B
{
    public class BD0023Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");    // 訂單號碼
            var p1 = form.Get("p1");    // 物料分類
            var p2 = form.Get("p2");    // 合約識別碼

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort") == null ? "" : form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0023Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, page, limit, sorters); //撈出object
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse AllD(FormDataCollection form)
        {
            var p0 = form.Get("p0");    // 訂單號碼
            var p1 = form.Get("p1");    // 訂單號碼
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort") == null ? "" : form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0023Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, page, limit, sorters); //撈出object
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMatClassSubCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0023Repository(DBWork);
                    session.Result.etts = repo.GetMatClassSubCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        
        public ApiResponse GetMContidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0023Repository(DBWork);
                    session.Result.etts = repo.GetMContidCombo();
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