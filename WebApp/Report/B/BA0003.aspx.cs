using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.B;

namespace WebApp.Report.B
{
    public partial class BA0003 : SiteBase.BasePage
    {
        string parPR_NO;
        //string parINID;
        //string praInidName;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parPR_NO = Request.QueryString["PR_NO"].ToString().Replace("null", "");
                //parINID = Request.QueryString["INID"].ToString().Replace("null", "");
                //praInidName = Request.QueryString["InidName"].ToString().Replace("null", "");

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
                    BA0003Repository repo = new BA0003Repository(DBWork);

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PR_NO", parPR_NO) });

                    //string[] str = repo.GetReport_PARM(parPR_NO, parINID);
                    string[] str = repo.GetReport_PARM(parPR_NO, DBWork.UserInfo.Inid);

                    string MAT_CLSNAME = str[0];
                    string WH_NAME= str[1];

                    ReportViewer1.EnableTelemetry = false;
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("INID_NAME", praInidName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("INID_NAME", DBWork.UserInfo.InidName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MAT_CLSNAME", MAT_CLSNAME) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WH_NAME", WH_NAME) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BA0003", repo.GetReport_Data(parPR_NO)));
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