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
    public partial class FA0001 : Page
    {
        string userID, APPTIME1, APPTIME2, task_id, appdept, showopt;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                userID = User.Identity.Name;
                APPTIME1 = Request.QueryString["APPTIME1"].ToString().Replace("null", "");
                APPTIME2 = Request.QueryString["APPTIME2"].ToString().Replace("null", "");
                task_id = Request.QueryString["task_id"].ToString().Replace("null", "");
                appdept = Request.QueryString["appdept"].ToString().Replace("null", "");
                showopt = Request.QueryString["showopt"].ToString().Replace("null", "");


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
                    FA0001Repository repo = new FA0001Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    string tmpTIME1 = "", tmpTIME2 = "";
                    int Intyear1 = 0, Intyear2 = 0;
                    if (APPTIME1 != "" && APPTIME1 != null)
                    {
                        tmpTIME1 = Convert.ToDateTime(APPTIME1.Substring(4, 11)).ToString("yyyyMM");
                        Intyear1 = Convert.ToInt32(tmpTIME1.Substring(0, 4)) - 1911;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APPTIME1", Convert.ToString(Intyear1) + tmpTIME1.Substring(4, 2)) });
                    }

                    if (APPTIME2 != "" && APPTIME2 != null)
                    {
                        tmpTIME2 = Convert.ToDateTime(APPTIME2.Substring(4, 11)).ToString("yyyyMM");
                        Intyear2 = Convert.ToInt32(tmpTIME2.Substring(0, 4)) - 1911;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APPTIME2", Convert.ToString(Intyear2) + tmpTIME2.Substring(4, 2)) });
                    }

                    //有選則有顯示,沒選則沒顯示類別
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MAT_CLSNAME", repo.GetMatClassName(task_id)) });
                    






                    if (showopt == "0") //非庫備品
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DATA_DESC", "非庫備品") });
                    }
                    else if (showopt == "1")  //庫備品
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DATA_DESC", "庫備品") });
                    }

                    //DataSet1 根據AA0061
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.GetPrintData(userID, APPTIME1, APPTIME2, task_id, appdept, showopt)));


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