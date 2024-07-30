using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System;
//using Tsghmm.Utility;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using JCLib.DB.Tool;

namespace BDS006
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //程式起始 BDS006-更新外部廠商資料，增加排程同步外部廠商資料
        //每天將 PH_VENDER copy 到外部 table UR_ID_VENDER
        //以 UR_ID_VENDER[AGEN_NAMEC] 更新UR_ID[UNA], 不存在者INSERT
        //UR_ID_VENDER[AGEN_NO] = UR_ID[TUSER] 
        //UR_ID_VENDER[AGEN_NAMEC] = UR_ID[UNA]
        private void Form1_Load(object sender, EventArgs e)
        {
            //將 PH_VENDER copy 到外部 table UR_ID_VENDER
            CopyToUR_ID_VENDER();
            //更新UR_ID[UNA];
            UpdateUNA();
            //不存在廠商新增UR_ID
            InsertUR_ID();
            //設定密碼
            SetupPW();
            this.Close();
        }
        //每天將 PH_VENDER copy 到外部 table UR_ID_VENDER
        private void CopyToUR_ID_VENDER()
        {
            try
            {
                // -- oracle -- 
                string msg_oracle = "error";
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_oralce = new DataTable();

                // -- msdb -- 
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                string msgDB = "error", sql_msdb = "";
                int rowsAffected = -1;

                //先 truncate 外部 table UR_ID_VENDER
                sql_msdb = " truncate table UR_ID_VENDER ";
                rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, null, "msdb", ref msgDB);
                if (msgDB != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BDS006", "BDS006-truncate UR_ID_VENDER失敗:" + msgDB, "AUTO");
                }

                string sql_oracle = " select agen_no, agen_namec from PH_VENDER ";
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    List<CallDBtools_MSDb.MSDbParam> paraList_msA = new List<CallDBtools_MSDb.MSDbParam>();
                    for (int i = 0; i < dt_oralce.Rows.Count; i++) 
                    {
                        sql_msdb = "insert into UR_ID_VENDER(agen_no, agen_namec, update_time) ";
                        sql_msdb += "       values(@agen_no, @agen_namec, getDate()) ";
                        paraList_msA.Clear();
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@agen_no", "VarChar", dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                        paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@agen_namec", "VarChar", dt_oralce.Rows[i]["AGEN_NAMEC"].ToString().Trim()));
                        rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, paraList_msA, "msdb", ref msgDB);
                        if (msgDB != "")
                        {
                            callDbtools_oralce.I_ERROR_LOG("BDS006", "BDS006-insert into UR_IN_VENDER 失敗:" + msgDB, "AUTO");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS006", "BDS006排程程式錯誤 - CopyToUR_ID_VENDER()。");
            }
        }
        private void UpdateUNA()
        {
            try
            {
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                string msgDB = "error", sql_msdb = "";
                int rowsAffected = -1;

                sql_msdb = " update UR_ID set UR_ID.una=UR_ID_VENDER.agen_namec, update_time=getDate(), update_user='AUTO' ";
                sql_msdb += " from UR_ID_VENDER ";
                sql_msdb += " where UR_ID_VENDER.agen_no=UR_ID.tuser ";
                sql_msdb += "   and UR_ID_VENDER.agen_namec<>UR_ID.una ";
                rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, null, "msdb", ref msgDB);
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS006", "BDS006排程程式 UpdateUNA()錯誤。");
            }
        }
        private void InsertUR_ID()
        {
            try
            {
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                string msgDB = "error", sql_msdb = "";
                int rowsAffected = -1;

                sql_msdb = "  insert into UR_ID(tuser, una, pa, fl, create_time, update_user)  ";
                sql_msdb += " select agen_no, agen_namec, agen_no, '1', getDate(), 'AUTO' ";
                sql_msdb += " from  UR_ID_VENDER ";
                sql_msdb += " where  agen_no not in (select tuser from UR_ID)";
                rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, null, "msdb", ref msgDB);
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS006", "BDS006排程程式 InsertUR_ID()錯誤。");
            }
        }
        public void SetupPW()
        {
            DataTable dt_msdb = new DataTable();
            CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
            string msgDB = "error", sql_msdb = "";
            sql_msdb = @" SELECT TUSER, PA FROM UR_ID WHERE SL IS NULL ";
            dt_msdb = callDBtools_msdb.CallOpenSQLReturnDT(sql_msdb, null, null, "msdb", "T1", ref msgDB);
            try
            {
                if (msgDB == "" && dt_msdb.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    List<CallDBtools_MSDb.MSDbParam> paraList_msA = new List<CallDBtools_MSDb.MSDbParam>();
                    for (int i = 0; i < dt_msdb.Rows.Count; i++)
                    {
                        UpdatePassword(dt_msdb.Rows[i]["TUSER"].ToString().Trim(), dt_msdb.Rows[i]["PA"].ToString().Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS006", "BDS006排程程式 SetupPW()失敗。");
            }
        }
        public int UpdatePassword(string tuser, string pwd)
        {
            DataTable dt_msdb = new DataTable();
            CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
            string msgDB = "error", sql_msdb = "";
            int rowsAffected = -1;
            var salt = JCLib.Encrypt.GetSalt();
            var hashPwd = JCLib.Encrypt.GetHash(pwd, salt);
            List<CallDBtools_MSDb.MSDbParam> paraList_msA = new List<CallDBtools_MSDb.MSDbParam>();
            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@NEW_PWD", "VarChar", hashPwd));
            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@SLT", "VarChar", salt));
            paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@TUSER", "VarChar", tuser));
            sql_msdb = @"UPDATE UR_ID SET PA=@NEW_PWD, SL=@SLT WHERE TUSER=@TUSER ";
            rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, paraList_msA, "msdb", ref msgDB);
            return rowsAffected;
        }
    }
}
