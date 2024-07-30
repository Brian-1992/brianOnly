using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AB0065 : SiteBase.BasePage
    {
        string D0="", D1 = "", P0, P1, P2, P3, P4, TRCODE1;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                D0 = Request.QueryString["D0"].ToString().Replace("null", "");
                D1 = Request.QueryString["D1"].ToString().Replace("null", "");

                P0 = Request.QueryString["P0"].ToString().Replace("null", "");
                P1 = Request.QueryString["P1"].ToString().Replace("null", "");
                P2 = Request.QueryString["P2"].ToString().Replace("null", "");
                P3 = Request.QueryString["P3"].ToString().Replace("null", "");
                P4 = Request.QueryString["P4"].ToString().Replace("null", "");

                TRCODE1 = Request.QueryString["TRCODE1"].ToString().Replace("null", "");


                report1Bind();
                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
            }
        }
        protected void report1Bind()
        {
            string finalfinddate1 = "", finalfinddate2 = "";
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    ReportViewer1.EnableTelemetry = false;

                    //正規化查詢日期
                    if (D0 != "" && D0 != null)
                    {
                        finalfinddate1 = D0.Substring(0, 3) + "/" + D0.Substring(3, 2) + "/" + D0.Substring(5, 2);
                    }
                    if(D1 !="" && D1 != null)
                    {
                        finalfinddate2 = D1.Substring(0, 3) + "/" + D1.Substring(3, 2) + "/" + D1.Substring(5, 2);
                    }

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("finddate1", finalfinddate1) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("finddate2", finalfinddate2) });


                    //寫入報表
                    AB0065Repository repo = new AB0065Repository(DBWork);
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.ToString("MM/dd") + " " + DateTime.Now.ToString("hh:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserId", DBWork.UserInfo.UserId) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserName", DBWork.UserInfo.UserName) });



                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.GetPrintData(TRCODE1,D0, D1, P0, P1, P2, P3, P4)));

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