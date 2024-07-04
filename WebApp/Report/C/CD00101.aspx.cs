using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.C;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;
using System.Drawing;
using BarcodeLib;
using System.Drawing.Imaging;

namespace WebApp.Report.C
{
    public partial class CD00101 : Page
    {
        string P0;
        string P1;
        string P2;
        string P3;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P0 = Request.QueryString["P0"].ToString().Replace("null", "");
                P1 = Request.QueryString["P1"].ToString().Replace("null", "");
                P2 = Request.QueryString["P2"].ToString().Replace("null", "");
                P3 = Request.QueryString["P3"].ToString().Replace("null", "");

                report1Bind();
            }
        }
        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0010Repository repo = new CD0010Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    string[] arr_p3 = { };
                    if (!string.IsNullOrEmpty(P3))
                    {
                        arr_p3 = P3.Trim().Split(','); //用,分割
                    }
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Userid", User.Identity.Name) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Username", repo.getUserName(User.Identity.Name)) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = "";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CD00101", repo.GetPrintData1(P0, P1, P2, arr_p3)));
                    
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