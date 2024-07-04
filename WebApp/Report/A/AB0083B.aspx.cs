using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AB0083B : SiteBase.BasePage
    {
        string P1;
        string P2;
        string P3;
        string[] arr_p3 = { };
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = Request.QueryString["p3"].ToString().Replace("null", "");

                if (!string.IsNullOrEmpty(P3))
                {
                    arr_p3 = P3.Trim().Split(','); //用,分割
                }
                Report1Bind();
            }
        }
        protected void Report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0083Repository repo = new AB0083Repository(DBWork);

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.ToString("MM/dd") + " " + DateTime.Now.ToString("hh:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Date1", P1) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Date2", P2) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Userid", DBWork.UserInfo.UserId) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Username", DBWork.UserInfo.UserName) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0083", repo.GetPrintDataB(P1, P2, arr_p3)));

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