using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.F
{
    public partial class FA0081 : SiteBase.BasePage
    {
        string P0;
        string P1;
        string P2;
        string P3;
        string P4;
        string P5;
        string P6;
        string P7;
        string P8;
        string P9;
        string P10;
        string P11;
        string P12;
        string P13;
        string USER;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = Request.QueryString["p3"].ToString().Replace("null", "");
                P4 = Request.QueryString["p4"].ToString().Replace("null", "");
                P5 = Request.QueryString["p5"].ToString().Replace("null", "");
                P6 = Request.QueryString["p6"].ToString().Replace("null", "");
                P7 = Request.QueryString["p7"].ToString().Replace("null", "");
                P8 = Request.QueryString["p8"].ToString().Replace("null", "");
                P9 = Request.QueryString["p9"].ToString().Replace("null", "");
                P10 = Request.QueryString["p10"].ToString().Replace("null", "");
                P11 = Request.QueryString["p11"].ToString().Replace("null", "");
                P12 = Request.QueryString["p12"].ToString().Replace("null", "");
                P13 = Request.QueryString["p13"].ToString().Replace("null", "");
                USER = User.Identity.Name;
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
                    FA0081Repository repo = new FA0081Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    string YYYMM = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    string str_discAmount = repo.GetExtraDiscAmout(P0, YYYMM);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DiscAmount", str_discAmount) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0081", repo.GetPrintData(P0, P1, P2, P3, P4, P5, P6, P7, P8, P9 ,P10, P11, P12, P13, USER)));
                    //ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA00812", repo.GetPrintData2(P0, P1, P2, P3, P4, P5, P6, P7, P8, YYYMM, ISAB, USER)));

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