using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Reflection;
using System.Web;

namespace WebApp.Report.F
{
    public partial class FA0049 : SiteBase.BasePage
    {
        /// <summary>
        /// 物料分類
        /// </summary>
        string P0;

        /// <summary>
        /// 物料分類名稱
        /// </summary>
        string P0_NAME;

        /// <summary>
        /// 院內碼
        /// </summary>
        string P1;

        /// <summary>
        /// 庫備否
        /// </summary>
        string P2;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //RDLC本身的列印功能鍵
                //ReportViewer1.ShowPrintButton = false;
                //ReportViewer1.ShowExportControls = false;
                P0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
                P0_NAME = (Request.QueryString["p0_Name"] is null) ? "" : Request.QueryString["p0_Name"].ToString().Replace("null", "");
                P1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = (Request.QueryString["p2"] is null) ? "" : Request.QueryString["p2"].ToString().Replace("null", "");

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
                    FA0026Repository repo = new FA0026Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    //DataTable tmpdt_FA0017 = LinqQueryToDataTable(repo.GetPrintData(P1, P2, P0));

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    //string str_ReportTittle = "";
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ReportTittle", str_ReportTittle) });

                    string str_UserDept = DBWork.UserInfo.InidName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserDept", str_UserDept) });

                    ReportViewer1.PageCountMode = PageCountMode.Actual;

                    //先清空報表的DataSet，再將讀到的FA0026 DataTable放到DataSources(對應到FA0026.xsd)
                    ReportViewer1.LocalReport.DataSources.Clear();

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0026", repo.GetPrintData(P0, P1, P2)));

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