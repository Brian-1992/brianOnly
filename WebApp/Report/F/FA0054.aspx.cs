using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.F
{
    public partial class FA0054 : SiteBase.BasePage
    {
        string P0;
        string YYY;
        string MM ;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
                YYY = P0.Substring(0,3) ;
                MM = P0.Substring(3,2) ;
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
                    FA0054Repository repo = new FA0054Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() +  DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYY", YYY) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MM", MM) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = YYY;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0054A", repo.GetPrintDataA(P0)));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0054B", repo.GetPrintDataB(P0)));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0054C", repo.GetPrintDataC(P0)));

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