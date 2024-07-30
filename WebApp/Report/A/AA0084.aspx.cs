using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using JCLib.DB;
using WebApp.Repository.AA;
using Microsoft.Reporting.WebForms;
using WebApp.Models;

namespace WebApp.Report.A
{
    public partial class AA0084 : Page
    {
        string startDate;
        string endDate;
        string wh_no;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                startDate = Request.QueryString["STARTDATE"].ToString().Replace("null", "");
                endDate = Request.QueryString["ENDDATE"].ToString().Replace("null", "");
                wh_no = Request.QueryString["WH_NO"].ToString().Replace("null", "");
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
                    AA0084Repository repo = new AA0084Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    

                    ReportViewer1.EnableTelemetry = false;

                    //基準量查詢與超出量查詢報表路徑設定
                    ReportViewer1.LocalReport.ReportPath =(wh_no == "PH1S" || wh_no == "PH1X") ? @"Report\A\AA0084.rdlc" : @"Report\A\AA0084_1.rdlc";

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    //string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    //string wh_name = repo.GetWhName(wh_no);
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WhName", str_PrintTime) });

                    /* int total_price = repo.GetReportTotalPrice(parDN);
                     ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOTALPRICE", total_price.ToString() )});
                     ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CHTTOTALPRICE", JCLib.ChtNumber.Transform(total_price)) });

                     ReportViewer1.LocalReport.DataSources.Clear();
                     ReportViewer1.LocalReport.DisplayName = parDN;*/

                    IEnumerable<AA0084M> list = repo.Print(startDate, endDate, wh_no);
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0084", list));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }


        }
    }
}