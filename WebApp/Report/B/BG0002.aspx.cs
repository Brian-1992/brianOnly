using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.BG;
using WebApp.Models;
using JCLib.DB;
using JCLib.DB.Tool;
using WebApp.Repository.AA;

namespace WebApp.Report.BG
{
    public partial class BG0002 : System.Web.UI.Page
    {
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                report1Bind();
                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
                this.ReportViewer1.EnableTelemetry = false; // 加快報表載入速度
            }
        } // 

        
        protected void report1Bind()
        {
            FL fl = new FL("WebApp.Report.BG.BG0002");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0002Repository repo = new BG0002Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    WebApp.Models.BG0002 v = new WebApp.Models.BG0002();
                    v.WH_NO = fl.reqQueryString(Request, "WH_NO", "");                          // 庫房代碼
                    DateTime data_ym_start;                                                     // 查詢年月 開始 10807
                    if (DateTime.TryParse(fl.reqQueryString(Request, "DATA_YM_START", ""), out data_ym_start))
                    {
                        v.DATA_YM_START = data_ym_start.ToString("yyyy/MM/01");
                    }
                    DateTime data_ym_end;                                                       // 查詢年月 開始 10807
                    if (DateTime.TryParse(fl.reqQueryString(Request, "DATA_YM_END", ""), out data_ym_end))
                    {
                        v.DATA_YM_END = data_ym_end.ToString("yyyy/MM/" + DateTime.DaysInMonth(data_ym_end.Year, data_ym_end.Month));
                    }
                    v.MAT_CLASS = fl.reqQueryString(Request, "MAT_CLASS", "");                  // 物料分類
                    v.MAT_CLASS_NAME = fl.reqQueryString(Request, "MAT_CLASS_NAME", "");        // 物料分類
                    v.RADIO_BUTTON = fl.reqQueryString(Request, "RADIO_BUTTON", "");            // 管控項目 0-庫備品、1-非庫備品(排除鎖E品項) 、2-庫備品(管控項目)
                    String inid = fl.reqQueryString(Request, "inid", "");                       // 使用單位:庫房別

                    String title_label1 = "";
                    title_label1 += fl.西元年月日轉民國年月(data_ym_start);
                    title_label1 += "至";
                    title_label1 += fl.西元年月日轉民國年月(data_ym_end);
                    title_label1 += "_";
                    title_label1 += System.Web.HttpUtility.UrlDecode(v.MAT_CLASS_NAME);
                    title_label1 += "非合約累積進貨金額表";

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("wh_no", inid) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("title_label1", title_label1) });
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BG0002DataSet", repo.GetPrintData(v)));
                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception ex)
                {
                    //fl.le("report1Bind()", ex.Message);
                    throw;
                } // end of try 
            } // end of use 
        } //


    } // ec
} // en