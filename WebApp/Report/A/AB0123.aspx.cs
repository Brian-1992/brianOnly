using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AB0123 : SiteBase.BasePage
    {
        string parYYYYMM;
        string parYYYYMM_D;
        string parWHG;
        string parWH_NO;
        string parWHGAll;
        string parMMCODE_B;
        string parMMCODE_E;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parYYYYMM = Request.QueryString["YYYYMM"].ToString().Replace("null", "");
                parYYYYMM_D = Request.QueryString["YYYYMM_D"].ToString().Replace("null", "");
                parWHG = Request.QueryString["WHG"].ToString().Replace("null", "");
                parWH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                parWHGAll = Request.QueryString["WHGAll"].ToString().Replace("null", "");
                parMMCODE_B = Request.QueryString["MMCODE_B"].ToString().Replace("null", "");
                parMMCODE_E = Request.QueryString["MMCODE_E"].ToString().Replace("null", "");

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
                    AB0123Repository repo = new AB0123Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    string IdName= DBWork.UserInfo.UserId+ " "+DBWork.UserInfo.UserName;

                    DataTable dt = repo.GetSetTime(parYYYYMM);

                    if (dt.Rows.Count > 0)
                    {
                        string SET_BTIME = dt.Rows[0]["SET_BTIME"].ToString();
                        string SET_ETIME = dt.Rows[0]["SET_ETIME"].ToString();

                        if (SET_BTIME != "")
                        {
                            DateTime Btime = DateTime.Parse(SET_BTIME);
                            SET_BTIME = (Btime.Year - 1911).ToString() + Btime.Month.ToString("00") + Btime.Day.ToString("00") + " "+ Btime.ToShortTimeString()+":"+ Btime.Second;
                        }
                        if (SET_ETIME != "")
                        {
                            DateTime Etime = DateTime.Parse(SET_ETIME);
                            SET_ETIME = (Etime.Year - 1911).ToString() + Etime.Month.ToString("00") + Etime.Day.ToString("00") + " " + Etime.ToShortTimeString() + ":" + Etime.Second;
                        }

                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SET_BTIME", SET_BTIME) });
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SET_ETIME", SET_ETIME) });

                    }



                    dt = repo.GetReportSum(parYYYYMM, parYYYYMM_D, parWHG, parWHGAll, parWH_NO, parMMCODE_B, parMMCODE_E);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM1", dt.Rows[0][0].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM2", dt.Rows[0][1].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM3", dt.Rows[0][2].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM4", dt.Rows[0][3].ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("IdName", IdName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYYYMM", parYYYYMM) });

                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0123", repo.Report(parYYYYMM, parYYYYMM_D, parWHG, parWHGAll, parWH_NO, parMMCODE_B, parMMCODE_E)));
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