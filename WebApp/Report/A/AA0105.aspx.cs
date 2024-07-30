using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Reflection;
using System.Web;

namespace WebApp.Report.A
{
    public partial class AA0105 : SiteBase.BasePage
    {
        /// <summary>
        /// 庫房代碼
        /// </summary>
        string P0;

        /// <summary>
        /// 院內碼
        /// </summary>
        string P1;

        /// <summary>
        /// 查詢基準量ro查詢超出
        /// </summary>
        string P2;

        /// <summary>
        /// 物料分類
        /// </summary>
        string P3;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //RDLC本身的列印功能鍵
                //ReportViewer1.ShowPrintButton = false;
                //ReportViewer1.ShowExportControls = false;
                P0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = (Request.QueryString["p2"] is null) ? "" : Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = (Request.QueryString["p3"] is null) ? "" : Request.QueryString["p3"].ToString().Replace("null", "");

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
                    AA0105Repository repo = new AA0105Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();


                    //停用收集使用方式資料，加快報表顯示
                    ReportViewer1.EnableTelemetry = false;

                    //基準量查詢與超出量查詢報表路徑設定
                    ReportViewer1.LocalReport.ReportPath = P2 == "1" ? @"Report\A\AA0105_Basic.rdlc" : @"Report\A\AA0105_Over.rdlc";

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    //string str_YearMonth = P1;
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YearMonth", str_YearMonth) });

                    string str_UserName = DBWork.UserInfo.UserName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintUser", str_UserName) });

                    string str_UserDept = DBWork.UserInfo.InidName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserDept", str_UserDept) });

                    ReportViewer1.PageCountMode = PageCountMode.Actual;

                    //先清空報表的DataSet，再將讀到的AA0105 DataTable放到DataSources(對應到AA0105.xsd)
                    ReportViewer1.LocalReport.DataSources.Clear();
                    if (P2 == "1")
                    {
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0105", repo.SearchReportData_Basic(P0, P1, P3)));
                    }
                    else
                    {
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0105", repo.SearchReportData_Over(P0, P1, P3)));
                    }

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