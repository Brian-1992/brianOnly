using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;
namespace WebApp.Report.A
{
    public partial class AB0058 : SiteBase.BasePage
    {
        string P1 = "";
        string P2= "";
        string P3 = "";
        string P4 = "";
        string P5 = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = Request.QueryString["p3"].ToString().Replace("null", "");
                P4 = Request.QueryString["p4"].ToString().Replace("null", "");
                P5 = Request.QueryString["p5"].ToString().Replace("null", "");
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
                    AB0058Repository repo = new AB0058Repository(DBWork);

                     //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CHTTOTALPRICE", JCLib.ChtNumber.Transform(total_price)) });
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("USERNAME", User.Identity.Name) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UID", User.Identity.Name) });
                    string str_UserDept = DBWork.UserInfo.InidName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserDept", str_UserDept) });


                    ReportViewer1.LocalReport.DataSources.Clear();
                    // ReportViewer1.LocalReport.DisplayName = parDN;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0058", repo.GetReport(P1,P2,P3,P4,P5)));

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