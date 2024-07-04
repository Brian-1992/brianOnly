using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System.Web.Security.AntiXss;

namespace WebApp.Controllers.B
{
    public class BD0002Controller : SiteBase.BaseApiController
    {
        // 查詢Master
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0002Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, p3, p4, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢Detail
        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0002Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        //產生臨時申購訂單
        public string CREATE_ORDER_0(FormDataCollection form)
        {
            var WH_NO = form.Get("WH_NO");
            var MAT_CLASS = form.Get("MAT_CLASS");
            var START_DATE = form.Get("START_DATE");
            var END_DATE = form.Get("END_DATE");
            var INID = "";
            var RET_CODE = "";

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0002Repository(DBWork);
                    INID = repo.GetINID(User.Identity.Name);
                    RET_CODE = repo.CALL_BD0002_0(START_DATE, END_DATE, INID, MAT_CLASS, User.Identity.Name, DBWork.ProcIP, WH_NO);
                }
                catch
                {
                    throw;
                }
                return RET_CODE;
            }
        }

        [HttpPost]
        //產生常態申購訂單
        public string CREATE_ORDER_1(FormDataCollection form)
        {
            var WH_NO = form.Get("WH_NO");
            var MAT_CLASS = form.Get("MAT_CLASS");
            var START_DATE = form.Get("START_DATE");
            var END_DATE = form.Get("END_DATE");
            var INID = "";
            var RET_CODE = "";

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0002Repository(DBWork);
                    INID = repo.GetINID(User.Identity.Name);
                    RET_CODE = repo.CALL_BD0002_1(START_DATE, END_DATE, INID, MAT_CLASS, User.Identity.Name, DBWork.ProcIP, WH_NO);
                }
                catch
                {
                    throw;
                }
                return AntiXssEncoder.HtmlEncode(RET_CODE, true);
            }
        }

        //庫房代碼combox
        public ApiResponse GetWH_NO()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0002Repository(DBWork);
                    session.Result.etts = repo.GetWH_NO(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //物料類別combox
        public ApiResponse GetMAT_CLASS()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0002Repository(DBWork);
                    session.Result.etts = repo.GetMAT_CLASS();
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