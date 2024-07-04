using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.SqlClient;
using JCLib.DB.Tool;

namespace BHS002
{
    public partial class Form1 : Form
    {
        #region " 單元測試 "

        DataTable WB_PUT_D;
        DataTable PH_PUT_D;
        DataTable WB_PUT_M;
        DataTable PH_PUT_M;
        DataTable WB_PUTTIME;
        DataTable PH_PUTTIME;
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


            // WB_PUT_D, PH_PUT_D
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sqlStr_MSDB = " select AGEN_NO, MMCODE, convert(nvarchar(19), TXTDAY, 120) TXTDAY, SEQ, DEPT, EXTYPE, QTY, MEMO, STATUS, convert(nvarchar(19), UPDATE_TIME, 120) UPDATE_TIME, UPDATE_USER, FLAG from WB_PUT_D where 1=1 and AGEN_NO='102' and CONVERT(VARCHAR, UPDATE_TIME, 111)=CONVERT(VARCHAR, getdate(), 111) "; // and CONVERT(VARCHAR, CREATE_TIME,111)='" + s當天 + "'
            dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
            s += l.getHtmlTable("WB_PUT_D", dt_MSDB, WB_PUT_D, "AGEN_NO, MMCODE, TXTDAY, SEQ, DEPT");
            WB_PUT_D = dt_MSDB;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = " select AGEN_NO, MMCODE, to_char(TXTDAY, 'yyyy/mm/dd') TXTDAY, SEQ, DEPT, EXTYPE, QTY, MEMO, STATUS, to_char(UPDATE_TIME, 'yyyy/mm/dd') UPDATE_TIME, UPDATE_USER, FLAG from PH_PUT_D where 1=1 and AGEN_NO='102' and to_char(UPDATE_TIME, 'yyyy/mm/dd')=to_char(sysdate,'yyyy/mm/dd')  "; // and to_char(CREATE_TIME, 'yyyy/mm/dd')='" + s當天 + "'
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_PUT_D", dt_oralce, PH_PUT_D, "AGEN_NO, MMCODE, TXTDAY, SEQ, DEPT");
            PH_PUT_D = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;


            // WB_PUT_M, PH_PUT_M
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sqlStr_MSDB = " select AGEN_NO, MMCODE, DEPT, MMNAME_C, DEPTNAME, QTY, STATUS, MEMO, CONVERT(VARCHAR, UPDATE_TIME, 111) UPDATE_TIME, UPDATE_USER from WB_PUT_M where 1=1 and AGEN_NO='102' and CONVERT(VARCHAR, UPDATE_TIME, 111)=CONVERT(VARCHAR, getdate(), 111) "; // and CONVERT(VARCHAR, CREATE_TIME, 111)= '" + s當天 + "'
            dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
            s += l.getHtmlTable("WB_PUT_M", dt_MSDB, WB_PUT_M, "AGEN_NO, MMCODE, DEPT");
            WB_PUT_M = dt_MSDB;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = " select AGEN_NO, MMCODE, DEPT, MMNAME_C, DEPTNAME, QTY, STATUS, MEMO, to_char(UPDATE_TIME, 'yyyy/mm/dd') UPDATE_TIME, UPDATE_USER, FLAG from PH_PUT_M where 1=1 and AGEN_NO='102' and to_char(UPDATE_TIME, 'yyyy/mm/dd')=to_char(sysdate,'yyyy/mm/dd') "; // and to_char(CREATE_TIME, 'yyyy/mm/dd')='" + s當天 + "'
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_PUT_M", dt_oralce, PH_PUT_M, "AGEN_NO, MMCODE, DEPT");
            PH_PUT_M = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;


            // -- WB_PUTTIME, PH_PUTTIME ----------
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sqlStr_MSDB = " select convert(nvarchar(19), update_time, 120) update_time from WB_PUTTIME ";
            dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
            s += l.getHtmlTable("WB_PUTTIME", dt_MSDB, WB_PUTTIME, "");
            WB_PUTTIME = dt_MSDB;
            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = "select to_char(UPDATE_TIME, 'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME from PH_PUTTIME ";
            dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            s += l.getHtmlTable("PH_PUTTIME", dt_oralce, PH_PUTTIME, "");
            PH_PUTTIME = dt_oralce;
            s += "</td>" + sBr;
            s += "</tr>" + sBr;

            // --  ERROR_LOG
            s += "<tr>" + sBr;
            s += "<td valign=\"top\">" + sBr;

            s += "</td>" + sBr;
            s += "<td valign=\"top\">" + sBr;
            sql_oracle = "select * from ERROR_LOG where PG='BHS002' and  to_char(LOGTIME,'yyyy/MM/dd')='" + s當天 + "' "; //   to_char(TXTDAY, 'yyyy/mm/dd')='2019/06/18'";
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

            try
            {
                // -- oracle -- 
                sql = "delete from PH_PUT_M where AGEN_NO='102' "; // to_char(CREATE_TIME, 'yyyy/mm/dd')='" + s當天 + "' 
                transcmd_oracle.Add(sql);

                sql = "delete from PH_PUT_D where AGEN_NO='102' "; // to_char(CREATE_TIME, 'yyyy/mm/dd')='" + s當天 + "'
                transcmd_oracle.Add(sql);

                // 3-1 廠商取回院內的
                sql = " insert into PH_PUT_M ( AGEN_NO, MMCODE, MMNAME_C, MMNAME_E, DEPTNAME, DEPT, QTY, STATUS, MEMO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, FLAG ) values (" + sBr;
                sql += " '102', " + sBr; // 00.廠商碼 AGEN_NO(VARCHAR2,6)
                sql += " '08071651', " + sBr; // 01.三總院內碼 MMCODE(VARCHAR2,13)
                sql += " '中文品名', " + sBr; // 02.中文品名 MMNAME_C(VARCHAR2,200)
                sql += " '英文品名', " + sBr; // 03.英文品名 MMNAME_E(VARCHAR2,200)
                sql += " '外科加護中心', " + sBr; // 04.寄放地點 DEPTNAME(VARCHAR2,30)
                sql += " '330300', " + sBr; // 05.責任中心 DEPT(VARCHAR2,6)
                sql += " 1000, " + sBr; // 06.現有寄放量 QTY(INTEGER,)
                sql += " 'B', " + sBr; // 07.狀態 STATUS(VARCHAR2,1) (A未處理,B已轉檔)
                sql += " '把廠商取回品項的院內狀態更新到院外1', " + sBr; // 08.備註 MEMO(VARCHAR2,100)
                sql += " sysdate, " + sBr; // 09.建立日期 CREATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 10.建立人員 CREATE_USER(VARCHAR2,10)
                sql += " sysdate, " + sBr; // 11.異動日期 UPDATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 12.異動人員 UPDATE_USER(VARCHAR2,10)
                sql += " '192.20.2.67', " + sBr; // 13.異動IP UPDATE_IP(VARCHAR2,20)
                sql += " 'A', " + sBr; // 14.狀態 FLAG(VARCHAR2,1)
                sql = sql.Substring(0, sql.Length - 4);
                sql += ") ";
                transcmd_oracle.Add(sql);

                // 3-2 廠商寄放院內的
                sql = " insert into PH_PUT_M ( AGEN_NO, MMCODE, MMNAME_C, MMNAME_E, DEPTNAME, DEPT, QTY, STATUS, MEMO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, FLAG ) values ( " + sBr;
                sql += " '102', " + sBr; // 00.廠商碼 AGEN_NO(VARCHAR2,6)
                sql += " '08071652', " + sBr; // 01.三總院內碼 MMCODE(VARCHAR2,13)
                sql += " '中文品名', " + sBr; // 02.中文品名 MMNAME_C(VARCHAR2,200)
                sql += " '英文品名', " + sBr; // 03.英文品名 MMNAME_E(VARCHAR2,200)
                sql += " '外科加護中心', " + sBr; // 04.寄放地點 DEPTNAME(VARCHAR2,30)
                sql += " '330300', " + sBr; // 05.責任中心 DEPT(VARCHAR2,6)
                sql += " 1000, " + sBr; // 06.現有寄放量 QTY(INTEGER,)
                sql += " 'B', " + sBr; // 07.狀態 STATUS(VARCHAR2,1) D-刪除, A-寄放中
                sql += " '把廠商寄放品項的院內狀態更新到院外2', " + sBr; // 08.備註 MEMO(VARCHAR2,100)
                sql += " sysdate, " + sBr; // 09.建立日期 CREATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 10.建立人員 CREATE_USER(VARCHAR2,10)
                sql += " sysdate, " + sBr; // 11.異動日期 UPDATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 12.異動人員 UPDATE_USER(VARCHAR2,10)
                sql += " '192.20.2.67', " + sBr; // 13.異動IP UPDATE_IP(VARCHAR2,20)
                sql += " 'A', " + sBr; // 14.狀態 FLAG(VARCHAR2,1)
                sql = sql.Substring(0, sql.Length - 4);
                sql += ") ";
                transcmd_oracle.Add(sql);

                // 4-1 外網沒資料，新增
                sql = " insert into PH_PUT_M ( AGEN_NO, MMCODE, MMNAME_C, MMNAME_E, DEPTNAME, DEPT, QTY, STATUS, MEMO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, FLAG ) values ( " + sBr;
                sql += " '102', " + sBr; // 00.廠商碼 AGEN_NO(VARCHAR2,6)
                sql += " '08071654', " + sBr; // 01.三總院內碼 MMCODE(VARCHAR2,13)
                sql += " '中文品名4', " + sBr; // 02.中文品名 MMNAME_C(VARCHAR2,200)
                sql += " '英文品名4', " + sBr; // 03.英文品名 MMNAME_E(VARCHAR2,200)
                sql += " '外科加護中心', " + sBr; // 04.寄放地點 DEPTNAME(VARCHAR2,30)
                sql += " '330300', " + sBr; // 05.責任中心 DEPT(VARCHAR2,6)
                sql += " 5678, " + sBr; // 06.現有寄放量 QTY(INTEGER,)
                sql += " 'B', " + sBr; // 07.狀態 STATUS(VARCHAR2,1) A-未處理, B-已轉檔
                sql += " '院內有，外網無，新增到外網', " + sBr; // 08.備註 MEMO(VARCHAR2,100)
                sql += " sysdate, " + sBr; // 09.建立日期 CREATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 10.建立人員 CREATE_USER(VARCHAR2,10)
                sql += " sysdate, " + sBr; // 11.異動日期 UPDATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 12.異動人員 UPDATE_USER(VARCHAR2,10)
                sql += " '192.20.2.67', " + sBr; // 13.異動IP UPDATE_IP(VARCHAR2,20)
                sql += " 'A', " + sBr; // 14.狀態 FLAG(VARCHAR2,1)
                sql = sql.Substring(0, sql.Length - 4);
                sql += ") ";
                transcmd_oracle.Add(sql);

                // 5-1 院內耗用
                sql = " insert into PH_PUT_D ( AGEN_NO, MMCODE, TXTDAY, SEQ, EXTYPE, QTY, MEMO, STATUS, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, FLAG, DEPT ) values ( " + sBr;
                sql += " '102', " + sBr; // 00.廠商碼 AGEN_NO(VARCHAR2,6)
                sql += " '08071653', " + sBr; // 01.三總院內碼 MMCODE(VARCHAR2,13)
                sql += " sysdate, " + sBr; // 02.交易日期 TXTDAY(DATE,)
                sql += " 99, " + sBr; // 03.交易流水號 SEQ(INTEGER,)
                sql += " '20', " + sBr; // 04.異動類別 EXTYPE(VARCHAR2,2) // 20耗用
                sql += " 20, " + sBr; // 05.異動數量 QTY(INTEGER,)
                sql += " '院內耗用要更新到外網', " + sBr; // 06.備註 MEMO(VARCHAR2,100)
                sql += " 'A', " + sBr; // 07.狀態 STATUS(VARCHAR2,1) A-未處理, B-已轉檔
                sql += " sysdate, " + sBr; // 08.建立日期 CREATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 09.建立人員 CREATE_USER(VARCHAR2,10)
                sql += " sysdate, " + sBr; // 10.異動日期 UPDATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 11.異動人員 UPDATE_USER(VARCHAR2,10)
                sql += " '192.20.2.67', " + sBr; // 12.異動IP UPDATE_IP(VARCHAR2,20)
                sql += " 'B', " + sBr; // 13.狀態 FLAG(VARCHAR2,1) A-未處理, B-已轉檔
                sql += " '330300', " + sBr; // 05.責任中心 DEPT(VARCHAR2,6)
                sql = sql.Substring(0, sql.Length - 4);
                sql += ") ";
                transcmd_oracle.Add(sql);
                
                sql = "delete  from ERROR_LOG where PG='BHS002' ";
                transcmd_oracle.Add(sql);

                int rowsAffected_oracle = -1;
                rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
                    transcmd_oracle,
                    listParam_oracle,
                    transaction_oracle,
                    conn_oracle);

                // -- msdb
                int iTransIdx = 0;
                sql = "delete from WB_PUT_D where AGEN_NO='102' "; //  CONVERT(VARCHAR, CREATE_TIME, 111)='" + s當天 + "' 
                transcmd_msdb.Add(sql);

                sql = "delete from WB_PUT_M where AGEN_NO='102' "; //  CONVERT(VARCHAR, CREATE_TIME, 111)='" + s當天 + "' 
                transcmd_msdb.Add(sql);

                // 2-1 外網取回院內的
                sql = "insert into WB_PUT_D (AGEN_NO, MMCODE, TXTDAY, SEQ, DEPT, EXTYPE, QTY, MEMO, STATUS, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, FLAG) values ( ";
                sql += " '102', " + sBr; // 00.廠商碼(VARCHAR2,6)
                sql += " '08071651', " + sBr; // 01.三總院內碼(VARCHAR2,13)
                sql += " SYSDATETIME(), " + sBr; // 02.TXTDAY 交易日期(DATE,)
                sql += " '1', " + sBr; // 03.SEQ 交易流水號(INTEGER,)
                sql += " '330300', " + sBr; // 14.責任中心 DEPT(VARCHAR2,6)
                sql += " '10', " + sBr; // 04.EXTYPE 異動類別(VARCHAR2,2) // 10(取回) 
                sql += " 1, " + sBr; // 05.QTY 異動數量(INTEGER,)
                sql += " '外網取回院內', " + sBr; // 06.MEMO 備註(VARCHAR2,100)
                sql += " 'B', " + sBr; // 07.STATUS 狀態(VARCHAR2,1)
                sql += " SYSDATETIME(), " + sBr; // 08.CREATE_TIME 建立日期(DATE,)
                sql += " '612670', " + sBr; // 09.CREATE_USER 建立人員(VARCHAR2,10)
                sql += " SYSDATETIME(), " + sBr; // 10.UPDATE_TIME 異動日期(DATE,)
                sql += " '612670', " + sBr; // 11.UPDATE_USER 異動人員(VARCHAR2,10)
                sql += " '192.20.2.67', " + sBr; // 12.UPDATE_IP 異動IP(VARCHAR2,20)
                sql += " 'A', " + sBr; // 13.FLAG 狀態(VARCHAR2,1)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) ";
                sql = sql.Replace(sBr, "");
                transcmd_msdb.Add(sql);

                // 2-2 外網寄放院內的
                sql = "insert into WB_PUT_D (AGEN_NO, MMCODE, TXTDAY, SEQ, DEPT, EXTYPE, QTY, MEMO, STATUS, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, FLAG) values ( ";
                sql += " '102', " + sBr; // 00.廠商碼(VARCHAR2,6)
                sql += " '08071652', " + sBr; // 01.三總院內碼(VARCHAR2,13)
                sql += " SYSDATETIME(), " + sBr; // 02.TXTDAY 交易日期(DATE,)
                sql += " '1', " + sBr; // 03.SEQ 交易流水號(INTEGER,)
                sql += " '330300', " + sBr; // 05.責任中心 DEPT(VARCHAR2,6)
                sql += " '31', " + sBr; // 04.EXTYPE 異動類別(VARCHAR2,2)   // 31(寄放)
                sql += " 1, " + sBr; // 05.QTY 異動數量(INTEGER,)
                sql += " '外網寄放院內', " + sBr; // 06.MEMO 備註(VARCHAR2,100)
                sql += " 'B', " + sBr; // 07.STATUS 狀態(VARCHAR2,1)
                sql += " SYSDATETIME(), " + sBr; // 08.CREATE_TIME 建立日期(DATE,)
                sql += " '612670', " + sBr; // 09.CREATE_USER 建立人員(VARCHAR2,10)
                sql += " SYSDATETIME(), " + sBr; // 10.UPDATE_TIME 異動日期(DATE,)
                sql += " '612670', " + sBr; // 11.UPDATE_USER 異動人員(VARCHAR2,10)
                sql += " '192.20.2.67', " + sBr; // 12.UPDATE_IP 異動IP(VARCHAR2,20)
                sql += " 'A', " + sBr; // 13.FLAG 狀態(VARCHAR2,1)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) ";
                sql = sql.Replace(sBr, "");
                transcmd_msdb.Add(sql);
                
                // 4-2 有資料->更新
                sql = "insert into WB_PUT_M (AGEN_NO, MMCODE, MMNAME_C, MMNAME_E, DEPTNAME, DEPT, QTY, STATUS, MEMO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) values ( ";
                sql += " '102', " + sBr; // 00.廠商碼 AGEN_NO(VARCHAR2,6)
                sql += " '08071651', " + sBr; // 01.三總院內碼 MMCODE(VARCHAR2,13)
                sql += " '中文品名1', " + sBr; // 02.中文品名 MMNAME_C(VARCHAR2,200)
                sql += " '英文品名1', " + sBr; // 03.英文品名 MMNAME_E(VARCHAR2,200)
                sql += " '外科加護中心', " + sBr; // 04.寄放地點 DEPTNAME(VARCHAR2,30)
                sql += " '330300', " + sBr; // 05.責任中心 DEPT(VARCHAR2,6)
                sql += " 4000, " + sBr; // 06.現有寄放量 QTY(INTEGER,)
                sql += " 'B', " + sBr; // 07.狀態 STATUS(VARCHAR2,1) A-未處理, B-已轉檔
                sql += " '院內有，外網有，更新到外網(取回)', " + sBr; // 08.備註 MEMO(VARCHAR2,100)
                sql += " SYSDATETIME(), " + sBr; // 09.建立日期 CREATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 10.建立人員 CREATE_USER(VARCHAR2,10)
                sql += " SYSDATETIME(), " + sBr; // 11.異動日期 UPDATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 12.異動人員 UPDATE_USER(VARCHAR2,10)
                sql += " '192.20.2.67', " + sBr; // 13.異動IP UPDATE_IP(VARCHAR2,20)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) ";
                sql = sql.Replace(sBr, "");
                transcmd_msdb.Add(sql);

                sql = "insert into WB_PUT_M (AGEN_NO, MMCODE, MMNAME_C, MMNAME_E, DEPTNAME, DEPT, QTY, STATUS, MEMO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) values ( ";
                sql += " '102', " + sBr; // 00.廠商碼 AGEN_NO(VARCHAR2,6)
                sql += " '08071652', " + sBr; // 01.三總院內碼 MMCODE(VARCHAR2,13)
                sql += " '中文品名2', " + sBr; // 02.中文品名 MMNAME_C(VARCHAR2,200)
                sql += " '英文品名2', " + sBr; // 03.英文品名 MMNAME_E(VARCHAR2,200)
                sql += " '外科加護中心', " + sBr; // 04.寄放地點 DEPTNAME(VARCHAR2,30)
                sql += " '330300', " + sBr; // 05.責任中心 DEPT(VARCHAR2,6)
                sql += " 4000, " + sBr; // 06.現有寄放量 QTY(INTEGER,)
                sql += " 'B', " + sBr; // 07.狀態 STATUS(VARCHAR2,1) A-未處理, B-已轉檔
                sql += " '院內有，外網有，更新到外網(寄放)', " + sBr; // 08.備註 MEMO(VARCHAR2,100)
                sql += " SYSDATETIME(), " + sBr; // 09.建立日期 CREATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 10.建立人員 CREATE_USER(VARCHAR2,10)
                sql += " SYSDATETIME(), " + sBr; // 11.異動日期 UPDATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 12.異動人員 UPDATE_USER(VARCHAR2,10)
                sql += " '192.20.2.67', " + sBr; // 13.異動IP UPDATE_IP(VARCHAR2,20)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) ";
                sql = sql.Replace(sBr, "");
                transcmd_msdb.Add(sql);

                // 6 院內耗用廠商的
                sql = "insert into WB_PUT_M (AGEN_NO, MMCODE, MMNAME_C, MMNAME_E, DEPTNAME, DEPT, QTY, STATUS, MEMO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) values ( ";
                sql += " '102', " + sBr; // 00.廠商碼 AGEN_NO(VARCHAR2,6)
                sql += " '08071653', " + sBr; // 01.三總院內碼 MMCODE(VARCHAR2,13)
                sql += " '中文品名1', " + sBr; // 02.中文品名 MMNAME_C(VARCHAR2,200)
                sql += " '英文品名1', " + sBr; // 03.英文品名 MMNAME_E(VARCHAR2,200)
                sql += " '外科加護中心', " + sBr; // 04.寄放地點 DEPTNAME(VARCHAR2,30)
                sql += " '330300', " + sBr; // 05.責任中心 DEPT(VARCHAR2,6)
                sql += " 4000, " + sBr; // 06.現有寄放量 QTY(INTEGER,)
                sql += " 'B', " + sBr; // 07.狀態 STATUS(VARCHAR2,1) A-未處理, B-已轉檔
                sql += " '廠商未耗用前', " + sBr; // 08.備註 MEMO(VARCHAR2,100)
                sql += " SYSDATETIME(), " + sBr; // 09.建立日期 CREATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 10.建立人員 CREATE_USER(VARCHAR2,10)
                sql += " SYSDATETIME(), " + sBr; // 11.異動日期 UPDATE_TIME(DATE,)
                sql += " '612670', " + sBr; // 12.異動人員 UPDATE_USER(VARCHAR2,10)
                sql += " '192.20.2.67', " + sBr; // 13.異動IP UPDATE_IP(VARCHAR2,20)
                sql = sql.Substring(0, sql.Length - 4);
                sql += " ) ";
                sql = sql.Replace(sBr, "");
                transcmd_msdb.Add(sql);


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

        L l = new L("BHS002");
        FL fl = new FL("BHS002");
        String sBr = "\r\n";

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        //程式起始 BHS002-寄售廠商寄放量資料轉檔 排程每天 0:30~24:00每30分鐘執行一次
        private void Form1_Load(object sender, System.EventArgs e)
        {
            try
            {
                l.ldb(get資料庫現況("初始資料庫"));
                //單元測試();
                //l.ldb(get資料庫現況("單元測試"));

                // 2_將外網 MSdb WB_PUT_D 異動資料複製到院內 Oracle PH_PUT_D
                SelectMS_WBputD_IntoOracle_PHputD_02();
                l.ldb(get資料庫現況("2.WB_PUT_D_to_PH_PUT_D  SelectMS_WBputD_IntoOracle_PHputD_02"));

                // 3_更新院內 Oracle PH_PUT_M
                UpdateOracle_PHputM_03();
                l.ldb(get資料庫現況("3.PH_PUT_D_to_PH_PUT_M.QTY  UpdateOracle_PHputM_03"));

                // 4_將院內 Oracle PH_PUT_M 異動資料複製或更新至外網 MSdb WB_PUT_M
                SelectOracle_PHputM_IntoMS_WBputM_04();
                l.ldb(get資料庫現況("4.PH_PUT_M_to_WB_PUT_M  SelectOracle_PHputM_IntoMS_WBputM_04"));

                // 5_將院內 Oracle PH_PUT_D 異動資料複製到外網 MSdb WB_PUT_D
                SelectOracle_PHputD_IntoMS_WBputD_05();
                l.ldb(get資料庫現況("5.PH_PUT_D_to_WB_PUT_D  SelectOracle_PHputD_IntoMS_WBputD_05"));

                // 6_院外異動檔->更新院外主檔(數量)
                f_06();
                l.ldb(get資料庫現況("6.WB_PUT_D_toWB_PUT_D.QTY  f_06"));

                // 7_院內 Oracle PH_PUTTIME 外部 MSdb WB_PUTTIME 記錄資料更新時間
                Update_PH_WB_PutTime_07();
                l.ldb(get資料庫現況("7.Update_PH_WB_PutTime_07"));

                // 寄送mail通知admin有執行程式 
                fl.sendMailToAdmin("三總排程-BHS002執行完畢", "程式正常執行完畢", l.get資料庫現況Log檔路徑());
            }
            catch (Exception ex)
            {
                // 寄送mail通知admin有執行程式 
                fl.sendMailToAdmin("三總排程-BHS002執行異常", "Exception=" + ex.Message, l.get資料庫現況Log檔路徑());
                l.le("Form1_Load()", ex.Message);
            }
            this.Close();
        }

        // 1_將外網 MSdb WB_PUT_D 異動資料複製到院內 Oracle PH_PUT_D
        private void SelectMS_WBputD_IntoOracle_PHputD_02()
        {
            l.lg("SelectMS_WBputD_IntoOracle_PHputD_02()", "");
            try
            {
                string msg_MSDB = "error";
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                DataTable dt_MSDB = new DataTable();
                string sqlStr_MSDB = "";
                sqlStr_MSDB += " select ";
                sqlStr_MSDB += " AGEN_NO, MMCODE, CONVERT(VARCHAR, TXTDAY, 120) TXTDAY, SEQ, DEPT, EXTYPE, QTY, MEMO, STATUS, CONVERT(VARCHAR, CREATE_TIME, 120) CREATE_TIME, CREATE_USER, UPDATE_IP ";
                sqlStr_MSDB += " from WB_PUT_D where STATUS='B' and FLAG='A' and EXTYPE in ('10','31') ";
                l.lg("SelectMS_WBputD_IntoOracle_PHputD_02()", "sqlStr_MSDB=" + sqlStr_MSDB);
                dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
                if (msg_MSDB == "") //取得 WB_PUT_D 無錯誤
                {
                    l.lg("SelectMS_WBputD_IntoOracle_PHputD_02()", "dt_MSDB.Rows.Count=" + dt_MSDB.Rows.Count);
                    if (dt_MSDB.Rows.Count > 0) //WB_PUT_D 有資料
                    {
                        for (int i = 0; i < dt_MSDB.Rows.Count; i++)
                        {
                            int rowsAffected_oracle = -1;
                            string msg_oracle = "error";
                            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                            List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                            string cmdstr = "insert into PH_PUT_D(AGEN_NO,MMCODE,TXTDAY,SEQ, DEPT, EXTYPE,QTY,MEMO,STATUS,CREATE_TIME,CREATE_USER,UPDATE_IP,UPDATE_TIME,UPDATE_USER, FLAG) ";
                            cmdstr += "                    values(:agen_no,:mmcode,to_date(:txtday, 'yyyy-mm-dd hh24:mi:ss'),:seq,:dept,:extype,:qty,:memo,'B',to_date(:create_time, 'yyyy-mm-dd hh24:mi:ss'),:create_user,:update_ip,sysdate,'AUTO', 'A') ";
                            l.lg("SelectMS_WBputD_IntoOracle_PHputD_02()", "cmdstr=" + cmdstr);
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", dt_MSDB.Rows[i]["AGEN_NO"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", dt_MSDB.Rows[i]["MMCODE"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":txtday", "VarChar", dt_MSDB.Rows[i]["TXTDAY"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":seq", "VarChar", dt_MSDB.Rows[i]["SEQ"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":dept", "VarChar", dt_MSDB.Rows[i]["DEPT"].ToString().Trim()));                            
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":extype", "VarChar", dt_MSDB.Rows[i]["EXTYPE"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":qty", "VarChar", dt_MSDB.Rows[i]["QTY"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":memo", "VarChar", dt_MSDB.Rows[i]["MEMO"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":create_time", "VarChar", dt_MSDB.Rows[i]["CREATE_TIME"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":create_user", "VarChar", dt_MSDB.Rows[i]["CREATE_USER"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":update_ip", "VarChar", dt_MSDB.Rows[i]["UPDATE_IP"].ToString().Trim()));
                            rowsAffected_oracle = callDbtools_oralce.CallExecSQL(cmdstr, paraList, "oracle", ref msg_oracle);
                            l.lg("SelectMS_WBputD_IntoOracle_PHputD_02()", "rowsAffected_oracle=" + rowsAffected_oracle);
                            if (msg_oracle == "") //insert PH_PUT_D 無錯誤
                            {
                                if (rowsAffected_oracle == 1) //insert PH_PUT_D 正常回傳筆數1
                                {
                                    int rowsAffected_msdb = -1;
                                    msg_MSDB = "error";
                                    List<CallDBtools_MSDb.MSDbParam> paraListA = new List<CallDBtools_MSDb.MSDbParam>();
                                    sqlStr_MSDB = "update WB_PUT_D set FLAG='B', UPDATE_TIME = SYSDATETIME(), UPDATE_USER = 'AUTO' ";
                                    sqlStr_MSDB += "  where AGEN_NO = @agen_no and MMCODE = @mmcode and convert(nvarchar(19),TXTDAY, 120) = @txtday and SEQ = @seq and DEPT = @dept ";
                                    l.lg("SelectMS_WBputD_IntoOracle_PHputD_02()", "sqlStr_MSDB=" + sqlStr_MSDB);
                                    paraListA.Add(new CallDBtools_MSDb.MSDbParam(1, "@agen_no", "nvarchar", dt_MSDB.Rows[i]["AGEN_NO"].ToString().Trim()));
                                    paraListA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmcode", "nvarchar", dt_MSDB.Rows[i]["MMCODE"].ToString().Trim()));
                                    paraListA.Add(new CallDBtools_MSDb.MSDbParam(1, "@txtday", "datetime", Convert.ToDateTime(dt_MSDB.Rows[i]["TXTDAY"]).ToString("yyyy-MM-dd HH:mm:ss").Trim()));
                                    paraListA.Add(new CallDBtools_MSDb.MSDbParam(1, "@seq", "int", dt_MSDB.Rows[i]["SEQ"].ToString().Trim()));
                                    paraListA.Add(new CallDBtools_MSDb.MSDbParam(1, "@dept", "int", dt_MSDB.Rows[i]["DEPT"].ToString().Trim()));                                    
                                    rowsAffected_msdb = callDBtools_msdb.CallExecSQL(sqlStr_MSDB, paraListA, "msdb", ref msg_MSDB);
                                    l.lg("SelectMS_WBputD_IntoOracle_PHputD_02()", "rowsAffected_msdb=" + rowsAffected_msdb);
                                    if (msg_MSDB != "")
                                        callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP1-update WB_PUT_D失敗:" + msg_MSDB, "AUTO");
                                }
                            }
                            else //insert PH_PUT_D 有錯誤
                            {
                                String sKey = "";
                                sKey += "select * from PH_PUT_D where 1=1 " + sBr;
                                sKey += "and AGEN_NO='" + dt_MSDB.Rows[i]["AGEN_NO"].ToString() + "' " + sBr;
                                sKey += "and MMCODE='" + dt_MSDB.Rows[i]["MMCODE"].ToString() + "' " + sBr;
                                sKey += "and to_char(TXTDAY,'yyyy-mm-dd hh24:mi:ss')='" + dt_MSDB.Rows[i]["TXTDAY"].ToString() + "' " + sBr;
                                sKey += "and SEQ='" + dt_MSDB.Rows[i]["SEQ"].ToString() + "' " + sBr;
                                sKey += "and DEPT='" + dt_MSDB.Rows[i]["DEPT"].ToString() + "' " + sBr;                                
                                callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP1-insert PH_PUT_D失敗:" + sKey + msg_oracle, "AUTO");
                            }
                        }
                    }
                }
                else //取得 WB_PUT_D 有錯誤，寫ERROR_LOG
                {
                    CallDBtools_Oracle callDBtools_oracle = new CallDBtools_Oracle();
                    callDBtools_oracle.I_ERROR_LOG("BHS002", "STEP1-取得WB_PUT_D失敗:" + msg_MSDB, "AUTO");
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS002", "程式錯誤:1_");
            }
        }

        // 2_更新院內 Oracle PH_PUT_M
        private void UpdateOracle_PHputM_03()
        {
            try
            {
                string msg_oracle = "error";
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_oralce = new DataTable();
                string sql_oracle = " select AGEN_NO,MMCODE, to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') TXTDAY,SEQ,DEPT,EXTYPE,QTY,STATUS ";
                sql_oracle += " from PH_PUT_D ";
                sql_oracle += " where FLAG= 'A' and EXTYPE in ('10','31') ";
                l.lg("UpdateOracle_PHputM_03()", "sql_oracle=" + sql_oracle);
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "TXTDAY, SEQ, MMCODE", null, "oracle", "T1", ref msg_oracle);
                l.lg("UpdateOracle_PHputM_03()", "dt_oralce.Rows.Count=" + dt_oralce.Rows.Count);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    {
                        int rowsAffected_oracle = -1;
                        if (dt_oralce.Rows[i]["EXTYPE"].ToString().Trim() == "10") //10-取回(減帳)
                        {
                            sql_oracle = " update PH_PUT_M set QTY = QTY - :qty, FLAG='A', UPDATE_TIME=sysdate, UPDATE_USER='AUTO'  ";
                            sql_oracle += " where AGEN_NO = :agen_no and MMCODE = :mmcode and DEPT=:dept ";
                            l.lg("UpdateOracle_PHputM_03()", "sql_oracle=" + sql_oracle);
                            List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":qty", "VarChar", dt_oralce.Rows[i]["QTY"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));
                            rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
                            l.lg("UpdateOracle_PHputM_03()", "rowsAffected_oracle=" + rowsAffected_oracle);
                            if (msg_oracle == "") 
                            {
                                rowsAffected_oracle = -1;
                                sql_oracle = " update PH_PUT_D set FLAG='B', UPDATE_TIME=sysdate, UPDATE_USER='AUTO'  ";
                                sql_oracle += " where AGEN_NO = :agen_no and MMCODE = :mmcode and to_char(TXTDAY,'YYYY/MM/DD HH24:MI:SS') = :txtday and SEQ = :seq and DEPT=:dept ";
                                l.lg("UpdateOracle_PHputM_03()", "sql_oracle=" + sql_oracle);
                                List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":txtday", "VarChar", Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"]).ToString("yyyy/MM/dd HH:mm:ss").Trim()));
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));
                                rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraListA, "oracle", ref msg_oracle);
                                l.lg("UpdateOracle_PHputM_03()", "rowsAffected_oracle=" + rowsAffected_oracle);
                                if (msg_oracle != "")
                                {
                                    callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP2-update PH_PUT_D 10失敗:" + msg_oracle, "AUTO");
                                }
                            }
                            else
                            {
                                callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP2-update PH_PUT_D -QTY失敗:" + msg_oracle, "AUTO");
                            }
                        }
                        else if (dt_oralce.Rows[i]["EXTYPE"].ToString().Trim() == "31") //31-寄放(加帳)
                        {
                            sql_oracle = " update PH_PUT_M set QTY = QTY + :qty, FLAG='A', UPDATE_TIME=sysdate, UPDATE_USER='AUTO'  ";
                            sql_oracle += " where AGEN_NO = :agen_no and MMCODE = :mmcode and DEPT=:dept ";
                            l.lg("UpdateOracle_PHputM_03()", "sql_oracle=" + sql_oracle);
                            List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":qty", "VarChar", dt_oralce.Rows[i]["QTY"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));
                            rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
                            l.lg("UpdateOracle_PHputM_03()", "rowsAffected_oracle=" + rowsAffected_oracle);
                            if (msg_oracle == "")
                            {
                                rowsAffected_oracle = -1;
                                sql_oracle = " update PH_PUT_D set FLAG='B', UPDATE_TIME=sysdate, UPDATE_USER='AUTO'  ";
                                sql_oracle += " where AGEN_NO = :agen_no and MMCODE = :mmcode and to_char(TXTDAY,'YYYY/MM/DD HH24:MI:SS') = :txtday and SEQ = :seq and DEPT=:dept ";
                                l.lg("UpdateOracle_PHputM_03()", "sql_oracle=" + sql_oracle);
                                List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":txtday", "VarChar", Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"]).ToString("yyyy/MM/dd HH:mm:ss").Trim()));
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));
                                rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraListA, "oracle", ref msg_oracle);
                                l.lg("UpdateOracle_PHputM_03()", "rowsAffected_oracle=" + rowsAffected_oracle);
                                if (msg_oracle != "")
                                {
                                    callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP2-update PH_PUT_D 31失敗:" + msg_oracle, "AUTO");
                                }
                            }
                            else
                            {
                                callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP2-update PH_PUT_D +QTY失敗:" + msg_oracle, "AUTO");
                            }
                        }
                    }
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP2-取得PH_PUT_D失敗:" + msg_oracle, "AUTO");
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS002", "程式錯誤:2_");
            }
        }

        // 3_將院內 Oracle PH_PUT_M 異動資料複製或更新至外網 MSdb WB_PUT_M
        private void SelectOracle_PHputM_IntoMS_WBputM_04()
        {
            try
            {
                string msg_oracle = "error";
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_oralce = new DataTable();
                string sql_oracle = " select AGEN_NO,MMCODE,MMNAME_C,MMNAME_E,QTY,DEPTNAME,DEPT,MEMO,STATUS,to_char(CREATE_TIME, 'yyyy-mm-dd hh24:mi:ss') CREATE_TIME,CREATE_USER,to_char(UPDATE_TIME, 'yyyy-mm-dd hh24:mi:ss') UPDATE_TIME,UPDATE_IP,UPDATE_USER ";
                sql_oracle += " from PH_PUT_M where FLAG = 'A' ";
                l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "sql_oracle=" + sql_oracle);
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "dt_oralce.Rows.Count=" + dt_oralce.Rows.Count);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                    string msgDB = "error", sql_msdb = "", ms_resStr = "";
                    int rowsAffected = -1;
                    for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    {
                        sql_msdb = "select count(*) from WB_PUT_M where AGEN_NO = @agen_no and MMCODE = @mmcode and DEPT = @dept ";
                        l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "sql_msdb=" + sql_msdb);
                        List<CallDBtools_MSDb.MSDbParam> paraList_msA = new List<CallDBtools_MSDb.MSDbParam>();
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));
                        ms_resStr = callDBtools_msdb.CallExecScalar(sql_msdb, paraList_msA, "msdb", ref msgDB);
                        l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "ms_resStr=" + ms_resStr);
                        if (msgDB == "" && ms_resStr == "0") // insert
                        {
                            rowsAffected = -1;
                            sql_msdb = "insert into WB_PUT_M(AGEN_NO,MMCODE,MMNAME_C,STATUS,DEPT, ";
                            sql_msdb += "                    MEMO,CREATE_TIME,CREATE_USER,MMNAME_E,UPDATE_TIME, ";
                            sql_msdb += "                    UPDATE_USER,DEPTNAME,UPDATE_IP,QTY) ";
                            sql_msdb += "             values(@agen_no,@mmcode,@mmname_c,@status,@dept, ";
                            sql_msdb += "                    @memo,convert(datetime, @create_time, 120),@create_user,@mmname_e,convert(datetime, @update_time, 120), ";
                            sql_msdb += "                    @update_user,@deptname,@update_ip,@qty) ";
                            l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "sql_msdb=" + sql_msdb);
                            paraList_msA.Clear();
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmname_c", "VarChar", dt_oralce.Rows[i]["MMNAME_C"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@status", "VarChar", dt_oralce.Rows[i]["STATUS"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));

                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@memo", "VarChar", dt_oralce.Rows[i]["MEMO"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@create_time", "VarChar", dt_oralce.Rows[i]["CREATE_TIME"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@create_user", "VarChar", dt_oralce.Rows[i]["CREATE_USER"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmname_e", "VarChar", dt_oralce.Rows[i]["MMNAME_E"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_time", "VarChar", dt_oralce.Rows[i]["UPDATE_TIME"].ToString().Trim()));

                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_user", "VarChar", dt_oralce.Rows[i]["UPDATE_USER"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@deptname", "VarChar", dt_oralce.Rows[i]["DEPTNAME"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_ip", "VarChar", dt_oralce.Rows[i]["UPDATE_IP"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@qty", "VarChar", dt_oralce.Rows[i]["QTY"].ToString().Trim()));
                            rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, paraList_msA, "msdb", ref msgDB);
                            l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "rowsAffected=" + rowsAffected);
                            if (msgDB == "" && rowsAffected == 1) // insert成功，更改FLAG狀態
                            {
                                int rowsAffected_oracle = -1;
                                sql_oracle = " update PH_PUT_M set FLAG='B', UPDATE_TIME=sysdate, UPDATE_USER='AUTO'  ";
                                sql_oracle += " where AGEN_NO = :agen_no and MMCODE = :mmcode ";
                                l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "sql_oracle=" + sql_oracle);
                                List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                                rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraListA, "oracle", ref msg_oracle);
                                l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "rowsAffected_oracle=" + rowsAffected_oracle);
                                if (msg_oracle != "")
                                {
                                    callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP3-update PH_PUT_M失敗:" + msg_oracle, "AUTO");
                                }
                            }
                        }
                        else if (msgDB == "" && ms_resStr == "1") // update
                        {
                            rowsAffected = -1;
                            sql_msdb = " update WB_PUT_M ";
                            sql_msdb += "   set MMNAME_C = @mmname_c, STATUS = @status, MEMO = @memo, MMNAME_E = @mmname_e, ";
                            sql_msdb += "       UPDATE_TIME = convert(datetime, @update_time, 120), UPDATE_USER = @update_user, DEPTNAME = @deptname, UPDATE_IP = @update_ip ";
                            sql_msdb += " where AGEN_NO = @agen_no and MMCODE = @mmcode and DEPT = @dept";
                            l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "sql_msdb=" + sql_msdb);
                            paraList_msA.Clear();
                            
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmname_c", "VarChar", dt_oralce.Rows[i]["MMNAME_C"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@status", "VarChar", dt_oralce.Rows[i]["STATUS"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@memo", "VarChar", dt_oralce.Rows[i]["MEMO"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmname_e", "VarChar", dt_oralce.Rows[i]["MMNAME_E"].ToString().Trim()));

                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_time", "VarChar", dt_oralce.Rows[i]["UPDATE_TIME"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_user", "VarChar", dt_oralce.Rows[i]["UPDATE_USER"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@deptname", "VarChar", dt_oralce.Rows[i]["DEPTNAME"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_ip", "VarChar", dt_oralce.Rows[i]["UPDATE_IP"].ToString().Trim()));

                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));
                            rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, paraList_msA, "msdb", ref msgDB);
                            l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "rowsAffected=" + rowsAffected);
                            if (msgDB == "" && rowsAffected == 1) // insert成功，更改FLAG狀態
                            {
                                int rowsAffected_oracle = -1;
                                sql_oracle = " update PH_PUT_M set FLAG='B', UPDATE_TIME=sysdate, UPDATE_USER='AUTO'  ";
                                sql_oracle += " where AGEN_NO = :agen_no and MMCODE = :mmcode and DEPT=:dept ";
                                l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "sql_oracle=" + sql_oracle);
                                List<CallDBtools_Oracle.OracleParam> paraListA = new List<CallDBtools_Oracle.OracleParam>();
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                                paraListA.Add(new CallDBtools_Oracle.OracleParam(1, ":dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));
                                rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraListA, "oracle", ref msg_oracle);
                                l.lg("SelectOracle_PHputM_IntoMS_WBputM_04()", "rowsAffected_oracle=" + rowsAffected_oracle);
                                if (msg_oracle != "")
                                {
                                    callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP3-update PH_PUT_M失敗:" + msg_oracle, "AUTO");
                                }
                            }
                        }
                        else if (msgDB != "")
                        {
                            callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP3-select WB_PUT_M失敗:" + msg_oracle, "AUTO");
                        }
                    }
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP3-取得PH_PUT_M失敗:" + msg_oracle, "AUTO");
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS002", "程式錯誤:3_");
            }
        }

        // 4_將院內 Oracle PH_PUT_D 異動資料複製到外網 MSdb WB_PUT_D
        private void SelectOracle_PHputD_IntoMS_WBputD_05()
        {
            try
            {
                string msg_oracle = "error";
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_oralce = new DataTable();
                string sql_oracle = " select AGEN_NO,MMCODE,to_char(TXTDAY, 'yyyy-mm-dd hh24:mi:ss') TXTDAY, SEQ, DEPT, EXTYPE, QTY, MEMO, STATUS, to_char(CREATE_TIME, 'yyyy-mm-dd hh24:mi:ss') CREATE_TIME, CREATE_USER, to_char(UPDATE_TIME, 'yyyy-mm-dd hh24:mi:ss'), UPDATE_USER, UPDATE_IP, FLAG ";
                sql_oracle += " from PH_PUT_D ";
                sql_oracle += " where STATUS = 'A' and EXTYPE = '20' ";
                l.lg("SelectOracle_PHputD_IntoMS_WBputD_05()", "sql_oracle=" + sql_oracle);
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                l.lg("SelectOracle_PHputD_IntoMS_WBputD_05()", "dt_oralce.Rows.Count=" + dt_oralce.Rows.Count);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    {
                        string msgDB = "";
                        CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                        int rowsAffected = -1;
                        List<CallDBtools_MSDb.MSDbParam> paraList1 = new List<CallDBtools_MSDb.MSDbParam>();
                        string cmdstr = "insert into WB_PUT_D(AGEN_NO,MMCODE,TXTDAY,SEQ,DEPT,EXTYPE,QTY,MEMO,STATUS,CREATE_TIME,CREATE_USER,UPDATE_TIME,UPDATE_USER,UPDATE_IP,FLAG) ";
                        cmdstr += "                   values (@agen_no,@mmcode,convert(datetime, @txtday, 120),@seq,@dept,'20',@qty,@memo,'A',convert(datetime, @create_time, 120),@create_user,SYSDATETIME(),'AUTO',@update_ip, @flag) ";
                        l.lg("SelectOracle_PHputD_IntoMS_WBputD_05()", "cmdstr=" + cmdstr);
                        paraList1.Add(new CallDBtools_MSDb.MSDbParam(1, "@agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                        paraList1.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                        paraList1.Add(new CallDBtools_MSDb.MSDbParam(1, "@txtday", "VarChar", dt_oralce.Rows[i]["TXTDAY"].ToString().Trim()));
                        paraList1.Add(new CallDBtools_MSDb.MSDbParam(1, "@seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));
                        paraList1.Add(new CallDBtools_MSDb.MSDbParam(1, "@dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));                        
                        paraList1.Add(new CallDBtools_MSDb.MSDbParam(1, "@qty", "VarChar", dt_oralce.Rows[i]["QTY"].ToString().Trim()));
                        paraList1.Add(new CallDBtools_MSDb.MSDbParam(1, "@memo", "VarChar", dt_oralce.Rows[i]["MEMO"].ToString().Trim()));
                        paraList1.Add(new CallDBtools_MSDb.MSDbParam(1, "@create_time", "VarChar", dt_oralce.Rows[i]["CREATE_TIME"].ToString().Trim()));
                        paraList1.Add(new CallDBtools_MSDb.MSDbParam(1, "@create_user", "VarChar", dt_oralce.Rows[i]["CREATE_USER"].ToString().Trim()));
                        paraList1.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_ip", "VarChar", dt_oralce.Rows[i]["UPDATE_IP"].ToString().Trim()));
                        paraList1.Add(new CallDBtools_MSDb.MSDbParam(1, "@flag", "VarChar", dt_oralce.Rows[i]["FLAG"].ToString().Trim()));
                        rowsAffected = callDBtools_msdb.CallExecSQL(cmdstr, paraList1, "msdb", ref msgDB);
                        l.lg("SelectOracle_PHputD_IntoMS_WBputD_05()", "rowsAffected=" + rowsAffected);
                        if (msgDB == "" && rowsAffected == 1)
                        {
                            int rowsAffected_oracle = -1;
                            sql_oracle = " update PH_PUT_D set STATUS='B', UPDATE_TIME=sysdate, UPDATE_USER='AUTO'  ";
                            sql_oracle += " where AGEN_NO = :agen_no and MMCODE = :mmcode and to_char(TXTDAY,'YYYY/MM/DD HH24:MI:SS') = :txtday and SEQ = :seq and DEPT = :dept ";
                            l.lg("SelectOracle_PHputD_IntoMS_WBputD_05()", "sql_oracle=" + sql_oracle);
                            List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":txtday", "VarChar", Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"]).ToString("yyyy/MM/dd HH:mm:ss").Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));                            
                            rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
                            l.lg("SelectOracle_PHputD_IntoMS_WBputD_05()", "rowsAffected_oracle=" + rowsAffected_oracle);
                            if (msg_oracle != "")
                            {
                                callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP4-update PH_PUT_D失敗:" + msg_oracle, "AUTO");
                            }
                        }
                        else
                        {
                            callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP4-insert WB_PUT_D失敗:" + msgDB, "AUTO");
                        }
                    }
                }
                else if(msg_oracle != "")
                    callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP4-取得PH_PUT_D失敗:" + msg_oracle, "AUTO");
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS002", "程式錯誤:4_");
            }
        }

        void f_06()
        {
            CallDBtools calldbtools = new CallDBtools();
            string ErrorStep = "Start" + Environment.NewLine;
            // -- oracle -- 
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            //String s_conn_oracle = calldbtools.SelectDB("oracle");
            //OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
            //if (conn_oracle.State == ConnectionState.Open)
            //    conn_oracle.Close();
            //conn_oracle.Open();
            //OracleTransaction transaction_oracle = conn_oracle.BeginTransaction();
            //List<string> transcmd_oracle = new List<string>();
            //List<CallDBtools_Oracle.OracleParam> listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();
            //int iTransOra = 0;

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
                string sqlStr_MSDB = "";
                sqlStr_MSDB += " select ";
                sqlStr_MSDB += " AGEN_NO, " + sBr; // 00.廠商碼 AGEN_NO(VARCHAR2,6)
                sqlStr_MSDB += " MMCODE, " + sBr; // 01.三總院內碼 MMCODE(VARCHAR2,13)
                sqlStr_MSDB += " convert(nvarchar(19), TXTDAY, 120) TXTDAY, " + sBr; // 02.交易日期 TXTDAY(DATE,)
                sqlStr_MSDB += " SEQ, " + sBr; // 03.交易流水號 SEQ(INTEGER,)
                sqlStr_MSDB += " DEPT, " + sBr; // 14.寄放地區(VARCHAR2,6)
                sqlStr_MSDB += " EXTYPE, " + sBr; // 04.異動類別 EXTYPE(VARCHAR2,2)
                sqlStr_MSDB += " QTY, " + sBr; // 05.異動數量 QTY(INTEGER,)
                sqlStr_MSDB += " MEMO, " + sBr; // 06.備註 MEMO(VARCHAR2,100)
                sqlStr_MSDB += " STATUS, " + sBr; // 07.狀態 STATUS(VARCHAR2,1)
                sqlStr_MSDB += " CREATE_TIME, " + sBr; // 08.建立日期 CREATE_TIME(DATE,)
                sqlStr_MSDB += " CREATE_USER, " + sBr; // 09.建立人員 CREATE_USER(VARCHAR2,10)
                sqlStr_MSDB += " UPDATE_TIME, " + sBr; // 10.異動日期 UPDATE_TIME(DATE,)
                sqlStr_MSDB += " UPDATE_USER, " + sBr; // 11.異動人員 UPDATE_USER(VARCHAR2,10)
                sqlStr_MSDB += " UPDATE_IP, " + sBr; // 12.異動IP UPDATE_IP(VARCHAR2,20)
                sqlStr_MSDB += " FLAG, " + sBr; // 13.狀態 FLAG(VARCHAR2,1)
                sqlStr_MSDB = sqlStr_MSDB.Substring(0, sqlStr_MSDB.Length - 4);
                sqlStr_MSDB += " from WB_PUT_D where STATUS = 'A' and EXTYPE in ('20') "; // 耗用
                l.lg("f_06()", "sqlStr_MSDB=" + sqlStr_MSDB);
                dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
                if (msg_MSDB == "") //取得 WB_PUT_D 無錯誤
                {
                    l.lg("f_06()", "dt_MSDB.Rows.Count=" + dt_MSDB.Rows.Count);
                    if (dt_MSDB.Rows.Count > 0) //WB_PUT_D 有資料
                    {
                        for (int i = 0; i < dt_MSDB.Rows.Count; i++)
                        {
                            msg_MSDB = "error";
                            //List<CallDBtools_MSDb.MSDbParam> paraListA = new List<CallDBtools_MSDb.MSDbParam>();
                            sqlStr_MSDB = "update WB_PUT_M set QTY=QTY-@qty, UPDATE_TIME = SYSDATETIME(), UPDATE_USER = 'AUTO' ";
                            sqlStr_MSDB += "  where AGEN_NO = @agen_no and MMCODE = @mmcode and DEPT = @dept ";
                            transcmd_msdb.Add(sqlStr_MSDB);
                            l.lg("f_06()", "sqlStr_MSDB=" + sqlStr_MSDB);
                            ++iTransMsdb;
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@qty", "int", dt_MSDB.Rows[i]["QTY"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@agen_no", "nvarchar", dt_MSDB.Rows[i]["AGEN_NO"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@mmcode", "nvarchar", dt_MSDB.Rows[i]["MMCODE"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@dept", "nvarchar", dt_MSDB.Rows[i]["DEPT"].ToString().Trim()));
                            //rowsAffected_msdb = callDBtools_msdb.CallExecSQL(sqlStr_MSDB, paraListA, "msdb", ref msg_MSDB);
                            //if (msg_MSDB != "")
                            //    callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP5-update WB_PUT_D.QTY失敗:" + msg_MSDB, "AUTO");


                            sqlStr_MSDB = "update WB_PUT_D set STATUS='B', FLAG='B', UPDATE_TIME = SYSDATETIME(), UPDATE_USER = 'AUTO' ";
                            sqlStr_MSDB += "where 1=1 ";
                            sqlStr_MSDB += "and AGEN_NO= @agen_no ";
                            sqlStr_MSDB += "and MMCODE= @mmcode ";
                            sqlStr_MSDB += "and convert(nvarchar(19), TXTDAY, 120)= @txtday ";
                            sqlStr_MSDB += "and SEQ= @seq ";
                            sqlStr_MSDB += "and DEPT= @dept ";
                            transcmd_msdb.Add(sqlStr_MSDB);
                            l.lg("f_06()", "sqlStr_MSDB=" + sqlStr_MSDB);
                            ++iTransMsdb;
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@agen_no", "nvarchar", dt_MSDB.Rows[i]["AGEN_NO"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@mmcode", "nvarchar", dt_MSDB.Rows[i]["MMCODE"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@txtday", "nvarchar", dt_MSDB.Rows[i]["TXTDAY"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@seq", "nvarchar", dt_MSDB.Rows[i]["SEQ"].ToString().Trim()));
                            listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTransMsdb, "@dept", "nvarchar", dt_MSDB.Rows[i]["DEPT"].ToString().Trim()));
                        }
                        rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
                                            transcmd_msdb,
                                            listParam_msdb,
                                            transaction_msdb,
                                            conn_msdb);
                        l.lg("f_06()", "rowsAffected_msdb=" + rowsAffected_msdb);


                        transaction_msdb.Commit();
                        conn_msdb.Close();
                        l.lg("f_06()", "transaction_msdb.Commit()");

                        ErrorStep += ",成功" + Environment.NewLine;
                    }
                }
                else //取得 WB_PUT_D 有錯誤，寫ERROR_LOG
                {
                    CallDBtools_Oracle callDBtools_oracle = new CallDBtools_Oracle();
                    callDBtools_oracle.I_ERROR_LOG("BHS002", "STEP5-取得WB_REPLY失敗:" + msg_MSDB, "AUTO");
                }
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP5 院外主檔WB_PUT_M.QTY失敗--" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                transaction_msdb.Rollback();
                conn_msdb.Close();

                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS002", "STEP5 院外主檔WB_PUT_M.QTY失敗--" + ex.Message);
                l.le("Transaction01()", "STEP5 院外主檔WB_PUT_M.QTY失敗--" + ex.Message);
            }
        } // 


        // 5_院內 Oracle PH_PUTTIME 外部 MSdb WB_PUTTIME 記錄資料更新時間
        private void Update_PH_WB_PutTime_07()
        {
            try
            {
                int rowsAffected_oracle = -1;
                string msg_oracle = "error", cmdstr = "", result ="";
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                cmdstr = "select count(*) from PH_PUTTIME ";
                l.lg("Update_PH_WB_PutTime_07()", "cmdstr=" + cmdstr);
                result = callDbtools_oralce.CallExecScalar(cmdstr, null, "oracle", ref msg_oracle);
                l.lg("Update_PH_WB_PutTime_07()", "result=" + result);
                if (result == "0") // 沒有資料，要 insert
                {
                    cmdstr = "insert into PH_PUTTIME(update_time) values(sysdate) ";
                    l.lg("Update_PH_WB_PutTime_07()", "cmdstr=" + cmdstr);
                    rowsAffected_oracle = callDbtools_oralce.CallExecSQL(cmdstr, null, "oracle", ref msg_oracle);
                    l.lg("Update_PH_WB_PutTime_07()", "rowsAffected_oracle=" + rowsAffected_oracle);
                }
                else //有資料，要 update 
                {
                    cmdstr = "update PH_PUTTIME set update_time = sysdate ";
                    l.lg("Update_PH_WB_PutTime_07()", "cmdstr=" + cmdstr);
                    rowsAffected_oracle = callDbtools_oralce.CallExecSQL(cmdstr, null, "oracle", ref msg_oracle);
                    l.lg("Update_PH_WB_PutTime_07()", "rowsAffected_oracle=" + rowsAffected_oracle);
                }
                if (msg_oracle != "" || rowsAffected_oracle == 0) 
                {
                    callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP5-更新 PH_PUTTIME 失敗:" + msg_oracle, "AUTO");
                }


                int rowsAffected_msdb = -1;
                string msg_msdb = "error";
                CallDBtools_MSDb callDbtools_msdb = new CallDBtools_MSDb();
                cmdstr = "select count(*) from WB_PUTTIME ";
                l.lg("Update_PH_WB_PutTime_07()", "cmdstr=" + cmdstr);
                result = callDbtools_msdb.CallExecScalar(cmdstr, null, "msdb", ref msg_msdb);
                l.lg("Update_PH_WB_PutTime_07()", "result=" + result);
                if (result == "0") // 沒有資料，要 insert
                {
                    cmdstr = "insert into WB_PUTTIME(update_time) values(SYSDATETIME()) ";
                    l.lg("Update_PH_WB_PutTime_07()", "cmdstr=" + cmdstr);
                    rowsAffected_msdb = callDbtools_msdb.CallExecSQL(cmdstr, null, "msdb", ref msg_msdb);
                    l.lg("Update_PH_WB_PutTime_07()", "rowsAffected_msdb=" + rowsAffected_msdb);
                }
                else //有資料，要 update 
                {
                    cmdstr = "update WB_PUTTIME set update_time = SYSDATETIME() ";
                    l.lg("Update_PH_WB_PutTime_07()", "cmdstr=" + cmdstr);
                    rowsAffected_msdb = callDbtools_msdb.CallExecSQL(cmdstr, null, "msdb", ref msg_msdb);
                    l.lg("Update_PH_WB_PutTime_07()", "rowsAffected_msdb=" + rowsAffected_msdb);
                }
                if (msg_msdb != "" || rowsAffected_msdb == 0)
                {
                    callDbtools_oralce.I_ERROR_LOG("BHS002", "STEP5-更新 WB_PUTTIME 失敗:" + msg_msdb, "AUTO");
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BHS002", "程式錯誤:5_");
            }
        }

    }
}

