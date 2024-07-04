using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.F
{
    public partial class FA0038B : SiteBase.BasePage
    {
        string P1;
        string P2;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P1 = Request.QueryString["P1"].ToString().Replace("null", "");
                P2 = Request.QueryString["P2"].ToString().Replace("null", "");
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
                    FA0038Repository repo = new FA0038Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() +  DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0038B", repo.GetPrintDataB(P1,P2)));
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