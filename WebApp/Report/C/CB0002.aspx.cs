using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.C;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.C
{
    public partial class CB0002 : Page
    {
        string parMMCODE;
        string parMMNAME_C;
        string parMMNAME_E;
        string parBARCODE;
        string parSTATUS;
        string parMAT_CLASS;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMMCODE = Request.QueryString["MMCODE"].ToString().Replace("null", "");
                parMMNAME_C = Request.QueryString["MMNAME_C"].ToString().Replace("null", "");
                parMMNAME_E = Request.QueryString["MMNAME_E"].ToString().Replace("null", "");
                parBARCODE = Request.QueryString["BARCODE"].ToString().Replace("null", "");
                parSTATUS = Request.QueryString["STATUS"].ToString().Replace("null", "");
                parMAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
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
                    CB0002Repository repo = new CB0002Repository(DBWork);

                   

                   /* int total_price = repo.GetReportTotalPrice(parDN);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOTALPRICE", total_price.ToString() )});
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CHTTOTALPRICE", JCLib.ChtNumber.Transform(total_price)) });
                    
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = parDN;*/
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CB0002", repo.Print(parMMCODE, parMMNAME_C, parMMNAME_E, parBARCODE, parSTATUS, parMAT_CLASS)));

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