using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AB0138 : System.Web.UI.Page
    {
        string P0;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
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
                    AB0138Repository repo = new AB0138Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;

                    string str_PrintTimeYYYY = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "年" + DateTime.Now.Month + "月" + DateTime.Now.Day + "日";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Docno", P0) });
                    string inidName = repo.GetInidName(User.Identity.Name);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("InidName", inidName) });
                    string SumAmt = repo.GetDocAppAmout(P0);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("sumAmt", SumAmt) });
                    string AppTime = repo.GetTwnapptime(P0);
                    string str_AppTime = AppTime.Substring(0, 3) + "年" + AppTime.Substring(4, 2) + "月" + AppTime.Substring(7, 2) + "日 " + AppTime.Substring(10, 2) + "時" + AppTime.Substring(13, 2) + "分";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("apptime", str_AppTime) });
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = str_PrintTimeYYYY;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0138", repo.GetPrintData(P0, User.Identity.Name)));

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