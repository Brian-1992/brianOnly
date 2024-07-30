using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AA0085_2 : SiteBase.BasePage
    {
        string parP0;
        string parP1;
        string parP2;
        string parP3;
        string parP4;
        string parP5;
        string parIsTCB;



        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parP0 = Request.QueryString["P0"].ToString().Replace("null", "");
                parP1 = Request.QueryString["P1"].ToString().Replace("null", "");
                parP2 = Request.QueryString["P2"].ToString().Replace("null", "");
                parP3 = Request.QueryString["P3"].ToString().Replace("null", "");
                parP4 = Request.QueryString["P4"].ToString().Replace("null", "");
                parP5 = Request.QueryString["P5"].ToString().Replace("null", "");
                parIsTCB = Request.QueryString["IsTCB"].ToString().Replace("null", "");


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
                    AA0085Repository repo = new AA0085Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMM", parP2) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("str_PrintTime", str_PrintTime) });
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0068", repo.ReportDetail(parP0, parP1, parP2, parP3, parP5, parIsTCB)));

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