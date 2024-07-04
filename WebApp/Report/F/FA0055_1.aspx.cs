using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;
using WebApp.Models.MI;
using System.Collections.Generic;

namespace WebApp.Report.F
{
    public partial class FA0055_1 : SiteBase.BasePage
    {
        string parDATA_YM;
        string userID;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                userID = User.Identity.Name;
                parDATA_YM = Request.QueryString["DATA_YM"].ToString().Replace("null", "");               
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
                    FA0055Repository repo = new FA0055Repository(DBWork);
                    MI_MNSET mnset = repo.GetMimnset(parDATA_YM);


                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();

                    string data_ym = string.Format("{0}年{1}月", parDATA_YM.Substring(0, 3), parDATA_YM.Substring(3, 2) );
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DATA_YM", data_ym) });

                    IEnumerable<Models.FA0055> temp = repo.GetPrint(parDATA_YM);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0055", temp));

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