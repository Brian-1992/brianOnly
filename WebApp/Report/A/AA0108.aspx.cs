using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AA0108 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string p0;
            string p1;
            string p2;
            string p3;
            string p4;
            string p5;
            string p4n;
            string mclsname;

            if (!IsPostBack)
            {

                p0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
                p1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
                p2 = (Request.QueryString["p2"] is null) ? "" : Request.QueryString["p2"].ToString().Replace("null", "");
                p3 = (Request.QueryString["p3"] is null) ? "" : Request.QueryString["p3"].ToString().Replace("null", "");
                p4 = (Request.QueryString["p4"] is null) ? "" : Request.QueryString["p4"].ToString().Replace("null", "");
                p5 = (Request.QueryString["p5"] is null) ? "" : Request.QueryString["p5"].ToString().Replace("null", "");

                using (WorkSession session = new WorkSession(this))
                {
                    UnitOfWork DBWork = session.UnitOfWork;
                    try
                    {
                        AA0108Repository repo = new AA0108Repository(DBWork);
                        AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                        string hospName = repo_rdlc.GetHospName();
                        string hospFullName = repo_rdlc.GetHospFullName();
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                        string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                        string str_UserDept = DBWork.UserInfo.InidName;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                        if (p4 == "0")
                        {
                            p4n = "非庫備品";
                        }
                        if (p4 == "1")
                        {
                            p4n = "庫備品";
                        }
                        if (p4 == "2")
                        {
                            p4n = "所有品項";
                        }
                        if (p0 != "")
                        {
                            mclsname = repo.MCLSNAME(p0);
                        }
                        else
                        {
                            mclsname = "";
                        }
                        string str_ReportTittle = "三軍總醫院" + str_UserDept + mclsname +  "盤點現況表";
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ReportTittle", str_ReportTittle) });

                        //string str_UserDept = DBWork.UserInfo.InidName;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("InidName", str_UserDept) });



                        ReportViewer1.LocalReport.DataSources.Clear();
                        // ReportViewer1.LocalReport.DisplayName = parDN;
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0108", repo.GetReport(p0, p1, p2, p3, p4)));

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