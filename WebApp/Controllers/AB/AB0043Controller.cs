using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;

namespace WebApp.Controllers.AB
{ 
    public class AB0043Controller : SiteBase.BaseApiController
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
                    var repo = new AB0043Repository(DBWork);
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
        public ApiResponse Create(PHRSDPT PHRSDPT)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0043Repository(DBWork);
                    if (!repo.CheckExists(PHRSDPT.RXTYPE, PHRSDPT.RXDATEKIND,PHRSDPT.DEADLINETIME,PHRSDPT.WORKFLAG)) // 新增前檢查主鍵是否已存在
                    {
                        PHRSDPT.CREATE_USER = User.Identity.Name;
                        PHRSDPT.UPDATE_USER = User.Identity.Name;
                        PHRSDPT.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(PHRSDPT);
                        session.Result.etts = repo.Get(PHRSDPT.RXTYPE, PHRSDPT.RXDATEKIND);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'> 調劑類別及調劑日期類別</span> 重複，請重新輸入。";
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
        //public ApiResponse Update(PHRSDPT PHRSDPT)
        public ApiResponse Update(FormDataCollection form)
        {
            {
                using (WorkSession session = new WorkSession(this))
                {
                    var oldp0 = form.Get("p0");
                    var oldp1 = form.Get("p1");
                    var oldp2 = form.Get("p2");
                    var oldp3 = form.Get("p3");

                    var newp0 = form.Get("newp0");
                    var newp1 = form.Get("newp1");
                    var newp2 = form.Get("newp2");
                    var newp3 = form.Get("newp3");

                    var DBWork = session.UnitOfWork;
                    DBWork.BeginTransaction();
                    try
                    {
                        var repo = new AB0043Repository(DBWork);
                        if (oldp0 != newp0 || oldp1 != newp1 || oldp2 != newp2 || oldp3 != newp3)
                        {
                            if (!repo.CheckExistsModify(newp0, newp1, newp2, newp3))
                            {
                                var update_user = User.Identity.Name;
                                var update_ip = DBWork.ProcIP;
                                session.Result.afrs = repo.Update(oldp0, oldp1, oldp2, oldp3, newp0, newp1, newp2, newp3, update_user, update_ip);
                                session.Result.etts = repo.Get(newp0, newp1);
                                DBWork.Commit();
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'> 調劑類別及調劑日期類別</span> 重複，請重新輸入。";
                            }
                        }
                        else
                        {
                            session.Result.etts = repo.Get(oldp0, oldp1);
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
        public ApiResponse Delete(PHRSDPT PHRSDPT)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0043Repository(DBWork);
                    if (repo.CheckExists(PHRSDPT.RXTYPE,PHRSDPT.RXDATEKIND,PHRSDPT.DEADLINETIME,PHRSDPT.WORKFLAG))
                    {
                        session.Result.afrs = repo.Delete(PHRSDPT);
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