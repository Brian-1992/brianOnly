using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.SqlClient;
using JCLib.DB.Tool;

using System.Net;
using System.Net.Sockets;
using System.Linq;
using Tsghmm.Utility;

namespace BHS005
{
    public partial class Form1 : Form
    {
        L l = new L("BHS005");
        #region " 主程式 "
        // 將外網 MSdb ME_MAILBACK 異動資料複製到院內 Oracle ME_MAILBACK
        private void SelectMS_MemailBACK_IntoOracle_MemailBACK()
        {
            l.lg("SelectMS_MemailBACK_IntoOracle_MemailBACK()", "");
            CallDBtools calldbtools = new CallDBtools();
            string ErrorStep = "Start" + Environment.NewLine;
            IPAddress fip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            // -- oracle -- 
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            String s_conn_oracle = calldbtools.SelectDB("oracle");
            OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
            if (conn_oracle.State == ConnectionState.Open)
                conn_oracle.Close();
            conn_oracle.Open();
            OracleTransaction transaction_oracle = conn_oracle.BeginTransaction();
            List<string> transcmd_oracle = new List<string>();
            List<CallDBtools_Oracle.OracleParam> listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();
            int rowsAffected_oracle = -1;
            int iTransOra = 0;

            // -- msdb -- 
            CallDBtools_MSDb callDbtools_msdb = new CallDBtools_MSDb();
            String s_conn_msdb = calldbtools.SelectDB("msdb");

            SqlConnection conn_msdb = new SqlConnection(s_conn_msdb);
            if (conn_msdb.State == ConnectionState.Open)
                conn_msdb.Close();
            conn_msdb.Open();
            SqlTransaction transaction_msdb = conn_msdb.BeginTransaction();
            List<string> transcmd_msdb = new List<string>();
            List<CallDBtools_MSDb.MSDbParam> listParam_msdb = new List<CallDBtools_MSDb.MSDbParam>();
            int rowsAffected_msdb = -1;
            int iTransMsdb = 0;

            string toMail = "neil132049@mail.ndmctsgh.edu.tw";//"ANGELCHEN@ms.aidc.com.tw";//
            string toMailCC = "";
            string toSubject = "即期藥品更換通知 - 廠商回覆";
            string toContent = "";
            bool sendMailFlag = false;
            SendMail sendmail = new SendMail();

            try
            {
                string msg_MSDB = "error";
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                DataTable dt_MSDB = new DataTable();
                string sqlStr_MSDB = @" SELECT SEQ,
                                               AGEN_NO,
                                               MAIL_NO
                                        FROM ME_MAILBACK
                                        WHERE STATUS = 'A' ";
                l.lg("SelectMS_MemailBACK_IntoOracle_MemailBACK()", sqlStr_MSDB);
                dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);

                if (msg_MSDB == "") //取得 ME_MAILBACK 無錯誤
                {
                    if (dt_MSDB.Rows.Count > 0) //ME_MAILBACK 有資料
                    {
                        toContent = "廠商已回覆本訊息<br>";

                        for (int i = 0; i < dt_MSDB.Rows.Count; i++)
                        {
                            toContent += "<br>(" + (i + 1) + ")" +
                                         "<br>廠商代碼 : " + dt_MSDB.Rows[i]["AGEN_NO"].ToString().Trim() +
                                         "<br>信件代碼 : " + dt_MSDB.Rows[i]["MAIL_NO"].ToString().Trim() + "<br>";

                            string cmdstr = @"UPDATE ME_MAILBACK
                                              SET STATUS = 'C',
                                                  BACK_DT = SYSDATE
                                              WHERE AGEN_NO = :AGEN_NO
                                              AND MAIL_NO = :MAIL_NO";

                            l.lg("SelectMS_MemailBACK_IntoOracle_MemailBACK()", cmdstr);
                            ++iTransOra;
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":AGEN_NO", "VarChar", dt_MSDB.Rows[i]["AGEN_NO"].ToString().Trim()));
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":MAIL_NO", "VarChar", dt_MSDB.Rows[i]["MAIL_NO"].ToString().Trim()));
                            transcmd_oracle.Add(cmdstr);

                            // -- mssql
                            sqlStr_MSDB = @"UPDATE ME_MAILBACK 
                                            SET STATUS = 'B'
                                            WHERE SEQ = @SEQ";

                            l.lg("SelectMS_MemailBACK_IntoOracle_MemailBACK()", sqlStr_MSDB);
                            ++iTransMsdb;
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@SEQ", "VarChar", dt_MSDB.Rows[i]["SEQ"].ToString().Trim()));
                            transcmd_msdb.Add(sqlStr_MSDB);

                        } // end of for (int i = 0; i < dt_MSDB.Rows.Count; i++)

                        sendMailFlag = sendmail.Send_Mail(calldbtools.GetDefaultMailSender(), toMail, toMailCC, "", toSubject, toContent, "", "");
                        if (sendMailFlag)
                        {
                            callDbtools_oralce.I_ERROR_LOG("BHS005", "BHS005-廠商回覆資料寄送失敗" + toContent, "AUTO");
                            l.le("SelectMS_MemailBACK_IntoOracle_MemailBACK()", "BHS005-廠商回覆資料寄送失敗" + toContent);
                        }
                        rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                                            transcmd_oracle,
                                            listParam_oracle,
                                            transaction_oracle,
                                            conn_oracle);
                        l.lg("SelectMS_MemailBACK_IntoOracle_MemailBACK()", "rowsAffected_oracle=" + rowsAffected_oracle);

                        rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
                                transcmd_msdb,
                                listParam_msdb,
                                transaction_msdb,
                                conn_msdb);
                        l.lg("SelectMS_MemailBACK_IntoOracle_MemailBACK()", "rowsAffected_msdb=" + rowsAffected_msdb);

                        transaction_oracle.Commit();
                        conn_oracle.Close();

                        transaction_msdb.Commit();
                        conn_msdb.Close();

                        ErrorStep += ",成功" + Environment.NewLine;
                    } // end of if (dt_MSDB.Rows.Count > 0)

                } // end of if (msg_MSDB == "")
                else //取得 WB_MAILBACK 有錯誤，寫ERROR_LOG
                {
                    CallDBtools_Oracle callDBtools_oracle = new CallDBtools_Oracle();
                    callDBtools_oracle.I_ERROR_LOG("BHS005", "BHS005-廠商回覆接收MAIL資料轉檔失敗 01:" + msg_MSDB, "AUTO");
                }
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BHS005", "BHS005-廠商回覆接收MAIL資料轉檔失敗 01:" + ex.Message, "AUTO");
                l.le("SelectMS_MemailBACK_IntoOracle_MemailBACK()", "BHS005-廠商回覆接收MAIL資料轉檔失敗 01:" + ex.Message);
                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                transaction_msdb.Rollback();
                conn_msdb.Close();
            } // end of try catch
        }

        //將回覆資料寄信通知管理人員
        public void SendMail()
        {

        }
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        FL fl = new FL("BHS005");
        //程式起始 BHS005-廠商回覆接收MAIL資料轉檔
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // 將外網 MSdb ME_MAILBACK 異動資料複製到院內 Oracle ME_MAILBACK
                SelectMS_MemailBACK_IntoOracle_MemailBACK();

                // 寄送mail通知admin有執行程式 
                //fl.sendMailToAdmin("三總排程-BHS005執行完畢", "程式正常執行完畢", l.get資料庫現況Log檔路徑());
            }
            catch (Exception ex)
            {
                // 寄送mail通知admin有執行程式 
                //fl.sendMailToAdmin("三總排程-BHS005執行異常", "Exception=" + ex.Message, l.get資料庫現況Log檔路徑());
                l.le("Form1_Load()", ex.Message);
            }
            this.Close();
        }

    } // ec
}
