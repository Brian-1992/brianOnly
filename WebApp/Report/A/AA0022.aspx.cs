using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0022 : System.Web.UI.Page
    {
        string P0;
        string P1;
        string YYYMM;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ReportViewer1.ShowPrintButton = false;
                ReportViewer1.ShowExportControls = false;
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                YYYMM = P0.Substring(0,5);
                report1Bind();
            }
        }
        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0022Repository repo = new AA0022Repository(DBWork);

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM", YYYMM) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ReportType", "2") });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Matname", P1) });
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0123", repo.GetPrintData(P0)));

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