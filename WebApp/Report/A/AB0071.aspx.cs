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
    public partial class AB0071 : SiteBase.BasePage
    {
        string parSTOCKCODE = "";
        string parWH_NO ="";
        string parYYYYMMDD_B ="";
        string parYYYYMMDD_E ="";
        string parP4 ="";
        string parMMCODE ="";
        string parE_RestrictCode="";
        bool parOrderByRXNO;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                parSTOCKCODE = Request.QueryString["STOCKCODE"].ToString().Replace("null", "");
                parYYYYMMDD_B = Request.QueryString["YYYYMMDD_B"].ToString().Replace("null", "");
                parYYYYMMDD_E = Request.QueryString["YYYYMMDD_E"].ToString().Replace("null", "");
                parP4 = Request.QueryString["P4"].ToString().Replace("null", "");
                parMMCODE = Request.QueryString["MMCODE"].ToString().Replace("null", "");
                parE_RestrictCode = Request.QueryString["E_RestrictCode"].ToString().Replace("null", "");
                parWH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                parOrderByRXNO = bool.Parse(Request.QueryString["OrderByRXNO"].ToString().Replace("null", ""));
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
                    AB0071Repository repo = new AB0071Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    string IdName= DBWork.UserInfo.UserId+ " "+DBWork.UserInfo.UserName;



                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("IdName", IdName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WH_NO", parWH_NO) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("E_RestrictCode", parE_RestrictCode) });
                    ReportViewer1.EnableTelemetry = false;
                    //ReportViewer1.LocalReport.DisplayName = "藥庫一～四級管制藥品出入帳明細總表（月報表）";
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0071", repo.Report(parSTOCKCODE, parYYYYMMDD_B, parYYYYMMDD_E, parP4, parMMCODE, parOrderByRXNO)));
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