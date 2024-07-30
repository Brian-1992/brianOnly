using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AA0085 : SiteBase.BasePage
    {
        string WH_NO = "";
        string ExportType = "";
        string YYYMM = "";
        string AGEN_NO = "";
        string IsTCB = "";



        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                WH_NO = Request.QueryString["P0"].ToString().Replace("null", "");
                ExportType = Request.QueryString["P1"].ToString().Replace("null", "");
                YYYMM = Request.QueryString["P2"].ToString().Replace("null", "");
                AGEN_NO = Request.QueryString["P3"].ToString().Replace("null", "");
                IsTCB = Request.QueryString["IsTCB"].ToString().Replace("null", "");

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
                    AA0085Repository repo = new AA0085Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    DataTable dt = repo.GetReportSumValue(WH_NO, ExportType, YYYMM, AGEN_NO, IsTCB);
                    DataTable dt1 = repo.GetReportTOTValue(WH_NO, ExportType, YYYMM, AGEN_NO, IsTCB);
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM", YYYMM) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("str_PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM1", dt.Rows[0][0].ToString() =="" ? "0" : dt.Rows[0][0].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM2", dt.Rows[1][0].ToString() == "" ? "0" : dt.Rows[1][0].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM3", dt.Rows[2][0].ToString() == "" ? "0" : dt.Rows[2][0].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CNT1", dt1.Rows[0][0].ToString() == "" ? "0" : dt1.Rows[0][0].ToString()) }); //合庫筆數
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOT1", dt1.Rows[0][1].ToString() == "" ? "0" : dt1.Rows[0][1].ToString()) }); //合庫合約金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOT1_DISC", dt1.Rows[0][2].ToString() == "" ? "0" : dt1.Rows[0][2].ToString()) }); //合庫折讓金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOT1_PAY", dt1.Rows[0][5].ToString() == "" ? "0" : dt1.Rows[0][5].ToString()) }); //合庫實付金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CNT2", dt1.Rows[1][0].ToString() == "" ? "0" : dt1.Rows[1][0].ToString()) }); //非合庫筆數
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOT2", dt1.Rows[1][1].ToString() == "" ? "0" : dt1.Rows[1][1].ToString()) }); //非合庫合約金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOT2_DISC", dt1.Rows[1][2].ToString() == "" ? "0" : dt1.Rows[1][2].ToString()) }); //合庫折讓金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOT2_PAY", dt1.Rows[1][5].ToString() == "" ? "0" : dt1.Rows[1][5].ToString()) }); //合庫實付金額
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0085", repo.Report(WH_NO, ExportType, YYYMM, AGEN_NO, IsTCB)));
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