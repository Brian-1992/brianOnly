using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.A
{
    public partial class AA0104 : SiteBase.BasePage
    {
        string parMAT_CLASS;
        string parSET_YM;
        string parINIDFLAG;
        string parM_STOREID;
        string parWH_NO;
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
                parWH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
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
                    AA0104Repository repo = new AA0104Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    string SubTitle = "";

                    if (parINIDFLAG != "4")
                    {
                        parWH_NO = null;
                        SubTitle = "衛星庫房(" + parGetInidFlagName + ")" + parSET_YM + parGetMatName + "成本明細報表";
                    }
                    else
                    {
                        SubTitle = "衛星庫房(" + parGetInidFlagName + "：" + parWH_NO + ")" + parSET_YM + parGetMatName + "成本明細報表";
                    }
              

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DeptName", repo.getDeptName()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parMAT_CLASS", parMAT_CLASS) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parSET_YM", parSET_YM) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parINIDFLAG", parINIDFLAG) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parM_STOREID", parM_STOREID) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parGetMatName", parGetMatName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parGetInidFlagName", parGetInidFlagName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parGetM_StoreIDName", parGetM_StoreIDName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("parWH_NO", parWH_NO) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SubTitle", SubTitle) });

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0104", repo.Print(parMAT_CLASS, parSET_YM, parINIDFLAG, parM_STOREID, parWH_NO)));
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