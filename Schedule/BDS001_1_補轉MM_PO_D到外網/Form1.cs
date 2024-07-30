using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System;
using Tsghmm.Utility;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using JCLib.DB.Tool;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;

namespace BDS001_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //程式起始 BDS001_1-衛材/藥材訂單EMAIL發送排程 排程每天 7:00~22:00每30分鐘執行一次
        private void Form1_Load(object sender, EventArgs e)
        {
            Transfer();
            this.Close();
        }

        private void Transfer() {

            CallDBtools calldbtools = new CallDBtools();

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
            int iTransMsdb = 0;

            try
            {
                CallDBtools_MSDb callDBtools_msdb = new CallDBtools_MSDb();
                string msgDB = "error", sql_msdb = "";
                int rowsAffected = -1;
                sql_msdb = @"
                    select PO_NO from WB_MM_PO_M a
                     where not exists (select 1 from WB_MM_PO_D where po_no = a.po_no)
                ";
                DataTable dt_MSDB = new DataTable();
                dt_MSDB = callDBtools_msdb.CallOpenSQLReturnDT(sql_msdb, null, null, "msdb", "T1", ref msgDB);
                if (dt_MSDB.Rows.Count > 0) {
                    for (int i = 0; i < dt_MSDB.Rows.Count; i++) {
                        string po_no = dt_MSDB.Rows[i]["PO_NO"].ToString().Trim();

                        string msg_ORDB = "error";
                        CallDBtools_Oracle callDBtools_oracle = new CallDBtools_Oracle();
                        DataTable dt_ORDB = new DataTable();

                        string sql_or = @"
                    select a.PO_NO,a.MMCODE,a.PO_QTY,a.PO_PRICE,
                           a.M_PURUN,a.M_AGENLAB,a.PO_AMT,a.M_DISCPERC,a.DELI_QTY,
                            a.BW_SQTY,a.DELI_STATUS,a.CREATE_TIME,a.CREATE_USER,a.UPDATE_TIME,
                            a.UPDATE_USER,a.UPDATE_IP,a.MEMO,a.PR_NO,a.UNIT_SWAP,
                            b.MMNAME_C,b.MMNAME_E,b.WEXP_ID,a.DISC_CPRICE,
                            a.ISWILLING, a.DISCOUNT_QTY, a.DISC_COST_UPRICE
                      from MM_PO_D a, MI_MAST b
                      where a.MMCODE=b.MMCODE and  a.PO_NO = :po_no and a.STATUS<>'D'
                    ";
                        DataTable dt_oralce = new DataTable();
                        List<CallDBtools_Oracle.OracleParam> paraList2 = new List<CallDBtools_Oracle.OracleParam>();
                        paraList2.Add(new CallDBtools_Oracle.OracleParam(1, ":po_no", "VarChar", po_no));
                        dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_or, null, paraList2, "oracle", "T1", ref msg_ORDB);

                        if (msg_ORDB == "" && dt_oralce.Rows.Count > 0)
                        {
                            for (int j = 0; j < dt_oralce.Rows.Count; j++) {
                                sql_msdb = "insert into WB_MM_PO_D(PO_NO,MMCODE,PO_QTY,PO_PRICE, ";
                                sql_msdb += "                      M_PURUN,M_AGENLAB,PO_AMT,M_DISCPERC,DELI_QTY, ";
                                sql_msdb += "                      BW_SQTY,DELI_STATUS,CREATE_TIME,CREATE_USER,UPDATE_TIME, ";
                                sql_msdb += "                      UPDATE_USER,UPDATE_IP,MEMO,PR_NO,UNIT_SWAP, ";
                                sql_msdb += "                      MMNAME_C,MMNAME_E,WEXP_ID, DISC_CPRICE," +
                                    "                              ISWILLING, DISCOUNT_QTY, DISC_COST_UPRICE) ";
                                sql_msdb += "               values(@po_no,@mmcode,@po_qty,@po_price, ";
                                sql_msdb += "                      @m_purun,@m_agenlab,@po_amt,@m_discperc,@deli_qty, ";
                                sql_msdb += "                      @bw_sqty,@deli_status,@create_time,@create_user,@update_time, ";
                                sql_msdb += "                      @update_user,@update_ip,@memo,@pr_no,@unit_swap, ";
                                sql_msdb += "                      @mmname_c,@mmname_e,@wexp_id, @disc_cprice, ";
                                sql_msdb += "                      @iswilling,@discount_qty,@disc_cost_uprice ) ";
                                List<CallDBtools_MSDb.MSDbParam> paraList_msA = new List<CallDBtools_MSDb.MSDbParam>();
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_no", "VarChar", dt_oralce.Rows[j]["PO_NO"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmcode", "VarChar", dt_oralce.Rows[j]["MMCODE"].ToString().Trim()));
                                if (dt_oralce.Rows[j]["PO_QTY"].ToString().Trim() != "")
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_qty", "VarChar", dt_oralce.Rows[j]["PO_QTY"].ToString().Trim()));
                                else
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_qty", "VarChar", DBNull.Value));
                                if (dt_oralce.Rows[j]["PO_PRICE"].ToString().Trim() != "")
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_price", "VarChar", dt_oralce.Rows[j]["PO_PRICE"].ToString().Trim()));
                                else
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_price", "VarChar", DBNull.Value));

                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@m_purun", "VarChar", dt_oralce.Rows[j]["M_PURUN"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@m_agenlab", "VarChar", dt_oralce.Rows[j]["M_AGENLAB"].ToString().Trim()));
                                if (dt_oralce.Rows[j]["PO_AMT"].ToString().Trim() != "")
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_amt", "VarChar", dt_oralce.Rows[j]["PO_AMT"].ToString().Trim()));
                                else
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@po_amt", "VarChar", DBNull.Value));
                                if (dt_oralce.Rows[j]["M_DISCPERC"].ToString().Trim() != "")
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@m_discperc", "VarChar", dt_oralce.Rows[j]["M_DISCPERC"].ToString().Trim()));
                                else
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@m_discperc", "VarChar", DBNull.Value));
                                if (dt_oralce.Rows[j]["DELI_QTY"].ToString().Trim() != "")
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@deli_qty", "VarChar", dt_oralce.Rows[j]["DELI_QTY"].ToString().Trim()));
                                else
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@deli_qty", "VarChar", DBNull.Value));

                                if (dt_oralce.Rows[j]["BW_SQTY"].ToString().Trim() != "")
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@bw_sqty", "VarChar", dt_oralce.Rows[j]["BW_SQTY"].ToString().Trim()));
                                else
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@bw_sqty", "VarChar", DBNull.Value));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@deli_status", "VarChar", dt_oralce.Rows[j]["DELI_STATUS"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@create_time", "VarChar", dt_oralce.Rows[j]["CREATE_TIME"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@create_user", "VarChar", dt_oralce.Rows[j]["CREATE_USER"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_time", "VarChar", dt_oralce.Rows[j]["UPDATE_TIME"].ToString().Trim()));

                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_user", "VarChar", dt_oralce.Rows[j]["UPDATE_USER"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@update_ip", "VarChar", dt_oralce.Rows[j]["UPDATE_IP"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@memo", "VarChar", dt_oralce.Rows[j]["MEMO"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@pr_no", "VarChar", dt_oralce.Rows[j]["PR_NO"].ToString().Trim()));
                                if (dt_oralce.Rows[j]["UNIT_SWAP"].ToString().Trim() != "")
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@unit_swap", "VarChar", dt_oralce.Rows[j]["UNIT_SWAP"].ToString().Trim()));
                                else
                                    paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@unit_swap", "VarChar", DBNull.Value));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmname_c", "VarChar", dt_oralce.Rows[j]["MMNAME_C"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@mmname_e", "VarChar", dt_oralce.Rows[j]["MMNAME_E"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@wexp_id", "VarChar", dt_oralce.Rows[j]["WEXP_ID"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@disc_cprice", "VarChar", dt_oralce.Rows[j]["DISC_CPRICE"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@iswilling", "VarChar", dt_oralce.Rows[j]["ISWILLING"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@discount_qty", "VarChar", string.IsNullOrEmpty(dt_oralce.Rows[j]["DISCOUNT_QTY"].ToString().Trim()) ? "0" : dt_oralce.Rows[j]["DISCOUNT_QTY"].ToString().Trim()));
                                paraList_msA.Add(new CallDBtools_MSDb.MSDbParam(1, "@disc_cost_uprice", "VarChar", string.IsNullOrEmpty(dt_oralce.Rows[j]["DISC_COST_UPRICE"].ToString().Trim()) ? "0" : dt_oralce.Rows[j]["DISC_COST_UPRICE"].ToString().Trim()));
                                rowsAffected = callDBtools_msdb.CallExecSQL(sql_msdb, paraList_msA, "msdb", ref msgDB);
                                if (msgDB != "")
                                {
                                    callDbtools_oralce.I_ERROR_LOG("BDS001", "(衛材)(衛材FAX)(藥材)STEP9-新增WB_MM_PO_D失敗:" + msgDB, "AUTO");
                                }
                            }
                        }
                    }
                }
                conn_oracle.Close();
                conn_msdb.Close();

            }
            catch (Exception e){
                Console.ReadLine();
            }
        }

    }
}
