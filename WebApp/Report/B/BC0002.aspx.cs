using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.BC;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.B
{
    public partial class BC0002 : Page
    {
        string parDN;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parDN = Request.QueryString["DN"].ToString().Replace("null", "");
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
                    ReportViewer1.EnableTelemetry = false;

                    BC0002Repository repo = new BC0002Repository(DBWork);

                    foreach (PH_SMALL_M smdata in repo.GetSmData(parDN))
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("USEWHERE", smdata.USEWHERE) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DEMAND", smdata.DEMAND) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ALT", smdata.ALT) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("USEWHEN", smdata.USEWHEN) });
                        //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("INID", smdata.INID) });
                        //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("INIDNAME", smdata.INIDNAME) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TEL", smdata.TEL) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DUEDATE", smdata.DUEDATE) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DELIVERY", smdata.DELIVERY) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ACCEPT", smdata.ACCEPT) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PAYWAY", smdata.PAYWAY) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("OTHERS", smdata.OTHERS) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DEPT", smdata.DEPT) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DO_USER", smdata.DO_USER) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DOTEL", smdata.DOTEL) });
                    }

                    int total_price = repo.GetReportTotalPrice(parDN);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOTALPRICE", total_price.ToString() )});
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CHTTOTALPRICE", JCLib.ChtNumber.Transform(total_price)) });
                    
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = parDN;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BC0002", repo.GetReport(parDN)));

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