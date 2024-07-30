using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Linq;
using WebApp.Repository.H;
using WebApp.Repository.AA;

namespace WebApp.Report.HA
{
    public partial class HA0001 : System.Web.UI.Page
    {
        string PrintType;
        string RemitnoFrom;
        string RemitnoTo;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PrintType = (Request.QueryString["PrintType"] is null) ? "" : Request.QueryString["PrintType"].ToString();
                RemitnoFrom = (Request.QueryString["RemitnoFrom"] is null) ? "0" : Request.QueryString["RemitnoFrom"].ToString();
                RemitnoTo = (Request.QueryString["RemitnoTo"] is null) ? "0" : Request.QueryString["RemitnoTo"].ToString();

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
                    HA0001Repository repo = new HA0001Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_TWNToday = (DateTime.Now.Year - 1911).ToString() + "年" + DateTime.Now.Month + "月" + DateTime.Now.Day + "日";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TWNToday", str_TWNToday) });

                    //停用收集使用方式資料，加快報表顯示
                    ReportViewer1.EnableTelemetry = false;

                    // ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DOCNO", docno) });
                    
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("HA0001", repo.GetPrintData(PrintType, RemitnoFrom, RemitnoTo)));

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