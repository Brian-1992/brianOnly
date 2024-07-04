using JCLib.DB.Tool;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Configuration;
using System.IO;

namespace BES001
{
    class SendMail
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.StartupPath + "\\Schedule.config");
        //fromMail-寄件人；toMail-收件人；toMailCC-收件人副本；mail_type-信件類別；toSubject-主旨；toContent-信件內容；create_user-建立人員
        public bool Send_Mail(string po_no, string fromMail, string toMail, string toMailCC, string mail_type, string toSubject, string toContent, string create_user, string toContentLog, List<string> attachmentNames)
        {
            bool rtn = true;
            CallDBtools calldbtools = new CallDBtools();
            string sendMailFlag = calldbtools.GetSendMailFlag();
            if (sendMailFlag == "Y")
            {
                try
                {
                    string serverName = calldbtools.GetServerName(); //主機名稱-AIDC_TEST:漢翔測試;AIDC_PRODUCTION:漢翔正式;TSGHMM_TEST:三總測試;TSGHMM_PRODUCTION:三總正式
                    string smtpAddress = calldbtools.GetSmtpServerIP(); //mail server IP
                    string emailDisplayName = calldbtools.GetDefaultMailSenderDisplayName(); //系統預設寄件者中文顯示
                    if (serverName.Contains("AIDC")) //漢翔主機
                    {
                        string emailFrom = fromMail;
                        //int portNumber = 66; //mail server port
                        //string userName = ""; //mail server 帳號
                        //string password = ""; //mail server 密碼
                        //bool enableSSL = true;

                        using (MailMessage mail = new MailMessage())
                        {
                            mail.From = new MailAddress(emailFrom, emailDisplayName);
                            mail.To.Add(toMail);
                            if (toMailCC.Trim() != "")
                                mail.CC.Add(toMailCC);
                            mail.Subject = toSubject;
                            mail.Body = toContent;
                            mail.IsBodyHtml = true; // Can set to false, if you are sending pure text.
                            foreach (string name in attachmentNames)
                            {
                                mail.Attachments.Add(new Attachment(name));
                            }
                            //mail.Attachments.Add(new Attachment("C:\\SomeFile.txt"));
                            //mail.Attachments.Add(new Attachment("C:\\SomeZip.zip"));

                            using (SmtpClient smtp = new SmtpClient(smtpAddress))
                            {
                                //smtp.Credentials = new System.Net.NetworkCredential(userName, password);
                                //smtp.EnableSsl = enableSSL;
                                //smtp.Timeout = 3000; //指定以毫秒為單位的逾時值。 預設值為 100,000 (100 秒)。 
                                smtp.Send(mail);
                            }
                        }
                    }
                    else //三總主機
                    {
                        if (serverName.Contains("TEST")) //測試主機
                        {
                            string emailFrom = fromMail;
                            int portNumber = 25; //mail server port
                            string userName = "dcs5688@mail.ndmctsgh.edu.tw"; //mail server 帳號
                            string password = "Jason1218@"; //mail server 密碼
                            //bool enableSSL = true;

                            using (MailMessage mail = new MailMessage())
                            {
                                mail.From = new MailAddress(emailFrom, emailDisplayName);
                                mail.To.Add(toMail);
                                if (toMailCC.Trim() != "")
                                    mail.CC.Add(toMailCC);
                                mail.Subject = toSubject;
                                mail.Body = toContent;
                                mail.IsBodyHtml = true; // Can set to false, if you are sending pure text.
                                foreach (string name in attachmentNames)
                                {
                                    mail.Attachments.Add(new Attachment(name));
                                }
                                //mail.Attachments.Add(new Attachment("C:\\SomeFile.txt"));
                                //mail.Attachments.Add(new Attachment("C:\\SomeZip.zip"));

                                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                                {
                                    smtp.Credentials = new System.Net.NetworkCredential(userName, password);
                                    //smtp.EnableSsl = enableSSL;
                                    //smtp.Timeout = 3000; //指定以毫秒為單位的逾時值。 預設值為 100,000 (100 秒)。 
                                    smtp.Send(mail);
                                }
                            }
                        }
                        else //正式主機
                        {
                            string emailFrom = fromMail;
                            int portNumber = 25; //mail server port
                            string userName = "dcs5688@mail.ndmctsgh.edu.tw"; //mail server 帳號
                            string password = "Jason1218@"; //mail server 密碼
                            //bool enableSSL = true;

                            using (MailMessage mail = new MailMessage())
                            {
                                mail.From = new MailAddress(emailFrom, emailDisplayName);
                                mail.To.Add(toMail);
                                if (toMailCC.Trim() != "")
                                    mail.CC.Add(toMailCC);
                                mail.Subject = toSubject;
                                mail.Body = toContent;
                                mail.IsBodyHtml = true; // Can set to false, if you are sending pure text.
                                foreach (string name in attachmentNames)
                                {
                                    mail.Attachments.Add(new Attachment(name));
                                }
                                //mail.Attachments.Add(new Attachment("C:\\SomeFile.txt"));
                                //mail.Attachments.Add(new Attachment("C:\\SomeZip.zip"));

                                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                                {
                                    smtp.Credentials = new System.Net.NetworkCredential(userName, password);
                                    //smtp.EnableSsl = enableSSL;
                                    //smtp.Timeout = 3000; //指定以毫秒為單位的逾時值。 預設值為 100,000 (100 秒)。 
                                    smtp.Send(mail);
                                }
                            }
                        }
                    }
                    SaveMailLog(po_no, toMail, toSubject, toContent);
                }
                catch (Exception ex)
                {
                    rtn = false;
                    SaveMailLog("失敗" + po_no, toMail, toSubject, toContent);
                    calldbtools.WriteExceptionLog(ex.Message, "Send Mail Fail, 主旨為 " + toSubject, toContent);
                }
            }
            return rtn;
        }

        //計算中英文字串的混合長度,超過長度限制就砍掉不要 
        public string truncate_iptstr(string str1, int limit_len)
        {
            string hopestring = "";
            string spp = "";
            int byteLenght;
            if (str1 != null && str1 != "" && limit_len > 0)
            {
                for (int i = 0; i <= str1.Length; i++)
                {
                    spp = str1.Substring(0, i);
                    byteLenght = Encoding.Default.GetBytes(spp).Length;
                    if (byteLenght <= limit_len)
                    {
                        hopestring = spp;
                    }
                    else
                    {
                        hopestring = str1.Substring(0, i - 1);
                        break;
                    }
                }
            }
            return hopestring;
        }

        //以html檔保留mail資料
        public void SaveMailLog(string po_no, string toMail, string toSubject, string toContent)
        {

            string filePath = config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule\\INVOICE_EMAIL\\" + DateTime.Now.ToString("yyyyMMdd");
            string fileName = config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule\\INVOICE_EMAIL\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + po_no + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".html";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(toMail);
            sb.AppendLine(toSubject);
            sb.Append(toContent);
            if (!Directory.Exists(filePath)) { Directory.CreateDirectory(filePath); }
            try
            {
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.WriteLine(sb.ToString());
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
