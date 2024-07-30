using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Web.Http;
using System.Net.Http.Formatting;
using JCLib.DB;

namespace WebApp.Controllers
{
    public class MailController : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse SendMail(FormDataCollection form)
        {
            string title = form.Get("title");
            string body = form.Get("body");
            string mailto = form.Get("mailto");
            string smtptyp = form.Get("smtptyp");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    String smtpServer = "";
                    if (smtptyp.Equals("AIDC"))
                        smtpServer = "tchsmtp2.aidc.com.tw";
                    else if (smtptyp.Equals("FOO"))
                        smtpServer = "10.1.1.59";

                    SmtpClient smtp = new SmtpClient(smtpServer);
                    MailMessage mail = new MailMessage();
                    if (smtptyp.Equals("FOO"))
                        smtp.Credentials = new System.Net.NetworkCredential("id", "password");

                    mail.From = new MailAddress("sys@ms.foo.com.tw", "SYS");
                    mail.To.Add(new MailAddress(mailto));
                    mail.Body = body;
                    mail.Subject = title;
                    mail.IsBodyHtml = true;

                    try
                    {
                        smtp.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine("Press any key to exit.");
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

            
        }
        [HttpPost]
        public ApiResponse Send_Mail(string title, string body, string mailto)
        {
            using (WorkSession session = new WorkSession())
            {
                string SMTP_SERV = JCLib.Util.GetEnvSetting("SMTP_SERV");
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    SmtpClient smtp = new SmtpClient(SMTP_SERV);
                    MailMessage mail = new MailMessage();

                    mail.From = new MailAddress("mmsms@ms.tsgh.ndmctsgh.edu.tw", "藥(衛)材系統");
                    mail.To.Add(new MailAddress(mailto));
                    mail.Body = body;
                    mail.Subject = title;
                    mail.IsBodyHtml = true;

                    try
                    {
                        smtp.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        //string rtnString = "";
                    }
                }
                catch
                {
                   // throw;
                }
                return session.Result;
            }


        }
    }
}