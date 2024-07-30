using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Repository.B;

namespace WebApp.Report.B
{
    public partial class BD0014 : SiteBase.BasePage
    {
        string PO_NO = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PO_NO = Request.QueryString["P0"].ToString().Replace("null", "");
                if (PO_NO.Length > 0)
                {
                    PO_NO = PO_NO.Substring(0, PO_NO.Length - 1);

                    if (!PO_NO.Contains(","))
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BD0014.rdlc";
                        report1Bind();
                    }
                    else
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BD0014_2.rdlc";
                        report1Bind2();

                    }
                }
            }
        }

        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0021Repository repo = new BD0021Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);


                    var MM_PO_D = repo.GetReport(PO_NO);

                    string isMemoTooLong = "0";
                    foreach (MM_PO_D item in MM_PO_D)
                    {
                        if (item.MEMO !=null && item.MEMO.Length > 100)
                        {
                            isMemoTooLong = "1";
                            break;
                        }
                    }

                    if (isMemoTooLong == "1")
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BD0014_3.rdlc";
                    }


                    ReportViewer1.EnableTelemetry = false;

                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("isMemoTooLong", isMemoTooLong) });


                    string hospName = repo_rdlc.GetHospName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    DataTable dt = repo.GetReportMain(PO_NO);
                    string[] po_time = dt.Rows[0]["PO_TIME"].ToString().Trim().Split('/');
                    string po_time_str = (Convert.ToInt32(po_time[0]) - 1911) + "年" + po_time[1] + "月" + po_time[2] + "日";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PO_NO", PO_NO) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PO_TIME", po_time_str) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN", dt.Rows[0]["AGEN_NO"].ToString().Trim() + " " + dt.Rows[0]["AGEN_NAMEC"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_TEL", dt.Rows[0]["AGEN_TEL"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_FAX", dt.Rows[0]["AGEN_FAX"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MEMO", dt.Rows[0]["MEMO"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SMEMO", dt.Rows[0]["SMEMO"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("REC_ADDR", dt.Rows[0]["REC_ADDR"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CONTACT", dt.Rows[0]["CONTACT"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AMOUNT", dt.Rows[0]["AMOUNT"].ToString().Trim()) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BD0014", repo.GetReport(PO_NO)));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch(Exception e)
                {
                    throw;
                }

            }
        }

        protected void report1Bind2()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0021Repository repo = new BD0021Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);


                    DataTable dt = repo.GetReport_BD0014_2(PO_NO);

                    string isMemoTooLong = "0";
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr["D_MEMO"] != null && dr["D_MEMO"].ToString().Length > 100)
                        {
                            isMemoTooLong = "1";
                            break;
                        }
                    }

                    if (isMemoTooLong == "1")
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BD0014_4.rdlc";
                    }


                    dt.Columns.Add("po_time_str");
                    dt.Columns.Add("AGEN");
                    foreach (DataRow row in dt.Rows)
                    {
                        string[] po_time = dt.Rows[0]["PO_TIME"].ToString().Trim().Split('/');
                        string po_time_str = (Convert.ToInt32(po_time[0]) - 1911) + "年" + po_time[1] + "月" + po_time[2] + "日";
                        string AGEN_str = dt.Rows[0]["AGEN_NO"].ToString().Trim() + " " + dt.Rows[0]["AGEN_NAMEC"].ToString().Trim();
                        row["po_time_str"] = po_time_str;
                        row["AGEN"] = AGEN_str;

                    }



                    ReportViewer1.EnableTelemetry = false;

                    string hospName = repo_rdlc.GetHospName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });




                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PO_NO", PO_NO) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PO_TIME", po_time_str) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN", dt.Rows[0]["AGEN_NO"].ToString().Trim() + " " + dt.Rows[0]["AGEN_NAMEC"].ToString().Trim()) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_TEL", dt.Rows[0]["AGEN_TEL"].ToString().Trim()) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_FAX", dt.Rows[0]["AGEN_FAX"].ToString().Trim()) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("MEMO", dt.Rows[0]["MEMO"].ToString().Trim()) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SMEMO", dt.Rows[0]["SMEMO"].ToString().Trim()) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("REC_ADDR", dt.Rows[0]["REC_ADDR"].ToString().Trim()) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CONTACT", dt.Rows[0]["CONTACT"].ToString().Trim()) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AMOUNT", dt.Rows[0]["AMOUNT"].ToString().Trim()) });

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BD0014", dt));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception e)
                {
                    throw;
                }

            }
        }

    }
}