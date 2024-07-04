using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AA0068 : SiteBase.BasePage
    {
        string parP0;
        string parP1;
        string parP2;
        string parP3;
        string parP4;



        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parP0 = Request.QueryString["P0"].ToString().Replace("null", "");
                parP1 = Request.QueryString["P1"].ToString().Replace("null", "");
                parP2 = Request.QueryString["P2"].ToString().Replace("null", "");
                parP3 = Request.QueryString["P3"].ToString().Replace("null", "");
                parP4 = Request.QueryString["P4"].ToString().Replace("null", "");

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
                    string YYYMMDD_B = parP1.Substring(0, 3) + "/" + parP1.Substring(3, 2) + "/" + parP1.Substring(5, 2);
                    string YYYMMDD_E = parP2.Substring(0, 3) + "/" + parP2.Substring(3, 2) + "/" + parP2.Substring(5, 2);
                    string CONTRACT = "";
                    if (parP3.Equals("零購"))
                        CONTRACT = "(零購)";
                    else if (parP3.Equals("合約")) CONTRACT = "(合約)";
                    AA0068Repository repo = new AA0068Repository(DBWork);
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("str_PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMMDD_B", YYYMMDD_B) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYMMDD_E", YYYMMDD_E) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CONTRACT", CONTRACT) });
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();

                    string hospCode = repo.GetHospCode();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0068", repo.Report(parP0, parP1, parP2, parP3, parP4, hospCode=="0")));

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