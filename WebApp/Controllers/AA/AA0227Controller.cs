using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models.D;
using System;

namespace WebApp.Controllers.AA
{
    public class AA0227Controller : SiteBase.BaseApiController
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

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0227Repository(DBWork);
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
        public ApiResponse Create(DGMISS_MAST dgmmiss_mast)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0227Repository(DBWork);
                    if (!repo.CheckExists(dgmmiss_mast.INID,dgmmiss_mast.MMCODE)) // 新增前檢查主鍵是否已存在
                    {
                        dgmmiss_mast.CREATE_USER = User.Identity.Name;
                        dgmmiss_mast.UPDATE_USER = User.Identity.Name;
                        dgmmiss_mast.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(dgmmiss_mast);
                        session.Result.etts = repo.Get(dgmmiss_mast.INID, dgmmiss_mast.MMCODE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "資料重複!";
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
        
        // 刪除
        [HttpPost]
        public ApiResponse Delete(DGMISS_MAST dgmmiss_mast)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0227Repository(DBWork);

                    var v_num = repo.CheckDgmissNum(dgmmiss_mast.INID, dgmmiss_mast.MMCODE);
                    if (v_num > 0)
                    {

                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "欲刪除責任中心+院內碼於補藥通知單未完成，不可删除!";
                    }
                    else
                    {
                        session.Result.afrs = repo.Delete(dgmmiss_mast.INID, dgmmiss_mast.MMCODE);
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

        [HttpPost]
        public ApiResponse GetInidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0227Repository repo = new AA0227Repository(DBWork);
                    session.Result.etts = repo.GetInidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0227Repository repo = new AA0227Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

    }
}