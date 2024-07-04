using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebAppVen.Repository.UR;
using WebAppVen.Models;

namespace WebAppVen.Controllers.UR
{
    public class UR1002Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public void Excel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    //JCLib.Excel.Export(form.Get("FN"), repo.GetExcel2(form.Get("TS")), 
                    //    new string[] { "帳號", "部門", "使用者名" });

                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(form.Get("TS")));
                }
                catch
                {
                    throw;
                }
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

        [HttpPost]
        public ApiResponse Create(UR_ID ur_id)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    if (!repo.CheckExists(ur_id))
                    {
                        ur_id.CREATE_USER = User.Identity.Name;
                        ur_id.UPDATE_USER = User.Identity.Name;
                        ur_id.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.Create(ur_id);

                        session.Result.etts = repo.Get(ur_id.TUSER);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>使用者帳號</span> 重複，請重新輸入。";
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
        public ApiResponse Update(UR_ID ur_id)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    session.Result.afrs = repo.Update(ur_id);
                    session.Result.etts = repo.Get(ur_id.TUSER);

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
        public ApiResponse Delete(UR_ID ur_id)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    //刪除群組使用者關聯檔
                    var repo1 = new UR_UIRRepository(DBWork);
                    session.Result.afrs += repo1.DeleteByUser(ur_id.TUSER);
                    //刪除使用者基本檔
                    var repo = new UR_IDRepository(DBWork);
                    session.Result.afrs += repo.Delete(ur_id.TUSER);

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
        public ApiResponse ResetPassword(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    var afrs = repo.UpdatePassword(form.Get("TUSER"), form.Get("TUSER"));
                    session.Result.afrs = afrs;
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
        public ApiResponse Reset(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var mailSubject = "密碼重設通知函";
                    var mailContent = @"您好,<br/>
                    這封信是由 <a href='https://mmsms.ndmctsgh.edu.tw/'>三軍總醫院-藥品及衛材供應管理系統</a> 所寄送的會員密碼重設信件。<br/>
                    <br/>
                    您收到這封信件，是因為系統已將您的密碼重設。<br/>
                    <br/>
                    ----------------------------------------<br/>
                    會員密碼重設說明<br/>
                    ----------------------------------------<br/>
                    <br/>
                    您是 <a href='https://mmsms.ndmctsgh.edu.tw/'>三軍總醫院-藥品及衛材供應管理系統</a> 的使用者，系統管理員已接受您的密碼重設申請。<br/>
                    所以系統將寄送一組自動產生的新密碼，以方便您再次使用。<br/>
                    <br/>
                    請連結至 <a href='https://mmsms.ndmctsgh.edu.tw/'>三軍總醫院-藥品及衛材供應管理系統</a>，輸入您的帳號及下方我們所提供的新密碼：<br/>
                    <br/>
                    您的新密碼如下：<br/>
                    <br/>
                    <span style='color:red;font-weight:bold;'>{0}</span><br/>
                    <br/>
                    (請妥善保管您的密碼，或以新密碼登入後立即變更您的密碼以增加安全性。)<br/>";
                    var repo = new UR_IDRepository(DBWork);
                    var msg = repo.ResetPasswordByAdmin(form.Get("TUSER"), mailSubject, mailContent);
                    if (msg == "")
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.msg = msg;
                        session.Result.success = false;
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
        public ApiResponse EncryptAll(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    var afrs = repo.EncryptAll();
                    session.Result.afrs = afrs;
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
        public ApiResponse Change(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var old_pwd = form.Get("OLD_PWD");
                    var new_pwd = form.Get("NEW_PWD");
                    var ur_pwd = new UR_PWD(User.Identity.Name)
                    {
                        OLD_PWD = old_pwd,
                        NEW_PWD = new_pwd
                    };
                    var repo = new UR_IDRepository(DBWork);
                    session.Result.afrs = repo.Change(ur_pwd);

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
        public ApiResponse GetEncBtn(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_IDRepository repo = new UR_IDRepository(DBWork);
                    session.Result.etts = repo.GetEncBtn(DBWork.ProcUser);
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
