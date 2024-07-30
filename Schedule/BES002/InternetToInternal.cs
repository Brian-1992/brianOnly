using System;
using System.Collections.Generic;
using System.Linq;
using JCLib.DB.Tool;

using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace BES002
{
    class InternetToInternal
    {
        public void Run()
        {
            L l = new L("BES002");

            l.lg("********************************************", "");
            l.lg("外網到內網", "");

            CallDBtools calldbtools = new CallDBtools();
            string ErrorStep = "Start" + Environment.NewLine;
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
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                DataTable dt_msdb = new DataTable();
                string msgDB = "error";
                sql_msdb = @" select CRDOCNO as wbCRDOCNO, REPLYTIME as wbREPLYTIME,
                                     INQTY as wbINQTY, LOT_NO as wbLOT_NO, convert(varchar, EXP_DATE, 111) as wbEXP_DATE,
                                     REPLY_STATUS,IN_STATUS
                                from WB_CR_DOC
                               where REPLY_STATUS='B' or IN_STATUS='B'
                            ";
                Console.WriteLine("撈取外網緊急醫療進貨記錄");
                l.lg("撈取外網緊急醫療進貨記錄", sql_msdb);
                dt_msdb = callDBtools_msdb.CallOpenSQLReturnDT(sql_msdb, null, null, "msdb", "T1", ref msgDB);
                Console.WriteLine(string.Format("資料抓取狀態:{0} 查詢比數:{1}", msgDB, dt_msdb.Rows.Count));
                if (msgDB == "" && dt_msdb.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    Console.WriteLine(string.Format("逐筆處理資料"));
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
                        Console.WriteLine(string.Format("CR_DOCNO = {0}", dt_msdb.Rows[i]["wbCRDOCNO"].ToString()));
                        Console.WriteLine(string.Format("REPLY_STATUS = {0}", dt_msdb.Rows[i]["REPLY_STATUS"].ToString()));
                        if (dt_msdb.Rows[i]["REPLY_STATUS"].ToString() == "B")
                        {
                            Console.WriteLine(string.Format("REPLY_STATUS = B，繼續處理"));
                            Console.WriteLine(string.Format("1. 更新外網 WB_CR_DOC(REPLY_STATUS=C)"));
                            #region 1.更新外網 WB_CR_DOC(REPLY_STATUS=C) 
                            sql_msdb = @"update WB_CR_DOC
                                            set REPLY_STATUS='C'
                                          where CRDOCNO=@crdocno
                                            and REPLY_STATUS='B'
                                        ";
                            l.lg("◆緊急醫療出貨單編號：", dt_msdb.Rows[i]["wbCRDOCNO"].ToString().Trim());
                            l.lg("1.更新外網 WB_CR_DOC(REPLY_STATUS=C)", sql_msdb);                            
                            ++iTransMsdb;
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@crdocno", "varchar", dt_msdb.Rows[i]["wbCRDOCNO"].ToString().Trim()));
                            transcmd_msdb.Add(sql_msdb);
                            #endregion

                            Console.WriteLine(string.Format("2.更新內網 CR_DOC(REPLYTIME)"));
                            #region 2.更新內網 CR_DOC(REPLYTIME)
                            sql_oracle = @"update CR_DOC
                                              set REPLYTIME=to_date(:wbREPLYTIME,'yyyy-mm-dd hh24:mi:ss')
                                            where CRDOCNO=:crdocno
                                           ";
                            l.lg("2.更新內網 CR_DOC(REPLYTIME)", sql_oracle);
                            ++iTransOra;
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":wbREPLYTIME", "VarChar2", dt_msdb.Rows[i]["wbREPLYTIME"].ToString().Trim()));
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":crdocno", "VarChar2", dt_msdb.Rows[i]["wbCRDOCNO"].ToString().Trim()));
                            transcmd_oracle.Add(sql_oracle);
                            #endregion
                        }
                        if (dt_msdb.Rows[i]["IN_STATUS"].ToString() == "B")
                        {
                            Console.WriteLine(string.Format("IN_STATUS = B，繼續處理"));
                            Console.WriteLine(string.Format("1. 更新外網 WB_CR_DOC(IN_STATUS=C)"));
                            #region 1.更新外網 WB_CR_DOC(IN_STATUS=C) 
                            sql_msdb = @"update WB_CR_DOC  
                                            set IN_STATUS='C'
                                          where CRDOCNO=@crdocno
                                            and IN_STATUS='B'
                                        ";
                            l.lg("◆緊急醫療出貨單編號：", dt_msdb.Rows[i]["wbCRDOCNO"].ToString().Trim());
                            l.lg("1.更新外網 WB_CR_DOC(IN_STATUS=C)", sql_msdb);                    
                            ++iTransMsdb;
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@crdocno", "varchar", dt_msdb.Rows[i]["wbCRDOCNO"].ToString().Trim()));
                            transcmd_msdb.Add(sql_msdb);
                            #endregion

                            Console.WriteLine(string.Format("2. 更新內網 CR_DOC(INQTY,LOT_NO,EXP_DATE)"));
                            #region 2.更新內網 CR_DOC(INQTY,LOT_NO,EXP_DATE)
                            sql_oracle = @"update CR_DOC
                                              set INQTY=:wbINQTY,LOT_NO=:wbLOT_NO,
                                                  EXP_DATE=to_date(:wbEXP_DATE,'yyyy/mm/dd')
                                            where CRDOCNO=:crdocno
                                           ";
                            l.lg("2.更新內網 CR_DOC(REPLYTIME)", sql_oracle);
                            ++iTransOra;
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":wbINQTY", "VarChar2", dt_msdb.Rows[i]["wbINQTY"].ToString().Trim()));
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":wbLOT_NO", "VarChar2", dt_msdb.Rows[i]["wbLOT_NO"].ToString().Trim()));
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":wbEXP_DATE", "VarChar2", dt_msdb.Rows[i]["wbEXP_DATE"].ToString().Trim()));
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":crdocno", "VarChar2", dt_msdb.Rows[i]["wbCRDOCNO"].ToString().Trim()));
                            transcmd_oracle.Add(sql_oracle);
                            #endregion

                            #region 3. 新增內網 CR_DOC_D 已註解
                           // sql_oracle = @"insert into CR_DOC_D
                           //                  (CRDOCNO,CR_D_SEQ,LOT_NO,EXP_DATE,INQTY,
                           //                   ISUDI,CREATE_TIME,CREATE_USER)
                           //                select crdocno,
                           //                       CRDOC_D_SEQ.nextval,
                           //                       LOT_NO, EXP_DATE, INQTY,
                           //                       'N', sysdate, '排程BES002'
                           //                  from CR_DOC
                           //                 where crdocno = :crdocno
                           //                ";
                           // l.lg("3.新增內網 CR_DOC_D", sql_oracle);
                           // ++iTransOra;
                           // listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":crdocno", "VarChar2", dt_msdb.Rows[i]["wbCRDOCNO"].ToString().Trim()));
                           //// listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":wbLOT_NO", "VarChar2", dt_msdb.Rows[i]["wbLOT_NO"].ToString().Trim()));
                           //// listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":wbINQTY", "VarChar2", dt_msdb.Rows[i]["wbINQTY"].ToString().Trim()));
                           // //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":wbEXP_DATE", "VarChar2", dt_msdb.Rows[i]["wbEXP_DATE"].ToString().Trim()));
                           // transcmd_oracle.Add(sql_oracle);

                            #endregion
                        }

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
                            l.lg("-----------------------------------------------------------", "");
                            #endregion

                            transaction_oracle.Commit();
                            transaction_msdb.Commit();
                            Console.WriteLine(string.Format("commit"));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("更新 ERROR Message: {0}", ex.Message));
                            Console.WriteLine(string.Format("{0}", ex.ToString()));
                            CallDBtools callDBtools = new CallDBtools();
                            callDbtools_oralce.I_ERROR_LOG("BES002", "BES002-外網緊急醫療進貨記錄、出貨點收資料，外網到內網轉檔失敗:" + ex.Message, "AUTO");
                            l.le("BES002", "BES002-外網緊急醫療進貨記錄、出貨點收資料，外網到內網轉檔失敗:" + ex.Message);
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

                        //    Console.ReadLine();
                        }
                    } //end of for
                    conn_oracle.Close();
                    conn_msdb.Close();

                    ErrorStep += ",成功" + Environment.NewLine;
                    Console.WriteLine(string.Format("處理成功"));
                }
                else //err
                {
                    
                    if (msgDB != "")
                    {
                        Console.WriteLine(string.Format("取得WB_CR_DOC失敗"));
                        CallDBtools_Oracle callDBtools_oracle = new CallDBtools_Oracle();
                        callDBtools_oracle.I_ERROR_LOG("BES002", "取得WB_CR_DOC失敗:" + msgDB, "AUTO");
                    }
                    else {
                        Console.WriteLine(string.Format("無需處理資料"));
                        Console.WriteLine(string.Format(""));
                        l.lg("無需處理資料", "");
                    }
                }

                Console.WriteLine("end");
               // Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("ERROR: {0}", ex.Message));
                Console.WriteLine(string.Format("ERROR content: {0}", ex.ToString()));

                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BES002", "BES002-外網緊急醫療進貨記錄資料轉檔失敗:" + ex.Message, "AUTO");
                l.le("BES002", "BES002-外網緊急醫療進貨記錄資料轉檔失敗:" + ex.Message);
                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;

                conn_oracle.Close();
                Console.WriteLine(string.Format("oracle connection closed"));
                conn_msdb.Close();
                Console.WriteLine(string.Format("msdb connection closed"));
              //  Console.ReadLine();
            }
        }
    }
}
