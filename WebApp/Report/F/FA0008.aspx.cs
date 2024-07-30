using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.F
{
    public partial class FA0008 : SiteBase.BasePage
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
                    FA0008Repository repo = new FA0008Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DeptName", repo.getDeptName()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parDATA_YM", parDATA_YM) });

                    string SubTitle = parDATA_YM + "庫存成本調整報表" ;

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SubTitle", SubTitle) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("User", repo.getUserName()) });

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0008", repo.Print(parDATA_YM)));
                            
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