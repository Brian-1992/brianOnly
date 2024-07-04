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
    public partial class CE0016_2 : Page
    {
        string chk_nos;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                chk_nos = Request.QueryString["chk_nos"].ToString().Replace("null", "");

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
                    CE0016Repository repo = new CE0016Repository(DBWork);
                    CE0002Repository repoCE0002 = new CE0002Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;

                    IEnumerable<CHK_DETAIL> items = repo.GetPrintData(chk_nos, string.Empty);
                    foreach (CHK_DETAIL item in items)
                    {
                        item.CHK_TYPE_NAME = repoCE0002.GetChkWhkindName(item.CHK_WH_KIND, item.CHK_TYPE);
                        item.TYPE_ABBRV = GetAbbreviation(item);
                    }
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0002", items));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }


        }

        public string GetAbbreviation(CHK_DETAIL item)
        {
            if (item.CHK_WH_KIND == "0")
            {
                switch (item.CHK_TYPE)
                {
                    case "1":
                        return "口";
                    case "2":
                        return "非口";
                    case "3":
                        return "1~3管";
                    case "4":
                        return "4管";
                    case "5":
                        return "公";
                    case "6":
                        return "專";
                    case "7":
                        return "藥";
                    case "8":
                        return "瓶";
                }
            }
            else if (item.CHK_WH_KIND == "1")
            {
                switch (item.CHK_TYPE)
                {
                    case "0":
                        return "非";
                    case "1":
                        return "庫";
                    case "3":
                        return "小";

                }
            }
            return string.Empty;
        }
    }
}