using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;
using System.Collections;
using System.Collections.Generic;

namespace WebApp.Report.A
{
    public partial class AA0088 : Page
    {
        string P0;
        string P1;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                P0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
                //parDN = Request.QueryString["DN"].ToString().Replace("null", "");
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
                    AA0088Repository repo = new AA0088Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    //foreach (PH_SMALL_M smdata in repo.GetSmData(parDN))
                    //{
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DAY", DateTime.Now.ToString) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("USEWHEN", smdata.USEWHEN) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("INID", smdata.INID) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("INIDNAME", smdata.INIDNAME) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TEL", smdata.TEL) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DUEDATE", smdata.DUEDATE) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DELIVERY", smdata.DELIVERY) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ACCEPT", smdata.ACCEPT) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PAYWAY", smdata.PAYWAY) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("OTHERS", smdata.OTHERS) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DEPT", smdata.DEPT) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DOMAN", smdata.DO_USER) });
                    //    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DOTEL", smdata.DOTEL) });
                    //}

                    //int total_price = repo.GetReportTotalPrice(parDN);
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOTALPRICE", total_price.ToString()) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CHTTOTALPRICE", JCLib.ChtNumber.Transform(total_price)) });
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("USERNAME", repo.GetUna(User.Identity.Name)) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UID", User.Identity.Name) });


                    ReportViewer1.LocalReport.DataSources.Clear();
                    string hospCode = repo.GetHospCode();
                    // ReportViewer1.LocalReport.DisplayName = parDN;
                    IEnumerable<ME_DOCM> temp = repo.GetReport(P0, P1, hospCode=="0");
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0088", temp));

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