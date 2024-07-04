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

namespace SetAvgPrice
{
    public partial class Form1 : Form
    {
        LogConteoller logController = new LogConteoller("SetAvgPrice", "SetAvgPrice");

        public Form1()
        {
            InitializeComponent();
        }

        //程式起始 SetAvgPrice
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

            string MMCODE = string.Empty, M_STOREID = string.Empty;
            string M_CONTID = string.Empty, E_SOURCECODE = string.Empty;

            int rowsAffected = 0;
            double DISC_UPRICE = 0, CONT_PRICE = 0, UPRICE = 0, DISC_CPRICE = 0 ;
            double P_INVQTY = 0, INQTY = 0, INCOST = 0, P_AVG_PRICE = 0, AVG_PEICE = 0;

            try
            {
                logController.AddLogs("1.查出符合條件的院內碼");
                // 1.查出符合條件的院內碼
                string msg_ORDB = "error";
                string sql = string.Empty;
                sql = @"
                    select distinct MMCODE 
                      from MM_PO_INREC
                     where 1=1
                       and iswilling = '是'
                       and po_qty >= discount_qty
                       and twn_yyymm(accountdate) = '11008'
                ";
                DataTable dt_oralce = new DataTable();
                List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql, null, paraList, "oracle", "T1", ref msg_ORDB);
                logController.AddLogs(string.Format("共{0}比", dt_oralce.Rows.Count));
                if (msg_ORDB == "" && dt_oralce.Rows.Count > 0)
                {
                    DataTable mi_mast_dt = new DataTable(); // MI_MAST
                    DataTable p_invqty_dt = new DataTable(); // 上期結存
                    DataTable p_avg_price_dt = new DataTable(); //上期平均單價
                    DataTable inqty_dt = new DataTable();      //  進貨量
                    DataTable incost_dt = new DataTable();      //  進貨總金額
                    int iTransOra = 0;

                    for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    {
                        
                        MMCODE = dt_oralce.Rows[i]["MMCODE"].ToString().Trim();

                        logController.AddLogs(string.Format("第 {0}比，院內碼 = {1} ", i+1, MMCODE));

                        logController.AddLogs(string.Format("取得MI_MAST"));
                        // 2.取得MI_MAST
                        sql = @"
                        SELECT MMCODE,DISC_UPRICE,M_CONTPRICE CONT_PRICE,
                               UPRICE,DISC_CPRICE,M_STOREID,M_CONTID,E_SOURCECODE
                          FROM MI_MAST
                         where mmcode = :mmcode
                        ";
                        paraList = new List<CallDBtools_Oracle.OracleParam>();
                        paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", MMCODE));
                        mi_mast_dt = callDbtools_oralce.CallOpenSQLReturnDT(sql, null, paraList, "oracle", "T1", ref msg_ORDB);
                        if (msg_ORDB == "" && mi_mast_dt.Rows.Count > 0) {
                            DISC_UPRICE = (string.IsNullOrEmpty(mi_mast_dt.Rows[0]["DISC_UPRICE"].ToString().Trim()) || mi_mast_dt.Rows[0]["DISC_UPRICE"].ToString().Trim() == "null") ?
                                            0 : double.Parse(mi_mast_dt.Rows[0]["DISC_UPRICE"].ToString().Trim());
                            CONT_PRICE = (string.IsNullOrEmpty(mi_mast_dt.Rows[0]["CONT_PRICE"].ToString().Trim()) || mi_mast_dt.Rows[0]["CONT_PRICE"].ToString().Trim() == "null") ?
                                            0 : double.Parse(mi_mast_dt.Rows[0]["CONT_PRICE"].ToString().Trim());
                            UPRICE = (string.IsNullOrEmpty(mi_mast_dt.Rows[0]["UPRICE"].ToString().Trim()) || mi_mast_dt.Rows[0]["UPRICE"].ToString().Trim() == "null") ?
                                            0 : double.Parse(mi_mast_dt.Rows[0]["UPRICE"].ToString().Trim());
                            DISC_CPRICE = (string.IsNullOrEmpty(mi_mast_dt.Rows[0]["DISC_CPRICE"].ToString().Trim()) || mi_mast_dt.Rows[0]["DISC_CPRICE"].ToString().Trim() == "null") ?
                                           0 : double.Parse(mi_mast_dt.Rows[0]["DISC_CPRICE"].ToString().Trim());
                            M_STOREID = mi_mast_dt.Rows[0]["M_STOREID"].ToString().Trim();
                            M_CONTID = mi_mast_dt.Rows[0]["M_CONTID"].ToString().Trim();
                            E_SOURCECODE = mi_mast_dt.Rows[0]["E_SOURCECODE"].ToString().Trim();

                            logController.AddLogs(string.Format("DISC_UPRICE = {0}", DISC_UPRICE));
                            logController.AddLogs(string.Format("CONT_PRICE = {0}", CONT_PRICE));
                            logController.AddLogs(string.Format("UPRICE = {0}", UPRICE));
                            logController.AddLogs(string.Format("DISC_CPRICE = {0}", DISC_CPRICE));
                            logController.AddLogs(string.Format("M_STOREID = {0}", M_STOREID));
                            logController.AddLogs(string.Format("M_CONTID = {0}", M_CONTID));
                            logController.AddLogs(string.Format("E_SOURCECODE = {0}", E_SOURCECODE));
                        }

                        logController.AddLogs(string.Format("取得上期結存量，若查無資料則設為0"));
                        // 3. 取得上期結存量，若查無資料則設為0
                        sql = @"
                        SELECT SUM(NVL(
                                (CASE
                                 WHEN GET_MAT_CLSID(A.MMCODE)='3' THEN
                                  (CASE A.WH_NO WHEN '560000' THEN INV_QTY ELSE 0 END)
                                 ELSE
                                  INV_QTY
                                END),0)) P_INVQTY
                          FROM MI_WINVMON A
                         WHERE DATA_YM='11007' AND MMCODE=:mmcode
                               AND EXISTS
                               (SELECT 1 FROM MI_WHMAST WHERE WH_NO=A.WH_NO AND WH_KIND IN ('0','1') AND WH_GRADE IN ('1','2','3','4'))
                        ";
                        paraList = new List<CallDBtools_Oracle.OracleParam>();
                        paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", MMCODE));
                        p_invqty_dt = callDbtools_oralce.CallOpenSQLReturnDT(sql, null, paraList, "oracle", "T1", ref msg_ORDB);
                        if (msg_ORDB == "" && p_invqty_dt.Rows.Count > 0) {
                            P_INVQTY = (string.IsNullOrEmpty(p_invqty_dt.Rows[0]["P_INVQTY"].ToString().Trim()) || p_invqty_dt.Rows[0]["P_INVQTY"].ToString().Trim() == "null") ?
                                            0 : double.Parse(p_invqty_dt.Rows[0]["P_INVQTY"].ToString().Trim());

                            logController.AddLogs(string.Format("P_INVQTY = {0}", P_INVQTY));
                        }

                        logController.AddLogs(string.Format("取得上期平均單價"));
                        // 4. 取得上期平均單價與軍品價，若查無資料則設為0
                        sql = @"
                        SELECT NVL(AVG_PRICE,0) P_AVG_PRICE,NVL(MIL_PRICE,0) P_MIL_PRICE
                          FROM MI_WHCOST
                         WHERE DATA_YM='11007' AND MMCODE=:mmcode
                        ";
                        paraList = new List<CallDBtools_Oracle.OracleParam>();
                        paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", MMCODE));
                        p_avg_price_dt = callDbtools_oralce.CallOpenSQLReturnDT(sql, null, paraList, "oracle", "T1", ref msg_ORDB);
                        if (msg_ORDB == "" && p_avg_price_dt.Rows.Count > 0)
                        {
                            P_AVG_PRICE = (string.IsNullOrEmpty(p_avg_price_dt.Rows[0]["P_AVG_PRICE"].ToString().Trim()) || p_avg_price_dt.Rows[0]["P_AVG_PRICE"].ToString().Trim() == "null") ?
                                            0 : double.Parse(p_avg_price_dt.Rows[0]["P_AVG_PRICE"].ToString().Trim());

                            logController.AddLogs(string.Format("P_AVG_PRICE = {0}", P_AVG_PRICE));
                        }

                        logController.AddLogs(string.Format("取得進貨量", P_AVG_PRICE));
                        // 5. 取得進貨量，若查無資料則設為0
                        sql = @"
                        SELECT SUM(NVL(APL_INQTY,0)) INQTY
                          FROM MI_WINVMON A
                         WHERE DATA_YM='11008' AND MMCODE=:mmcode
                               AND EXISTS
                               (SELECT 1 FROM MI_WHMAST WHERE WH_NO=A.WH_NO AND WH_KIND IN ('0','1') AND WH_GRADE IN ('1'))
                        ";
                        paraList = new List<CallDBtools_Oracle.OracleParam>();
                        paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", MMCODE));
                        inqty_dt = callDbtools_oralce.CallOpenSQLReturnDT(sql, null, paraList, "oracle", "T1", ref msg_ORDB);
                        if (msg_ORDB == "" && inqty_dt.Rows.Count > 0)
                        {
                            INQTY = (string.IsNullOrEmpty(inqty_dt.Rows[0]["INQTY"].ToString().Trim()) || inqty_dt.Rows[0]["INQTY"].ToString().Trim() == "null") ?
                                            0 : double.Parse(inqty_dt.Rows[0]["INQTY"].ToString().Trim());
                            logController.AddLogs(string.Format("INQTY = {0}", INQTY));
                        }

                        logController.AddLogs(string.Format("取得本期進貨總金額", INQTY));
                        // 6.取得本期進貨總金額
                        sql = @"
                        select MMCODE_INCOST('11008', :mmcode, :inqty,:disc_uprice) as INCOST from dual
                        ";
                        paraList = new List<CallDBtools_Oracle.OracleParam>();
                        paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", MMCODE));
                        paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":inqty", "VarChar", INQTY.ToString().Trim()));
                        paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":disc_uprice", "VarChar", DISC_UPRICE.ToString().Trim()));
                        incost_dt = callDbtools_oralce.CallOpenSQLReturnDT(sql, null, paraList, "oracle", "T1", ref msg_ORDB);
                        if (msg_ORDB == "" && incost_dt.Rows.Count > 0)
                        {
                            INCOST = (string.IsNullOrEmpty(incost_dt.Rows[0]["INCOST"].ToString().Trim()) || incost_dt.Rows[0]["INCOST"].ToString().Trim() == "null") ?
                                            0 : double.Parse(incost_dt.Rows[0]["INCOST"].ToString().Trim());

                            logController.AddLogs(string.Format("INCOST = {0}", INCOST));
                        }

                        logController.AddLogs(string.Format("計算", INCOST));
                        // 7. 計算
                        if (P_INVQTY + INQTY == 0)
                        {
                            if (P_AVG_PRICE == 0)
                            {
                                AVG_PEICE = DISC_UPRICE;
                            }
                            else
                            {
                                AVG_PEICE = P_AVG_PRICE;
                            }
                        }
                        else {
                            AVG_PEICE = Math.Round(( (P_INVQTY * P_AVG_PRICE) + (INCOST)) / (P_INVQTY + INQTY), 10);
                        }
                        logController.AddLogs(string.Format("AVG_PEICE = {0}", AVG_PEICE));

                        logController.AddLogs(string.Format("更新MI_WHCOST"));
                        // 8. 更新MI_WHCOST
                        sql = @"
                            update MI_WHCOST
                               set avg_price = :avg_price
                             where data_ym in ('11008', '11009')
                               and mmcode = :mmcode
                        ";

                        logController.AddLogs(string.Format("執行更新"));
                        paraList = new List<CallDBtools_Oracle.OracleParam>();
                        paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":avg_price", "VarChar", AVG_PEICE.ToString().Trim()));
                        paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":mmcode", "VarChar", MMCODE));
                       
                        rowsAffected = callDbtools_oralce.CallExecSQL(sql, paraList, "oracle", ref msg_ORDB);
                        if (rowsAffected == 0) {
                            logController.AddLogs(string.Format("更新失敗"));
                        }
                    }

                    

                }

                logController.AddLogs(string.Format("更新成功後commit，關閉資料庫連線"));

                conn_oracle.Close();

                logController.CreateLogFile();
            }
            catch (Exception e){
                logController.AddLogs(string.Format("error：{0}", e.Message));
                logController.AddLogs(string.Format("trace：{0}", e.StackTrace));
                logController.CreateLogFile();
            }
        }

    }
}
