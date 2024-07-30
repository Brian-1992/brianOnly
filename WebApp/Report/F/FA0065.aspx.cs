using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using WebApp.Repository.F;
using JCLib.DB;

namespace WebApp.Report.F
{
    public partial class FA0065 : SiteBase.BasePage
    {
        string data_ym;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                data_ym = (Request.QueryString["DATA_YM"] is null) ? "" : Request.QueryString["DATA_YM"].ToString().Replace("null", "");

                reportBind();
            }
        }
        protected void reportBind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0065Repository repo = new FA0065Repository(DBWork);
                    DataTable dt = new DataTable();
                    dt.Columns.Add("ITEM_NAME");        // 統計分類
                    dt.Columns.Add("AMT_1");            // 藥品類
                    dt.Columns.Add("AMT_2");            // 消耗性醫療器材類
                    dt.Rows.Add(SetRow(dt, "當月醫療總收入金額(A)", "--------", "--------", ""));

                    IEnumerable<FA0065M> myEnum = repo.GetAll(data_ym);
                    myEnum.GetEnumerator();
                    foreach (var item in myEnum)
                    {
                        for (int i = 1; i < 10; i++)
                        {
                            bool flag = false;
                            if (item.MAT_CLASS.Contains("$"))
                                flag = true;

                            if (i == 1)
                                AddRow(ref dt, flag, "當月期初結存金額(B)", item.MAT_CLASS, item.P_INV_AMT);
                            else if (i == 2)
                                AddRow(ref dt, flag, "當月期末結存金額(C)", item.MAT_CLASS, item.INV_AMT);
                            else if (i == 3)
                                AddRow(ref dt, flag, "差異金額(K)", item.MAT_CLASS, item.D_AMT);
                            else if (i == 4)
                            {
                                double b = (Convert.ToDouble(dt.Rows[1][2].ToString()) + Convert.ToDouble(dt.Rows[2][2].ToString())) / 2;
                                dt.Rows.Add(SetRow(dt, "當月平均庫儲金額(D)=(B+C)/2", "--------", b.ToString("f2"), ""));
                            }
                            else if (i == 5)
                                AddRow(ref dt, flag, "當月消耗金額(E)", item.MAT_CLASS, item.USE_AMT);
                            else if (i == 6)
                            {
                                double b = 0;

                                if (Convert.ToDouble(dt.Rows[4][2].ToString()) == 0)
                                    b = 0;  // 分母為零
                                else
                                    b = Convert.ToDouble(dt.Rows[2][2].ToString()) / Convert.ToDouble(dt.Rows[4][2].ToString());
                                dt.Rows.Add(SetRow(dt, "當月期末比值(F)=(C/E)", "--------", b.ToString("f2"), ""));
                            }
                            else if (i == 7)
                            {
                                double b = 0;
                                if (Convert.ToDouble(dt.Rows[3][2].ToString()) == 0)
                                    b = 0;  // 分母為零
                                else
                                    b = Convert.ToDouble(dt.Rows[4][2].ToString()) / Convert.ToDouble(dt.Rows[3][2].ToString());
                                dt.Rows.Add(SetRow(dt, "當月存貨週轉率(G)=(E/D)", "--------", b.ToString("f2"), ""));
                            }
                            else if (i == 8)
                                dt.Rows.Add(SetRow(dt, "當月消耗金額佔醫療收入之百分比(H)=(E/A)*100%", "--------", "--------", ""));
                            else if (i == 9)
                                AddRow(ref dt, flag, "當月低週轉率品項結存金額(I)", item.MAT_CLASS, item.LOWTURN_INV_AMT);
                            else if (i == 10)
                            {
                                double b = 0;
                                if (Convert.ToDouble(dt.Rows[4][2].ToString()) == 0)
                                    b = 0;  // 分母為零
                                else
                                    b = Convert.ToDouble(dt.Rows[8][2].ToString()) / Convert.ToDouble(dt.Rows[4][2].ToString()) * 100;
                                dt.Rows.Add(SetRow(dt, "當月低週轉率品項之百分比(J)=(I/E)*100%", "-------- ", b.ToString("f2") + "%", ""));

                            }
                        }
                    }
                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DataYm", data_ym) });
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dt));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
            }
        }

        protected void AddRow(ref DataTable dt, bool flag, string item_name, string mat_class, string value)
        {
            if (flag)
            {
                string[] tmp = value.Split('$');
                dt.Rows.Add(SetRow(dt, item_name, "--------", tmp[1], ""));
            }
            else
                dt.Rows.Add(SetRow(dt, item_name, "--------", value, ""));
        }

        protected DataRow SetRow(DataTable dt, string item_name, string amt_1, string amt_2, string remark)
        {
            DataRow dr = dt.NewRow();
            dr["ITEM_NAME"] = item_name;
            dr["AMT_1"] = amt_1;
            dr["AMT_2"] = amt_2;

            return dr;
        }
    }
}