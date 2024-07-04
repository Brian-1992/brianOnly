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
    public partial class FA0053 : SiteBase.BasePage
    {
        string P1;
        string P2;
        string P3;
        string P4;
        string P5;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = Request.QueryString["p3"].ToString().Replace("null", "");
                P4 = Request.QueryString["p4"].ToString().Replace("null", "");
                P5 = Request.QueryString["p5"].ToString().Replace("null", "");

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
                    FA0053Repository repo = new FA0053Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.ToString("MM/dd") + " " + DateTime.Now.ToString("hh:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Date1", P1) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Date2", P2) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Userid", DBWork.UserInfo.UserId) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Username", DBWork.UserInfo.UserName) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0053", repo.GetPrintData(P1, P2, P3, P4, P5)));

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