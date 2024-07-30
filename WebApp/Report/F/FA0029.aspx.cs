using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Repository.AA;

namespace WebApp.Report.F
{
    public partial class FA0029 : SiteBase.BasePage
    { 
        protected void Page_Load(object sender, EventArgs e)
        {
            string P1;

            if (!IsPostBack)
            {
                P1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
             
                using (WorkSession session = new WorkSession(this))
                {
                    UnitOfWork DBWork = session.UnitOfWork;
                    try
                    {
                        FA0029Repository repo = new FA0029Repository(DBWork);
                        AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                        string hospName = repo_rdlc.GetHospName();
                        string hospFullName = repo_rdlc.GetHospFullName();
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                        string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                        
                        string str_ReportTittle = P1 + "申領品項總材積明細表";
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ReportTittle", str_ReportTittle) });

                        string str_UserDept = DBWork.UserInfo.InidName;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("InidName", str_UserDept) });



                        ReportViewer1.LocalReport.DataSources.Clear();
                        // ReportViewer1.LocalReport.DisplayName = parDN;
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0029", repo.GetReport(P1)));

                        ReportViewer1.LocalReport.Refresh();
                    }
                    catch
                    {
                        throw;
                    }
                    //return session.Result;
                }
                //parDN = Request.QueryString["DN"].ToString().Replace("null", "");
                //report1Bind();
            }
        }

    }
}