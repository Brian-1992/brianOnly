using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.B;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.B
{
    public partial class BD0009 : System.Web.UI.Page
    {
        string parWH_NO;
        string parPURDATE;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parWH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                parPURDATE = Request.QueryString["PURDATE"].ToString().Replace("null", "");
                reportBind();
            }
        }

        protected void reportBind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    ReportViewer1.EnableTelemetry = false;

                    BD0009Repository repo = new BD0009Repository(DBWork);

                    int total_cnt = repo.GetReportTotalCnt(parWH_NO, parPURDATE);
                    int total_adprice = 0;
                    int total_price = 0;
                    if (total_cnt > 0)
                    {
                        total_adprice = repo.GetReportTotalAdPrice(parWH_NO, parPURDATE);
                        total_price = repo.GetReportTotalPrice(parWH_NO, parPURDATE);
                    }
                        
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOTALCNT", total_cnt.ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOTALADPRICE", total_adprice.ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOTALPRICE", total_price.ToString()) });
        
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "藥庫藥品申購報表";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BD0009", repo.GetReport(parWH_NO, parPURDATE)));

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