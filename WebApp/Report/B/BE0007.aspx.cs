using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Repository.AA;


namespace WebApp.Report.B
{
    public partial class BE0007 : SiteBase.BasePage
    {
        string p0 = "";   //廠商代碼
        string p1 = "";   //發票號碼
        string p2 = "";   //進貨日期 起
        string p3 = ""; //進貨日期 迄
        string p4 = "";   //月結年月
        string p5 = "";     //物料類別
        string prtStyle = ""; // 1: 列印廠商發票對帳明細(不分頁); 2:列印廠商發票對帳明細(分頁); 3:列印廠商發票對帳明細(A3)


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                p0 = Request.QueryString["p0"].ToString().Replace("null", "");
                p1 = Request.QueryString["p1"].ToString().Replace("null", "");
                p2 = Request.QueryString["p2"].ToString().Replace("null", "");
                p3 = Request.QueryString["p3"].ToString().Replace("null", "");
                p4 = Request.QueryString["p4"].ToString().Replace("null", "");
                p5 = Request.QueryString["p5"].ToString().Replace("null", "");
                prtStyle = Request.QueryString["prtStyle"].ToString().Replace("null", "");
               

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
                    BE0007Repository repo = new BE0007Repository(DBWork);

                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    //需先設定ReportPath參數才有用
                    if (prtStyle == "2") 
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BE0007_2.rdlc";
                    }
                    else if (prtStyle == "3")//A3 橫式不分頁
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BE0007_3.rdlc";
                    }
                    else if (prtStyle == "4") //A3 直式不分頁
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BE0007_4.rdlc";
                    }
                    else if (prtStyle == "5") //A3 直式分頁
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BE0007_5.rdlc";
                    }
                    else if (prtStyle == "6") //A3 橫式分頁
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BE0007_6.rdlc";
                    }
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("StartDate", p2) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("EndDate", p3) });

                    //string _recYM = (int.Parse(parRECYM.Substring(0, parRECYM.Length < 5 ? 2 : 3)) + 1911).ToString() + "/" + parRECYM.Substring(parRECYM.Length < 5 ? 2 : 3, 2) + "/" + "01";
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BE0007", repo.Report(p0, p1, p2, p3, p4, p5)));
                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception e)
                {
                    throw;
                }
            }


        }
    }
}