using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JCLib.DB.Tool;
using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using Oracle.ManagedDataAccess.Client;

namespace BES002
{
    class InternalToInternet
    {
        L l = new L("BES002");
        public void Run()
        {


            try
            {
                l.lg("********************************************", "");
                l.lg("內網到外網", "");

                CallDBtools calldbtools = new CallDBtools();
                string ErrorStep = "Start" + Environment.NewLine;
                Console.WriteLine("start");
                IPAddress fip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                // -- oracle -- 
                #region oracle 連線設定及參數
                Console.WriteLine("iracle 連線設定及參數");
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                String s_conn_oracle = calldbtools.SelectDB("oracle");
                OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
                if (conn_oracle.State == ConnectionState.Open)
                    conn_oracle.Close();
                conn_oracle.Open();
                Console.WriteLine("oracle連線開啟");
                List<string> transcmd_oracle = new List<string>();
                List<CallDBtools_Oracle.OracleParam> listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();
                int rowsAffected_oracle = -1;
                int iTransOra = 0;
                string sql_oracle = "";
                #endregion

                // -- msdb -- 
                #region msdb 連線設定及參數
                Console.WriteLine("msdb 連線設定及參數");
                CallDBtools_MSDb callDbtools_msdb = new CallDBtools_MSDb();
                String s_conn_msdb = calldbtools.SelectDB("msdb");
                SqlConnection conn_msdb = new SqlConnection(s_conn_msdb);
                if (conn_msdb.State == ConnectionState.Open)
                    conn_msdb.Close();
                conn_msdb.Open();
                Console.WriteLine("msdb連線開啟");
                List<string> transcmd_msdb = new List<string>();
                List<CallDBtools_MSDb.MSDbParam> listParam_msdb = new List<CallDBtools_MSDb.MSDbParam>();
                int rowsAffected_msdb = -1;
                int iTransMsdb = 0;
                string sql_msdb = "";
                #endregion

                try
                {
                    string msg_oracle = "error";
                    DataTable dt_oralce = new DataTable();
                    DataTable dt_oralce_Batno = new DataTable();
                    sql_oracle = @" select CRDOCNO
                                  from CR_DOC 
                                 where CR_STATUS in('E','G')
                                   and EMAIL is not null
                              ";
                    l.lg("撈取待寄信資料處理範圍", sql_oracle);
                    Console.WriteLine("撈取待寄信資料處理範圍");
                    dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                    Console.WriteLine(string.Format("資料抓取狀態:{0} 查詢比數:{1}", msg_oracle, dt_oralce.Rows.Count));
                    if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                    {
                        Console.WriteLine(string.Format("逐筆處理資料，依緊急醫療出貨單編號分類要寄幾封信"));
                        for (int i = 0; i < dt_oralce.Rows.Count; i++) //依緊急醫療出貨單編號分類要寄幾封信
                        {
                            Console.WriteLine(string.Format("CR_DOCNO = {0}", dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim()));
                            //依緊急醫療出貨單編號撈出信件基本資料
                            Console.WriteLine(string.Format("依緊急醫療出貨單編號撈出信件基本資料"));
                            DataTable dtM = new DataTable();
                            dtM = GetMailBasicData(dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim());


                            //組出信件內容
                            Console.WriteLine(string.Format("組出信件內容"));
                            string mailBodyStr = GetMailContent(dtM);

                            l.lg("sendMail", "");
                            Console.WriteLine(string.Format("sendMail"));
                            SendMail sendmail = new SendMail();
                            bool sendMailFlag = sendmail.Send_Mail(dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim(),
                                                                   calldbtools.GetDefaultMailSender(), dtM.Rows[0]["EMAIL"].ToString().Trim(),
                                                                   calldbtools.GetDefaultMailCC(), "三軍總醫院緊急醫療出貨通知",
                                                                   "三軍總醫院緊急醫療出貨通知，緊急醫療出貨單編號：" + dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim(),
                                                                   mailBodyStr, dtM.Rows[0]["UPDATE_USER"].ToString().Trim(), "", new List<string>());
                            Console.WriteLine(string.Format("是否計件成功: {0}", sendMailFlag ? "Y" : "N"));
                            if (sendMailFlag) //寄件成功
                            {
                                iTransOra = 0;
                                iTransMsdb = 0;
                                sql_oracle = "";
                                sql_msdb = "";
                                transcmd_oracle.Clear();
                                listParam_oracle.Clear();
                                transcmd_msdb.Clear();
                                listParam_msdb.Clear();

                                //begin trans 
                                OracleTransaction transaction_oracle = conn_oracle.BeginTransaction();
                                SqlTransaction transaction_msdb = conn_msdb.BeginTransaction();


                                DataTable dt_seq = new DataTable();
                                string sql_seq = @" select CR_MAILLOG_SEQ.nextval as SEQ from dual ";
                                dt_seq = callDbtools_oralce.CallOpenSQLReturnDT(sql_seq, null, null, "oracle", "T1", ref msg_oracle);
                                l.lg("◆撈取CR_MAILLOG_SEQ序號值,SQL:", sql_seq + "=" + dt_seq.Rows[0]["seq"].ToString().Trim());
                                Console.WriteLine(string.Format("撈取CR_MAILLOG_SEQ序號值: {0}", dt_seq.Rows[0]["seq"].ToString().Trim()));

                                #region 1.新增 CRDOC_MAILLOG

                                sql_oracle = @"insert into CRDOC_MAILLOG(CRMAIL_SEQ, LOG_TIME, CRDOCNO, MAILFROM, MAILTO,
                                                                     MAILCC, MSUBJECT, MAILTYPE, MAILBODY, CREATE_USER)
                                           values(:seq, sysdate, :crdocno, :mail_sender, :mail_reciver,
                                                  null, :subject, '緊急醫療進貨', substr(:mail_body,1,4000), :update_user)
                                          ";
                                l.lg("◆緊急醫療出貨單編號：", dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim());
                                l.lg("1.新增內網 CRDOC_MAILLOG", sql_oracle);
                                Console.WriteLine(string.Format("1.新增內網 CRDOC_MAILLOG"));
                                ++iTransOra;
                                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar2", dt_seq.Rows[0]["seq"].ToString().Trim()));
                                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":crdocno", "VarChar2", dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim()));
                                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mail_sender", "VarChar2", calldbtools.GetDefaultMailSender()));
                                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mail_reciver", "VarChar2", dtM.Rows[0]["EMAIL"].ToString().Trim()));
                                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":subject", "VarChar2", "緊急醫療進貨"));
                                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mail_body", "VarChar2", mailBodyStr));
                                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_user", "VarChar2", dtM.Rows[0]["UPDATE_USER"].ToString().Trim()));
                                transcmd_oracle.Add(sql_oracle);
                                #endregion

                                #region 2.更新 CR_DOC狀態(CR_STATUS=F.H 已寄信)  
                                sql_oracle = @" update CR_DOC
                                               set EMAILTIME = sysdate,
                                                   CR_STATUS = (case when CR_STATUS = 'E' then 'F'
                                                                     when CR_STATUS = 'G' then 'H'
                                                                 else CR_STATUS
                                                                end)
                                             where CRDOCNO = :crdocno
                                          ";
                                l.lg("2.更新內網 CR_DOC(CR_STATUS=F.H 已寄信) ", sql_oracle);
                                Console.WriteLine(string.Format("2.更新內網 CR_DOC(CR_STATUS=F.H 已寄信)"));
                                ++iTransOra;
                                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar2", dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim()));
                                transcmd_oracle.Add(sql_oracle);
                                #endregion

                                #region 3.新增外網 WB_CR_DOC(STATUS=A 待廠商回覆) 
                                listParam_msdb.Clear();
                                string msgDB = "error";
                                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                                DataTable dt_msdb = new DataTable();
                                sql_msdb = @" select CRDOCNO as wbCRDOCNO
                                            from WB_CR_DOC
                                           where CRDOCNO=@crdocno
                                        ";
                                Console.WriteLine(string.Format("3.新增外網 WB_CR_DOC(STATUS=A 待廠商回覆) "));
                                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@crdocno", "VarChar", dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim()));
                                dt_msdb = callDBtools_msdb.CallOpenSQLReturnDT(sql_msdb, null, listParam_msdb, "msdb", "T1", ref msgDB);
                                Console.WriteLine(string.Format("檢查是否有資料 資料抓取狀態:{0} 查詢比數:{1}", msgDB, dt_msdb.Rows.Count));
                                if (msgDB == "" && dt_msdb.Rows.Count == 0) //資料抓取沒有錯誤 且無資料
                                {
                                    sql_msdb = @"insert into WB_CR_DOC (CRDOCNO, MMCODE, MMNAME_C, MMNAME_E, APPQTY,
                                                                BASE_UNIT, TOWH, WH_NAME, REQDATE, CR_UPRICE,
                                                                APPTIME, AGEN_NO, EMAIL,
                                                                AGEN_NAMEC, AGEN_TEL, AGEN_BOSS,
                                                                REPLY_STATUS, WEXP_ID, IN_STATUS, CREATE_TIME, CREATE_USER,
                                                                PATIENTNAME, CHARTNO)
                                         values (@crdocno, @mmcode, @mmname_c, @mmname_e, @appqty,
                                                 @base_unit, @towh, @wh_name, @reqdate, @cr_uprice,
                                                 @apptime, @agen_no, @email,
                                                 @agen_namec, @agen_tel, @agen_boss,               
                                                 'A', @wexp_id, 'A', getdate(), @update_user,
                                                 @patientname, @chartno)
                                         ";
                                    l.lg("3.新增外網 WB_CR_DOC", sql_msdb);
                                    Console.WriteLine(string.Format("3.新增外網 WB_CR_DOC"));
                                    listParam_msdb.Clear();
                                    ++iTransMsdb;
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@crdocno", "VarChar", dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@mmcode", "VarChar", dtM.Rows[0]["MMCODE"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@mmname_c", "VarChar", dtM.Rows[0]["MMNAME_C"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@mmname_e", "VarChar", dtM.Rows[0]["MMNAME_E"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@appqty", "VarChar", dtM.Rows[0]["APPQTY"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@base_unit", "VarChar", dtM.Rows[0]["BASE_UNIT"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@towh", "VarChar", dtM.Rows[0]["TOWH"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@wh_name", "VarChar", dtM.Rows[0]["WH_NAME"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@reqdate", "VarChar", dtM.Rows[0]["REQDATE"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@cr_uprice", "VarChar", dtM.Rows[0]["CR_UPRICE"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@apptime", "VarChar", dtM.Rows[0]["APPTIME"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@agen_no", "VarChar", dtM.Rows[0]["AGEN_NO"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@email", "VarChar", dtM.Rows[0]["EMAIL"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@agen_namec", "VarChar", dtM.Rows[0]["AGEN_NAMEC"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@agen_tel", "VarChar", dtM.Rows[0]["AGEN_TEL"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@agen_boss", "VarChar", dtM.Rows[0]["AGEN_BOSS"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@wexp_id", "VarChar", dtM.Rows[0]["WEXP_ID"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@update_user", "VarChar", dtM.Rows[0]["UPDATE_USER"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@patientname", "VarChar", dtM.Rows[0]["PATIENTNAME"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@chartno", "VarChar", dtM.Rows[0]["CHARTNO"].ToString().Trim()));
                                    transcmd_msdb.Add(sql_msdb);
                                }
                                #endregion

                                try
                                {
                                    #region 更新內網Oracle
                                    Console.WriteLine(string.Format("更新內網Oracle"));
                                    rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                                                                                          transcmd_oracle,
                                                                                          listParam_oracle,
                                                                                          transaction_oracle,
                                                                                          conn_oracle);
                                    Console.WriteLine(string.Format("異動比數: {0}", rowsAffected_oracle));
                                    l.lg("更新內網 Oracle", "rowsAffected_oracle=" + rowsAffected_oracle);
                                    #endregion

                                    #region 更新外網MsDb
                                    Console.WriteLine(string.Format("更新外網MsDb"));
                                    rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
                                                                                        transcmd_msdb,
                                                                                        listParam_msdb,
                                                                                        transaction_msdb,
                                                                                        conn_msdb);
                                    l.lg("更新外網 MsDb", "rowsAffected_msdb=" + rowsAffected_msdb);
                                    Console.WriteLine(string.Format("異動比數: {0}", rowsAffected_msdb));
                                    #endregion

                                    transaction_oracle.Commit();
                                    transaction_msdb.Commit();
                                    Console.WriteLine(string.Format("commit"));

                                    l.lg("-----------------------------------------------------------", "");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(string.Format("更新 ERROR Message: {0}", ex.Message));
                                    Console.WriteLine(string.Format("{0}", ex.ToString()));
                                    CallDBtools callDBtools = new CallDBtools();
                                    callDbtools_oralce.I_ERROR_LOG("BES002", "BES002-緊急醫療出貨申請產生通知單，內網到外網轉檔失敗:" + ex.Message, "AUTO");
                                    l.le("BES002", "BES002-緊急醫療出貨申請產生通知單，內網到外網轉檔失敗:" + ex.Message);
                                    ErrorStep += ",失敗" + Environment.NewLine;
                                    ErrorStep += ex.ToString() + Environment.NewLine;
                                    transaction_oracle.Rollback();
                                    Console.WriteLine(string.Format("oracle rollback"));
                                    conn_oracle.Close();
                                    Console.WriteLine(string.Format("oracle connection closed"));
                                    transaction_msdb.Rollback();
                                    Console.WriteLine(string.Format("msdb rollback"));
                                    conn_msdb.Close();
                                    Console.WriteLine(string.Format("msdb connection closed"));

                                  //  Console.ReadLine();
                                }
                            }
                            else //寄件失敗
                            {
                                Console.WriteLine(string.Format("寄件失敗"));
                                callDbtools_oralce.I_ERROR_LOG("BES002", "寄件失敗:" + dt_oralce.Rows[i]["CRDOCNO"].ToString().Trim(), "AUTO");
                            }
                        } //end of for i
                        conn_oracle.Close();
                        conn_msdb.Close();

                        ErrorStep += ",成功" + Environment.NewLine;
                        Console.WriteLine(string.Format("處理成功"));
                    }
                    else if (msg_oracle != "")
                    {
                        Console.WriteLine(string.Format("取得CR_DOC待寄信資料失敗"));
                        callDbtools_oralce.I_ERROR_LOG("BES002", "取得CR_DOC待寄信資料失敗:" + msg_oracle, "AUTO");
                    }
                    else {
                        Console.WriteLine(string.Format("無需處理資料"));
                        Console.WriteLine(string.Format(""));
                        l.lg("無需處理資料", "");
                    }
                    Console.WriteLine("end");
                   // Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("ERROR: {0}", ex.Message));
                    Console.WriteLine(string.Format("ERROR content: {0}", ex.ToString()));
                    CallDBtools callDBtools = new CallDBtools();
                    callDbtools_oralce.I_ERROR_LOG("BES002", "BES002-緊急醫療出貨申請產生通知單資料轉檔失敗:" + ex.Message, "AUTO");
                    l.le("BES002", "BES002-緊急醫療出貨申請產生通知單資料轉檔失敗:" + ex.Message);
                    ErrorStep += ",失敗" + Environment.NewLine;
                    ErrorStep += ex.ToString() + Environment.NewLine;
                    
                    conn_oracle.Close();
                    Console.WriteLine(string.Format("oracle connection closed"));
                    conn_msdb.Close();
                    Console.WriteLine(string.Format("msdb connection closed"));
                   // Console.ReadLine();
                }
            }
            catch (Exception e) {
                Console.WriteLine(string.Format("connection ERROR: {0}", e.Message));
                l.le("BES002", "BES002-緊急醫療出貨申請產生通知單資料轉檔失敗:" + e.Message);
               // Console.ReadLine();
            }
        }
        //撈出待寄信件資料
        private DataTable GetMailBasicData(string crdocno)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = @"select CRDOCNO,CRDOCNO as 緊急醫療出貨單編號, MMCODE,MMCODE as 院內碼, MMNAME_C,MMNAME_C as 中文品名, 
                                         MMNAME_E,MMNAME_E as 英文品名,APPQTY, APPQTY as 申請數量, CR_UPRICE,CR_UPRICE as 單價,
                                         BASE_UNIT,BASE_UNIT as 計量單位, TOWH, WH_NAME,WH_NAME as 入庫庫房, REQDATE,
                                         TO_CHAR(REQDATE,'YYYY/MM/DD') as 要求到貨日期,
                                         APPTIME, AGEN_NO, EMAIL,
                                         (select AGEN_NAMEC from PH_VENDER
                                           where AGEN_NO = a.AGEN_NO) as AGEN_NAMEC,
                                         (select AGEN_TEL from PH_VENDER
                                           where AGEN_NO = a.AGEN_NO) as AGEN_TEL,
                                         (select AGEN_BOSS from PH_VENDER
                                           where AGEN_NO = a.AGEN_NO) as AGEN_BOSS,
                                         WEXP_ID,CR_STATUS,UPDATE_USER,
                                         (case when length(patientName) = 2 
                                                then substr(patientName, 1,1)||'○'
                                                else substr(patientName, 1,1)|| replace(LPAD(' ', length(patientName)-1,'○'),' ','')||substr(patientName, length(patientName),1)
                                            end) as PATIENTNAME, 
                                         chartno as CHARTNO
                                    from CR_DOC a
                                   where CRDOCNO=:crdocno
                                ";
            List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
            paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":crdocno", "VarChar", crdocno));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraListA, "oracle", "T1", ref msg_oracle);
            if (msg_oracle != "")
                callDbtools_oralce.I_ERROR_LOG("BES002", "取得待寄信件資料失敗:" + msg_oracle, "AUTO");
            return dt_oralce;
        }

        //組出信件內容
        private string GetMailContent(DataTable dtM)
        {
            l.lg("出貨通知", "");
            string mail_body = "";
            string headerStr = "";
            mail_body += "<P align=center><font size=4 face=新細明體 color=black>三軍總醫院緊急醫療出貨通知" + headerStr + " </font></p><br><br>";
            mail_body += "緊急醫療出貨單編號：" + dtM.Rows[0]["CRDOCNO"].ToString().Trim() + " <br>";

            l.lg("回覆函 URL + 參數加密 串接", "");
            //回覆函 URL + 參數加密 串接
            CallDBtools calldbtools = new CallDBtools();
            string urlStr = calldbtools.GetInternetWebServerIP(); //正式時使用
            //string urlStr = "http://localhost:54835"; //測試時使用
            urlStr += "/Form/Show/B/BH0010?";
            //參數加密格式 CRDOCNO = EMG0000019
            l.lg("參數加密格式", "");
            string enCodeStr = "CRDOCNO=" + dtM.Rows[0]["CRDOCNO"].ToString().Trim();
            urlStr += EnCode(enCodeStr);

            mail_body += "<font size=3 face=新細明體 color=red>謝謝您的合作,請按一下回覆----></font><a href='" + urlStr + "'>回覆函</a>" + " <br>";

            //mail_body += "<table style=\"border-collapse: collapse; width: 100%;border:2px #000000 solid;\" border=\"1\">";
            //mail_body += "  <tbody>";
            //mail_body += "    <tr>";
            //mail_body += "      <td style=\"width: auto;\">流水號</td>";
            //mail_body += "      <td style=\"width: auto;\">院內碼</td>";
            //mail_body += "      <td style=\"width: auto;\">中文品名</td>";
            //mail_body += "      <td style=\"width: auto;\">英文品名</td>";
            //mail_body += "      <td style=\"width: auto;\">申請數量</td>";
            //mail_body += "      <td style=\"width: auto;\">計量單位</td>";
            //mail_body += "      <td style=\"width: auto;\">入庫庫房</td>";
            //mail_body += "      <td style=\"width: auto;\">要求到貨日期</td>";
            //mail_body += "      <td style=\"width: auto;\">單價</td>";
            //mail_body += "    </tr>";
            mail_body += "<br/>";
            l.lg("foreac新增資料", "");
            foreach (DataRow datarow in dtM.Rows)
            {
                //mail_body += "    <tr>";
                //mail_body += "      <td>" + datarow["流水號"].ToString().Trim() + "</td>";
                //mail_body += "      <td>" + datarow["院內碼"].ToString().Trim() + "</td>";
                //mail_body += "      <td>" + datarow["中文品名"].ToString().Trim() + "</td>";
                //mail_body += "      <td>" + datarow["英文品名"].ToString().Trim() + "</td>";
                //mail_body += "      <td>" + datarow["申請數量"].ToString().Trim() + "</td>";
                //mail_body += "      <td>" + datarow["計量單位"].ToString().Trim() + "</td>";
                //mail_body += "      <td>" + datarow["入庫庫房"].ToString().Trim() + "</td>";
                //mail_body += "      <td>" + datarow["要求到貨日期"].ToString().Trim() + "</td>";
                //mail_body += "      <td>" + datarow["單價"].ToString().Trim() + "</td>";
                //mail_body += "    </tr>";

                mail_body += "緊急醫療出貨單編號：" + datarow["緊急醫療出貨單編號"].ToString().Trim() + "<br/>";
                mail_body += "院內碼：" + datarow["院內碼"].ToString().Trim() + "<br/>";
                mail_body += "中文品名：" + datarow["中文品名"].ToString().Trim() + "<br/>";
                mail_body += "英文品名：" + datarow["英文品名"].ToString().Trim() + "<br/>";
                mail_body += "申請數量：" + datarow["申請數量"].ToString().Trim() + "<br/>";
                mail_body += "計量單位：" + datarow["計量單位"].ToString().Trim() + "<br/>";
                mail_body += "入庫庫房：" + datarow["入庫庫房"].ToString().Trim() + "<br/>";
                mail_body += "要求到貨日期：" + datarow["要求到貨日期"].ToString().Trim() + "<br/>";
                mail_body += "單價：" + datarow["單價"].ToString().Trim() + "<br/>";
            }
            mail_body += "<br/><br/>";

            l.lg("mailbody結尾", "");
            //mail_body += "  </tbody>";
            //mail_body += "</table><br><br>";
            mail_body += "<p align=left><font color=red size=3 face=標楷體>***本郵件為系統自動發送，請勿直接回信*** </font></p><br>";
            l.lg("回傳mailbody", "");
            return mail_body;
        }

        //參數值加密 po_no=INV010712270196&agen_no=826&invoice_batno=25
        private string EnCode(string EnString)
        {
            l.lg("參數值加密", "");
            byte[] data = Encoding.UTF8.GetBytes(EnString);
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Padding = PaddingMode.PKCS7;
            DES.Key = ASCIIEncoding.ASCII.GetBytes("BH06BHS4");
            DES.IV = ASCIIEncoding.ASCII.GetBytes("TsghTsgh");
            ICryptoTransform desencrypt = DES.CreateEncryptor();
            byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
            return BitConverter.ToString(result);
        }
    }
}
