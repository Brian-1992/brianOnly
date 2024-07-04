using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AB0097 : SiteBase.BasePage
    {
        string P0;
        string P1;
        string P2;
        string P3;
        string YYYMM ;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = Request.QueryString["p3"].ToString().Replace("null", "");
                YYYMM = P0 ;
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
                    AB0097Repository repo = new AB0097Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() +  DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = YYYMM;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0097", repo.GetPrintData(P0,P1,P2,P3)));

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