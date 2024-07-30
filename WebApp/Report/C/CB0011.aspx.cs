using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.C;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.C
{
    public partial class CB0011 : System.Web.UI.Page
    {
        string BOXNO;
        string BARCODE;
        string STATUS;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BOXNO = Request.QueryString["BOXNO"];
                BARCODE = Request.QueryString["BARCODE"];
                STATUS = Request.QueryString["STATUS"];
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
                    CB0011Repository repo = new CB0011Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.Print(BOXNO, BARCODE, STATUS)));
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