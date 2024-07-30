using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using JCLib.DB.Tool;
using Tsghmm.Utility;

namespace BDS002
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //程式起始 BDS002-新訊息通知廠商EMAIL發送排程
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string msg_oracle = "error";
                int rowsAffected_oracle = 0;
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_oralce = new DataTable();
                DataTable dt_oralce_opt_A = new DataTable();
                string sql_oracle = "select THEME, MSG, OPT, AGEN_NO, CREATE_USER, DOCID from PH_MSGMAIL_M where STATUS ='84' ";
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    CallDBtools calldbtools = new CallDBtools();
                    string mailSender = calldbtools.GetDefaultMailSender();
                    for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    {
                        int sendmailerr = 0;
                        string mailSubject = "三軍總醫院訊息通知-" + dt_oralce.Rows[i]["THEME"].ToString().Trim();
                        string mailBody = dt_oralce.Rows[i]["MSG"].ToString().Trim();
                        if (dt_oralce.Rows[i]["OPT"].ToString().Trim() == "P")
                        {
                            string[] agenNoAry = dt_oralce.Rows[i]["AGEN_NO"].ToString().Trim().Split(',');
                            for (int j = 0; j < agenNoAry.Length; j++)
                            {
                                sql_oracle = "select EMAIL from PH_VENDER where AGEN_NO = :agen_no ";
                                List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                                paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":agen_no", "VarChar", agenNoAry[j].ToString().Trim()));
                                string mailAddress = callDbtools_oralce.CallExecScalar(sql_oracle, paraList, "oracle", ref msg_oracle);
                                if (msg_oracle == "" && mailAddress != "")
                                {
                                    SendMail sendmail = new SendMail();
                                    bool sendMailFlag = sendmail.Send_Mail(mailSender, mailAddress, "", "新訊息通知(單獨傳送)", mailSubject, mailBody, dt_oralce.Rows[i]["CREATE_USER"].ToString().Trim(), "");
                                    if (sendMailFlag) //寄件成功
                                    {}
                                    else //寄件失敗
                                    {
                                        sendmailerr++;
                                        callDbtools_oralce.I_ERROR_LOG("BDS002", "STEP3-寄件失敗:" + agenNoAry[j].ToString().Trim(), "AUTO");
                                    }
                                }
                                else if (msg_oracle != "")
                                {
                                    sendmailerr++;
                                    callDbtools_oralce.I_ERROR_LOG("BDS002", "STEP2-取得AGEN_NO="+ agenNoAry[j].ToString().Trim() + "的EMAIL失敗:" + msg_oracle, "AUTO");
                                }
                                paraList.Clear();
                            }
                        }
                        else if (dt_oralce.Rows[i]["OPT"].ToString().Trim() == "A")
                        {
                            if(dt_oralce_opt_A.Rows.Count == 0)
                            {
                                sql_oracle = "select AGEN_NO, EMAIL from PH_VENDER where REC_STATUS = 'A' ";
                                dt_oralce_opt_A = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                                if (msg_oracle != "")
                                {
                                    sendmailerr++;
                                    callDbtools_oralce.I_ERROR_LOG("BDS002", "STEP5-取得PH_VENDER失敗:" + msg_oracle, "AUTO");
                                }
                            }
                            if(dt_oralce_opt_A.Rows.Count > 0)
                            {
                                for (int k = 0; k < dt_oralce_opt_A.Rows.Count; k++)
                                {
                                    SendMail sendmail = new SendMail();
                                    bool sendMailFlag = sendmail.Send_Mail(mailSender, dt_oralce_opt_A.Rows[k]["EMAIL"].ToString().Trim(), "", "新訊息通知(單獨傳送)", mailSubject, mailBody, dt_oralce.Rows[i]["CREATE_USER"].ToString().Trim(), "");
                                    if (sendMailFlag) //寄件成功
                                    {}
                                    else //寄件失敗
                                    {
                                        sendmailerr++;
                                        callDbtools_oralce.I_ERROR_LOG("BDS002", "STEP6-寄件失敗:" + dt_oralce_opt_A.Rows[k]["EMAIL"].ToString().Trim(), "AUTO");
                                    }
                                }
                            }
                        }
                        if(sendmailerr == 0) //多筆寄件都成功才去更新狀態
                        {
                            //更新狀態
                            sql_oracle = " update PH_MSGMAIL_M set STATUS='82' where DOCID = :docid ";
                            List<CallDBtools_Oracle.OracleParam> paraList2 = new List<CallDBtools_Oracle.OracleParam>();
                            paraList2.Add(new CallDBtools_Oracle.OracleParam(1, ":docid", "VarChar", dt_oralce.Rows[i]["DOCID"].ToString().Trim()));
                            rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList2, "oracle", ref msg_oracle);
                            if (msg_oracle != "")
                                callDbtools_oralce.I_ERROR_LOG("BDS002", "STEP4-更新PH_MSGMAIL_M失敗:" + dt_oralce.Rows[i]["DOCID"].ToString().Trim(), "AUTO");
                            paraList2.Clear();
                        }
                    }
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BDS002", "STEP1-取得PH_MSGMAIL_M失敗:" + msg_oracle, "AUTO");
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS002", "程式錯誤:");
            }
            this.Close();
        }
    }
}
