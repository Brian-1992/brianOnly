using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using WebApp.Controllers.B;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.B;

namespace WebApp.Report.B
{
    public partial class BD0005 : System.Web.UI.Page
    {
        //信件備註內容
        private string GetMailContentRemarks(string memo, string smemo)
        {
            string mail_body = "<p align=left><font color=brown size=4 face=標楷體>備註內容 </font></p><br>";
            //mail_body += "<p align=left><font color=red size=3 face=標楷體>***本郵件為系統自動發送，請勿直接回信*** </font></p><br>";
            if (memo != null)
            {
                memo = memo.Replace("\r", "<br><br>");
                memo = memo.Replace("\n", "<br><br>");
            }
            else
            {
                memo = "";
            }
            if (smemo != null)
            {
                smemo = smemo.Replace("\r", "<br><br>");
                smemo = smemo.Replace("\n", "<br><br>");
            }
            else
            {
                smemo = "";
            }
            mail_body += "<font size=3 face=標楷體 color=black>" + memo + " </font><br><br>";
            mail_body += "<font size=4 face=標楷體 color=red>" + smemo + " </font><br>";
            return mail_body;
        }

        private string GetMailContent(
            //DataTable dtM, 
            //DataTable dtD1, 
            //DataTable dtD2
            BD0005M qV,
            List<BD0005M> lstM,
            List<BD0005M> lstD
        )
        {
            string mail_body = "";
            string headerStr = qV.PO_NO;
            if (qV.M_CONTID.Trim() == "0")
                headerStr = "合約";
            else
                headerStr = "非合約";

            mail_body += "<P align=center><font size=4 face=新細明體 color=black>三軍總醫院訂購單(" + headerStr + ") </font></p><br><br>";
            BD0005M hM;
            if (lstM.Count > 0)
            {
                hM = (BD0005M)lstM[0];
                mail_body += "訂單編號：" + hM.PO_NO + " <br>";
                mail_body += "廠商編號：" + hM.AGEN_NO + " <br>";
                mail_body += "廠商名稱：" + hM.廠商名稱 + " <br>";
                mail_body += "交貨日期：詳如備註" + " <br><br>";

                //回覆函 URL + 參數加密 串接
                //CallDBtools calldbtools = new CallDBtools();
                //string urlStr = "http://192.168.99.52:8080"; // calldbtools.GetInternetWebServerIP();
                //urlStr += "/Form/Show/B/BH0006?";
                //參數加密格式 po_no=INV010712270196&agen_no=826
                //string enCodeStr = "po_no=" + dtM.Rows[0]["PO_NO"].ToString().Trim() + "&agen_no=" + dtM.Rows[0]["AGEN_NO"].ToString().Trim();
                //urlStr += EnCode(enCodeStr);

                //mail_body += "<font size=3 face=新細明體 color=red>謝謝您的合作,請按一下回覆----></font><a href='" + urlStr + "'>回覆函</a>" + " <br>";
                mail_body += "<table style=\"border-collapse: collapse; width: 100%;border:2px #000000 solid;\" border=\"1\">";
                mail_body += "  <tbody>";
                mail_body += "    <tr>";
                mail_body += "      <td style=\"width: auto;\">院內碼</td>";
                mail_body += "      <td style=\"width: auto;\">中文品名</td>";
                mail_body += "      <td style=\"width: auto;\">英文品名</td>";
                mail_body += "      <td style=\"width: auto;\">廠牌</td>";
                mail_body += "      <td style=\"width: auto;\">單位</td>";
                mail_body += "      <td style=\"width: auto;\">單價</td>";
                mail_body += "      <td style=\"width: auto;\">數量</td>";
                mail_body += "      <td style=\"width: auto;\">金額</td>";
                mail_body += "      <td style=\"width: auto;\">折讓百分比</td>";
                mail_body += "    </tr>";
                double dTotal = 0.0;
                double d = 0.0;
                foreach (BD0005M mV in lstM)
                {
                    mail_body += "    <tr>";
                    mail_body += "      <td>" + mV.院內碼 + "</td>";
                    mail_body += "      <td>" + mV.中文品名 + "</td>";
                    mail_body += "      <td>" + mV.英文品名 + "</td>";
                    mail_body += "      <td>" + mV.廠牌 + "</td>";
                    mail_body += "      <td>" + mV.單位 + "</td>";
                    mail_body += "      <td>" + mV.單價 + "</td>";
                    mail_body += "      <td>" + mV.數量 + "</td>";
                    mail_body += "      <td>" + mV.金額 + "</td>";
                    mail_body += "      <td>" + mV.折讓百分比 + "</td>";
                    mail_body += "    </tr>";
                    if (double.TryParse(mV.金額, out d))
                        dTotal += d;
                }
                mail_body += "  </tbody>";
                mail_body += "</table><br><br>";

                mail_body += "<p align=left><font size=4 face=新細明體 color=#ff0000>合計金額(新台幣):$" + Math.Floor(dTotal).ToString() + " </font></p><br><br>";
                if (lstD.Count > 0)
                {

                    mail_body += "<p align=left><font color=blue size=4 face=標楷體>**單位非庫備品申請明細** </font></p><br>";
                    mail_body += "<table style=\"border-collapse: collapse; width: 30%;border:2px #000000 solid;\" border=\"1\">";
                    mail_body += "  <tbody>";
                    mail_body += "    <tr>";
                    mail_body += "      <td style=\"width: auto;\">院內碼</td>";
                    mail_body += "      <td style=\"width: auto;\">單位名稱</td>";
                    mail_body += "      <td style=\"width: auto;\">申請量</td>";
                    mail_body += "    </tr>";
                    foreach (BD0005M eV in lstD)
                    {
                        mail_body += "    <tr>";
                        mail_body += "      <td>" + eV.院內碼 + "</td>";
                        mail_body += "      <td>" + eV.單位名稱 + "</td>";
                        mail_body += "      <td>" + eV.申請量 + "</td>";
                        mail_body += "    </tr>";
                    }
                    mail_body += "  </tbody>";
                    mail_body += "</table><br><br>";


                }
                mail_body += GetMailContentRemarks(hM.MEMO, hM.SMEMO);
            }
            
            return mail_body;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!String.IsNullOrEmpty(Request.QueryString["PO_NO"]))
                {
                    BD0005Controller c = new BD0005Controller();
                    BD0005M qV = new BD0005M();
                    qV.PO_NO = Request.QueryString["PO_NO"]; 
                    qV.M_CONTID = Request.QueryString["M_CONTID"];
                    qV.UPDATE_USER = Request.QueryString["UPDATE_USER"];
                    qV.UPDATE_IP = Request.QueryString["UPDATE_IP"];
                    List<BD0005M> lstM = (List < BD0005M >)c.ReportsM(qV);
                    List<BD0005M> lstD = (List<BD0005M>)c.ReportsD(qV);
                    c.UpdateStatus(qV);
                    ////撈出信件基本資料
                    //DataTable dtM = new DataTable();
                    //dtM = GetMailBasicData(PO_NO);
                    ////撈出信件訂單內容
                    //DataTable dtD1 = new DataTable();
                    //dtD1 = GetMailDt1(PO_NO);
                    ////撈出信件單位非庫備品申請明細
                    //DataTable dtD2 = new DataTable();
                    //dtD2 = GetMailDt2(PO_NO);
                    ////組出信件內容
                    string mailBodyStr = GetMailContent(qV, lstM, lstD); // dtM, dtD1, dtD2
                    Response.Write(mailBodyStr);

                    if (mailBodyStr.Length >= 3000)
                        mailBodyStr = mailBodyStr.Substring(0, 2990) + "...";
                    PH_MAILLOG pV = new PH_MAILLOG();
                    pV.MSUBJECT = "三軍總醫院訂購單" + qV.PO_NO;
                    pV.MAILBODY = mailBodyStr;
                    pV.CREATE_USER = User.Identity.Name;
                    new BD0005Controller().Create_ph_maillog(pV);
                }
            }
        }
        public void UpdateStatus(BD0005M vBD0005M)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0005Repository repo = new BD0005Repository(DBWork);
                    repo.UpdateStatus(vBD0005M);
                }
                catch
                {
                    throw;
                }
            }
        }


    } // ec
} // en