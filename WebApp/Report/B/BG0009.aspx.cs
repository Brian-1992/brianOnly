using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.B;

namespace WebApp.Report.B
{
    public partial class BG0009 : SiteBase.BasePage
    {
        string pACT = "";
        string pDATA_YM = "";
        string p0, p1, p2, p2_1, p3, p4, p5, p6;
        string YYY, MM;
        string[] arr_p0 = { };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pACT = (Request.QueryString["act"] is null) ? "" : Request.QueryString["act"].ToString().Replace("null", "");
                pDATA_YM = (Request.QueryString["pDATA_YM"] is null) ? "" : Request.QueryString["pDATA_YM"].ToString().Replace("null", "");
                p0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
                p1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
                p2 = (Request.QueryString["p2"] is null) ? "" : Request.QueryString["p2"].ToString().Replace("null", "");
                p2_1 = (Request.QueryString["p2_1"] is null) ? "" : Request.QueryString["p2_1"].ToString().Replace("null", "");
                p3 = (Request.QueryString["p3"] is null) ? "" : Request.QueryString["p3"].ToString().Replace("null", "");
                p4 = (Request.QueryString["p4"] is null) ? "" : Request.QueryString["p4"].ToString().Replace("null", "");
                p5 = (Request.QueryString["p5"] is null) ? "" : Request.QueryString["p5"].ToString().Replace("null", "");
                p6 = (Request.QueryString["p6"] is null) ? "" : Request.QueryString["p6"].ToString().Replace("null", "");
                YYY = pDATA_YM.Substring(0, 3);
                MM = pDATA_YM.Substring(3, 2);
                if (MM.Substring(0, 1) == "0")
                {
                    MM = pDATA_YM.Substring(4, 1);
                }
                if (!string.IsNullOrEmpty(p0))
                {
                    arr_p0 = p0.Trim().Split(','); //用,分割
                }
                report1Bind();
            }
        }
        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0009Repository repo = new BG0009Repository(DBWork);

                    string hospName = repo.GetHospName();
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "年" + DateTime.Now.Month + "月" + DateTime.Now.Day + "日 " + DateTime.Now.ToString("HH:mm:ss");

                    ReportViewer1.EnableTelemetry = false;
                    //報表路徑設定
                    if (pACT == "DL")
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BG0009.rdlc";
                    }
                    else if (pACT == "G")
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BG0009_1.rdlc";
                    }
                    else if (pACT == "A")
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BG0009_2.rdlc";
                    }

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYY", YYY) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MM", MM) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    if (pACT == "DL")
                    {
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BG0009", repo.ReportDL(pDATA_YM, arr_p0, p1, p2, p2_1, p3, p4, p5,p6)));
                        ReportViewer1.LocalReport.DisplayName = "對帳單-印明細"; //匯出指定檔名
                    }
                    else if (pACT == "G")
                    {
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BG0009", repo.ReportG(pDATA_YM, arr_p0, p1, p2, p2_1, p3, p4, p5, p6)));
                        ReportViewer1.LocalReport.DisplayName = "對帳單-廠商分組明細";
                    }
                    else if (pACT == "A")
                    {
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BG0009", repo.ReportAGEN(pDATA_YM, arr_p0, p1, p2, p2_1, p3, p4, p5, p6)));
                        ReportViewer1.LocalReport.DisplayName = "對帳單-廠商支付明細表";
                    }
                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}