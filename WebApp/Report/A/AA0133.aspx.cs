using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0133 : SiteBase.BasePage
    {
        string P0;
        string P1;
        string P2;
        bool isAll;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = Request.QueryString["p2"].ToString().Replace("null", "");
                isAll = (Request.QueryString["isAll"].ToString().Replace("null", "") == "Y");
                report1Bind();
            }
        }
        protected void report1Bind()
        {
            string inid;
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0133Repository repo = new AA0133Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    inid = repo.GetUserInid(P2);

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day ;
                    string drugType = P1 == "3" ? "1~3" : "4";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DrugType", drugType) });

                    //var V_DeptName = repo.GetDeptName(P0);
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DeptName", V_DeptName) });
                    var month_temp = new DateTime(DateTime.Now.Year, (DateTime.Now.Month-1), 1);
                    var V_YYYMM = (month_temp.Year - 1911).ToString() + "年" + month_temp.Month + "月";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM", V_YYYMM) });
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "各衛星庫房管制藥品清點";
                    string hospCode = repo.GetHospCode();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0133", repo.GetPrintData(P0, P1, P2, inid, isAll, hospCode=="0")));

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