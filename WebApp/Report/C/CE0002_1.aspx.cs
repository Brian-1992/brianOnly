using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.C;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;

namespace WebApp.Report.C
{
    public partial class CE0002_1 : Page
    {
        string chk_no;
        string print_order;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                chk_no = Request.QueryString["CHK_NO"].ToString().Replace("null", "");
                print_order = Request.QueryString["PRINT_ORDER"].ToString().Replace("null", "");
                
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
                    CE0002Repository repo = new CE0002Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    string wh_no = repo.GetWhno(chk_no);

                    IEnumerable<CE0002M> temp = repo.Print(chk_no, print_order, wh_no);
                    foreach (CE0002M item in temp)
                    {
                        item.CHK_TYPE_NAME = repo.GetChkWhkindName(item.CHK_WH_KIND, item.CHK_TYPE);
                        if (item.CHK_WH_NO == "PH1S")
                        {
                            if (item.MMNAME_C.Length > 7) {
                                item.MMNAME_C = item.MMNAME_C.Substring(0, 7);
                            }
                        }
                    }
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0002", temp));

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