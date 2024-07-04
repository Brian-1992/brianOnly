using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AB0084 : System.Web.UI.Page
    {
        string D0 = "", D1 = "", D2 = "", D3 = "", P0;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                D0 = Request.QueryString["D0"].ToString().Replace("null", "");
                D1 = Request.QueryString["D1"].ToString().Replace("null", "");
                D2 = Request.QueryString["D2"].ToString().Replace("null", "");
                D3 = Request.QueryString["D3"].ToString().Replace("null", "");
               

              
                P0 = Request.QueryString["P0"].ToString().Replace("null", "");
                //parDN = Request.QueryString["DN"].ToString().Replace("null", "");
                report1Bind();
            }
        }
        protected void report1Bind()
        {
            string createdatetime1 = "", createdatetime2 = "", finalcreatedatetime1 = "", finalcreatedatetime2 = "";
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0084Repository repo = new AB0084Repository(DBWork);
                    if (D1 != "" && D1 != null)
                    {
                        createdatetime1 = D1.Substring(0, 2) + D1.Substring(3, 2);
                        finalcreatedatetime1 = D0 + createdatetime1;
                    }

                    if (D3 != "" && D3 != null)
                    {
                        createdatetime2 = D3.Substring(0, 2) + D3.Substring(3, 2);
                        finalcreatedatetime2 = D2 + createdatetime2;
                    }

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserId", DBWork.UserInfo.UserId) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("USERNAME", User.Identity.Name) });
   
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0084", repo.GetReport( P0, finalcreatedatetime1, finalcreatedatetime2)));

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