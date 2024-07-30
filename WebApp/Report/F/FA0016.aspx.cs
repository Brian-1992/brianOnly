using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Repository.AA;

namespace WebApp.Report.F
{
    public partial class FA0016 : SiteBase.BasePage
    {
        string parMAT_CLASS;
        string parAPVTIME_B;
        string parAPVTIME_E;
        string parMMCODE;
        string parMAT_CLASS_N;

        bool isFlowid6only;
        bool isApvqtynot0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                parAPVTIME_B = Request.QueryString["APVTIME_B"].ToString().Replace("null", "");
                parAPVTIME_E = Request.QueryString["APVTIME_E"].ToString().Replace("null", "");
                parMMCODE =  Request.QueryString["MMCODE"].ToString().Replace("null", "");
                parMAT_CLASS_N = Request.QueryString["MAT_CLASS_N"].ToString().Replace("null", "");

                isFlowid6only = Request.QueryString["flowid6only"].ToString().Replace("null", "") == "Y";
                isApvqtynot0 = Request.QueryString["apvqtynot0"].ToString().Replace("null", "") == "Y";

                parMAT_CLASS_N = parMAT_CLASS_N != "" ? parMAT_CLASS_N.Substring(3) : "";
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
                    FA0016Repository repo = new FA0016Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APVTIME_B", parAPVTIME_B )});
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APVTIME_E", parAPVTIME_E) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MAT_CLASS_N", parMAT_CLASS_N) });
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0016", repo.Report(parMAT_CLASS, parAPVTIME_B, parAPVTIME_E, parMMCODE, isFlowid6only, isApvqtynot0)));
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