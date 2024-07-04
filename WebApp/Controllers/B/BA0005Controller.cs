using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;

namespace WebApp.Controllers.B
{
    public class BA0005Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            //var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0005Repository(DBWork);
                    //session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, page, limit, sorters);
                    session.Result.etts = repo.GetAll(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //// 新增
        //[HttpPost]
        //public ApiResponse Create(PH_VENDER ph_vender)
        //{
        //    using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BA0005Repository(DBWork);
        //            if (!repo.CheckExists(ph_vender.AGEN_NO)) // 新增前檢查主鍵是否已存在
        //            {
        //                ph_vender.CREATE_USER = User.Identity.Name;
        //                ph_vender.UPDATE_USER = User.Identity.Name;
        //                ph_vender.UPDATE_IP = DBWork.ProcIP;
        //                session.Result.afrs = repo.Create(ph_vender);
        //                session.Result.etts = repo.Get(ph_vender.AGEN_NO);
        //            }
        //            else
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>廠商碼</span>重複，請重新輸入。";
        //            }

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //// 修改
        //[HttpPost]
        //public ApiResponse Update(PH_VENDER ph_vender)
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BA0005Repository(DBWork);
        //            ph_vender.UPDATE_USER = User.Identity.Name;
        //            ph_vender.UPDATE_IP = DBWork.ProcIP;
        //            session.Result.afrs = repo.Update(ph_vender);
        //            session.Result.etts = repo.Get(ph_vender.AGEN_NO);

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        // 刪除
        //[HttpPost]
        //public ApiResponse Delete(PH_VENDER ph_vender)
        //{
        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BA0005Repository(DBWork);
        //            if (repo.CheckExists(ph_vender.AGEN_NO))
        //            {
        //                session.Result.afrs = repo.Delete(ph_vender.AGEN_NO);
        //            }
        //            else
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>廠商碼</span>不存在。";
        //            }

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //取得物料分類下拉式選單
        public ApiResponse GetCLSNAME()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BA0005Repository(DBWork);
                    session.Result.etts = repo.GetCLSNAME();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CreateOrder(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");            

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BA0005Repository repo = new BA0005Repository(DBWork);
                    if (repo.CreateOrder(p0, p1, p2, User.Identity.Name, DBWork.ProcIP) == "000")
                    {
                        session.Result.msg = "<span style='color:blue'>申購單</span>產生<span style='color:red'>成功</span>！";
                        return session.Result;
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:blue'>申購單</span>產生<span style='color:red'>失敗</span>，發生錯誤。";                        
                        return session.Result;
                    }
                }
                catch
                {
                    throw;
                }                
            }
        }

    }
}