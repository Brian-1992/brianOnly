using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;

namespace WebApp.Controllers.UR
{
    public class UR1005Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection f)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_MENURepository(DBWork);
                    session.Result.etts = repo.GetMenuAll(f.Get("PG"));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Append(UR_MENU ur_menu)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_MENURepository(DBWork);
                    if (!repo.CheckExists(ur_menu.FG))
                    {
                        ur_menu.FS = repo.GetNextFS(ur_menu.PG);
                        session.Result.afrs = repo.Create(ur_menu);
                        session.Result.etts = repo.Get(ur_menu.FG);
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>程式代碼</span> 重複，請重新輸入。";
                        DBWork.Rollback();
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

        [HttpPost]
        public ApiResponse Insert(UR_MENU ur_menu)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_MENURepository(DBWork);
                    if (!repo.CheckExists(ur_menu.FG))
                    {
                        ur_menu.FS--;
                        session.Result.afrs = repo.Create(ur_menu);
                        session.Result.etts = repo.Get(ur_menu.FG);
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>程式代碼</span> 重複，請重新輸入。";
                        DBWork.Rollback();
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

        [HttpPost]
        public ApiResponse Update(UR_MENU ur_menu)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_MENURepository(DBWork);
                    session.Result.afrs = repo.Update(ur_menu);
                    session.Result.etts = repo.Get(ur_menu.FG);

                    var repo2 = new UR_UploadRepository(DBWork);
                    repo2.Confirm(ur_menu.ATTACH_URL);

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
        public ApiResponse UpdateFS(UR_MENU ur_menu)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_MENURepository(DBWork);
                    session.Result.afrs = repo.UpdateFS(ur_menu);
                    session.Result.etts = repo.Get(ur_menu.FG);

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
        public ApiResponse Delete(UR_MENU ur_menu)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    //把附件刪除
                    var repoUpload = new UR_UploadRepository(DBWork);
                    repoUpload.DeleteByUK(ur_menu.ATTACH_URL);

                    //從表單主檔刪除功能項目
                    var repo = new UR_MENURepository(DBWork);
                    session.Result.afrs = repo.Delete(ur_menu.FG);

                    //把此功能項目的權限都刪除 (群組)
                    var repo2 = new UR_TACLRepository(DBWork);
                    session.Result.afrs += repo2.DeleteByFG(ur_menu.FG);

                    //把此功能項目的權限都刪除 (個人)
                    var repo3 = new UR_TACL2Repository(DBWork);
                    session.Result.afrs += repo3.DeleteByFG(ur_menu.FG);

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

        [HttpGet, HttpPost]
        public ApiResponse GetMenu(FormDataCollection f)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_MENURepository(DBWork);
                    session.Result.etts = repo.GetMenuByUser(f.Get("PG"), f.Get("p0"));
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
