using System;
using System.Configuration;
using System.Net.Mail;

namespace JCLib
{
    public class Mail
    {
        public static string Send(string pSubject, string pMsg,string pTo,  string pCC = "")
        {
            pTo = pTo ?? "";
            if (pTo == "") return "無效的收件者";

            string rtnMessage = "";
            string smtpServer = JCLib.Util.GetEnvSetting("SMTP_SERV");
            SmtpClient smtp = new SmtpClient(smtpServer);
            MailMessage mail = new MailMessage();

            string sysFrom = JCLib.Util.GetEnvSetting("SMTP_SYS_FROM");
            string sysName = JCLib.Util.GetEnvSetting("SMTP_SYS_NAME");
            mail.From = new MailAddress(sysFrom, sysName);
            string strTo = pTo.Replace(',', ';');
            if (strTo.IndexOf(';') > -1)
            {
                string[] arrAddr = strTo.Split(';');
                foreach(string addr in arrAddr)
                    if(addr != "")
                        mail.To.Add(new MailAddress(addr));
            }
            else
            {
                mail.To.Add(new MailAddress(strTo));
            }

            pCC = pCC ?? "";
            if (pCC != "")
            {
                string strCC = pCC.Replace(',', ';');
                if (strCC.IndexOf(';') > -1)
                {
                    string[] arrAddr = strCC.Split(';');
                    foreach (string addr in arrAddr)
                        if (addr != "")
                            mail.CC.Add(new MailAddress(addr));
                }
                else
                {
                    mail.CC.Add(new MailAddress(strCC));
                }
            }

            mail.Body = pMsg;
            mail.Subject = pSubject;
            mail.IsBodyHtml = true;

            try
            {
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                rtnMessage = ex.Message;
            }

            return rtnMessage;
        }
    }
}
