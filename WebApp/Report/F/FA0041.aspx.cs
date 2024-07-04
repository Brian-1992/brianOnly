using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;
using System.Linq;
using System.Linq;

namespace WebApp.Report.F
{
    public partial class FA0041 : Page
    {
        string chk_ym, unit_class, unit_class_name, is_chk, chk_type, chk_status;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                chk_ym = Request.QueryString["CHK_YM"].ToString().Replace("null", "");
                unit_class = Request.QueryString["UNIT_CLASS"].ToString().Replace("null", "");
                unit_class_name = Request.QueryString["UNIT_CLASS_NAME"].ToString().Replace("null", "");
                is_chk = Request.QueryString["IS_CHK"].ToString().Replace("null", "");
                chk_type = Request.QueryString["CHK_TYPE"].ToString().Replace("null", "");
                chk_status = Request.QueryString["CHK_STATUS"].ToString().Replace("null", "");
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
                    FA0041Repository repo = new FA0041Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    IEnumerable<FA0041M> list = repo.Print(chk_ym, unit_class, is_chk, chk_type, chk_status);

                    foreach (FA0041M item in list) {
                        item.UNIT_CLASS_NAME = unit_class_name;
                        item.CHK_YM = chk_ym;
                        item.IS_CHK = is_chk;
                        item.UNIT_CLASS = unit_class;
                    }
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0041", list));

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