using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0063 : SiteBase.BasePage
    {
        string parMAT_CLASS;
        string parMMCODE;
        string parFRWH;
        string parFLOWID;
        string parPR_TIME_B;
        string parPR_TIME_E;
        string parDIS_TIME_B;
        string parDIS_TIME_E;
        bool parclsALL;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                parMMCODE = Request.QueryString["MMCODE"].ToString().Replace("null", "");
                parFRWH = Request.QueryString["FRWH"].ToString().Replace("null", "");
                parclsALL = bool.Parse(Request.QueryString["clsALL"].ToString().Replace("null", ""));
                parFLOWID = Request.QueryString["FLOWID"].ToString().Replace("null", "");
                parPR_TIME_B = Request.QueryString["PR_TIME_B"].ToString().Replace("null", "");
                parPR_TIME_E = Request.QueryString["PR_TIME_E"].ToString().Replace("null", "");
                parDIS_TIME_B = Request.QueryString["DIS_TIME_B"].ToString().Replace("null", "");
                parDIS_TIME_E = Request.QueryString["DIS_TIME_E"].ToString().Replace("null", "");
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
                    AA0063Repository repo = new AA0063Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });
                    //string _parPR_TIME_B = "";
                    //string _parPR_TIME_E = "";
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    //ReportViewer1.LocalReport.DisplayName = parDN;
                    //if (!string.IsNullOrWhiteSpace(parPR_TIME_B))
                    //{
                    //    _parPR_TIME_B = (int.Parse(parPR_TIME_B.Substring(0, parPR_TIME_B.Length < 7 ? 2 : 3)) + 1911).ToString() + "/" + parPR_TIME_B.Substring(parPR_TIME_B.Length < 7 ? 2 : 3, 2) + "/" + parPR_TIME_B.Substring(parPR_TIME_B.Length < 7 ? 4 : 5, 2);
                    //}
                    //if (!string.IsNullOrWhiteSpace(parPR_TIME_E))
                    //{
                    //    _parPR_TIME_E = (int.Parse(parPR_TIME_E.Substring(0, parPR_TIME_E.Length < 7 ? 2 : 3)) + 1911).ToString() + "/" + parPR_TIME_E.Substring(parPR_TIME_E.Length < 7 ? 2 : 3, 2) + "/" + parPR_TIME_E.Substring(parPR_TIME_E.Length < 7 ? 4 : 5, 2);
                    //}
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DeptName", repo.getDeptName()) });
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
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0063", repo.Print(parMAT_CLASS, parMMCODE, parclsALL, parFLOWID, parPR_TIME_B, parPR_TIME_E, parDIS_TIME_B, parDIS_TIME_E)));
                    }
                    else
                    {
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0063", repo.Print(parMAT_CLASS, parMMCODE, parclsALL, parFLOWID, parPR_TIME_B, parPR_TIME_E, parDIS_TIME_B, parDIS_TIME_E)));
                    }
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