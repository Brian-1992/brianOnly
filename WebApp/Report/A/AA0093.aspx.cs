using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using JCLib.DB;

using System.Data;

namespace WebApp.Report.A
{
    public partial class AA0093 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                report1Bind();
                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
                this.ReportViewer1.EnableTelemetry = false; // 加快報表載入速度
            }
        }

        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0093Repository repo = new AA0093Repository(DBWork);
                    // AA0093Repository.MI_MAST_QUERY_PARAMS query = new AA0093Repository.MI_MAST_QUERY_PARAMS();
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    String P0 = Request.QueryString["p0"].ToString().Replace("null", ""); // WH_NO
                    String P1 = Request.QueryString["p1"].ToString().Replace("null", ""); // MONTHS
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Inid", Request.QueryString["Inid"].ToString().Split(' ')[0]) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("InidName", Request.QueryString["Inid"].ToString().Split(' ')[1]) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("months", P1) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("who", DBWork.UserInfo.InidName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("report_date", str_PrintTime) });

                    //query.DATA_YM = Request.QueryString["ym"].ToString().Replace("null", ""); ;

                    ReportViewer1.LocalReport.DataSources.Clear();
                    //ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0093", repo.GetPrintData(query)));


                    //DataTable dt = new DataTable();
                    //dt.Columns.Add(new DataColumn("WH_NO", typeof(String)));
                    //dt.Columns.Add(new DataColumn("MMCODE", typeof(String)));
                    //dt.Columns.Add(new DataColumn("MMNAME_C", typeof(String)));
                    //dt.Columns.Add(new DataColumn("MMNAME_E", typeof(String)));
                    //dt.Columns.Add(new DataColumn("EXP_DATE", typeof(String)));
                    //dt.Columns.Add(new DataColumn("LOT_NO", typeof(String)));
                    //dt.Columns.Add(new DataColumn("INV_QTY", typeof(String)));
                    //DataRow dr = dt.NewRow();
                    //dr["WH_NO"] = "WH_NO1";
                    //dr["MMCODE"] = "MMCODE2";
                    //dr["MMNAME_C"] = "MMNAME_C3";
                    //dr["MMNAME_E"] = "MMNAME_E4";
                    //dr["EXP_DATE"] = "EXP_DATE5";
                    //dr["LOT_NO"] = "LOT_NO6";
                    //dr["INV_QTY"] = "INV_QTY7";
                    //dt.Rows.Add(dr);
                    //dt.AcceptChanges();
                    //ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0093", dt)); 
                    
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0093", repo.GetPrintData(P0, P1))); // 


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