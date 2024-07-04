using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0151 : System.Web.UI.Page
    {
        string P0;
        //string[] arr_p0 = { };
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
                /*if (!string.IsNullOrEmpty(P0))
                {
                    arr_p0 = P0.Trim().Split(','); //用,分割
                }*/
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
                    AA0151Repository repo = new AA0151Repository(DBWork);

                    ReportViewer1.EnableTelemetry = false;
                    string hospName = repo.GetHospName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "年" + DateTime.Now.ToString("MM") + "月" + DateTime.Now.ToString("dd") + "日";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0151", repo.GetPrintDataM(P0)));
                    ReportViewer1.ShowPrintButton = true;
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