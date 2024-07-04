using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;
using System.Globalization;

namespace WebApp.Report.F
{
    public partial class FA0071 : SiteBase.BasePage
    {
        string P0;
        string YYY;
        string MM;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P0 = Request.QueryString["p0"].ToString().Replace("null", "");
                YYY = P0.Substring(0, 3);
                MM = P0.Substring(3, 2);
                if ( MM.Substring(0, 1) == "0")
                {
                    MM = P0.Substring(4, 1);
                }
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
                    FA0071Repository repo = new FA0071Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YYY", YYY) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MM", MM) });
                    string hospName = repo.GetHospName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    string Remark1 = int.Parse(repo.GetExpPrice(P0), NumberStyles.AllowThousands).ToString();
                    string Remark2 = "";
                    if (Remark1 != "0")
                    {
                        IEnumerable<FA0071ReportMODEL2> myEnum = repo.GetPrintData2(P0);
                        myEnum.GetEnumerator();
                        foreach (var item in myEnum)
                        {
                            if (Remark2 != "")
                            {
                                Remark2 += ";";
                            }
                            else
                            {
                                Remark2 += "，";
                            }
                            Remark2 += Int64.Parse(item.F3, NumberStyles.AllowThousands).ToString() + "(" + item.F2 + ")已開立折讓";
                        }
                    }
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Remark", Remark1 + "元" + Remark2) });
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DisplayName = YYY;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0071", repo.GetPrintData(P0)));
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA00712", repo.GetPrintData2(P0)));

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