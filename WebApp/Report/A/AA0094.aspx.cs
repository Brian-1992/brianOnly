using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0094 : System.Web.UI.Page
    {
        string P0 = "", P1 = "", P2 = "", P4 = "", P5 = "",P7="";        
        int P6;
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
                P5 = (Request.QueryString["p3"] is null) ? "" : Request.QueryString["p3"].ToString().Replace("null", "");
                DateTime P3 = DateTime.ParseExact(P0, "yyyMM", new System.Globalization.DateTimeFormatInfo());
                //var P3 = P0.Substring(3, 1);
                //var P4 = P0.Substring(4, 1);
                //DateTime P4 = DateTime.ParseExact(P3, "yyyyMM", new System.Globalization.DateTimeFormatInfo());
              
                if (P5 == "")
                {
                    P4 = P3.AddMonths(-2).ToString("yyyMM");
                    P7 = "3";
                }
              else
                {
                    P6 = int.Parse(P5);
                    P4 = P3.AddMonths(-P6+1).ToString("yyyMM");
                    P7 = P5;
                }
                //DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-1).ToShortDateString();
                report1Bind();
            }
        }

        protected void report1Bind()
        {
            string wname = "";
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0094Repository repo = new AA0094Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    if (P1 != "")
                    {
                        wname = repo.WNAME(P1);
                    }

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                   
                    //string BM = P0;
                    //string EM = P0;
                    //string DEPT = P1;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("USERNAME", User.Identity.Name) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("BM", P0) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("EM", P4) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DEPT", wname) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MM", P7) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0094", repo.GetReport(P0, P1, P2,P4)));
                   
                   // System.Threading.Thread.Sleep(25000);
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