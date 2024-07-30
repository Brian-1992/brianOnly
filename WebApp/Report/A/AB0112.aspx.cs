using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AB0112 : System.Web.UI.Page
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
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;

                    string str_PrintTimeYYYY = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    DataTable dt = repo.GetPrintTitleData(P0);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", dt.Rows[0][0].ToString()) });
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Docno", P0) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("apptime", dt.Rows[0][1].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("supplyWhName", dt.Rows[0][2].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("appInidName", dt.Rows[0][8].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("sumAmt", dt.Rows[0][4].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("memo", dt.Rows[0][5].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("isArmy", dt.Rows[0][6].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("appUser", dt.Rows[0][7].ToString()) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = str_PrintTimeYYYY;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0112", repo.GetPrintData(P0)));

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