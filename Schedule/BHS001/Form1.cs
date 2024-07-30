using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.SqlClient;
using JCLib.DB.Tool;
using System.IO;

namespace BHS001
{
    public partial class Form1 : Form
    {
        #region " 整包transaction案例 "

        //void tranSample() {
        //    CallDBtools calldbtools = new CallDBtools();
        //    String sql = "";
        //    string ErrorStep = "Start" + Environment.NewLine;

        //    // -- oracle -- 
        //    CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
        //    String s_conn_oracle = calldbtools.SelectDB("oracle");
        //    OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
        //    if (conn_oracle.State == ConnectionState.Open)
        //        conn_oracle.Close();
        //    conn_oracle.Open();
        //    OracleTransaction transaction_oracle = conn_oracle.BeginTransaction();
        //    List<string> transcmd_oracle = new List<string>();
        //    List<CallDBtools_Oracle.OracleParam> listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();


        //    // -- msdb -- 
        //    CallDBtools_MSDb callDbtools_msdb = new CallDBtools_MSDb();
        //    String s_conn_msdb = calldbtools.SelectDB("msdb");

        //    SqlConnection conn_msdb = new SqlConnection(s_conn_msdb);
        //    if (conn_msdb.State == ConnectionState.Open)
        //        conn_msdb.Close();
        //    conn_msdb.Open();
        //    SqlTransaction transaction_msdb = conn_msdb.BeginTransaction();
        //    List<string> transcmd_msdb = new List<string>();
        //    List<CallDBtools_MSDb.MSDbParam> listParam_msdb = new List<CallDBtools_MSDb.MSDbParam>();

        //    try
        //    {
        //        // -- oracle -- 
        //        sql = "insert into table(..) values(:aa)";
        //        listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(1, ":aa", "VarChar", "值"));
        //        transcmd_oracle.Add(sql);

        //        sql = "insert into table(..) values(:aa)";
        //        listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(2, ":aa", "VarChar", "值"));
        //        transcmd_oracle.Add(sql);

        //        int rowsAffected_oracle = -1;
        //        rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
        //            transcmd_oracle,
        //            listParam_oracle,
        //            transaction_oracle,
        //            conn_oracle);

        //        // -- msdb
        //        sql = "insert into WB_AIRHIS (";
        //        sql += " FBNO, " + sBr; // 01.瓶號(VARCHAR2,20)
        //        sql += " MMCODE, " + sBr; // 02.三總院內碼(VARCHAR2,13)
        //        sql += " SEQ, " + sBr; // 03.交易流水號(INTEGER,)
        //        sql += " TXTDAY, " + sBr; // 04.更換日期(DATE,)
        //        sql += " AGEN_NO, " + sBr; // 05.廠商碼(VARCHAR2,6)
        //        sql += " AIR, " + sBr; // 06.氣體(VARCHAR2,100)
        //        sql += " CHK_DATE, " + sBr; // 07.檢驗日期(DATE,)
        //        sql += " DEPT, " + sBr; // 08.放置地點(VARCHAR2,6)
        //        sql += " EXP_DATE, " + sBr; // 09.保存期限(DATE,)
        //        sql += " EXTYPE, " + sBr; // 10.更換類別(VARCHAR2,2)
        //        sql += " INPUT_DATE, " + sBr; // 11.灌氣日期(DATE,)
        //        sql += " MAT, " + sBr; // 12.材質類別(VARCHAR2,20)
        //        sql += " MEMO, " + sBr; // 13.備註(VARCHAR2,50)
        //        sql += " SBNO, " + sBr; // 14.鋼號(VARCHAR2,20)
        //        sql += " STATUS, " + sBr; // 15.狀態(VARCHAR2,1)
        //        sql += " XSIZE, " + sBr; // 16.容量(VARCHAR2,50)
        //        sql += " CREATE_TIME, " + sBr; // 17.建立日期(DATE,)
        //        sql += " CREATE_USER, " + sBr; // 18.建立人員(VARCHAR2,10)
        //        sql += " UPDATE_IP, " + sBr; // 19.異動IP(VARCHAR2,20)
        //        sql += " UPDATE_TIME, " + sBr; // 20.異動日期(DATE,)
        //        sql += " UPDATE_USER, " + sBr; // 21.異動人員(VARCHAR2,10)
        //        sql = sql.Substring(0, sql.Length - 4);
        //        sql += " ) values ( " + sBr;
        //        sql += " '瓶號', " + sBr; // 01.瓶號(VARCHAR2,20)
        //        sql += " '三總院內碼', " + sBr; // 02.三總院內碼(VARCHAR2,13)
        //        sql += " 1, " + sBr; // 03.交易流水號(INTEGER,)
        //        sql += " SYSDATETIME(), " + sBr; // 04.更換日期(DATE,)                    SYSDATETIME(),
        //        sql += " '1', " + sBr; // 05.廠商碼(VARCHAR2,6)
        //        sql += " '氣體', " + sBr; // 06.氣體(VARCHAR2,100)
        //        sql += " SYSDATETIME(), " + sBr; // 07.檢驗日期(DATE,)
        //        sql += " '地點', " + sBr; // 08.放置地點(VARCHAR2,6)
        //        sql += " SYSDATETIME(), " + sBr; // 09.保存期限(DATE,)
        //        sql += " 'A', " + sBr; // 10.更換類別(VARCHAR2,2)
        //        sql += " SYSDATETIME(), " + sBr; // 11.灌氣日期(DATE,)
        //        sql += " '類別', " + sBr; // 12.材質類別(VARCHAR2,20)
        //        sql += " '備註', " + sBr; // 13.備註(VARCHAR2,50)
        //        sql += " '鋼號', " + sBr; // 14.鋼號(VARCHAR2,20)
        //        sql += " 'A', " + sBr; // 15.狀態(VARCHAR2,1)
        //        sql += " '容量', " + sBr; // 16.容量(VARCHAR2,50)
        //        sql += " SYSDATETIME(), " + sBr; // 17.建立日期(DATE,)
        //        sql += " '1', " + sBr; // 18.建立人員(VARCHAR2,10)
        //        sql += " '192', " + sBr; // 19.異動IP(VARCHAR2,20)
        //        sql += " SYSDATETIME(), " + sBr; // 20.異動日期(DATE,)
        //        sql += " @update_user, " + sBr; // 21.異動人員(VARCHAR2,10)
        //        sql = sql.Substring(0, sql.Length - 4);
        //        sql += " ) " + sBr;
        //        sql = sql.Replace(sBr, "");
        //        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_user", "VarChar", "1"));
        //        transcmd_msdb.Add(sql);

        //        sql = "insert into WB_AIRHIS (";
        //        sql += " FBNO, " + sBr; // 01.瓶號(VARCHAR2,20)
        //        sql += " MMCODE, " + sBr; // 02.三總院內碼(VARCHAR2,13)
        //        sql += " SEQ, " + sBr; // 03.交易流水號(INTEGER,)
        //        sql += " TXTDAY, " + sBr; // 04.更換日期(DATE,)
        //        sql += " AGEN_NO, " + sBr; // 05.廠商碼(VARCHAR2,6)
        //        sql += " AIR, " + sBr; // 06.氣體(VARCHAR2,100)
        //        sql += " CHK_DATE, " + sBr; // 07.檢驗日期(DATE,)
        //        sql += " DEPT, " + sBr; // 08.放置地點(VARCHAR2,6)
        //        sql += " EXP_DATE, " + sBr; // 09.保存期限(DATE,)
        //        sql += " EXTYPE, " + sBr; // 10.更換類別(VARCHAR2,2)
        //        sql += " INPUT_DATE, " + sBr; // 11.灌氣日期(DATE,)
        //        sql += " MAT, " + sBr; // 12.材質類別(VARCHAR2,20)
        //        sql += " MEMO, " + sBr; // 13.備註(VARCHAR2,50)
        //        sql += " SBNO, " + sBr; // 14.鋼號(VARCHAR2,20)
        //        sql += " STATUS, " + sBr; // 15.狀態(VARCHAR2,1)
        //        sql += " XSIZE, " + sBr; // 16.容量(VARCHAR2,50)
        //        sql += " CREATE_TIME, " + sBr; // 17.建立日期(DATE,)
        //        sql += " CREATE_USER, " + sBr; // 18.建立人員(VARCHAR2,10)
        //        sql += " UPDATE_IP, " + sBr; // 19.異動IP(VARCHAR2,20)
        //        sql += " UPDATE_TIME, " + sBr; // 20.異動日期(DATE,)
        //        sql += " UPDATE_USER, " + sBr; // 21.異動人員(VARCHAR2,10)
        //        sql = sql.Substring(0, sql.Length - 4);
        //        sql += " ) values ( " + sBr;
        //        sql += " '瓶號', " + sBr; // 01.瓶號(VARCHAR2,20)
        //        sql += " '三總院內碼', " + sBr; // 02.三總院內碼(VARCHAR2,13)
        //        sql += " 1, " + sBr; // 03.交易流水號(INTEGER,)
        //        sql += " SYSDATETIME(), " + sBr; // 04.更換日期(DATE,)                    SYSDATETIME(),
        //        sql += " '1', " + sBr; // 05.廠商碼(VARCHAR2,6)
        //        sql += " '氣體', " + sBr; // 06.氣體(VARCHAR2,100)
        //        sql += " SYSDATETIME(), " + sBr; // 07.檢驗日期(DATE,)
        //        sql += " '地點', " + sBr; // 08.放置地點(VARCHAR2,6)
        //        sql += " SYSDATETIME(), " + sBr; // 09.保存期限(DATE,)
        //        sql += " 'A', " + sBr; // 10.更換類別(VARCHAR2,2)
        //        sql += " SYSDATETIME(), " + sBr; // 11.灌氣日期(DATE,)
        //        sql += " '類別', " + sBr; // 12.材質類別(VARCHAR2,20)
        //        sql += " '備註', " + sBr; // 13.備註(VARCHAR2,50)
        //        sql += " '鋼號', " + sBr; // 14.鋼號(VARCHAR2,20)
        //        sql += " 'A', " + sBr; // 15.狀態(VARCHAR2,1)
        //        sql += " '容量', " + sBr; // 16.容量(VARCHAR2,50)
        //        sql += " SYSDATETIME(), " + sBr; // 17.建立日期(DATE,)
        //        sql += " '1', " + sBr; // 18.建立人員(VARCHAR2,10)
        //        sql += " '192', " + sBr; // 19.異動IP(VARCHAR2,20)
        //        sql += " SYSDATETIME(), " + sBr; // 20.異動日期(DATE,)
        //        sql += " @update_user, " + sBr; // 21.異動人員(VARCHAR2,10)
        //        sql = sql.Substring(0, sql.Length - 4);
        //        sql += " ) " + sBr;
        //        sql = sql.Replace(sBr, "");
        //        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(2, "@update_user", "VarChar", "1"));
        //        transcmd_msdb.Add(sql);


        //        int rowsAffected_msdb = -1;
        //        rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
        //            transcmd_msdb,
        //            listParam_msdb,
        //            transaction_msdb,
        //            conn_msdb);

        //        transaction_oracle.Commit();
        //        conn_oracle.Close();

        //        transaction_msdb.Commit();
        //        conn_msdb.Close();

        //        ErrorStep += ",成功" + Environment.NewLine;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorStep += ",失敗" + Environment.NewLine;
        //        ErrorStep += ex.ToString() + Environment.NewLine;
        //        transaction_oracle.Rollback();
        //        conn_oracle.Close();
        //        transaction_msdb.Rollback();
        //        conn_msdb.Close();
        //    }
        //} // 

        //select* from WB_AIRHIS
        //select convert(nvarchar(19), INPUT_DATE, 120) TXTDAY, *from WB_AIRHIS where convert(nvarchar(19), INPUT_DATE, 120) = '2019-05-16 00:00:00'
        //select convert(nvarchar(19), INPUT_DATE, 120) TXTDAY, *from WB_AIRHIS where convert(nvarchar(19), INPUT_DATE, 120) = '2019-05-16 15:50:29'
        //select convert(nvarchar(19), INPUT_DATE, 120) TXTDAY, *from WB_AIRHIS where convert(nvarchar(19), INPUT_DATE, 120) = '2019-05-16 15:50:31'
        //select convert(nvarchar(19), INPUT_DATE, 120) TXTDAY, *from WB_AIRHIS where convert(nvarchar(19), INPUT_DATE, 120) = '2019-05-17 00:00:00'

        //update WB_AIRHIS set AGEN_NO = 'admin', SEQ = '1', EXTYPE = 'GO', DEPT = '010000', STATUS = 'A', update_IP = '192.20.2.243' where convert(nvarchar(19), INPUT_DATE, 120)= '2019-05-16 00:00:00';
        //update WB_AIRHIS set AGEN_NO = 'admin', SEQ = '2', EXTYPE = 'GI', DEPT = '010000', STATUS = 'A', update_IP = '192.20.2.243' where convert(nvarchar(19), INPUT_DATE, 120)= '2019-05-16 15:50:29';
        //update WB_AIRHIS set AGEN_NO = 'admin', SEQ = '3', EXTYPE = 'UP', DEPT = '010000', STATUS = 'A', update_IP = '192.20.2.243' where convert(nvarchar(19), INPUT_DATE, 120)= '2019-05-16 15:50:31';
        //update WB_AIRHIS set AGEN_NO = 'admin', SEQ = '4', EXTYPE = 'GI', DEPT = '010000', STATUS = 'A', update_IP = '192.20.2.243' where convert(nvarchar(19), INPUT_DATE, 120)= '2019-05-17 00:00:00';            

        // -- 驗證 Oracle 正確性
        //select EXTYPE, AGEN_NO, FBNO, SEQ, to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') TXTDAY, STATUS  from PH_AIRHIS;
        //select AGEN_NO, FBNO, SEQ, to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') TXTDAY from PH_AIRST;
        //select * from PH_AIRTIME
        // -- 驗證 MsSql 正確性
        //select EXTYPE, AGEN_NO, FBNO, SEQ, convert(nvarchar(19), TXTDAY, 120) TXTDAY, STATUS from WB_AIRHIS;
        //select AGEN_NO, FBNO, SEQ, convert(nvarchar(19), TXTDAY, 120) TXTDAY from WB_AIRST;
        //select convert(nvarchar(19), UPDATE_TIME, 120) UPDATE_TIME from WB_AIRTIME
        // --------------------

        DataTable WB_AIRHIS;
        DataTable PH_AIRHIS;
        DataTable WB_AIRST;
        DataTable PH_AIRST;
        DataTable WB_AIRTIME;
        DataTable PH_AIRTIME;
        DataTable ERROR_LOG;
        String get資料庫現況(String tableTitle)
        {
            String s = "";
            // -- 顯示院外 MsSql -- 
            string msg_MSDB = "error";
            CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();

            DataTable dt_MSDB = new DataTable();
            string sqlStr_MSDB = "";

            // -- 顯示院內 -- 
            //select EXTYPE, AGEN_NO, FBNO, SEQ, to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') TXTDAY, STATUS  from PH_AIRHIS;
            //select AGEN_NO, FBNO, SEQ, to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') TXTDAY from PH_AIRST;
            //select to_char(UPDATE_TIME, 'yyyy/mm/dd hh24:mi:ss') from PH_AIRTIME

            string msg_oracle = "error";
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            DataTable dt_oralce = new DataTable();
            string sql_oracle = "";
            String sDtToday = DateTime.Now.ToString("yyyy/MM/dd");

            s += "<table border=\"1\">" + sBr;
            s += "<tr>" + sBr;
            s += "<td colspan=\"2\">" + tableTitle + "</td>" + sBr;
            s += "</tr>" + sBr;
            s += "<tr>" + sBr;
            s += "<td>院外</td>" + sBr;
            s += "<td>院內</td>" + sBr;
            s += "</tr>" + sBr;


            // WB_AIRHIS, PH_AIRHIS
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sqlStr_MSDB = " select EXTYPE, AGEN_NO, FBNO, SEQ, convert(nvarchar(19), TXTDAY, 120) TXTDAY, STATUS, FLAG, NAMEC from WB_AIRHIS where 1=1 and CONVERT(VARCHAR, TXTDAY, 111)='" + sDtToday + "' "; //  
            dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
            s += l.getHtmlTable("WB_AIRHIS", dt_MSDB, WB_AIRHIS, "AGEN_NO, FBNO");
            WB_AIRHIS = dt_MSDB;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = " select EXTYPE, AGEN_NO, FBNO, SEQ, to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') TXTDAY, STATUS, FLAG, NAMEC from PH_AIRHIS where 1=1 and to_char(TXTDAY, 'yyyy/mm/dd')='" + sDtToday + "' ";
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_AIRHIS", dt_oralce, PH_AIRHIS, "AGEN_NO, FBNO");
            PH_AIRHIS = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;


            // WB_AIRST, PH_AIRST
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sqlStr_MSDB = " select AGEN_NO, FBNO, SEQ, convert(nvarchar(19), TXTDAY, 120) TXTDAY, XSIZE, NAMEC from WB_AIRST where 1=1 and CONVERT(VARCHAR, TXTDAY, 111)='" + sDtToday + "' "; 
            dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
            s += l.getHtmlTable("WB_AIRST", dt_MSDB, WB_AIRST, "AGEN_NO, FBNO");
            WB_AIRST = dt_MSDB;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = "select AGEN_NO, FBNO, SEQ, to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') TXTDAY, XSIZE, NAMEC from PH_AIRST where 1=1 and to_char(TXTDAY, 'yyyy/mm/dd')='" + sDtToday + "' ";
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_AIRST", dt_oralce, PH_AIRST, "AGEN_NO, FBNO");
            PH_AIRST = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;
            

            // WB_AIRTIME, PH_AIRTIME
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sqlStr_MSDB = " select convert(nvarchar(19), update_time, 120) update_time from WB_AIRTIME ";
            dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
            s += l.getHtmlTable("WB_AIRTIME", dt_MSDB, WB_AIRTIME, "UPDATE_TIME");
            WB_AIRTIME = dt_MSDB;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = "select to_char(UPDATE_TIME, 'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME from PH_AIRTIME ";
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_AIRTIME", dt_oralce, PH_AIRTIME, "UPDATE_TIME");
            PH_AIRTIME = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;
            
            // ERROR_LOG
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = "select * from ERROR_LOG where PG='BHS001' ";
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("ERROR_LOG", dt_oralce, ERROR_LOG, "LOGTIME");
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
            CallDBtools calldbtools = new CallDBtools();
            String sql = "";
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

            String sDtToday = DateTime.Now.ToString("yyyy/MM/dd");
            String sDtTodayMsSql = DateTime.Now.ToString("yyyy-MM-dd");

            try
            {
                // -- oracle -- 
                sql = "delete from PH_AIRHIS where to_char(TXTDAY, 'yyyy/mm/dd')='" + sDtToday + "' ";
                transcmd_oracle.Add(sql);

                sql = "delete from PH_AIRST where to_char(TXTDAY, 'yyyy/mm/dd')='" + sDtToday + "' ";
                transcmd_oracle.Add(sql);

                sql = " insert into PH_AIRST ( " + sBr;
                sql += " AGEN_NO, " + sBr; // 01.廠商碼(VARCHAR2,6)
                sql += " FBNO, " + sBr; // 02.瓶號(VARCHAR2,20)
                //sql += " MMCODE, " + sBr; // 03.三總院內碼(VARCHAR2,13)
                sql += " SEQ, " + sBr; // 04.交易流水號(INTEGER,)
                sql += " TXTDAY, " + sBr; // 05.更換日期(DATE,)
                //sql += " AIR, " + sBr; // 06.氣體(VARCHAR2,100)
                //sql += " CHK_DATE, " + sBr; // 07.檢驗日期(DATE,)
                //sql += " DEPT, " + sBr; // 08.放置位置(VARCHAR2,6)
                //sql += " EXP_DATE, " + sBr; // 09.保存期限(DATE,)
                //sql += " INPUT_DATE, " + sBr; // 10.灌氣日期(DATE,)
                //sql += " MAT, " + sBr; // 11.材質類別(VARCHAR2,20)
                //sql += " MEMO, " + sBr; // 12.備註(VARCHAR2,50)
                //sql += " SBNO, " + sBr; // 13.鋼號(VARCHAR2,20)
                sql += " XSIZE, " + sBr; // 14.鋼瓶尺寸(VARCHAR2,50)
                sql += " NAMEC, " + sBr; // 15.品名(VARCHAR2,200)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) values ( " + sBr;
                sql += " 'admin', " + sBr; // 01.廠商碼(VARCHAR2,Y)
                sql += " '1', " + sBr; // 02.瓶號(VARCHAR2,Y)
                //sql += " '1', " + sBr; // 03.三總院內碼(VARCHAR2,Y)
                sql += " '1', " + sBr; // 04.交易流水號(INTEGER,Y)
                sql += " to_date('" + sDtToday + " 00:00:01', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 05.更換日期(DATE,Y)
                //sql += " 'air1', " + sBr; // 06.氣體(VARCHAR2,Y)
                //sql += " to_date('2019/05/16 00:00:01', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 07.檢驗日期(DATE,Y)
                //sql += " '01000', " + sBr; // 08.放置位置(VARCHAR2,)
                //sql += " to_date('2019/05/16 00:00:01', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 09.保存期限(DATE,Y)
                //sql += " to_date('2019/05/16 00:00:01', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 10.灌氣日期(DATE,Y)
                //sql += " '1', " + sBr; // 11.材質類別(VARCHAR2,Y)
                //sql += " 'memo1', " + sBr; // 12.備註(VARCHAR2,)
                //sql += " 'sbno1', " + sBr; // 13.鋼號(VARCHAR2,)
                sql += " 'xsize1', " + sBr; // 14.鋼瓶尺寸(VARCHAR2,Y)
                sql += " '待拿走_品名1', " + sBr; // 15.品名(VARCHAR2,200)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) " + sBr;
                transcmd_oracle.Add(sql);

                //sql = " insert into PH_AIRST ( " + sBr;
                //sql += " AGEN_NO, " + sBr; // 01.廠商碼(VARCHAR2,6)
                //sql += " FBNO, " + sBr; // 02.瓶號(VARCHAR2,20)
                //sql += " MMCODE, " + sBr; // 03.三總院內碼(VARCHAR2,13)
                //sql += " SEQ, " + sBr; // 04.交易流水號(INTEGER,)
                //sql += " TXTDAY, " + sBr; // 05.更換日期(DATE,)
                //sql += " AIR, " + sBr; // 06.氣體(VARCHAR2,100)
                //sql += " CHK_DATE, " + sBr; // 07.檢驗日期(DATE,)
                //sql += " DEPT, " + sBr; // 08.放置位置(VARCHAR2,6)
                //sql += " EXP_DATE, " + sBr; // 09.保存期限(DATE,)
                //sql += " INPUT_DATE, " + sBr; // 10.灌氣日期(DATE,)
                //sql += " MAT, " + sBr; // 11.材質類別(VARCHAR2,20)
                //sql += " MEMO, " + sBr; // 12.備註(VARCHAR2,50)
                //sql += " SBNO, " + sBr; // 13.鋼號(VARCHAR2,20)
                //sql += " XSIZE, " + sBr; // 14.鋼瓶尺寸(VARCHAR2,50)
                //sql = sql.Substring(0, sql.Length - 4);
                //sql += " ) values ( " + sBr;
                //sql += " 'admin', " + sBr; // 01.廠商碼(VARCHAR2,Y)
                //sql += " '2', " + sBr; // 02.瓶號(VARCHAR2,Y)
                //sql += " '2', " + sBr; // 03.三總院內碼(VARCHAR2,Y)
                //sql += " '2', " + sBr; // 04.交易流水號(INTEGER,Y)
                //sql += " to_date('2019/05/16 00:00:02', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 05.更換日期(DATE,Y)
                //sql += " 'air2', " + sBr; // 06.氣體(VARCHAR2,Y)
                //sql += " to_date('2019/05/16 15:50:31', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 07.檢驗日期(DATE,Y)
                //sql += " '01000', " + sBr; // 08.放置位置(VARCHAR2,)
                //sql += " to_date('2019/05/16 15:50:31', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 09.保存期限(DATE,Y)
                //sql += " to_date('2019/05/16 00:00:02', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 10.灌氣日期(DATE,Y)
                //sql += " '2', " + sBr; // 11.材質類別(VARCHAR2,Y)
                //sql += " 'memo2', " + sBr; // 12.備註(VARCHAR2,)
                //sql += " 'sbno2', " + sBr; // 13.鋼號(VARCHAR2,)
                //sql += " 'xsize2', " + sBr; // 14.鋼瓶尺寸(VARCHAR2,Y)
                //sql = sql.Substring(0, sql.Length - 4);
                //sql += " ) " + sBr;
                //transcmd_oracle.Add(sql);

                sql = " insert into PH_AIRST ( " + sBr;
                sql += " AGEN_NO, " + sBr; // 01.廠商碼(VARCHAR2,6)
                sql += " FBNO, " + sBr; // 02.瓶號(VARCHAR2,20)
                //sql += " MMCODE, " + sBr; // 03.三總院內碼(VARCHAR2,13)
                sql += " SEQ, " + sBr; // 04.交易流水號(INTEGER,)
                sql += " TXTDAY, " + sBr; // 05.更換日期(DATE,)
                //sql += " AIR, " + sBr; // 06.氣體(VARCHAR2,100)
                //sql += " CHK_DATE, " + sBr; // 07.檢驗日期(DATE,)
                //sql += " DEPT, " + sBr; // 08.放置位置(VARCHAR2,6)
                //sql += " EXP_DATE, " + sBr; // 09.保存期限(DATE,)
                //sql += " INPUT_DATE, " + sBr; // 10.灌氣日期(DATE,)
                //sql += " MAT, " + sBr; // 11.材質類別(VARCHAR2,20)
                //sql += " MEMO, " + sBr; // 12.備註(VARCHAR2,50)
                //sql += " SBNO, " + sBr; // 13.鋼號(VARCHAR2,20)
                sql += " XSIZE, " + sBr; // 14.鋼瓶尺寸(VARCHAR2,50)
                sql += " NAMEC, " + sBr; // 15.品名(VARCHAR2,200)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) values ( " + sBr;
                sql += " 'admin', " + sBr; // 01.廠商碼(VARCHAR2,Y)
                sql += " '3', " + sBr; // 02.瓶號(VARCHAR2,Y)
                //sql += " '3', " + sBr; // 03.三總院內碼(VARCHAR2,Y)
                sql += " '3', " + sBr; // 04.交易流水號(INTEGER,Y)
                sql += " to_date('" + sDtToday + " 00:00:03', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 05.更換日期(DATE,Y)
                //sql += " 'air1', " + sBr; // 06.氣體(VARCHAR2,Y)
                //sql += " to_date('2019/05/16 15:50:31', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 07.檢驗日期(DATE,Y)
                //sql += " '333333', " + sBr; // 08.放置位置(VARCHAR2,)
                //sql += " to_date('2019/05/16 15:50:31', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 09.保存期限(DATE,Y)
                //sql += " to_date('2019/05/16 00:00:03', 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 10.灌氣日期(DATE,Y)
                //sql += " '3', " + sBr; // 11.材質類別(VARCHAR2,Y)
                //sql += " 'memo3', " + sBr; // 12.備註(VARCHAR2,)
                //sql += " 'sbno3', " + sBr; // 13.鋼號(VARCHAR2,)
                sql += " 'xsize3', " + sBr; // 14.鋼瓶尺寸(VARCHAR2,Y)
                sql += " '院內還在使用的_品名3', " + sBr; // 15.品名(VARCHAR2,200)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) " + sBr;
                transcmd_oracle.Add(sql);

                sql = "delete from ERROR_LOG where PG='BHS001' ";
                transcmd_oracle.Add(sql);

                int rowsAffected_oracle = -1;
                rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                    transcmd_oracle,
                    listParam_oracle,
                    transaction_oracle,
                    conn_oracle);

                // -- msdb
                int iTransIdx = 0;


                sql = "delete from WB_AIRHIS where AGEN_NO='admin' or CONVERT(VARCHAR, TXTDAY, 111)='" + sDtToday + "' "; // 
                transcmd_msdb.Add(sql);


                sql = "delete from WB_AIRST where AGEN_NO='admin' or CONVERT(VARCHAR, TXTDAY, 111)='" + sDtToday + "' "; // 
                transcmd_msdb.Add(sql);

                //sql = "delete WB_AIRHIS where convert(VARCHAR, TXTDAY, 111) <>@txtday ";
                //sql = sql.Replace(sBr, "");
                //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(++iTransIdx, "@txtday", "VarChar", "2019/05/16"));
                //transcmd_msdb.Add(sql);

                //sql = "update WB_AIRHIS set AGEN_NO = 'admin', FBNO='1', MMCODE='1', SEQ= '1', EXTYPE = 'GO', DEPT = '010000', STATUS=@status , FLAG='A', update_IP = '192.20.2.243' where convert(nvarchar(19), INPUT_DATE, 120)= '2019-05-16 00:00:00' ";
                //sql = sql.Replace(sBr, "");
                //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(++iTransIdx, "@status", "VarChar", "A"));
                //transcmd_msdb.Add(sql);

                //sql = "update WB_AIRHIS set AGEN_NO = 'admin', FBNO='2', MMCODE='2', SEQ= '2', EXTYPE = 'GI', DEPT = '010000', STATUS =@status, FLAG='A', update_IP = '192.20.2.243' where convert(nvarchar(19), INPUT_DATE, 120)= '2019-05-16 15:50:29' ";
                //sql = sql.Replace(sBr, "");
                //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(++iTransIdx, "@status", "VarChar", "A"));
                //transcmd_msdb.Add(sql);

                //sql = "update WB_AIRHIS set AGEN_NO = 'admin', FBNO='3', MMCODE='3', SEQ= '3', EXTYPE = 'CH', DEPT = '010000', STATUS =@status, FLAG='A', update_IP = '192.20.2.243' where convert(nvarchar(19), INPUT_DATE, 120)= '2019-05-16 15:50:31' ";
                //sql = sql.Replace(sBr, "");
                //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(++iTransIdx, "@status", "VarChar", "A"));
                //transcmd_msdb.Add(sql);

                sql = "insert into WB_AIRHIS (AGEN_NO, FBNO, SEQ, EXTYPE, STATUS, FLAG, update_IP, TXTDAY, XSIZE, NAMEC) values ( ";
                sql += " 'admin', ";    // AGEN_NO
                sql += " '1', ";        // FBNO
                //sql += " '1', ";        // MMCODE
                sql += " '1', ";        // SEQ  
                sql += " 'GO', ";       // ** EXTYPE
                //sql += " '010000', ";   // DEPT
                sql += " 'B', ";        // STATUS
                sql += " 'A', ";        // FLAG
                sql += " '192.20.2.243', ";    // update_IP
                //sql += " convert(datetime, '2019-05-16 00:00:01', 120), ";   // INPUT_DATE
                sql += " convert(datetime, '" + sDtTodayMsSql + " 00:00:01', 120), ";    // TXTDAY
                //sql += " '1', ";                                              // AIR, 
                //sql += " convert(datetime, '2019-05-16 00:00:01', 120), ";    //CHK_DATE
                //sql += " convert(datetime, '2019-05-16 00:00:01', 120), ";    // EXP_DATE 
                //sql += " '1', ";                                              // MAT 
                sql += " '1', ";                                              // XSIZE
                sql += " '品名GO_1', " + sBr; // 15.品名(VARCHAR2,200)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) ";
                sql = sql.Replace(sBr, "");
                transcmd_msdb.Add(sql);

                sql = "insert into WB_AIRHIS (AGEN_NO, FBNO, SEQ, EXTYPE, STATUS, FLAG, update_IP, TXTDAY, XSIZE, NAMEC) values ( ";
                sql += " 'admin', ";    // AGEN_NO
                sql += " '2', ";        // FBNO
                //sql += " '2', ";        // MMCODE
                sql += " '2', ";        // SEQ  
                sql += " 'GI', ";       // ** EXTYPE
                //sql += " '010000', ";   // DEPT
                sql += " 'B', ";        // STATUS
                sql += " 'A', ";        // FLAG
                sql += " '192.20.2.243', ";    // update_IP
                //sql += " convert(datetime, '2019-05-16 00:00:02', 120), ";    // INPUT_DATE
                sql += " convert(datetime, '" + sDtTodayMsSql + " 00:00:02', 120), ";     // TXTDAY
                //sql += " '2', ";                                              // AIR, 
                //sql += " convert(datetime, '2019-05-16 00:00:02', 120), ";    //CHK_DATE
                //sql += " convert(datetime, '2019-05-16 00:00:02', 120), ";    // EXP_DATE 
                //sql += " '2', ";                                              // MAT 
                sql += " '2', ";                                              // XSIZE
                sql += " '品名GI_2', " + sBr; // 15.品名(VARCHAR2,200)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) ";
                sql = sql.Replace(sBr, "");
                transcmd_msdb.Add(sql);

                //sql = "insert into WB_AIRHIS (AGEN_NO, FBNO, SEQ, EXTYPE, STATUS, FLAG, update_IP, TXTDAY, XSIZE, NAMEC) values ( ";
                //sql += " 'admin', ";    // AGEN_NO
                //sql += " '3', ";        // FBNO
                ////sql += " '3', ";        // MMCODE
                //sql += " '3', ";        // SEQ  
                //sql += " 'CH', ";       // ** EXTYPE
                ////sql += " '444444', ";   // DEPT
                //sql += " 'B', ";        // STATUS
                //sql += " 'A', ";        // FLAG
                //sql += " '192.20.2.243', ";    // update_IP
                ////sql += " convert(datetime, '2019-05-16 00:00:03', 120), ";   // INPUT_DATE
                //sql += " convert(datetime, '" + sDtTodayMsSql + " 00:00:03', 120), ";    // TXTDAY
                ////sql += " '4', ";                                              // AIR, 
                ////sql += " convert(datetime, '2019-05-16 00:00:03', 120), ";    //CHK_DATE
                ////sql += " convert(datetime, '2019-05-16 00:00:03', 120), ";    // EXP_DATE 
                ////sql += " '4', ";                                              // MAT 
                //sql += " '4', ";                                              // XSIZE
                //sql += " '品名CH_3', " + sBr; // 15.品名(VARCHAR2,200)
                //sql += " ) ";
                //sql = sql.Replace(sBr, "");
                //transcmd_msdb.Add(sql);


                int rowsAffected_msdb = -1;
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
                callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP1-單元測試 失敗:" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                transaction_msdb.Rollback();
                conn_msdb.Close();
            }
        } // 

        #endregion


        public Form1()
        {
            InitializeComponent();
        }

        L l = new L("BHS001");
        FL fl = new FL("BHS001");

        //程式起始 BHS002-寄售廠商寄放量資料轉檔 排程每天 0:30~24:00每30分鐘執行一次
        private void Form1_Load(object sender, System.EventArgs e)
        {
            l.lg("Form1_Load()", "");
            try
            {
                l.ldb(get資料庫現況("資料庫現況"));
                l.lg("Form1_Load()", "get資料庫現況()");

                //單元測試();
                //l.ldb(get資料庫現況("單元測試"));
                //l.lg("Form1_Load()", "get單元測試()");


                // 1 01.外網 MSdb WB_AIRHIS -> 院內 Oracle PH_AIRHIS, 02.update WB_AIRHIS.STATUS='B'已處理
                SelectMS_WBputD_IntoOracle_PHputD();
                l.ldb(get資料庫現況("02.WB_AIRHIS->PH_AIRHIS"));
                l.lg("Form1_Load()", "02.getSelectMS_WBputD_IntoOracle_PHputD()");

                // 2 01.更新院內 Oracle PH_AIRHIS -> PH_AIRST(GO取走,GI換入,CH修改)
                UpdateOracle_PHputM();
                l.ldb(get資料庫現況("03.PH_AIRHIS->PH_AIRST"));
                l.lg("Form1_Load()", "03.UpdateOracle_PHputM()");

                // 3 01.更新PH_AIRTIME.update_time=sysdate, 02.刪除WB_AIRST, 03.複製PH_AIRST->WB_AIRST, 04 更新更新WB_AIRTIME.update_time=SYSDATETIME()
                Update_PH_WB_PutTime();
                l.ldb(get資料庫現況("04.刪WB_AIRST後，copy PH_AIRST->WB_AIRST"));
                l.lg("Form1_Load()", "04.Update_PH_WB_PutTime()");

                // 寄送mail通知admin有執行程式 
                fl.sendMailToAdmin("三總排程-BHS001執行完畢", "程式正常執行完畢", l.get資料庫現況Log檔路徑());
            }
            catch (Exception ex)
            {
                // 寄送mail通知admin有執行程式 
                fl.sendMailToAdmin("三總排程-BHS001執行異常", "Exception=" + ex.Message, l.get資料庫現況Log檔路徑());
                l.le("Form1_Load()", ex.Message);
            }
            this.Close();
        }

        String sBr = "\r\n";
        // 1_將外網 MSdb WB_PUT_D 異動資料複製到院內 Oracle PH_PUT_D
        private void SelectMS_WBputD_IntoOracle_PHputD()
        {
            CallDBtools calldbtools = new CallDBtools();
            String sql = "";
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
                sqlStr_MSDB += " FBNO, " + sBr; // 01.瓶號(VARCHAR2,20)
                //sqlStr_MSDB += " MMCODE, " + sBr; // 02.三總院內碼(VARCHAR2,13)
                sqlStr_MSDB += " SEQ, " + sBr; // 03.交易流水號(INTEGER,)
                sqlStr_MSDB += " convert(nvarchar(19), TXTDAY, 120) TXTDAY , " + sBr; // 04.更換日期(DATE,)
                sqlStr_MSDB += " AGEN_NO, " + sBr; // 05.廠商碼(VARCHAR2,6)
                //sqlStr_MSDB += " AIR, " + sBr; // 06.氣體(VARCHAR2,100)
                //sqlStr_MSDB += " convert(nvarchar(19), CHK_DATE, 120) CHK_DATE , " + sBr; // 07.檢驗日期(DATE,)
                //sqlStr_MSDB += " DEPT, " + sBr; // 08.放置地點(VARCHAR2,6)
                //sqlStr_MSDB += " convert(nvarchar(19), EXP_DATE, 120) EXP_DATE , " + sBr; // 09.保存期限(DATE,)
                sqlStr_MSDB += " EXTYPE, " + sBr; // 10.更換類別(VARCHAR2,2)
                //sqlStr_MSDB += " convert(nvarchar(19), INPUT_DATE, 120) INPUT_DATE , " + sBr; // 11.灌氣日期(DATE,)
                //sqlStr_MSDB += " MAT, " + sBr; // 12.材質類別(VARCHAR2,20)
                //sqlStr_MSDB += " MEMO, " + sBr; // 13.備註(VARCHAR2,50)
                //sqlStr_MSDB += " SBNO, " + sBr; // 14.鋼號(VARCHAR2,20)
                sqlStr_MSDB += " STATUS, " + sBr; // 15.狀態(VARCHAR2,1)
                sqlStr_MSDB += " XSIZE, " + sBr; // 16.容量(VARCHAR2,50)
                sqlStr_MSDB += " convert(nvarchar(19), CREATE_TIME, 120) CREATE_TIME , " + sBr; // 17.建立日期(DATE,)
                sqlStr_MSDB += " CREATE_USER, " + sBr; // 18.建立人員(VARCHAR2,10)
                sqlStr_MSDB += " UPDATE_IP, " + sBr; // 19.異動IP(VARCHAR2,20)
                sqlStr_MSDB += " convert(nvarchar(19), UPDATE_TIME, 120) UPDATE_TIME , " + sBr; // 20.異動日期(DATE,)
                sqlStr_MSDB += " UPDATE_USER, " + sBr; // 21.異動人員(VARCHAR2,10)
                sqlStr_MSDB += " FLAG, " + sBr; // 22.轉檔旗標(VARCHAR2,1) 
                sqlStr_MSDB += " NAMEC, " + sBr; // 23.品名(VARCHAR2,200)
                sqlStr_MSDB = sqlStr_MSDB.Substring(0, sqlStr_MSDB.Length - 4);
                sqlStr_MSDB += " from WB_AIRHIS where 1=1 ";
                sqlStr_MSDB += " and STATUS = 'B' "; // (A-未處理, B-已轉檔)
                sqlStr_MSDB += " and FLAG = 'A' "; // 22.轉檔旗標(VARCHAR2,1) (A-未處理, B-已轉檔)
                dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
                if (msg_MSDB == "") //取得 WB_PUT_D 無錯誤
                {
                    l.lg("SelectMS_WBputD_IntoOracle_PHputD()", "dt_MSDB.Rows.Count=" + dt_MSDB.Rows.Count);
                    if (dt_MSDB.Rows.Count > 0) //WB_PUT_D 有資料
                    {
                        int rowsAffected_oracle = -1;
                        int rowsAffected_msdb = -1;

                        for (int i = 0; i < dt_MSDB.Rows.Count; i++)
                        {

                            //string msg_oracle = "error";
                            //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                            //List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                            string cmdstr = "insert into PH_AIRHIS( ";
                            cmdstr += " FBNO, " + sBr; // 01.瓶號(VARCHAR2,20)
                            //cmdstr += " MMCODE, " + sBr; // 02.三總院內碼(VARCHAR2,13)
                            cmdstr += " SEQ, " + sBr; // 03.交易流水號(INTEGER,)
                            cmdstr += " TXTDAY, " + sBr; // 04.更換日期(DATE,)
                            cmdstr += " AGEN_NO, " + sBr; // 05.廠商碼(VARCHAR2,6)
                            //cmdstr += " AIR, " + sBr; // 06.氣體(VARCHAR2,100)
                            //cmdstr += " CHK_DATE, " + sBr; // 07.檢驗日期(DATE,)
                            //cmdstr += " DEPT, " + sBr; // 08.放置地點(VARCHAR2,6)
                            //cmdstr += " EXP_DATE, " + sBr; // 09.保存期限(DATE,)
                            cmdstr += " EXTYPE, " + sBr; // 10.更換類別(VARCHAR2,2)
                            //cmdstr += " INPUT_DATE, " + sBr; // 11.灌氣日期(DATE,)
                            //cmdstr += " MAT, " + sBr; // 12.材質類別(VARCHAR2,20)
                            //cmdstr += " MEMO, " + sBr; // 13.備註(VARCHAR2,50)
                            //cmdstr += " SBNO, " + sBr; // 14.鋼號(VARCHAR2,20)
                            cmdstr += " STATUS, " + sBr; // 15.狀態(VARCHAR2,1)
                            cmdstr += " XSIZE, " + sBr; // 16.容量(VARCHAR2,50)
                            cmdstr += " CREATE_TIME, " + sBr; // 17.建立日期(DATE,)
                            cmdstr += " CREATE_USER, " + sBr; // 18.建立人員(VARCHAR2,10)
                            cmdstr += " UPDATE_IP, " + sBr; // 19.異動IP(VARCHAR2,20)
                            cmdstr += " UPDATE_TIME, " + sBr; // 20.異動日期(DATE,)
                            cmdstr += " UPDATE_USER, " + sBr; // 21.異動人員(VARCHAR2,10)
                            cmdstr += " FLAG, " + sBr; // 22.FLAG轉檔旗標
                            cmdstr += " NAMEC, " + sBr; // 23.品名(VARCHAR2,200)
                            cmdstr = cmdstr.Substring(0, cmdstr.Length - 4);
                            cmdstr += ") values( " + sBr;
                            cmdstr += " :fbno, " + sBr; // 01.瓶號(VARCHAR2,Y)
                            //cmdstr += " :mmcode, " + sBr; // 02.三總院內碼(VARCHAR2,Y)
                            cmdstr += " :seq, " + sBr; // 03.交易流水號(INTEGER,Y)
                            cmdstr += " to_date(:txtday, 'yyyy-mm-dd hh24:mi:ss'), " + sBr; // 04.更換日期(DATE,Y)
                            cmdstr += " :agen_no, " + sBr; // 05.廠商碼(VARCHAR2,Y)
                            //cmdstr += " :air, " + sBr; // 06.氣體(VARCHAR2,Y)
                            //cmdstr += " to_date(:chk_date, 'yyyy-mm-dd hh24:mi:ss'), " + sBr; // 07.檢驗日期(DATE,Y)
                            //cmdstr += " :dept, " + sBr; // 08.放置地點(VARCHAR2,Y)
                            //cmdstr += " to_date(:exp_date, 'yyyy-mm-dd hh24:mi:ss'), " + sBr; // 09.保存期限(DATE,Y)
                            cmdstr += " :extype, " + sBr; // 10.更換類別(VARCHAR2,Y)
                            //cmdstr += " to_date(:input_date, 'yyyy-mm-dd hh24:mi:ss'), " + sBr; // 11.灌氣日期(DATE,Y)
                            //cmdstr += " :mat, " + sBr; // 12.材質類別(VARCHAR2,Y)
                            //cmdstr += " :memo, " + sBr; // 13.備註(VARCHAR2,)
                            //cmdstr += " :sbno, " + sBr; // 14.鋼號(VARCHAR2,)
                            cmdstr += " :status, " + sBr; // 15.狀態(VARCHAR2,Y)
                            cmdstr += " :xsize, " + sBr; // 16.容量(VARCHAR2,Y)
                            cmdstr += " to_date(:create_time, 'yyyy-mm-dd hh24:mi:ss'), " + sBr; // 17.建立日期(DATE,)
                            cmdstr += " :create_user, " + sBr; // 18.建立人員(VARCHAR2,)
                            cmdstr += " :update_ip, " + sBr; // 19.異動IP(VARCHAR2,Y)
                            cmdstr += " sysdate, " + sBr; // 20.異動日期(DATE,Y)
                            cmdstr += " 'AUTO', " + sBr; // 21.異動人員(VARCHAR2,Y)
                            cmdstr += " 'A', " + sBr; // 22.FLAG轉檔旗標(A-未處理, B-已轉檔)
                            cmdstr += " :namec, " + sBr; // 23.品名(VARCHAR2,200)
                            cmdstr = cmdstr.Substring(0, cmdstr.Length - 4);
                            cmdstr += ") ";
                            transcmd_oracle.Add(cmdstr);

                            ++iTransOra;
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_MSDB.Rows[i]["FBNO"].ToString().Trim())); // 01.瓶號(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_MSDB.Rows[i]["MMCODE"].ToString().Trim())); // 02.三總院內碼(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dt_MSDB.Rows[i]["SEQ"].ToString().Trim())); // 03.交易流水號(INTEGER,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":txtday", "VarChar", dt_MSDB.Rows[i]["TXTDAY"].ToString().Trim())); // 04.更換日期(DATE,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_MSDB.Rows[i]["AGEN_NO"].ToString().Trim())); // 05.廠商碼(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":air", "VarChar", dt_MSDB.Rows[i]["AIR"].ToString().Trim())); // 06.氣體(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":chk_date", "VarChar", dt_MSDB.Rows[i]["CHK_DATE"].ToString().Trim())); // 07.檢驗日期(DATE,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dept", "VarChar", dt_MSDB.Rows[i]["DEPT"].ToString().Trim())); // 08.放置地點(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":exp_date", "VarChar", dt_MSDB.Rows[i]["EXP_DATE"].ToString().Trim())); // 09.保存期限(DATE,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":extype", "VarChar", dt_MSDB.Rows[i]["EXTYPE"].ToString().Trim())); // 10.更換類別(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":input_date", "VarChar", dt_MSDB.Rows[i]["INPUT_DATE"].ToString().Trim())); // 11.灌氣日期(DATE,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mat", "VarChar", dt_MSDB.Rows[i]["MAT"].ToString().Trim())); // 12.材質類別(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":memo", "VarChar", dt_MSDB.Rows[i]["MEMO"].ToString().Trim())); // 13.備註(VARCHAR2,)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":sbno", "VarChar", dt_MSDB.Rows[i]["SBNO"].ToString().Trim())); // 14.鋼號(VARCHAR2,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":status", "VarChar", "B")); // 15.狀態(VARCHAR2,Y) (A-未處理, B-已轉檔)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":xsize", "VarChar", dt_MSDB.Rows[i]["XSIZE"].ToString().Trim())); // 16.容量(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":create_time", "VarChar", dt_MSDB.Rows[i]["CREATE_TIME"].ToString().Trim())); // 17.建立日期(DATE,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":create_user", "VarChar", dt_MSDB.Rows[i]["CREATE_USER"].ToString().Trim())); // 18.建立人員(VARCHAR2,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":update_ip", "VarChar", dt_MSDB.Rows[i]["UPDATE_IP"].ToString().Trim())); // 19.異動IP(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":namec", "VarChar", dt_MSDB.Rows[i]["NAMEC"].ToString().Trim())); // 23.品名(VARCHAR2,200)
                            // rowsAffected_oracle = callDbtools_oralce.CallExecSQL(cmdstr, listParam_oracle, "oracle", ref msg_oracle);

                            //if (msg_oracle == "") //insert PH_PUT_D 無錯誤
                            //{
                            //    if (rowsAffected_oracle == 1) //insert PH_PUT_D 正常回傳筆數1
                            //    {

                            msg_MSDB = "error";
                            //List<CallDBtools_MSDb.MSDbParam> paraListA = new List<CallDBtools_MSDb.MSDbParam>();
                            sqlStr_MSDB = "update WB_AIRHIS set FLAG = 'B', UPDATE_TIME = SYSDATETIME(), UPDATE_USER = 'AUTO' where 1=1 " + sBr;
                            sqlStr_MSDB += " and FBNO = @fbno " + sBr;// 01.瓶號(VARCHAR2,Y)
                            //sqlStr_MSDB += " and MMCODE = @mmcode " + sBr;// 02.三總院內碼(VARCHAR2,Y)
                            sqlStr_MSDB += " and SEQ = @seq " + sBr;// 03.交易流水號(INTEGER,Y)
                            sqlStr_MSDB += " and convert(nvarchar(19),TXTDAY, 120) = @txtday " + sBr;// 04.更換日期(DATE,Y)
                            sqlStr_MSDB += " and AGEN_NO = @agen_no " + sBr;// 05.廠商碼(VARCHAR2,Y)
                            transcmd_msdb.Add(sqlStr_MSDB);

                            ++iTransMsdb;
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@fbno", "nvarchar", dt_MSDB.Rows[i]["FBNO"].ToString().Trim()));
                            //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@mmcode", "nvarchar", dt_MSDB.Rows[i]["MMCODE"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@seq", "int", dt_MSDB.Rows[i]["SEQ"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@txtday", "datetime", Convert.ToDateTime(dt_MSDB.Rows[i]["TXTDAY"]).ToString("yyyy-MM-dd HH:mm:ss").Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@agen_no", "nvarchar", dt_MSDB.Rows[i]["AGEN_NO"].ToString().Trim()));

                            // rowsAffected_msdb = callDBtools_msdb.CallExecSQL(sqlStr_MSDB, listParam_msdb, "msdb", ref msg_MSDB);

                            //if (msg_MSDB != "")
                            //    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP1-update WB_AIRHIS失敗:" + msg_MSDB, "AUTO");
                            //    }
                            //}
                            //else //insert PH_PUT_D 有錯誤
                            //{
                            //    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP1-insert PH_AIRHIS失敗:" + msg_oracle, "AUTO");
                            //}
                        }

                        rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                                transcmd_oracle,
                                listParam_oracle,
                                transaction_oracle,
                                conn_oracle);

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
                }
                else //取得 WB_PUT_D 有錯誤，寫ERROR_LOG
                {
                    CallDBtools_Oracle callDBtools_oracle = new CallDBtools_Oracle();
                    callDBtools_oracle.I_ERROR_LOG("BHS001", "STEP1-取得WB_AIRHIS失敗:" + msg_MSDB, "AUTO");
                }



            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP1 失敗:" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                transaction_msdb.Rollback();
                conn_msdb.Close();

                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS001", "程式錯誤:1_");
            }
        }

        // 2_更新院內 Oracle PH_PUT_M
        private void UpdateOracle_PHputM()
        {
            CallDBtools calldbtools = new CallDBtools();
            String sql = "";
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
                DataTable dt_oralce = new DataTable();
                string sql_oracle = " select ";
                sql_oracle += " FBNO, " + sBr; 
                //sql_oracle += " MMCODE, " + sBr;
                sql_oracle += " SEQ, " + sBr;
                sql_oracle += " to_char(TXTDAY,'yyyy/mm/dd hh24:mi:ss') TXTDAY, " + sBr;
                sql_oracle += " AGEN_NO, " + sBr;
                //sql_oracle += " AIR, " + sBr;
                //sql_oracle += " to_char(CHK_DATE,'yyyy/mm/dd hh24:mi:ss') CHK_DATE, " + sBr;
                //sql_oracle += " DEPT, " + sBr;
                //sql_oracle += " to_char(EXP_DATE,'yyyy/mm/dd hh24:mi:ss') EXP_DATE, " + sBr;
                sql_oracle += " EXTYPE, " + sBr;
                //sql_oracle += " to_char(INPUT_DATE,'yyyy/mm/dd hh24:mi:ss') INPUT_DATE, " + sBr;
                //sql_oracle += " MAT, " + sBr;
                //sql_oracle += " MEMO, " + sBr;
                //sql_oracle += " SBNO, " + sBr;
                sql_oracle += " STATUS, " + sBr;
                sql_oracle += " XSIZE, " + sBr;
                sql_oracle += " to_char(CREATE_TIME,'yyyy/mm/dd hh24:mi:ss') CREATE_TIME, " + sBr;
                sql_oracle += " CREATE_USER, " + sBr;
                sql_oracle += " UPDATE_IP, " + sBr;
                sql_oracle += " to_char(UPDATE_TIME,'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME, " + sBr;
                sql_oracle += " UPDATE_USER, " + sBr;
                sql_oracle += " FLAG, " + sBr;
                sql_oracle += " NAMEC, " + sBr; // 23.品名(VARCHAR2,200)
                sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
                sql_oracle += " from PH_AIRHIS where 1=1 " + sBr;
                sql_oracle += " and FLAG= 'A' " + sBr;
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "TXTDAY", null, "oracle", "T1", ref msg_oracle);
                l.lg("UpdateOracle_PHputM()", "dt_oralce.Rows.Count=" + dt_oralce.Rows.Count);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    int rowsAffected_oracle = -1;
                    for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    {
                        if (dt_oralce.Rows[i]["EXTYPE"].ToString().Trim() == "GO") // 取走
                        {
                            sql_oracle = " delete from PH_AIRST where 1=1 " + sBr;
                            sql_oracle += " and AGEN_NO = :agen_no " + sBr; // 01.廠商碼(VARCHAR2,Y)
                            sql_oracle += " and FBNO = :fbno " + sBr;       // 02.瓶號(VARCHAR2,Y)
                            //sql_oracle += " and MMCODE = :mmcode " + sBr; // 03.三總院內碼(VARCHAR2,Y)
                            //sql_oracle += " and SEQ = :seq " + sBr;         // 04.交易流水號(  INTEGER)
                            //sql_oracle += " and to_char(TXTDAY,'yyyy/mm/dd hh24:mi:ss') = :txtday " + sBr;   // 05.更換日期(  DATE)
                            transcmd_oracle.Add(sql_oracle);

                            ++iTransOra;
                            //List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));// 01.廠商碼(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));// 02.瓶號(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));// 03.三總院內碼(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));// 04.交易流水號(  INTEGER)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":txtday", "VarChar", dt_oralce.Rows[i]["TXTDAY"].ToString().Trim()));// 05.更換日期(  DATE)x
                            //rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
                            //if (msg_oracle == "")
                            //{
                            //rowsAffected_oracle = -1;
                            sql_oracle = " update PH_AIRHIS set FLAG='B', UPDATE_TIME=sysdate, UPDATE_USER='AUTO' where 1=1 " + sBr;
                            sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商碼(VARCHAR2,Y)
                            sql_oracle += " and FBNO = :fbno " + sBr;// 02.瓶號(VARCHAR2,Y)
                            //sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.三總院內碼(VARCHAR2,Y)
                            sql_oracle += " and SEQ = :seq " + sBr;// 04.交易流水號(INTEGER,Y)
                            sql_oracle += " and to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') = :txtday " + sBr;// 05.更換日期(DATE,Y)
                            transcmd_oracle.Add(sql_oracle);
                            //paraList = new List<CallDBtools_Oracle.OracleParam>();
                            ++iTransOra;
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));// 01.廠商碼(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));// 02.瓶號(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));// 03.三總院內碼(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));// 04.交易流水號(INTEGER,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":txtday", "VarChar", (dt_oralce.Rows[i]["TXTDAY"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 05.更換日期(DATE,Y)
                            //    rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
                            //    if (msg_oracle != "")
                            //    {
                            //        callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP2-update PH_AIRHIS (EXTYPE=GO 取走) 失敗:" + msg_oracle, "AUTO");
                            //    }
                            //}
                            //else
                            //{
                            //    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP2-delete PH_AIRST (EXTYPE=GO 取走) 失敗:" + msg_oracle, "AUTO");
                            //}
                        }
                        else if (dt_oralce.Rows[i]["EXTYPE"].ToString().Trim() == "GI") // 換入
                        {
                            sql_oracle = " insert into PH_AIRST ( " + sBr;
                            sql_oracle += " AGEN_NO, " + sBr; // 01.廠商碼(VARCHAR2,6)
                            sql_oracle += " FBNO, " + sBr; // 02.瓶號(VARCHAR2,20)
                            //sql_oracle += " MMCODE, " + sBr; // 03.三總院內碼(VARCHAR2,13)
                            sql_oracle += " SEQ, " + sBr; // 04.交易流水號(INTEGER,)
                            sql_oracle += " TXTDAY, " + sBr; // 05.更換日期(DATE,)
                            //sql_oracle += " AIR, " + sBr; // 06.氣體(VARCHAR2,100)
                            //sql_oracle += " CHK_DATE, " + sBr; // 07.檢驗日期(DATE,)
                            //sql_oracle += " DEPT, " + sBr; // 08.放置位置(VARCHAR2,6)
                            //sql_oracle += " EXP_DATE, " + sBr; // 09.保存期限(DATE,)
                            //sql_oracle += " INPUT_DATE, " + sBr; // 10.灌氣日期(DATE,)
                            //sql_oracle += " MAT, " + sBr; // 11.材質類別(VARCHAR2,20)
                            //sql_oracle += " MEMO, " + sBr; // 12.備註(VARCHAR2,50)
                            //sql_oracle += " SBNO, " + sBr; // 13.鋼號(VARCHAR2,20)
                            sql_oracle += " XSIZE, " + sBr; // 14.鋼瓶尺寸(VARCHAR2,50)
                            sql_oracle += " NAMEC, " + sBr; // 15.品名(VARCHAR2,200)
                            sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
                            sql_oracle += " ) values ( " + sBr;
                            sql_oracle += " :agen_no, " + sBr; // 01.廠商碼(VARCHAR2,Y)
                            sql_oracle += " :fbno, " + sBr; // 02.瓶號(VARCHAR2,Y)
                            //sql_oracle += " :mmcode, " + sBr; // 03.三總院內碼(VARCHAR2,Y)
                            sql_oracle += " :seq, " + sBr; // 04.交易流水號(INTEGER,Y)
                            sql_oracle += " to_date(:txtday, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 05.更換日期(DATE,Y)
                            //sql_oracle += " :air, " + sBr; // 06.氣體(VARCHAR2,Y)
                            //sql_oracle += " to_date(:chk_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 07.檢驗日期(DATE,Y)
                            //sql_oracle += " :dept, " + sBr; // 08.放置位置(VARCHAR2,)
                            //sql_oracle += " to_date(:exp_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 09.保存期限(DATE,Y)
                            //sql_oracle += " to_date(:input_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 10.灌氣日期(DATE,Y)
                            //sql_oracle += " :mat, " + sBr; // 11.材質類別(VARCHAR2,Y)
                            //sql_oracle += " :memo, " + sBr; // 12.備註(VARCHAR2,)
                            //sql_oracle += " :sbno, " + sBr; // 13.鋼號(VARCHAR2,)
                            sql_oracle += " :xsize, " + sBr; // 14.鋼瓶尺寸(VARCHAR2,Y)
                            sql_oracle += " :namec, " + sBr; // 15.品名(VARCHAR2,200)
                            sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
                            sql_oracle += " ) " + sBr;
                            transcmd_oracle.Add(sql_oracle);

                            //List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                            //int iTransSeq = 1;
                            ++iTransOra;
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim())); // 01.廠商碼(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim())); // 02.瓶號(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim())); // 03.三總院內碼(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim())); // 04.交易流水號(INTEGER,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":txtday", "VarChar", (dt_oralce.Rows[i]["TXTDAY"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 05.更換日期(DATE,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":air", "VarChar", dt_oralce.Rows[i]["AIR"].ToString().Trim())); // 06.氣體(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":chk_date", "VarChar", (dt_oralce.Rows[i]["CHK_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["CHK_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 07.檢驗日期(DATE,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim())); // 08.放置位置(VARCHAR2,)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":exp_date", "VarChar", (dt_oralce.Rows[i]["EXP_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["EXP_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 09.保存期限(DATE,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":input_date", "VarChar", (dt_oralce.Rows[i]["INPUT_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["INPUT_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 10.灌氣日期(DATE,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mat", "VarChar", dt_oralce.Rows[i]["MAT"].ToString().Trim())); // 11.材質類別(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":memo", "VarChar", dt_oralce.Rows[i]["MEMO"].ToString().Trim())); // 12.備註(VARCHAR2,)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":sbno", "VarChar", dt_oralce.Rows[i]["SBNO"].ToString().Trim())); // 13.鋼號(VARCHAR2,)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":xsize", "VarChar", dt_oralce.Rows[i]["XSIZE"].ToString().Trim())); // 14.鋼瓶尺寸(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":namec", "VarChar", dt_oralce.Rows[i]["NAMEC"].ToString().Trim())); // 15.品名(VARCHAR2,200)

                            //rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, listParam_oracle, "oracle", ref msg_oracle);
                            //if (msg_oracle == "")
                            //{
                            //rowsAffected_oracle = -1;
                            sql_oracle = " update PH_AIRHIS set FLAG='B', UPDATE_TIME=sysdate, UPDATE_USER='AUTO' where 1=1 " + sBr;
                            sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商碼(VARCHAR2,Y)
                            sql_oracle += " and FBNO = :fbno " + sBr;// 02.瓶號(VARCHAR2,Y)
                            //sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.三總院內碼(VARCHAR2,Y)
                            sql_oracle += " and SEQ = :seq " + sBr;// 04.交易流水號(INTEGER,Y)
                            sql_oracle += " and to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') = :txtday " + sBr;// 05.更換日期(DATE,Y)
                            transcmd_oracle.Add(sql_oracle);

                            //paraList = new List<CallDBtools_Oracle.OracleParam>();
                            ++iTransOra;
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));// 01.廠商碼(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));// 02.瓶號(VARCHAR2,Y)
                            //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));// 03.三總院內碼(VARCHAR2,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));// 04.交易流水號(INTEGER,Y)
                            listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":txtday", "VarChar", (dt_oralce.Rows[i]["TXTDAY"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 05.更換日期(DATE,Y)

                            //rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
                            //if (msg_oracle != "")
                            //{
                            //    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP2-update PH_AIRHIS (EXTYPE=GI 換入) 失敗:" + msg_oracle, "AUTO");
                            //}
                            //}
                            //else
                            //{
                            //    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP2-insert PH_AIRST (EXTYPE=GI 換入) 失敗:" + msg_oracle, "AUTO");
                            //}
                        }
                        //else if (
                        //    dt_oralce.Rows[i]["EXTYPE"].ToString().Trim() == "CH" ||
                        //    dt_oralce.Rows[i]["EXTYPE"].ToString().Trim() == "UP"
                        //) // 修改
                        //{
                        //    sql_oracle = " update PH_AIRST set " + sBr;
                        //    //sql_oracle += " AIR = :air, " + sBr;// 06.氣體(VARCHAR2,Y)
                        //    //sql_oracle += " CHK_DATE = to_date(:chk_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr;// 07.檢驗日期(DATE,Y)
                        //    //sql_oracle += " DEPT = :dept, " + sBr;// 08.放置位置(VARCHAR2,)
                        //    //sql_oracle += " EXP_DATE = to_date(:exp_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr;// 09.保存期限(DATE,Y)
                        //    //sql_oracle += " INPUT_DATE = to_date(:input_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr;// 10.灌氣日期(DATE,Y)
                        //    //sql_oracle += " MAT = :mat, " + sBr;// 11.材質類別(VARCHAR2,Y)
                        //    //sql_oracle += " MEMO = :memo, " + sBr;// 12.備註(VARCHAR2,)
                        //    //sql_oracle += " SBNO = :sbno, " + sBr;// 13.鋼號(VARCHAR2,)
                        //    sql_oracle += " XSIZE = :xsize, " + sBr;// 14.鋼瓶尺寸(VARCHAR2,Y)
                        //    sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
                        //    sql_oracle += " where 1=1 " + sBr;
                        //    sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 05.廠商碼(VARCHAR2,Y)
                        //    sql_oracle += " and FBNO = :fbno " + sBr;// 01.瓶號(VARCHAR2,Y)
                        //    //sql_oracle += " and MMCODE = :mmcode " + sBr;// 02.三總院內碼(VARCHAR2,Y)
                        //    transcmd_oracle.Add(sql_oracle);

                        //    //List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                        //    ++iTransOra;
                        //    //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":air", "VarChar", dt_oralce.Rows[i]["AIR"].ToString().Trim()));// 06.氣體(VARCHAR2,Y)
                        //    //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":chk_date", "VarChar", (dt_oralce.Rows[i]["CHK_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["CHK_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 07.檢驗日期(DATE,Y)
                        //    //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));// 08.放置位置(VARCHAR2,)
                        //    //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":exp_date", "VarChar", (dt_oralce.Rows[i]["EXP_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["EXP_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 09.保存期限(DATE,Y)
                        //    //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":input_date", "VarChar", (dt_oralce.Rows[i]["INPUT_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["INPUT_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 10.灌氣日期(DATE,Y)
                        //    //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mat", "VarChar", dt_oralce.Rows[i]["MAT"].ToString().Trim()));// 11.材質類別(VARCHAR2,Y)
                        //    //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":memo", "VarChar", dt_oralce.Rows[i]["MEMO"].ToString().Trim()));// 12.備註(VARCHAR2,)
                        //    //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":sbno", "VarChar", dt_oralce.Rows[i]["SBNO"].ToString().Trim()));// 13.鋼號(VARCHAR2,)
                        //    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":xsize", "VarChar", dt_oralce.Rows[i]["XSIZE"].ToString().Trim()));// 14.鋼瓶尺寸(VARCHAR2,Y)
                        //    // -- key
                        //    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));// 01.廠商碼(VARCHAR2,Y)
                        //    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));// 02.瓶號(VARCHAR2,Y)
                        //    //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));// 03.三總院內碼(VARCHAR2,Y)

                        //    // rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, listParam_oracle, "oracle", ref msg_oracle);
                        //    //if (msg_oracle == "")
                        //    //{
                        //    //rowsAffected_oracle = -1;
                        //    sql_oracle = " update PH_AIRHIS set FLAG='B', UPDATE_TIME=sysdate, UPDATE_USER='AUTO' where 1=1 ";
                        //    sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商碼(VARCHAR2,Y)
                        //    sql_oracle += " and FBNO = :fbno " + sBr;// 02.瓶號(VARCHAR2,Y)
                        //    //sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.三總院內碼(VARCHAR2,Y)
                        //    sql_oracle += " and SEQ = :seq " + sBr;// 04.交易流水號(INTEGER,Y)
                        //    sql_oracle += " and to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') = :txtday " + sBr;// 05.更換日期(DATE,Y)
                        //    transcmd_oracle.Add(sql_oracle);

                        //    ++iTransOra;
                        //    //listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();
                        //    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));// 01.廠商碼(VARCHAR2,Y)
                        //    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));// 02.瓶號(VARCHAR2,Y)
                        //    //listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));// 03.三總院內碼(VARCHAR2,Y)
                        //    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));// 04.交易流水號(INTEGER,Y)
                        //    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":txtday", "VarChar", (dt_oralce.Rows[i]["TXTDAY"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 05.更換日期(DATE,Y)
                        //    // rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, listParam_oracle, "oracle", ref msg_oracle);
                        //    //    if (msg_oracle != "")
                        //    //    {
                        //    //        callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP2-update PH_AIRHIS (EXTYPE=GH 修改) 失敗:" + msg_oracle, "AUTO");
                        //    //    }
                        //    //}
                        //    //else
                        //    //{
                        //    //    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP2-insert PH_AIRST (EXTYPE=GH 修改) 失敗:" + msg_oracle, "AUTO");
                        //    //}
                        //}
                    } // end of for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                            transcmd_oracle,
                            listParam_oracle,
                            transaction_oracle,
                            conn_oracle);

                    transaction_oracle.Commit();
                    conn_oracle.Close();
                    ErrorStep += ",成功" + Environment.NewLine;
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP2-取得PH_AIRHIS失敗:" + msg_oracle, "AUTO");
                }
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP2-程式錯誤:2_:" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();

                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS001", "程式錯誤:2_");

            }
        }




        private void Update_PH_WB_PutTime()
        {
            CallDBtools calldbtools = new CallDBtools();
            String sql = "";
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
            int iTranOra = 0;


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
            int iTranMsdb = 0;

            try
            {
                int rowsAffected_oracle = -1;
                string msg_oracle = "error", cmdstr = "", result = "";
                string msg_MSDB = "error";
                int rowsAffected_msdb = -1;
                //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                //CallDBtools_MSDb callDbtools_msdb = new CallDBtools_MSDb();


                cmdstr = "select count(*) from PH_AIRTIME ";
                result = callDbtools_oralce.CallExecScalar(cmdstr, null, "oracle", ref msg_oracle);
                l.lg("Update_PH_WB_PutTime()", "select count(*) cnts from PH_AIRTIME, result=" + result);
                if (result == "0") // 沒有資料，要 insert
                {
                    cmdstr = "insert into PH_AIRTIME(update_time) values(sysdate) ";
                    transcmd_oracle.Add(cmdstr);
                    ++iTranOra;
                    //rowsAffected_oracle = callDbtools_oralce.CallExecSQL(cmdstr, null, "oracle", ref msg_oracle);
                }
                else //有資料，要 update 
                {
                    cmdstr = "update PH_AIRTIME set update_time = sysdate ";
                    transcmd_oracle.Add(cmdstr);
                    ++iTranOra;
                    //rowsAffected_oracle = callDbtools_oralce.CallExecSQL(cmdstr, null, "oracle", ref msg_oracle);
                }
                //if (msg_oracle != "" || rowsAffected_oracle == 0)
                //{
                //    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP3-更新 PH_AIRTIME 失敗:" + msg_oracle, "AUTO");
                //}

                // 02 刪除外網WB_AIRST
                cmdstr = "delete from WB_AIRST ";
                transcmd_msdb.Add(cmdstr);
                ++iTranMsdb;
                //rowsAffected_msdb = callDbtools_msdb.CallExecSQL(cmdstr, null, "msdb", ref msg_MSDB);
                //if (msg_MSDB != "")
                //{
                //    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP3-truncate WB_AIRST 失敗:" + msg_oracle, "AUTO");
                //}

                // 03 copy PH_AIRST 到 WB_AIRST
                //string msg_oracle = "error";
                //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_oralce = new DataTable();
                string sql_oracle = " select ";
                sql_oracle += " AGEN_NO, " + sBr;
                sql_oracle += " FBNO, " + sBr;
                //sql_oracle += " MMCODE, " + sBr;
                sql_oracle += " SEQ, " + sBr;
                sql_oracle += " to_char(TXTDAY,'yyyy/mm/dd hh24:mi:ss') TXTDAY , " + sBr;
                //sql_oracle += " AIR, " + sBr;
                //sql_oracle += " to_char(CHK_DATE,'yyyy/mm/dd hh24:mi:ss') CHK_DATE , " + sBr;
                //sql_oracle += " DEPT, " + sBr;
                //sql_oracle += " to_char(EXP_DATE,'yyyy/mm/dd hh24:mi:ss') EXP_DATE , " + sBr;
                //sql_oracle += " to_char(INPUT_DATE,'yyyy/mm/dd hh24:mi:ss') INPUT_DATE , " + sBr;
                //sql_oracle += " MAT, " + sBr;
                //sql_oracle += " MEMO, " + sBr;
                //sql_oracle += " SBNO, " + sBr;
                sql_oracle += " XSIZE, " + sBr;
                sql_oracle += " NAMEC, " + sBr; // 15.品名(VARCHAR2,200)
                sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
                sql_oracle += " from PH_AIRST where 1=1 ";
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                l.lg("Update_PH_WB_PutTime()", "dt_oralce.Rows.Count=" + dt_oralce.Rows.Count);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    //CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                    string msgDB = "error", sql_msdb = "", ms_resStr = "";
                    //int rowsAffected = -1;
                    for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    {
                        // rowsAffected = -1;
                        sql_msdb = "insert into WB_AIRST ( " + sBr;
                        sql_msdb += " AGEN_NO, " + sBr; // 01.廠商碼(VARCHAR2,6)
                        sql_msdb += " FBNO, " + sBr; // 02.瓶號(VARCHAR2,20)
                        //sql_msdb += " MMCODE, " + sBr; // 03.三總院內碼(VARCHAR2,13)
                        sql_msdb += " SEQ, " + sBr; // 04.交易流水號(INTEGER,)
                        sql_msdb += " TXTDAY, " + sBr; // 05.更換日期(DATE,)
                        //sql_msdb += " AIR, " + sBr; // 06.氣體(VARCHAR2,100)
                        //sql_msdb += " CHK_DATE, " + sBr; // 07.檢驗日期(DATE,)
                        //sql_msdb += " DEPT, " + sBr; // 08.放置位置(VARCHAR2,6)
                        //sql_msdb += " EXP_DATE, " + sBr; // 09.保存期限(DATE,)
                        //sql_msdb += " INPUT_DATE, " + sBr; // 10.灌氣日期(DATE,)
                        //sql_msdb += " MAT, " + sBr; // 11.材質類別(VARCHAR2,20)
                        //sql_msdb += " MEMO, " + sBr; // 12.備註(VARCHAR2,50)
                        //sql_msdb += " SBNO, " + sBr; // 13.鋼號(VARCHAR2,20)
                        sql_msdb += " XSIZE, " + sBr; // 14.鋼瓶尺寸(VARCHAR2,50)
                        sql_msdb += " NAMEC, " + sBr; // 15.品名(VARCHAR2,200)
                        sql_msdb = sql_msdb.Substring(0, sql_msdb.Length - 4);
                        sql_msdb += " ) values ( " + sBr;
                        sql_msdb += " @agen_no, " + sBr; // 01.廠商碼(VARCHAR2,6)
                        sql_msdb += " @fbno, " + sBr; // 02.瓶號(VARCHAR2,20)
                        //sql_msdb += " @mmcode, " + sBr; // 03.三總院內碼(VARCHAR2,13)
                        sql_msdb += " @seq, " + sBr; // 04.交易流水號(INTEGER,)
                        sql_msdb += " @txtday, " + sBr; // 05.更換日期(DATE,)
                        //sql_msdb += " @air, " + sBr; // 06.氣體(VARCHAR2,100)
                        //sql_msdb += " @chk_date, " + sBr; // 07.檢驗日期(DATE,)
                        //sql_msdb += " @dept, " + sBr; // 08.放置位置(VARCHAR2,6)
                        //sql_msdb += " @exp_date, " + sBr; // 09.保存期限(DATE,)
                        //sql_msdb += " @input_date, " + sBr; // 10.灌氣日期(DATE,)
                        //sql_msdb += " @mat, " + sBr; // 11.材質類別(VARCHAR2,20)
                        //sql_msdb += " @memo, " + sBr; // 12.備註(VARCHAR2,50)
                        //sql_msdb += " @sbno, " + sBr; // 13.鋼號(VARCHAR2,20)
                        sql_msdb += " @xsize, " + sBr; // 14.鋼瓶尺寸(VARCHAR2,50)
                        sql_msdb += " @namec, " + sBr; // 15.品名(VARCHAR2,200)
                        sql_msdb = sql_msdb.Substring(0, sql_msdb.Length - 4);
                        sql_msdb += " )" + sBr;
                        sql_msdb.Replace(sBr, "");
                        transcmd_msdb.Add(sql_msdb);

                        //List<CallDBtools_MSDb.MSDbParam> paraListA = new List<CallDBtools_MSDb.MSDbParam>();
                        ++iTranMsdb;
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));
                        //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));
                        String txtday = "";
                        if (dt_oralce.Rows[i]["TXTDAY"].ToString().Length > 0)
                            txtday = Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"]).ToString("yyyy-MM-dd HH:mm:ss").Trim();
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@txtday", "VarChar", txtday));
                        //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@air", "VarChar", dt_oralce.Rows[i]["AIR"].ToString().Trim()));
                        //String chk_date = "";
                        //if (dt_oralce.Rows[i]["CHK_DATE"].ToString().Length > 0)
                        //    chk_date = Convert.ToDateTime(dt_oralce.Rows[i]["CHK_DATE"]).ToString("yyyy-MM-dd HH:mm:ss").Trim();
                        //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@chk_date", "VarChar", chk_date));
                        //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));
                        //String exp_date = "";
                        //if (dt_oralce.Rows[i]["EXP_DATE"].ToString().Length > 0)
                        //    exp_date = Convert.ToDateTime(dt_oralce.Rows[i]["EXP_DATE"]).ToString("yyyy-MM-dd HH:mm:ss").Trim();
                        //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@exp_date", "VarChar", exp_date));
                        //String input_date = "";
                        //if (dt_oralce.Rows[i]["INPUT_DATE"].ToString().Length > 0)
                        //    input_date = Convert.ToDateTime(dt_oralce.Rows[i]["INPUT_DATE"]).ToString("yyyy-MM-dd HH:mm:ss").Trim();
                        //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@input_date", "VarChar", input_date));
                        //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@mat", "VarChar", dt_oralce.Rows[i]["MAT"].ToString().Trim()));
                        //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@memo", "VarChar", dt_oralce.Rows[i]["MEMO"].ToString().Trim()));
                        //listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@sbno", "VarChar", dt_oralce.Rows[i]["SBNO"].ToString().Trim()));
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@xsize", "VarChar", dt_oralce.Rows[i]["XSIZE"].ToString().Trim()));
                        listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@namec", "VarChar", dt_oralce.Rows[i]["NAMEC"].ToString().Trim())); // 15.品名(VARCHAR2,200)

                        //rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, listParam_msdb, "msdb", ref msgDB);
                        //if (msgDB != "")
                        //{
                        //    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP3-insert WB_AIRST 失敗:" + msgDB, "AUTO");
                        //}
                    }
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP3-取得PH_AIRST失敗:" + msg_oracle, "AUTO");
                }


                // 04 
                cmdstr = "select count(*) from WB_AIRTIME ";
                result = callDbtools_msdb.CallExecScalar(cmdstr, null, "msdb", ref msg_MSDB);
                l.lg("Update_PH_WB_PutTime()", "select count(*) from WB_AIRTIME, result=" + result);
                if (result == "0") // 沒有資料，要 insert
                {
                    cmdstr = "insert into WB_AIRTIME(update_time) values(SYSDATETIME()) ";
                    transcmd_msdb.Add(cmdstr);
                    ++iTranMsdb;
                    //rowsAffected_msdb = callDbtools_msdb.CallExecSQL(cmdstr, null, "msdb", ref msg_MSDB);
                }
                else //有資料，要 update 
                {
                    cmdstr = "update WB_AIRTIME set update_time = SYSDATETIME() ";
                    transcmd_msdb.Add(cmdstr);
                    ++iTranMsdb;
                    //rowsAffected_msdb = callDbtools_msdb.CallExecSQL(cmdstr, null, "msdb", ref msg_MSDB);
                }
                //if (msg_MSDB != "" || rowsAffected_msdb == 0)
                //{
                //    callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP3-更新 WB_AIRTIME 失敗:" + msg_oracle, "AUTO");
                //}

                rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                    transcmd_oracle,
                    listParam_oracle,
                    transaction_oracle,
                    conn_oracle);

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
                callDbtools_oralce.I_ERROR_LOG("BHS001", "STEP3 失敗:" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_oracle.Rollback();
                conn_oracle.Close();
                transaction_msdb.Rollback();
                conn_msdb.Close();

                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS001", "程式錯誤:3_");
            }
        } // 


    } // ec
} // en