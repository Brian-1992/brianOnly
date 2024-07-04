using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.C;
namespace WebApp.Report.C
{
    public partial class CE0026 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string p0;
            string p1;

            if (!IsPostBack)
            {

                p0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
                p1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
              
                using (WorkSession session = new WorkSession(this))
                {
                    UnitOfWork DBWork = session.UnitOfWork;
                    try
                    {
                        CE0026Repository repo = new CE0026Repository(DBWork);
                        string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                        string str_UserDept = DBWork.UserInfo.InidName;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                       
                        string str_ReportTittle = "藥局盤點差異分析報表";
                        string chk_ym = p0.Substring(0, 3) + "年" + p0.Substring(3, 2) + "月";
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ReportTittle", chk_ym + " " + str_ReportTittle) });

                        //string str_UserDept = DBWork.UserInfo.InidName;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("InidName", str_UserDept) });



                        ReportViewer1.LocalReport.DataSources.Clear();
                        // ReportViewer1.LocalReport.DisplayName = parDN;
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0026", repo.GetReport(p0, p1)));

                        ReportViewer1.LocalReport.Refresh();
                    }
                    catch
                    {
                        throw;
                    }
                    //return session.Result;
                }
                //parDN = Request.QueryString["DN"].ToString().Replace("null", "");
                //report1Bind();
            }
        }
    }
}