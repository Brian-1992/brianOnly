using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;

namespace WebApp.Report.A
{
    public partial class AA0123 : SiteBase.BasePage
    {
        string P0;
        string P1;
        bool P2;
        string YYYMM ;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = Request.QueryString["p2"].ToString() == "Y";
                YYYMM = P0 ;
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
                    AA0123Repository repo = new AA0123Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    //var taskid = repo.GetTaskid(User.Identity.Name);
                    if (P1 == "2")
                    {
                        P1 = "2";
                    }
                    else
                    {
                        P1 = "3";
                    }
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() +  DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM", YYYMM) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ReportType", "1") });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Matname", P1) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = YYYMM;
                    if (P2 == true)
                    {
                        IEnumerable < AA0123ReportMODEL > temp = repo.GetPrintDataAll(P0, P1);
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0123", repo.GetPrintDataAll(P0, P1)));
                    }
                    else {
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0123", repo.GetPrintData(P0, P1)));
                    }
                    

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