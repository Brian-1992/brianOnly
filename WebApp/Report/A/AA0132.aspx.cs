using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0132 : SiteBase.BasePage
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
                    AA0132Repository repo = new AA0132Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    string str_P0;

                    if (P0 == "1")
                    {
                        str_P0 = "口服";
                    }
                    else if (P0 == "2")
                    {
                        str_P0 = "非口服";

                    }
                    else if (P0 == "3")
                    {
                        str_P0 = "1~3級管制";
                    }
                    else
                    {
                        str_P0 = "4級管制";
                    }
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Type", str_P0) });
                    var username = repo.GetUserName(User.Identity.Name);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("User", username) });
                    var userinid = repo.GetUserInid(User.Identity.Name);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Inid", userinid) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "藥庫平時查詢";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0081", repo.GetPrintData(P0)));

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