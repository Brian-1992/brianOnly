using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Report.A
{
    public partial class AA0102 : Page
    {
        string  type, APPTIME1, showdata;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                type = Request.QueryString["type"].ToString().Replace("null", "");
                APPTIME1 = Request.QueryString["APPTIME1"].ToString().Replace("null", "");
                showdata = Request.QueryString["showdata"].ToString().Replace("null", "");

                if (type.Equals("AA0102"))
                {
                    report1Bind();
                }else if (type.Equals("AA0103"))
                {
                    report2Bind();
                }
                else if (type.Equals("FA0043"))
                {
                    report3Bind();
                }

                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
            }
        }
        protected void report1Bind()
        {
            var Trshowdata = "";
            if (showdata == "1")
            {
                Trshowdata = "A";
            }
            else if (showdata == "2")
            {
                Trshowdata = "B";
            }
            else if (showdata == "3")
            {
                Trshowdata = "C";
            }
            else if (showdata == "4")
            {
                Trshowdata = "D";
            }

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0102Repository repo = new AA0102Repository(DBWork);

                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();

                    IEnumerable<AA0102_MODEL> list = repo.GetPrintData(APPTIME1, Trshowdata);
                    foreach (AA0102_MODEL item in list)
                    {
                        item.REPORT_TYPE = "AA0102";
                        item.CHK_YM = APPTIME1;
                        if (item.CHK_LEVEL == "0")
                        {
                            continue;
                        }
                        item.CHK_TYPE_NAME = repo.GetChkWhkindName(item.WH_KIND, item.CHK_TYPE);
                    }
                    list = list.OrderBy(x => x.INID).ThenBy(x => x.WH_NO).ThenBy(x => x.CHK_TYPE).ToList<AA0102_MODEL>();

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", list));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }


        }
        protected void report2Bind()
        {
            var Trshowdata = "";
            if (showdata == "1")
            {
                Trshowdata = "A";
            }
            else if (showdata == "2")
            {
                Trshowdata = "B";
            }
            else if (showdata == "3")
            {
                Trshowdata = "C";
            }
            else if (showdata == "4")
            {
                Trshowdata = "D";
            }

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0102Repository repo = new AA0102Repository(DBWork);

                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();

                    IEnumerable<AA0102_MODEL> list = repo.AA0103_GetPrintData(APPTIME1, Trshowdata);
                    foreach (AA0102_MODEL item in list)
                    {
                        item.REPORT_TYPE = "AA0103";
                        item.CHK_YM = APPTIME1;
                        if (item.CHK_LEVEL == "0")
                        {
                            continue;
                        }
                        item.CHK_TYPE_NAME = repo.GetChkWhkindName(item.WH_KIND, item.CHK_TYPE);
                    }

                    list = list.OrderBy(x => x.INID).ThenBy(x => x.WH_NO).ThenBy(x => x.CHK_TYPE).ToList<AA0102_MODEL>();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", list));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }


        }
        protected void report3Bind()
        {
            var Trshowdata = "";
            if (showdata == "1")
            {
                Trshowdata = "A";
            }
            else if (showdata == "2")
            {
                Trshowdata = "B";
            }
            else if (showdata == "3")
            {
                Trshowdata = "C";
            }
            else if (showdata == "4")
            {
                Trshowdata = "D";
            }

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0102Repository repo = new AA0102Repository(DBWork);

                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();

                    IEnumerable<AA0102_MODEL> list = repo.FA0043_GetPrintData(APPTIME1, Trshowdata);
                    foreach (AA0102_MODEL item in list)
                    {
                        item.REPORT_TYPE = "FA0043";
                        item.CHK_YM = APPTIME1;
                        if (item.CHK_LEVEL == "0")
                        {
                            continue;
                        }
                        item.CHK_TYPE_NAME = repo.GetChkWhkindName(item.WH_KIND, item.CHK_TYPE);
                        
                    }
                    list = list.OrderBy(x => x.INID).ThenBy(x => x.WH_NO).ThenBy(x => x.CHK_TYPE).ToList<AA0102_MODEL>();

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", list));

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