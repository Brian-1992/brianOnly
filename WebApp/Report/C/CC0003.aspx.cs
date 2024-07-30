using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.C;

namespace WebApp.Report.C
{
    public partial class CC0003 : System.Web.UI.Page
    {
        string WH_NO;
        string AGEN_NO;
        string PURDATE;
        string PURDATE_1;
        string KIND;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                WH_NO = Request.QueryString["p0"].ToString().Replace("null", "");
                AGEN_NO = Request.QueryString["p1"].ToString().Replace("null", ""); 
                PURDATE = Request.QueryString["p2"].ToString().Replace("null", "");//PURDATE=yyymmdd
                PURDATE_1 = Request.QueryString["p2_1"].ToString().Replace("null", "");//PURDATE=yyymmdd
                KIND = Request.QueryString["p3"].ToString().Replace("null", "");
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
                    CC0003Repository repo = new CC0003Repository(DBWork);
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PURDATE", PURDATE) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PURDATE_1", PURDATE_1) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("str_PrintTime", str_PrintTime) });
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CC0003", repo.Report(WH_NO, AGEN_NO, PURDATE, PURDATE_1, KIND)));

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