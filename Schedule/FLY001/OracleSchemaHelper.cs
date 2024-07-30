using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.SqlClient;
using JCLib.DB.Tool;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace GenTools
{
    class OracleSchemaHelper
    {
        //// 2_更新院內 Oracle PH_PUT_M
        //private void UpdateOracle_PHputM()
        //{
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
        //    int iTransOra = 0;

        //    try
        //    {
        //        string msg_oracle = "error";
        //        //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
        //        DataTable dt_oralce = new DataTable();
        //        string sql_oracle = " select ";
        //        sql_oracle += " FBNO, " + sBr;
        //        sql_oracle += " MMCODE, " + sBr;
        //        sql_oracle += " SEQ, " + sBr;
        //        sql_oracle += " to_char(TXTDAY,'yyyy/mm/dd hh24:mi:ss') TXTDAY, " + sBr;
        //        sql_oracle += " AGEN_NO, " + sBr;
        //        sql_oracle += " AIR, " + sBr;
        //        sql_oracle += " to_char(CHK_DATE,'yyyy/mm/dd hh24:mi:ss') CHK_DATE, " + sBr;
        //        sql_oracle += " DEPT, " + sBr;
        //        sql_oracle += " to_char(EXP_DATE,'yyyy/mm/dd hh24:mi:ss') EXP_DATE, " + sBr;
        //        sql_oracle += " EXTYPE, " + sBr;
        //        sql_oracle += " to_char(INPUT_DATE,'yyyy/mm/dd hh24:mi:ss') INPUT_DATE, " + sBr;
        //        sql_oracle += " MAT, " + sBr;
        //        sql_oracle += " MEMO, " + sBr;
        //        sql_oracle += " SBNO, " + sBr;
        //        sql_oracle += " STATUS, " + sBr;
        //        sql_oracle += " XSIZE, " + sBr;
        //        sql_oracle += " to_char(CREATE_TIME,'yyyy/mm/dd hh24:mi:ss') CREATE_TIME, " + sBr;
        //        sql_oracle += " CREATE_USER, " + sBr;
        //        sql_oracle += " UPDATE_IP, " + sBr;
        //        sql_oracle += " to_char(UPDATE_TIME,'yyyy/mm/dd hh24:mi:ss') UPDATE_TIME, " + sBr;
        //        sql_oracle += " UPDATE_USER, " + sBr;
        //        sql_oracle += " FLAG, " + sBr;
        //        sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
        //        sql_oracle += " from PH_AIRHIS where 1=1 " + sBr;
        //        sql_oracle += " and FLAG= 'A' " + sBr;
        //        dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, "TXTDAY, MMCODE", null, "oracle", "T1", ref msg_oracle);
        //        if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
        //        {
        //            int rowsAffected_oracle = -1;
        //            for (int i = 0; i < dt_oralce.Rows.Count; i++)
        //            {
        //                if (dt_oralce.Rows[i]["EXTYPE"].ToString().Trim() == "GO") // 取走
        //                {
        //                    sql_oracle = " delete from PH_AIRST where 1=1 " + sBr;
        //                    sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商碼(VARCHAR2,Y)
        //                    sql_oracle += " and FBNO = :fbno " + sBr;// 02.瓶號(VARCHAR2,Y)
        //                    sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.三總院內碼(VARCHAR2,Y)
        //                    transcmd_oracle.Add(sql_oracle);

        //                    ++iTransOra;
        //                    //List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));// 01.廠商碼(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));// 02.瓶號(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));// 03.三總院內碼(VARCHAR2,Y)
        //                    //rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
        //                    //if (msg_oracle == "")
        //                    //{
        //                    //rowsAffected_oracle = -1;
        //                    sql_oracle = " update PH_AIRHIS set FLAG='B', UPDATE_TIME=sysdate, UPDATE_USER='AUTO' where 1=1 ";
        //                    sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商碼(VARCHAR2,Y)
        //                    sql_oracle += " and FBNO = :fbno " + sBr;// 02.瓶號(VARCHAR2,Y)
        //                    sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.三總院內碼(VARCHAR2,Y)
        //                    sql_oracle += " and SEQ = :seq " + sBr;// 04.交易流水號(INTEGER,Y)
        //                    sql_oracle += " and to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') = :txtday " + sBr;// 05.更換日期(DATE,Y)
        //                    transcmd_oracle.Add(sql_oracle);
        //                    //paraList = new List<CallDBtools_Oracle.OracleParam>();
        //                    ++iTransOra;
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));// 01.廠商碼(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));// 02.瓶號(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));// 03.三總院內碼(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));// 04.交易流水號(INTEGER,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":txtday", "VarChar", (dt_oralce.Rows[i]["TXTDAY"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 05.更換日期(DATE,Y)
        //                    //    rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);

        //                    //    if (msg_oracle != "")
        //                    //    {
        //                    //        callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP2-update PH_AIRHIS (EXTYPE=GO 取走) 失敗:" + msg_oracle, "AUTO");
        //                    //    }
        //                    //}
        //                    //else
        //                    //{
        //                    //    callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP2-delete PH_AIRST (EXTYPE=GO 取走) 失敗:" + msg_oracle, "AUTO");
        //                    //}
        //                }
        //                else if (dt_oralce.Rows[i]["EXTYPE"].ToString().Trim() == "GI") // 換入
        //                {
        //                    sql_oracle = " insert into PH_AIRST ( " + sBr;
        //                    sql_oracle += " AGEN_NO, " + sBr; // 01.廠商碼(VARCHAR2,6)
        //                    sql_oracle += " FBNO, " + sBr; // 02.瓶號(VARCHAR2,20)
        //                    sql_oracle += " MMCODE, " + sBr; // 03.三總院內碼(VARCHAR2,13)
        //                    sql_oracle += " SEQ, " + sBr; // 04.交易流水號(INTEGER,)
        //                    sql_oracle += " TXTDAY, " + sBr; // 05.更換日期(DATE,)
        //                    sql_oracle += " AIR, " + sBr; // 06.氣體(VARCHAR2,100)
        //                    sql_oracle += " CHK_DATE, " + sBr; // 07.檢驗日期(DATE,)
        //                    sql_oracle += " DEPT, " + sBr; // 08.放置位置(VARCHAR2,6)
        //                    sql_oracle += " EXP_DATE, " + sBr; // 09.保存期限(DATE,)
        //                    sql_oracle += " INPUT_DATE, " + sBr; // 10.灌氣日期(DATE,)
        //                    sql_oracle += " MAT, " + sBr; // 11.材質類別(VARCHAR2,20)
        //                    sql_oracle += " MEMO, " + sBr; // 12.備註(VARCHAR2,50)
        //                    sql_oracle += " SBNO, " + sBr; // 13.鋼號(VARCHAR2,20)
        //                    sql_oracle += " XSIZE, " + sBr; // 14.鋼瓶尺寸(VARCHAR2,50)
        //                    sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
        //                    sql_oracle += " ) values ( " + sBr;
        //                    sql_oracle += " :agen_no, " + sBr; // 01.廠商碼(VARCHAR2,Y)
        //                    sql_oracle += " :fbno, " + sBr; // 02.瓶號(VARCHAR2,Y)
        //                    sql_oracle += " :mmcode, " + sBr; // 03.三總院內碼(VARCHAR2,Y)
        //                    sql_oracle += " :seq, " + sBr; // 04.交易流水號(INTEGER,Y)
        //                    sql_oracle += " to_date(:txtday, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 05.更換日期(DATE,Y)
        //                    sql_oracle += " :air, " + sBr; // 06.氣體(VARCHAR2,Y)
        //                    sql_oracle += " to_date(:chk_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 07.檢驗日期(DATE,Y)
        //                    sql_oracle += " :dept, " + sBr; // 08.放置位置(VARCHAR2,)
        //                    sql_oracle += " to_date(:exp_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 09.保存期限(DATE,Y)
        //                    sql_oracle += " to_date(:input_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr; // 10.灌氣日期(DATE,Y)
        //                    sql_oracle += " :mat, " + sBr; // 11.材質類別(VARCHAR2,Y)
        //                    sql_oracle += " :memo, " + sBr; // 12.備註(VARCHAR2,)
        //                    sql_oracle += " :sbno, " + sBr; // 13.鋼號(VARCHAR2,)
        //                    sql_oracle += " :xsize, " + sBr; // 14.鋼瓶尺寸(VARCHAR2,Y)
        //                    sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
        //                    sql_oracle += " ) " + sBr;
        //                    transcmd_oracle.Add(sql_oracle);

        //                    //List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
        //                    //int iTransSeq = 1;
        //                    ++iTransOra;
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim())); // 01.廠商碼(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim())); // 02.瓶號(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim())); // 03.三總院內碼(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim())); // 04.交易流水號(INTEGER,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":txtday", "VarChar", (dt_oralce.Rows[i]["TXTDAY"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 05.更換日期(DATE,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":air", "VarChar", dt_oralce.Rows[i]["AIR"].ToString().Trim())); // 06.氣體(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":chk_date", "VarChar", (dt_oralce.Rows[i]["CHK_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["CHK_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 07.檢驗日期(DATE,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim())); // 08.放置位置(VARCHAR2,)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":exp_date", "VarChar", (dt_oralce.Rows[i]["EXP_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["EXP_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 09.保存期限(DATE,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":input_date", "VarChar", (dt_oralce.Rows[i]["INPUT_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["INPUT_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : "")); // 10.灌氣日期(DATE,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mat", "VarChar", dt_oralce.Rows[i]["MAT"].ToString().Trim())); // 11.材質類別(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":memo", "VarChar", dt_oralce.Rows[i]["MEMO"].ToString().Trim())); // 12.備註(VARCHAR2,)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":sbno", "VarChar", dt_oralce.Rows[i]["SBNO"].ToString().Trim())); // 13.鋼號(VARCHAR2,)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":xsize", "VarChar", dt_oralce.Rows[i]["XSIZE"].ToString().Trim())); // 14.鋼瓶尺寸(VARCHAR2,Y)

        //                    //rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, listParam_oracle, "oracle", ref msg_oracle);
        //                    //if (msg_oracle == "")
        //                    //{
        //                    //rowsAffected_oracle = -1;
        //                    sql_oracle = " update PH_AIRHIS set FLAG='B', UPDATE_TIME=sysdate, UPDATE_USER='AUTO' where 1=1 ";
        //                    sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商碼(VARCHAR2,Y)
        //                    sql_oracle += " and FBNO = :fbno " + sBr;// 02.瓶號(VARCHAR2,Y)
        //                    sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.三總院內碼(VARCHAR2,Y)
        //                    sql_oracle += " and SEQ = :seq " + sBr;// 04.交易流水號(INTEGER,Y)
        //                    sql_oracle += " and to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') = :txtday " + sBr;// 05.更換日期(DATE,Y)
        //                    transcmd_oracle.Add(sql_oracle);

        //                    //paraList = new List<CallDBtools_Oracle.OracleParam>();
        //                    ++iTransOra;
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));// 01.廠商碼(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));// 02.瓶號(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));// 03.三總院內碼(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));// 04.交易流水號(INTEGER,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":txtday", "VarChar", (dt_oralce.Rows[i]["TXTDAY"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 05.更換日期(DATE,Y)

        //                    //rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
        //                    //if (msg_oracle != "")
        //                    //{
        //                    //    callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP2-update PH_AIRHIS (EXTYPE=GI 換入) 失敗:" + msg_oracle, "AUTO");
        //                    //}
        //                    //}
        //                    //else
        //                    //{
        //                    //    callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP2-insert PH_AIRST (EXTYPE=GI 換入) 失敗:" + msg_oracle, "AUTO");
        //                    //}
        //                }
        //                else if (
        //                    dt_oralce.Rows[i]["EXTYPE"].ToString().Trim() == "CH" ||
        //                    dt_oralce.Rows[i]["EXTYPE"].ToString().Trim() == "UP"
        //                ) // 修改
        //                {
        //                    sql_oracle = " update PH_AIRST set " + sBr;
        //                    sql_oracle += " AIR = :air, " + sBr;// 06.氣體(VARCHAR2,Y)
        //                    sql_oracle += " CHK_DATE = to_date(:chk_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr;// 07.檢驗日期(DATE,Y)
        //                    sql_oracle += " DEPT = :dept, " + sBr;// 08.放置位置(VARCHAR2,)
        //                    sql_oracle += " EXP_DATE = to_date(:exp_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr;// 09.保存期限(DATE,Y)
        //                    sql_oracle += " INPUT_DATE = to_date(:input_date, 'yyyy/mm/dd hh24:mi:ss'), " + sBr;// 10.灌氣日期(DATE,Y)
        //                    sql_oracle += " MAT = :mat, " + sBr;// 11.材質類別(VARCHAR2,Y)
        //                    sql_oracle += " MEMO = :memo, " + sBr;// 12.備註(VARCHAR2,)
        //                    sql_oracle += " SBNO = :sbno, " + sBr;// 13.鋼號(VARCHAR2,)
        //                    sql_oracle += " XSIZE = :xsize, " + sBr;// 14.鋼瓶尺寸(VARCHAR2,Y)
        //                    sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
        //                    sql_oracle += " where 1=1 " + sBr;
        //                    sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 05.廠商碼(VARCHAR2,Y)
        //                    sql_oracle += " and FBNO = :fbno " + sBr;// 01.瓶號(VARCHAR2,Y)
        //                    sql_oracle += " and MMCODE = :mmcode " + sBr;// 02.三總院內碼(VARCHAR2,Y)
        //                    transcmd_oracle.Add(sql_oracle);

        //                    //List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
        //                    ++iTransOra;
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":air", "VarChar", dt_oralce.Rows[i]["AIR"].ToString().Trim()));// 06.氣體(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":chk_date", "VarChar", (dt_oralce.Rows[i]["CHK_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["CHK_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 07.檢驗日期(DATE,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));// 08.放置位置(VARCHAR2,)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":exp_date", "VarChar", (dt_oralce.Rows[i]["EXP_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["EXP_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 09.保存期限(DATE,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":input_date", "VarChar", (dt_oralce.Rows[i]["INPUT_DATE"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["INPUT_DATE"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 10.灌氣日期(DATE,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mat", "VarChar", dt_oralce.Rows[i]["MAT"].ToString().Trim()));// 11.材質類別(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":memo", "VarChar", dt_oralce.Rows[i]["MEMO"].ToString().Trim()));// 12.備註(VARCHAR2,)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":sbno", "VarChar", dt_oralce.Rows[i]["SBNO"].ToString().Trim()));// 13.鋼號(VARCHAR2,)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":xsize", "VarChar", dt_oralce.Rows[i]["XSIZE"].ToString().Trim()));// 14.鋼瓶尺寸(VARCHAR2,Y)
        //                    // -- key
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));// 01.廠商碼(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));// 02.瓶號(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));// 03.三總院內碼(VARCHAR2,Y)

        //                    // rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, listParam_oracle, "oracle", ref msg_oracle);
        //                    //if (msg_oracle == "")
        //                    //{
        //                    //rowsAffected_oracle = -1;
        //                    sql_oracle = " update PH_AIRHIS set FLAG='B', UPDATE_TIME=sysdate, UPDATE_USER='AUTO' where 1=1 ";
        //                    sql_oracle += " and AGEN_NO = :agen_no " + sBr;// 01.廠商碼(VARCHAR2,Y)
        //                    sql_oracle += " and FBNO = :fbno " + sBr;// 02.瓶號(VARCHAR2,Y)
        //                    sql_oracle += " and MMCODE = :mmcode " + sBr;// 03.三總院內碼(VARCHAR2,Y)
        //                    sql_oracle += " and SEQ = :seq " + sBr;// 04.交易流水號(INTEGER,Y)
        //                    sql_oracle += " and to_char(TXTDAY, 'yyyy/mm/dd hh24:mi:ss') = :txtday " + sBr;// 05.更換日期(DATE,Y)
        //                    transcmd_oracle.Add(sql_oracle);

        //                    ++iTransOra;
        //                    //listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));// 01.廠商碼(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));// 02.瓶號(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));// 03.三總院內碼(VARCHAR2,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));// 04.交易流水號(INTEGER,Y)
        //                    listParam_oracle.Add(new CallDBtools_Oracle.OracleParam(iTransOra, ":txtday", "VarChar", (dt_oralce.Rows[i]["TXTDAY"].ToString().Length > 0) ? Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"].ToString()).ToString("yyyy/MM/dd HH:mm:ss") : ""));// 05.更換日期(DATE,Y)
        //                    // rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, listParam_oracle, "oracle", ref msg_oracle);
        //                    //    if (msg_oracle != "")
        //                    //    {
        //                    //        callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP2-update PH_AIRHIS (EXTYPE=GH 修改) 失敗:" + msg_oracle, "AUTO");
        //                    //    }
        //                    //}
        //                    //else
        //                    //{
        //                    //    callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP2-insert PH_AIRST (EXTYPE=GH 修改) 失敗:" + msg_oracle, "AUTO");
        //                    //}
        //                }
        //            } // end of for (int i = 0; i < dt_oralce.Rows.Count; i++)
        //            rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
        //                    transcmd_oracle,
        //                    listParam_oracle,
        //                    transaction_oracle,
        //                    conn_oracle);

        //            transaction_oracle.Commit();
        //            conn_oracle.Close();
        //            ErrorStep += ",成功" + Environment.NewLine;
        //        }
        //        else if (msg_oracle != "")
        //        {
        //            callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP2-取得PH_AIRHIS失敗:" + msg_oracle, "AUTO");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP2-程式錯誤:2_:" + ex.Message, "AUTO");

        //        ErrorStep += ",失敗" + Environment.NewLine;
        //        ErrorStep += ex.ToString() + Environment.NewLine;
        //        transaction_oracle.Rollback();
        //        conn_oracle.Close();

        //        CallDBtools callDBtools = new CallDBtools();
        //        callDBtools.WriteExceptionLog(ex.Message, "FLY001", "程式錯誤:2_");

        //    }
        //}




        //private void Update_PH_WB_PutTime()
        //{
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
        //    int iTranOra = 0;


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
        //    int iTranMsdb = 0;

        //    try
        //    {
        //        int rowsAffected_oracle = -1;
        //        string msg_oracle = "error", cmdstr = "", result = "";
        //        string msg_MSDB = "error";
        //        int rowsAffected_msdb = -1;
        //        //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
        //        //CallDBtools_MSDb callDbtools_msdb = new CallDBtools_MSDb();


        //        cmdstr = "select count(*) from PH_AIRTIME ";
        //        result = callDbtools_oralce.CallExecScalar(cmdstr, null, "oracle", ref msg_oracle);
        //        if (result == "0") // 沒有資料，要 insert
        //        {
        //            cmdstr = "insert into PH_AIRTIME(update_time) values(sysdate) ";
        //            transcmd_oracle.Add(cmdstr);
        //            ++iTranOra;
        //            //rowsAffected_oracle = callDbtools_oralce.CallExecSQL(cmdstr, null, "oracle", ref msg_oracle);
        //        }
        //        else //有資料，要 update 
        //        {
        //            cmdstr = "update PH_AIRTIME set update_time = sysdate ";
        //            transcmd_oracle.Add(cmdstr);
        //            ++iTranOra;
        //            //rowsAffected_oracle = callDbtools_oralce.CallExecSQL(cmdstr, null, "oracle", ref msg_oracle);
        //        }
        //        //if (msg_oracle != "" || rowsAffected_oracle == 0)
        //        //{
        //        //    callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP3-更新 PH_AIRTIME 失敗:" + msg_oracle, "AUTO");
        //        //}

        //        // 02 刪除外網WB_AIRST
        //        cmdstr = "delete from WB_AIRST ";
        //        transcmd_msdb.Add(cmdstr);
        //        ++iTranMsdb;
        //        //rowsAffected_msdb = callDbtools_msdb.CallExecSQL(cmdstr, null, "msdb", ref msg_MSDB);
        //        //if (msg_MSDB != "")
        //        //{
        //        //    callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP3-truncate WB_AIRST 失敗:" + msg_oracle, "AUTO");
        //        //}

        //        // 03 copy PH_AIRST 到 WB_AIRST
        //        //string msg_oracle = "error";
        //        //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
        //        DataTable dt_oralce = new DataTable();
        //        string sql_oracle = " select ";
        //        sql_oracle += " AGEN_NO, " + sBr;
        //        sql_oracle += " FBNO, " + sBr;
        //        sql_oracle += " MMCODE, " + sBr;
        //        sql_oracle += " SEQ, " + sBr;
        //        sql_oracle += " to_char(TXTDAY,'yyyy/mm/dd hh24:mi:ss') TXTDAY , " + sBr;
        //        sql_oracle += " AIR, " + sBr;
        //        sql_oracle += " to_char(CHK_DATE,'yyyy/mm/dd hh24:mi:ss') CHK_DATE , " + sBr;
        //        sql_oracle += " DEPT, " + sBr;
        //        sql_oracle += " to_char(EXP_DATE,'yyyy/mm/dd hh24:mi:ss') EXP_DATE , " + sBr;
        //        sql_oracle += " to_char(INPUT_DATE,'yyyy/mm/dd hh24:mi:ss') INPUT_DATE , " + sBr;
        //        sql_oracle += " MAT, " + sBr;
        //        sql_oracle += " MEMO, " + sBr;
        //        sql_oracle += " SBNO, " + sBr;
        //        sql_oracle += " XSIZE, " + sBr;
        //        sql_oracle = sql_oracle.Substring(0, sql_oracle.Length - 4);
        //        sql_oracle += " from PH_AIRST where 1=1 ";
        //        dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
        //        if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
        //        {
        //            //CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
        //            string msgDB = "error", sql_msdb = "", ms_resStr = "";
        //            //int rowsAffected = -1;
        //            for (int i = 0; i < dt_oralce.Rows.Count; i++)
        //            {
        //                // rowsAffected = -1;
        //                sql_msdb = "insert into WB_AIRST ( " + sBr;
        //                sql_msdb += " AGEN_NO, " + sBr; // 01.廠商碼(VARCHAR2,6)
        //                sql_msdb += " FBNO, " + sBr; // 02.瓶號(VARCHAR2,20)
        //                sql_msdb += " MMCODE, " + sBr; // 03.三總院內碼(VARCHAR2,13)
        //                sql_msdb += " SEQ, " + sBr; // 04.交易流水號(INTEGER,)
        //                sql_msdb += " TXTDAY, " + sBr; // 05.更換日期(DATE,)
        //                sql_msdb += " AIR, " + sBr; // 06.氣體(VARCHAR2,100)
        //                sql_msdb += " CHK_DATE, " + sBr; // 07.檢驗日期(DATE,)
        //                sql_msdb += " DEPT, " + sBr; // 08.放置位置(VARCHAR2,6)
        //                sql_msdb += " EXP_DATE, " + sBr; // 09.保存期限(DATE,)
        //                sql_msdb += " INPUT_DATE, " + sBr; // 10.灌氣日期(DATE,)
        //                sql_msdb += " MAT, " + sBr; // 11.材質類別(VARCHAR2,20)
        //                sql_msdb += " MEMO, " + sBr; // 12.備註(VARCHAR2,50)
        //                sql_msdb += " SBNO, " + sBr; // 13.鋼號(VARCHAR2,20)
        //                sql_msdb += " XSIZE, " + sBr; // 14.鋼瓶尺寸(VARCHAR2,50)
        //                sql_msdb = sql_msdb.Substring(0, sql_msdb.Length - 4);
        //                sql_msdb += " ) values ( " + sBr;
        //                sql_msdb += " @agen_no, " + sBr; // 01.廠商碼(VARCHAR2,6)
        //                sql_msdb += " @fbno, " + sBr; // 02.瓶號(VARCHAR2,20)
        //                sql_msdb += " @mmcode, " + sBr; // 03.三總院內碼(VARCHAR2,13)
        //                sql_msdb += " @seq, " + sBr; // 04.交易流水號(INTEGER,)
        //                sql_msdb += " @txtday, " + sBr; // 05.更換日期(DATE,)
        //                sql_msdb += " @air, " + sBr; // 06.氣體(VARCHAR2,100)
        //                sql_msdb += " @chk_date, " + sBr; // 07.檢驗日期(DATE,)
        //                sql_msdb += " @dept, " + sBr; // 08.放置位置(VARCHAR2,6)
        //                sql_msdb += " @exp_date, " + sBr; // 09.保存期限(DATE,)
        //                sql_msdb += " @input_date, " + sBr; // 10.灌氣日期(DATE,)
        //                sql_msdb += " @mat, " + sBr; // 11.材質類別(VARCHAR2,20)
        //                sql_msdb += " @memo, " + sBr; // 12.備註(VARCHAR2,50)
        //                sql_msdb += " @sbno, " + sBr; // 13.鋼號(VARCHAR2,20)
        //                sql_msdb += " @xsize, " + sBr; // 14.鋼瓶尺寸(VARCHAR2,50)
        //                sql_msdb = sql_msdb.Substring(0, sql_msdb.Length - 4);
        //                sql_msdb += " )" + sBr;
        //                sql_msdb.Replace(sBr, "");
        //                transcmd_msdb.Add(sql_msdb);

        //                //List<CallDBtools_MSDb.MSDbParam> paraListA = new List<CallDBtools_MSDb.MSDbParam>();
        //                ++iTranMsdb;
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@fbno", "VarChar", dt_oralce.Rows[i]["FBNO"].ToString().Trim()));
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@mmcode", "VarChar", dt_oralce.Rows[i]["MMCODE"].ToString().Trim()));
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@seq", "VarChar", dt_oralce.Rows[i]["SEQ"].ToString().Trim()));
        //                String txtday = "";
        //                if (dt_oralce.Rows[i]["TXTDAY"].ToString().Length > 0)
        //                    txtday = Convert.ToDateTime(dt_oralce.Rows[i]["TXTDAY"]).ToString("yyyy-MM-dd HH:mm:ss").Trim();
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@txtday", "VarChar", txtday));
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@air", "VarChar", dt_oralce.Rows[i]["AIR"].ToString().Trim()));
        //                String chk_date = "";
        //                if (dt_oralce.Rows[i]["CHK_DATE"].ToString().Length > 0)
        //                    chk_date = Convert.ToDateTime(dt_oralce.Rows[i]["CHK_DATE"]).ToString("yyyy-MM-dd HH:mm:ss").Trim();
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@chk_date", "VarChar", chk_date));
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@dept", "VarChar", dt_oralce.Rows[i]["DEPT"].ToString().Trim()));
        //                String exp_date = "";
        //                if (dt_oralce.Rows[i]["EXP_DATE"].ToString().Length > 0)
        //                    exp_date = Convert.ToDateTime(dt_oralce.Rows[i]["EXP_DATE"]).ToString("yyyy-MM-dd HH:mm:ss").Trim();
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@exp_date", "VarChar", exp_date));
        //                String input_date = "";
        //                if (dt_oralce.Rows[i]["INPUT_DATE"].ToString().Length > 0)
        //                    input_date = Convert.ToDateTime(dt_oralce.Rows[i]["INPUT_DATE"]).ToString("yyyy-MM-dd HH:mm:ss").Trim();
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@input_date", "VarChar", input_date));
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@mat", "VarChar", dt_oralce.Rows[i]["MAT"].ToString().Trim()));
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@memo", "VarChar", dt_oralce.Rows[i]["MEMO"].ToString().Trim()));
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@sbno", "VarChar", dt_oralce.Rows[i]["SBNO"].ToString().Trim()));
        //                listParam_msdb.Add(new CallDBtools_MSDb.MSDbParam(iTranMsdb, "@xsize", "VarChar", dt_oralce.Rows[i]["XSIZE"].ToString().Trim()));

        //                //rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, listParam_msdb, "msdb", ref msgDB);
        //                //if (msgDB != "")
        //                //{
        //                //    callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP3-insert WB_AIRST 失敗:" + msgDB, "AUTO");
        //                //}
        //            }
        //        }
        //        else if (msg_oracle != "")
        //        {
        //            callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP3-取得PH_AIRST失敗:" + msg_oracle, "AUTO");
        //        }


        //        // 04 
        //        cmdstr = "select count(*) from WB_AIRTIME ";
        //        result = callDbtools_msdb.CallExecScalar(cmdstr, null, "msdb", ref msg_MSDB);
        //        if (result == "0") // 沒有資料，要 insert
        //        {
        //            cmdstr = "insert into WB_AIRTIME(update_time) values(SYSDATETIME()) ";
        //            transcmd_msdb.Add(cmdstr);
        //            ++iTranMsdb;
        //            //rowsAffected_msdb = callDbtools_msdb.CallExecSQL(cmdstr, null, "msdb", ref msg_MSDB);
        //        }
        //        else //有資料，要 update 
        //        {
        //            cmdstr = "update WB_AIRTIME set update_time = SYSDATETIME() ";
        //            transcmd_msdb.Add(cmdstr);
        //            ++iTranMsdb;
        //            //rowsAffected_msdb = callDbtools_msdb.CallExecSQL(cmdstr, null, "msdb", ref msg_MSDB);
        //        }
        //        //if (msg_MSDB != "" || rowsAffected_msdb == 0)
        //        //{
        //        //    callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP3-更新 WB_AIRTIME 失敗:" + msg_oracle, "AUTO");
        //        //}

        //        rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
        //            transcmd_oracle,
        //            listParam_oracle,
        //            transaction_oracle,
        //            conn_oracle);

        //        rowsAffected_msdb = callDbtools_msdb.CallExecSQLByTransaction(
        //                transcmd_msdb,
        //                listParam_msdb,
        //                transaction_msdb,
        //                conn_msdb);

        //        transaction_oracle.Commit();
        //        conn_oracle.Close();

        //        transaction_msdb.Commit();
        //        conn_msdb.Close();

        //        ErrorStep += ",成功" + Environment.NewLine;
        //    }
        //    catch (Exception ex)
        //    {
        //        callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP3 失敗:" + ex.Message, "AUTO");

        //        ErrorStep += ",失敗" + Environment.NewLine;
        //        ErrorStep += ex.ToString() + Environment.NewLine;
        //        transaction_oracle.Rollback();
        //        conn_oracle.Close();
        //        transaction_msdb.Rollback();
        //        conn_msdb.Close();

        //        CallDBtools callDBtools = new CallDBtools();
        //        callDBtools.WriteExceptionLog(ex.Message, "FLY001", "程式錯誤:3_");
        //    }
        //} // 

        //程式起始 BHS002-寄售廠商寄放量資料轉檔 排程每天 0:30~24:00每30分鐘執行一次
        //private void Form1_Load(object sender, System.EventArgs e)
        //{
        //    l.lg("Form1_Load()", "");
        //    String sCurPath = AppDomain.CurrentDomain.BaseDirectory;
        //    String sDbStatusFilePath = sCurPath + "\\db_status.html";
        //    if (File.Exists(sDbStatusFilePath))
        //        File.Delete(sDbStatusFilePath);
        //    try
        //    {
        //        // 單元測試();
        //        l.writeToStringFile(sDbStatusFilePath, get資料庫現況());
        //        l.lg("Form1_Load()", "get資料庫現況()");

        //        // 1 01.外網 MSdb WB_AIRHIS -> 院內 Oracle PH_AIRHIS, 02.update WB_AIRHIS.STATUS='B'已處理
        //        SelectMS_WBputD_IntoOracle_PHputD();
        //        l.writeToStringFile(sDbStatusFilePath, get資料庫現況());
        //        l.lg("Form1_Load()", "getSelectMS_WBputD_IntoOracle_PHputD()");

        //        // 2 01.更新院內 Oracle PH_AIRHIS -> PH_AIRST(GO取走,GI換入,CH修改)
        //        UpdateOracle_PHputM();
        //        l.writeToStringFile(sDbStatusFilePath, get資料庫現況());
        //        l.lg("Form1_Load()", "UpdateOracle_PHputM()");

        //        // 3 01.更新PH_AIRTIME.update_time=sysdate, 02.刪除WB_AIRST, 03.複製PH_AIRST->WB_AIRST, 04 更新更新WB_AIRTIME.update_time=SYSDATETIME()
        //        Update_PH_WB_PutTime();
        //        l.writeToStringFile(sDbStatusFilePath, get資料庫現況());
        //        l.lg("Form1_Load()", "Update_PH_WB_PutTime()");
        //    }
        //    catch (Exception ex)
        //    {
        //        l.le("Form1_Load()", ex.Message);
        //    }
        //    this.Close();
        //}

        void a()
        {
            //CallDBtools calldbtools = new CallDBtools();
            //String sql = "";
            //string ErrorStep = "Start" + Environment.NewLine;


            //// -- oracle -- 
            //CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            //String s_conn_oracle = calldbtools.SelectDB("oracle");
            //OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
            //if (conn_oracle.State == ConnectionState.Open)
            //    conn_oracle.Close();
            //conn_oracle.Open();
            //OracleTransaction transaction_oracle = conn_oracle.BeginTransaction();
            //List<string> transcmd_oracle = new List<string>();
            //List<CallDBtools_Oracle.OracleParam> listParam_oracle = new List<CallDBtools_Oracle.OracleParam>();
            //int iTransOra = 0;

            //// -- msdb -- 
            //CallDBtools_MSDb callDbtools_msdb = new CallDBtools_MSDb();
            //String s_conn_msdb = calldbtools.SelectDB("msdb");
            //SqlConnection conn_msdb = new SqlConnection(s_conn_msdb);
            //if (conn_msdb.State == ConnectionState.Open)
            //    conn_msdb.Close();
            //conn_msdb.Open();
            //SqlTransaction transaction_msdb = conn_msdb.BeginTransaction();
            //List<string> transcmd_msdb = new List<string>();
            //List<CallDBtools_MSDb.MSDbParam> listParam_msdb = new List<CallDBtools_MSDb.MSDbParam>();
            //int iTransMsdb = 0;
            //try
            //{
            //    //string msg_MSDB = "error";
            //    //CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
            //    //DataTable dt_MSDB = new DataTable();
            //    //string sqlStr_MSDB = " select "; 
            //    //dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sqlStr_MSDB, null, null, "msdb", "T1", ref msg_MSDB);
            //    //if (msg_MSDB == "")
            //    //{
            //    //    if (dt_MSDB.Rows.Count > 0) //WB_PUT_D 有資料
            //    //    {
            //    //    }
            //    //}
            //    // -----------------------------------


            //    string msg_oracle = "error";
            //    DataTable dt_oralce = new DataTable();
            //    string sql_oracle = " select * from " + s_table_name  + " where 1=2 " + sBr;
            //    dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
            //    if (msg_oracle == "" 
            //        // && dt_oralce.Rows.Count > 0
            //    ) //資料抓取沒有錯誤 且有資料
            //    {
            //        int rowsAffected_oracle = -1;

            //        String s = "";
            //        for (int i = 0; i < dt_oralce.Columns.Count; i++)
            //        {
            //            DataColumn c = (DataColumn)dt_oralce.Columns[i];
            //            //中文表名	table	索引	中文	name	type	size	isNotNull	iskey	isIndex	預設值	值域	備註
            //            s += c.ColumnName + sT;
            //            s += c.DataType + sT;

            //            //c.AllowDBNull 

            //            s += "" + sT; // 01.中文表名
            //            s += s_table_name + sT; // 02.table
            //            s += i.ToString() + sT; // 03.索引(Ordinal)
            //            s += "" + sT; // 04.中文
            //            s += c.ColumnName + sT; // 04.name
            //            s += c.DataType.ToString() + sT; // 06.type
            //            s += c.MaxLength + sT; // 07.size
            //            if (c.AllowDBNull)
            //                s += "Y" + sT; // 08.isNotNull
            //            else
            //                s += "" + sT; // 08.isNotNull
            //            if (c.AutoIncrement)
            //                s += c.ColumnName + sT; // 09.iskey
            //            else
            //                s += "" + sT;
            //            s += "" + sT; // 10.isIndex
            //            s += c.DefaultValue + sT; // 11.預設值
            //            s += "" + sT; // 12.值域
            //            s += "" + sT; // 13.備註
            //            s += sBr;
            //        }



            //        //for (int i = 0; i < dt_oralce.Rows.Count; i++)
            //        //{
            //        //    if (dt_oralce.Rows[i]["EXTYPE"].ToString().Trim() == "GO") // 取走
            //        //    {

            //        //        // dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));                                                                                                                                                                                                                                         //}
            //        //    } // end if 
            //        //} // end of for (int i = 0; i < dt_oralce.Rows.Count; i++)

            //        //rowsAffected_oracle = callDbtools_oralce.CallExecSQLByTransaction(
            //        //        transcmd_oracle,
            //        //        listParam_oracle,
            //        //        transaction_oracle,
            //        //        conn_oracle);

            //        //transaction_oracle.Commit();
            //        conn_oracle.Close();
            //        ErrorStep += ",成功" + Environment.NewLine;
            //    }
            //    else if (msg_oracle != "")
            //    {
            //        callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP2-取得PH_AIRHIS失敗:" + msg_oracle, "AUTO");
            //    }

            //    // ---------------
            //}
            //catch (Exception ex)
            //{
            //    callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP1 失敗:" + ex.Message, "AUTO");

            //    ErrorStep += ",失敗" + Environment.NewLine;
            //    ErrorStep += ex.ToString() + Environment.NewLine;
            //    transaction_oracle.Rollback();
            //    conn_oracle.Close();
            //    transaction_msdb.Rollback();
            //    conn_msdb.Close();

            //    CallDBtools callDBtools = new CallDBtools();
            //    callDBtools.WriteExceptionLog(ex.Message, "FLY001", "程式錯誤:1_");
            //}
        }


        L l = new L("GenTools.GenTools");
        static String sBr = "\r\n";
        static String sT = "\t";

        /// <summary>
        /// 從資料庫取得DB Schema
        /// </summary>
        /// <param name="s_table_name">傳入Table資料</param>
        /// <returns>整理後的Table Schema字串</returns>
        public static String readDataTableSchema(
            String s_table_name
        )
        {
            // select column_name from user_tab_columns where table_name = '" + selTbl + "' order by column_id
            CallDBtools calldbtools = new CallDBtools();
            string ErrorStep = "Start" + Environment.NewLine;

            //// -- oracle -- 
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            String s_conn_oracle = calldbtools.SelectDB("oracle");
            OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
            if (conn_oracle.State == ConnectionState.Open)
                conn_oracle.Close();
            conn_oracle.Open();
            String s = "";
            try
            {
                string msg_oracle = "error";
                DataTable dt_oralce = new DataTable();
                string sql_oracle = " select * from " + s_table_name + " where 1=2 " + sBr;
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == ""
                // && dt_oralce.Rows.Count > 0
                ) //資料抓取沒有錯誤 且有資料
                {
                    for (int i = 0; i < dt_oralce.Columns.Count; i++)
                    {
                        DataColumn c = (DataColumn)dt_oralce.Columns[i];
                        //中文表名	table	索引	中文	name	type	size	isNotNull	iskey	isIndex	預設值	值域	備註
                        s += "" + sT; // 01.中文表名
                        s += s_table_name + sT; // 02.table
                        s += i.ToString() + sT; // 03.索引(Ordinal)
                        s += "" + sT; // 04.中文
                        s += c.ColumnName + sT; // 04.name
                        s += c.DataType.ToString() + sT; // 06.type
                        s += c.MaxLength + sT; // 07.size
                        if (c.AllowDBNull)
                            s += "Y" + sT; // 08.isNotNull
                        else
                            s += "" + sT; // 08.isNotNull
                        if (c.AutoIncrement)
                            s += c.ColumnName + sT; // 09.iskey
                        else
                            s += "" + sT;
                        s += "" + sT; // 10.isIndex
                        s += c.DefaultValue + sT; // 11.預設值
                        s += "" + sT; // 12.值域
                        s += "" + sT; // 13.備註
                        s += sBr;
                    }
                    conn_oracle.Close();
                    ErrorStep += ",成功" + Environment.NewLine;
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP2-取得PH_AIRHIS失敗:" + msg_oracle, "AUTO");
                }

                // ---------------
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("FLY001", "STEP1 失敗:" + ex.Message, "AUTO");

                ErrorStep += ",失敗" + Environment.NewLine;
                ErrorStep += ex.ToString() + Environment.NewLine;
                conn_oracle.Close();

                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "FLY001", "程式錯誤:1_");
            }
            return s;
        }


        static String s_primary_key_on_alter_table = "PRIMARY KEY";
        static String s_create_index  = "CREATE INDEX ";

        public String 功能名稱; // 01.
        public String 中文表名; // 02.
        public String table; // 03.
        public String 索引; // 04.
        public String 中文; // 05.
        public String name; // 06.
        public String type; // 07.
        public String size; // 08.
        public String isNotNull; // 09.
        public String iskey; // 10.
        public String isIndex; // 11.
        public String 預設值; // 12.
        public String 值域; // 13.
        public String 備註; // 15.

        static String getDefaultValue(String s)
        {
            // APPQTY         NUMBER(11,3)                   DEFAULT 0                     NOT NULL,
            // GTAPL_RESON    VARCHAR2(2 CHAR)               DEFAULT NULL,
            // FLAG         VARCHAR2(1 CHAR)                 DEFAULT 'A'
            String s_pre_key = "DEFAULT ";
            int iStartIdx = s.IndexOf(s_pre_key);
            String s_end_key = ",";
            int iEndIdx = s.LastIndexOf(s_end_key);
            if (iStartIdx > -1 && iEndIdx > -1)
            {
                s = s.Substring(iStartIdx + s_pre_key.Length, iEndIdx - iStartIdx - s_pre_key.Length);
                // 0                     NOT NULL,
                Regex pattern = new Regex(" +");
                s = pattern.Replace(s, " ");  // 去除多個空格，變成1個
                // 0 NOT NULL,
                String[] sAry = s.Split(' ');
                return sAry[0]; //  
            }
            else if (iStartIdx > -1)
            {   // FLAG         VARCHAR2(1 CHAR)                 DEFAULT 'A'
                s = s.Substring(iStartIdx + s_pre_key.Length);
                return s;
            }
            return "";
        } // 

        static String getIsNotNull(String s)
        {   // ACKQTY         NUMBER(11,3)                   DEFAULT 0                     NOT NULL,

            String s_key = "NOT NULL";
            int iStartIdx = s.IndexOf(s_key);
            if (iStartIdx > -1)
            {
                return "Y";
            }
            return "";
        } // 

        static String get中文(String s, String e)
        {
            s = s.Replace("\r", "").Replace("\n", "");
            //                     COMMENT ON COLUMN INAD.LOGIN.VIEWOTHER IS '(保留未用)';
            Regex r = new Regex(@"^COMMENT ON COLUMN \w+\.[\w|_]+\.[\w|_]+ IS \'([\w|_| ]+).*");
            Match m = r.Match(s);
            if (m.Groups.Count >= 2)
            {
                return m.Groups[1].ToString();
            }
            return e;
        }

        static String get備註(String s)
        {
            s = s.Replace("\r", "").Replace("\n", "");
            //                     COMMENT ON COLUMN INAD.LOGIN.VIEWOTHER IS '(保留未用)';
            //                     COMMENT ON COLUMN INAD.LOGIN.AUTHORITY2 IS '角色代碼  0:測試, 1:一般, 2: Duty';
            //                     COMMENT ON COLUMN INAD.LOGIN.ALARM_3G_PAGE IS '是否 Paging(Y:yes,N:no)';
            Regex r = new Regex(@"^COMMENT ON COLUMN \w+\.[\w|_]+\.[\w|_]+ IS \'[\w| ]*([\(|\,|\ ]+[\w|\ |\,|\/|\.|\-|\(|\)|\:|\、|\、|\。|\d]+[\)]?)\'\;$");
            Match m = r.Match(s);
            if (m.Groups.Count >= 2)
            {
                return m.Groups[1].ToString();
            }
            return "";
        }



        public static String readOraTableScript(String scripts)
        {
            ArrayList al = getAlOracleSchemaHelpers(scripts);
            String s = "";
            foreach (OracleSchemaHelper v in al)
            {
                s += v.功能名稱 + "\t"; // 01.
                s += v.中文表名 + "\t"; // 02.
                s += v.table + "\t"; // 03.
                s += v.索引 + "\t"; // 04.
                s += v.中文 + "\t"; // 05.
                s += v.name + "\t"; // 06.
                s += v.type + "\t"; // 07.
                s += v.size + "\t"; // 08.
                s += v.isNotNull + "\t"; // 09.
                s += v.iskey + "\t"; // 10.
                s += v.isIndex + "\t"; // 11.
                s += v.預設值 + "\t"; // 12.
                s += v.值域 + "\t"; // 13.
                s += v.備註 + "\n"; // 14.
            }
            return s;
        }

        static String getColType(String s)
        {
            // VARCHAR2(15)
            // DATE
            String s_key = "(";
            int iStartIdx = s.IndexOf(s_key);
            if (iStartIdx > -1)
            {
                return s.Substring(0, iStartIdx);
            }
            else
                return s.Replace(",","");
        }

        static String getColSize(String s)
        {
            // VARCHAR2(15)
            // DATE
            // NUMBER(11,3)
            // VARCHAR2(15
            Regex r = new Regex(@"^\w+\((\d+)\)?\,?$");
            Match match = r.Match(s);
            if (match.Success)
            {
                if (match.Groups.Count>=2)
                    return match.Groups[1].ToString();
            }
            return "";
        }

        static List<String> getParenthesesKeys(String s)
        {
            //  (PO_NO) 或  (PO_NO,A,B,C)
            Regex r = new Regex(@"^\(([ ?|\w|\,]+)\)$");
            Match match = r.Match(s);
            List<String> lstColKeys = new List<string>();
            if (match.Success)
            {
                if (match.Groups.Count >= 2)
                {
                    String sCommaColNm = match.Groups[1].ToString();
                    String[] sAry = sCommaColNm.Split(',');
                    foreach (String eS in sAry)
                    {
                        lstColKeys.Add(eS.Trim());
                    }
                    
                }
            }
            return lstColKeys;
        }


        public static ArrayList getAlOracleSchemaHelpers(String scripts)
        {
            scripts = scripts.Replace("\r", "");
            String[] sBody = scripts.Split('\n');
            String s_table_name = "";       // 英文表格名稱
            String s_tw_table_name = "";    // 中文表格名稱
            ArrayList al = new ArrayList(); // 存OracleSchemaHelper的陣列資料
            int i_col_idx = 0;              // 欄位順序

            bool b_startReadColumns = false;
            bool b_startReadPrimaryKey = false;
            bool b_startReadIndex = false;
            bool b_startReadComment = false;
            String s_comment = "";
            foreach (String l in sBody)
            {
                if (new Regex(@"CREATE TABLE \w+\.([\w|_]+)").Match(l).Success)    //"CREATE TABLE MMSADM.TABLE_NAME", CREATE TABLE INAD.ALARM_3G_PAGING_FUNCTION
                {
                    Match m = new Regex(@"CREATE TABLE \w+\.([\w|_]+)").Match(l);
                    if (m.Groups.Count > 1)
                    {
                        s_table_name = m.Groups[1].Value.ToString();
                        b_startReadColumns = true;
                    }
                }
                else if (b_startReadColumns)
                {
                    String s = l.Trim();
                    // 解決掉 CHAR),
                    s = s.Replace(" CHAR)", ")");

                    if (s.IndexOf(")") == 0)
                    {
                        b_startReadColumns = false; // 找到結尾，不做工
                    }
                    else if (s.IndexOf("(") == 0)
                    {
                    }
                    else
                    {   // 讀取每個columns
                        //  PO_NO        VARCHAR2(15 CHAR),
                        Regex pattern = new Regex(" +");
                        s = pattern.Replace(s, " ");  // 去除多個空格，變成1個
                        String[] sAry = s.Split(' ');
                        String s_col_name = sAry[0];
                        if (s_col_name.ToLower().Equals("CONTRACNO".ToLower()))
                        {
                            String sss = "";
                            sss += "";
                        }
                        String s_col_type = getColType(sAry[1]); // VARCHAR2(15)
                        String s_col_size = getColSize(sAry[1]); // VARCHAR2(15)
                        String s_default_val = getDefaultValue(l);
                        String s_is_not_null = getIsNotNull(l); // ACKQTY         NUMBER(11,3)                   DEFAULT 0                     NOT NULL,

                        OracleSchemaHelper v = new OracleSchemaHelper();
                        v.功能名稱 = ""; // 01.
                        v.中文表名 = s_tw_table_name; // 02.
                        v.table = s_table_name; // 03.
                        v.索引 = i_col_idx++.ToString(); // 04.
                        v.中文 = s_col_name; // 05.
                        v.name = s_col_name; // 06.
                        v.type = s_col_type; // 07.
                        v.size = s_col_size; // 08.
                        v.isNotNull = s_is_not_null; // 09.
                        v.iskey = ""; // 10.
                        v.isIndex = ""; // 11.
                        v.預設值 = s_default_val; // 12.
                        v.值域 = ""; // 13.
                        v.備註 = ""; // 14.
                        al.Add(v);
                    }
                }
                else if (new Regex(@"COMMENT ON TABLE \w+\.([\w|_]+)").Match(l).Success) // "COMMENT ON TABLE MMSADM.table_name"
                {   // 找到表格名稱
                    Match m = new Regex(@"COMMENT ON TABLE \w+\.([\w|_]+)").Match(l);
                    if (m.Groups.Count > 1)
                    {
                        s_tw_table_name = m.Groups[1].Value.ToString();
                    }
                }
                else if (new Regex(@"COMMENT ON COLUMN \w+\.([\w|_]+)").Match(l).Success) // "COMMENT ON COLUMN MMSADM."
                {
                    //COMMENT ON COLUMN MMSADM.MM_PO_M.PR_DEPT IS '申購單位(責任中心)';
                    b_startReadComment = true;
                    s_comment = l;
                }
                else if (b_startReadComment && l.Length == 0)
                {   // 解析Comment資料
                    Match m = new Regex(@"^COMMENT ON COLUMN \w+\." + @s_table_name + @"." + @"([\w|_]+).*$").Match(s_comment); // "COMMENT ON COLUMN MMSADM."
                    if (m.Success)
                    {
                        if (m.Groups.Count > 1)
                        {
                            String s_col_name = m.Groups[1].Value.ToString();
                            foreach (OracleSchemaHelper v in al)
                            {
                                v.中文表名 = s_tw_table_name;
                                if (v.name.ToLower().Equals(s_col_name.ToLower()))
                                {
                                    v.中文 = get中文(s_comment, v.name);
                                    v.備註 = get備註(s_comment);
                                    break;
                                }
                            }
                        }
                    }
                    b_startReadComment = false; // 關閉解析                    
                }
                else if (b_startReadComment)
                {
                    s_comment += l;
                }                
                else if (l.IndexOf(s_primary_key_on_alter_table) > -1)
                {   // 找到主鍵設定
                    b_startReadPrimaryKey = true;
                }
                else if (b_startReadPrimaryKey)
                {   // 分析主鍵設定
                    List<String> lstPrimaryKey = getParenthesesKeys(l.Trim());
                    foreach (OracleSchemaHelper eV in al)
                    {
                        foreach (String eK in lstPrimaryKey)
                        {
                            if (eV.name.ToLower().Equals(eK.ToLower()))
                                eV.iskey = "Y";
                        }
                    }
                    b_startReadPrimaryKey = false;
                }
                else if (l.IndexOf(s_create_index) > -1)
                {   // 找到索引設定
                    b_startReadIndex = true;
                }
                else if (b_startReadIndex)
                {   // 分析索引設定
                    List<String> lstIndexKey = getParenthesesKeys(l.Trim());
                    foreach (OracleSchemaHelper eV in al)
                    {
                        foreach (String eK in lstIndexKey)
                        {
                            if (eV.name.ToLower().Equals(eK.ToLower()))
                                eV.isIndex = "Y";
                        }
                    }
                    b_startReadIndex = false;
                }
            }
            return al;
        } // 


        public static DataTable query(
            String sql_oracle
        )
        {
            CallDBtools calldbtools = new CallDBtools();
            CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
            String s_conn_oracle = calldbtools.SelectDB("oracle");
            OracleConnection conn_oracle = new OracleConnection(s_conn_oracle);
            if (conn_oracle.State == ConnectionState.Open)
                conn_oracle.Close();
            conn_oracle.Open();
            String s = "";
            DataTable dt_oralce = new DataTable();
            try
            {
                string msg_oracle = "error";
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == ""
                // && dt_oralce.Rows.Count > 0
                ) //資料抓取沒有錯誤 且有資料
                {
                    conn_oracle.Close();
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("FLY001", "讀取資料庫失敗:\r\nSQL:" + sql_oracle + "\r\nException:" + msg_oracle, "AUTO");
                }
            }
            catch (Exception ex)
            {
                callDbtools_oralce.I_ERROR_LOG("FLY001", "讀取資料庫失敗:\r\nSQL:" + sql_oracle + "\r\nException:" + ex.Message, "AUTO");
                conn_oracle.Close();

                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "FLY001", "SQL:" + sql_oracle + "\r\nException:" + ex.Message);
            }
            return dt_oralce;
        } // 



    } // ec
} // en
