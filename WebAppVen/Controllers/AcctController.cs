using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Formatting;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Security;
using JCLib.DB;
using JCLib.DB.Impl;
using WebAppVen.Repository.UR;

namespace SiteBase.Controllers
{
    //api/acct
    public class AcctController : ApiController
    {
        [HttpPost]
        public ApiResponse Info(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                UR_IDRepository repo = new UR_IDRepository(DBWork);
                session.Result.etts = repo.GetInfo(User.Identity.Name);

                return session.Result;
            }
        }
        
        [HttpPost]
        public ApiResponse Reset(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string mailSubject = "密碼重設通知函";
                    string mailContent = @"您好,<br/>
                    這封信是由 <a href='https://mmsms.ndmctsgh.edu.tw/'>三軍總醫院-藥品及衛材供應管理系統</a> 所寄送的會員密碼重設信件。<br/>
                    <br/>
                    您收到這封信件，是因為您在 <a href='https://mmsms.ndmctsgh.edu.tw/'>三軍總醫院-藥品及衛材供應管理系統</a> 要求會員密碼重設。<br/>
                    <br/>
                    ----------------------------------------<br/>
                    會員密碼重設說明<br/>
                    ----------------------------------------<br/>
                    <br/>
                    您是 <a href='https://mmsms.ndmctsgh.edu.tw/'>三軍總醫院-藥品及衛材供應管理系統</a> 的使用者，系統已接受您的密碼重設申請。<br/>
                    於是系統將寄送一組自動產生的新密碼，以方便您再次使用。<br/>
                    <br/>
                    請連結至 <a href='https://mmsms.ndmctsgh.edu.tw/'>三軍總醫院-藥品及衛材供應管理系統</a>，輸入您的帳號及下方我們所提供的新密碼：<br/>
                    <br/>
                    您的新密碼如下：<br/>
                    <br/>
                    <span style='color:red;font-weight:bold;'>{0}</span><br/>
                    <br/>
                    (請妥善保管您的密碼，或以新密碼登入後立即變更您的密碼以增加安全性。)<br/>";
                    UR_IDRepository repo = new UR_IDRepository(DBWork);
                    bool result = repo.ResetPassword(form.Get("UID"), mailSubject, mailContent);
                    if (result)
                    {
                        DBWork.Commit();
                    }
                    else
                    {
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

        // 從engSettings.config取得是正式機或測試機
        [HttpPost]
        public ApiResponse getDbConnType()
        {
            using (WorkSession session = new WorkSession())
            {
                if (JCLib.Util.GetEnvSetting("DB_CONN_TYPE") == "TEST")
                {
                    session.Result.msg = "TEST";
                }
                else
                {
                    session.Result.msg = "OFFICIAL";
                }

                return session.Result;
            }
        }

    }
}
