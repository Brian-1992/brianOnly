using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebAppVen.Repository.UR;
using WebAppVen.Models;

namespace WebAppVen.Controllers.UR
{
    public class UR1003Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_RoleRepository(DBWork);
                    session.Result.etts = repo.GetAllD(page, limit, p0, p1);
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
                        ur_role.ROLE_CREATE_BY = DBWork.UserInfo.UserId;
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
                        ur_role.ROLE_CREATE_BY = DBWork.UserInfo.UserId;
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
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo1 = new UR_UIRRepository(DBWork);
                    if (repo1.CheckUserInRole(ur_role.RLNO, User.Identity.Name))
                    {
                        var repo = new UR_RoleRepository(DBWork);
                        session.Result.afrs = repo.Update(ur_role);
                        session.Result.etts = repo.Get(ur_role.RLNO);
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = "您無權限異動此群組。";
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
        public ApiResponse Delete(UR_ROLE ur_role)
        {
            using (WorkSession session = new WorkSession())
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
                    session.Result.afrs += repo2.DeleteD(ur_role.RLNO);

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
