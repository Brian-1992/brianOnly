using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.C;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;

namespace WebApp.Report.C
{
    public partial class CE0027 : SiteBase.BasePage
    {
        string p0;
        string p1;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //盤點年月
                p0 = Request.QueryString["p0"].ToString().Replace("null", "");
                //顯示內容
                p1 = Request.QueryString["p1"].ToString().Replace("null", "");

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
                    CE0027Repository repo = new CE0027Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    string str_UserName = DBWork.UserInfo.UserName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintUser", str_UserName) });

                    string str_UserDept = DBWork.UserInfo.InidName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserDept", str_UserDept) });

                    string ContentType = p1 == "0" ? "盤盈虧報表-總表": (p1=="1" ? "盤盈虧報表-盤盈" : "盤盈虧報表-盤虧");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ContentType", ContentType) });

                    string chk_ym = p0;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CHK_YM", chk_ym.Substring(0, 3) + "年" + chk_ym.Substring(3, 2) + "月") });

                    string chk_period = repo.GetChkPeriod(chk_ym);
                    if (chk_period == "S 季")
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CHK_PERIOD", "季") });
                    }
                    else {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CHK_PERIOD", "月") });
                    }

                    ReportViewer1.PageCountMode = PageCountMode.Actual;
                    ReportViewer1.LocalReport.DisplayName = string.Format("{0}_藥局{1}", p0, ContentType);

                    //先清空報表的DataSet，再將讀到的DataTable放到DataSources
                    ReportViewer1.LocalReport.DataSources.Clear();

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0027", repo.PrintData(p0, p1)));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0027Count", repo.GetDetails(p0)));

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