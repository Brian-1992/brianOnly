using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;
using JCLib.Report;
using System.Web.UI.WebControls;

namespace WebApp.Report.A
{
    public partial class AA0174 : SiteBase.BasePage
    {
        string userID, p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16;

        protected void Page_Load(object sender, EventArgs e)
        {
            HiddenField antiforgery = (HiddenField)this.FindControl("_token");
            // check if csrf
            AntiforgeryChecker.Check(this, antiforgery);

            if (!IsPostBack)
            {
                userID = User.Identity.Name;
                p0 = Request.QueryString["p0"].ToString().Replace("null", "");
                p1 = Request.QueryString["p1"].ToString().Replace("null", "");
                p2 = Request.QueryString["p2"].ToString().Replace("null", "");
                p3 = Request.QueryString["p3"].ToString().Replace("null", "");
                p4 = Request.QueryString["p4"].ToString().Replace("null", "");
                p5 = Request.QueryString["p5"].ToString().Replace("null", "");
                p6 = Request.QueryString["p6"].ToString().Replace("null", "");
                p7 = Request.QueryString["p7"].ToString().Replace("null", "");
                p8 = Request.QueryString["p8"].ToString().Replace("null", "");
                p10 = Request.QueryString["p10"].ToString().Replace("null", "");
                p11 = Request.QueryString["p11"].ToString().Replace("null", "");
                p12 = Request.QueryString["p12"].ToString().Replace("null", "");
                p13 = Request.QueryString["p13"].ToString().Replace("null", "");
                p14 = Request.QueryString["p14"].ToString().Replace("null", "");
                p15 = Request.QueryString["p15"].ToString().Replace("null", "");
                p16 = Request.QueryString["p16"].ToString().Replace("null", "");

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
                    AA0174Repository repo = new AA0174Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    string str_UserDept = DBWork.UserInfo.InidName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserDept", str_UserDept) });

                    ReportViewer1.LocalReport.DataSources.Clear();

                    IEnumerable<AA0174PrintModel> list = repo.GetPrintData(p0, p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, p12, p13, p14, p15, p16);
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0174", list));

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