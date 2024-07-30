using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AB0016 : SiteBase.BasePage
    {
        string FR;
        string P0;
        string P1;
        string P2;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FR = Request.QueryString["fr"].ToString().Replace("null", "");
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = Request.QueryString["p2"].ToString().Replace("null", "");
                report1Bind();
            }
        }
        protected void report1Bind()
        {
            string FR2;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0016Repository repo = new AB0016Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    var matname = repo.GetMatclassname(P1);
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM", P0) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MATNAME", matname) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("KIND", P2) });
                    
                    if (FR == "AB0016")
                    {
                        FR2 = "EF2";
                    }
                    else
                    {
                        FR2 = "CM2";
                    }
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = P0;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0016", repo.GetPrintData(P0,P1,FR2)));

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