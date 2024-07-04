using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;
using System.Text;

namespace WebApp.Report.A
{
    public partial class AA0187 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string p0 = Request.QueryString["P0"] is null ? "" : Request.QueryString["P0"].ToString().PadLeft(7, '0').Replace("null", ""); // 民國年補0
                string p1 = Request.QueryString["P1"] is null ? "" : Request.QueryString["P1"].ToString().Replace("null", "");
                string p2 = Request.QueryString["P2"] is null ? "" : Request.QueryString["P2"].ToString().Replace("null", "");
                string p3 = Request.QueryString["P3"] is null ? "" : Request.QueryString["P3"].ToString().Replace("null", "");
                string p4 = Request.QueryString["P4"] is null ? "" : Request.QueryString["P4"].ToString().Replace("null", "");
                string p5 = Request.QueryString["P5"] is null ? "" : Request.QueryString["P5"].ToString().Replace("null", "");
                string p6 = Request.QueryString["P6"] is null ? "" : Request.QueryString["P6"].ToString().Replace("null", "");

                report1Bind(p0, p1, p2, p3, p4, p5, p6);
            }
        }
        protected void report1Bind(string p0, string p1, string p2, string p3, string p4, string p5, string p6)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0187Repository repo = new AA0187Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();

                    ReportViewer1.EnableTelemetry = false;

                    // 解析民國年月日字串 組成列印日期跟標題
                    string p0_year = p0.Substring(0, 3);
                    string p0_month = p0.Substring(3, 2);
                    string p0_date = p0.Substring(5, 2);

                    string printDate = p0_year + "年" + p0_month + "月" + p0_date + "日";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintDate", printDate) });

                    string reportTitle = hospName + "  " + p0_year + "年度" + p0_month + "月份庫存盤點記錄表";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ReportTitle", reportTitle) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "";

                    IEnumerable<Models.AA0187> printQueryResult = repo.GetPrint(p1, p2, p3, p4, p5, p6);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0187", printQueryResult));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
    }
}