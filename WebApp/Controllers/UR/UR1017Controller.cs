using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;

namespace WebApp.Controllers.UR
{
    public class UR1017Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetByUser(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_RoleRepository(DBWork);
                    session.Result.etts = repo.GetByUser(p0, p1, DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Create(UR_ROLE ur_role)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_RoleRepository(DBWork);
                    if (!repo.CheckExists(ur_role))
                    {
                        ur_role.ROLE_CREATE_BY = User.Identity.Name;
                        session.Result.afrs = repo.Create(ur_role);
                        session.Result.etts = repo.Get(ur_role.RLNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>群組代碼</span> 重複，請重新輸入。";
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
        public ApiResponse Copy(UR_ROLE ur_role)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_RoleRepository(DBWork);
                    if (!repo.CheckExists(ur_role))
                    {
                        ur_role.ROLE_CREATE_BY = User.Identity.Name;
                        session.Result.afrs = repo.Create(ur_role);
                        if (session.Result.afrs > 0)
                        {
                            var repo2 = new UR_TACLRepository(DBWork);
                            repo2.Copy(ur_role.RLNO1, ur_role.RLNO);
                        }
                        session.Result.etts = repo.Get(ur_role.RLNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>群組代碼</span> 重複，請重新輸入。";
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
        public ApiResponse Update(UR_ROLE ur_role)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ur_role.ROLE_MODIFY_BY = User.Identity.Name;
                    var repo = new UR_RoleRepository(DBWork);
                    session.Result.afrs = repo.Update(ur_role);
                    session.Result.etts = repo.Get(ur_role.RLNO);

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
        public ApiResponse Delete(UR_ROLE ur_role)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    //刪除群組功能關聯檔
                    var repo = new UR_TACLRepository(DBWork);
                    repo.DeleteByRole(ur_role.RLNO);
                    //刪除群組使用者關聯檔
                    var repo1 = new UR_UIRRepository(DBWork);
                    session.Result.afrs += repo1.DeleteByRole(ur_role.RLNO);
                    //刪除群組基本檔
                    var repo2 = new UR_RoleRepository(DBWork);
                    session.Result.afrs += repo2.Delete(ur_role.RLNO);

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
