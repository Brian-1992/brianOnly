using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.B;
using System.Collections.Generic;

namespace WebApp.Report.B
{
    public partial class BD0008 : SiteBase.BasePage
    {
        string parWH_NO;
        string parRECYM;
        string parCONTRACNO;
        string parORDERBY;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                parWH_NO = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                parRECYM = Request.QueryString["RECYM"].ToString().Replace("null", "");
                parCONTRACNO = Request.QueryString["CONTRACNO"].ToString().Replace("null", "");
                parORDERBY = Request.QueryString["ORDERBY"].ToString().Replace("null", "");

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
                    BD0008Repository repo = new BD0008Repository(DBWork);



                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WH_NO", parWH_NO) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("RECYM", parRECYM) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CONTRACNO", parCONTRACNO) });


                    ReportViewer1.EnableTelemetry = false;
                    string _recYM = (int.Parse(parRECYM.Substring(0, parRECYM.Length < 5 ? 2 : 3)) + 1911).ToString() + "/" + parRECYM.Substring(parRECYM.Length < 5 ? 2 : 3, 2) + "/" + "01";
                    double SUM1 = 0, SUM2 = 0;

                    IEnumerable<PH_EQPD> ph_eqpd = repo.GetSum(_recYM, parWH_NO, parCONTRACNO);
                    foreach (PH_EQPD data in ph_eqpd)
                    {
                        SUM1 = data.SUM1;
                        SUM2 = data.SUM2;
                    }
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM1", SUM1.ToString()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SUM2", SUM2.ToString()) });


                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BD0008", repo.Report(_recYM, parWH_NO, parCONTRACNO, parORDERBY)));
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