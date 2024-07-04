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
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AB0070 : SiteBase.BasePage
    {
        string P0;  //庫別
        string P1;  //異動類別
        string P1_Name;  //異動類別名稱
        string P2;  //出入庫
        string P3;  //異動日期起
        string P4;  //異動日期迄
        string P5;  //院內碼起
        string P6;  //院內碼迄

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //RDLC本身的列印功能鍵
                //ReportViewer1.ShowPrintButton = false;
                //ReportViewer1.ShowExportControls = false;
                P0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
                P1_Name = (Request.QueryString["p1_Name"] is null) ? "" : Request.QueryString["p1_Name"].ToString().Replace("null", "");
                P2 = (Request.QueryString["p2"] is null) ? "" : Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = (Request.QueryString["p3"] is null) ? "" : Request.QueryString["p3"].ToString().Replace("null", "");
                P4 = (Request.QueryString["p4"] is null) ? "" : Request.QueryString["p4"].ToString().Replace("null", "");
                P5 = (Request.QueryString["p5"] is null) ? "" : Request.QueryString["p5"].ToString().Replace("null", "");
                P6 = (Request.QueryString["p6"] is null) ? "" : Request.QueryString["p6"].ToString().Replace("null", "");

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
                    AB0070Repository repo = new AB0070Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    //停用收集使用方式資料，加快報表顯示
                    ReportViewer1.EnableTelemetry = false;

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    string str_DiffCls = P1 + "_" + P1_Name;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DiffCls", str_DiffCls) });

                    string str_PrintUser = DBWork.UserInfo.UserId + " " + DBWork.UserInfo.UserName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintUser", str_PrintUser) });

                    ReportViewer1.PageCountMode = PageCountMode.Actual;

                    //先清空報表的DataSet，再將讀到的AB0070 DataTable放到DataSources(對應到AB0070.xsd)
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0070", repo.SearchReportData(P0, P1, P2, P3, P4, P5, P6)));

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