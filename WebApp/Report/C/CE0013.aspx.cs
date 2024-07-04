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
    public partial class CE0013 : Page
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
                condition = Request.QueryString["CONDITION"].ToString().Replace("null", "");
                wh_kind = Request.QueryString["WH_KIND"].ToString().Replace("null", "");

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
                    CE0013Repository repo = new CE0013Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;

                    IEnumerable<CE0013Data> temp = repo.PrintData(chk_no, chk_level, wh_kind, condition);
                    foreach (CE0013Data item in temp) {
                        item.CHK_TYPE_NAME = repo.GetChkWhkindName(item.CHK_WH_KIND, item.CHK_TYPE);
                    }
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0013Data", temp));

                    CE0013Count countAll = repo.GetCountAll(chk_no, chk_level, wh_kind);
                    CE0013Count countP = repo.GetCountP(chk_no, chk_level, wh_kind);
                    CE0013Count countN = repo.GetCountN(chk_no, chk_level, wh_kind);
                    IEnumerable<CE0013Count> count = GetCountItem(countAll, countP, countN);

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0013Count", count));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }


        }

        public IEnumerable<CE0013Count> GetCountItem(CE0013Count all, CE0013Count p, CE0013Count n)
        {
            List<CE0013Count> list = new List<CE0013Count>();
            CE0013Count item = new CE0013Count()
            {
                TOT1 = all.TOT1,
                TOT2 = all.TOT2,
                TOT3 = all.TOT3,
                TOT4 = all.TOT4,
                TOT5 = all.TOT5,
                P_TOT1 = p.P_TOT1,
                P_TOT2 = p.P_TOT2,
                P_TOT3 = p.P_TOT3,
                P_TOT4 = p.P_TOT4,
                P_TOT5 = p.P_TOT5,
                N_TOT1 = n.N_TOT1,
                N_TOT2 = n.N_TOT2,
                N_TOT3 = n.N_TOT3,
                N_TOT4 = n.N_TOT4,
                N_TOT5 = n.N_TOT5
            };
            list.Add(item);
            return list;
        }
    }
}