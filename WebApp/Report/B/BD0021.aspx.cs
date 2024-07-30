using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Repository.B;

namespace WebApp.Report.B
{
    public partial class BD0021 : SiteBase.BasePage
    {
        string WH_NO = "";
        string MAT_CLASS = "";
        string MMCODE = "";
        string M_CONTID = "";
        string PO_NO = "";
        string PO_TIME_FROM = "";
        string PO_TIME_TO = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                WH_NO = Request.QueryString["p0"].ToString().Replace("null", "");
                MAT_CLASS = Request.QueryString["p1"].ToString().Replace("null", "");
                MMCODE = Request.QueryString["p2"].ToString().Replace("null", "");
                M_CONTID = Request.QueryString["p3"].ToString().Replace("null", "");
                PO_NO = Request.QueryString["p4"].ToString().Replace("null", "");
                PO_TIME_FROM = Request.QueryString["d0"].ToString().Replace("null", "");
                PO_TIME_TO = Request.QueryString["d1"].ToString().Replace("null", "");

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
                    ReportViewer1.EnableTelemetry = false;

                    BD0021Repository repo = new BD0021Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "年" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "月" + DateTime.Now.Day.ToString().PadLeft(2, '0') + "日";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    DataTable dt = repo.GetReportMain_1();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("REC_ADDR", dt.Rows[0]["REC_ADDR"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CONTACT", dt.Rows[0]["CONTACT"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HOSP_FAX", dt.Rows[0]["HOSP_FAX"].ToString().Trim()) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BD0021", repo.GetReport_1(WH_NO, MAT_CLASS, MMCODE, M_CONTID, PO_NO, PO_TIME_FROM, PO_TIME_TO)));

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