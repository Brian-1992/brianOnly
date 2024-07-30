using System.Collections.Generic;
using System.Data;
//using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.SqlClient;
using System.IO;
using JCLib.DB.Tool;
using System.Text.RegularExpressions;

namespace BDS004
{
    class PH_MAILSP_M
    {
        public string MSGRECNO { get; set; } // 00.Mail訊息流水號(INTEGER, )
        public string MSGTEXT { get; set; } // 01.Mail訊息(VARCHAR2, 4000)
        public string MSGTEXT_NEW { get; set; } // 01.Mail訊息(VARCHAR2, 4000)
        public string CREATE_TIME { get; set; } // 02.建立日期(DATE)
        public string CREATE_USER { get; set; } // 03.建立人員(VARCHAR2, 10)
        public string UPDATE_TIME { get; set; } // 04.異動日期(DATE)
        public string UPDATE_USER { get; set; } // 05.異動人員(VARCHAR2, 10)
        public string UPDATE_IP { get; set; } // 06.異動IP(VARCHAR2, 20)
        public string MSGNO { get; set; } // 07.MSGNO(INTEGER, )
    }
    public class C_BDS004
    {
        #region " 資料庫現況 "

        DataTable PH_MAILSP_M; // 
        DataTable ERROR_LOG;

        String get資料庫現況(String tableTitle)
        {
            l.lg("get資料庫現況()", "");
            String s = "";
            // -- MsSql -- 
            string msg_MSDB = "error";
            CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
            DataTable dt_MSDB = new DataTable();
            string sqlStr_MSDB = "";

            // -- Oracle -- 
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


            // PH_MAILSP_M
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            //sqlStr_MSDB = " select PO_NO, MMCODE, SEQ, FLAG, STATUS from WB_REPLY where convert(varchar(10), UPDATE_TIME, 126)='" + DateTime.Now.ToString("yyyy-MM-dd") + "' "; // convert(nvarchar(19)
            //dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
            //s += l.getHtmlTable("WB_REPLY", dt_MSDB, WB_REPLY, "PO_NO, MMCODE, SEQ"); // AGEN_NO, DNO, MMCODE, PO_NO, SEQ
            //WB_REPLY = dt_MSDB;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = " select MSGNO, MSGRECNO, MSGTEXT, to_char(CREATE_TIME, 'yyyy/mm/dd hh24:mi:ss') CREATE_TIME, CREATE_USER, to_char(UPDATE_TIME, 'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME, UPDATE_USER, UPDATE_IP from PH_MAILSP_M where 1=1 "; // and to_char(UPDATE_TIME, 'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "' "; // to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss')
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "MSGNO, MSGRECNO", null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_MAILSP_M", dt_oralce, PH_MAILSP_M, "MSGNO, MSGRECNO");
            PH_MAILSP_M = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;



            // --  ERROR_LOG
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;

            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = "select * from ERROR_LOG where PG='BDS004' and to_char(LOGTIME, 'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "' order by LOGTIME desc  ";
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

        String getUnitTest單筆測試資料Sql(
            String mmcode,
            String po_no,
            String seq
        )
        {
            l.lg("getUnitTest單筆測試資料Sql()", "");

            String sql = "";
            sql = "insert into WB_REPLY ( "; // delete WB_AIRHIS where convert(VARCHAR, TXTDAY, 111) <>@txtday ";
            sql += " AGEN_NO, " + sBr; // 01.廠商代碼(VARCHAR2,6)
            sql += " DNO, " + sBr; // 02.交貨批次(INTEGER,)
            sql += " MMCODE, " + sBr; // 03.院內碼(VARCHAR2,13)
            sql += " PO_NO, " + sBr; // 04.訂單號碼(VARCHAR2,21)
            //sql += " SEQ, " + sBr; // 05.流水號(INTEGER,)
            sql += " BARCODE, " + sBr; // 06.條碼(VARCHAR2,50)
            sql += " BW_SQTY, " + sBr; // 07.借貨量(INTEGER,)
            sql += " DELI_DT, " + sBr; // 08.預計交貨日(DATE,)
            sql += " EXP_DATE, " + sBr; // 09.效期(DATE,)
            sql += " FLAG, " + sBr; // 10.轉檔標記(VARCHAR2,1)
            sql += " INQTY, " + sBr; // 11.交貨量(INTEGER,)
            sql += " INVOICE, " + sBr; // 12.發票號碼(VARCHAR2,10)
            sql += " LOT_NO, " + sBr; // 13.批號(VARCHAR2,20)
            sql += " MEMO, " + sBr; // 14.備註(VARCHAR2,150)
            sql += " STATUS, " + sBr; // 15.狀態(VARCHAR2,1)
            sql += " CREATE_TIME, " + sBr; // 16.建立日期(DATE,)
            sql += " CREATE_USER, " + sBr; // 17.建立人員(VARCHAR2,10)
            sql += " UPDATE_IP, " + sBr; // 18.異動IP(VARCHAR2,20)
            sql += " UPDATE_TIME, " + sBr; // 19.異動日期(DATE,)
            sql += " UPDATE_USER, " + sBr; // 20.異動人員(VARCHAR2,10)
            sql += " INVOICE_DT, " + sBr; // 21.發票日期(DATE)
            sql = sql.Substring(0, sql.Length - 4);
            sql += " ) values ( " + sBr;
            sql += " '012', " + sBr; // 01.AGEN_NO 廠商代碼(VARCHAR2,6)
            sql += " '1', " + sBr; // 02.DNO交貨批次(INTEGER,)
            sql += " '" + mmcode + "', " + sBr; // 03.MMCODE院內碼(VARCHAR2,13) 會變(如:08001145)                
            sql += " '" + po_no + "', " + sBr; // 04.PO_NO訂單號碼(VARCHAR2,21) (如:INV010805090003)
            //sql += " '" + seq + "', " + sBr; // 05.SEQ流水號(INTEGER,) 流水號 (如:1115)
            sql += " null, " + sBr; // 06.BARCODE條碼(VARCHAR2,50)
            sql += " 3, " + sBr; // 07.BW_SQTY借貨量(INTEGER,)
            sql += " null, " + sBr; // 08.DELI_DT預計交貨日(DATE,)
            sql += " convert(datetime, '2019-08-01 15:42:11', 120), " + sBr; // 09.EXP_DATE效期(DATE,)
            sql += " 'A', " + sBr; // 10.FLAG轉檔標記(VARCHAR2,1)   STATUS='B',   ★★ FLAG='A' 時才會觸發廠商進貨資料轉檔
            sql += " 10, " + sBr; // 11.INQTY交貨量(INTEGER,)
            sql += " '" + seq + "', " + sBr; // 12.INVOICE發票號碼(VARCHAR2,10)
            sql += " '612670', " + sBr; // 13.LOT_NO批號(VARCHAR2,20)
            sql += " 'memo_" + seq + "', " + sBr; // 14.MEMO備註(VARCHAR2,150)
            sql += " 'B', " + sBr; // 15.STATUS狀態(VARCHAR2,1)                   ★★ FLAG='A' 時才會觸發廠商進貨資料轉檔
            sql += " SYSDATETIME(), " + sBr; // 16.CREATE_TIME建立日期(DATE,)
            sql += " '012', " + sBr; // 17.CREATE_USER建立人員(VARCHAR2,10)
            sql += " '192.20.2.67', " + sBr; // 18.UPDATE_IP異動IP(VARCHAR2,20)
            sql += " SYSDATETIME(), " + sBr; // 19.UPDATE_TIME異動日期(DATE,)
            sql += " 'AUTO', " + sBr; // 20.UPDATE_USER異動人員(VARCHAR2,10)
            sql += " SYSDATETIME(), " + sBr; // 21.發票日期(DATE)
            sql = sql.Substring(0, sql.Length - 4);
            sql += " ) ";
            sql = sql.Replace(sBr, "");
            return sql;
        }


        // mode參數說明
        // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
        // 2.建立PH_REPLY_LOG.FLAG='A',PH_REPLY無資料
        // 3.建立PH_REPLY_LOG.FLAG='A',PH_REPLY.STTUS='B'
        // 4.建立PH_REPLY_LOG.FLAG='A',PH_REPLY.STTUS='D'　其他狀態
        void 建立單元測試資料(
            int mode
        )
        {
            l.lg("建立單元測試資料()", "mode=" + mode);

            CallDBtools calldbtools = new CallDBtools();
            String sql = "";
            string ErrorStep = "Start" + Environment.NewLine;


            // -- oracle -- 
            String msg_Oracle = "";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            String s_conn_oracle = calldbtools.SelectDB("oracle");
            OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
            if (conn_oracle.State == ConnectionState.Open)
                conn_oracle.Close();
            conn_oracle.Open();
            OracleTransaction transaction_oracle = conn_oracle.BeginTransaction();
            List<string> transcmd_oracle = new List<string>();
            List<CallDBtools_Oracle.OracleParam> listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();
            int iTransOra = 0;

            // -- msdb -- 
            String msg_MSDB = "";
            CallDBtools_MSDb callDbtools_msdb = new CallDBtools_MSDb();
            String s_conn_msdb = calldbtools.SelectDB("msdb");

            SqlConnection conn_msdb = new SqlConnection(s_conn_msdb);
            if (conn_msdb.State == ConnectionState.Open)
                conn_msdb.Close();
            conn_msdb.Open();
            SqlTransaction transaction_msdb = conn_msdb.BeginTransaction();
            List<string> transcmd_msdb = new List<string>();
            List<CallDBtools_MSDb.MSDbParam> listParam_msdb = new List<CallDBtools_MSDb.MSDbParam>();
            int iTransMsdb = 0;

            try
            {
                if (mode.Equals(1))         // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
                {
                    建立外網資料(ref callDbtools_msdb, ref transcmd_msdb, ref iTransMsdb, ref msg_MSDB);

                    ++iTransOra;
                    sql = "delete from ERROR_LOG where pg='BDS004' and to_char(LOGTIME,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'"; //刪除
                    transcmd_oracle.Add(sql);
                }
                else if (mode.Equals(2))    // 2.建立PH_REPLY_LOG.FLAG='A',PH_REPLY無資料
                {
                    ++iTransOra;
                    sql = "delete from PH_REPLY_LOG where to_char(update_time,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'"; //刪除
                    transcmd_oracle.Add(sql);

                    ++iTransOra;
                    sql = "delete from PH_REPLY where to_char(update_time,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'"; //刪除
                    transcmd_oracle.Add(sql);

                    ++iTransOra;
                    sql = "delete from PH_INVOICE where to_char(update_time,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'"; //刪除
                    transcmd_oracle.Add(sql);
                }
                else if (mode.Equals(3))    // 3.建立PH_REPLY_LOG.FLAG='A',PH_REPLY.STTUS='B'
                {
                    ++iTransMsdb;
                    sql = "update PH_REPLY set STATUS='B' where 1=1 and FLAG='B' and to_char(update_time,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'";
                    transcmd_oracle.Add(sql);
                }
                else if (mode.Equals(4))    // 4.建立PH_REPLY_LOG.FLAG='A',PH_REPLY.STTUS='D'　其他狀態
                {
                    ++iTransMsdb;
                    sql = "update PH_REPLY set STATUS='D' where 1=1 and FLAG='B' and to_char(update_time,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'";
                    transcmd_oracle.Add(sql);
                }
                else if (mode.Equals(5))    // 5.刪除PH_LOTNO資料
                {
                    ++iTransOra;
                    sql = "delete from PH_LOTNO where to_char(update_time,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'"; //刪除
                    transcmd_oracle.Add(sql);
                }
                else if (mode.Equals(6))    // 6.刪除PH_REPLY資料
                {
                    ++iTransOra;
                    sql = "delete from PH_REPLY where to_char(update_time,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'"; //刪除
                    transcmd_oracle.Add(sql);
                }
                else if (mode.Equals(7))    // 7.刪除PH_INVOICE資料
                {
                    foreach (String mmcode in LST_MMCODE_FOR_TEST)
                    {
                        ++iTransOra;
                        sql = "delete from PH_REPLY where to_char(update_time,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "' and MMCODE='" + mmcode + "' "; //刪除
                        transcmd_oracle.Add(sql);
                    }
                }
                else if (mode.Equals(8))    // 8.更新PH_REPLY_LOG.FLAG ='A' 待轉檔
                {
                    ++iTransOra;
                    sql = "update PH_INVOICE set CKSTATUS='T', FLAG='A' where to_char(update_time,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "' ";  // 8.更新 PH_INVOICE.CKSTATUS='T' and FLAG='A'
                    transcmd_oracle.Add(sql);
                }





                int rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
                        transcmd_msdb,
                        listParam_msdb,
                        transaction_msdb,
                        conn_msdb);

                int rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                    transcmd_oracle,
                    listParam_oracle,
                    transaction_oracle,
                    conn_oracle);

                transaction_oracle.Commit();
                conn_oracle.Close();

                transaction_msdb.Commit();
                conn_msdb.Close();

                l.lg("建立單元測試資料()", "ok");
                ErrorStep += ",成功" + Environment.NewLine;
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BDS004", "建立單元測試資料 失敗:" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                transaction_msdb.Rollback();
                conn_msdb.Close();
                l.le("建立單元測試資料()", "Exception=" + ex.Message);
            }
        } //
        void 刪除單元測試資料(
            String table_name
        )
        {
            l.lg("刪除單元測試資料()", "table=" + table_name);

            CallDBtools calldbtools = new CallDBtools();
            String sql = "";
            string ErrorStep = "Start" + Environment.NewLine;


            // -- oracle -- 
            String msg_Oracle = "";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            String s_conn_oracle = calldbtools.SelectDB("oracle");
            OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
            if (conn_oracle.State == ConnectionState.Open)
                conn_oracle.Close();
            conn_oracle.Open();
            OracleTransaction transaction_oracle = conn_oracle.BeginTransaction();
            List<string> transcmd_oracle = new List<string>();
            List<CallDBtools_Oracle.OracleParam> listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();
            int iTransOra = 0;

            // -- msdb -- 
            String msg_MSDB = "";
            CallDBtools_MSDb callDbtools_msdb = new CallDBtools_MSDb();
            String s_conn_msdb = calldbtools.SelectDB("msdb");

            SqlConnection conn_msdb = new SqlConnection(s_conn_msdb);
            if (conn_msdb.State == ConnectionState.Open)
                conn_msdb.Close();
            conn_msdb.Open();
            SqlTransaction transaction_msdb = conn_msdb.BeginTransaction();
            List<string> transcmd_msdb = new List<string>();
            List<CallDBtools_MSDb.MSDbParam> listParam_msdb = new List<CallDBtools_MSDb.MSDbParam>();
            int iTransMsdb = 0;

            try
            {

                ++iTransOra;
                sql = "delete from " + table_name + " where to_char(update_time,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'"; //刪除
                transcmd_oracle.Add(sql);


                int rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
                        transcmd_msdb,
                        listParam_msdb,
                        transaction_msdb,
                        conn_msdb);

                int rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                    transcmd_oracle,
                    listParam_oracle,
                    transaction_oracle,
                    conn_oracle);

                transaction_oracle.Commit();
                conn_oracle.Close();

                transaction_msdb.Commit();
                conn_msdb.Close();

                l.lg("刪除單元測試資料()", "ok");
                ErrorStep += ",成功" + Environment.NewLine;
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BDS004", "刪除單元測試資料 失敗:" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                transaction_msdb.Rollback();
                conn_msdb.Close();
                l.le("刪除單元測試資料()", "Exception=" + ex.Message);
            }
        } //
        //建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
        void 建立外網資料(
            ref CallDBtools_MSDb callDbtools_msdb,
            ref List<string> transcmd_msdb,
            ref int iTransMsdbIdx,
            ref String msg_MSDB
        )
        {
            l.lg("建立外網資料()", "");

            String max_mmcode = callDbtools_msdb.CallExecScalar("select max(MMCODE)+1 MMCODE from WB_REPLY ", null, "msdb", ref msg_MSDB); // 查最大mmcode            
            String max_seq = callDbtools_msdb.CallExecScalar("select max(SEQ)+1 MMCODE from WB_REPLY ", null, "msdb", ref msg_MSDB); // 查最大seq
            String po_no = "SML210808010004";  // "INV0" + (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMddHHmm"); // 範例:INV010805090003
            // -- 刪除flylon今天建的廠商進貨資料 -- 
            ++iTransMsdbIdx;
            String sql = "delete from WB_REPLY where UPDATE_USER='AUTO' and CONVERT(char(10), GetDate(),126)='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
            transcmd_msdb.Add(sql);

            //// -- 建立一筆廠商進貨資料 -- 
            //++iTransMsdbIdx;
            //sql = getUnitTest單筆測試資料Sql(max_mmcode, po_no, max_seq);
            ////listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdbIdx, "@txtday", "VarChar", "2019/05/16"));
            //transcmd_msdb.Add(sql);
            //// -- 建立多筆筆廠商進貨資料 -- 
            //for (int i = 0; i <= 3; i++)
            //{
            //    ++iTransMsdbIdx;
            //    max_mmcode = (int.Parse(max_mmcode) + 1).ToString();
            //    max_seq = (int.Parse(max_seq) + 1).ToString();
            //    sql = getUnitTest單筆測試資料Sql(max_mmcode, po_no, max_seq);
            //    //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdbIdx, "@txtday", "VarChar", "2019/05/16"));
            //    transcmd_msdb.Add(sql);
            //}

            // -- 依清單建立多筆外網廠商進貨資料 -- 
            LST_MMCODE_FOR_TEST.Add("08490052");
            LST_MMCODE_FOR_TEST.Add("08490053");
            LST_MMCODE_FOR_TEST.Add("08490051");
            for (int i = 0; i < LST_MMCODE_FOR_TEST.Count; i++)
            {
                String mmcode = LST_MMCODE_FOR_TEST[i];
                ++iTransMsdbIdx;
                max_mmcode = (int.Parse(max_mmcode) + 1).ToString();
                //max_seq = (int.Parse(max_seq) + 1).ToString();
                max_seq = i.ToString();
                sql = getUnitTest單筆測試資料Sql(mmcode, po_no, max_seq);
                transcmd_msdb.Add(sql);
            }
        } // 
        // -- 依清單建立多筆外網廠商進貨資料 -- 
        List<String> LST_MMCODE_FOR_TEST = new List<string>();


        #endregion


        #region " 單元測試 "

        // --------------
        // -- 單元測試 -- 
        // --------------
        public void 全部單元測試(string[] args)
        {
            l.lg("全部單元測試()", "");

            // -- file 
            String sCurPath = AppDomain.CurrentDomain.BaseDirectory;
            String sDbStatusFilePath = sCurPath + "\\db_status.html";
            if (File.Exists(sDbStatusFilePath))
                File.Delete(sDbStatusFilePath);
            // -- Msdb -- 
            string msg_MSDB = "error";
            CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
            DataTable dt_MSDB = new DataTable();
            string sqlStr_MSDB = "";

            // -- Oracle -- 
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = "";

            // -- 
            執行交易前後比較 c = new 執行交易前後比較();

            //// 02 -- WB_REPLY.STATUS='B',FLAG='A' -> ins PH_REPLY_LOG.FLAG='A' -------------------------------------
            //l.ldb(get資料庫現況("初始化資料庫"));
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //l.ldb(get資料庫現況("建立單元測試資料(1)"));
            //建立單元測試資料(2);  // 2.建立PH_REPLY_LOG.FLAG='A',PH_REPLY無資料
            //l.ldb(get資料庫現況("建立單元測試資料(2)"));
            //l.ldb(get資料庫現況("2.外網WB_REPLY(外網寄售品項資料檔) COPY至院內PH_REPLY_LOG.FLAG='A'(外網廠商交貨回覆資料歷史)"));
            //c.讀取執行交易前Table筆數();

            //Transaction01();

            //c.讀取執行交易後Table筆數();
            //l.ldb(get資料庫現況("Transaction01"));
            //if ( // 檢查結果是否正確
            //    c.i_wb_reply_before.Equals(c.i_ph_reply_log_after) &&
            //    c.i_wb_reply_before.Equals(c.i_wb_reply_after) &&
            //    true
            //)
            //{
            //    Console.WriteLine(l.ldb("單元測試01 ok"));
            //}
            //else
            //{
            //    Console.WriteLine(l.ldb("單元測試01 失敗"));
            //}

        } //

        class 執行交易前後比較
        {
            public int i_wb_reply_before = 0;
            public int i_ph_reply_before = 0;
            public int i_ph_reply_log_before = 0;
            public int i_ph_lotno_before = 0;
            public int i_ph_invoice_before = 0;

            public int i_wb_reply_after = 0;
            public int i_ph_reply_after = 0;
            public int i_ph_reply_log_after = 0;
            public int i_ph_lotno_after = 0;
            public int i_ph_invoice_after = 0;

            public String sql_wb_reply = "select count(*) from WB_REPLY where convert(varchar(10), UPDATE_TIME, 126)='" + DateTime.Now.ToString("yyyy-MM-dd") + "' ";
            public String sql_ph_reply = "select count(*) from PH_REPLY where to_char(UPDATE_TIME, 'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "' ";
            public String sql_ph_reply_log = "select count(*) from PH_REPLY_LOG where to_char(UPDATE_TIME, 'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "' ";
            public String sql_ph_lotno = "select count(*) from PH_LOTNO where to_char(UPDATE_TIME, 'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'  ";
            public String sql_ph_invoice = "select count(*) from PH_INVOICE where to_char(UPDATE_TIME, 'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'  ";

            // -- Msdb -- 
            string msg_MSDB = "error";
            CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
            DataTable dt_MSDB = new DataTable();
            string sqlStr_MSDB = "";

            // -- Oracle -- 
            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = "";

            public void 讀取執行交易前Table筆數()
            {
                i_wb_reply_before = int.Parse(callDBtools_msdb.CallExecScalar(sql_wb_reply, null, "msdb", ref msg_MSDB));
                i_ph_reply_before = int.Parse(callDbtools_oralce.CallExecScalar(sql_ph_reply, null, "oracle", ref msg_MSDB));
                i_ph_reply_log_before = int.Parse(callDbtools_oralce.CallExecScalar(sql_ph_reply_log, null, "oracle", ref msg_MSDB));
                i_ph_lotno_before = int.Parse(callDbtools_oralce.CallExecScalar(sql_ph_lotno, null, "oracle", ref msg_MSDB));
                i_ph_invoice_before = int.Parse(callDbtools_oralce.CallExecScalar(sql_ph_invoice, null, "oracle", ref msg_MSDB));
            } // 
            public void 讀取執行交易後Table筆數()
            {
                i_wb_reply_after = int.Parse(callDBtools_msdb.CallExecScalar(sql_wb_reply, null, "msdb", ref msg_MSDB));
                i_ph_reply_after = int.Parse(callDbtools_oralce.CallExecScalar(sql_ph_reply, null, "oracle", ref msg_MSDB));
                i_ph_reply_log_after = int.Parse(callDbtools_oralce.CallExecScalar(sql_ph_reply_log, null, "oracle", ref msg_MSDB));
                i_ph_lotno_after = int.Parse(callDbtools_oralce.CallExecScalar(sql_ph_lotno, null, "oracle", ref msg_MSDB));
                i_ph_invoice_after = int.Parse(callDbtools_oralce.CallExecScalar(sql_ph_invoice, null, "oracle", ref msg_MSDB));
                Console.WriteLine("wb_reply=" + i_wb_reply_before + "->" + i_wb_reply_after);
                Console.WriteLine("ph_reply=" + i_ph_reply_before + "->" + i_ph_reply_after);
                Console.WriteLine("ph_reply_log=" + i_ph_reply_log_before + "->" + i_ph_reply_log_after);
                Console.WriteLine("ph_lotno=" + i_ph_lotno_before + "->" + i_ph_lotno_after);
                Console.WriteLine("ph_invoice=" + i_ph_invoice_before + "->" + i_ph_invoice_after);
            } // 
        } // 
        #endregion



        #region " Transaction區 "


        // -------------------
        // -- Transaction區 -- 
        // -------------------


        // 1_將外網 MSdb WB_PUT_D 異動資料複製到院內 Oracle PH_PUT_D
        private void Transaction01()
        {
            l.lg("Transaction01()", "");

            CallDBtools calldbtools = new CallDBtools();
            string ErrorStep = "Start" + Environment.NewLine;


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
            int iTransOra = 0;
            try
            {
                DateTime dtNow = DateTime.Now;
                String sDt = (int.Parse(dtNow.ToString("yyyy")) - 1911) + "年" + int.Parse(dtNow.ToString("MM")).ToString() + "月份"; // 抓系統前一個月份字串
                Regex r;

                string msg_oracle = "error";
                //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_ph_mailsp_m = new DataTable();
                string sql_oracle = " select ";
                sql_oracle += sBr + " MSGRECNO, "; // 00.Mail訊息流水號(INTEGER)
                sql_oracle += sBr + " MSGTEXT, "; // 01.Mail訊息(VARCHAR2)
                sql_oracle += sBr + " to_char(CREATE_TIME,'yyyy/mm/dd hh24:mi:ss') CREATE_TIME, "; // 02.建立日期(DATE) 2019-06-19 09:41:50
                sql_oracle += sBr + " CREATE_USER, "; // 03.建立人員(VARCHAR2)
                sql_oracle += sBr + " to_char(UPDATE_TIME,'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME, "; // 04.異動日期(DATE) 2019-06-19 09:41:50
                sql_oracle += sBr + " UPDATE_USER, "; // 05.異動人員(VARCHAR2)
                sql_oracle += sBr + " UPDATE_IP, "; // 06.異動IP(VARCHAR2)
                sql_oracle += sBr + " MSGNO, "; // 07.MSGNO(INTEGER)
                sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 2);
                sql_oracle += " from PH_MAILSP_M a where 1=1 " + sBr;
                dt_ph_mailsp_m = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "MSGNO,MSGRECNO", null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_ph_mailsp_m.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    l.lg("Transaction01()", "dt_ph_mailsp_m.Rows.Count=" + dt_ph_mailsp_m.Rows.Count);
                    int rowsAffected_oracle = -1;
                    for (int i = 0; i < dt_ph_mailsp_m.Rows.Count; i++)
                    {
                        PH_MAILSP_M v = new BDS004.PH_MAILSP_M();
                        v.MSGNO = dt_ph_mailsp_m.Rows[i]["MSGNO"].ToString().Trim();        // key
                        v.MSGRECNO = dt_ph_mailsp_m.Rows[i]["MSGRECNO"].ToString().Trim();  // key
                        v.MSGTEXT = dt_ph_mailsp_m.Rows[i]["MSGTEXT"].ToString().Trim();
                        // 108年9月份E-MAIL訂貨單
                        r = new Regex(@"[0-9]{3}年[0-9]+月份E");
                        if (r.Match(v.MSGTEXT).Success)
                        {
                            v.MSGTEXT_NEW = r.Replace(v.MSGTEXT, sDt + "E");
                        }
                        //108年9月份發票
                        r = new Regex(@"[0-9]{3}年[0-9]+月份發票");
                        if (r.Match(v.MSGTEXT).Success)
                        {
                            v.MSGTEXT_NEW = r.Replace(v.MSGTEXT, sDt + "發票");
                        }
                        //108年9月份驗收單
                        r = new Regex(@"[0-9]{3}年[0-9]+月份驗收單");
                        if (r.Match(v.MSGTEXT).Success)
                        {
                            v.MSGTEXT_NEW = r.Replace(v.MSGTEXT, sDt + "驗收單");
                        }
                        if (!String.IsNullOrEmpty(v.MSGTEXT_NEW))
                        {
                            更新PH_MAILSP_M(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, v);
                        }
                    } // for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                            transcmd_oracle,
                            listParam_oracle,
                            transaction_oracle,
                            conn_oracle);
                    l.lg("Transaction01()", "rowsAffected_oracle=" + rowsAffected_oracle);

                    transaction_oracle.Commit();
                    conn_oracle.Close();
                    l.lg("Transaction01()", "transaction_oracle.Commit();");

                    ErrorStep += ",成功" + Environment.NewLine;
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BDS004", "STEP1-取得PH_MAILSP_M失敗:" + msg_oracle, "AUTO");
                    l.le("Transaction01()", "STEP2-取得PH_MAILSP_M失敗:" + msg_oracle);
                }
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BDS004", "STEP1 更新失敗--" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                //transaction_msdb.Rollback();
                //conn_msdb.Close();

                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS004", "STEP1 更新失敗--" + ex.Message);
                l.le("Transaction01()", "STEP1 更新失敗--" + ex.Message);
            }
        } // 

        

        void 更新PH_MAILSP_M(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            PH_MAILSP_M v
        )
        {
            String sql_oracle = "";
            sql_oracle += " update PH_MAILSP_M set ";
            sql_oracle += " MSGTEXT=:msgtext, " + sBr; 
            sql_oracle += " UPDATE_TIME = SYSDATE, " + sBr;
            sql_oracle += " UPDATE_USER = 'AUTO' " + sBr;
            sql_oracle += " where 1=1  " + sBr;
            sql_oracle += " and MSGNO = :msgno " + sBr; 
            sql_oracle += " and MSGRECNO = :msgrecno " + sBr;
            transcmd_oracle.Add(sql_oracle);
            l.lg("Transaction01()", "sql_oracle=" + sql_oracle);
            l.lg("Transaction01()", "更新MSGTXT「" + v.MSGTEXT + "」->「" + v.MSGTEXT + "」where MSGNO='" + v.MSGNO + "' and MSGRECNO='" + v.MSGRECNO + "' ");
            ++iTransOra;
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":msgtext", "VarChar", v.MSGTEXT_NEW));
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":msgno", "VarChar", v.MSGNO));
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":msgrecno", "VarChar", v.MSGRECNO));
        } // 



        #endregion


        L l = new L("BDS004");
        FL fl = new FL("BDS004");
        String sBr = "\r\n";
        CallDBtools calldbtools = new CallDBtools();

        public C_BDS004(string[] args)
        {
        } // 

        public void run()
        {
            l.lg("run()", "");

            String sCurPath = AppDomain.CurrentDomain.BaseDirectory;
            String sDbStatusFilePath = sCurPath + "\\db_status.html";
            if (File.Exists(sDbStatusFilePath))
                File.Delete(sDbStatusFilePath);
            try
            {
                l.ldb(get資料庫現況("run()"));

                // 1 01.外網 MSdb WB_REPLY -> 院內 Oracle PH_REPLY_LOG, 02.update WB_REPLY.FLAG='B' 已處理
                // delete from PH_MAILSP_M where MSGNO='99'
                Transaction01();
                l.ldb(get資料庫現況("Transaction01()"));

                //// 2 01.更新院內 Oracle PH_AIRHIS -> PH_AIRST(GO取走,GI換入,CH修改)
                //Transaction02();
                //l.ldb(get資料庫現況("Transaction02()"));

                //Transaction03();
                //l.ldb(get資料庫現況("Transaction03()"));

                //Transaction04();
                //l.ldb(get資料庫現況("Transaction04()"));

                // 寄送mail通知admin有執行程式 
                fl.sendMailToAdmin("三總排程-BDS004執行完畢", "程式正常執行完畢", l.get資料庫現況Log檔路徑());
            }
            catch (Exception ex)
            {
                // 寄送mail通知admin有執行程式 
                fl.sendMailToAdmin("三總排程-BDS004執行異常", "Exception=" + ex.Message, l.get資料庫現況Log檔路徑());
                l.le("run()", ex.Message);
            }
        } // 

    } // ec
} // en
