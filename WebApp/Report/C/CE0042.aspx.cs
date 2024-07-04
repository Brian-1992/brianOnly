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
    public partial class CE0042 : Page
    {
        string chk_no;
        string chk_level;
        string wh_kind;
        string condition;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                chk_no = Request.QueryString["CHK_NO"].ToString().Replace("null", "");
                chk_level = Request.QueryString["CHK_LEVEL"].ToString().Replace("null", "");
                wh_kind = Request.QueryString["WH_KIND"].ToString().Replace("null", "");
                condition = Request.QueryString["CONDITION"].ToString().Replace("null", "");

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
                    CE0042Repository repo = new CE0042Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    IEnumerable<CE0042M> temp = repo.Print(chk_no, chk_level, wh_kind, condition);
                    foreach (CE0042M item in temp)
                    {
                        item.CHK_TYPE_NAME = repo.GetChkWhkindName(item.CHK_WH_KIND, item.CHK_TYPE);
                    }
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0042", temp));

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