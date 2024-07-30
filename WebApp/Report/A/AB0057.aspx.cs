using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AB0057 : SiteBase.BasePage
    {
        string parMAT_CLASS;
        string parEXP_DATE_B;
        string parEXP_DATE_E;
        string parM_STOREID;
        string parFRWH;
        bool parclsALL;
        string parGetWhName;
        string parGetMatName;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                parFRWH = Request.QueryString["FRWH"].ToString().Replace("null", "");
                parclsALL = bool.Parse(Request.QueryString["clsALL"].ToString().Replace("null", ""));
                parM_STOREID = Request.QueryString["M_STOREID"].ToString().Replace("null", "");
                parEXP_DATE_B = Request.QueryString["EXP_DATE_B"].ToString().Replace("null", "");
                parEXP_DATE_E = Request.QueryString["EXP_DATE_E"].ToString().Replace("null", "");
                parGetWhName = Request.QueryString["getWhName"].ToString().Replace("null", "");
                parGetMatName = Request.QueryString["getMatName"].ToString().Replace("null", "");
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
                    ReportViewer1.EnableTelemetry = false;
                    AB0057Repository repo = new AB0057Repository(DBWork);
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parGetWhName", parGetWhName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parGetMatName", parGetMatName) });

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
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0062", repo.Print(parMAT_CLASS, parEXP_DATE_B, parEXP_DATE_E, parM_STOREID, parFRWH, parclsALL)));
                    }
                    else
                    {
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0062", repo.Print(parMAT_CLASS, parEXP_DATE_B, parEXP_DATE_E, parM_STOREID, parFRWH, parclsALL)));
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