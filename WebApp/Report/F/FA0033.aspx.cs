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
    public partial class FA0033 : SiteBase.BasePage
    {

        string parMAT_CLASS;
        string parYYYMM_B;
        string parYYYMM_E;
        string parP3;
        string parP4;
        string parMMCODE;
        string parMAT_CLASS_N;
        string parISCHK;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMAT_CLASS = Request.QueryString["P0"].ToString().Replace("null", "");
                parYYYMM_B = Request.QueryString["P1"].ToString().Replace("null", "");
                parYYYMM_E = Request.QueryString["P2"].ToString().Replace("null", "");
                parP3 = Request.QueryString["P3"].ToString().Replace("null", "");
                parP4 = Request.QueryString["P4"].ToString().Replace("null", "");
                parMMCODE = Request.QueryString["P5"].ToString().Replace("null", "");
                parMAT_CLASS_N = Request.QueryString["P6"].ToString().Replace("null", "");
                parMAT_CLASS_N = parMAT_CLASS_N != "" ? parMAT_CLASS_N.Substring(3) : "";
                parISCHK = Request.QueryString["P7"].ToString().Replace("null", "");

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
                    FA0033Repository repo = new FA0033Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM_B", parYYYMM_B) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM_E", parYYYMM_E) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MAT_CLASS_N", parMAT_CLASS_N) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("P4", parP4) });
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0033", repo.Report(parMAT_CLASS, parYYYMM_B, parYYYMM_E, parP3, parP4, parMMCODE, parISCHK)));
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