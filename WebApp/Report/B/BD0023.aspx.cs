using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.B;

namespace WebApp.Report.B
{
    public partial class BD0023 : SiteBase.BasePage
    {
        string PO_NO = "";
        string MAT_CLASS = "";
        string M_CONTID = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PO_NO = Request.QueryString["p0"].ToString().Replace("null", "");
                MAT_CLASS = Request.QueryString["p1"].ToString().Replace("null", "");
                M_CONTID = Request.QueryString["p2"].ToString().Replace("null", "");

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
                    BD0023Repository repo = new BD0023Repository(DBWork);

                    string hospName = repo.GetHospName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "年" + DateTime.Now.Month + "月" + DateTime.Now.Day + "日";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BD0023", repo.GetReport(PO_NO, MAT_CLASS, M_CONTID)));
                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception e)
                {
                    throw;
                }

            }
        }
    }
}