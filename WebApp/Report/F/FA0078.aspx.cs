using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Repository.F;

namespace WebApp.Report.F
{
    public partial class FA0078 : SiteBase.BasePage
    {
        string MAT_CLASS = "";
        string ExportType = "";
        string YYYMM = "";
        string IsTCB = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                MAT_CLASS = Request.QueryString["P0"].ToString().Replace("null", "");
                ExportType = Request.QueryString["P1"].ToString().Replace("null", "");
                YYYMM = Request.QueryString["P2"].ToString().Replace("null", "");
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
                    FA0078Repository repo = new FA0078Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    DataTable dt_IsTCB = repo.GetReportSumValue(MAT_CLASS, ExportType, YYYMM, "0");
                    DataTable dt_NotTCB = repo.GetReportSumValue(MAT_CLASS, ExportType, YYYMM, "1");
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM", YYYMM) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("str_PrintTime", str_PrintTime) });
                    if (dt_IsTCB.Rows.Count > 0 && (IsTCB == "0" || IsTCB == ""))
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CNT1", dt_IsTCB.Rows[0][5].ToString() == "" ? "0" : dt_IsTCB.Rows[0][5].ToString()) }); //合庫筆數
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM1", dt_IsTCB.Rows[0][4].ToString() == "" ? "0" : dt_IsTCB.Rows[0][4].ToString()) }); //合庫金額
                    }
                    else
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CNT1", "0") }); //合庫筆數
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM1", "0") }); //合庫金額
                    }
                    if (dt_NotTCB.Rows.Count > 0 && (IsTCB == "1" || IsTCB == ""))
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CNT2", dt_NotTCB.Rows[0][5].ToString() == "" ? "0" : dt_NotTCB.Rows[0][5].ToString()) }); //非合庫筆數
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM2", dt_NotTCB.Rows[0][4].ToString() == "" ? "0" : dt_NotTCB.Rows[0][4].ToString()) }); //非合庫金額
                    }
                    else
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CNT2", "0") }); //非合庫筆數
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM2", "0") }); //非合庫金額
                    }
                        
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0078", repo.Report(MAT_CLASS, ExportType, YYYMM, IsTCB)));
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