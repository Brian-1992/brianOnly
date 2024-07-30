using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.G;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Controllers.G
{
    public class GA0004Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0004Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 新增
        [HttpPost]
        public ApiResponse Create(TC_PURUNCOV tc_puruncov)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0004Repository(DBWork);
                    if (!repo.CheckExists(tc_puruncov)) // 新增前檢查主鍵是否已存在
                    {
                        tc_puruncov.CREATE_USER = User.Identity.Name;
                        tc_puruncov.UPDATE_USER = User.Identity.Name;
                        tc_puruncov.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(tc_puruncov);
                        session.Result.etts = repo.Get(tc_puruncov);

                        ReCalculate(DBWork);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>單位劑量 " + tc_puruncov.PUR_UNIT + " ，計量單位 " + tc_puruncov.BASE_UNIT + " 已建檔。</span>，請重新輸入。";
                    }

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        // 修改
        [HttpPost]
        public ApiResponse Update(TC_PURUNCOV tc_puruncov)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0004Repository(DBWork);
                    tc_puruncov.UPDATE_USER = User.Identity.Name;
                    tc_puruncov.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(tc_puruncov);
                    session.Result.etts = repo.Get(tc_puruncov);

                    ReCalculate(DBWork);

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

       // 刪除
       [HttpPost]
        public ApiResponse Delete(TC_PURUNCOV tc_puruncov)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0004Repository(DBWork);
                    if (repo.CheckExists(tc_puruncov))
                    {
                        session.Result.afrs = repo.Delete(tc_puruncov);

                        ReCalculate(DBWork);

                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>單位劑量 " + tc_puruncov.PUR_UNIT + " ，計量單位 " + tc_puruncov.BASE_UNIT + " </span>不存在。";
                    } 
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }



        public int ReCalculate(UnitOfWork DBWork)
        {
            try
            {
                GA0001Controller ctrl = new GA0001Controller();
                GA0001Repository repo = new GA0001Repository(DBWork);

                IEnumerable<TC_INVQMTR> list = repo.GetInvqmtrs();
                repo.DeleteInvqmtr();

                return ctrl.CalculateRCM(list, DBWork);
            }
            catch
            {
                DBWork.Rollback();
                throw;
            }
        }
    }
}