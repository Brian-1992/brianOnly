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
    public partial class FA0007 : SiteBase.BasePage
    {
        string parMAT_CLASS;
        string parDATA_YM;
        string parM_STOREID;
        string parMIL;
        bool parclsALL;
        string parGetM_StoreIDName;
        string parGetMatName;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parMAT_CLASS = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                parDATA_YM = Request.QueryString["DATA_YM"].ToString().Replace("null", "");               
                parM_STOREID = Request.QueryString["M_STOREID"].ToString().Replace("null", "");
                parMIL = Request.QueryString["MIL"].ToString().Replace("null", "");
                parclsALL = bool.Parse(Request.QueryString["clsALL"].ToString().Replace("null", ""));
                parGetM_StoreIDName = Request.QueryString["getM_StoreIDName"].ToString().Replace("null", "");
                parGetMatName = Request.QueryString["GetMatName"].ToString().Replace("null", "");
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
                    FA0007Repository repo = new FA0007Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DeptName", repo.getDeptName()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parMAT_CLASS", parMAT_CLASS) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parDATA_YM", parDATA_YM) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parM_STOREID", parM_STOREID) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parGetMatName", parGetMatName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parGetM_StoreIDName", parGetM_StoreIDName) });

                    string SubTitle = repo.getDeptName() + parDATA_YM + "(" + parGetMatName + "類" + parGetM_StoreIDName + ")" + "庫存成本報表" ;

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SubTitle", SubTitle) });

                    if (parclsALL == true)
                    {
                        string _parMAT_CLASS = parMAT_CLASS.Substring(0, parMAT_CLASS.Length - 1); //去除前端傳進來最後一個逗號
                        string[] tmp = _parMAT_CLASS.Split(',');
                        parMAT_CLASS = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            parMAT_CLASS += "'" + tmp[i] + "',";
                        }
                        parMAT_CLASS = parMAT_CLASS.Substring(0, parMAT_CLASS.Length - 1);
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0007", repo.Print(parMAT_CLASS, parDATA_YM, parM_STOREID, parMIL, parclsALL)));
                    }
                    else
                    {
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0007", repo.Print(parMAT_CLASS, parDATA_YM, parM_STOREID, parMIL, parclsALL)));
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