using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using JCLib.DB;
using WebApp.Repository.AB;
using Microsoft.Reporting.WebForms;
using WebApp.Models;

namespace WebApp.Report.A
{
    public partial class AB0075_1 : Page
    {
        string MONTH;
        string WH_NO;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                MONTH = Request.QueryString["MONTH"].ToString().Replace("null", "");
                WH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
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
                    AB0075Repository repo = new AB0075Repository(DBWork);
                    
                    IEnumerable<AB0075_1M> list = repo.Print_1(MONTH);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WH_NO", WH_NO) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("USER_NAME", User.Identity.Name) });
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0075_1", list));

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