using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0168 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string matClass = Request.QueryString["matClass"].ToString().Replace("null", "");
                string fromYM = Request.QueryString["fromYM"].ToString().Replace("null", "");
                string toYM = Request.QueryString["toYM"].ToString().Replace("null", "");
                string whNo = Request.QueryString["whNo"].ToString().Replace("null", "");
                string docType = Request.QueryString["docType"].ToString().Replace("null", "");
                string printDate = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                string printTitle = Request.QueryString["printTitle"].ToString().Replace("null", "");
                string frompgm = Request.QueryString["frompgm"].ToString().Replace("null", "");

                using (WorkSession session = new WorkSession(this))
                {
                    UnitOfWork DBWork = session.UnitOfWork;
                    try
                    {
                        AA0168Repository repo = new AA0168Repository(DBWork);
                        AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                        string hospName = repo_rdlc.GetHospName();
                        string hospFullName = repo_rdlc.GetHospFullName();
                        ReportViewer1.PageCountMode = PageCountMode.Actual;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                        ReportViewer1.EnableTelemetry = false;
                        ReportViewer1.LocalReport.DisplayName = printTitle + "單位申請品項表";
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("InidName", DBWork.UserInfo.InidName) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", printDate) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTitle", printTitle) });
                        ReportViewer1.LocalReport.DataSources.Clear();
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0168", repo.GetQueryData("rdlc", matClass, fromYM, toYM, whNo, docType, 0, 0, "", frompgm)));
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0168APL", repo.GetApplyData(matClass, fromYM, toYM, whNo, docType, frompgm)));
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