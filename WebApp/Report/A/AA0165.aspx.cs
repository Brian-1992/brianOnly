using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0165 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                report1Bind();
                ReportViewer1.EnableTelemetry = false; // 加快報表載入速度
                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
            }
        } // 

        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0165Repository repo = new AA0165Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });


                    AA0165_MODEL v = new AA0165_MODEL();
                    v.APPTIME_START = Request.QueryString["APPTIME_START"].ToString().Replace("null", "");
                    v.APPTIME_END = Request.QueryString["APPTIME_END"].ToString().Replace("null", "");
                    v.MAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                    v.FRWH = Request.QueryString["FRWH"].ToString().Replace("null", "");
                    v.USERID = User.Identity.Name;

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DeptName", new AA0063Repository(DBWork).getDeptName()) });

                    ReportViewer1.LocalReport.DataSources.Clear();

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0165DataSet", repo.GetPrintData(v)));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }


        } // 

    } // ec 
} // en