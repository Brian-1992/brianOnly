using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;

namespace WebApp.Controllers.C
{
    public class CB0008Controller : SiteBase.BaseApiController
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0008Repository(DBWork);
                    session.Result.etts = repo.GetAll2(p0, p1, p2, p3, p4, p5, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        /// <summary>
        /// 讀取庫房別
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ApiResponse GetWHNOCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    string User_Inid = DBWork.UserInfo.Inid;
                    CB0008Repository repo = new CB0008Repository(DBWork);
                    session.Result.etts = repo.GetWHNOCombo(User_Inid);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse GetCLSNAME()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    string User_Inid = DBWork.UserInfo.Inid;
                    CB0008Repository repo = new CB0008Repository(DBWork);
                    session.Result.etts = repo.GetCLSNAME(User_Inid);
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