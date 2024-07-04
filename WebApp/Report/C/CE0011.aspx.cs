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
    public partial class CE0011 : Page
    {
        string chk_no;
        string chk_level;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                chk_no = Request.QueryString["CHK_NO"].ToString().Replace("null", "");
                chk_level = Request.QueryString["CHK_LEVEL"].ToString().Replace("null", "");
                
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
                    CE0011Repository repo = new CE0011Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    IEnumerable<CE0011M> temp = repo.Print(chk_no, chk_level);
                    foreach (CE0011M item in temp) {
                        item.CHK_TYPE_NAME = repo.GetChkWhkindName(item.CHK_WH_KIND, item.CHK_TYPE);
                    }
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0011", temp));

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