using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Report.F
{
    public partial class FA0004 : Page
    {
        string chk_ym, chk_yyyymm, mat_class, m_sotreid, yyy, mm;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                chk_ym = Request.QueryString["CHK_YM"].ToString().Replace("null", "");
                chk_yyyymm = Request.QueryString["CHK_YYYYMM"].ToString().Replace("null", "");
                mat_class = Request.QueryString["MAT_CLASS"].ToString().Replace("null", "");
                m_sotreid = Request.QueryString["M_STOREID"].ToString().Replace("null", "");
                SetYYYYMM(chk_yyyymm);
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
                    FA0004Repository repo = new FA0004Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    IEnumerable<FA0004M> list = repo.Print(chk_ym, m_sotreid, mat_class, User.Identity.Name);
                    list = GetGroup(list);
                    list = GetCompleteData(list);
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0004", list));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }
        }

        public IEnumerable<FA0004M> GetGroup(IEnumerable<FA0004M> list)
        {
            var o = from a in list
                    group a by new
                    {
                        MMCODE = a.MMCODE,
                        MMNAME_C = a.MMNAME_C,
                        MMNAME_E = a.MMNAME_E,
                        PRINTUSER = a.PRINTUSER
                        // STATUS_TOT = a.STATUS_TOT,
                        //CHK_QTY1 = a.CHK_QTY1,
                        //CHK_QTY2 = a.CHK_QTY2,
                        //CHK_QTY3 = a.CHK_QTY3,
                        //APL_OUTQTY = a.APL_OUTQTY,
                        //STORE_QTY = a.STORE_QTY,
                        //M_CONTPRICE = a.M_CONTPRICE
                    } into g
                    select new FA0004M
                    {
                        MMCODE = g.Key.MMCODE,
                        MMNAME_C = g.Key.MMNAME_C,
                        MMNAME_E = g.Key.MMNAME_E,
                        PRINTUSER = g.Key.PRINTUSER,
                        // STATUS_TOT = g.Select(x => x.STATUS_TOT).ToString(),
                        //CHK_QTY1 = g.Key.CHK_QTY1,
                        //CHK_QTY2 = g.Key.CHK_QTY2,
                        // CHK_QTY3 = g.Key.CHK_QTY3,
                        //APL_OUTQTY = g.Key.APL_OUTQTY,
                        //STORE_QTY = g.Key.STORE_QTY,
                        // M_CONTPRICE = g.Key.M_CONTPRICE,
                        items = g.OrderByDescending(x => int.Parse(x.STATUS_TOT)).ToList()
                    };

            return o.ToList();
        }

        public IEnumerable<FA0004M> GetCompleteData(IEnumerable<FA0004M> list)
        {
            string lineBreak = "\n";
            foreach (FA0004M item in list)
            {
                FA0004M temp = item.items.Select(x => x).First();
                item.CHK_QTY1 = temp.CHK_QTY1;
                item.CHK_QTY2 = temp.CHK_QTY2;
                item.CHK_QTY3 = temp.CHK_QTY3;
                item.STATUS_TOT = temp.STATUS_TOT;
                item.APL_OUTQTY = temp.APL_OUTQTY;
                item.M_CONTPRICE = temp.M_CONTPRICE;
                item.STORE_QTY = temp.STORE_QTY;
                item.COMMENT = string.Empty;
                item.CHK_YYY = yyy;
                item.CHK_MM = mm;

                foreach (FA0004M innerItem in item.items)
                {
                    if (innerItem.CHK_REMARK != string.Empty)
                    {
                        if (item.COMMENT != string.Empty)
                        {
                            item.COMMENT += lineBreak;
                        }

                        item.COMMENT += string.Format("{0} {1}", innerItem.STORE_LOC, innerItem.CHK_REMARK);
                    }
                }

                float chk_qty = item.STATUS_TOT == "1" ? float.Parse(item.CHK_QTY1) :
                                item.STATUS_TOT == "2" ? float.Parse(item.CHK_QTY2) :
                                                         float.Parse(item.CHK_QTY3);
                float apl_outqty = item.APL_OUTQTY == null ? 0 : float.Parse(item.APL_OUTQTY);
                item.DIFF_QTY = (chk_qty - float.Parse(item.STORE_QTY)).ToString();
                item.DIFF_COST = (float.Parse(item.DIFF_QTY) * float.Parse(item.M_CONTPRICE)).ToString();
                item.DIFF_PERC = apl_outqty == 0 ? "0" :
                                 Math.Round((float.Parse(item.DIFF_QTY) / apl_outqty * 100), 2).ToString() + "%";

            }

            return list;
        }

        private void SetYYYYMM(string chk_yyyymm)
        {
            DateTime date = DateTime.Parse(chk_yyyymm);
            yyy = (date.Year - 1911).ToString();
            mm = (date.Month).ToString().PadLeft(2, '0');
        }
    }
}