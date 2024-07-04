using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0081 : SiteBase.BasePage
    {
        string P0;
        string P1;
        string YYYMM;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                YYYMM = P0;
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
                    AA0081Repository repo = new AA0081Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM", YYYMM) });
                    string str_P1;
                    if (P1 == "1")
                    {
                        str_P1 = "口服";
                    }
                    else if ( P1 == "2" )
                    {
                        str_P1 = "非口服";

                    }
                    else if (P1 == "3")
                    {
                        str_P1 = "1~3級管制";

                    }
                    else 
                    {
                        str_P1 = "4級管制";
                    }
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Type", str_P1) });
                    var username = repo.GetUserName(User.Identity.Name);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("User", username) });
                    var userinid = repo.GetUserInid(User.Identity.Name);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Inid", userinid) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = YYYMM;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0081", repo.GetPrintData(P0, P1)));

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