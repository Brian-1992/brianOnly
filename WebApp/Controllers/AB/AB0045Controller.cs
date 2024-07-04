using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System;

namespace WebApp.Controllers.AB
{
    public class AB0045Controller : SiteBase.BaseApiController  
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
                    var repo = new AB0045Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters);
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
        public ApiResponse Create(ME_CSTM ME_CSTM)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0045Repository(DBWork);
                    if (!repo.CheckExists(ME_CSTM.CSTM)) // 新增前檢查主鍵是否已存在  
                    {
                        ME_CSTM.CREATE_ID = User.Identity.Name;
                        ME_CSTM.UPDATE_ID = User.Identity.Name;
                        ME_CSTM.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(ME_CSTM); 
                        session.Result.etts = repo.Get(ME_CSTM.CSTM);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'> 扣庫時點</span> 重複，請重新輸入。";
                    }

                    DBWork.Commit();
                }
                catch (Exception ex)
                {
                    DBWork.Rollback();
                    throw ex;
                }
                return session.Result;
            }
        }

        // 修改
        [HttpPost]
        public ApiResponse Update(FormDataCollection form)
        {
            {
                using (WorkSession session = new WorkSession(this))
                {
                    var oldp0 = form.Get("p0");
                    
                    var newp0 = form.Get("newp0");
                 
                    var DBWork = session.UnitOfWork;
                    DBWork.BeginTransaction();
                    try
                    {
                        var repo = new AB0045Repository(DBWork);
                        if (oldp0 != newp0)
                        {
                            if (!repo.CheckExistsModify(newp0))
                            {
                                var update_user = User.Identity.Name;
                                var update_ip = DBWork.ProcIP;
                                session.Result.afrs = repo.Update(oldp0, newp0,update_user, update_ip); 
                                session.Result.etts = repo.Get(newp0);
                                DBWork.Commit();
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'> 扣庫時點</span> 重複，請重新輸入。";
                            }
                        }
                        else
                        {
                            session.Result.etts = repo.Get(oldp0);
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
        }

        // 刪除
        [HttpPost]
        public ApiResponse Delete(ME_CSTM ME_CSTM)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0045Repository(DBWork);
                    if (repo.CheckExists(ME_CSTM.CSTM))
                    {
                        session.Result.afrs = repo.Delete(ME_CSTM);
                    }
                    else
                    {
                        //session.Result.afrs = 0;
                        //session.Result.success = false;
                        //session.Result.msg = "<span style='color:red'>廠商碼</span>不存在。";
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

    }
}