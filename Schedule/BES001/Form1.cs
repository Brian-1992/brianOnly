using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System;
using System.Text;
using System.Security.Cryptography;
using JCLib.DB.Tool;

using System.Net;
using System.Net.Sockets;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;
using System.Linq;


namespace BES001
{
    public partial class Form1 : Form
    {
        L l = new L("BES001");
        public Form1()
        {
            InitializeComponent();
        }

        //程式起始 BES001-發票缺漏EMAIL發送排程 排程每天 7:00~22:00每30分鐘執行一次
        private void Form1_Load(object sender, EventArgs e)
        {
            //內網到外網
            InternalToInternet();

            //外網到內網
            InternetToInternal();

            this.Close();
        }

        //內網到外網 
        private void InternalToInternet()
        {
            l.lg("********************************************", "");
            l.lg("內網到外網", "");

            CallDBtools calldbtools = new CallDBtools();
            string ErrorStep = "Start" + Environment.NewLine;
            IPAddress fip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            // -- oracle -- 
            #region oracle 連線設定及參數
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            String s_conn_oracle = calldbtools.SelectDB("oracle");
            OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
            if (conn_oracle.State == ConnectionState.Open)
                conn_oracle.Close();
            conn_oracle.Open();
            List<string> transcmd_oracle = new List<string>();
            List<CallDBtools_Oracle.OracleParam> listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();
            int rowsAffected_oracle = -1;
            int iTransOra = 0;
            string sql_oracle = "";
            #endregion

            // -- msdb -- 
            #region msdb 連線設定及參數
            CallDBtools_MSDb callDbtools_msdb = new CallDBtools_MSDb();
            String s_conn_msdb = calldbtools.SelectDB("msdb");
            SqlConnection conn_msdb = new SqlConnection(s_conn_msdb);
            if (conn_msdb.State == ConnectionState.Open)
                conn_msdb.Close();
            conn_msdb.Open();
            List<string> transcmd_msdb = new List<string>();
            List<CallDBtools_MSDb.MSDbParam> listParam_msdb = new List<CallDBtools_MSDb.MSDbParam>();
            int rowsAffected_msdb = -1;
            int iTransMsdb = 0;
            string sql_msdb = "";
            #endregion

            l.lg("撈取廠商未開發票通知資料", "");

            try
            {
                string msg_oracle = "error";
                DataTable dt_oralce = new DataTable();
                DataTable dt_oralce_Batno = new DataTable();
                sql_oracle = @" select distinct EMAIL,AGEN_NO,EMAIL as mail_reciver,UPDATE_USER 
                                  from INVOICE_EMAIL 
                                 where STATUS = 'B'
                                 order by AGEN_NO
                              ";
                l.lg("撈取廠商未開發票通知資料", sql_oracle);
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    for (int i = 0; i < dt_oralce.Rows.Count; i++) //依廠商代碼分類要寄幾封信
                    {

                        //依廠商代碼撈出信件基本資料
                        DataTable dtM = new DataTable();
                        dtM = GetMailBasicData(dt_oralce.Rows[i]["agen_no"].ToString().Trim());

                        //組出信件內容
                        string mailBodyStr = GetMailContent(dtM);

                        SendMail sendmail = new SendMail();
                        bool sendMailFlag = sendmail.Send_Mail(dtM.Rows[0]["PO_NO"].ToString().Trim(), 
                                                               calldbtools.GetDefaultMailSender(), dt_oralce.Rows[i]["EMAIL"].ToString().Trim(), 
                                                               calldbtools.GetDefaultMailCC(), "發票缺漏",
                                                               "三軍總醫院衛材訂購單發票缺漏通知", mailBodyStr, dtM.Rows[0]["UPDATE_USER"].ToString().Trim(),
                                                               "", new List<string>());
                    
                        if (sendMailFlag) //寄件成功
                        {
                            sql_oracle = @" select distinct AGEN_NO,INVOICE_BATNO                                       
                                              from INVOICE_EMAIL 
                                             where AGEN_NO=:agen_no 
                                             order by INVOICE_BATNO
                                          ";
                            List<CallDBtools_Oracle.OracleParam> paraList2 = new List<CallDBtools_Oracle.OracleParam>();
                            paraList2.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", dt_oralce.Rows[i]["agen_no"].ToString()));
                            l.lg("撈取廠商未開發票通知資料_發票缺漏BATNO", sql_oracle);
                            dt_oralce_Batno = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraList2, "oracle", "T1", ref msg_oracle);
                            if (msg_oracle == "" && dt_oralce_Batno.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                            {
                                for (int j = 0; j < dt_oralce_Batno.Rows.Count; j++) //依BATNO更改資料
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
                                    string sql_seq = @" select INVOICE_MAILLOG_SEQ.nextval as SEQ from dual ";
                                    l.lg("撈取INVOICE_MAILLOG_SEQ序號值", sql_seq);
                                    dt_seq = callDbtools_oralce.CallOpenSQLReturnDT(sql_seq, null, null, "oracle", "T1", ref msg_oracle);

                                    #region 1.新增 INVOICE_MAILLOG
                                    sql_oracle = @" insert into INVOICE_MAILLOG(SEQ, LOG_TIME, MAILFROM, MAILTO, MAILCC, 
                                                                        MSUBJECT,MAILTYPE,MAILBODY,CREATE_USER) 
                                            values(:seq, sysdate, :mail_sender, :mail_reciver, null, 
                                                  :subject, '發票缺漏',substr(:mail_body,1,4000),:update_user) 
                                          ";
                                    l.lg("1.新增內網 INVOICE_MAILLOG", sql_oracle);
                                    l.lg("1.INVOICE_MAILLOG_SEQ.SEQ參數：", dt_seq.Rows[0]["seq"].ToString().Trim());
                                    ++iTransOra;
                                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar2", dt_seq.Rows[0]["seq"].ToString().Trim()));
                                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mail_sender", "VarChar2", calldbtools.GetDefaultMailSender()));
                                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mail_reciver", "VarChar2", dt_oralce.Rows[i]["mail_reciver"].ToString().Trim()));
                                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":subject", "VarChar2", "發票缺漏"));
                                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mail_body", "VarChar2", mailBodyStr));
                                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_user", "VarChar2", dt_oralce.Rows[i]["UPDATE_USER"].ToString().Trim()));
                                    transcmd_oracle.Add(sql_oracle);
                                    #endregion

                                    #region 2.更新 INVOICE_EMAIL狀態(STATUS=C 已寄信)                            
                                    sql_oracle = @" update INVOICE_EMAIL 
                                                       set STATUS = 'C',
                                                           EMAIL_DT=sysdate,
                                                           UPDATE_USER='AUTO',
                                                           UPDATE_TIME=sysdate                                 
                                                     where STATUS='B' 
                                                       and AGEN_NO=:agen_no 
                                                       and INVOICE_BATNO=:invoice_batno
                                                  ";
                                    l.lg("2.更新內網 INVOICE_EMAIL(STATUS=C 已寄信)", sql_oracle);
                                    l.lg("2.廠商代號：", dt_oralce_Batno.Rows[j]["AGEN_NO"].ToString().Trim());
                                    l.lg("2.發票缺漏EMAIL Batch no：", dt_oralce_Batno.Rows[j]["INVOICE_BATNO"].ToString().Trim());
                                    ++iTransOra;
                                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar2", dt_oralce_Batno.Rows[j]["AGEN_NO"].ToString().Trim()));
                                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice_batno", "VarChar2", dt_oralce_Batno.Rows[j]["INVOICE_BATNO"].ToString().Trim()));
                                    transcmd_oracle.Add(sql_oracle);
                                    #endregion

                                    #region 3.新增 INVOICE_E_BATNO
                                    //取得發票缺漏寄信BATNO關聯的INVOICE_E_SEQ
                                    DataTable dt_Eseq = new DataTable();
                                    string sql_Eseq = @" select INVOICE_E_SEQ.nextval as SEQ from dual ";
                                    l.lg("撈取INVOICE_E_BATNO序號值", sql_Eseq);
                                    dt_Eseq = callDbtools_oralce.CallOpenSQLReturnDT(sql_Eseq, null, null, "oracle", "T1", ref msg_oracle);

                                    sql_oracle = @" insert into INVOICE_E_BATNO(INVOICE_E_SEQ, INVOICE_BATNO) 
                                            values(:invoice_eseq, :invoice_batno) 
                                          ";
                                    l.lg("3.新增內網 INVOICE_E_BATNO", sql_oracle);
                                    l.lg("3.INVOICE_E_SEQ參數：", dt_Eseq.Rows[0]["seq"].ToString().Trim());
                                    ++iTransOra;
                                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice_eseq", "VarChar2", dt_Eseq.Rows[0]["seq"].ToString().Trim()));
                                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice_batno", "VarChar2", dt_oralce_Batno.Rows[j]["INVOICE_BATNO"].ToString().Trim()));
                                    transcmd_oracle.Add(sql_oracle);
                                    #endregion

                                    #region 4.新增外網 WB_INVOICE_MAILBACK(STATUS=A 待廠商回覆)                               
                                    sql_msdb = @"insert into WB_INVOICE_MAILBACK(INVOICE_BATNO, BACK_DT, STATUS, CREATE_TIME,AGEN_NO) 
                                         values(@invoice_batno,null,'A',getdate(),@agen_no) 
                                        ";
                                    l.lg("4.新增外網 WB_INVOICE_MAILBACK", sql_msdb);
                                    l.lg("4.發票缺漏EMAIL Batch no：", dt_oralce_Batno.Rows[j]["INVOICE_BATNO"].ToString().Trim());
                                    ++iTransMsdb;
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@invoice_batno", "VarChar", dt_oralce_Batno.Rows[j]["INVOICE_BATNO"].ToString().Trim()));
                                    listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@agen_no", "VarChar", dt_oralce_Batno.Rows[j]["AGEN_NO"].ToString().Trim()));
                                    transcmd_msdb.Add(sql_msdb);
                                    #endregion

                                    try
                                    {
                                        #region 更新內網Oracle
                                        rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                                                                                              transcmd_oracle,
                                                                                              listParam_oracle,
                                                                                              transaction_oracle,
                                                                                              conn_oracle);
                                        l.lg("更新內網 Oracle", "rowsAffected_oracle=" + rowsAffected_oracle);
                                        #endregion

                                        #region 更新外網MsDb
                                        rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
                                                                                            transcmd_msdb,
                                                                                            listParam_msdb,
                                                                                            transaction_msdb,
                                                                                            conn_msdb);
                                        l.lg("更新外網 MsDb", "rowsAffected_msdb=" + rowsAffected_msdb);
                                        l.lg("-----------------------------------------------------------", "");
                                        #endregion

                                        transaction_oracle.Commit();
                                        transaction_msdb.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        CallDBtools callDBtools = new CallDBtools();
                                        callDbtools_oralce.I_ERROR_LOG("BES001", "BES001-廠商未開發票回覆接收MAIL資料，內網到外網轉檔失敗:" + ex.Message, "AUTO");
                                        l.le("BES001", "BES001-廠商未開發票回覆接收MAIL資料，內網到外網轉檔失敗:" + ex.Message);
                                        ErrorStep += ",失敗" + Environment.NewLine;
                                        ErrorStep += ex.ToString() + Environment.NewLine;
                                        transaction_oracle.Rollback();
                                        conn_oracle.Close();
                                        transaction_msdb.Rollback();
                                        conn_msdb.Close();
                                    }
                                } //end of for j
                            }
                        }
                        else //寄件失敗
                        {
                            callDbtools_oralce.I_ERROR_LOG("BES001", "寄件失敗:" + dt_oralce.Rows[i]["agen_no"].ToString().Trim(), "AUTO");
                        }
                    } //end of for i
                    conn_oracle.Close();
                    conn_msdb.Close();

                    ErrorStep += ",成功" + Environment.NewLine;
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BES001", "取得INVOICE_EMAIL失敗:" + msg_oracle, "AUTO");
                }
            }
            catch (Exception ex)
            {
                //CallDBtools callDBtools = new CallDBtools();
                //callDBtools.WriteExceptionLog(ex.Message, "BES001", "程式錯誤:");
                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BES001", "BES001-廠商未開發票回覆接收MAIL資料轉檔失敗:" + ex.Message, "AUTO");
                l.le("BES001", "BES001-廠商未開發票回覆接收MAIL資料轉檔失敗:" + ex.Message);
                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                conn_oracle.Close();
                conn_msdb.Close();
            }
        }

        //外網到內網 
        private void InternetToInternal()
        {
            l.lg("********************************************", "");
            l.lg("外網到內網", "");

            CallDBtools calldbtools = new CallDBtools();
            string ErrorStep = "Start" + Environment.NewLine;
            IPAddress fip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);


            // -- oracle -- 
            #region oracle 連線設定及參數
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            String s_conn_oracle = calldbtools.SelectDB("oracle");
            OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
            if (conn_oracle.State == ConnectionState.Open)
                conn_oracle.Close();
            conn_oracle.Open();
            List<string> transcmd_oracle = new List<string>();
            List<CallDBtools_Oracle.OracleParam> listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();
            int rowsAffected_oracle = -1;
            int iTransOra = 0;
            string sql_oracle = "";
            #endregion

            // -- msdb -- 
            #region msdb 連線設定及參數
            CallDBtools_MSDb callDbtools_msdb = new CallDBtools_MSDb();
            String s_conn_msdb = calldbtools.SelectDB("msdb");
            SqlConnection conn_msdb = new SqlConnection(s_conn_msdb);
            if (conn_msdb.State == ConnectionState.Open)
                conn_msdb.Close();
            conn_msdb.Open();
            List<string> transcmd_msdb = new List<string>();
            List<CallDBtools_MSDb.MSDbParam> listParam_msdb = new List<CallDBtools_MSDb.MSDbParam>();
            int rowsAffected_msdb = -1;
            int iTransMsdb = 0;
            string sql_msdb = "";
            #endregion

            l.lg("撈取廠商已收未開發票通知資料", "");

            try
            {
                string msg_oracle = "error";
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                DataTable dt_msdb = new DataTable();
                string msgDB = "error";
                //因WB_INVOICE_MAILBACK會有相同BATNO有按回覆卻無打發票號碼的情況，故回覆日取相同BATNO的最大值
                sql_msdb = @" select INVOICE_BATNO, max(BACK_DT) as BACK_DT 
                                from WB_INVOICE_MAILBACK
                               where STATUS='B'
                               group by INVOICE_BATNO
                            ";
                l.lg("撈取廠商已收未開發票通知資料(STATUS=B 廠商回覆)", sql_msdb);
                dt_msdb = callDBtools_msdb.CallOpenSQLReturnDT(sql_msdb, null, null, "msdb", "T1", ref msgDB);
                if (msgDB == "" && dt_msdb.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    for (int i = 0; i < dt_msdb.Rows.Count; i++)
                    {
                        iTransOra = 0;
                        iTransMsdb = 0;
                        sql_oracle = "";
                        sql_msdb = "";
                        transcmd_oracle.Clear();
                        listParam_oracle.Clear();
                        transcmd_msdb.Clear();
                        listParam_msdb.Clear();

                        OracleTransaction transaction_oracle = conn_oracle.BeginTransaction();
                        SqlTransaction transaction_msdb = conn_msdb.BeginTransaction();

                        #region 1.新增至內網 INVOICE_MAILBACK(STATUS=A 待轉入內網)                        
                        DataTable dt_INVOICE_MAILBACK = new DataTable();

                        sql_oracle = @" select INVOICE_BATNO
                                        from INVOICE_MAILBACK
                                       where INVOICE_BATNO=:invoice_batno                               
                                     ";
                        List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                        paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":invoice_batno", "VarChar", dt_msdb.Rows[i]["INVOICE_BATNO"].ToString()));
                        l.lg("檢查INVOICE_MAILBACK是否已有資料", sql_oracle); //BATNO會有重覆回饋的問題，須先檢查是否已存在INVOICE_MAILBACK
                        dt_INVOICE_MAILBACK = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraList, "oracle", "T1", ref msg_oracle);
                        if (msg_oracle == "" && dt_INVOICE_MAILBACK.Rows.Count == 0) //INVOICE_MAILBACK無資料
                        {
                            sql_oracle = @"insert into INVOICE_MAILBACK(INVOICE_BATNO, BACK_DT, STATUS, CREATE_TIME)
                                           values(:invoice_batno,to_date(:back_dt,'yyyy-mm-dd hh24:mi:ss'),'A',sysdate)
                                          ";
                            l.lg("1.新增至內網 INVOICE_MAILBACK(STATUS=A 待轉入內網)", sql_oracle);
                            l.lg("1.發票缺漏EMAIL Batch no：", dt_msdb.Rows[i]["INVOICE_BATNO"].ToString().Trim());
                            ++iTransOra;
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice_batno", "VarChar", dt_msdb.Rows[i]["INVOICE_BATNO"].ToString().Trim()));
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":back_dt", "VarChar", dt_msdb.Rows[i]["BACK_DT"].ToString().Trim()));
                            transcmd_oracle.Add(sql_oracle);
                        }
                        else
                        {

                            l.lg("1.內網 INVOICE_MAILBACK 已有資料", "");
                        }
                        #endregion

                        #region 2.更新外網 WB_INVOICE_MAILBACK(STATUS=C 已轉內網)
                        sql_msdb = @"update WB_INVOICE_MAILBACK 
                                        set STATUS='C'
                                      where INVOICE_BATNO = @invoice_batno
                                        and STATUS='B'
                                    ";
                        l.lg("2.更新外網 WB_INVOICE_MAILBACK(STATUS=C 已轉內網)", sql_msdb);
                        l.lg("2.發票缺漏EMAIL Batch no：", dt_msdb.Rows[i]["INVOICE_BATNO"].ToString().Trim());
                        ++iTransMsdb;
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@invoice_batno", "varchar", dt_msdb.Rows[i]["INVOICE_BATNO"].ToString().Trim()));                        
                        transcmd_msdb.Add(sql_msdb);
                        #endregion

                        #region 3.更新內網 INVOICE_EMAIL(REPLY_DT,UPDATE_USER='BH0009',UPDATE_TIME)
                        sql_oracle = @" update INVOICE_EMAIL 
                                           set REPLY_DT=to_date(:reply_dt,'yyyy-mm-dd hh24:mi:ss'),UPDATE_USER='BH0009',UPDATE_TIME=sysdate
                                         where INVOICE_BATNO = :invoice_batno
                                      ";
                        l.lg("3.更新內網 INVOICE_EMAIL(REPLY_DT,UPDATE_USER='BH0009',UPDATE_TIME)", sql_oracle);
                        l.lg("3.發票缺漏EMAIL Batch no：", dt_msdb.Rows[i]["INVOICE_BATNO"].ToString().Trim());
                        ++iTransOra;
                        listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":reply_dt", "VarChar", dt_msdb.Rows[i]["BACK_DT"].ToString().Trim()));
                        listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice_batno", "VarChar", dt_msdb.Rows[i]["INVOICE_BATNO"].ToString().Trim()));
                        transcmd_oracle.Add(sql_oracle);
                        #endregion

                        #region 4.更新內網 INVOICE_MAILBACK(STATUS=B 已轉入內網)
                        sql_oracle = @"update INVOICE_MAILBACK 
                                          set STATUS='B',
                                              BACK_DT=to_date(:back_dt,'yyyy-mm-dd hh24:mi:ss')
                                        where INVOICE_BATNO = :invoice_batno 
                                      ";          
                        l.lg("4.更新內網 INVOICE_MAILBACK(STATUS=B 已轉入內網)", sql_oracle);
                        l.lg("4.發票缺漏EMAIL Batch no：", dt_msdb.Rows[i]["INVOICE_BATNO"].ToString().Trim());
                        ++iTransOra;
                        listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":back_dt", "VarChar", dt_msdb.Rows[i]["BACK_DT"].ToString().Trim()));
                        listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice_batno", "VarChar", dt_msdb.Rows[i]["INVOICE_BATNO"].ToString().Trim()));                        
                        transcmd_oracle.Add(sql_oracle);
                        #endregion

                        try
                        {
                            #region 更新內網Oracle
                            rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                                                                                  transcmd_oracle,
                                                                                  listParam_oracle,
                                                                                  transaction_oracle,
                                                                                  conn_oracle);
                            l.lg("更新內網 Oracle", "rowsAffected_oracle=" + rowsAffected_oracle);
                            #endregion

                            #region 更新外網MsDb
                            rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
                                                                                transcmd_msdb,
                                                                                listParam_msdb,
                                                                                transaction_msdb,
                                                                                conn_msdb);
                            l.lg("更新外網 MsDb", "rowsAffected_msdb=" + rowsAffected_msdb);
                            l.lg("-----------------------------------------------------------", "");
                            #endregion

                            transaction_oracle.Commit();
                            transaction_msdb.Commit();
                        }
                        catch (Exception ex)
                        {
                            CallDBtools callDBtools = new CallDBtools();
                            callDbtools_oralce.I_ERROR_LOG("BES001", "BES001-廠商未開發票回覆接收MAIL資料，外網到內網轉檔失敗:" + ex.Message, "AUTO");
                            l.le("BES001", "BES001-廠商未開發票回覆接收MAIL資料，外網到內網轉檔失敗:" + ex.Message);
                            ErrorStep += ",失敗" + Environment.NewLine;
                            ErrorStep += ex.ToString() + Environment.NewLine;
                            transaction_oracle.Rollback();
                            conn_oracle.Close();
                            transaction_msdb.Rollback();
                            conn_msdb.Close();
                        }
                    } //end of for
                    conn_oracle.Close();
                    conn_msdb.Close();

                    ErrorStep += ",成功" + Environment.NewLine;
                }
                else //err
                {
                    if (string.IsNullOrEmpty(msgDB) == false) {
                        CallDBtools_Oracle callDBtools_oracle = new CallDBtools_Oracle();
                        callDBtools_oracle.I_ERROR_LOG("BES001", "取得WB_INVOICE_MAILBACK失敗:" + msgDB, "AUTO");
                    }
                }
            }
            catch (Exception ex)
            {
                // callDBtools.WriteExceptionLog(ex.Message, "BES001", "程式錯誤:");
                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BES001", "BES001-廠商未開發票回覆接收MAIL資料轉檔失敗:" + ex.Message, "AUTO");
                l.le("BES001", "BES001-廠商未開發票回覆接收MAIL資料轉檔失敗:" + ex.Message);
                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                conn_oracle.Close();
                conn_msdb.Close();
            }
        }


        //撈出待寄信件資料
        private DataTable GetMailBasicData(string agen_no)
        {
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = @" select a.PO_NO,a.PO_NO as 訂單號碼,a.MMCODE as 院內碼,a.AGEN_NO,a.MMNAME_C as 中文名稱,a.MMNAME_E as 英文名稱,
                                          a.PO_QTY as 訂單數量,a.DELI_QTY as 進貨數量,twn_date(a.DELI_DT) as 進貨日期,a.INVOICE_BATNO,
                                          (select AGEN_NAMEC from PH_VENDER where AGEN_NO=a.AGEN_NO) as AGEN_NAMEC,UPDATE_USER
                                     from INVOICE_EMAIL a 
                                    where a.AGEN_NO=:agen_no
                                      and a.status = 'B'
                                    order by a.INVOICE_BATNO,a.PO_NO,a.MMCODE
                                 ";
            List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
            paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", agen_no));
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraListA, "oracle", "T1", ref msg_oracle);
            if (msg_oracle != "")
                callDbtools_oralce.I_ERROR_LOG("BES001", "取得待寄信件資料失敗:" + msg_oracle, "AUTO");
            return dt_oralce;
        }

        //組出信件內容
        private string GetMailContent(DataTable dtM)
        {
            string mail_body = "";
            string headerStr = "";
            mail_body += "<P align=center><font size=4 face=新細明體 color=black>廠商未開發票明細表" + headerStr + " </font></p><br><br>";
            mail_body += "廠商編號：" + dtM.Rows[0]["AGEN_NO"].ToString().Trim() + " <br>";
            mail_body += "廠商名稱：" + dtM.Rows[0]["AGEN_NAMEC"].ToString().Trim() + " <br>";

            //回覆函 URL + 參數加密 串接
            CallDBtools calldbtools = new CallDBtools();
            string urlStr = calldbtools.GetInternetWebServerIP();
            //string urlStr = "http://localhost:54835";
            urlStr += "/Form/Show/B/BH0009?";
            //參數加密格式 po_no = INV010712270196 & agen_no = 826 & invoice_batno = 26            
            string enCodeStr = "po_no=" + dtM.Rows[0]["PO_NO"].ToString().Trim() + "&agen_no=" + dtM.Rows[0]["AGEN_NO"].ToString().Trim();
            for (int i = 0; i < dtM.Rows.Count; i++) //依廠商代碼找出INVOICE_BATNO，串接起來解碼
            {
                if (i == 0)
                {
                    enCodeStr = enCodeStr + "&invoice_batno=" + dtM.Rows[i]["INVOICE_BATNO"].ToString().Trim();
                }
                else
                if ((i > 0) && (dtM.Rows[i - 1]["INVOICE_BATNO"].ToString().Trim() != dtM.Rows[i]["INVOICE_BATNO"].ToString().Trim()))
                {
                    enCodeStr = enCodeStr + "," + dtM.Rows[i]["INVOICE_BATNO"].ToString().Trim();
                }
            }
            urlStr += EnCode(enCodeStr);

            mail_body += "<font size=3 face=新細明體 color=red>謝謝您的合作,請按一下回覆----></font><a href='" + urlStr + "'>回覆函</a>" + " <br>";

            mail_body += "<table style=\"border-collapse: collapse; width: 100%;border:2px #000000 solid;\" border=\"1\">";
            mail_body += "  <tbody>";
            mail_body += "    <tr>";
            mail_body += "      <td style=\"width: auto;\">訂單號碼</td>";
            mail_body += "      <td style=\"width: auto;\">院內碼</td>";
            mail_body += "      <td style=\"width: auto;\">中文名稱</td>";
            mail_body += "      <td style=\"width: auto;\">英文名稱</td>";
            mail_body += "      <td style=\"width: auto;\">訂單數量</td>";
            mail_body += "      <td style=\"width: auto;\">進貨數量</td>";
            mail_body += "      <td style=\"width: auto;\">進貨日期</td>";
            mail_body += "    </tr>";
            foreach (DataRow datarow in dtM.Rows)
            {
                mail_body += "    <tr>";
                mail_body += "      <td>" + datarow["訂單號碼"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["院內碼"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["中文名稱"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["英文名稱"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["訂單數量"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["進貨數量"].ToString().Trim() + "</td>";
                mail_body += "      <td>" + datarow["進貨日期"].ToString().Trim() + "</td>";
                mail_body += "    </tr>";
            }
            mail_body += "  </tbody>";
            mail_body += "</table><br><br>";
            mail_body += "<p align=left><font color=red size=3 face=標楷體>***本郵件為系統自動發送，請勿直接回信*** </font></p><br>";
            return mail_body;
        }

        //參數值加密 po_no=INV010712270196&agen_no=826&invoice_batno=25
        private string EnCode(string EnString)
        {
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

