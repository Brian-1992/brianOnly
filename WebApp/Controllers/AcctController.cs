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
using WebApp.Repository.UR;

namespace SiteBase.Controllers
{
    //api/acct
    public class AcctController : ApiController
    {
        [HttpPost]
        public ApiResponse Slide(FormDataCollection form)
        {
            return new ApiResponse();
        }

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

        // 從engSettings.config取得設定後決定畫面配置
        [HttpPost]
        public ApiResponse getEnvConfig()
        {
            ApiResponse result = new ApiResponse();
            string rtnStr = "";
            // 是否是804醫院(國軍桃園總院),是[Y]則使用API登入
            if (JCLib.Util.GetEnvSetting("804_LOGIN") == "Y")
            {
                rtnStr += "Y^";
            }
            else
            {
                rtnStr += "N^";
            }
            // 是否有設定AD註冊網站,有則顯示訊息,無則顯示預設訊息
            if (JCLib.Util.GetEnvSetting("AD_WEBSITE") != null)
            {
                rtnStr += JCLib.Util.GetEnvSetting("AD_WEBSITE");
            }
            else
            {
                rtnStr += "http://webad/apply/";
            }
            // 是否是813醫院(國軍新竹醫院),是[Y]則使用HISDB登入
            if (JCLib.Util.GetEnvSetting("813_LOGIN") == "Y")
            {
                rtnStr += "^Y";
            }
            else
            {
                rtnStr += "^N";
            }
            result.msg = rtnStr;

            return result;
        }

        // 從engSettings.config取得是正式機或測試機
        [HttpPost]
        public ApiResponse getDbConnType()
        {
            ApiResponse result = new ApiResponse();
            if (JCLib.Util.GetEnvSetting("DB_CONN_TYPE") == "TEST")
            {
                result.msg = "TEST";
            }
            else
            {
                result.msg = "OFFICIAL";
            }

            return result;
        }
    }
}
