using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Data;
using Dapper;
using System.Web.Mail;
using System.Diagnostics;

namespace JCLib.DB.Tool
{
    public class CallDBtools
    {
        String connStrOracle = "";
        String connStrMSDb = "";
        Configuration config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.StartupPath + "\\Schedule.config");

        //依據 Schedule.config <add key="servernm" value="AIDC_TEST" /> 取得主機名稱
        public string GetServerName()
        {
            string serverName = config.AppSettings.Settings["servernm"].Value.ToString();
            return serverName;
        }

        //設定資料庫
        public string SelectDB(string dbNM)
        {
            GetDBConn();
            string dbConn = "";
            if (dbNM == "oracle") { dbConn = connStrOracle; }
            else if (dbNM == "msdb") { dbConn = connStrMSDb; }
            return dbConn;
        }

        //設定資料庫連線字串，依據 Schedule.config <add key="servernm" value="AIDC_TEST"/> 去 Schedule.config 讀取資料庫連線
        public void GetDBConn()
        {
            string serverName = GetServerName();
            if (serverName == "AIDC_TEST")
            {
                connStrOracle = config.ConnectionStrings.ConnectionStrings["AIDC_ORACLE_TEST"].ConnectionString;
                connStrMSDb = config.ConnectionStrings.ConnectionStrings["AIDC_MS_TEST"].ConnectionString;
            }
            else if (serverName == "AIDC_PRODUCTION")
            {
                connStrOracle = config.ConnectionStrings.ConnectionStrings["AIDC_ORACLE_PRODUCTION"].ConnectionString;
                connStrMSDb = config.ConnectionStrings.ConnectionStrings["AIDC_MS_PRODUCTION"].ConnectionString;
            }
            else if (serverName == "TSGHMM_TEST")
            {
                connStrOracle = config.ConnectionStrings.ConnectionStrings["TSGHMM_ORACLE_TEST"].ConnectionString;
                connStrMSDb = config.ConnectionStrings.ConnectionStrings["TSGHMM_MS_TEST"].ConnectionString;
            }
            else if (serverName == "TSGHMM_PRODUCTION")
            {
                connStrOracle = config.ConnectionStrings.ConnectionStrings["TSGHMM_ORACLE_PRODUCTION"].ConnectionString;
                connStrMSDb = config.ConnectionStrings.ConnectionStrings["TSGHMM_MS_PRODUCTION"].ConnectionString;
            }
        }

        //依據 Schedule.config <add kry="SendMailFlag" value="Y"> 是否要寄出信件
        public string GetSendMailFlag()
        {
            string sendMailFlag = config.AppSettings.Settings["SendMailFlag"].Value.ToString();
            return sendMailFlag;
        }

        //例外訊息寫入LOG
        public void WriteExceptionLog(string msg, string cmd, string param)
        {
            string filePath = config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule";
            string fileName = config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule\\TsghmmLog_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(DateTime.Now.ToString());
            sb.AppendLine("錯誤訊息：" + msg);
            sb.AppendLine("內容：" + cmd);
            sb.AppendLine("參數：" + param);
            sb.AppendLine("------------------------------");

            if (File.Exists(fileName))
            {
                using (StreamWriter sw = File.AppendText(fileName))
                {
                    sw.WriteLine(sb.ToString());
                }
            }
            else
            {
                if (!Directory.Exists(filePath)) { Directory.CreateDirectory(filePath); }
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.WriteLine(sb.ToString());
                }
            }
        }

        //依據 Schedule.config <add key="Default_MailSender" value="sTIP@taiwanmobile.com"/> 取得系統預設寄件者
        public string GetDefaultMailSender()
        {
            string defaultMailSender = config.AppSettings.Settings["Default_MailSender"].Value.ToString();
            return defaultMailSender;
        }

        //依據  <add key="Default_MailSender_DisplayName" value="三軍總醫院中央庫房"/> 取得系統預設寄件者中文顯示
        public string GetDefaultMailSenderDisplayName()
        {
            string defaultMailSenderDisplayName = config.AppSettings.Settings["Default_MailSender_DisplayName"].Value.ToString();
            return defaultMailSenderDisplayName;
        }

        //依據 Schedule.config <add key="Default_MailCC" value="ycchou@ms.aidc.com.tw"/> 取得系統預設副本收件者
        public string GetDefaultMailCC()
        {
            string defaultMailCC = config.AppSettings.Settings["Default_MailCC"].Value.ToString();
            return defaultMailCC;
        }

        //依據 Schedule.config <add key="AIDC_SMTPServer_IP" value="192.168.100.43"/> 取得smtp server ip
        public string GetSmtpServerIP()
        {
            string serverName = GetServerName();
            string smtpServerIP = "";
            if (serverName.Contains("AIDC"))
                smtpServerIP = config.AppSettings.Settings["AIDC_SMTPServer_IP"].Value.ToString();
            else if (serverName.Contains("TSGHMM"))
                smtpServerIP = config.AppSettings.Settings["TSGHMM_SMTPServer_IP"].Value.ToString();
            return smtpServerIP;
        }

        //依據 Schedule.config <add key="INTERNET_WebServerIP" value="http://192.168.99.52:8080"/> 取得外部網頁web server IP
        public string GetInternetWebServerIP()
        {
            string internetWebServerIP = config.AppSettings.Settings["INTERNET_WebServerIP"].Value.ToString();
            return internetWebServerIP;
        }

        
    } // ec
    public class L
    {
        string s_namespace = "";
        public L(string string_namespace)
        {
            s_namespace = string_namespace;
        }
        public void lg(
            string s_function_name,
            string msg
        )
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.StartupPath + "\\Schedule.config");
            string filePath = "";
            if (!String.IsNullOrEmpty(JCLib.Util.GetEnvSetting("TsghmmLog")))
                filePath = JCLib.Util.GetEnvSetting("TsghmmLog");
            else if (!String.IsNullOrEmpty(config.AppSettings.Settings["TsghmmLog"].Value))
                filePath = config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule";
            string fileName = s_namespace + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_log.txt";

            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            sb.Append("\t");
            sb.Append(s_function_name);
            sb.Append("\t");
            sb.Append(msg);
            sb.Append("\r\n");
            writeFile(filePath, fileName, sb.ToString());
        } // 

        public void clg(String s)
        {
            StackTrace trace = new StackTrace();
            int caller = 1;
            StackFrame frame = trace.GetFrame(caller);
            string callerName = frame.GetMethod().Name;

            lg(callerName, s);
            Console.WriteLine(s);
        } // 
        // 回傳 C:\TsghmmLog\Schedule 
        public String getLogDirPath()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.StartupPath + "\\Schedule.config");
            return config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule";
        }

        public void le(
            string s_function_name,
            string msg
        )
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.StartupPath + "\\Schedule.config");
            string filePath = config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule";
            string fileName = s_namespace + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_errlog.txt";

            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            sb.Append("\t");
            sb.Append(s_function_name);
            sb.Append("\t");
            sb.Append(msg);
            sb.Append("\r\n");
            writeFile(filePath, fileName, sb.ToString());
        }
        public String ldb(string msg)
        {
            if (isInAidc())
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.StartupPath + "\\Schedule.config");
                string filePath = config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule";
                string fileName = s_namespace + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_db_status.html";
                writeFile(filePath, fileName, msg);
            }
            return msg;
        }
        public String get資料庫現況Log檔路徑()
        {
            if (isInAidc())
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(System.Windows.Forms.Application.StartupPath + "\\Schedule.config");
                string filePath = config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule";
                return config.AppSettings.Settings["TsghmmLog"].Value.ToString() + "\\Schedule\\" + s_namespace + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_db_status.html";
            }
            return "";
        }

        public String getHtmlTable(String tableTitle, DataTable dt_new, DataTable dt_old, String sCommaColKeys)
        {
            List<String> lstColKeys = new List<String>();
            if (!String.IsNullOrEmpty(sCommaColKeys))
            {
                String[] sAry = sCommaColKeys.Split(',');
                foreach (String s in sAry)
                {
                    lstColKeys.Add(s.Trim());
                }
            }
            return getHtmlTable(tableTitle, dt_new, dt_old, lstColKeys);
        }
        public String getHtmlTable(String tableTitle, DataTable dt_new, DataTable dt_old, List<String> lstColKeys)
        {
            String pink刪除列 = "pink";
            String green新增列 = "#ccf7cc";
            String s = "";
            String sBr = "\r\n";
            String sCssStyle = "";
            s += "<table border=\"1\">" + sBr;
            s += "\t<tr><td colspan=\"" + dt_new.Columns.Count + "\">" + tableTitle + "</td></tr>" + sBr;
            s += "\t<tr>" + sBr;
            foreach (DataColumn dc in dt_new.Columns)
            {
                sCssStyle = "";
                if (isKey(dc.ColumnName, lstColKeys))
                    sCssStyle = "background-color:pink;";
                s += "\t\t<td style=\"" + sCssStyle + "\">" + dc.ColumnName + "</td>" + sBr;
            }
            s += "\t</tr>" + sBr;


            // 刪除(舊有,新沒有)
            if (dt_old != null)
            {
                foreach (DataRow dr_old in dt_old.Rows)
                {
                    DataRow dr_new = GetRowByKeys(dt_new, dr_old, lstColKeys);
                    if (dr_new == null)
                    { // 舊有,新沒有, 此列為新增
                        s += "\t<tr title=\"此列已被刪除，不存在於table\" style=\"background-color:" + pink刪除列 + "\"> " + sBr;
                        foreach (DataColumn dc in dt_old.Columns)
                        {
                            s += "\t\t<td>" + dr_old[dc.ColumnName].ToString() + "</td>" + sBr;
                        }
                        s += "\t</tr>" + sBr;
                    }
                } // 
            }


            // 新增(舊沒有,新有 )
            foreach (DataRow dr_new in dt_new.Rows)
            {
                if (dt_old != null)
                {
                    DataRow dr_old = GetRowByKeys(dt_old, dr_new, lstColKeys);
                    if (dr_old == null)
                    { // 舊沒有,新有, 此列為新增
                        s += "\t<tr title=\"此列是新增列\" style=\"background-color:" + green新增列 + "\"> " + sBr;
                        foreach (DataColumn dc in dt_new.Columns)
                        {
                            s += "\t\t<td>" + dr_new[dc.ColumnName].ToString() + "</td>" + sBr;
                        }
                        s += "\t</tr>" + sBr;
                    }
                }
                else
                {
                    s += "\t<tr style=\"background-color:;\"> " + sBr;
                    foreach (DataColumn dc in dt_new.Columns)
                    {
                        s += "\t\t<td>" + dr_new[dc.ColumnName].ToString() + "</td>" + sBr;
                    }
                    s += "\t</tr>" + sBr;
                }
            } // 

            // 修改(舊有,新有)
            foreach (DataRow dr_new in dt_new.Rows)
            {
                if (dt_old != null)
                {
                    DataRow dr_old = GetRowByKeys(dt_old, dr_new, lstColKeys);
                    if (dr_old != null)
                    { // 舊有,新有, 此列為修改
                        s += "\t<tr title=\"此列有被修改過\" style=\"" + sCssStyle + "\"> " + sBr;
                        foreach (DataColumn dc in dt_new.Columns)
                        {
                            String sNew = dr_new[dc.ColumnName].ToString();
                            String sOld = dr_old[dc.ColumnName].ToString();
                            if (sNew.Equals(sOld))
                            {
                                s += "\t\t";
                                s += "<td>" + sNew + "</td>" + sBr;
                            }
                            else
                            {
                                s += "\t\t";
                                s += "<td>";
                                s += "  <table border=\"1\">";
                                s += "      <tr style=\"background-color:;\"><td>" + sOld + "</td></tr>";
                                s += "      <tr style=\"background-color:ffff99;\"><td>" + sNew + "</td></tr>";
                                s += "  </table>";
                                s += "</td>" + sBr;
                            }

                        }
                        s += "\t</tr>" + sBr;
                    }
                }
            } // 
            s += "</table>" + sBr;
            return s;
        }
        bool isKey(String colNm, List<String> lstColKeys)
        {
            foreach (String eK in lstColKeys)
            {
                if (colNm.ToLower().Equals(eK.ToLower()))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 傳入要搜尋的DataRow(lstColKeys為key欄位)去查dt_source是否有此列
        /// </summary>
        /// <remarks>
        /// 使用方法的出處 https://docs.microsoft.com/zh-tw/dotnet/csharp/codedoc
        /// </remarks>
        /// <returns>
        /// 有找到回傳DataRow, 沒找到回傳null
        /// </returns>
        /// <example>
        /// <code>
        /// 程式範例如下
        /// </code>
        /// </example>
        DataRow GetRowByKeys(
            DataTable dt_source, // 資料來源的table
            DataRow dr_search,    // 有存放key的row
            List<String> lstColKeys // 是key的欄位
        )
        {
            for (int i = 0; i < dt_source.Rows.Count; i++)
            {
                DataRow dr = (DataRow)dt_source.Rows[i];
                bool bIsFind = true;
                foreach (String keyColumnName in lstColKeys) // 依key判斷是否為同一筆資料
                {
                    String source_val = dr[keyColumnName].ToString();
                    String search_val = dr_search[keyColumnName].ToString();
                    if (!source_val.Equals(search_val))
                    {
                        bIsFind = false;
                        break;
                    }
                }
                if (bIsFind)
                    return dr;
            }
            return null;
        }

        public void writeFile(
            string dirPath,    // C:\TsghmmLog\Schedule
            string fileName,    // HISBACK.Program_19-08-16_113233_MMRep_PhrP021_table_log.txt";
            string msg
        ) 
        {
            string filePath = dirPath + "\\" + fileName;
            if (File.Exists(filePath))
            {
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.WriteLine(msg);
                }
            }
            else
            {
                if (!Directory.Exists(dirPath)) { Directory.CreateDirectory(dirPath); }
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.WriteLine(msg);
                }
            }
        }

        public bool isInAidc()
        {
            IPAddress[] aryIPAddress = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ipAddress in aryIPAddress)
            {

                String sEachIp = ipAddress.ToString();
                if (IPAddress.Parse(sEachIp).AddressFamily == AddressFamily.InterNetwork)
                {
                    if (
                        (sEachIp.IndexOf("192.20") > -1) ||
                        (sEachIp.IndexOf("192.27") > -1)
                    )
                    {
                        return true;
                    }
                }
            }
            return false;
        } // 

    } // ec L
    public class FL
    {
        private String sCls = "";
        public FL(String sCls)
        {
            this.sCls = sCls;
        }
        public void lg(String sFun, String s)
        {
            if (isInAidc())
            {
                String sLogPath = @"\InadArea\" + System.DateTime.Now.ToString("yyyyMMdd") + "_" + sCls + ".log.txt";
                String sLogFilePath = @"D:\wwwroot\InadArea\" + System.DateTime.Now.ToString("yyyyMMdd") + "_" + sCls + ".log.txt";
                StringBuilder sb = new StringBuilder();
                sb.Append(System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:fff") + "\t");
                sb.Append(sFun + "\t");
                sb.Append(s + "\r\n");
                writeAppendTxtFile(sLogFilePath, sb.ToString());    // 寫log檔
            }
        } // 
        public void le(String sFun, String s)
        {
            if (isInAidc())
            {
                lg(sFun, s);
                String sLogPath = @"\InadArea\" + System.DateTime.Now.ToString("yyyyMMdd") + "_" + sCls + ".err.log.txt";
                String sLogFilePath = @"D:\wwwroot\InadArea\" + System.DateTime.Now.ToString("yyyyMMdd") + "_" + sCls + ".err.log.txt";
                StringBuilder sb = new StringBuilder();
                sb.Append(System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:fff") + "\t");
                sb.Append(sFun + "\t");
                sb.Append(s + "\r\n");

                sendMailToAdmin("三總系統錯誤\t" + sCls + "\t" + sFun, sb.ToString());
                writeAppendTxtFile(sLogFilePath, sb.ToString());    // 寫log檔
            }
        } //
        private readonly object LockFile = new object();
        public void writeAppendTxtFile(String sFilePath, String sTxt)
        {
            try
            {
                lock (LockFile)
                {
                    using (var fs = new FileStream(sFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var log = new StreamWriter(fs, System.Text.Encoding.GetEncoding("big5")))
                        {
                            log.Write(sTxt);
                        } // 
                    }  // 
                }
            }
            catch (System.Exception ex)
            {
                // 寫檔產生錯誤 
                sendMailToAdmin("三總系統錯誤 L.cs writeAppendTxtFile('" + sFilePath + "','" + sTxt + "')", ex.Message);
            }
        } // 

        public void sendMailToAdmin(
            String sSubject,
            String sHtmlBody
        )
        {
            if (isInAidc())
            {
                sndMail(
                "yuhsianghuang@ms.aidc.com.tw",
                "yuhsianghuang@ms.aidc.com.tw",
                sSubject,
                sHtmlBody
                );
            }
        }
        public void sendMailToAdmin(
            String sSubject,
            String sHtmlBody,
            String sAttchFilePath
        )
        {
            if (isInAidc())
            {
                sndMail(
                "yuhsianghuang@ms.aidc.com.tw",
                "yuhsianghuang@ms.aidc.com.tw",
                sSubject,
                sHtmlBody,
                sAttchFilePath
                );
            }
        }
        public void sndMail(
            String sFrom,  // "yuhsianghuang@ms.aidc.com.tw"
            String sTo,
            String sSubject,
            String sHtmlBody
        )
        {
            MailMessage mailMsg = new MailMessage();
            mailMsg.From = sFrom; //  "yuhsianghuang@ms.aidc.com.tw";
            mailMsg.To = sTo;
            mailMsg.Bcc = sFrom;
            mailMsg.Subject = sSubject;
            mailMsg.Body = sHtmlBody + "<hr>from " + getIp() + "<hr>";

            String sSmtp = "192.168.100.43"; // "tchsmtp1.aidc.com.tw"; //  "192.168.100.12";    // tchsmtp1.aidc.com.tw
            int iSmtpPort = 25;
            mailMsg.Fields["http://schemas.microsoft.com/cdo/configuration/sendusing"] = 2;
            mailMsg.Fields["http://schemas.microsoft.com/cdo/configuration/smtpserver"] = sSmtp; // tchsmtp1.aidc.com.tw
            mailMsg.Fields["http://schemas.microsoft.com/cdo/configuration/smtpserverport"] = iSmtpPort;
            mailMsg.BodyFormat = MailFormat.Html; //  翔加

            SmtpMail.SmtpServer.Insert(0, sSmtp);
            try
            {
                SmtpMail.Send(mailMsg);
            }
            catch (Exception e)
            {
                // le("sndml()", e.Message);
            }
        } // 
        
        public void sndMail(
            String sFrom,  // "yuhsianghuang@ms.aidc.com.tw"
            String sTo,
            String sSubject,
            String sHtmlBody,
            String sAttchFilePath
        )
        {
            MailMessage mailMsg = new MailMessage();
            mailMsg.From = sFrom; //  "yuhsianghuang@ms.aidc.com.tw";
            mailMsg.To = sTo;
            mailMsg.Bcc = sFrom;
            mailMsg.Subject = sSubject;
            mailMsg.Body = sHtmlBody + "<hr>from " + getIp() + "<hr>";
            if (
                !String.IsNullOrEmpty(sAttchFilePath) &&
                File.Exists(sAttchFilePath)
            )
            {
                mailMsg.Attachments.Add(new MailAttachment(sAttchFilePath));
            }
            

            String sSmtp = "192.168.100.43"; // "tchsmtp1.aidc.com.tw"; //  "192.168.100.12";    // tchsmtp1.aidc.com.tw
            int iSmtpPort = 25;
            mailMsg.Fields["http://schemas.microsoft.com/cdo/configuration/sendusing"] = 2;
            mailMsg.Fields["http://schemas.microsoft.com/cdo/configuration/smtpserver"] = sSmtp; // tchsmtp1.aidc.com.tw
            mailMsg.Fields["http://schemas.microsoft.com/cdo/configuration/smtpserverport"] = iSmtpPort;
            mailMsg.BodyFormat = MailFormat.Html; //  翔加

            SmtpMail.SmtpServer.Insert(0, sSmtp);
            try
            {
                SmtpMail.Send(mailMsg);
            }
            catch (Exception e)
            {
                // le("sndml()", e.Message);
            }
        } // 

        public bool isInAidc()
        {
            IPAddress[] aryIPAddress = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ipAddress in aryIPAddress)
            {

                String sEachIp = ipAddress.ToString();
                if (IPAddress.Parse(sEachIp).AddressFamily == AddressFamily.InterNetwork)
                {
                    if (
                        (sEachIp.IndexOf("192.20") > -1) ||
                        (sEachIp.IndexOf("192.27") > -1)
                    )
                    {
                        return true;
                    }
                }
            }
            return false;
        } // 

        public String getIp()
        {
            IPAddress[] aryIPAddress = Dns.GetHostAddresses(Dns.GetHostName());
            String s = "";
            foreach (IPAddress ipAddress in aryIPAddress)
            {
                String sEachIp = ipAddress.ToString();
                if (IPAddress.Parse(sEachIp).AddressFamily == AddressFamily.InterNetwork)
                {
                    s += sEachIp + ", ";
                }
            }
            return s;
        } // 

        public String reqQueryString(System.Web.HttpRequest httprequest, String key, String defaultValue)
        {
            if (!String.IsNullOrEmpty(httprequest.QueryString[key]))    // 庫房代碼
                return httprequest.QueryString[key];
            return defaultValue;
        }


        // --------------
        // -- SQL處理函式 -- 
        // --------------
        String sBr = "\r\n";
        public void getTwoDateCondition(
            String s_col_name,
            String start_date,
            String end_date,
            ref String sql,
            ref int iP,
            ref DynamicParameters p
        )
        {
            String dt_start = toYmd(start_date);
            String dt_end = toYmd(end_date);
            String sP = "";
            if (
                dt_start != "" &&
                dt_end != ""
            )
            {
                //sql += sBr + "and " + s_col_name + ">={開始揀貨日期} and " + s_col_name + "<{結束揀貨日期}+1";
                sP = ":p" + iP++;
                sql += sBr + " and " + s_col_name + " between to_date(" + sP + ",'yyyy/mm/dd') and ";
                p.Add(sP, string.Format("{0}", dt_start));

                sP = ":p" + iP++;
                sql += sBr + "     to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", dt_end));
            }
            else if (dt_start != "")
            {
                sP = ":p" + iP++;
                sql += sBr + " and " + s_col_name + " >= to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", dt_start));
            }
            else if (dt_end != "")
            {
                sP = ":p" + iP++;
                sql += sBr + " and " + s_col_name + " <= to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", dt_end));
            }
        } // 
        public void getTwoDateConditionBy民國年月(
            String s_col_name,
            String start_date,
            String end_date,
            ref String sql,
            ref int iP,
            ref DynamicParameters p
        )
        {
            String dt_start = toYmd(start_date);
            String dt_end = toYmd(end_date);
            String sP = "";
            if (
                dt_start != "" &&
                dt_end != ""
            )
            {
                sP = ":p" + iP++;
                sql += sBr + " and to_date(to_char(to_number(substr(" + s_col_name + ",1,3))+1911) || '/' || substr(" + s_col_name + ", 4,2) || '/01', 'yyyy/mm/dd') between to_date(" + sP + ",'yyyy/mm/dd') and ";
                p.Add(sP, string.Format("{0}", dt_start));

                sP = ":p" + iP++;
                sql += sBr + "     to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", dt_end));
            }
            else if (dt_start != "")
            {
                sP = ":p" + iP++;
                sql += sBr + " and to_date(to_char(to_number(substr(" + s_col_name + ",1,3))+1911) || '/' || substr(" + s_col_name + ", 4,2) || '/01', 'yyyy/mm/dd') >= to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", dt_start));
            }
            else if (dt_end != "")
            {
                sP = ":p" + iP++;
                sql += sBr + " and to_date(to_char(to_number(substr(" + s_col_name + ",1,3))+1911) || '/' || substr(" + s_col_name + ", 4,2) || '/01', 'yyyy/mm/dd') <= to_date(" + sP + ",'yyyy/mm/dd') ";
                p.Add(sP, string.Format("{0}", dt_end));
            }
        } // 

        public String getDebugSql(String sql, DynamicParameters p)
        {
            if (p != null)
            {
                foreach (var name in p.ParameterNames)
                {
                    var pValue = p.Get<dynamic>(name);
                    sql = sql.Replace(":" + name, "'" + pValue + "'");
                }
            }
            return sql;
        }

        // --------------
        // -- 日期處理 -- 
        // --------------
        public String toYmd(String s)
        {
            if (!String.IsNullOrEmpty(s))
            {
                if (s.Length >= 7) // "2019-05-27T00:00:00"
                {
                    int y;
                    int m;
                    int d;
                    DateTime dt;
                    if (
                        int.TryParse(s.Substring(0, 4), out y) &&
                        int.TryParse(s.Substring(5, 2), out m) &&
                        int.TryParse(s.Substring(8, 2), out d) &&
                        true
                    )
                    {
                        String sYmd = y + "/" + m.ToString().PadLeft(2, '0') + "/" + d.ToString().PadLeft(2, '0');
                        if (DateTime.TryParse(sYmd, out dt))
                        {
                            return sYmd;
                        }
                    }
                }
            }
            return "";
        }
        public String 西元年月日轉民國年月(DateTime dt)
        {
            try
            {
                int y = int.Parse(System.DateTime.Now.ToString("yyyy"));
                int m = int.Parse(System.DateTime.Now.ToString("MM"));
                if (
                    int.TryParse(dt.ToString("yyyy"), out y) &&
                    int.TryParse(dt.ToString("MM"), out m)
                )
                {
                    return (y - 1911).ToString() + m.ToString().PadLeft(2, '0');
                }
            }
            catch (Exception)
            {
            }
            return "";
        }
        

    } // ec FL 

} // en
