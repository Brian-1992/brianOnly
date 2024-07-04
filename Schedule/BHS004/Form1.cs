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

namespace BHS004
{
    public partial class Form1 : Form
    {
        #region " 單元測試 "

        DataTable WB_MAILBACK;
        DataTable PH_MAILBACK;
        DataTable MM_PO_M;
        DataTable ERROR_LOG;
        String get資料庫現況(String tableTitle)
        {
            String s = "";
            String s當天 = DateTime.Now.ToString("yyyy/MM/dd");
            // -- 顯示院外 MsSql -- 
            string msg_MSDB = "error";
            CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();

            DataTable dt_MSDB = new DataTable();
            string sqlStr_MSDB = "";

            // -- 顯示院內 --

            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = "";

            s += "<table border=\"1\">" + sBr;
            s += "<tr>" + sBr;
            s += "<td colspan=\"2\">" + tableTitle + "</td>" + sBr;
            s += "</tr>" + sBr;
            s += "<tr>" + sBr;
            s += "<td>院外</td>" + sBr;
            s += "<td>院內</td>" + sBr;
            s += "</tr>" + sBr;


            // WB_MAILBACK, PH_MAILBACK
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            //sqlStr_MSDB = " select SEQ, AGEN_NO, PO_NO, convert(nvarchar(19), BACK_DT, 120) BACK_DT, STATUS, UPDATE_IP from WB_MAILBACK where 1=1 and STATUS='A' and AGEN_NO='102' "; 
            sqlStr_MSDB = " select SEQ, AGEN_NO, PO_NO, STATUS, UPDATE_IP from WB_MAILBACK where 1=1 and AGEN_NO='102' ";
            dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
            s += l.getHtmlTable("WB_MAILBACK", dt_MSDB, WB_MAILBACK, "AGEN_NO, PO_NO");
            WB_MAILBACK = dt_MSDB;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            //sql_oracle = " select SEQ, AGEN_NO, PO_NO, to_char(BACK_DT, 'yyyy/mm/dd') BACK_DT, STATUS, to_char(CREATE_TIME, 'yyyy/mm/dd') CREATE_TIME, UPDATE_IP from PH_MAILBACK where 1=1 and STATUS='A' and AGEN_NO='102' "; 
            sql_oracle = " select SEQ, AGEN_NO, PO_NO, STATUS, UPDATE_IP from PH_MAILBACK where 1=1 and AGEN_NO='102' ";
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_MAILBACK", dt_oralce, PH_MAILBACK, "AGEN_NO, PO_NO");
            PH_MAILBACK = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;


            // MM_PO_M
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">&nbsp;</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            //sql_oracle = " select PO_NO, AGEN_NO, to_char(PO_TIME, 'yyyy/mm/dd') PO_TIME, M_CONTID, PO_STATUS, to_char(CREATE_TIME, 'yyyy/mm/dd') CREATE_TIME, CREATE_USER, to_char(UPDATE_TIME, 'yyyy/mm/dd') UPDATE_TIME, UPDATE_USER, UPDATE_IP, MEMO, ISCONFIRM, ISBACK, PHONE, SMEMO, ISCOPY, SDN, MAT_CLASS, PR_DEPT, WH_NO, XACTION, E_PURTYPE, CONTRACNO from MM_PO_M where 1=1 and AGEN_NO='102' "; 
            sql_oracle = " select PO_NO, AGEN_NO, to_char(UPDATE_TIME, 'yyyy/mm/dd') UPDATE_TIME, UPDATE_USER, UPDATE_IP, ISBACK from MM_PO_M where 1=1 and AGEN_NO='102' ";
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("MM_PO_M", dt_oralce, MM_PO_M, "AGEN_NO, PO_NO");
            MM_PO_M = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;


            // --  ERROR_LOG
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;

            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = "select * from ERROR_LOG where PG='BHS004' and  to_char(LOGTIME,'yyyy/MM/dd')='" + s當天 + "' "; 
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("ERROR_LOG", dt_oralce, ERROR_LOG, "PG, LOGTIME");
            ERROR_LOG = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;


            s += "<tr><td colspan=\"2\">於" + System.DateTime.Now.ToString("yyyy/MM/dd HH: mm: ss") + "執行</td></tr>" + sBr;
            s += "</table>";
            s += "<hr>";
            return s;
        } // 




        void 單元測試()
        {
            String s當天 = DateTime.Now.ToString("yyyy/MM/dd");
            CallDBtools calldbtools = new CallDBtools();
            String sql = "";
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

            try
            {
                // -- oracle -- 
                sql = "delete from PH_MAILBACK where AGEN_NO='102' "; 
                transcmd_oracle.Add(sql);

                sql = "delete from MM_PO_M where AGEN_NO='102' "; 
                transcmd_oracle.Add(sql);

                sql = "delete from ERROR_LOG where PG='BHS004' ";
                transcmd_oracle.Add(sql);

                sql = " insert into MM_PO_M ( PO_NO, AGEN_NO, PO_TIME, M_CONTID, PO_STATUS, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, MEMO, ISCONFIRM, ISBACK, PHONE, SMEMO, ISCOPY, SDN, MAT_CLASS, PR_DEPT, WH_NO, XACTION, E_PURTYPE, CONTRACNO ) values ( " + sBr;
                sql += " 'INVA10805250001', " + sBr; // 00.訂單號碼 PO_NO(VARCHAR2,15)
                sql += " '102', " + sBr; // 01.廠商代碼 AGEN_NO(VARCHAR2,6)
                sql += " sysdate, " + sBr; // 02.訂單時間 PO_TIME(DATE,)
                sql += " '2', " + sBr; // 03.合約識 M_CONTID(VARCHAR2,1)
                sql += " '80', " + sBr; // 04.訂單狀態 PO_STATUS(VARCHAR2,2)
                sql += " sysdate, " + sBr; // 05.建立時間 CREATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 06.建立人員代碼 CREATE_USER(VARCHAR2,8)
                sql += " sysdate, " + sBr; // 07.異動時間 UPDATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 08.異動人員代碼 UPDATE_USER(VARCHAR2,8)
                sql += " '" + fip + "', " + sBr; // 09.異動IP UPDATE_IP(VARCHAR2,20)
                sql += " 'MEMO', " + sBr; // 10.主備註-MAIL內容 MEMO(VARCHAR2,4000)
                sql += " null, " + sBr; // 11.是否確認彙 ISCONFIRM(VARCHAR2,1)
                sql += " 'N', " + sBr; // 12.是否回覆 ISBACK(VARCHAR2,1)
                sql += " '0918851080', " + sBr; // 13.廠商電話 PHONE(VARCHAR2,60)
                sql += " 'SMEMO', " + sBr; // 14.特殊備註-MAIL內容特別註記(紅色顯示) SMEMO(VARCHAR2,4000)
                sql += " 'N', " + sBr; // 15.複製到外網 ISCOPY(VARCHAR2,1)
                sql += " null, " + sBr; // 16.來源單號 SDN(VARCHAR2,21)
                sql += " '02', " + sBr; // 17.物料分類 MAT_CLASS(VARCHAR2,2)
                sql += " '560000', " + sBr; // 18.申購單位(責任中心) PR_DEPT(VARCHAR2,6)
                sql += " '560000', " + sBr; // 19.庫房別 WH_NO(VARCHAR2,8)
                sql += " '1', " + sBr; // 20.申購類別 XACTION(VARCHAR2,1)
                sql += " null, " + sBr; // 21.藥品採購案別 E_PURTYPE(VARCHAR2,1)
                sql += " null, " + sBr; // 22.合約碼	CONTRACNO	 CONTRACNO(VARCHAR2,2)"
                sql = sql.Substring(0, sql.Length - 4);
                sql += ") ";
                transcmd_oracle.Add(sql);                

                rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                    transcmd_oracle,
                    listParam_oracle,
                    transaction_oracle,
                    conn_oracle);


                // -- msdb
                sql = "delete from WB_MAILBACK where AGEN_NO='102' "; 
                transcmd_msdb.Add(sql);

                sql = "insert into WB_MAILBACK (SEQ, AGEN_NO, PO_NO, BACK_DT, STATUS, UPDATE_IP) values ( ";
                sql += " '5', " + sBr; // 00.流水號 SEQ(NUMBER,)
                sql += " '102', " + sBr; // 01.廠商碼 AGEN_NO(VARCHAR2,6)
                sql += " 'INVA10805250001', " + sBr; // 02.訂單編號 PO_NO(VARCHAR2,21)
                sql += " SYSDATETIME(), " + sBr; // 03.回覆接收日期 BACK_DT(DATE,)
                sql += " 'A', " + sBr; // 04.狀態 STATUS(VARCHAR2,1)
                sql += " '" + fip + "', " + sBr; // 05.異動IP UPDATE_IP(VARCHAR2,20)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) ";
                sql = sql.Replace(sBr, "");
                transcmd_msdb.Add(sql);
                
                rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
                        transcmd_msdb,
                        listParam_msdb,
                        transaction_msdb,
                        conn_msdb);

                transaction_oracle.Commit();
                conn_oracle.Close();

                transaction_msdb.Commit();
                conn_msdb.Close();

                ErrorStep += ",成功" + Environment.NewLine;
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BHS004", "BHS004-廠商回覆接收MAIL資料轉檔【單元測試】失敗:" + ex.Message, "AUTO");
                l.le("單元測試()", "BHS004-廠商回覆接收MAIL資料轉檔【單元測試】失敗:" + ex.Message);
                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                transaction_msdb.Rollback();
                conn_msdb.Close();
            }
        } // 

        L l = new L("BHS004");
        String sBr = "\r\n";

        #endregion

        #region " 主程式 "

        // 1_將外網 MSdb WB_MAILBACK 異動資料複製到院內 Oracle PH_MAILBACK
        private void SelectMS_WBmailBACK_IntoOracle_PHmailBACK()
        {
            l.lg("SelectMS_WBmailBACK_IntoOracle_PHmailBACK()", "");
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

            try
            {
                string msg_MSDB = "error";
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                DataTable dt_MSDB = new DataTable();
                string sqlStr_MSDB = " select SEQ, AGEN_NO, PO_NO, convert(nvarchar(19), BACK_DT, 120) BACK_DT, STATUS, UPDATE_IP from WB_MAILBACK where status='A' ";
                l.lg("SelectMS_WBmailBACK_IntoOracle_PHmailBACK()", sqlStr_MSDB);
                dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
                if (msg_MSDB == "") //取得 WB_MAILBACK 無錯誤
                {
                    if (dt_MSDB.Rows.Count > 0) //WB_MAILBACK 有資料
                    {
                        for (int i = 0; i < dt_MSDB.Rows.Count; i++)
                        {
                            string cmdstr = "insert into PH_MAILBACK(SEQ, AGEN_NO, PO_NO, BACK_DT, STATUS, CREATE_TIME, UPDATE_IP) values ( ";
                            cmdstr += "PH_MAILBACK_SEQ.NEXTVAL, ";
                            cmdstr += ":agen_no, ";
                            cmdstr += ":po_no, ";
                            cmdstr += "to_date(:back_dt, 'yyyy-mm-dd hh24:mi:ss'), ";
                            cmdstr += ":status, ";
                            cmdstr += "sysdate, ";
                            cmdstr += ":update_ip, ";
                            cmdstr = cmdstr.Substring(0, cmdstr.Length - 2);
                            cmdstr += ") ";
                            l.lg("SelectMS_WBmailBACK_IntoOracle_PHmailBACK()", cmdstr);
                            ++iTransOra;
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_MSDB.Rows[i]["AGEN_NO"].ToString().Trim()));
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dt_MSDB.Rows[i]["PO_NO"].ToString().Trim()));
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":back_dt", "VarChar", dt_MSDB.Rows[i]["BACK_DT"].ToString().Trim()));
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":status", "VarChar", "A"));
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_ip", "VarChar", dt_MSDB.Rows[i]["UPDATE_IP"].ToString().Trim()));
                            transcmd_oracle.Add(cmdstr);


                            // -- mssql
                            sqlStr_MSDB = "update WB_MAILBACK set STATUS = 'B' ";
                            sqlStr_MSDB += "  where SEQ = @seq and STATUS='A' ";
                            l.lg("SelectMS_WBmailBACK_IntoOracle_PHmailBACK()", sqlStr_MSDB);
                            ++iTransMsdb;
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@seq", "VarChar", dt_MSDB.Rows[i]["SEQ"].ToString().Trim()));
                            transcmd_msdb.Add(sqlStr_MSDB);
                        } // end of for (int i = 0; i < dt_MSDB.Rows.Count; i++)

                        rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                                            transcmd_oracle,
                                            listParam_oracle,
                                            transaction_oracle,
                                            conn_oracle);
                        l.lg("SelectMS_WBmailBACK_IntoOracle_PHmailBACK()", "rowsAffected_oracle=" + rowsAffected_oracle);

                        rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
                                transcmd_msdb,
                                listParam_msdb,
                                transaction_msdb,
                                conn_msdb);
                        l.lg("SelectMS_WBmailBACK_IntoOracle_PHmailBACK()", "rowsAffected_msdb=" + rowsAffected_msdb);

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
                    callDBtools_oracle.I_ERROR_LOG("BHS004", "BHS004-廠商回覆接收MAIL資料轉檔失敗 01:" + msg_MSDB, "AUTO");
                }
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BHS004", "BHS004-廠商回覆接收MAIL資料轉檔失敗 01:" + ex.Message, "AUTO");
                l.le("SelectMS_WBmailBACK_IntoOracle_PHmailBACK()", "BHS004-廠商回覆接收MAIL資料轉檔失敗 01:" + ex.Message);
                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                transaction_msdb.Rollback();
                conn_msdb.Close();
            } // end of try catch
        } // 

        // 2_更新院內 Oracle MM_PO_M
        private void UpdateOracle_MMpoM()
        {
            l.lg("UpdateOracle_MMpoM()", "");

            CallDBtools calldbtools = new CallDBtools();
            String sql = "";
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

            try
            {
                string msg_oracle = "error";
                DataTable dt_oralce = new DataTable();
                string sql_oracle = " select SEQ, AGEN_NO, PO_NO, to_char(BACK_DT, 'yyyy/mm/dd') BACK_DT, STATUS, to_char(CREATE_TIME, 'yyyy/mm/dd') CREATE_TIME, UPDATE_IP from PH_MAILBACK where STATUS='A' ";
                l.lg("UpdateOracle_MMpoM()", sql_oracle);
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "SEQ", null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    {
                        // -- oracle
                        sql_oracle = " update MM_PO_M set ISBACK='Y',UPDATE_USER='AUTO',UPDATE_TIME=sysdate, UPDATE_IP=:update_ip ";
                        sql_oracle += " where PO_NO = :po_no ";
                        l.lg("UpdateOracle_MMpoM()", sql_oracle);
                        ++iTransOra;
                        listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_ip", "VarChar", dt_oralce.Rows[i]["UPDATE_IP"].ToString().Trim()));
                        listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dt_oralce.Rows[i]["PO_NO"].ToString().Trim()));
                        transcmd_oracle.Add(sql_oracle);


                        sql_oracle = "update PH_MAILBACK set STATUS = 'B' ";
                        sql_oracle += "  where STATUS='A' and PO_NO = :po_no ";
                        l.lg("UpdateOracle_MMpoM()", sql_oracle);
                        ++iTransOra;
                        listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dt_oralce.Rows[i]["PO_NO"].ToString().Trim()));
                        transcmd_oracle.Add(sql_oracle);
                    }
                    rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                                            transcmd_oracle,
                                            listParam_oracle,
                                            transaction_oracle,
                                            conn_oracle);
                    l.lg("UpdateOracle_MMpoM()", "rowsAffected_oracle=" + rowsAffected_oracle);

                    rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
                            transcmd_msdb,
                            listParam_msdb,
                            transaction_msdb,
                            conn_msdb);
                    l.lg("UpdateOracle_MMpoM()", "rowsAffected_msdb=" + rowsAffected_msdb);

                    transaction_oracle.Commit();
                    conn_oracle.Close();

                    transaction_msdb.Commit();
                    conn_msdb.Close();

                    ErrorStep += ",成功" + Environment.NewLine;
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BHS004", "BHS004-廠商回覆接收MAIL資料轉檔失敗 02:" + msg_oracle, "AUTO");
                }

            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BHS004", "BHS004-廠商回覆接收MAIL資料轉檔失敗 02:" + ex.Message, "AUTO");
                l.le("SelectMS_WBmailBACK_IntoOracle_PHmailBACK()", "BHS004-廠商回覆接收MAIL資料轉檔失敗 02:" + ex.Message);
                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                transaction_msdb.Rollback();
                conn_msdb.Close();
            } // end of try catch
        } // 

        #endregion

        public Form1()
        {
            InitializeComponent();
        }
        FL fl = new FL("BHS004");
        //程式起始 BHS004-廠商回覆接收MAIL資料轉檔 排程每天 7:00~22:00每30分鐘執行一次
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                l.ldb(get資料庫現況("初始資料庫"));
                //單元測試();
                //l.ldb(get資料庫現況("單元測試"));

                // 1_將外網 MSdb WB_MAILBACK 異動資料複製到院內 Oracle PH_MAILBACK
                SelectMS_WBmailBACK_IntoOracle_PHmailBACK();
                l.ldb(get資料庫現況("01.SelectMS_WBmailBACK_IntoOracle_PHmailBACK"));

                // 2_更新院內 Oracle MM_PO_M
                UpdateOracle_MMpoM();
                l.ldb(get資料庫現況("02.UpdateOracle_MMpoM"));

                // 寄送mail通知admin有執行程式 
                fl.sendMailToAdmin("三總排程-BHS004執行完畢", "程式正常執行完畢", l.get資料庫現況Log檔路徑());
            }
            catch (Exception ex)
            {
                // 寄送mail通知admin有執行程式 
                fl.sendMailToAdmin("三總排程-BHS004執行異常", "Exception=" + ex.Message, l.get資料庫現況Log檔路徑());
                l.le("Form1_Load()", ex.Message);
            }
            this.Close();
        }

    } // ec
} // en
