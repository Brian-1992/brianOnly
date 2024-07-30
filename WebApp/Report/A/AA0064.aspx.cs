using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0064 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                report1Bind();
                ReportViewer1.EnableTelemetry = false; // 加快報表載入速度
                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
            }
        } // 

        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0064Repository repo = new AA0064Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    //userID = User.Identity.Name;
                    //APPTIME1 = Request.QueryString["APPTIME1"].ToString().Replace("null", "");
                    //APPTIME2 = Request.QueryString["APPTIME2"].ToString().Replace("null", "");
                    //task_id = Request.QueryString["task_id"].ToString().Replace("null", "");
                    //mmcode = Request.QueryString["mmcode"].ToString().Replace("null", "");
                    //showopt = Request.QueryString["showopt"].ToString().Replace("null", "");
                    //showdata = Request.QueryString["showdata"].ToString().Replace("null", "");
                    //type = Request.QueryString["type"].ToString().Replace("null", ""); //分辨是AA0064 or AB0056

                    AA0064_MODEL v = new AA0064_MODEL();
                    v.APPTIME_START = Request.QueryString["APPTIME_START"].ToString().Replace("null", "");
                    v.APPTIME_END = Request.QueryString["APPTIME_END"].ToString().Replace("null", "");
                    v.MAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                    v.FRWH = Request.QueryString["FRWH"].ToString().Replace("null", "");
                    v.USERID = User.Identity.Name;

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DeptName", new AA0063Repository(DBWork).getDeptName()) });
                    //string tmpTIME1 = "", tmpTIME2 = "";
                    //int Intyear1 = 0, Intyear2 = 0;
                    //if (APPTIME1 != "" && APPTIME1 != null)
                    //{
                    //    tmpTIME1 = Convert.ToDateTime(APPTIME1.Substring(4, 11)).ToString("yyyyMM");
                    //    Intyear1 = Convert.ToInt32(tmpTIME1.Substring(0, 4)) - 1911;
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APPTIME1", Convert.ToString(Intyear1) + tmpTIME1.Substring(4, 2)) });
                    //}

                    //if (APPTIME2 != "" && APPTIME2 != null)
                    //{
                    //    tmpTIME2 = Convert.ToDateTime(APPTIME2.Substring(4, 11)).ToString("yyyyMM");
                    //    Intyear2 = Convert.ToInt32(tmpTIME2.Substring(0, 4)) - 1911;
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APPTIME2", Convert.ToString(Intyear2) + tmpTIME2.Substring(4, 2)) });
                    //}






                    //if (showopt == "0") //非庫備品
                    //{
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DATA_DESC", "非庫備品") });
                    //}
                    //else if (showopt == "1")  //庫備品
                    //{
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DATA_DESC", "庫備品") });
                    //}


                    ReportViewer1.LocalReport.DataSources.Clear();
                    //if (type == "AA0064")
                    //{
                    //    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.GetPrintData(userID, APPTIME1, APPTIME2, task_id, mmcode, showopt, showdata)));
                    //}
                    //else if (type == "AB0056")
                    //{
                    //    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.GetPrintDataByDept(userID, APPTIME1, APPTIME2, task_id, mmcode, showopt, showdata)));
                    //}
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0064DataSet", repo.GetPrintData(v)));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }


        } // 

    } // ec 
} // en