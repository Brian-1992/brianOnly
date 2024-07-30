using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.F
{
    public partial class FA0010 : Page
    {
        string year;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                year = Request.QueryString["year"].ToString().Replace("null", "");


                report1Bind();
                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
            }
        }
        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0010Repository repo = new FA0010Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    //DataSet1 根據FA0010
                    ReportViewer1.LocalReport.DataSources.Clear();
                    //mysysdate = "20190717120700";
                    this.Server.ScriptTimeout = 1800;  //timeout時間拉長
                    session.Result.afrs = repo.Delete(); //先刪除TMP_INVCTL_YR_RP table
                    var mysysdate = repo.GetSYSDATE(); //查詢目前系統時間
                    session.Result.afrs = repo.Create(mysysdate, year);         //新增到TMP_INVCTL_YR_RP table
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.GetPrintData(mysysdate)));


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