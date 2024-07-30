using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.C;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.C
{
    public partial class CE0032 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string chkYM = Request.QueryString["chkYM"].ToString().Replace("null", "");
                string P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                string P2 = Request.QueryString["p2"].ToString().Replace("null", "");
                string printDate= (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss"); ;

                using (WorkSession session = new WorkSession(this))
                {
                    UnitOfWork DBWork = session.UnitOfWork;
                    try
                    {
                        CE0032Repository repo = new CE0032Repository(DBWork);

                        ReportViewer1.EnableTelemetry = false;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintDate", printDate) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ChkYM", chkYM.Substring(0, 3) + "年" + chkYM.Substring(3, 2) + "月") });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Userid", DBWork.UserInfo.UserId) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Username", DBWork.UserInfo.UserName) });
                        ReportViewer1.LocalReport.DataSources.Clear();
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.GetPrintData(chkYM, P1, P2)));
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
}