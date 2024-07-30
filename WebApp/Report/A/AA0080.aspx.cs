using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AA0080 : SiteBase.BasePage
    {
        string parWH_NO;
        string parYYYYMM;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parWH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                parYYYYMM = Request.QueryString["YYYYMM"].ToString().Replace("null", "");

                report1Bind();
            }
        }

        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0080Repository repo = new AA0080Repository(DBWork);

                    string IdName= DBWork.UserInfo.UserId+ " "+DBWork.UserInfo.UserName;



                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("IdName", IdName) });
                    ReportViewer1.EnableTelemetry = false;
                    //ReportViewer1.LocalReport.DisplayName = "藥庫一～四級管制藥品出入帳明細總表（月報表）";
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0080", repo.Report(parWH_NO, parYYYYMM)));
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