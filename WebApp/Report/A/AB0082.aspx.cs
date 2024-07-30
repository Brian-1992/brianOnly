using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AB0082 : SiteBase.BasePage
    {
        string D0="", D1 = "", D2 = "", D3 = "", D4 = "", D5 = "", D6 = "", D7 = "", NRCODE1 = "", NRCODE2 = "", P0, P1, P2, P3, WH_NO;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                D0 = Request.QueryString["D0"].ToString().Replace("null", "");
                D1 = Request.QueryString["D1"].ToString().Replace("null", "");
                D2 = Request.QueryString["D2"].ToString().Replace("null", "");
                D3 = Request.QueryString["D3"].ToString().Replace("null", "");
                D4 = Request.QueryString["D4"].ToString().Replace("null", "");
                D5 = Request.QueryString["D5"].ToString().Replace("null", "");
                D6 = Request.QueryString["D6"].ToString().Replace("null", "");
                D7 = Request.QueryString["D7"].ToString().Replace("null", "");

                NRCODE1 = Request.QueryString["NRCODE1"].ToString().Replace("null", "");
                NRCODE2 = Request.QueryString["NRCODE2"].ToString().Replace("null", "");
                P0 = Request.QueryString["P0"].ToString().Replace("null", "");
                P1 = Request.QueryString["P1"].ToString().Replace("null", "");
                P2 = Request.QueryString["P2"].ToString().Replace("null", "");
                P3 = Request.QueryString["P3"].ToString().Replace("null", "");
                WH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");


                report1Bind();
                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
            }
        }
        protected void report1Bind()
        {
            string createdatetime1 = "", createdatetime2 = "", finalcreatedatetime1 = "", finalcreatedatetime2 = "";
            string finalfinddate1 = "", finalfinddate2 = "", usedatetime1 = "", usedatetime2 = "";
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    //正規化查詢日期
                    if (D0 != "" && D0 != null)
                    {
                        finalfinddate1 = D0.Substring(0, 3) + "/" + D0.Substring(3, 2) + "/" + D0.Substring(5, 2) + " " + D1;
                        usedatetime1 = D0 + D1;
                    }
                    else
                    {
                        finalfinddate1 = "";
                        usedatetime1 = "";
                    }
                    if (D2 != "" && D2 != null)
                    {
                        finalfinddate2 = D2.Substring(0, 3) + "/" + D2.Substring(3, 2) + "/" + D2.Substring(5, 2) + " " + D3;
                        usedatetime2 = D2 + D3;
                    }
                    else
                    {
                        finalfinddate2 = "";
                        usedatetime2 = "";
                    }

                    //正規化退藥日期時間
                    if (D5 != "" && D5 != null)
                    {
                        createdatetime1 = D5.Substring(0, 2) + D5.Substring(3, 2);
                        finalcreatedatetime1 = D4 + createdatetime1;
                    }

                    if (D7 != "" && D7 != null)
                    {
                        createdatetime2 = D7.Substring(0, 2) + D7.Substring(3, 2);
                        finalcreatedatetime2 = D6 + createdatetime2;
                    }


                    //寫入報表
                    AB0082Repository repo = new AB0082Repository(DBWork);
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.ToString("MM/dd") + " " + DateTime.Now.ToString("hh:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("finddate1", finalfinddate1) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("finddate2", finalfinddate2) });

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WH_NO", WH_NO) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserId", DBWork.UserInfo.UserId) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserName", DBWork.UserInfo.UserName) });



                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.GetPrintData(NRCODE1, NRCODE2, P0, P1, P2, P3, WH_NO, finalcreatedatetime1, finalcreatedatetime2, usedatetime1, usedatetime2)));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }


        }
    }
}