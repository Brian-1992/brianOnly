using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AB0122 : SiteBase.BasePage
    {
        string parDATA_YM;
        string parROWNUM;
        string parNONE_COST;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parDATA_YM = Request.QueryString["DATA_YM"].ToString().Replace("null", "");
                parROWNUM = Request.QueryString["ROWNUM"].ToString().Replace("null", "");
                parNONE_COST = Request.QueryString["NONE_COST"].ToString().Replace("null", "");
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
                    AB0122Repository repo = new AB0122Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserId", repo.getUserName(User.Identity.Name)) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "藥品消耗結存月報表";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0122", repo.GetPrintData(parDATA_YM, parROWNUM, parNONE_COST)));

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