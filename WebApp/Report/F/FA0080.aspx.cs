using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;

namespace WebApp.Report.F
{
    public partial class FA0080 : SiteBase.BasePage
    {
        string P0;
        string P1;
        string P4;
        string P5;
        string P6;
        string P7;
        string MENULINK;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = Request.QueryString["p1"].ToString().Replace("null", "");
                P4 = Request.QueryString["p4"].ToString().Replace("null", "");
                P5 = Request.QueryString["p5"].ToString().Replace("null", "");
                P6 = Request.QueryString["p6"].ToString().Replace("null", "");
                //P7 = Request.QueryString["p7"].ToString().Replace("null", "");
                MENULINK = Request.QueryString["MENULINK"].ToString().Replace("null", "");
                report1Bind();
            }
        }

        protected void report1Bind()
        {
            var user = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0080Repository repo = new FA0080Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    string str_bf_tr_invqty = repo.BF_TR_INVQTY(P0, P4, P5) ?? "0";
                    string str_af_tr_invqty1 = repo.AF_TR_INVQTY1(P0, P4, P5) ?? "0";
                    DataTable dt = repo.GetLastRec(P0, P1, P4, P5);
                    string p_yyymm, p_whno, p_mmcode;
                    if (dt.Rows.Count > 0)
                    {
                        p_yyymm = dt.Rows[0][0].ToString();
                        p_whno = dt.Rows[0][1].ToString();
                        p_mmcode = dt.Rows[0][2].ToString();
                    }
                    else
                    {
                        p_yyymm = "";
                        p_whno = "";
                        p_mmcode = "";
                    }
                    string str_af_tr_invqty2 = repo.AF_TR_INVQTY2(p_yyymm, p_whno, p_mmcode) ?? "0";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Bfinvqty", str_bf_tr_invqty) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Afinvqty1", str_af_tr_invqty1) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Afinvqty2", str_af_tr_invqty2) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0080", repo.GetPrintData(P0, P1, P4, P5, P6, Convert.ToBoolean(P7) == true ? "Y" : "N", MENULINK, user)));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch(Exception e)
                {
                    throw;
                }
            }
        }
    }
}