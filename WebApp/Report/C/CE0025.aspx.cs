using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.C;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.C
{
    public partial class CE0025 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string chkYM = Request.QueryString["chkYM"].ToString().Replace("null", "");
                string printDate= (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss"); ;

                using (WorkSession session = new WorkSession(this))
                {
                    UnitOfWork DBWork = session.UnitOfWork;
                    try
                    {
                        CE0025Repository repo = new CE0025Repository(DBWork);

                        ReportViewer1.EnableTelemetry = false;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("InidName", DBWork.UserInfo.InidName) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintDate", printDate) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ChkYM", chkYM.Substring(0, 3) + "年" + chkYM.Substring(3, 2) + "月") });
                        ReportViewer1.LocalReport.DataSources.Clear();
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0025", repo.GetQueryData("rdlc", chkYM, 0, 0, "")));
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