using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using JCLib.DB;
using WebApp.Repository.F;
using Microsoft.Reporting.WebForms;
using WebApp.Models;

namespace WebApp.Report.F
{
    public partial class FA0030 : Page
    {
        string DATA_YM;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DATA_YM = Request.QueryString["DATA_YM"].ToString().Replace("null", "");
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
                    FA0030Repository repo = new FA0030Repository(DBWork);

                    IEnumerable<Models.F.FA0030> list = repo.Print(DATA_YM);
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TITLE_NAME", repo.GetDept(User.Identity.Name) + DATA_YM + "總材積明細表") });
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0030", list));

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