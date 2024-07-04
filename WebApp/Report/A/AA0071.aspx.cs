using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0071 : Page
    {
        string parD0;
        string parD1;
        string parP1;
        string parP2;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parD0 = Request.QueryString["d0"].ToString().Replace("null", "");
                parD1 = Request.QueryString["d1"].ToString().Replace("null", "");
                parP1 = Request.QueryString["p1"].ToString().Replace("null", "");
                parP2 = Request.QueryString["p2"].ToString().Replace("null", "");
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
                    AA0071Repository repo = new AA0071Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0071", repo.GetPrintData(parD0, parD1, parP1, parP2)));

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