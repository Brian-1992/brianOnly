using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AA0070 : SiteBase.BasePage
    {
        string parMAT_CLASS;
        string parDIS_TIME_B;
        string parDIS_TIME_E;
        bool parP3;
        string parAPPTIME_B;
        string parAPPTIME_E;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                parDIS_TIME_B = Request.QueryString["DIS_TIME_B"].ToString().Replace("null", "");
                parDIS_TIME_E = Request.QueryString["DIS_TIME_E"].ToString().Replace("null", "");
                parP3 = bool.Parse(Request.QueryString["P3"].ToString().Replace("null", ""));
                parAPPTIME_B = Request.QueryString["APPTIME_B"].ToString().Replace("null", "");
                parAPPTIME_E = Request.QueryString["APPTIME_E"].ToString().Replace("null", "");

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
                    AA0070Repository repo = new AA0070Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    if (parDIS_TIME_B != "")
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DIS_TIME_B", parDIS_TIME_B) });
                    }
                    if (parDIS_TIME_E != "")
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DIS_TIME_E", parDIS_TIME_E) });
                    }



                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WH_NAME", repo.GetReportWH_NAME()) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0070", repo.Report(parMAT_CLASS, parDIS_TIME_B, parDIS_TIME_E, parP3, parAPPTIME_B, parAPPTIME_E)));
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