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
    public partial class AA0156 : Page
    {
        string startDate;
        string endDate;
        string mat_class;
        string printType;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                startDate = Request.QueryString["STARTDATE"].ToString().Replace("null", "");
                endDate = Request.QueryString["ENDDATE"].ToString().Replace("null", "");
                mat_class = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                printType = Request.QueryString["PRINTTYPE"].ToString().Replace("null", "");
                
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
                    AA0156Repository repo = new AA0156Repository(DBWork);

                    ReportViewer1.EnableTelemetry = false;

                    string HospName = repo.GetHospName();
                    string MatClsName = repo.GetMatClsName(mat_class);

                    ReportViewer1.LocalReport.DisplayName = "AA0156";
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", HospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MatClsName", MatClsName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintType", printType) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintUser", repo.GetUserName(User.Identity.Name)) });
                    IEnumerable<Models.AA0156> list = repo.Print(mat_class, startDate, endDate, printType);
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0156", list));

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