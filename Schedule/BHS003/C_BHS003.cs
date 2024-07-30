using System.Collections.Generic;
using System.Data;
//using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.SqlClient;
using System.IO;
using JCLib.DB.Tool;

namespace BHS003
{
    class C_BHS003
    {

        #region " 資料庫現況 "

        DataTable WB_REPLY;
        DataTable PH_REPLY;
        DataTable PH_REPLY_LOG;
        DataTable PH_LOTNO;
        DataTable PH_INVOICE; // PH_INVOICE(發票資料檔)
        DataTable ERROR_LOG;
        String get資料庫現況(String tableTitle)
        {
            //l.lg("get資料庫現況()", "");
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


            // WB_REPLY, PH_REPLY
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sqlStr_MSDB = " select PO_NO, MMCODE, SEQ, FLAG, STATUS from WB_REPLY where convert(varchar(10), UPDATE_TIME, 126)='" + DateTime.Now.ToString("yyyy-MM-dd") + "' "; // convert(nvarchar(19)
            dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
            s += l.getHtmlTable("WB_REPLY", dt_MSDB, WB_REPLY, "PO_NO, MMCODE, SEQ"); // AGEN_NO, DNO, MMCODE, PO_NO, SEQ
            WB_REPLY = dt_MSDB;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = " select PO_NO, MMCODE, SEQ, FLAG, STATUS, MEMO, to_char(UPDATE_TIME, 'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME from PH_REPLY where to_char(UPDATE_TIME, 'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "' "; // to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss')
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_REPLY", dt_oralce, PH_REPLY, "PO_NO, MMCODE, SEQ");
            PH_REPLY = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;


            // PH_REPLY_LOG, PH_LOTNO
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = "select PO_NO, MMCODE, SEQ, FLAG, STATUS, LOGTIME, LOT_NO, INVOICE, INQTY, BW_SQTY, to_char(EXP_DATE,'yyyy/mm/dd') EXP_DATE from PH_REPLY_LOG where to_char(UPDATE_TIME, 'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "' ";
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_REPLY_LOG", dt_oralce, PH_REPLY_LOG, "PO_NO, MMCODE, SEQ, LOGTIME");
            PH_REPLY_LOG = dt_oralce;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = "select PO_NO, MMCODE, QTY, SOURCE, STATUS from PH_LOTNO where to_char(UPDATE_TIME, 'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "' ";
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_LOTNO", dt_oralce, PH_LOTNO, "PO_NO, MMCODE");
            PH_LOTNO = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;

            // PH_INVOICE
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = "select PO_NO, MMCODE, TRANSNO, CKSTATUS, FLAG, INVOICE, INVOICE_DT, CKIN_QTY, MEMO from PH_INVOICE where to_char(UPDATE_TIME, 'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "' ";
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_INVOICE", dt_oralce, PH_INVOICE, "PO_NO, MMCODE, TRANSNO");
            PH_INVOICE = dt_oralce;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">&nbsp;</td>" + sBr;
            s += "</tr>" + sBr;


            // --  ERROR_LOG
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;

            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = "select * from ERROR_LOG where PG='BHS003' and to_char(LOGTIME, 'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "' order by LOGTIME desc  ";
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
                    sql = "delete from ERROR_LOG where pg='BHS003' and to_char(LOGTIME,'yyyy/mm/dd')='" + DateTime.Now.ToString("yyyy/MM/dd") + "'"; //刪除
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
                callDbtools_oralce.I_ERROR_LOG("BHS003", "建立單元測試資料 失敗:" + ex.Message, "AUTO");

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
                callDbtools_oralce.I_ERROR_LOG("BHS003", "刪除單元測試資料 失敗:" + ex.Message, "AUTO");

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


            //// -- new 03-01 (PH_LOTNO沒資料) --------------------------------------- 
            //l.ldb(get資料庫現況("單元測試02"));
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //建立單元測試資料(2);  // 2.建立PH_REPLY_LOG.FLAG='A',PH_REPLY無資料
            //建立單元測試資料(5);  // 5.刪除PH_LOTNO資料
            //l.ldb(get資料庫現況("2.外網WB_REPLY(外網寄售品項資料檔) COPY至院內PH_REPLY_LOG.FLAG='A'(外網廠商交貨回覆資料歷史)"));
            //Transaction01();
            //l.ldb(get資料庫現況("Transaction01"));
            //c.讀取執行交易前Table筆數();

            //Transaction02();

            //c.讀取執行交易後Table筆數();
            //l.ldb(get資料庫現況("Transaction02"));
            //if ( // 檢查結果是否正確
            //    c.i_ph_reply_log_before.Equals(c.i_ph_lotno_after) &&
            //    true
            //)
            //{
            //    Console.WriteLine(l.ldb("單元測試03-01 ph_lotno(沒資料) ok"));
            //}
            //else
            //{
            //    Console.WriteLine(l.ldb("單元測試03-01 ph_lotno(沒資料) 失敗"));
            //}


            //// -- new 03-01 (PH_LOTNO已有資料) --------------------------------------- 
            //l.ldb(get資料庫現況("單元測試02"));
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //建立單元測試資料(2);  // 2.建立PH_REPLY_LOG.FLAG='A',PH_REPLY無資料
            //l.ldb(get資料庫現況("2.外網WB_REPLY(外網寄售品項資料檔) COPY至院內PH_REPLY_LOG.FLAG='A'(外網廠商交貨回覆資料歷史)"));
            //Transaction01();
            //l.ldb(get資料庫現況("Transaction01"));
            //Transaction02();
            //l.ldb(get資料庫現況("Transaction02"));
            //c.讀取執行交易前Table筆數();

            //Transaction02();

            //c.讀取執行交易後Table筆數();
            //l.ldb(get資料庫現況("Transaction02"));
            //if ( // 檢查結果是否正確
            //    c.i_ph_reply_log_before.Equals(c.i_ph_lotno_after) &&
            //    true
            //)
            //{
            //    Console.WriteLine(l.ldb("單元測試03-01 ph_lotno(沒資料) ok"));
            //}
            //else
            //{
            //    Console.WriteLine(l.ldb("單元測試03-01 ph_lotno(沒資料) 失敗"));
            //}


            //// -- new 03-02 (PH_REPLY沒資料) --------------------------------------- 
            //l.ldb(get資料庫現況("單元測試02"));
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //建立單元測試資料(2);  // 2.建立PH_REPLY_LOG.FLAG='A',PH_REPLY無資料
            //建立單元測試資料(6);  // 5.刪除PH_REPLY資料
            //l.ldb(get資料庫現況("2.外網WB_REPLY(外網寄售品項資料檔) COPY至院內PH_REPLY_LOG.FLAG='A'(外網廠商交貨回覆資料歷史)"));
            //Transaction01();
            //l.ldb(get資料庫現況("Transaction01"));
            //c.讀取執行交易前Table筆數();

            //Transaction02();

            //c.讀取執行交易後Table筆數();
            //l.ldb(get資料庫現況("Transaction02"));
            //if ( // 檢查結果是否正確
            //    c.i_ph_reply_log_before.Equals(c.i_ph_lotno_after) &&
            //    true
            //)
            //{
            //    Console.WriteLine(l.ldb("單元測試03-02 PH_REPLY(沒資料) ok"));
            //}
            //else
            //{
            //    Console.WriteLine(l.ldb("單元測試03-02 PH_REPLY(沒資料) 失敗"));
            //}


            //// -- new 03-02-01 (PH_REPLY沒資料)(PH_INVOICE沒資料) --------------------------------------- 
            //l.ldb(get資料庫現況("單元測試02"));
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //建立單元測試資料(2);  // 2.建立PH_REPLY_LOG.FLAG='A',PH_REPLY無資料
            //刪除單元測試資料("PH_REPLY");  // 6.刪除PH_REPLY資料
            //刪除單元測試資料("PH_INVOICE");
            //建立單元測試資料(7);
            //l.ldb(get資料庫現況("2.外網WB_REPLY(外網寄售品項資料檔) COPY至院內PH_REPLY_LOG.FLAG='A'(外網廠商交貨回覆資料歷史)"));
            //Transaction01();
            //l.ldb(get資料庫現況("Transaction01"));
            //c.讀取執行交易前Table筆數();

            //Transaction02();

            //c.讀取執行交易後Table筆數();
            //l.ldb(get資料庫現況("Transaction02"));
            //if ( // 檢查結果是否正確
            //    c.i_ph_invoice_before.Equals(c.i_ph_invoice_after) &&
            //    true
            //)
            //{
            //    Console.WriteLine(l.ldb("單元測試03-02-01 (PH_REPLY沒資料)(PH_INVOICE沒資料) ok"));
            //}
            //else
            //{
            //    Console.WriteLine(l.ldb("單元測試03-02-01 (PH_REPLY沒資料)(PH_INVOICE沒資料) 失敗"));
            //}


            //// -- new 03-02-02 (PH_REPLY沒資料)(PH_INVOICE有資料) --------------------------------------- 
            //l.ldb(get資料庫現況("單元測試02"));
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //建立單元測試資料(2);  // 2.建立PH_REPLY_LOG.FLAG='A',PH_REPLY無資料
            //刪除單元測試資料("PH_REPLY");  // 6.刪除PH_REPLY資料
            //刪除單元測試資料("PH_INVOICE");
            //建立單元測試資料(7);
            //l.ldb(get資料庫現況("2.外網WB_REPLY(外網寄售品項資料檔) COPY至院內PH_REPLY_LOG.FLAG='A'(外網廠商交貨回覆資料歷史)"));
            //Transaction01();
            //Transaction02();
            //l.ldb(get資料庫現況("Transaction01與Transaction02"));
            //c.讀取執行交易前Table筆數();

            //Transaction02();

            //c.讀取執行交易後Table筆數();
            //l.ldb(get資料庫現況("Transaction02"));
            //if ( // 檢查結果是否正確
            //    c.i_ph_invoice_before.Equals(c.i_ph_invoice_after) &&
            //    true
            //)
            //{
            //    Console.WriteLine(l.ldb("單元測試03-02-01 (PH_REPLY沒資料)(PH_INVOICE有資料) ok"));
            //}
            //else
            //{
            //    Console.WriteLine(l.ldb("單元測試03-02-01 (PH_REPLY沒資料)(PH_INVOICE有資料) 失敗"));
            //}


            //// -- new 03-02-03 (更新PH_REPLY_LOG.FLAG='B'已轉檔) --------------------------------------- 
            //l.ldb(get資料庫現況("單元測試02"));
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //建立單元測試資料(2);  // 2.建立PH_REPLY_LOG.FLAG='A',PH_REPLY無資料
            //刪除單元測試資料("PH_LOTNO");
            //刪除單元測試資料("PH_REPLY");  // 6.刪除PH_REPLY資料
            //刪除單元測試資料("PH_INVOICE");
            //建立單元測試資料(7);
            //Transaction01();
            //l.ldb(get資料庫現況("2.外網WB_REPLY(外網寄售品項資料檔) COPY至院內PH_REPLY_LOG.FLAG='A'(外網廠商交貨回覆資料歷史)"));
            //c.讀取執行交易前Table筆數();

            //Transaction02();

            //c.讀取執行交易後Table筆數();
            //l.ldb(get資料庫現況("Transaction02"));
            //if ( // 檢查結果是否正確
            //    c.i_ph_reply_log_before.Equals(c.i_ph_reply_log_after) &&
            //    true
            //)
            //{
            //    Console.WriteLine(l.ldb("單元測試03-02-03 (更新PH_REPLY_LOG.FLAG='B'已轉檔) ok"));
            //}
            //else
            //{
            //    Console.WriteLine(l.ldb("單元測試03-02-03 (更新PH_REPLY_LOG.FLAG='B'已轉檔) 失敗"));
            //}


            //// -- new 03-02-04 (PH_LOTNO, PH_REPLY, PH_INVOICE修改)(更新PH_REPLY_LOG.FLAG='B'已轉檔) --------------------------------------- 
            //l.ldb(get資料庫現況("Step2"));
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //刪除單元測試資料("PH_REPLY_LOG");
            //刪除單元測試資料("PH_LOTNO");
            //刪除單元測試資料("PH_REPLY");  // 6.刪除PH_REPLY資料
            //刪除單元測試資料("PH_INVOICE");
            //建立單元測試資料(7); // 刪除PH_INVOICE
            //Transaction01();
            //Transaction02();
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //Transaction01();
            //l.ldb(get資料庫現況("Transaction02 第一次執行完畢"));
            //c.讀取執行交易前Table筆數();

            //Transaction02();

            //c.讀取執行交易後Table筆數();
            //l.ldb(get資料庫現況("03-02-04 (PH_LOTNO, PH_REPLY, PH_INVOICE修改)(更新PH_REPLY_LOG.FLAG='B'已轉檔)"));
            //if ( // 檢查結果是否正確
            //    c.i_ph_reply_log_before.Equals(c.i_ph_reply_log_after) &&
            //    true
            //)
            //{
            //    Console.WriteLine(l.ldb("單元測試03-02-04 (PH_LOTNO, PH_REPLY, PH_INVOICE修改)(更新PH_REPLY_LOG.FLAG='B'已轉檔) ok"));
            //}
            //else
            //{
            //    Console.WriteLine(l.ldb("單元測試03-02-04 (PH_LOTNO, PH_REPLY, PH_INVOICE修改)(更新PH_REPLY_LOG.FLAG='B'已轉檔) 失敗"));
            //}


            //// --------------------------------------------------
            //// -- new Step3--------------------------------------- 
            //l.ldb(get資料庫現況("Step3"));
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //刪除單元測試資料("PH_REPLY_LOG");
            //刪除單元測試資料("PH_LOTNO");
            //刪除單元測試資料("PH_REPLY");  // 6.刪除PH_REPLY資料
            //刪除單元測試資料("PH_INVOICE");
            //建立單元測試資料(7); // 刪除PH_INVOICE
            //Transaction01();
            //Transaction02();
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //Transaction01();
            //l.ldb(get資料庫現況("Transaction01->02->01 第一次執行完畢"));
            //c.讀取執行交易前Table筆數();

            //Transaction03();

            //c.讀取執行交易後Table筆數();
            //l.ldb(get資料庫現況("Step3"));
            //if ( // 檢查結果是否正確
            //    c.i_ph_invoice_before.Equals(c.i_ph_invoice_after) &&
            //    c.i_ph_reply_before.Equals(c.i_ph_reply_after) &&
            //    c.i_ph_reply_log_before.Equals(c.i_ph_reply_log_after) &&
            //    true
            //)
            //{
            //    Console.WriteLine(l.ldb("單元測試 Step3 ok"));
            //}
            //else
            //{
            //    Console.WriteLine(l.ldb("單元測試 Step3 失敗"));
            //}


            //// --------------------------------------------------
            //// -- new Step4--------------------------------------- 
            //l.ldb(get資料庫現況("單元測試04"));
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //刪除單元測試資料("PH_REPLY_LOG");
            //刪除單元測試資料("PH_LOTNO");
            //刪除單元測試資料("PH_REPLY");  // 6.刪除PH_REPLY資料
            //刪除單元測試資料("PH_INVOICE");
            //建立單元測試資料(7); // 刪除PH_INVOICE
            //Transaction01();
            //Transaction02();
            //建立單元測試資料(1);  // 1.建立外網資料(WB_REPLY.STATUS='B',FLAG='A')
            //Transaction01();
            //Transaction03();
            //建立單元測試資料(8);  // 8.更新 PH_INVOICE.CKSTATUS='T' and FLAG='A'

            //l.ldb(get資料庫現況("執行Step4前"));
            //c.讀取執行交易前Table筆數();

            //Transaction04();

            //c.讀取執行交易後Table筆數();
            //l.ldb(get資料庫現況("Step4"));
            //if ( // 檢查結果是否正確
            //    c.i_wb_reply_before.Equals(c.i_wb_reply_after) &&
            //    true
            //)
            //{
            //    Console.WriteLine(l.ldb("單元測試 Step4 ok"));
            //}
            //else
            //{
            //    Console.WriteLine(l.ldb("單元測試 Step4 失敗"));
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


        #region " 副程式整理 "

        void 新增PH_LOTNO(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr
        )
        {
            String sql_oracle = "";
            sql_oracle += " INSERT INTO PH_LOTNO (PO_NO, MMCODE, LOT_NO, EXP_DATE, QTY, MEMO, STATUS, SOURCE,  CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) " + sBr;
            sql_oracle += " select PO_NO, MMCODE, LOT_NO, EXP_DATE, ";
            sql_oracle += " (INQTY- BW_SQTY), "; // QTY(數量)=INQTY(交貨量)-BW_SQTY(借貨量)
            sql_oracle += " MEMO, 'N', " + sBr;
            sql_oracle += " 'V', " + sBr; // STATUS
            sql_oracle += " CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP " + sBr;
            sql_oracle += " From PH_REPLY_LOG  " + sBr;
            sql_oracle += " where 1=1  " + sBr;
            sql_oracle += " and LOGTIME = :logtime " + sBr;     // 01.轉檔日期(VARCHAR2,Y)
            sql_oracle += " and AGEN_NO = :agen_no " + sBr;     // 02.廠商代碼 AGEN_NO(VARCHAR2,Y)
            sql_oracle += " and PO_NO = :po_no " + sBr;         // 03.訂單號碼(VARCHAR2,Y)
            sql_oracle += " and DNO = :dno " + sBr;             // 04.交貨批次(Integer)
            sql_oracle += " and MMCODE = :mmcode " + sBr;       // 05.院內碼(VARCHAR2,Y)
            sql_oracle += " and SEQ = :seq " + sBr;             // 06.流水號(INTEGER,Y)
            sql_oracle += " and LOT_NO is not null " + sBr;

            transcmd_oracle.Add(sql_oracle);
            //l.lg("Transaction02()", "sql_oracle=" + sql_oracle);
            ++iTransOra;
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":logtime", "VarChar", dr["LOGTIME"].ToString().Trim())); // 01.轉檔日期(VARCHAR2,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dr["AGEN_NO"].ToString().Trim())); // 02.廠商代碼(VARCHAR2,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dr["PO_NO"].ToString().Trim()));     // 03.訂單號碼(VARCHAR2,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dno", "VarChar", dr["DNO"].ToString().Trim()));         // 04.交貨批次(INTEGER,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dr["MMCODE"].ToString().Trim()));   // 05.院內碼(VARCHAR2,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dr["SEQ"].ToString().Trim()));         // 06.流水號(INTEGER,Y)
        } // 




        void 更新PH_LOTNO(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log
        )
        {
            int INQTY = 0;
            int BW_SQTY = 0;
            if (
                int.TryParse(dr_ph_reply_log["INQTY"].ToString(), out INQTY) &&
                int.TryParse(dr_ph_reply_log["BW_SQTY"].ToString(), out BW_SQTY)
            )
            {
                int INQTY_MINUS_BW_SQTY = INQTY - BW_SQTY;  // 交貨量 INQTY-借貨量 BW_SQTY
                String sql_oracle = "";
                sql_oracle += " update PH_LOTNO set ";
                sql_oracle += " QTY=QTY+:inqty_minus_bw_sqty " + sBr; // QTY=QTY+{record[INQTY- BW_SQTY]}
                sql_oracle += " where 1=1  " + sBr;
                sql_oracle += " and PO_NO = :po_no " + sBr;         // 03.訂單號碼(VARCHAR2,Y)
                sql_oracle += " and MMCODE = :mmcode " + sBr;       // 05.院內碼(VARCHAR2,Y)
                sql_oracle += " and LOT_NO = :lot_no " + sBr;       // 05.批號(VARCHAR2,Y)
                sql_oracle += " and to_char(EXP_DATE,'yyyy/mm/dd hh24:mi:ss') = :exp_date " + sBr;       // 05.效期(DATE)
                transcmd_oracle.Add(sql_oracle);
                //l.lg("Transaction02()", "sql_oracle=" + sql_oracle);
                ++iTransOra;
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":inqty_minus_bw_sqty", "VarChar", INQTY_MINUS_BW_SQTY.ToString()));     // 交貨量 INQTY-借貨量 BW_SQTY
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));     // 03.訂單號碼(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));   // 05.院內碼(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":lot_no", "VarChar", dr_ph_reply_log["LOT_NO"].ToString().Trim()));         // 06.流水號(INTEGER,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":exp_date", "VarChar", dr_ph_reply_log["EXP_DATE"].ToString().Trim()));         // 06.流水號(INTEGER,Y)
            }
            else
            {
                String sE = "";
                if (!int.TryParse(dr_ph_reply_log["INQTY"].ToString(), out INQTY))
                {
                    sE += " select INQTY from PH_REPLY_LOG where 1=1 ";
                    sE += " and LOGTIME='" + dr_ph_reply_log["LOGTIME"].ToString() + "' "; // 轉檔日期 
                    sE += " and PO_NO='" + dr_ph_reply_log["PO_NO"].ToString() + "' "; // 訂單號碼 
                    sE += " and AGEN_NO='" + dr_ph_reply_log["AGEN_NO"].ToString() + "' "; // 廠商代碼 
                    sE += " and MMCODE='" + dr_ph_reply_log["MMCODE"].ToString() + "' "; // 院內碼 
                    sE += " and DNO='" + dr_ph_reply_log["DNO"].ToString() + "' "; // 交貨批次 
                    sE += " and SEQ='" + dr_ph_reply_log["SEQ"].ToString() + "' "; // 流水號 
                    sE += "; -- 異常原因【INQTY】值不是數字" + sBr;
                }
                if (!int.TryParse(dr_ph_reply_log["BW_SQTY"].ToString(), out BW_SQTY))
                {
                    sE += " select BW_SQTY from PH_REPLY_LOG where 1=1 ";
                    sE += " and LOGTIME='" + dr_ph_reply_log["LOGTIME"].ToString() + "' "; // 轉檔日期 
                    sE += " and PO_NO='" + dr_ph_reply_log["PO_NO"].ToString() + "' "; // 訂單號碼 
                    sE += " and AGEN_NO='" + dr_ph_reply_log["AGEN_NO"].ToString() + "' "; // 廠商代碼 
                    sE += " and MMCODE='" + dr_ph_reply_log["MMCODE"].ToString() + "' "; // 院內碼 
                    sE += " and DNO='" + dr_ph_reply_log["DNO"].ToString() + "' "; // 交貨批次 
                    sE += " and SEQ='" + dr_ph_reply_log["SEQ"].ToString() + "' "; // 流水號 
                    sE += "; -- 異常原因【BW_SQTY】值不是數字" + sBr;
                }
                l.le("Transaction02()", sE);
            }
        } // 

        void 更新PH_REPLY_LOG_FLAG(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log,
            String flag
        )
        {
            try
            {

            String sql_oracle = "";
            sql_oracle += " update PH_REPLY_LOG set " + sBr;
            sql_oracle += " FLAG ='" + flag + "', " + sBr;
            sql_oracle += " UPDATE_TIME =sysdate, " + sBr;
            sql_oracle += " UPDATE_USER ='AUTO' " + sBr;
            sql_oracle += " where 1=1 ";
            sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商代碼(VARCHAR2,Y)
            sql_oracle += " and DNO = :dno " + sBr;// 02.交貨批次(INTEGER,Y)
            sql_oracle += " and LOGTIME = :logtime " + sBr;// 03.轉檔日期(VARCHAR2,Y)
            sql_oracle += " and MMCODE = :mmcode " + sBr;// 04.院內碼(VARCHAR2,Y)
            sql_oracle += " and PO_NO = :po_no " + sBr;// 05.訂單號碼(VARCHAR2,Y)
            sql_oracle += " and SEQ = :seq " + sBr;// 06.流水號(INTEGER,Y)
            transcmd_oracle.Add(sql_oracle);
            //l.lg("Transaction02()", "sql_oracle=" + sql_oracle);
            ++iTransOra;
            //listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dr_ph_reply_log["AGEN_NO"].ToString().Trim()));// 01.廠商代碼(VARCHAR2,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dno", "VarChar", dr_ph_reply_log["DNO"].ToString().Trim()));// 02.交貨批次(INTEGER,Y)
            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":logtime", "VarChar", (dr_ph_reply_log["LOGTIME"].ToString().Length > 0) ? Convert.ToDateTime(dr_ph_reply_log["LOGTIME"].ToString()).ToString("yyyy/MM/dd") : ""));// 03.轉檔日期(VARCHAR2,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":logtime", "VarChar", dr_ph_reply_log["LOGTIME"].ToString()));// 03.轉檔日期(VARCHAR2,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 04.院內碼(VARCHAR2,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 05.訂單號碼(VARCHAR2,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dr_ph_reply_log["SEQ"].ToString().Trim()));// 06.流水號(INTEGER,Y)
            }
            catch (Exception ex)
            {
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                String s_conn_oracle = calldbtools.SelectDB("oracle");
                OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
                if (conn_oracle.State == ConnectionState.Open)
                    conn_oracle.Close();
                conn_oracle.Open();
                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP3 更新PH_REPLY_LOG_FLAG()失敗--" + dr_ph_reply_log["PO_NO"].ToString().Trim() + ex.Message, "AUTO");
                conn_oracle.Close();
                callDBtools.WriteExceptionLog(ex.Message, "BHS003", "STEP3 更新PH_REPLY_LOG_FLAG()失敗--" + ex.Message);
                l.le("Transaction03()", "STEP3 更新PH_REPLY_LOG_FLAG()失敗--" + ex.Message);
            }
        } // 

        void 更新PH_REPLY(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log
        )
        {
            try
            {

            String sql_oracle = "";
            sql_oracle += " update PH_REPLY set " + sBr;
            sql_oracle += " BARCODE = :barcode, " + sBr;// 06.條碼(VARCHAR2,)
            sql_oracle += " BW_SQTY = :bw_sqty, " + sBr;// 07.借貨量(INTEGER,Y)
            sql_oracle += " DELI_DT = to_date(:deli_dt, 'yyyy/mm/dd hh24:mi:ss'), " + sBr;// 08.預計交貨日(DATE,)
            sql_oracle += " EXP_DATE = to_date(:exp_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr;// 09.效期(DATE,)
            //sql_oracle += " FLAG = :flag, " + sBr;// 10.轉檔標記(VARCHAR2,Y)
            sql_oracle += " INQTY = :inqty, " + sBr;// 11.交貨量(INTEGER,Y)
            sql_oracle += " INVOICE = :invoice, " + sBr;// 12.發票號碼(VARCHAR2,)
            sql_oracle += " LOT_NO = :lot_no, " + sBr;// 13.批號(VARCHAR2,)
            //sql_oracle += " MEMO = :memo, " + sBr;// 14.備註(VARCHAR2,)
            sql_oracle += " MEMO = MEMO || :memo, " + sBr;// 14.備註(VARCHAR2,)
            //sql_oracle += " STATUS = :status, " + sBr;// 15.狀態(VARCHAR2,Y)
            //sql_oracle += " CREATE_TIME = to_date(:create_time, 'yyyy/mm/dd hh24:mi:ss'), " + sBr;// 16.建立日期(DATE,)
            //sql_oracle += " CREATE_USER = :create_user, " + sBr;// 17.建立人員(VARCHAR2,)
            sql_oracle += " UPDATE_IP = :update_ip, " + sBr;// 18.異動IP(VARCHAR2,)
            sql_oracle += " UPDATE_TIME = sysdate, " + sBr;// 19.異動日期(DATE,)
            sql_oracle += " UPDATE_USER = :update_user, " + sBr;// 20.異動人員(VARCHAR2,)

            sql_oracle += " INVOICE_DT = to_date(:invoicedt, 'yyyy/mm/dd hh24:mi:ss'), " + sBr;// 21.發票號碼日期
            sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
            sql_oracle += " where 1=1 " + sBr;
            sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商代碼(VARCHAR2,Y)
            sql_oracle += " and DNO = :dno " + sBr;// 02.交貨批次(INTEGER,Y)
            sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.院內碼(VARCHAR2,Y)
            sql_oracle += " and PO_NO = :po_no " + sBr;// 04.訂單號碼(VARCHAR2,Y)
            // sql_oracle += " and SEQ = :seq " + sBr;// 05.流水號(INTEGER,Y)
            transcmd_oracle.Add(sql_oracle);
            //List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
            //l.lg("Transaction02()", "sql_oracle=" + sql_oracle);
            ++iTransOra;

            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":barcode", "VarChar", dr_ph_reply_log["BARCODE"].ToString().Trim()));// 06.條碼(VARCHAR2,)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":bw_sqty", "VarChar", dr_ph_reply_log["BW_SQTY"].ToString().Trim()));// 07.借貨量(INTEGER,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":deli_dt", "VarChar", (dr_ph_reply_log["DELI_DT"].ToString().Length > 0) ? Convert.ToDateTime(dr_ph_reply_log["DELI_DT"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 08.預計交貨日(DATE,)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":exp_date", "VarChar", (dr_ph_reply_log["EXP_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dr_ph_reply_log["EXP_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 09.效期(DATE,)
            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":flag", "VarChar", "B"));// 10.轉檔標記(VARCHAR2,Y) (B:已轉檔)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":inqty", "VarChar", dr_ph_reply_log["INQTY"].ToString().Trim()));// 11.交貨量(INTEGER,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice", "VarChar", dr_ph_reply_log["INVOICE"].ToString().Trim()));// 12.發票號碼(VARCHAR2,)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":lot_no", "VarChar", dr_ph_reply_log["LOT_NO"].ToString().Trim()));// 13.批號(VARCHAR2,)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":memo", "VarChar", dr_ph_reply_log["MEMO"].ToString().Trim()));// 14.備註(VARCHAR2,)
            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":status", "VarChar", dr_ph_reply_log["STATUS"].ToString().Trim()));// 15.狀態(VARCHAR2,Y)
            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":create_time", "VarChar", (dr_ph_reply_log["CREATE_TIME"].ToString().Length > 0) ? Convert.ToDateTime(dr_ph_reply_log["CREATE_TIME"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 16.建立日期(DATE,)
            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":create_user", "VarChar", dr_ph_reply_log["CREATE_USER"].ToString().Trim()));// 17.建立人員(VARCHAR2,)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_ip", "VarChar", dr_ph_reply_log["UPDATE_IP"].ToString().Trim()));// 18.異動IP(VARCHAR2,)
            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_time", "VarChar", (dr_ph_reply_log["UPDATE_TIME"].ToString().Length > 0) ? Convert.ToDateTime(dr_ph_reply_log["UPDATE_TIME"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 19.異動日期(DATE,)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_user", "VarChar", dr_ph_reply_log["UPDATE_USER"].ToString().Trim()));// 20.異動人員(VARCHAR2,)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoicedt", "VarChar", dr_ph_reply_log["INVOICE_DT"].ToString().Trim()));// 21.發票號碼日期

            // -- Key
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dr_ph_reply_log["AGEN_NO"].ToString().Trim()));// 01.廠商代碼(VARCHAR2,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dno", "VarChar", dr_ph_reply_log["DNO"].ToString().Trim()));// 02.交貨批次(INTEGER,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 03.院內碼(VARCHAR2,Y)
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 04.訂單號碼(VARCHAR2,Y)
                                                                                                                                                 // listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dr_ph_reply_log["SEQ"].ToString().Trim()));// 05.流水號(INTEGER,Y)
            }
            catch (Exception ex)
            {
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                String s_conn_oracle = calldbtools.SelectDB("oracle");
                OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
                if (conn_oracle.State == ConnectionState.Open)
                    conn_oracle.Close();
                conn_oracle.Open();
                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP3 更新PH_REPLY()失敗--" + dr_ph_reply_log["PO_NO"].ToString().Trim() + ex.Message, "AUTO");
                conn_oracle.Close();
                callDBtools.WriteExceptionLog(ex.Message, "BHS003", "STEP3 更新PH_REPLY()失敗--" + ex.Message);
                l.le("Transaction03()", "STEP3 更新PH_REPLY()失敗--" + ex.Message);
            }
        } // 


        void 新增PH_INVOICE(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log
        )
        {
            try
            {
                string SQL_INVOICE = "";
                if (dr_ph_reply_log["INVOICE_OLD"].ToString().Trim() == "")
                    SQL_INVOICE = dr_ph_reply_log["INVOICE"].ToString().Trim();
                else
                    SQL_INVOICE = dr_ph_reply_log["INVOICE_OLD"].ToString().Trim();
                String sql_oracle = "";
                sql_oracle += sBr + " INSERT INTO PH_INVOICE ( ";
                sql_oracle += sBr + "    PO_NO, MMCODE, TRANSNO, ";
                sql_oracle += sBr + "    INVOICE, INVOICE_DT, MEMO, CKIN_QTY, DELI_DT, ";
                sql_oracle += sBr + "    PO_QTY, PO_PRICE, M_PURUN, M_AGENLAB, PO_AMT, ";
                sql_oracle += sBr + "    M_DISCPERC, PR_NO, UNIT_SWAP, UPRICE, ";
                sql_oracle += sBr + "    M_PHCTNCO, DISC_CPRICE, DISC_UPRICE, ";
                sql_oracle += sBr + "    STATUS, CKSTATUS, FLAG, CREATE_USER, CREATE_TIME, UPDATE_TIME, UPDATE_USER ";
                sql_oracle += sBr + " )  ";
                sql_oracle += sBr + " select  ";
                sql_oracle += sBr + "    a.PO_NO, a.MMCODE, ";
                sql_oracle += sBr + "    to_char(sysdate,'yyyymmddhh24miss'), -- TRANSNO(資料流水號) ";
                sql_oracle += sBr + "    a.INVOICE, ";
                sql_oracle += sBr + "    a.INVOICE_DT, ";
                sql_oracle += sBr + "    a.MEMO, ";
                sql_oracle += sBr + "    0, "; //a.INQTY+a.BW_SQTY, -- CKIN_QTY(發票驗證數量)
                sql_oracle += sBr + "    a.DELI_DT, ";
                sql_oracle += sBr + "    b.PO_QTY, b.PO_PRICE, b.M_PURUN, b.M_AGENLAB, b.PO_AMT, ";
                sql_oracle += sBr + "    b.M_DISCPERC, b.PR_NO, b.UNIT_SWAP, b.UPRICE, ";
                sql_oracle += sBr + "    b.M_PHCTNCO, b.DISC_CPRICE, b.DISC_UPRICE, ";
                sql_oracle += sBr + "    b.STATUS, ";
                sql_oracle += sBr + "    'N', -- CKSTATUS(驗證狀態) ";
                sql_oracle += sBr + "    'B', -- FLAG(轉檔識別,預設為A) ";
                sql_oracle += sBr + "    a.CREATE_USER, sysdate, ";
                sql_oracle += sBr + "    sysdate, 'AUTO' ";
                sql_oracle += sBr + " from PH_REPLY_LOG a, MM_PO_D b  ";
                sql_oracle += sBr + " where 1=1 ";
                sql_oracle += sBr + " and a.PO_NO=b.PO_NO ";
                sql_oracle += sBr + " and a.MMCODE=b.MMCODE ";
                sql_oracle += sBr + " and a.LOGTIME= :logtime " + sBr;// 01.轉檔日期(VARCHAR2,Y)
                sql_oracle += sBr + " and a.PO_NO= :po_no " + sBr;// 02.訂單號碼(VARCHAR2,Y)
                sql_oracle += sBr + " and a.AGEN_NO= :agen_no " + sBr;// 03.廠商代碼(VARCHAR2,Y)
                sql_oracle += sBr + " and a.MMCODE= :mmcode " + sBr;// 04.院內碼(VARCHAR2,Y)
                sql_oracle += sBr + " and a.DNO= :dno " + sBr;// 05.交貨批次(INTEGER,Y)
                sql_oracle += sBr + " and a.SEQ= :seq " + sBr;// 06.流水號(INTEGER,Y)
                sql_oracle += sBr + " and a.INVOICE= :INVOICE " + sBr;// 
                transcmd_oracle.Add(sql_oracle);
                //l.lg("Transaction02()", "sql_oracle=" + sql_oracle);
                ++iTransOra;
                //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":logtime", "VarChar", (dr_ph_reply_log["LOGTIME"].ToString().Length > 0) ? Convert.ToDateTime(dr_ph_reply_log["LOGTIME"].ToString()).ToString("yyyy/MM/dd") : ""));// 01.轉檔日期(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":logtime", "VarChar", dr_ph_reply_log["LOGTIME"].ToString()));// 01.轉檔日期(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 02.訂單號碼(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dr_ph_reply_log["AGEN_NO"].ToString().Trim()));// 03.廠商代碼(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 04.院內碼(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dno", "VarChar", dr_ph_reply_log["DNO"].ToString().Trim()));// 05.交貨批次(INTEGER,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dr_ph_reply_log["SEQ"].ToString().Trim()));// 06.流水號(INTEGER,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":INVOICE", "VarChar", SQL_INVOICE));
            }
            catch (Exception ex)
            {
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                String s_conn_oracle = calldbtools.SelectDB("oracle");
                OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
                if (conn_oracle.State == ConnectionState.Open)
                    conn_oracle.Close();
                conn_oracle.Open();
                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP3 新增PH_INVOICE()失敗--"+ dr_ph_reply_log["PO_NO"].ToString().Trim() + ex.Message, "AUTO");
                conn_oracle.Close();
                callDBtools.WriteExceptionLog(ex.Message, "BHS003", "STEP3 新增PH_INVOICE()失敗--" + ex.Message);
                l.le("Transaction03()", "STEP3 新增PH_INVOICE()失敗--" + ex.Message);
            }
        } // 

        void 更新PH_INVOICE(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log
        )
        {
            try
            {
                string SQL_INVOICE = "";
                if (dr_ph_reply_log["INVOICE_OLD"].ToString().Trim() == "")
                    SQL_INVOICE = dr_ph_reply_log["INVOICE"].ToString().Trim();
                else
                    SQL_INVOICE = dr_ph_reply_log["INVOICE_OLD"].ToString().Trim();
                String sql_oracle = "";
                sql_oracle = " update PH_INVOICE set " + sBr;
                sql_oracle += " INVOICE = :ainvoice, " + sBr; // 12.發票號碼(VARCHAR2,)
                                                              //sql_oracle += " CKIN_QTY = CKIN_QTY+ :inqty + :bw_sqty, " + sBr; // CKIN_QTY(發票驗證數量)=INQTY(交貨量) + BW_SQTY(借貨量)
                sql_oracle += " INVOICE_DT = to_date(:invoice_dt,'yyyy/mm/dd hh24:mi:ss'), " + sBr;  // 21.發票號碼日期
                sql_oracle += " MEMO = memo || :memo, " + sBr;    // 備註
                sql_oracle += " UPDATE_TIME = sysdate, " + sBr;
                sql_oracle += " UPDATE_USER = :update_user, " + sBr;
                sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
                sql_oracle += " where 1=1 " + sBr;
                sql_oracle += " and PO_NO = :po_no " + sBr;// 04.訂單號碼(VARCHAR2,Y)
                sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.院內碼(VARCHAR2,Y)
                sql_oracle += " and INVOICE = :SQL_INVOICE " + sBr;// 發票號碼(VARCHAR2,)
                sql_oracle += " and CKSTATUS<>'Y' " + sBr; // CKSTATUS(驗證狀態)預設為'N'
                transcmd_oracle.Add(sql_oracle);
                //List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                //l.lg("Transaction02_or_03()", "sql_oracle=" + sql_oracle);
                ++iTransOra;

                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":ainvoice", "VarChar", dr_ph_reply_log["INVOICE"].ToString().Trim()));// 01.發票號碼
                                                                                                                                                          //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":inqty", "VarChar", dr_ph_reply_log["INQTY"].ToString().Trim()));// 03.交貨量
                                                                                                                                                          //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":bw_sqty", "VarChar", dr_ph_reply_log["BW_SQTY"].ToString().Trim()));// 04.借貨量
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice_dt", "VarChar", dr_ph_reply_log["INVOICE_DT"].ToString().Trim()));// 05.發票號碼日期
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":memo", "VarChar", dr_ph_reply_log["MEMO"].ToString().Trim()));// 06.備註
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_user", "VarChar", dr_ph_reply_log["UPDATE_USER"].ToString().Trim()));// 07.更新人
                                                                                                                                                                 // -- Key
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 08.訂單號碼
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 09.院內碼
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":SQL_INVOICE", "VarChar", SQL_INVOICE));
            }
            catch (Exception ex)
            {
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                String s_conn_oracle = calldbtools.SelectDB("oracle");
                OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
                if (conn_oracle.State == ConnectionState.Open)
                    conn_oracle.Close();
                conn_oracle.Open();
                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP3 更新PH_INVOICE()失敗--"+ dr_ph_reply_log["PO_NO"].ToString().Trim() + ex.Message, "AUTO");
                conn_oracle.Close();
                callDBtools.WriteExceptionLog(ex.Message, "BHS003", "STEP3 更新PH_INVOICE()失敗--" + ex.Message);
                l.le("Transaction03()", "STEP3 更新PH_INVOICE()失敗--" + ex.Message);
            }

        } // end of 
        void 更新PH_INVOICE_NULL(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log
        )
        {
            try
            {

                String sql_oracle = "";
                sql_oracle = " update PH_INVOICE set " + sBr;
                sql_oracle += " INVOICE = :ainvoice, " + sBr; // 12.發票號碼(VARCHAR2,)
                                                              //sql_oracle += " CKIN_QTY = CKIN_QTY+ :inqty + :bw_sqty, " + sBr; // CKIN_QTY(發票驗證數量)=INQTY(交貨量) + BW_SQTY(借貨量)
                sql_oracle += " INVOICE_DT = to_date(:invoice_dt,'yyyy/mm/dd hh24:mi:ss'), " + sBr;  // 21.發票號碼日期
                sql_oracle += " MEMO = memo || :memo, " + sBr;    // 備註
                sql_oracle += " UPDATE_TIME = sysdate, " + sBr;
                sql_oracle += " UPDATE_USER = :update_user, " + sBr;
                sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
                sql_oracle += " where 1=1 " + sBr;
                sql_oracle += " and PO_NO = :po_no " + sBr;// 04.訂單號碼(VARCHAR2,Y)
                sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.院內碼(VARCHAR2,Y)
                sql_oracle += " and CKSTATUS<>'Y' " + sBr; // CKSTATUS(驗證狀態)預設為'N'
                sql_oracle += " and INVOICE is null " + sBr;
                transcmd_oracle.Add(sql_oracle);
                //List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                //l.lg("Transaction02_or_03()", "sql_oracle=" + sql_oracle);
                ++iTransOra;

                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":ainvoice", "VarChar", dr_ph_reply_log["INVOICE"].ToString().Trim()));// 01.發票號碼
                                                                                                                                                          //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":inqty", "VarChar", dr_ph_reply_log["INQTY"].ToString().Trim()));// 03.交貨量
                                                                                                                                                          //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":bw_sqty", "VarChar", dr_ph_reply_log["BW_SQTY"].ToString().Trim()));// 04.借貨量
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice_dt", "VarChar", dr_ph_reply_log["INVOICE_DT"].ToString().Trim()));// 05.發票號碼日期
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":memo", "VarChar", dr_ph_reply_log["MEMO"].ToString().Trim()));// 06.備註
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_user", "VarChar", dr_ph_reply_log["UPDATE_USER"].ToString().Trim()));// 07.更新人
                                                                                                                                                                 // -- Key
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 08.訂單號碼
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 09.院內碼
            }
            catch (Exception ex)
            {
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                String s_conn_oracle = calldbtools.SelectDB("oracle");
                OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
                if (conn_oracle.State == ConnectionState.Open)
                    conn_oracle.Close();
                conn_oracle.Open();
                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP3 更新PH_INVOICE_NULL()失敗--" + dr_ph_reply_log["PO_NO"].ToString().Trim() + ex.Message, "AUTO");
                conn_oracle.Close();
                callDBtools.WriteExceptionLog(ex.Message, "BHS003", "STEP3 更新PH_INVOICE_NULL()失敗--" + ex.Message);
                l.le("Transaction03()", "STEP3 更新PH_INVOICE_NULL()失敗--" + ex.Message);
            }
        } // end of 
        void 更新PH_INVOICE__FLAG_B(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_invoice
        )
        {
            try
            {
            String sql_oracle = "";
            sql_oracle = " update PH_INVOICE set " + sBr;
            sql_oracle += " FLAG='B', " + sBr;
            sql_oracle += " UPDATE_TIME = sysdate, " + sBr;
            sql_oracle += " UPDATE_USER = :update_user, " + sBr;
            sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
            sql_oracle += " where 1=1 " + sBr;
            sql_oracle += " and PO_NO = :po_no " + sBr;// 04.訂單號碼(VARCHAR2,Y)
            sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.院內碼(VARCHAR2,Y)
            sql_oracle += " and CKSTATUS='T' " + sBr;
            sql_oracle += " and FLAG='A' " + sBr;
            transcmd_oracle.Add(sql_oracle);
            //l.lg("Transaction04()", "sql_oracle=" + sql_oracle);
            ++iTransOra;

            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_user", "VarChar", "AUTO"));// 07.更新人
            // -- Key
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dr_ph_invoice["PO_NO"].ToString().Trim()));// 08.訂單號碼
            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dr_ph_invoice["MMCODE"].ToString().Trim()));// 09.院內碼
            }
            catch (Exception ex)
            {
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                String s_conn_oracle = calldbtools.SelectDB("oracle");
                OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
                if (conn_oracle.State == ConnectionState.Open)
                    conn_oracle.Close();
                conn_oracle.Open();
                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP3 更新PH_INVOICE__FLAG_B()失敗--" + dr_ph_invoice["PO_NO"].ToString().Trim() + ex.Message, "AUTO");
                conn_oracle.Close();
                callDBtools.WriteExceptionLog(ex.Message, "BHS003", "STEP3 更新PH_INVOICE__FLAG_B()失敗--" + ex.Message);
                l.le("Transaction03()", "STEP3 更新PH_INVOICE__FLAG_B()失敗--" + ex.Message);
            }
        } // end of 


        void 新增PH_REPLY(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log
        )
        {
            try
            {
                String sql_oracle = "";
                // 01.新增PH_REPLY
                sql_oracle = " insert into PH_REPLY ( " + sBr;
                sql_oracle += " AGEN_NO, " + sBr; // 01.廠商代碼(VARCHAR2,6)
                sql_oracle += " DNO, " + sBr; // 02.交貨批次(INTEGER,)
                sql_oracle += " MMCODE, " + sBr; // 03.院內碼(VARCHAR2,13)
                sql_oracle += " PO_NO, " + sBr; // 04.訂單號碼(VARCHAR2,21)
                sql_oracle += " SEQ, " + sBr; // 05.流水號(INTEGER,)
                sql_oracle += " BARCODE, " + sBr; // 06.條碼(VARCHAR2,50)
                sql_oracle += " BW_SQTY, " + sBr; // 07.借貨量(INTEGER,)
                sql_oracle += " DELI_DT, " + sBr; // 08.預計交貨日(DATE,)
                sql_oracle += " EXP_DATE, " + sBr; // 09.效期(DATE,)
                sql_oracle += " FLAG, " + sBr; // 10.轉檔標記(VARCHAR2,1)
                sql_oracle += " INQTY, " + sBr; // 11.交貨量(INTEGER,)
                sql_oracle += " INVOICE, " + sBr; // 12.發票號碼(VARCHAR2,10)
                sql_oracle += " LOT_NO, " + sBr; // 13.批號(VARCHAR2,20)
                sql_oracle += " MEMO, " + sBr; // 14.備註(VARCHAR2,150)
                sql_oracle += " STATUS, " + sBr; // 15.狀態(VARCHAR2,1)
                sql_oracle += " CREATE_TIME, " + sBr; // 16.建立日期(DATE,)
                sql_oracle += " CREATE_USER, " + sBr; // 17.建立人員(VARCHAR2,10)
                sql_oracle += " UPDATE_IP, " + sBr; // 18.異動IP(VARCHAR2,20)
                sql_oracle += " UPDATE_TIME, " + sBr; // 19.異動日期(DATE,)
                sql_oracle += " UPDATE_USER, " + sBr; // 20.異動人員(VARCHAR2,10)
                sql_oracle += " INVOICE_DT, " + sBr; // 21.發票號碼日期(DATE)
                sql_oracle += " INVOICE_OLD, " + sBr;
                sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
                sql_oracle += " ) " + sBr;
                sql_oracle += " select " + sBr;
                sql_oracle += " AGEN_NO, " + sBr; // 01.廠商代碼(VARCHAR2,6)
                sql_oracle += " DNO, " + sBr; // 02.交貨批次(INTEGER,)
                sql_oracle += " MMCODE, " + sBr; // 04.院內碼(VARCHAR2,13)
                sql_oracle += " PO_NO, " + sBr; // 05.訂單號碼(VARCHAR2,21)
                sql_oracle += " SEQ, " + sBr; // 06.流水號(INTEGER,)
                sql_oracle += " BARCODE, " + sBr; // 07.條碼(VARCHAR2,50)
                sql_oracle += " BW_SQTY, " + sBr; // 08.借貨量(INTEGER,)
                sql_oracle += " DELI_DT, " + sBr; // 09.預計交貨日(DATE,)
                sql_oracle += " EXP_DATE, " + sBr; // 10.效期(DATE,)
                sql_oracle += " 'B', " + sBr; // 11.FLAG 轉檔標記(VARCHAR2,1)
                sql_oracle += " INQTY, " + sBr; // 12.交貨量(INTEGER,)
                sql_oracle += " INVOICE, " + sBr; // 13.發票號碼(VARCHAR2,10)
                sql_oracle += " LOT_NO, " + sBr; // 14.批號(VARCHAR2,20)
                sql_oracle += " MEMO, " + sBr; // 15.備註(VARCHAR2,150)
                sql_oracle += " STATUS, " + sBr; // 16.狀態(VARCHAR2,1)
                sql_oracle += " CREATE_TIME, " + sBr; // 17.建立日期(DATE,)
                sql_oracle += " CREATE_USER, " + sBr; // 18.建立人員(VARCHAR2,10)
                sql_oracle += " UPDATE_IP, " + sBr; // 19.異動IP(VARCHAR2,20)
                sql_oracle += " UPDATE_TIME, " + sBr; // 20.異動日期(DATE,)
                sql_oracle += " UPDATE_USER, " + sBr; // 21.異動人員(VARCHAR2,10)
                sql_oracle += " INVOICE_DT, " + sBr; // 22.發票號碼日期(DATE)
                sql_oracle += " INVOICE_OLD, " + sBr;
                sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
                sql_oracle += " from PH_REPLY_LOG " + sBr;
                sql_oracle += " where 1=1 " + sBr;
                sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商代碼(VARCHAR2,Y)
                sql_oracle += " and DNO = :dno " + sBr;// 02.交貨批次(INTEGER,Y)
                sql_oracle += " and LOGTIME = :logtime " + sBr;// 03.轉檔日期(VARCHAR2,Y)
                sql_oracle += " and MMCODE = :mmcode " + sBr;// 04.院內碼(VARCHAR2,Y)
                sql_oracle += " and PO_NO = :po_no " + sBr;// 05.訂單號碼(VARCHAR2,Y)
                sql_oracle += " and SEQ = :seq " + sBr;// 06.流水號(INTEGER,Y)
                transcmd_oracle.Add(sql_oracle);



                //List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                //int iTransSeq = 1;
                //l.lg("Transaction02()", "sql_oracle=" + sql_oracle);
                ++iTransOra;
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dr_ph_reply_log["AGEN_NO"].ToString().Trim()));// 01.廠商代碼(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dno", "VarChar", dr_ph_reply_log["DNO"].ToString().Trim()));// 02.交貨批次(INTEGER,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":logtime", "VarChar", dr_ph_reply_log["LOGTIME"].ToString()));// 03.轉檔日期(VARCHAR2,Y)  (dr_ph_reply_log["LOGTIME"].ToString().Length > 0) ? Convert.ToDateTime(dr_ph_reply_log["LOGTIME"].ToString()).ToString("yyyy/MM/dd") : "")
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 04.院內碼(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 05.訂單號碼(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dr_ph_reply_log["SEQ"].ToString().Trim()));// 06.流水號(INTEGER,Y)
            }
            catch (Exception ex)
            {
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                String s_conn_oracle = calldbtools.SelectDB("oracle");
                OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
                if (conn_oracle.State == ConnectionState.Open)
                    conn_oracle.Close();
                conn_oracle.Open();
                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP3 新增PH_REPLY()失敗--" + dr_ph_reply_log["PO_NO"].ToString().Trim() + ex.Message, "AUTO");
                conn_oracle.Close();
                callDBtools.WriteExceptionLog(ex.Message, "BHS003", "STEP3 新增PH_REPLY()失敗--" + ex.Message);
                l.le("Transaction03()", "STEP3 新增PH_REPLY()失敗--" + ex.Message);
            }
        } // 

        void 處理PH_INVOICE(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log,
            CallDBtools_Oracle callDbtools_oralce,
            ref String msg_oracle
        )
        {
            String sql_oracle = "";
            sql_oracle += " select " + sBr;
            sql_oracle += " PO_NO, " + sBr; // 00.訂單號碼(VARCHAR2,Y)
            sql_oracle += " MMCODE, " + sBr; // 01.院內碼(VARCHAR2,Y)
            sql_oracle += " TRANSNO, " + sBr; // 02.資料流水號(VARCHAR2,Y)
            sql_oracle += " to_char(CREATE_TIME,'yyyy/mm/dd hh24:mi:ss') CREATE_TIME, " + sBr; // 03.建立日期(DATE,Y)
            sql_oracle += " PO_QTY, " + sBr; // 04.訂單數量(NUMBER,)
            sql_oracle += " PO_PRICE, " + sBr; // 05.訂單單價(NUMBER,)
            sql_oracle += " M_PURUN, " + sBr; // 06.申購計量單位(VARCHAR2,)
            sql_oracle += " M_AGENLAB, " + sBr; // 07.廠牌(VARCHAR2,)
            sql_oracle += " PO_AMT, " + sBr; // 08.總金額(NUMBER,)
            sql_oracle += " M_DISCPERC, " + sBr; // 09.折讓比(NUMBER,)
            sql_oracle += " DELI_QTY, " + sBr; // 10.已交數量(NUMBER,)
            sql_oracle += " BW_SQTY, " + sBr; // 11.借貨數量(NUMBER,)
            sql_oracle += " DELI_STATUS, " + sBr; // 12.交貨狀態(VARCHAR2,)
            sql_oracle += " MEMO, " + sBr; // 13.備註(VARCHAR2,)
            sql_oracle += " PR_NO, " + sBr; // 14.申購單號(VARCHAR2,)
            sql_oracle += " UNIT_SWAP, " + sBr; // 15.轉換率(NUMBER,)
            sql_oracle += " INVOICE, " + sBr; // 16.發票號碼(VARCHAR2,)
            sql_oracle += " to_char(INVOICE_DT,'yyyy/mm/dd hh24:mi:ss') INVOICE_DT, " + sBr; // 17.發票號碼日期(DATE,)
            sql_oracle += " CKIN_QTY, " + sBr; // 18.發票驗證數量(NUMBER,)
            sql_oracle += " CHK_USER, " + sBr; // 19.發票驗證人員(VARCHAR2,)
            sql_oracle += " to_char(CHK_DT,'yyyy/mm/dd hh24:mi:ss') CHK_DT, " + sBr; // 20.發票驗證日期(DATE,)
            sql_oracle += " to_char(ACCOUNTDATE,'yyyy/mm/dd hh24:mi:ss') ACCOUNTDATE, " + sBr; // 21.入帳日期(DATE,)
            sql_oracle += " STATUS, " + sBr; // 22.狀態(VARCHAR2,)
            sql_oracle += " UPRICE, " + sBr; // 23.最小單價(NUMBER,)
            sql_oracle += " M_PHCTNCO, " + sBr; // 24.衛署字號(VARCHAR2,)
            sql_oracle += " DISC_CPRICE, " + sBr; // 25.優惠合約單價(NUMBER,)
            sql_oracle += " DISC_UPRICE, " + sBr; // 26.優惠最小單價(NUMBER,)
            sql_oracle += " CREATE_USER, " + sBr; // 27.建立人員(VARCHAR2,)
            sql_oracle += " to_char(UPDATE_TIME,'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME, " + sBr; // 28.異動日期(DATE,)
            sql_oracle += " UPDATE_USER, " + sBr; // 29.異動人員(VARCHAR2,)
            sql_oracle += " UPDATE_IP, " + sBr; // 30.異動IP(VARCHAR2,)
            sql_oracle += " CKSTATUS, " + sBr; // 31.驗證狀態(VARCHAR2,)
            sql_oracle += " FLAG, " + sBr; // 32.轉檔識別(VARCHAR2,)
            sql_oracle += " to_char(DELI_DT,'yyyy/mm/dd hh24:mi:ss') DELI_DT, " + sBr; // 33.進貨日(DATE,)
            sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
            sql_oracle += " from PH_INVOICE where 1=1 " + sBr;
            sql_oracle += " and PO_NO = :po_no " + sBr;// 04.訂單號碼(VARCHAR2,Y)
            sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.院內碼(VARCHAR2,Y)
            sql_oracle += " and INVOICE is null " + sBr;
            List<CallDBtools_Oracle.OracleParam> listParam_oracle_ph_invoice = new List<CallDBtools_Oracle.OracleParam>();
            int iTransOra_ph_invoice = 1;
            listParam_oracle_ph_invoice.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_invoice, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 04.訂單號碼(VARCHAR2,Y)
            listParam_oracle_ph_invoice.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_invoice, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 03.院內碼(VARCHAR2,Y)
            DataTable dt_oralce_ph_invoice = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "", listParam_oracle_ph_invoice, "oracle", "T1", ref msg_oracle);
            // 檢查 INVOICE is null + PO_NO + MMCODE
            if (msg_oracle == "") //資料抓取沒有錯誤 且有資料
            {
                //l.lg("Transaction02()", "dt_oralce_ph_invoice.Rows.Count=" + dt_oralce_ph_invoice.Rows.Count.ToString());
                if (dt_oralce_ph_invoice.Rows.Count == 0)
                {
                    string SQL_INVOICE = "";
                    if (dr_ph_reply_log["INVOICE_OLD"].ToString().Trim() == "")
                        SQL_INVOICE = dr_ph_reply_log["INVOICE"].ToString().Trim();
                    else
                        SQL_INVOICE = dr_ph_reply_log["INVOICE_OLD"].ToString().Trim();
                    //檢查 加上INVOICE, PO_NO+MMCODE
                    sql_oracle = " select * from PH_INVOICE  ";
                    sql_oracle += " where  PO_NO = :po_no ";// 04.訂單號碼(VARCHAR2,Y)
                    sql_oracle += " and MMCODE = :mmcode ";// 03.院內碼(VARCHAR2,Y)
                    sql_oracle += " and INVOICE = :invoice ";
                    listParam_oracle_ph_invoice.Clear();
                    listParam_oracle_ph_invoice.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_invoice, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 04.訂單號碼(VARCHAR2,Y)
                    listParam_oracle_ph_invoice.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_invoice, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 03.院內碼(VARCHAR2,Y)
                    listParam_oracle_ph_invoice.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_invoice, ":invoice", "VarChar", SQL_INVOICE));
                    dt_oralce_ph_invoice = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "", listParam_oracle_ph_invoice, "oracle", "T1", ref msg_oracle);
                    if (msg_oracle == "")
                    {//資料抓取沒有錯誤 且有資料
                        if (dt_oralce_ph_invoice.Rows.Count == 0)
                        {
                            //新增PH_INVOICE(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr_ph_reply_log);
                        }
                        else
                        {
                            更新PH_INVOICE(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr_ph_reply_log);
                        }
                    }
                }
                else if (dt_oralce_ph_invoice.Rows.Count == 1)
                {
                    更新PH_INVOICE_NULL(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr_ph_reply_log);
                } // end of if (i_count_ph_invoice == 0) 
            } // end of if (msg_oracle == "") //資料抓取沒有錯誤 且有資料
        } // 

        void 處理PH_LOTNO_01(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log,
            CallDBtools_Oracle callDbtools_oralce,
            ref String msg_oracle
        )
        {
            // 【PH_LOTNO】開始
            String sql_oracle = "";
            sql_oracle += " select ";
            sql_oracle += " MMCODE, " + sBr; // 00.院內碼(VARCHAR2,Y)
            sql_oracle += " LOT_NO, " + sBr; // 01.批號(VARCHAR2,Y)
            sql_oracle += " to_char(EXP_DATE,'yyyy/mm/dd hh24:mi:ss') EXP_DATE, " + sBr; // 02.效期(DATE,Y)
            sql_oracle += " PO_NO, " + sBr; // 03.訂單號碼(VARCHAR2,Y)
            sql_oracle += " QTY, " + sBr; // 04.數量(NUMBER,Y)
            sql_oracle += " MEMO, " + sBr; // 05.備註(VARCHAR2,)
            sql_oracle += " SOURCE, " + sBr; // 06.來源(VARCHAR2,Y)
            sql_oracle += " STATUS, " + sBr; // 07.狀態(VARCHAR2,)
            sql_oracle += " to_char(CREATE_TIME,'yyyy/mm/dd hh24:mi:ss') CREATE_TIME, " + sBr; // 08.建立日期(DATE,)
            sql_oracle += " CREATE_USER, " + sBr; // 09.建立人員(VARCHAR2,)
            sql_oracle += " to_char(UPDATE_TIME,'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME, " + sBr; // 10.異動日期(DATE,)
            sql_oracle += " UPDATE_USER, " + sBr; // 11.異動人員(VARCHAR2,)
            sql_oracle += " UPDATE_IP, " + sBr; // 12.異動IP(VARCHAR2,)
            sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4) + sBr;
            sql_oracle += " from PH_LOTNO where 1=1 " + sBr;    // 廠商批號效期資料檔
            sql_oracle += " and PO_NO = :po_no " + sBr;         // 01.訂單號碼(VARCHAR2,Y)
            sql_oracle += " and MMCODE = :mmcode " + sBr;       // 02.院內碼(VARCHAR2,Y)
            sql_oracle += " and LOT_NO = :lot_no " + sBr;       // 03.批號(VARCHAR2,Y)
            sql_oracle += " and to_char(EXP_DATE,'yyyy/mm/dd hh24:mi:ss') = :exp_date " + sBr;   // 04.效期(DATE,Y)

            List<CallDBtools_Oracle.OracleParam> listParam_oracle_ph_lotno = new List<CallDBtools_Oracle.OracleParam>();
            int iTransOra_ph_lotno = 1;
            listParam_oracle_ph_lotno.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_lotno, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 01.訂單號碼(VARCHAR2,Y)
            listParam_oracle_ph_lotno.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_lotno, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 02.院內碼(VARCHAR2,Y)
            listParam_oracle_ph_lotno.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_lotno, ":lot_no", "VarChar", dr_ph_reply_log["LOT_NO"].ToString().Trim()));// 03.批號(VARCHAR2,Y)
            listParam_oracle_ph_lotno.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_lotno, ":exp_date", "VarChar", dr_ph_reply_log["EXP_DATE"].ToString().Trim()));// 04.效期(DATE,Y)
            DataTable dt_oralce_ph_lotno = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "", listParam_oracle_ph_lotno, "oracle", "T1", ref msg_oracle);
            if (msg_oracle == "") //資料抓取沒有錯誤 且有資料
            {
                //l.lg("Transaction02()", "dt_oralce_ph_lotno.Rows.Count=" + dt_oralce_ph_lotno.Rows.Count);
                // 處理PH_LOTNO
                處理PH_LOTNO(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr_ph_reply_log, dt_oralce_ph_lotno);
            } // end of if(msg_oracle == "") 
        } // 

        void 處理PH_REPLY(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log,
            CallDBtools_Oracle callDbtools_oralce
        )
        {
            String msg_oracle = "";
            String sql_oracle = "";
            sql_oracle = " select ";
            sql_oracle += " AGEN_NO, " + sBr; // 01.廠商代碼(VARCHAR2,Y)
            sql_oracle += " DNO, " + sBr; // 02.交貨批次(INTEGER,Y)
            sql_oracle += " MMCODE, " + sBr; // 03.院內碼(VARCHAR2,Y)
            sql_oracle += " PO_NO, " + sBr; // 04.訂單號碼(VARCHAR2,Y)
            sql_oracle += " SEQ, " + sBr; // 05.流水號(INTEGER,Y)
            sql_oracle += " BARCODE, " + sBr; // 06.條碼(VARCHAR2,)
            sql_oracle += " BW_SQTY, " + sBr; // 07.借貨量(INTEGER,Y)
            sql_oracle += " to_char(DELI_DT,'yyyy/mm/dd hh24:mi:ss') DELI_DT, " + sBr; // 08.預計交貨日(DATE,)
            sql_oracle += " to_char(EXP_DATE,'yyyy/mm/dd hh24:mi:ss') EXP_DATE, " + sBr; // 09.效期(DATE,)
            sql_oracle += " FLAG, " + sBr; // 10.轉檔標記(VARCHAR2,Y)
            sql_oracle += " INQTY, " + sBr; // 11.交貨量(INTEGER,Y)
            sql_oracle += " INVOICE, " + sBr; // 12.發票號碼(VARCHAR2,)
            sql_oracle += " LOT_NO, " + sBr; // 13.批號(VARCHAR2,)
            sql_oracle += " MEMO, " + sBr; // 14.備註(VARCHAR2,)
            sql_oracle += " STATUS, " + sBr; // 15.狀態(VARCHAR2,Y)
            sql_oracle += " to_char(CREATE_TIME,'yyyy/mm/dd hh24:mi:ss') CREATE_TIME, " + sBr; // 16.建立日期(DATE,)
            sql_oracle += " CREATE_USER, " + sBr; // 17.建立人員(VARCHAR2,)
            sql_oracle += " UPDATE_IP, " + sBr; // 18.異動IP(VARCHAR2,)
            sql_oracle += " to_char(UPDATE_TIME,'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME, " + sBr; // 19.異動日期(DATE,)
            sql_oracle += " UPDATE_USER, " + sBr; // 20.異動人員(VARCHAR2,)
            sql_oracle += " to_char(INVOICE_DT,'yyyy/mm/dd hh24:mi:ss') INVOICE_DT, " + sBr; // 22.發票號碼日期(DATE,)
            sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
            sql_oracle += " from PH_REPLY where 1=1 " + sBr;
            sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商代碼(VARCHAR2,Y)
            sql_oracle += " and DNO = :dno " + sBr;// 02.交貨批次(INTEGER,Y)
            sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.院內碼(VARCHAR2,Y)
            sql_oracle += " and PO_NO = :po_no " + sBr;// 04.訂單號碼(VARCHAR2,Y)
            sql_oracle += " and SEQ = :seq " + sBr;// 05.流水號(INTEGER,Y)
            List<CallDBtools_Oracle.OracleParam> listParam_oracle_ph_reply = new List<CallDBtools_Oracle.OracleParam>();
            int iTransOra_ph_reply = 1;
            listParam_oracle_ph_reply.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_reply, ":agen_no", "VarChar", dr_ph_reply_log["AGEN_NO"].ToString().Trim()));// 01.廠商代碼(VARCHAR2,Y)
            listParam_oracle_ph_reply.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_reply, ":dno", "VarChar", dr_ph_reply_log["DNO"].ToString().Trim()));// 02.交貨批次(INTEGER,Y)
            listParam_oracle_ph_reply.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_reply, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 03.院內碼(VARCHAR2,Y)
            listParam_oracle_ph_reply.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_reply, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 04.訂單號碼(VARCHAR2,Y)
            listParam_oracle_ph_reply.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_reply, ":seq", "VarChar", dr_ph_reply_log["SEQ"].ToString().Trim()));// 05.流水號(INTEGER,Y)
            DataTable dt_oralce_ph_reply = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "", listParam_oracle_ph_reply, "oracle", "T1", ref msg_oracle);
            if (msg_oracle == "") //資料抓取沒有錯誤 且有資料
            {
                //l.lg("Transaction02()", "dt_oralce_ph_reply.Rows.Count=" + dt_oralce_ph_reply.Rows.Count);
                if (dt_oralce_ph_reply.Rows.Count == 0) // 新增
                {
                    新增PH_REPLY(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr_ph_reply_log);
                }
                //else if (dt_oralce_ph_reply.Rows.Count > 0)
                //{
                //    for (int j = 0; j < dt_oralce_ph_reply.Rows.Count; j++)
                //    {
                //        String status_ph_reply = dt_oralce_ph_reply.Rows[j]["STATUS"].ToString().Trim();
                //        l.lg("Transaction02()", "status_ph_reply=" + status_ph_reply);
                //        if (status_ph_reply.ToUpper().Equals("B")) // 處理中
                //        {
                //          更新PH_REPLY();
                //                                                                                                                                               // rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, listParam_oracle, "oracle", ref msg_oracle);
                //                                                                                                                                               //if (msg_oracle == "")
                //                                                                                                                                               //{
                //                                                                                                                                               //rowsAffected_oracle = -1;

                //            更新PH_REPLY_LOG_FLAG(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr, dt_oralce.Rows[j]);
                //            // rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, listParam_oracle, "oracle", ref msg_oracle);
                //            //    if (msg_oracle != "")
                //            //    {
                //            //        callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP2-update PH_REPLY_LOG.FLAG='B' 失敗:" + msg_oracle, "AUTO");
                //            //    }
                //            //}
                //            //else
                //            //{
                //            //    callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP2-update PH_REPLY.FLAG='B' 失敗:" + msg_oracle, "AUTO");
                //            //}
                //        }
                //        else // 處理過，不再處理
                //        {
                //            更新PH_REPLY_LOG__FLAG(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dt_ph_reply_log.Rows[i], "C");
                //        }
                //    } // end of for (int j=0, .. 
                //} // end of if (dt_oralce_ph_reply.Rows.Count == 0) // 新增
            } // end of if 
        } // end of void 處理PH_REPLY(

        void 處理PH_REPLY__PH_REPLY_LOG_FALG是A__且已有資料(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log
        )
        {
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            String msg_oracle = "";
            String sql_oracle = "";
            sql_oracle = " select ";
            sql_oracle += " AGEN_NO, " + sBr; // 01.廠商代碼(VARCHAR2,Y)
            sql_oracle += " DNO, " + sBr; // 02.交貨批次(INTEGER,Y)
            sql_oracle += " MMCODE, " + sBr; // 03.院內碼(VARCHAR2,Y)
            sql_oracle += " PO_NO, " + sBr; // 04.訂單號碼(VARCHAR2,Y)
            sql_oracle += " SEQ, " + sBr; // 05.流水號(INTEGER,Y)
            sql_oracle += " BARCODE, " + sBr; // 06.條碼(VARCHAR2,)
            sql_oracle += " BW_SQTY, " + sBr; // 07.借貨量(INTEGER,Y)
            sql_oracle += " to_char(DELI_DT,'yyyy/mm/dd hh24:mi:ss') DELI_DT, " + sBr; // 08.預計交貨日(DATE,)
            sql_oracle += " to_char(EXP_DATE,'yyyy/mm/dd hh24:mi:ss') EXP_DATE, " + sBr; // 09.效期(DATE,)
            sql_oracle += " FLAG, " + sBr; // 10.轉檔標記(VARCHAR2,Y)
            sql_oracle += " INQTY, " + sBr; // 11.交貨量(INTEGER,Y)
            sql_oracle += " INVOICE, " + sBr; // 12.發票號碼(VARCHAR2,)
            sql_oracle += " LOT_NO, " + sBr; // 13.批號(VARCHAR2,)
            sql_oracle += " MEMO, " + sBr; // 14.備註(VARCHAR2,)
            sql_oracle += " STATUS, " + sBr; // 15.狀態(VARCHAR2,Y)
            sql_oracle += " to_char(CREATE_TIME,'yyyy/mm/dd hh24:mi:ss') CREATE_TIME, " + sBr; // 16.建立日期(DATE,)
            sql_oracle += " CREATE_USER, " + sBr; // 17.建立人員(VARCHAR2,)
            sql_oracle += " UPDATE_IP, " + sBr; // 18.異動IP(VARCHAR2,)
            sql_oracle += " to_char(UPDATE_TIME,'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME, " + sBr; // 19.異動日期(DATE,)
            sql_oracle += " UPDATE_USER, " + sBr; // 20.異動人員(VARCHAR2,)
            sql_oracle += " to_char(INVOICE_DT,'yyyy/mm/dd hh24:mi:ss') INVOICE_DT, " + sBr; // 22.發票號碼日期(DATE,)
            sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
            sql_oracle += " from PH_REPLY where 1=1 " + sBr;
            sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商代碼(VARCHAR2,Y)
            sql_oracle += " and DNO = :dno " + sBr;// 02.交貨批次(INTEGER,Y)
            sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.院內碼(VARCHAR2,Y)
            sql_oracle += " and PO_NO = :po_no " + sBr;// 04.訂單號碼(VARCHAR2,Y)
            //sql_oracle += " and SEQ = :seq " + sBr;// 05.流水號(INTEGER,Y)
            List<CallDBtools_Oracle.OracleParam> listParam_oracle_ph_reply = new List<CallDBtools_Oracle.OracleParam>();
            int iTransOra_ph_reply = 1;
            listParam_oracle_ph_reply.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_reply, ":agen_no", "VarChar", dr_ph_reply_log["AGEN_NO"].ToString().Trim()));// 01.廠商代碼(VARCHAR2,Y)
            listParam_oracle_ph_reply.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_reply, ":dno", "VarChar", dr_ph_reply_log["DNO"].ToString().Trim()));// 02.交貨批次(INTEGER,Y)
            listParam_oracle_ph_reply.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_reply, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 03.院內碼(VARCHAR2,Y)
            listParam_oracle_ph_reply.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_reply, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 04.訂單號碼(VARCHAR2,Y)
            //listParam_oracle_ph_reply.Add(new CallDBtools_Oracle.OracleParam(iTransOra_ph_reply, ":seq", "VarChar", dr_ph_reply_log["SEQ"].ToString().Trim()));// 05.流水號(INTEGER,Y)
            DataTable dt_oralce_ph_reply = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "", listParam_oracle_ph_reply, "oracle", "T1", ref msg_oracle);
            if (msg_oracle == "") //資料抓取沒有錯誤 且有資料
            {
                //l.lg("Transaction02()", "dt_oralce_ph_reply.Rows.Count=" + dt_oralce_ph_reply.Rows.Count);
                if (dt_oralce_ph_reply.Rows.Count > 0)
                {
                    for (int j = 0; j < dt_oralce_ph_reply.Rows.Count; j++)
                    {
                        String status_ph_reply = dt_oralce_ph_reply.Rows[j]["STATUS"].ToString().Trim();
                        //l.lg("Transaction02()", "status_ph_reply=" + status_ph_reply);
                        if (status_ph_reply.ToUpper().Equals("B")) // 處理中
                        {
                            更新PH_REPLY(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr_ph_reply_log);

                            更新PH_REPLY_LOG_FLAG(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr_ph_reply_log, "B");
                        }
                        else // 處理過，不再處理
                        {
                            更新PH_REPLY_LOG_FLAG(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr_ph_reply_log, "C");
                        }
                    } // end of for (int j=0, .. 
                } // end of if (dt_oralce_ph_reply.Rows.Count> 0) 
            } // end of if 
        } // end of void 處理PH_REPLY__PH_REPLY_LOG_FALG是A__且已有資料(


        void 更新PH_REPLY_LOG__FLAG(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log,
            String flag
        )
        {
            try
            {
                String sql_oracle = "";
                sql_oracle += " update PH_REPLY_LOG set FLAG='" + flag + "', UPDATE_TIME=sysdate, UPDATE_USER='AUTO' where 1=1 ";
                sql_oracle += " and LOGTIME = :logtime " + sBr;// 01.轉檔日期(VARCHAR2,Y)
                sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商代碼(VARCHAR2,Y)
                sql_oracle += " and DNO = :dno " + sBr;// 02.交貨批次(INTEGER,Y)
                sql_oracle += " and MMCODE = :mmcode " + sBr;// 04.院內碼(VARCHAR2,Y)
                sql_oracle += " and PO_NO = :po_no " + sBr;// 05.訂單號碼(VARCHAR2,Y)
                sql_oracle += " and SEQ = :seq " + sBr;// 06.流水號(INTEGER,Y)
                transcmd_oracle.Add(sql_oracle);
                //l.lg("Transaction02()", "sql_oracle=" + sql_oracle);
                ++iTransOra;
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":logtime", "VarChar", dr_ph_reply_log["LOGTIME"].ToString().Trim()));// 01.
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dr_ph_reply_log["AGEN_NO"].ToString().Trim()));// 01.廠商代碼(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dno", "VarChar", dr_ph_reply_log["DNO"].ToString().Trim()));// 02.交貨批次(INTEGER,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dr_ph_reply_log["MMCODE"].ToString().Trim()));// 04.院內碼(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dr_ph_reply_log["PO_NO"].ToString().Trim()));// 05.訂單號碼(VARCHAR2,Y)
                listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dr_ph_reply_log["SEQ"].ToString().Trim()));// 06.流水號(INTEGER,Y)
            }
            catch (Exception ex)
            {
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                String s_conn_oracle = calldbtools.SelectDB("oracle");
                OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
                if (conn_oracle.State == ConnectionState.Open)
                    conn_oracle.Close();
                conn_oracle.Open();
                CallDBtools callDBtools = new CallDBtools();
                callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP3 更新PH_REPLY_LOG__FLAG()失敗--" + dr_ph_reply_log["PO_NO"].ToString().Trim() + ex.Message, "AUTO");
                conn_oracle.Close();
                callDBtools.WriteExceptionLog(ex.Message, "BHS003", "STEP3 更新PH_REPLY_LOG__FLAG()失敗--" + ex.Message);
                l.le("Transaction03()", "STEP3 更新PH_REPLY_LOG__FLAG()失敗--" + ex.Message);
            }
        } // end of void 處理PH_REPLY_LOG(

        void 處理PH_LOTNO(
            ref List<string> transcmd_oracle,
            ref int iTransOra,
            ref List<CallDBtools_Oracle.OracleParam> listParam_oracle,
            DataRow dr_ph_reply_log,
            DataTable dt_oralce_ph_lotno
        )
        {
            //String sql_oracle = "";
            if (dt_oralce_ph_lotno.Rows.Count == 0) // 不存在
            {
                新增PH_LOTNO(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr_ph_reply_log);
            }
            else
            {
                String SOURCE = dt_oralce_ph_lotno.Rows[0]["SOURCE"].ToString().Trim();
                if (SOURCE.ToLower().Equals("v"))
                {
                    更新PH_LOTNO(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr_ph_reply_log);
                }
            } // end of if (dt_oralce_ph_lotno.Rows.Count == 0)
        } // end of void 處理PH_LOTNO(

        #endregion


        #region " Transaction區 "


        // -------------------
        // -- Transaction區 -- 
        // -------------------


        // 1_將外網 MSdb WB_PUT_D 異動資料複製到院內 Oracle PH_PUT_D
        private void Transaction01()
        {
            //l.lg("Transaction01()", "");

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
            int iTransMsdb = 0;
            try
            {
                string msg_MSDB = "error";
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();

                DataTable dt_MSDB = new DataTable();
                string sqlStr_MSDB = " select " + sBr;
                sqlStr_MSDB += " AGEN_NO, " + sBr; // 01.廠商代碼(VARCHAR2,6)
                sqlStr_MSDB += " DNO, " + sBr; // 02.交貨批次(INTEGER,)
                sqlStr_MSDB += " MMCODE, " + sBr; // 03.院內碼(VARCHAR2,13)
                sqlStr_MSDB += " PO_NO, " + sBr; // 04.訂單號碼(VARCHAR2,21)
                sqlStr_MSDB += " SEQ, " + sBr; // 05.流水號(INTEGER,)
                sqlStr_MSDB += " BARCODE, " + sBr; // 06.條碼(VARCHAR2,50)
                sqlStr_MSDB += " BW_SQTY, " + sBr; // 07.借貨量(INTEGER,)
                sqlStr_MSDB += " convert(nvarchar(19), DELI_DT, 120) DELI_DT , " + sBr; // 08.預計交貨日(DATE,) 2019-06-19 09:41:50
                sqlStr_MSDB += " convert(nvarchar(19), EXP_DATE, 120) EXP_DATE , " + sBr; // 09.效期(DATE,) 2019-06-19 09:41:50
                sqlStr_MSDB += " FLAG, " + sBr; // 10.轉檔標記(VARCHAR2,1)
                sqlStr_MSDB += " INQTY, " + sBr; // 11.交貨量(INTEGER,)
                sqlStr_MSDB += " INVOICE, " + sBr; // 12.發票號碼(VARCHAR2,10)
                sqlStr_MSDB += " LOT_NO, " + sBr; // 13.批號(VARCHAR2,20)
                sqlStr_MSDB += " MEMO, " + sBr; // 14.備註(VARCHAR2,150)
                sqlStr_MSDB += " STATUS, " + sBr; // 15.狀態(VARCHAR2,1)
                sqlStr_MSDB += " convert(nvarchar(19), CREATE_TIME, 120) CREATE_TIME , " + sBr; // 16.建立日期(DATE,)
                sqlStr_MSDB += " CREATE_USER, " + sBr; // 17.建立人員(VARCHAR2,10)
                sqlStr_MSDB += " UPDATE_IP, " + sBr; // 18.異動IP(VARCHAR2,20)
                sqlStr_MSDB += " convert(nvarchar(19), UPDATE_TIME, 120) UPDATE_TIME , " + sBr; // 19.異動日期(DATE,)
                sqlStr_MSDB += " UPDATE_USER, " + sBr; // 20.異動人員(VARCHAR2,10)
                sqlStr_MSDB += " convert(nvarchar(19), INVOICE_DT, 120) INVOICE_DT, " + sBr; // 09.效期(DATE,) 2019-06-19 09:41:50
                sqlStr_MSDB += " INVOICE_OLD, " + sBr; // 舊發票號碼(VARCHAR2,10)
                sqlStr_MSDB = sqlStr_MSDB.Substring(0, sqlStr_MSDB.Length - 4);
                sqlStr_MSDB += " from WB_REPLY where 1=1 ";
                sqlStr_MSDB += " and STATUS = 'B' ";    // 處理中
                sqlStr_MSDB += " and FLAG = 'A' ";      // 未處理
                dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
                //l.lg("Transaction01()", "sql=" + sqlStr_MSDB);
                if (msg_MSDB == "") //取得 WB_PUT_D 無錯誤
                {
                    //l.lg("Transaction01()", "dt_MSDB.Rows.Count=" + dt_MSDB.Rows.Count);
                    if (dt_MSDB.Rows.Count > 0) //WB_PUT_D 有資料
                    {
                        int rowsAffected_oracle = -1;
                        int rowsAffected_msdb = -1;

                        for (int i = 0; i < dt_MSDB.Rows.Count; i++)
                        {

                            string msg_oracle = "error";
                            string cmdstr = "insert into PH_REPLY_LOG( ";
                            cmdstr += " AGEN_NO, " + sBr; // 01.廠商代碼(VARCHAR2,6)
                            cmdstr += " DNO, " + sBr; // 02.交貨批次(INTEGER,)
                            cmdstr += " LOGTIME, " + sBr; // 03.轉檔日期(VARCHAR2,14)
                            cmdstr += " MMCODE, " + sBr; // 04.院內碼(VARCHAR2,13)
                            cmdstr += " PO_NO, " + sBr; // 05.訂單號碼(VARCHAR2,21)
                            cmdstr += " SEQ, " + sBr; // 06.流水號(INTEGER,)
                            cmdstr += " BARCODE, " + sBr; // 07.條碼(VARCHAR2,50)
                            cmdstr += " BW_SQTY, " + sBr; // 08.借貨量(INTEGER,)
                            cmdstr += " DELI_DT, " + sBr; // 09.預計交貨日(DATE,)
                            cmdstr += " EXP_DATE, " + sBr; // 10.效期(DATE,)
                            cmdstr += " FLAG, " + sBr; // 11.轉檔標記(VARCHAR2,1)
                            cmdstr += " INQTY, " + sBr; // 12.交貨量(INTEGER,)
                            cmdstr += " INVOICE, " + sBr; // 13.發票號碼(VARCHAR2,10)
                            cmdstr += " LOT_NO, " + sBr; // 14.批號(VARCHAR2,20)
                            cmdstr += " MEMO, " + sBr; // 15.備註(VARCHAR2,150)
                            cmdstr += " STATUS, " + sBr; // 16.狀態(VARCHAR2,1)
                            cmdstr += " CREATE_TIME, " + sBr; // 17.建立日期(DATE,)
                            cmdstr += " CREATE_USER, " + sBr; // 18.建立人員(VARCHAR2,10)
                            cmdstr += " UPDATE_IP, " + sBr; // 19.異動IP(VARCHAR2,20)
                            cmdstr += " UPDATE_TIME, " + sBr; // 20.異動日期(DATE,)
                            cmdstr += " UPDATE_USER, " + sBr; // 21.異動人員(VARCHAR2,10)
                            cmdstr += " INVOICE_DT, " + sBr; // 22.發票日期(DATE,)
                            cmdstr += " INVOICE_OLD, " + sBr; // 23.舊發票號碼(VARCHAR2,10)
                            cmdstr = cmdstr.Substring(0, cmdstr.Length - 4);
                            cmdstr += ") values( " + sBr;

                            cmdstr += " :agen_no, " + sBr; // 01.廠商代碼(VARCHAR2,Y)
                            cmdstr += " :dno, " + sBr; // 02.交貨批次(INTEGER,Y)
                            cmdstr += " :logtime, " + sBr; // 03.轉檔日期(VARCHAR2,Y)
                            cmdstr += " :mmcode, " + sBr; // 04.院內碼(VARCHAR2,Y)
                            cmdstr += " :po_no, " + sBr; // 05.訂單號碼(VARCHAR2,Y)
                            cmdstr += " :seq, " + sBr; // 06.流水號(INTEGER,Y)
                            cmdstr += " :barcode, " + sBr; // 07.條碼(VARCHAR2,)
                            cmdstr += " :bw_sqty, " + sBr; // 08.借貨量(INTEGER,Y)
                            cmdstr += " to_date(:deli_dt, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 09.預計交貨日(DATE,)
                            cmdstr += " to_date(:exp_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 10.效期(DATE,)
                            cmdstr += " :flag, " + sBr; // 11.轉檔標記(VARCHAR2,Y)
                            cmdstr += " :inqty, " + sBr; // 12.交貨量(INTEGER,Y)
                            cmdstr += " :invoice, " + sBr; // 13.發票號碼(VARCHAR2,)
                            cmdstr += " :lot_no, " + sBr; // 14.批號(VARCHAR2,)
                            cmdstr += " :memo, " + sBr; // 15.備註(VARCHAR2,)
                            cmdstr += " :status, " + sBr; // 16.狀態(VARCHAR2,Y)
                            cmdstr += " to_date(:create_time, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 17.建立日期(DATE,)
                            cmdstr += " :create_user, " + sBr; // 18.建立人員(VARCHAR2,)
                            cmdstr += " :update_ip, " + sBr; // 19.異動IP(VARCHAR2,)
                            cmdstr += " sysdate, " + sBr; // 20.異動日期(DATE,)
                            cmdstr += " 'AUTO', " + sBr; // 21.異動人員(VARCHAR2,)
                            cmdstr += " to_date(:invoice_dt, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 22.發票日期(DATE,)
                            cmdstr += " :invoice_old, " + sBr; // 23.舊發票號碼(VARCHAR2,)
                            cmdstr = cmdstr.Substring(0, cmdstr.Length - 4);
                            cmdstr += ") ";
                            transcmd_oracle.Add(cmdstr);
                            //l.lg("Transaction01()", "cmdstr=" + cmdstr);

                            ++iTransOra;
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_MSDB.Rows[i]["AGEN_NO"].ToString().Trim())); // 01.廠商代碼(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dno", "VarChar", dt_MSDB.Rows[i]["DNO"].ToString().Trim())); // 02.交貨批次(INTEGER,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":logtime", "VarChar", DateTime.Now.ToString("yyyyMMddHHmmss"))); // 03.轉檔日期(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_MSDB.Rows[i]["MMCODE"].ToString().Trim())); // 04.院內碼(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":po_no", "VarChar", dt_MSDB.Rows[i]["PO_NO"].ToString().Trim())); // 05.訂單號碼(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dt_MSDB.Rows[i]["SEQ"].ToString().Trim())); // 06.流水號(INTEGER,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":barcode", "VarChar", dt_MSDB.Rows[i]["BARCODE"].ToString().Trim())); // 07.條碼(VARCHAR2,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":bw_sqty", "VarChar", dt_MSDB.Rows[i]["BW_SQTY"].ToString().Trim())); // 08.借貨量(INTEGER,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":deli_dt", "VarChar", (dt_MSDB.Rows[i]["DELI_DT"].ToString().Length > 0) ? Convert.ToDateTime(dt_MSDB.Rows[i]["DELI_DT"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 09.預計交貨日(DATE,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":exp_date", "VarChar", (dt_MSDB.Rows[i]["EXP_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_MSDB.Rows[i]["EXP_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 10.效期(DATE,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":flag", "VarChar", dt_MSDB.Rows[i]["FLAG"].ToString().Trim())); // 11.轉檔標記(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":inqty", "VarChar", dt_MSDB.Rows[i]["INQTY"].ToString().Trim())); // 12.交貨量(INTEGER,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice", "VarChar", dt_MSDB.Rows[i]["INVOICE"].ToString().Trim())); // 13.發票號碼(VARCHAR2,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":lot_no", "VarChar", dt_MSDB.Rows[i]["LOT_NO"].ToString().Trim())); // 14.批號(VARCHAR2,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":memo", "VarChar", dt_MSDB.Rows[i]["MEMO"].ToString().Trim())); // 15.備註(VARCHAR2,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":status", "VarChar", dt_MSDB.Rows[i]["STATUS"].ToString().Trim())); // 16.狀態(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":create_time", "VarChar", (dt_MSDB.Rows[i]["CREATE_TIME"].ToString().Length > 0) ? Convert.ToDateTime(dt_MSDB.Rows[i]["CREATE_TIME"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 17.建立日期(DATE,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":create_user", "VarChar", dt_MSDB.Rows[i]["CREATE_USER"].ToString().Trim())); // 18.建立人員(VARCHAR2,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_ip", "VarChar", dt_MSDB.Rows[i]["UPDATE_IP"].ToString().Trim())); // 19.異動IP(VARCHAR2,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice_dt", "VarChar", (dt_MSDB.Rows[i]["INVOICE_DT"].ToString().Length > 0) ? Convert.ToDateTime(dt_MSDB.Rows[i]["INVOICE_DT"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 20
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":invoice_old", "VarChar", dt_MSDB.Rows[i]["INVOICE_OLD"].ToString().Trim())); // 23.舊發票號碼(VARCHAR2,)

                            //if (msg_oracle == "") //insert PH_PUT_D 無錯誤
                            //{
                            //    if (rowsAffected_oracle == 1) //insert PH_PUT_D 正常回傳筆數1
                            //    {

                            msg_MSDB = "error";
                            //List<CallDBtools_MSDb.MSDbParam> paraListA = new List<CallDBtools_MSDb.MSDbParam>();
                            sqlStr_MSDB = "update WB_REPLY set STATUS='C', FLAG = 'B', UPDATE_TIME =SYSDATETIME(), UPDATE_USER='AUTO' where 1=1 " + sBr;
                            sqlStr_MSDB += " and AGEN_NO = @agen_no " + sBr;// 01.廠商代碼(VARCHAR2,Y)
                            sqlStr_MSDB += " and DNO = @dno " + sBr;// 02.交貨批次(INTEGER,Y)
                            sqlStr_MSDB += " and MMCODE = @mmcode " + sBr;// 04.院內碼(VARCHAR2,Y)
                            sqlStr_MSDB += " and PO_NO = @po_no " + sBr;// 05.訂單號碼(VARCHAR2,Y)
                            sqlStr_MSDB += " and SEQ = @seq " + sBr;// 06.流水號(INTEGER,Y)
                            transcmd_msdb.Add(sqlStr_MSDB);
                            //l.lg("Transaction01()", "sqlStr_MSDB=" + sqlStr_MSDB);

                            ++iTransMsdb;
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@agen_no", "nvarchar", dt_MSDB.Rows[i]["AGEN_NO"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@dno", "nvarchar", dt_MSDB.Rows[i]["DNO"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@mmcode", "nvarchar", dt_MSDB.Rows[i]["MMCODE"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@po_no", "nvarchar", dt_MSDB.Rows[i]["PO_NO"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@seq", "nvarchar", dt_MSDB.Rows[i]["SEQ"].ToString().Trim()));
                            // rowsAffected_msdb = callDBtools_msdb.CallExecSQL(sqlStr_MSDB, listParam_msdb, "msdb", ref msg_MSDB);

                            //if (msg_MSDB != "")
                            //    callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP1-update WB_AIRHIS失敗:" + msg_MSDB, "AUTO");
                            //    }
                            //}
                            //else //insert PH_PUT_D 有錯誤
                            //{
                            //    callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP1-insert PH_AIRHIS失敗:" + msg_oracle, "AUTO");
                            //}
                        }

                        rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                                transcmd_oracle,
                                listParam_oracle,
                                transaction_oracle,
                                conn_oracle);
                        //l.lg("Transaction01()", "rowsAffected_oracle=" + rowsAffected_oracle);

                        rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
                                        transcmd_msdb,
                                        listParam_msdb,
                                        transaction_msdb,
                                        conn_msdb);
                        //l.lg("Transaction01()", "rowsAffected_msdb=" + rowsAffected_msdb);

                        transaction_oracle.Commit();
                        conn_oracle.Close();
                        //l.lg("Transaction01()", "transaction_oracle.Commit()");

                        transaction_msdb.Commit();
                        conn_msdb.Close();
                        //l.lg("Transaction01()", "transaction_msdb.Commit()");

                        ErrorStep += ",成功" + Environment.NewLine;
                    }
                }
                else //取得 WB_PUT_D 有錯誤，寫ERROR_LOG
                {
                    CallDBtools_Oracle callDBtools_oracle = new CallDBtools_Oracle();
                    callDBtools_oracle.I_ERROR_LOG("BHS003", "STEP1-取得WB_REPLY失敗:" + msg_MSDB, "AUTO");
                }
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP1 更新失敗--" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                transaction_msdb.Rollback();
                conn_msdb.Close();

                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS003", "STEP1 更新失敗--" + ex.Message);
                l.le("Transaction01()", "STEP1 更新失敗--" + ex.Message);
            }
        }



        // 2_更新院內 Oracle PH_PUT_M
        private void Transaction02()
        {
            //l.lg("Transaction02()", "");

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
                string msg_oracle = "error";
                //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_ph_reply_log = new DataTable();
                string sql_oracle = " select ";
                sql_oracle += " AGEN_NO, " + sBr; // 01.廠商代碼(VARCHAR2,Y)
                sql_oracle += " DNO, " + sBr; // 02.交貨批次(INTEGER,Y)
                sql_oracle += " LOGTIME, " + sBr; // 03.轉檔日期(VARCHAR2,Y)
                sql_oracle += " MMCODE, " + sBr; // 04.院內碼(VARCHAR2,Y)
                sql_oracle += " PO_NO, " + sBr; // 05.訂單號碼(VARCHAR2,Y)
                sql_oracle += " SEQ, " + sBr; // 06.流水號(INTEGER,Y)
                sql_oracle += " BARCODE, " + sBr; // 07.條碼(VARCHAR2,)
                sql_oracle += " BW_SQTY, " + sBr; // 08.借貨量(INTEGER,Y)
                sql_oracle += " to_char(DELI_DT,'yyyy/mm/dd hh24:mi:ss') DELI_DT, " + sBr; // 09.預計交貨日(DATE,)
                sql_oracle += " to_char(EXP_DATE,'yyyy/mm/dd hh24:mi:ss') EXP_DATE, " + sBr; // 10.效期(DATE,)
                sql_oracle += " FLAG, " + sBr; // 11.轉檔標記(VARCHAR2,Y)
                sql_oracle += " INQTY, " + sBr; // 12.交貨量(INTEGER,Y)
                sql_oracle += " INVOICE, " + sBr; // 13.發票號碼(VARCHAR2,)
                sql_oracle += " INVOICE_OLD, " + sBr; // 13.發票號碼 OLD(VARCHAR2,)
                sql_oracle += " LOT_NO, " + sBr; // 14.批號(VARCHAR2,)
                sql_oracle += " MEMO, " + sBr; // 15.備註(VARCHAR2,)
                sql_oracle += " STATUS, " + sBr; // 16.狀態(VARCHAR2,Y)
                sql_oracle += " to_char(CREATE_TIME,'yyyy/mm/dd hh24:mi:ss') CREATE_TIME, " + sBr; // 17.建立日期(DATE,)
                sql_oracle += " CREATE_USER, " + sBr; // 18.建立人員(VARCHAR2,)
                sql_oracle += " UPDATE_IP, " + sBr; // 19.異動IP(VARCHAR2,)
                sql_oracle += " to_char(UPDATE_TIME,'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME, " + sBr; // 20.異動日期(DATE,)
                sql_oracle += " UPDATE_USER, " + sBr; // 21.異動人員(VARCHAR2,)
                sql_oracle += " to_char(INVOICE_DT,'yyyy/mm/dd hh24:mi:ss') INVOICE_DT, " + sBr; // 22.發票號碼日期(DATE,)
                sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
                sql_oracle += " from PH_REPLY_LOG a where 1=1 " + sBr;
                sql_oracle += " and FLAG='A' " + sBr; // 未處理
                sql_oracle += " and( " + sBr;
                sql_oracle += "       select count(*) from PH_REPLY_LOG where 1=1 ";
                sql_oracle += "       and FLAG = 'B' " + sBr;
                sql_oracle += "       and AGEN_NO = a.AGEN_NO " + sBr;
                sql_oracle += "       and PO_NO = a.PO_NO " + sBr;
                sql_oracle += "       and DNO = a.DNO " + sBr;
                sql_oracle += "       and MMCODE = a.MMCODE " + sBr;
                sql_oracle += "       and SEQ = a.SEQ " + sBr;
                sql_oracle += " ) = 0 " + sBr;
                dt_ph_reply_log = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "AGEN_NO, PO_NO, DNO, MMCODE ", null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_ph_reply_log.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    //l.lg("Transaction02()", "dt_ph_reply_log.Rows.Count=" + dt_ph_reply_log.Rows.Count);

                    int rowsAffected_oracle = -1;
                    for (int i = 0; i < dt_ph_reply_log.Rows.Count; i++)
                    {

                        // 新增table PH_LOTNO(廠商批號效期資料檔)
                        String LOT_NO = dt_ph_reply_log.Rows[i]["LOT_NO"].ToString().Trim();
                        String EXP_DATE = dt_ph_reply_log.Rows[i]["EXP_DATE"].ToString().Trim();

                        if (!String.IsNullOrEmpty(LOT_NO) && !String.IsNullOrEmpty(EXP_DATE))
                        {
                            //移到進貨處理
                            //處理PH_LOTNO_01(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dt_ph_reply_log.Rows[i], callDbtools_oralce, ref msg_oracle); 
                        }
                        else
                        { // end of if (!String.IsNullOrEmpty(LOT_NO) && !String.IsNullOrEmpty(EXP_DATE))
                        } // end of if (!String.IsNullOrEmpty(LOT_NO) && !String.IsNullOrEmpty(EXP_DATE))

                        // 更新table PH_REPLY (院內廠商交貨回覆資料), 用Key去找PH_REPLY
                        處理PH_REPLY(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dt_ph_reply_log.Rows[i], callDbtools_oralce);

                        // (2)--更新 PH_INVOICE(發票資料檔)
                        處理PH_INVOICE(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dt_ph_reply_log.Rows[i], callDbtools_oralce, ref msg_oracle);

                        // (3) Update PH_REPLY_LOG [FLAG]= 'B'
                        更新PH_REPLY_LOG__FLAG(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dt_ph_reply_log.Rows[i], "B");
                    } // for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                            transcmd_oracle,
                            listParam_oracle,
                            transaction_oracle,
                            conn_oracle);
                    //l.lg("Transaction02()", "rowsAffected_oracle=" + rowsAffected_oracle);

                    transaction_oracle.Commit();
                    conn_oracle.Close();
                    //l.lg("Transaction02()", "transaction_oracle.Commit();");

                    ErrorStep += ",成功" + Environment.NewLine;
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP2-取得PH_REPLY_LOG失敗:" + msg_oracle, "AUTO");
                    l.le("Transaction02()", "STEP2-取得PH_REPLY_LOG失敗:" + msg_oracle);
                }
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP2 更新失敗--" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();

                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS003", "STEP2 更新失敗--" + ex.Message);
                l.le("Transaction02()", "STEP2 更新失敗--" + ex.Message);
            }
        } // end of private void Transaction02()



        private void Transaction03()
        {
            //l.lg("Transaction03()", "");

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
                string msg_oracle = "error";
                //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_ph_reply_log = new DataTable();
                string sql_oracle = " select ";
                sql_oracle += " AGEN_NO, " + sBr; // 01.廠商代碼(VARCHAR2,Y)
                sql_oracle += " DNO, " + sBr; // 03.交貨批次(INTEGER,Y)
                sql_oracle += " LOGTIME, " + sBr; // 03.轉檔日期(VARCHAR2,Y)
                sql_oracle += " MMCODE, " + sBr; // 04.院內碼(VARCHAR2,Y)
                sql_oracle += " PO_NO, " + sBr; // 05.訂單號碼(VARCHAR2,Y)
                sql_oracle += " SEQ, " + sBr; // 06.流水號(INTEGER,Y)
                sql_oracle += " BARCODE, " + sBr; // 07.條碼(VARCHAR2,)
                sql_oracle += " BW_SQTY, " + sBr; // 08.借貨量(INTEGER,Y)
                sql_oracle += " to_char(DELI_DT,'yyyy/mm/dd hh24:mi:ss') DELI_DT, " + sBr; // 09.預計交貨日(DATE,)
                sql_oracle += " to_char(EXP_DATE,'yyyy/mm/dd hh24:mi:ss') EXP_DATE, " + sBr; // 10.效期(DATE,)
                sql_oracle += " FLAG, " + sBr; // 11.轉檔標記(VARCHAR2,Y)
                sql_oracle += " INQTY, " + sBr; // 12.交貨量(INTEGER,Y)
                sql_oracle += " INVOICE, " + sBr; // 13.發票號碼(VARCHAR2
                sql_oracle += " INVOICE_OLD, " + sBr; // 13.發票號碼 OLD(VARCHAR2,)
                sql_oracle += " LOT_NO, " + sBr; // 14.批號(VARCHAR2,)
                sql_oracle += " MEMO, " + sBr; // 15.備註(VARCHAR2,)
                sql_oracle += " STATUS, " + sBr; // 16.狀態(VARCHAR2,Y)
                sql_oracle += " to_char(CREATE_TIME,'yyyy/mm/dd hh24:mi:ss') CREATE_TIME, " + sBr; // 17.建立日期(DATE,)
                sql_oracle += " CREATE_USER, " + sBr; // 18.建立人員(VARCHAR2,)
                sql_oracle += " UPDATE_IP, " + sBr; // 19.異動IP(VARCHAR2,)
                sql_oracle += " to_char(UPDATE_TIME,'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME, " + sBr; // 20.異動日期(DATE,)
                sql_oracle += " UPDATE_USER, " + sBr; // 21.異動人員(VARCHAR2,)
                sql_oracle += " to_char(INVOICE_DT,'yyyy/mm/dd hh24:mi:ss') INVOICE_DT, " + sBr; // 22.發票號碼日期(DATE,)
                sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4) + sBr;
                sql_oracle += " from PH_REPLY_LOG a where 1=1 " + sBr;
                sql_oracle += " and FLAG='A' " + sBr; // 未處理
                sql_oracle += " and( " + sBr;
                sql_oracle += "       select count(*) from PH_REPLY_LOG where 1=1 ";
                sql_oracle += "       and FLAG = 'B' " + sBr;
                sql_oracle += "       and AGEN_NO = a.AGEN_NO " + sBr;
                sql_oracle += "       and PO_NO = a.PO_NO " + sBr;
                sql_oracle += "       and DNO = a.DNO " + sBr;
                sql_oracle += "       and MMCODE = a.MMCODE " + sBr;
                //sql_oracle += "       and SEQ = a.SEQ " + sBr;
                sql_oracle += " ) > 0 " + sBr;


                dt_ph_reply_log = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "AGEN_NO, PO_NO, DNO, MMCODE ", null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_ph_reply_log.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    //l.lg("Transaction03()", "dt_ph_reply_log.Rows.Count=" + dt_ph_reply_log.Rows.Count);
                    int rowsAffected_oracle = -1;
                    for (int i = 0; i < dt_ph_reply_log.Rows.Count; i++)
                    {
                        // (1) 更新 PH_INVOICE(發票資料檔)
                        更新PH_INVOICE(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dt_ph_reply_log.Rows[i]);

                        // (2) 更新table PH_REPLY (院內廠商交貨回覆資料)
                        處理PH_REPLY__PH_REPLY_LOG_FALG是A__且已有資料(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dt_ph_reply_log.Rows[i]);
                    } // for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                            transcmd_oracle,
                            listParam_oracle,
                            transaction_oracle,
                            conn_oracle);
                    //l.lg("Transaction03()", "rowsAffected_oracle=" + rowsAffected_oracle);

                    transaction_oracle.Commit();
                    conn_oracle.Close();
                    //l.lg("Transaction03()", "transaction_oracle.Commit();");

                    ErrorStep += ",成功" + Environment.NewLine;
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP3-取得PH_REPLY_LOG失敗:" + msg_oracle, "AUTO");
                    l.le("Transaction03()", "STEP3-取得PH_REPLY_LOG失敗:" + msg_oracle);
                }
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BHS003", "STEP3 更新失敗--" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();

                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS003", "STEP3 更新失敗--" + ex.Message);
                l.le("Transaction03()", "STEP3 更新失敗--" + ex.Message);
            }
        } // end of private void Transaction03()



        private void Transaction04()
        {
            //l.lg("Transaction04()", "");

            string ErrorStep = "Start" + Environment.NewLine;
            // -- oracle -- 
            String msg_oracle = "";
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
            String msg_msdb = "";
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
            msg_msdb = "error";
            String sqlStr_MSDB = "";

            try
            {  //發票驗退
                String sql_oracle = "";
                sql_oracle += " select " + sBr;
                sql_oracle += " PO_NO, " + sBr;
                sql_oracle += " MMCODE, " + sBr;
                sql_oracle += " CKSTATUS, " + sBr;
                sql_oracle += " MEMO, " + sBr;
                sql_oracle += " INVOICE " + sBr;
                sql_oracle += " from PH_INVOICE " + sBr;
                sql_oracle += " where 1=1 " + sBr;
                sql_oracle += " and CKSTATUS='T' " + sBr;
                sql_oracle += " and FLAG ='A' " + sBr;
                List<CallDBtools_Oracle.OracleParam> listParam_oracle_ph_invoice = new List<CallDBtools_Oracle.OracleParam>();
                DataTable dt_oralce_ph_invoice = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "", listParam_oracle_ph_invoice, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "") //資料抓取沒有錯誤 且有資料
                {
                    //l.lg("Transaction04()", "dt_oralce_ph_invoice.Rows.Count=" + dt_oralce_ph_invoice.Rows.Count.ToString());
                    foreach (DataRow dr_ph_invoice in dt_oralce_ph_invoice.Rows)
                    {
                        sqlStr_MSDB = "";
                        sqlStr_MSDB += " update WB_REPLY set ";
                        sqlStr_MSDB += " STATUS = @status, " + sBr;
                        sqlStr_MSDB += " MEMO = @memo, " + sBr;
                        sqlStr_MSDB += " UPDATE_TIME = SYSDATETIME(), UPDATE_USER ='AUTO' where 1=1 " + sBr;
                        sqlStr_MSDB += " and PO_NO = @po_no " + sBr;// 05.訂單號碼(VARCHAR2,Y)
                        sqlStr_MSDB += " and MMCODE = @mmcode " + sBr;// 04.院內碼(VARCHAR2,Y)
                        sqlStr_MSDB += " and INVOICE = @invoice " + sBr;
                        transcmd_msdb.Add(sqlStr_MSDB);
                        //l.lg("Transaction01()", "sqlStr_MSDB=" + sqlStr_MSDB);
                        ++iTransMsdb;
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@status", "nvarchar", "T"));
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@memo", "nvarchar", dr_ph_invoice["MEMO"].ToString().Trim()));
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@po_no", "nvarchar", dr_ph_invoice["PO_NO"].ToString().Trim()));
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@mmcode", "nvarchar", dr_ph_invoice["MMCODE"].ToString().Trim()));
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@invoice", "nvarchar", dr_ph_invoice["INVOICE"].ToString().Trim()));
                        更新PH_INVOICE__FLAG_B(ref transcmd_oracle, ref iTransOra, ref listParam_oracle, dr_ph_invoice);
                    }
                } // end of if (msg_oracle == "") 

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

                //l.lg("Transaction04()", "ok");
                ErrorStep += ",成功" + Environment.NewLine;
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BHS003", "Transaction04 失敗:" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                transaction_msdb.Rollback();
                conn_msdb.Close();
                l.le("Transaction04()", "Exception=" + ex.Message);
            }
        } // 

        #endregion


        L l = new L("BHS003");
        FL fl = new FL("BHS003");
        String sBr = "\r\n";
        CallDBtools calldbtools = new CallDBtools();

        public C_BHS003(string[] args)
        {
        } // 

        public void run()
        {
            //l.lg("run()", "");

            String sCurPath = AppDomain.CurrentDomain.BaseDirectory;
            String sDbStatusFilePath = sCurPath + "\\db_status.html";
            if (File.Exists(sDbStatusFilePath))
                File.Delete(sDbStatusFilePath);
            try
            {
                //l.ldb(get資料庫現況("run()"));

                // 1 01.外網 MSdb WB_REPLY -> 院內 Oracle PH_REPLY_LOG, 02.update WB_REPLY.FLAG='B' 已處理
                Transaction01();
                //l.ldb(get資料庫現況("Transaction01()"));

                // 2 01.更新院內 Oracle PH_AIRHIS -> PH_AIRST(GO取走,GI換入,CH修改)
                Transaction02();
                //l.ldb(get資料庫現況("Transaction02()"));

                Transaction03();
                //l.ldb(get資料庫現況("Transaction03()"));

                Transaction04();
                //l.ldb(get資料庫現況("Transaction04()"));

                // 寄送mail通知admin有執行程式 
                //fl.sendMailToAdmin("三總排程-BHS003執行完畢", "程式正常執行完畢", l.get資料庫現況Log檔路徑());
            }
            catch (Exception ex)
            {
                // 寄送mail通知admin有執行程式 
                //fl.sendMailToAdmin("三總排程-BHS003執行異常", "Exception=" + ex.Message, l.get資料庫現況Log檔路徑());
                l.le("run()", ex.Message);
            }
        } // 
    } // ec
} // en
