using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;

namespace WebApp.Report.A
{
    public partial class AB0022 : SiteBase.BasePage
    {
        string docnoString;
        string[] docnos;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                docnoString = Request.QueryString["DOCNOS"].ToString().Replace("null", "0");
                docnos = docnoString.Split('|');

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
                    AB0022Repository repo = new AB0022Repository(DBWork);

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.ToString("MM/dd") + " " + DateTime.Now.ToString("hh:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Userid", DBWork.UserInfo.UserId) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Username", DBWork.UserInfo.UserName) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "";
                    

                    IEnumerable<Models.AB0022> temp = repo.GetPrint(docnos);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0022", temp));

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