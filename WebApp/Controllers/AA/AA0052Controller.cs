using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0052Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
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
                    var repo = new AA0052Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters);
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
        //public ApiResponse Create(AA0052 tc_mmagen)
        //{
        //    using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new AA0052Repository(DBWork);
        //            if (!repo.CheckExists(tc_mmagen)) // 新增前檢查主鍵是否已存在
        //            {
        //                tc_mmagen.CREATE_USER = User.Identity.Name;
        //                tc_mmagen.UPDATE_USER = User.Identity.Name;
        //                tc_mmagen.UPDATE_IP = DBWork.ProcIP;
        //                session.Result.afrs = repo.Create(tc_mmagen);
        //                session.Result.etts = repo.Get(tc_mmagen);
        //            }
        //            else
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>電腦編號 "+ tc_mmagen .MMCODE+ " ，藥商名稱 "+ tc_mmagen.AGEN_NAMEC+ " 已建檔。</span>，請重新輸入。";
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

        // 修改
        [HttpPost]
        public ApiResponse Update(AA0052 aa0052)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0052Repository(DBWork);
                    aa0052.UPDATE_USER = User.Identity.Name;
                    aa0052.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(aa0052);
                    session.Result.etts = repo.Get(aa0052);

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

       //// 刪除
       //[HttpPost]
       // public ApiResponse Delete(TC_MMAGEN tc_mmagen)
       // {
       //     using (WorkSession session = new WorkSession())
       //     {
       //         var DBWork = session.UnitOfWork;
       //         DBWork.BeginTransaction();
       //         try
       //         {
       //             var repo = new AA0052Repository(DBWork);
       //             if (repo.CheckExists(tc_mmagen))
       //             {
       //                 session.Result.afrs = repo.Delete(tc_mmagen);
       //             }
       //             else
       //             {
       //                 session.Result.afrs = 0;
       //                 session.Result.success = false;
       //                 session.Result.msg = "<span style='color:red'>電腦編號 " + tc_mmagen.MMCODE + " ，藥商名稱 " + tc_mmagen.AGEN_NAMEC + " </span>不存在。";
       //             }

       //             DBWork.Commit();
       //         }
       //         catch
       //         {
       //             DBWork.Rollback();
       //             throw;
       //         }
       //         return session.Result;
       //     }
       // }
    }
}