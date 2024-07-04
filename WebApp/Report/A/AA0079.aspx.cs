using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using JCLib.DB;
using WebApp.Repository.AA;
using Microsoft.Reporting.WebForms;
using WebApp.Models;

namespace WebApp.Report.A
{
    public partial class AA0079 : Page
    {
        string condition1;
        string whno;
        string whgrade;
        string condition2;
        string startDate;
        string endDate;
        string selectmonth;
        string reporttype;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                condition1 = Request.QueryString["CONDITION1"].ToString().Replace("null", "");
                whno = Request.QueryString["WHNO"].ToString().Replace("null", "");
                whgrade = Request.QueryString["WHGRADE"].ToString().Replace("null", "");
                condition2 = Request.QueryString["CONDITION2"].ToString().Replace("null", "");
                startDate = Request.QueryString["STARTDATE"].ToString().Replace("null", "");
                endDate = Request.QueryString["ENDDATE"].ToString().Replace("null", "");
                selectmonth = Request.QueryString["SELECTMONTH"].ToString().Replace("null", "");
                reporttype = Request.QueryString["REPORTTYPE"].ToString().Replace("null", "");

                if (condition2 == "1") {
                    DateTime temp = DateTime.Parse(selectmonth);

                    startDate = new DateTime(temp.Year, temp.Month, 1).ToString("yyyy-MM-dd");
                    endDate = DateTime.Parse(startDate).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
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
                    AA0079Repository repo = new AA0079Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    string hospCode = repo.GetHospCode();
                    IEnumerable<AA0079M> list = repo.Print(condition1, whno, whgrade, startDate, endDate, reporttype, hospCode == "0");
                    list = GetSum(list);
                   ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0079", list));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }


        }

        public IEnumerable<AA0079M> GetSum(IEnumerable<AA0079M> list) {
            var o = from a in list
                    orderby a.DOCTYPE, a.POST_TIME, a.TOWH, a.MMCODE ascending
                    group a by new
                    {
                        DOCTYPE = a.DOCTYPE,
                        POST_TIME = a.POST_TIME,
                        TOWH = a.TOWH,
                        MMCODE = a.MMCODE,
                        MMNAME_E = a.MMNAME_E
                    } into g
                    select new AA0079M
                    {
                        DOCTYPE = g.Key.DOCTYPE,
                        POST_TIME = g.Key.POST_TIME,
                        TOWH = g.Key.TOWH,
                        MMCODE = g.Key.MMCODE,
                        MMNAME_E = g.Key.MMNAME_E,
                        SUM = g.Sum(x=>float.Parse(x.APPQTY)),
                        ITEMS = g.ToList()
                    };
            List<AA0079M> tempList = o.ToList();
            List<AA0079M> newList = new List<AA0079M>();
            foreach (AA0079M item in tempList) {
                foreach (AA0079M innerItem in item.ITEMS) {
                    newList.Add(innerItem);
                }
                AA0079M total = new AA0079M();
                total.APPID = "-------";
                total.SUM = item.SUM;
                newList.Add(total);
            }
            return newList;
        }
    }
}