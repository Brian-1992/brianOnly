using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.CB;

namespace WebApp.Report.C
{
    public partial class CB0010 : Page
    {
        string parWH_NO;
        string parSTORE_LOC;
        string parFLAG;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parWH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                parSTORE_LOC = Request.QueryString["STORE_LOC"].ToString().Replace("null", "");
                parFLAG = Request.QueryString["FLAG"].ToString().Replace("null", "");
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
                    CB0010Repository repo = new CB0010Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CB0010", repo.GetSmData(parWH_NO, parSTORE_LOC, parFLAG)));
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