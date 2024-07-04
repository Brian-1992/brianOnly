using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using JCLib.DB;
using WebApp.Models;

namespace WebApp.Report.A
{
    public partial class AA0122 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                report1Bind();
                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
            }
        }

        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0122Repository repo = new AA0122Repository(DBWork);
                    AA0122Repository.MI_MAST_QUERY_PARAMS query = new AA0122Repository.MI_MAST_QUERY_PARAMS();

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Inid", Request.QueryString["Inid"].ToString().Split(' ')[0]) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("InidName", Request.QueryString["Inid"].ToString().Split(' ')[1]) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Inid", DBWork.UserInfo.Inid) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("InidName", DBWork.UserInfo.InidName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Sum", repo.GetSum()) });

                    query.DATA_YM = Request.QueryString["ym"].ToString().Replace("null", "");
                    query.MAT_CLASS = Request.QueryString["mat_class"].ToString().Replace("null", "");

                    if (query.MAT_CLASS == "")
                    {
                        IEnumerable<COMBO_MODEL> myEnum = repo.GetMatClassCombo(User.Identity.Name);
                        myEnum.GetEnumerator();
                        int i = 0;
                        foreach (var item in myEnum)
                        {
                            if (i == 0)
                                query.MAT_CLASS += item.VALUE;
                            else
                                query.MAT_CLASS += "," + item.VALUE;
                            i++;
                        }
                    }

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.GetPrintData(query)));

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