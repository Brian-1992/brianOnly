using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.B;

namespace WebApp.Report.B
{
    public partial class BG0008 : SiteBase.BasePage
    {
        string parPo_time_b =   "";
        string parPo_time_e =   "";
        string parMat_class =   "";
        string parAgen_no =     "";
        string parM_storeid =   "";
        string parM_contid = "";
        string parMat_classRawValue = "";


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parPo_time_b = Request.QueryString["po_time_b"].ToString().Replace("null", "");
                parPo_time_e = Request.QueryString["po_time_e"].ToString().Replace("null", "");
                parMat_class = Request.QueryString["mat_class"].ToString().Replace("null", "");
                parAgen_no = Request.QueryString["agen_no"].ToString().Replace("null", "");
                parM_storeid = Request.QueryString["m_storeid"].ToString().Replace("null", "");
                parM_contid = Request.QueryString["m_contid"].ToString().Replace("null", "");
                parMat_classRawValue = Request.QueryString["Mat_classRawValue"].ToString().Replace("null", "");
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
                    BG0008Repository repo = new BG0008Repository(DBWork);



                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PO_TIME_B", parPo_time_b) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MAT_CLASS", parMat_classRawValue) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("M_STOREID", parM_storeid) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("M_CONTID", parM_contid) });



                    //string _recYM = (int.Parse(parRECYM.Substring(0, parRECYM.Length < 5 ? 2 : 3)) + 1911).ToString() + "/" + parRECYM.Substring(parRECYM.Length < 5 ? 2 : 3, 2) + "/" + "01";
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BG0008", repo.Report(parPo_time_b, parPo_time_e, parMat_class, parAgen_no, parM_storeid, parM_contid)));
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