using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.B;

namespace WebApp.Report.B
{
    public partial class BD0022 : SiteBase.BasePage
    {
        string rdlc;
        string PO_NO = "";
        string p0 = "";    // 訂單號碼
        string p0_1 = "";
        string p1 = "";    // 物料分類
        string p2 = "";    // 合約識別碼
        string p3 = "";    // 訂單日期起
        string p3_1 = "";    // 訂單日期訖
        string p4 = "";  //廠商代碼
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                rdlc = Request.QueryString["rdlc"].ToString();
                PO_NO = Request.QueryString["PO_NO"].ToString().Replace("null", "");

                p0 = Request.QueryString["p0"].ToString().Replace("null", "");
                p0_1 = Request.QueryString["p0_1"].ToString().Replace("null", "");
                p1 = Request.QueryString["p1"].ToString().Replace("null", "");
                p2 = Request.QueryString["p2"].ToString().Replace("null", "");
                p3 = Request.QueryString["p3"].ToString().Replace("null", "");
                p3_1 = Request.QueryString["p3_1"].ToString().Replace("null", "");
                p4 = Request.QueryString["p4"].ToString().Replace("null", "");
                if (rdlc == "1")  //列印
                    report1Bind();
                else if (rdlc == "2") //清單列印
                    report2Bind();
                else if (rdlc == "3") //清單列印
                {
                    report2Bind();
                }
            }
        }
      
        protected void report1Bind()
        {
            ReportViewer1.Visible = true;
            ReportViewer2.Visible = false;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0022Repository repo = new BD0022Repository(DBWork);

                    string hospName = repo.GetHospName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    DataTable dt = repo.GetReportMain(PO_NO);

                    string[] po_time = dt.Rows[0]["PO_TIME"].ToString().Trim().Split('/');
                    string po_time_str = (Convert.ToInt32(po_time[0]) - 1911) + "年" + po_time[1] + "月" + po_time[2] + "日";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PO_NO", PO_NO) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PO_TIME", po_time_str) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN", dt.Rows[0]["AGEN_NO"].ToString().Trim() + " " + dt.Rows[0]["AGEN_NAMEC"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_TEL", dt.Rows[0]["AGEN_TEL"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_FAX", dt.Rows[0]["AGEN_FAX"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MEMO", dt.Rows[0]["MEMO"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SMEMO", dt.Rows[0]["SMEMO"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("REC_ADDR", dt.Rows[0]["REC_ADDR"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CONTACT", dt.Rows[0]["CONTACT"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AMOUNT", dt.Rows[0]["AMOUNT"].ToString().Trim()) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BD0022", repo.GetReport(PO_NO)));
                    ReportViewer1.LocalReport.DisplayName = "訂購單("+PO_NO+")_" + DateTime.Now.ToString("yyyyMMddHHmmss"); //匯出指定檔名

                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception e)
                {
                    throw;
                }

            }
        }

        protected void report2Bind()
        {
            ReportViewer1.Visible = false;
            ReportViewer2.Visible = true;

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0022Repository repo = new BD0022Repository(DBWork);
                    string hospName = repo.GetHospName();
                    ReportViewer2.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day;
                    ReportViewer2.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    ReportViewer2.LocalReport.DataSources.Clear();
                    if (rdlc == "2")
                    { 
                        ReportViewer2.LocalReport.DataSources.Add(new ReportDataSource("BD0022", repo.GetListReport(p0, p0_1, p1, p2, p3, p3_1, p4)));
                    }
                    else
                    {
                        string[] arr_pono = { };
                        if (!string.IsNullOrEmpty(PO_NO))
                        {
                            arr_pono = PO_NO.Trim().Split(','); //用,分割
                        }
                        ReportViewer2.LocalReport.DataSources.Add(new ReportDataSource("BD0022", repo.GetListReport(p0, p0_1, p1, p2, p3, p3_1, p4, arr_pono)));

                    }
                    ReportViewer2.LocalReport.DisplayName = "訂購單清冊明細表_" + DateTime.Now.ToString("yyyyMMddHHmmss"); //匯出指定檔名

                    ReportViewer2.LocalReport.Refresh();
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

    }
}