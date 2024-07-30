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
    public partial class AA0086 : Page
    {
        string startDate;
        string endDate;
        string printType;
        string printUser;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                startDate = Request.QueryString["STARTDATE"].ToString().Replace("null", "");
                endDate = Request.QueryString["ENDDATE"].ToString().Replace("null", "");
                printType = Request.QueryString["PRINTTYPE"].ToString().Replace("null", "");
                printUser = Request.QueryString["PRINTUSER"].ToString().Replace("null", "");
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
                    AA0086Repository repo = new AA0086Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;

                    /* int total_price = repo.GetReportTotalPrice(parDN);
                     ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOTALPRICE", total_price.ToString() )});
                     ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CHTTOTALPRICE", JCLib.ChtNumber.Transform(total_price)) });

                     ReportViewer1.LocalReport.DataSources.Clear();
                     ReportViewer1.LocalReport.DisplayName = parDN;*/
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintType", printType ) });
                    IEnumerable<AA0086M> list = repo.Print(startDate, endDate, printType, printUser);
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0086", list));

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