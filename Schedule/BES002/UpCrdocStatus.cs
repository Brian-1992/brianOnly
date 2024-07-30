using JCLib.DB;
using JCLib.DB.Tool;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace BES002
{
    class UpCrdocStatus
    {
        L l = new L("BES002");
        public void Run()
        {
            //using (WorkSession session = new WorkSession())
            //{
            //    var DBWork = session.UnitOfWork;
            //    UpCrdocStatusRepository repo = new UpCrdocStatusRepository(DBWork);

            //    DBWork.BeginTransaction();
            //    try
            //    {
            //        int a = repo.UpCrdocStatus();
            //        DBWork.Commit();
            //    }
            //    catch
            //    {
            //        DBWork.Rollback();
            //        throw;
            //    }
            //}

            CallDBtools calldbtools = new CallDBtools();
            string ErrorStep = "Start" + Environment.NewLine;

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

            try
            {
                //begin trans 
                OracleTransaction transaction_oracle = conn_oracle.BeginTransaction();

                string msg_oracle = "error";
                DataTable dt_oralce = new DataTable();
                DataTable dt_oralce_Batno = new DataTable();
                sql_oracle = @" update CR_DOC 
                                   set ORDERTIME = sysdate, ORDERID = '排程',
                                       CR_STATUS = 'E'
                                 where CR_STATUS = 'B' and EMAIL is not null
                              ";
                l.lg("UpCrdocStatus", sql_oracle);

                transcmd_oracle.Add(sql_oracle);

                try
                {
                    

                    rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                                                                                     transcmd_oracle,
                                                                                     listParam_oracle,
                                                                                     transaction_oracle,
                                                                                     conn_oracle);
                    l.lg("UpCrdocStatus", "rowsAffected_oracle=" + rowsAffected_oracle);

                    transaction_oracle.Commit();
                }
                catch (Exception ex) {
                    CallDBtools callDBtools = new CallDBtools();
                    callDbtools_oralce.I_ERROR_LOG("BES002", "BES002-緊急醫療出貨申請產生通知單，更新狀態失敗:" + ex.Message, "AUTO");
                    l.le("BES002", "BES002-緊急醫療出貨申請產生通知單，更新狀態失敗:" + ex.Message);
                    ErrorStep += ",失敗" + Environment.NewLine;
                    ErrorStep += ex.ToString() + Environment.NewLine;
                    transaction_oracle.Rollback();
                    conn_oracle.Close();
                }
                conn_oracle.Close();
            }
            catch (Exception ex) {
                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BES002", "BES002-緊急醫療出貨申請產生通知單 更新狀態失敗:" + ex.Message, "AUTO");
                l.le("BES002", "BES002-緊急醫療出貨申請產生通知單 更新狀態失敗:" + ex.Message);
                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                conn_oracle.Close();
            }
        }
    }
}
