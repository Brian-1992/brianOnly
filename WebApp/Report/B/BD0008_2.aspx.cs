using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.B;

namespace WebApp.Report.B
{
    public partial class BD0008_2 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string parWH_NO;
            if (!IsPostBack)
            {
                if (!IsPostBack)
                {
                    parWH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                    report1Bind(parWH_NO);
                }
            }
        }

        protected void report1Bind(string parWH_NO)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0008Repository repo = new BD0008Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BD0008", repo.Report2(parWH_NO)));
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