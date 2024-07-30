using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0106 : SiteBase.BasePage
    {
        string parMAT_CLASS;
        string parSET_YM;
        string parINIDFLAG;
        string parM_STOREID;
        bool parclsALL;
        string parGetMatName;
        string parGetInidFlagName;
        string parGetM_StoreIDName;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                parSET_YM = Request.QueryString["SET_YM"].ToString().Replace("null", "");
                parINIDFLAG = Request.QueryString["INIDFLAG"].ToString().Replace("null", "");
                parM_STOREID = Request.QueryString["M_STOREID"].ToString().Replace("null", "");
                parclsALL = bool.Parse(Request.QueryString["clsALL"].ToString().Replace("null", ""));
                parGetMatName = Request.QueryString["getMatName"].ToString().Replace("null", "");
                parGetInidFlagName = Request.QueryString["getInidFlagName"].ToString().Replace("null", "");
                parGetM_StoreIDName = Request.QueryString["getM_StoreIDName"].ToString().Replace("null", "");
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
                    AA0106Repository repo = new AA0106Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DeptName", repo.getDeptName()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parMAT_CLASS", parMAT_CLASS) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parSET_YM", parSET_YM) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parINIDFLAG", parINIDFLAG) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parM_STOREID", parM_STOREID) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parGetMatName", parGetMatName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parGetInidFlagName", parGetInidFlagName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parGetM_StoreIDName", parGetM_StoreIDName) });

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
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0106", repo.Print(parMAT_CLASS, parSET_YM, parINIDFLAG, parM_STOREID, parclsALL)));
                    }
                    else
                    {
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0106", repo.Print(parMAT_CLASS, parSET_YM, parINIDFLAG, parM_STOREID, parclsALL)));
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