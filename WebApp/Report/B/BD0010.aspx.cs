using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.B;

namespace WebApp.Report.B
{
    public partial class BD0010 : SiteBase.BasePage
    {
        string parWH_NO = "";
        string parYYYYMMDD = "";
        string parYYYYMMDD_E = "";
        string parPO_STATUS = "";
        string parAgen_No = "";
        string parTitle = "";
        string parFrom = "";
        string parCONTRACT = "";


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parWH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                parYYYYMMDD = Request.QueryString["YYYYMMDD"].ToString().Replace("null", "");
                parYYYYMMDD_E = Request.QueryString["YYYYMMDD_E"].ToString().Replace("null", "");
                parPO_STATUS = Request.QueryString["PO_STATUS"].ToString().Replace("null", "");
                parAgen_No = Request.QueryString["Agen_No"].ToString().Replace("null", "");
                parFrom = Request.QueryString["RptFrom"].ToString().Replace("null", "");
                parCONTRACT = "";
                if (parYYYYMMDD == parYYYYMMDD_E)
                    parTitle = "採購日期 : " + parYYYYMMDD;
                else
                    parTitle = "採購日期 : "+parYYYYMMDD + "~" + parYYYYMMDD_E;
                if (parFrom == "AA0068-1")
                {
                    parCONTRACT = Request.QueryString["CONTRACT"].ToString();
                    if (parCONTRACT.Equals("零購"))
                        parTitle += " (零購)";
                    else if (parCONTRACT.Equals("合約")) parTitle += " (合約)";
                }

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
                    BD0010Repository repo = new BD0010Repository(DBWork);
                    int total_cnt = 0;
                    if (parFrom == "AA0068-1")
                        total_cnt = repo.TotalCntAA0068(parWH_NO, parYYYYMMDD, parYYYYMMDD_E, parCONTRACT, parAgen_No);
                    else
                        total_cnt = repo.GetReportTotalCnt(parWH_NO, parYYYYMMDD, parYYYYMMDD_E, parPO_STATUS, parAgen_No);
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");                   
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("str_PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parTitle", parTitle) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parFrom", parFrom) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOTALCNT", total_cnt.ToString()) });
                    ReportViewer1.EnableTelemetry = false;

                    ReportViewer1.LocalReport.DataSources.Clear();
                    if (parFrom == "AA0068-1")
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BD0010", repo.ReportAA0068(parWH_NO, parYYYYMMDD, parYYYYMMDD_E, parCONTRACT, parAgen_No)));
                    else 
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BD0010", repo.Report(parWH_NO, parYYYYMMDD, parYYYYMMDD_E, parPO_STATUS, parAgen_No)));
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