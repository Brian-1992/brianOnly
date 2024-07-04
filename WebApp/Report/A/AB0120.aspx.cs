using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AB0120 : SiteBase.BasePage
    {
        string parMAT_CLASS;
        string parSET_YM;
        string parM_STOREID;
        bool parclsALL;
        string parMAT_CLSNAME;
        string parM_STOREIDNAME;
        string parWH_NO;
        bool notZeroOnly;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                parSET_YM = Request.QueryString["SET_YM"].ToString().Replace("null", "");
                parM_STOREID = Request.QueryString["M_STOREID"].ToString().Replace("null", "");
                parclsALL = bool.Parse(Request.QueryString["clsALL"].ToString().Replace("null", ""));
                parMAT_CLSNAME = Request.QueryString["MAT_CLSNAME"].ToString().Replace("null", "");
                parM_STOREIDNAME = Request.QueryString["M_STOREIDNAME"].ToString().Replace("null", "");
                parWH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                notZeroOnly = (Request.QueryString["NotZeroOnly"].ToString().Replace("null", "") == "true");
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
                    AB0120Repository repo = new AB0120Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parDeptName", repo.getWhName(parWH_NO)) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parMAT_CLASS", parMAT_CLASS) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parSET_YM", parSET_YM) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parM_STOREID", parM_STOREID) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parMAT_CLSNAME", parMAT_CLSNAME) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parM_STOREIDNAME", parM_STOREIDNAME) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parWH_NAME", repo.getWH_NAME()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parWH_NO", parWH_NO) });

                    if (parclsALL == true)
                    {
                        string _parMAT_CLASS = parMAT_CLASS.Substring(0, parMAT_CLASS.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _parMAT_CLASS.Split(',');
                        parMAT_CLASS = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            parMAT_CLASS += "'" + tmp[i] + "',";
                        }
                        parMAT_CLASS = parMAT_CLASS.Substring(0, parMAT_CLASS.Length - 1);
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0120", repo.Print(parMAT_CLASS, parSET_YM, parM_STOREID, parclsALL, parWH_NO, notZeroOnly)));
                    }
                    else
                    {
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0120", repo.Print(parMAT_CLASS, parSET_YM, parM_STOREID, parclsALL, parWH_NO, notZeroOnly)));
                    }
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