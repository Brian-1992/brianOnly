using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;
using System.Reflection;
using WebApp.Repository.B;
using System.IO;
using System.Web;

namespace WebApp.Report.B
{
    public partial class BG0009M : SiteBase.BasePage
    {
        string pDATA_YM = "", agenno = "", YYY = "", MM = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pDATA_YM = (Request.QueryString["pDATA_YM"] is null) ? "" : Request.QueryString["pDATA_YM"].ToString().Replace("null", "");
                agenno = (Request.QueryString["p2"] is null) ? "" : Request.QueryString["p2"].ToString().Replace("null", "");

                YYY = pDATA_YM.Substring(0, 3);
                MM = pDATA_YM.Substring(3, 2);
                if (MM.Substring(0, 1) == "0")
                {
                    MM = pDATA_YM.Substring(4, 1);
                }
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
                    BG0009Repository repo = new BG0009Repository(DBWork);

                    string hospName = repo.GetHospName();
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "年" + DateTime.Now.Month + "月" + DateTime.Now.Day + "日 ";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    //停用收集使用方式資料，加快報表顯示
                    ReportViewer1.EnableTelemetry = false;

                    DataTable dt = repo.GetReportMain(agenno);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYY", YYY) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MM", MM) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("M_AGENNO", dt.Rows[0]["M_AGENNO"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_NAME", dt.Rows[0]["AGEN_NAME"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_TEL", dt.Rows[0]["AGEN_TEL"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_FAX", dt.Rows[0]["AGEN_FAX"].ToString().Trim()) });

                    //先清空報表的DataSet，再將讀到的BG0009M DataTable放到DataSources(對應到BG0009M.xsd)
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BG0009M", repo.GetPrintData(pDATA_YM, agenno)));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BG0009Count", repo.GetPrintDetails(pDATA_YM, agenno)));
                    ReportViewer1.LocalReport.DisplayName = "廠商貨款對帳單(" + agenno + ")" + DateTime.Now.ToString("yyyyMMddHHmmss"); //匯出指定檔名

                    //更新頁面上的報表
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