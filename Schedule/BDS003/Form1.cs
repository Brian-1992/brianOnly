using JCLib.DB.Tool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tsghmm.Utility;

namespace BDS003
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetData();
            this.Close();
        }

        private void GetData()
        {
            try
            {
                int rowsAffected_oracle = -1;
                SendMail sendmail = new SendMail();
                CallDBtools calldbtools = new CallDBtools();
                DataTable dt_oralce = new DataTable();
                DataTable ag_oralce = new DataTable();
                CallDBtools_Oracle callDbtools_oralce = new CallDBtools_Oracle();
                string msg_oracle = "error";
                string toContent = "";
                string create_user = "";

                //找出要寄送的廠商
                string sql_oracle = @"SELECT DISTINCT MMK.AGEN_NO,
                                                      MMK.MAIL_NO,
                                                      PVR.EMAIL
                                      FROM ME_MAILBACK MMK, PH_VENDER PVR
                                      WHERE MMK.AGEN_NO = PVR.AGEN_NO
                                      AND MMK.STATUS IN ('A') ";

                ag_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, null, "oracle", "T1", ref msg_oracle);

                if (msg_oracle == "" && ag_oralce.Rows.Count > 0) //資料抓取沒有錯誤 且有資料
                {
                    for (int i = 0; i < ag_oralce.Rows.Count; i++) //有幾個廠商 就是要寄幾封信
                    {
                        //找出要寄送的資料
                        sql_oracle = @"  SELECT PVR.EMAIL,
                                                PVR.AGEN_NO,
                                                PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC AGEN_NAMEC,
                                                MEM.MMCODE,
                                                MMT.MMNAME_C,
                                                MMT.MMNAME_E,
                                                TO_CHAR (MEM.EXP_DATE, 'YYYYMM') - 191100 EXP_DATE,
                                                MEM.WARNYM,
                                                MEM.LOT_NO,
                                                MEM.EXP_QTY,
                                                MEM.MEMO,
                                                DECODE (MEM.CLOSEFLAG,  'Y', '是',  'N', '否',  '') CLOSEFLAG_NAME,
                                                MEM.UPDATE_USER
                                         FROM ME_MAILBACK MMK,
                                              ME_EXPM MEM,
                                              MI_MAST MMT,
                                              PH_VENDER PVR
                                         WHERE     MMK.MMCODE = MEM.MMCODE
                                         AND MMK.EXP_DATE = MEM.EXP_DATE
                                         AND MMK.LOT_NO = MEM.LOT_NO
                                         AND MMK.MMCODE = MMT.MMCODE
                                         AND MMK.AGEN_NO = PVR.AGEN_NO
                                         AND MMK.AGEN_NO = :AGEN_NO
                                         AND MMK.STATUS IN ('A','B')
                                         and MEM.EXP_QTY>0
                                         ORDER BY MMCODE";
                        List<CallDBtools_Oracle.OracleParam> paraList_1 = new List<CallDBtools_Oracle.OracleParam>();
                        paraList_1.Add(new CallDBtools_Oracle.OracleParam(1, ":AGEN_NO", "VarChar", ag_oralce.Rows[i]["AGEN_NO"].ToString().Trim()));
                        dt_oralce = callDbtools_oralce.CallOpenSQLReturnDT(sql_oracle, null, paraList_1, "oracle", "T1", ref msg_oracle);

                        //收件者
                        string toMail = ag_oralce.Rows[i]["EMAIL"].ToString().Trim();
                        //string toMail = "ANGELCHEN@ms.aidc.com.tw";
                        //副本收件者
                        string toMailCC = "";// "ANGELCHEN@ms.aidc.com.tw";
                        //主旨
                        string toSubject = "即期藥品更換通知";
                        toContent = "<br>廠商 : " + dt_oralce.Rows[0]["AGEN_NAMEC"].ToString().Trim();

                        for (int j = 0; j < dt_oralce.Rows.Count; j++)
                        {
                            //組信件內容
                            toContent += "<br><br>(" + (j + 1) + ")  -------------------------------------------- ";
                            toContent += "<br>院內碼 : " + dt_oralce.Rows[j]["MMCODE"].ToString().Trim() +
                                               "<br>中文品名 : " + dt_oralce.Rows[j]["MMNAME_C"].ToString().Trim() +
                                               "<br>英文品名 : " + dt_oralce.Rows[j]["MMNAME_E"].ToString().Trim() +
                                               "<br>月份 : " + dt_oralce.Rows[j]["EXP_DATE"].ToString().Trim() +
                                               "<br>警示效期(年/月) : " + dt_oralce.Rows[j]["WARNYM"].ToString().Trim() +
                                               "<br>批號 : " + dt_oralce.Rows[j]["LOT_NO"].ToString().Trim() +
                                               "<br>數量 : " + dt_oralce.Rows[j]["EXP_QTY"].ToString().Trim() +
                                               "<br>備註 : " + dt_oralce.Rows[j]["MEMO"].ToString().Trim() +
                                               "<br>結案否 : " + dt_oralce.Rows[j]["CLOSEFLAG_NAME"].ToString().Trim();
                            create_user = dt_oralce.Rows[j]["UPDATE_USER"].ToString().Trim();
                        }

                        //組出回覆URL
                        string urlStr = calldbtools.GetInternetWebServerIP();
                        urlStr += "/Form/Show/B/BH0007?";
                        //參數加密格式 AGEN_NO=013&MAIL_NO=1080920150841
                        string enCodeStr = "AGEN_NO=" + ag_oralce.Rows[i]["AGEN_NO"].ToString().Trim() + "&MAIL_NO=" + ag_oralce.Rows[i]["MAIL_NO"].ToString();
                        urlStr += EnCode(enCodeStr);
                        toContent += @"<br><br>謝謝您的合作,請按一下回覆----></font><a href='" + urlStr + "'>訂單回覆</a><br>";

                        //發送信件
                        bool sendMailFlag = sendmail.Send_Mail(calldbtools.GetDefaultMailSender(), toMail, toMailCC, "", toSubject, toContent, create_user, "");
                        if (sendMailFlag) //寄件成功
                        {
                            //更新已發送資料的狀態
                            sql_oracle = @" UPDATE ME_MAILBACK 
                                           SET STATUS = 'B'
                                           WHERE STATUS = 'A'
                                           AND AGEN_NO = :AGEN_NO ";
                            List<CallDBtools_Oracle.OracleParam> paraList = new List<CallDBtools_Oracle.OracleParam>();
                            paraList.Add(new CallDBtools_Oracle.OracleParam(1, ":AGEN_NO", "VarChar", ag_oralce.Rows[i]["AGEN_NO"].ToString()));
                            rowsAffected_oracle = callDbtools_oralce.CallExecSQL(sql_oracle, paraList, "oracle", ref msg_oracle);
                            if (msg_oracle != "")
                                callDbtools_oralce.I_ERROR_LOG("BDS003", "效期管理作業MAIL發送更新TABLE失敗:" + dt_oralce.Rows[i]["AGEN_NAMEC"].ToString().Trim() + "-" + dt_oralce.Rows[i]["MMCODE"].ToString().Trim(), "AUTO");
                        }
                    }
                }
                else if (msg_oracle != "")
                {
                    callDbtools_oralce.I_ERROR_LOG("BDS003", "效期管理作業MAIL發送取得資料失敗" + msg_oracle, "AUTO");
                }
            }
            catch (Exception ex)
            {
                CallDBtools callDBtools = new CallDBtools();
                callDBtools.WriteExceptionLog(ex.Message, "BDS003", "程式發生錯誤");
            }
        }

        //參數值加密 AGEN_NO=013&MMCODE=003ACT09&EXP_DATE=10805&LOT_NO=24-456-4&EXP_QTY=50
        private string EnCode(string EnString)
        {
            byte[] data = Encoding.UTF8.GetBytes(EnString);
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Padding = PaddingMode.PKCS7;
            DES.Key = ASCIIEncoding.ASCII.GetBytes("BH07BHS5");
            DES.IV = ASCIIEncoding.ASCII.GetBytes("TsghTsgh");
            ICryptoTransform desencrypt = DES.CreateEncryptor();
            byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
            return BitConverter.ToString(result);
        }
    }
}
