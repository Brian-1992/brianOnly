using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.BG;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.B
{
    public partial class BG0006 : SiteBase.BasePage
    {
        string parMAT_CLASS;
        string parACC_TIME_B;
        string parACC_TIME_E;
        string parM_CONTID;
        //string parCONTRACNO;
        string parMAT_CLASS_NAME;
        string parM_CONTID_NAME;
        string parMMCODE;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                parACC_TIME_B = Request.QueryString["ACC_TIME_B"].ToString().Replace("null", "");
                parACC_TIME_E = Request.QueryString["ACC_TIME_E"].ToString().Replace("null", "");
                parM_CONTID = Request.QueryString["M_CONTID"].ToString().Replace("null", "");
                //parCONTRACNO = Request.QueryString["CONTRACNO"].ToString().Replace("null", "");
                parMAT_CLASS_NAME = Request.QueryString["MAT_CLASS_NAME"].ToString().Replace("null", "").Substring(3);
                parM_CONTID_NAME = Request.QueryString["M_CONTID_NAME"].ToString().Replace("null", "").Substring(2);
                parMMCODE = Request.QueryString["MMCODE"].ToString().Replace("null", "");
                report1Bind();
            }
        }
        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0006Repository repo = new BG0006Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    string _parACC_TIME_B = "";
                    string _parACC_TIME_E = "";
                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("str_PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parMAT_CLASS", parMAT_CLASS) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parACC_TIME_B", parACC_TIME_B) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parACC_TIME_E", parACC_TIME_E) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parM_CONTID", parM_CONTID) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parCONTRACNO", parCONTRACNO) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parMAT_CLASS_NAME", parMAT_CLASS_NAME) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parM_CONTID_NAME", parM_CONTID_NAME) });

                    if (!string.IsNullOrWhiteSpace(parACC_TIME_B))
                    {
                        _parACC_TIME_B = (int.Parse(parACC_TIME_B.Substring(0, parACC_TIME_B.Length < 7 ? 2 : 3)) + 1911).ToString() + "-" + parACC_TIME_B.Substring(parACC_TIME_B.Length < 7 ? 2 : 3, 2) + "-" + parACC_TIME_B.Substring(parACC_TIME_B.Length < 7 ? 4 : 5, 2);
                    }
                    if (!string.IsNullOrWhiteSpace(parACC_TIME_E))
                    {
                        _parACC_TIME_E = (int.Parse(parACC_TIME_E.Substring(0, parACC_TIME_E.Length < 7 ? 2 : 3)) + 1911).ToString() + "-" + parACC_TIME_E.Substring(parACC_TIME_E.Length < 7 ? 2 : 3, 2) + "-" + parACC_TIME_E.Substring(parACC_TIME_E.Length < 7 ? 4 : 5, 2);
                    }

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BG0006", repo.Print_2(parMAT_CLASS, _parACC_TIME_B, parM_CONTID, _parACC_TIME_E, parMMCODE)));
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