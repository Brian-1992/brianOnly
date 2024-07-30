using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using JCLib.DB.Tool;
using System.Configuration;
using System.IO;

namespace Tsghmm.Utility
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
                            foreach (string name in attachmentNames) {
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
                    //寫 log
                    MailLog(fromMail, toMail, toMailCC, toSubject, mail_type, toContentLog, create_user);
                }
                catch (Exception ex)
                {
                    rtn = false;
                    SaveMailLog("失敗"+po_no, toMail, toSubject, toContent);
                    calldbtools.WriteExceptionLog(ex.Message, "Send Mail Fail, 主旨為 " + toSubject, toContent);
                }
            }
            return rtn;
        }

        // 寫 mail log
        public void MailLog(string mail_from, string mail_to, string mail_cc, string mail_subject, string mail_type, string mail_body, string create_user)
        {
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            string dbmsg = "error";
            int rowsAffected = -1;
            string cmdstrI = " insert into PH_MAILLOG (SEQ, LOG_TIME, MAILFROM, MAILTO, MAILCC, ";
            cmdstrI += "                               MSUBJECT, MAILTYPE, MAILBODY, CREATE_USER) ";
            cmdstrI += "                       values (PH_MAILLOG_SEQ.nextval, sysdate, :mailfrom, :mailto, :mailcc, ";
            cmdstrI += "                               :msubject, :mailtype, :mailbody, :create_user) ";
            List<CallDBtools_Oracle.OracleParam> paraListI = new List<CallDBtools_Oracle.OracleParam>();
            paraListI.Add(new CallDBtools_Oracle.OracleParam(1, ":mailfrom", "VARCHAR2", mail_from));                    
            paraListI.Add(new CallDBtools_Oracle.OracleParam(1, ":mailto", "VARCHAR2", mail_to));                        
            paraListI.Add(new CallDBtools_Oracle.OracleParam(1, ":mailcc", "VARCHAR2", mail_cc)); 
                       
            paraListI.Add(new CallDBtools_Oracle.OracleParam(1, ":msubject", "VARCHAR2", mail_subject));     
            paraListI.Add(new CallDBtools_Oracle.OracleParam(1, ":mail_type", "VARCHAR2", mail_type));                 
            mail_body = mail_body.Trim().Replace("\r", "");
            mail_body = mail_body.Trim().Replace("\n", "");
            //mail_body = truncate_iptstr(mail_body, 3700);
            paraListI.Add(new CallDBtools_Oracle.OracleParam(1, ":mail_body", "VARCHAR2", mail_body));
            paraListI.Add(new CallDBtools_Oracle.OracleParam(1, ":create_user", "VARCHAR2", create_user));

            rowsAffected = callDbtools_oralce.CallExecSQL(cmdstrI, paraListI, "oracle", ref dbmsg);
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

            string filePath = config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule\\MAIL_LOG\\"+ DateTime.Now.ToString("yyyyMMdd");
            string fileName = config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule\\MAIL_LOG\\"+ DateTime.Now.ToString("yyyyMMdd") +"\\"+ po_no + "_"+DateTime.Now.ToString("yyyyMMddHHmmss") + ".html";
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
            //if (File.Exists(fileName))
            //{
            //    using (StreamWriter sw = File.AppendText(fileName))
            //    {
            //        sw.WriteLine(sb.ToString());
            //    }
            //}
            //else
            //{
            //    if (!Directory.Exists(filePath)) { Directory.CreateDirectory(filePath); }
            //    using (StreamWriter sw = File.CreateText(fileName))
            //    {
            //        sw.WriteLine(sb.ToString());
            //    }
            //}
        }
        //寄送mail 含附件
        //public bool SendMailAttach(string fromMail, string toMail, string toSubject, string toContent, string toCCMail, string attFilePath, ref string errorMsg)
        //{
        //    bool rtn = false;
        //    string servernm = ConfigurationManager.AppSettings["servernm"].ToString();
        //    string smtpAddress = "";

        //    try
        //    {
        //        //string smtpAddress = ConfigurationManager.AppSettings["SMTPServer_IP"].ToString();

        //        if (servernm.StartsWith("AIDC"))
        //        {
        //            smtpAddress = ConfigurationManager.AppSettings["AIDC_SMTPServer_IP"].ToString();
        //        }
        //        else if (servernm == "TWM_TEST")
        //        {
        //            smtpAddress = ConfigurationManager.AppSettings["TWM_SMTPServer_IP"].ToString();
        //        }
        //        else if (servernm == "TWM")
        //        {
        //            smtpAddress = ConfigurationManager.AppSettings["TWM_SMTPServer_IP"].ToString();
        //        }

        //        string emailFrom = fromMail;


        //        using (MailMessage mail = new MailMessage())
        //        {
        //            mail.From = new MailAddress(emailFrom);
        //            //toMail = "kewehuang@ms.aidc.com.tw,"+toMail;
        //            string[] mail_array = toMail.Split(',');
        //            for (int i = 0; i < mail_array.Length; i++)
        //            {
        //                if (mail_array[i].ToString() != "")
        //                    mail.To.Add(mail_array[i].ToString());
        //            }
        //            //mail.To.Add(new MailAddress(toMail));
        //            if (toCCMail != "")
        //            {
        //                mail_array = toCCMail.Split(',');
        //                for (int i = 0; i < mail_array.Length; i++)
        //                {
        //                    if (mail_array[i].ToString() != "")
        //                        mail.CC.Add(mail_array[i].ToString());
        //                }
        //                //mail.CC.Add(new MailAddress(toCCMail));
        //            }
        //            mail.Subject = toSubject;
        //            mail.Body = toContent;
        //            mail.IsBodyHtml = true;
        //            if (attFilePath != "")
        //                mail.Attachments.Add(new Attachment(attFilePath));
        //            // Can set to false, if you are sending pure text.

        //            //mail.Attachments.Add(new Attachment("C:\\SomeFile.txt"));
        //            //mail.Attachments.Add(new Attachment("C:\\SomeZip.zip"));

        //            using (SmtpClient smtp = new SmtpClient(smtpAddress))
        //            {
        //                smtp.Send(mail);

        //            }
        //            rtn = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //CallDBtools callDbtools = new CallDBtools();
        //        errorMsg = "發mail失敗：" + Environment.NewLine + "主旨:" + toSubject + Environment.NewLine + "收件者:" + toMail + Environment.NewLine + "CC收件者:" + toCCMail + Environment.NewLine + "內容:" + toContent + Environment.NewLine + ex.ToString();
        //        DBtools dbtools = new DBtools();
        //        dbtools.WriteExceptionLog("發mail失敗：", "主旨:" + toSubject + Environment.NewLine + "收件者:" + toMail + Environment.NewLine + "CC收件者:" + toCCMail + Environment.NewLine + "內容:" + toContent + Environment.NewLine + ex.Message, "");
        //    }

        //    return rtn;
        //}
    }
}
