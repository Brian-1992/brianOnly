using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Repository.AA;

namespace WebApp.Report.F
{
    public partial class FA0037_2 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string P0;
            string P1;
            string P2;
            string P3;
            string P4;
            string P5;
            string P6;
            string mclsname;
            string title;

            if (!IsPostBack)
            {

                P0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = (Request.QueryString["p2"] is null) ? "" : Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = (Request.QueryString["p3"] is null) ? "" : Request.QueryString["p3"].ToString().Replace("null", "");
                P4 = (Request.QueryString["p4"] is null) ? "" : Request.QueryString["p4"].ToString().Replace("null", "");
                P5 = (Request.QueryString["p5"] is null) ? "" : Request.QueryString["p5"].ToString().Replace("null", "");
                P6 = (Request.QueryString["p6"] is null) ? "" : Request.QueryString["p6"].ToString().Replace("null", "");

                using (WorkSession session = new WorkSession(this))
                {
                    UnitOfWork DBWork = session.UnitOfWork;
                    try
                    {
                        FA0037Repository repo = new FA0037Repository(DBWork);
                        AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                        string hospName = repo_rdlc.GetHospName();
                        string hospFullName = repo_rdlc.GetHospFullName();
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                        ReportViewer1.EnableTelemetry = false;
                        string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                        mclsname = repo.MCLSNAME(P2);
                        string P3_Y = P3.Substring(0, 3);
                        string P3_M = P3.Substring(3, 2);
                        string P5_C = (P5 == "0") ? "合約" : "非合約";
                        string P4_C = (P3 == "0") ? "非庫備" : "庫備";
                        title = mclsname + "申購單";
                        if (P4 == "0")
                        {
                            if (P5 == "0")
                            {
                                title += "(非庫備合約)品項";
                            }
                            if (P5 == "2")
                            {
                                title += "(非庫備非合約)品項";
                            }
                            if (P5 == "")
                            {
                                title += "(非庫備)品項";
                            }
                        }
                        if (P4 == "1")
                        {
                            if (P5 == "0")
                            {
                                title += "(庫備合約)品項";
                            }
                            if (P5 == "2")
                            {
                                title += "(庫備非合約)品項";
                            }
                            if (P5 == "")
                            {
                                title += "(庫備)品項";
                            }
                        }
                        if (P4 == "")
                        {
                            if (P5 == "0")
                            {
                                title += "(合約)品項";
                            }
                            if (P5 == "2")
                            {
                                title += "(非合約)品項";
                            }
                            if (P5 == "")
                            {
                                title += "(全部)品項";
                            }
                        }
                        string str_ReportTittle =  P3_Y + " 年" + P3_M + " 月" +  title;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ReportTittle", str_ReportTittle) });

                        string str_UserDept = DBWork.UserInfo.InidName;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("InidName", str_UserDept) });



                        ReportViewer1.LocalReport.DataSources.Clear();
                        // ReportViewer1.LocalReport.DisplayName = parDN;
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0037", repo.GetReport2(P2, P0, P1, P4, P5, P6)));

                        ReportViewer1.LocalReport.Refresh();
                    }
                    catch(Exception ex)
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