using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0167 : Page
    {
        string APPTIME1, APPTIME2, MAT_CLASS, DOCTYPE, MMCODE;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                APPTIME1 = Request.QueryString["APPTIME1"].ToString().Replace("null", "");
                APPTIME2 = Request.QueryString["APPTIME2"].ToString().Replace("null", "");
                MAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                DOCTYPE = Request.QueryString["DOCTYPE"].ToString().Replace("null", "");
                MMCODE = Request.QueryString["MMCODE"].ToString().Replace("null", "");

                report1Bind();
                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
            }
        }
        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0167Repository repo = new AA0167Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");

                    ReportViewer1.LocalReport.DataSources.Clear();
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0167", repo.GetPrintData(APPTIME1, APPTIME2, MAT_CLASS, DOCTYPE, MMCODE, User.Identity.Name)));

                    if (DOCTYPE == "RJ1")
                    {
                        DOCTYPE = "退貨明細表";
                    }
                    else if (DOCTYPE == "EX1")
                    {
                        DOCTYPE = "換貨明細表";
                    }
                    else
                    {
                        DOCTYPE = "退換貨明細表";
                    }
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("INIDNAME", repo.GetINIDNAME(User.Identity.Name)) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DOCTYPE", DOCTYPE) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
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