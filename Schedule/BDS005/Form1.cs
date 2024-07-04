using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JCLib.DB.Tool;
using Tsghmm.Utility;

namespace BDS005
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //衛材訂單逾期通知
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string msg_oracle = "error";
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                DataTable dt_oralce = new DataTable();
                DataTable dt_oralce_opt_A = new DataTable();
                string sql_oracle = @"SELECT PO_NO, A.AGEN_NO, TWN_DAT_FORMAT(DLINE_DT) DLINE_DT,
                                      B.AGEN_NAMEC,
                                      (SELECT DATA_VALUE  FROM PARAM_D WHERE GRP_CODE='MM_PO_M' AND DATA_NAME='DLINE_MSG') MSG,
                                      (SELECT DATA_VALUE  FROM PARAM_D WHERE GRP_CODE='MM_PO_M' AND DATA_NAME='DLINE_NOTE') NOTE,
                                      EMAIL
                                      FROM MM_PO_M A, PH_VENDER B
                                      WHERE A.AGEN_NO=B.AGEN_NO
                                      AND MAT_CLASS = '02' AND PO_STATUS IN ('82','85')
                                      AND DLINE_DT IS NOT NULL
                                      AND DLINE_DT-SYSDATE <= (SELECT DATA_VALUE FROM PARAM_D WHERE GRP_CODE = 'MM_PO_M' AND DATA_NAME = 'DLINE_DT') 
                                      AND (SELECT count(*) FROM BC_CS_ACC_LOG WHERE PO_NO=a.PO_NO AND MAT_CLASS='02' ) =0 ";
                dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);
                if (msg_oracle == "" && dt_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    CallDBtools calldbtools = new CallDBtools();
                    string mailSender = calldbtools.GetDefaultMailSender();
                    string agen_no = "";
                    for (int i = 0; i < dt_oralce.Rows.Count; i++)
                    {
                        int sendmailerr = 0;
                        agen_no = dt_oralce.Rows[i]["AGEN_NO"].ToString();
                        string mailSubject = "三軍總醫院訂單稽催通知-" + dt_oralce.Rows[i]["PO_NO"].ToString().Trim();
                        string mailBody = "1.訂單編號 : " + dt_oralce.Rows[i]["PO_NO"].ToString() + " <br>";
                        mailBody += "2.交貨截止日 : " + dt_oralce.Rows[i]["DLINE_DT"].ToString() + " <br>";
                        mailBody += "3." + dt_oralce.Rows[i]["MSG"].ToString().Trim() + " <br>";
                        if (!dt_oralce.Rows[i]["NOTE"].ToString().Equals(""))
                            mailBody += "4." + dt_oralce.Rows[i]["NOTE"].ToString().Trim() + " <br>";
                        mailBody += "*** 如已交貨，請忽略此信。 ***" + " <br> ";
                        mailBody += "*** 本郵件為系統自動發送，請勿直接回信 *** ";
                        string mailAddress = dt_oralce.Rows[i]["EMAIL"].ToString();
                        if (msg_oracle == "" && mailAddress != "")
                        {
                            SendMail sendmail = new SendMail();
                            bool sendMailFlag = sendmail.Send_Mail(mailSender, mailAddress, "", "廠商訂單稽催訊息", mailSubject, mailBody, "AUTO", "");
                            if (sendMailFlag) //寄件成功
                            { }
                            else //寄件失敗
                            {
                                sendmailerr++;
                                callDbtools_oralce.I_ERROR_LOG("BDS005", "STEP3-寄件失敗:" + agen_no, "AUTO");
                            }
                        }
                    }
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BDS005", "STEP1-取得MM_PO_M失敗:" + msg_oracle, "AUTO");
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS005", "程式錯誤:");
            }
            this.Close();
        }
    }
}
