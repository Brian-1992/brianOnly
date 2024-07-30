using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System;

namespace WebApp.Controllers.C
{
    public class CB0005Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0005Repository(DBWork);
                    if (p0 != "")
                    {
                        session.Result.etts = repo.GetAll(p0, p1, p2, page, limit, sorters);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房別不可為空</span>，請重新輸入。";
                    }

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
        public ApiResponse Create(BC_MANAGER bc_manager)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CB0005Repository(DBWork);
                    if (!repo.CheckExists(bc_manager.WH_NO, bc_manager.MANAGERID)) // 新增前檢查主鍵是否已存在
                    {
                        bc_manager.CREATE_USER = User.Identity.Name;
                        bc_manager.UPDATE_USER = User.Identity.Name;
                        bc_manager.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(bc_manager);
                        session.Result.etts = repo.Get(bc_manager.WH_NO, bc_manager.MANAGERID);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房別及管理人員代號</span>重複，請重新輸入。";
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
        public ApiResponse Update(BC_MANAGER bc_manager)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CB0005Repository(DBWork);
                    bc_manager.UPDATE_USER = User.Identity.Name;
                    bc_manager.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(bc_manager);
                    session.Result.etts = repo.Get(bc_manager.WH_NO, bc_manager.MANAGERID);

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
        public ApiResponse Delete(BC_MANAGER bc_manager)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CB0005Repository(DBWork);
                    if (repo.CheckExists(bc_manager.WH_NO, bc_manager.MANAGERID))
                    {
                        session.Result.afrs = repo.Delete(bc_manager.WH_NO, bc_manager.MANAGERID);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房別及管理人員代號</span>不存在。";
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

        public ApiResponse GetWhno()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0005Repository(DBWork);
                    session.Result.etts = repo.GetWhno(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse GetUserId()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0005Repository(DBWork);
                    session.Result.etts = repo.GetUserId(User.Identity.Name);
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
            var WH_NO = form.Get("P0");
            var MANAGERID = form.Get("P1");
            var MANAGERNM = form.Get("P2");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CB0005Repository repo = new CB0005Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    JCLib.Excel.Export("品項管理員資料表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(WH_NO, MANAGERID, MANAGERNM));
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