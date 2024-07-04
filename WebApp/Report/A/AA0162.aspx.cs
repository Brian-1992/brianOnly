using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0162 : SiteBase.BasePage
    {
        string P1 = "";
        string D0 = "";
        string D1 = "";
        string P2 = "";
        string P3 = "";
        string[] arr_p2 = { };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                D0 = Request.QueryString["d0"].ToString().Replace("null", "");
                D1 = Request.QueryString["d1"].ToString().Replace("null", "");
                P2 = Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = Request.QueryString["p3"].ToString().Replace("null", "");
                if (!string.IsNullOrEmpty(P2))
                {
                    arr_p2 = P2.Trim().Split(','); //用,分割
                }
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
                    AA0162Repository repo = new AA0162Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    // string wh_userId = User.Identity.Name;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    string str_UserDept = DBWork.UserInfo.InidName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserDept", str_UserDept) });


                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0162", repo.GetReport(P1, D0, D1, P3, arr_p2, "")));

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