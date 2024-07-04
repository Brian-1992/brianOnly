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
    public partial class FA0015 : SiteBase.BasePage
    {
        string parMAT_CLASS;
        string parDIS_TIME_B;
        string parDIS_TIME_E;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                parDIS_TIME_B = Request.QueryString["DIS_TIME_B"].ToString().Replace("null", "");
                parDIS_TIME_E = Request.QueryString["DIS_TIME_E"].ToString().Replace("null", "");

                report1Bind();
            }
        }

        protected void report1Bind()
        {
            string parDIS_TIME_B_TITLE;
            string parDIS_TIME_E_TITLE;

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0015Repository repo = new FA0015Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    if (parDIS_TIME_B != "")
                    {
                        parDIS_TIME_B_TITLE = parDIS_TIME_B.Substring(0, parDIS_TIME_B.Length < 5 ? 2 : 3) + "年" + parDIS_TIME_B.Substring(parDIS_TIME_B.Length < 5 ? 2 : 3, 2) + "月" ;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DIS_TIME_B", parDIS_TIME_B ) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DIS_TIME_B_TITLE", parDIS_TIME_B_TITLE ) });
                    }
                    if (parDIS_TIME_E != "")
                    {
                        parDIS_TIME_E_TITLE = parDIS_TIME_E.Substring(0, parDIS_TIME_E.Length < 5 ? 2 : 3) + "年" + parDIS_TIME_E.Substring(parDIS_TIME_E.Length < 5 ? 2 : 3, 2) + "月";
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DIS_TIME_E", parDIS_TIME_E) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DIS_TIME_E_TITLE", parDIS_TIME_E_TITLE) });
                    }

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DeptName", repo.getDeptName()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MatName", repo.getMatName(parMAT_CLASS)) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WH_NAME", repo.GetReportWH_NAME()) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0015", repo.Report(parMAT_CLASS, parDIS_TIME_B, parDIS_TIME_E)));
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