using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Models;
using WebApp.Repository.AA;
using JCLib.DB;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
namespace WebApp.Report.A
{
    public partial class AA0163 : SiteBase.BasePage
    {
        string money1;
        string money2;
        string data_ym;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                money1 = (Request.QueryString["MONEY1"] is null) ? "" : Request.QueryString["MONEY1"].ToString().Replace("null", "");
                money2 = (Request.QueryString["MONEY2"] is null) ? "" : Request.QueryString["MONEY2"].ToString().Replace("null", "");
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
                    AA0163Repository repo = new AA0163Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    DataTable dt = new DataTable();
                    dt.Columns.Add("ITEM_NAME");        // 統計分類
                    dt.Columns.Add("AMT_1");            // 藥品類
                    dt.Columns.Add("AMT_2");            // 消耗性醫療器材類

                    dt.Rows.Add(SetRow(dt, "當月醫療總收入金額(A)", money1, money2, ""));

                    IEnumerable<AA0163M> myEnum = repo.GetAll(data_ym);
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
                                double a = (Convert.ToDouble(dt.Rows[1][1].ToString()) + Convert.ToDouble(dt.Rows[2][1].ToString())) / 2;
                                double b = (Convert.ToDouble(dt.Rows[1][2].ToString()) + Convert.ToDouble(dt.Rows[2][2].ToString())) / 2;
                                dt.Rows.Add(SetRow(dt, "當月平均庫儲金額(D)=(B+C)/2", a.ToString("f2"), b.ToString("f2"), ""));
                            }
                            else if (i == 5)
                                AddRow(ref dt, flag, "當月消耗金額(E)", item.MAT_CLASS, item.USE_AMT);
                            else if (i == 6)
                            {
                                double a = 0, b = 0;
                                if (Convert.ToDouble(dt.Rows[4][1].ToString()) == 0)
                                    a = 0;  // 分母為零
                                else
                                    a = Convert.ToDouble(dt.Rows[2][1].ToString()) / Convert.ToDouble(dt.Rows[4][1].ToString());

                                if (Convert.ToDouble(dt.Rows[4][2].ToString()) == 0)
                                    b = 0;  // 分母為零
                                else
                                    b = Convert.ToDouble(dt.Rows[2][2].ToString()) / Convert.ToDouble(dt.Rows[4][2].ToString());
                                dt.Rows.Add(SetRow(dt, "當月期末比值(F)=(C/E)", a.ToString("f2"), b.ToString("f2"), ""));
                            }
                            else if (i == 7)
                            {
                                double a = 0, b = 0;
                                if (Convert.ToDouble(dt.Rows[3][1].ToString()) == 0)
                                    a = 0;  // 分母為零
                                else
                                    a = Convert.ToDouble(dt.Rows[4][1].ToString()) / Convert.ToDouble(dt.Rows[3][1].ToString());

                                if (Convert.ToDouble(dt.Rows[3][2].ToString()) == 0)
                                    b = 0;  // 分母為零
                                else
                                    b = Convert.ToDouble(dt.Rows[4][2].ToString()) / Convert.ToDouble(dt.Rows[3][2].ToString());
                                dt.Rows.Add(SetRow(dt, "當月存貨週轉率(G)=(E/D)", a.ToString("f2"), b.ToString("f2"), ""));
                            }
                            else if (i == 8)
                            {
                                double a = 0, b = 0;
                                if (Convert.ToDouble(dt.Rows[0][1].ToString()) == 0)
                                    a = 0;  // 分母為零
                                else
                                    a = Convert.ToDouble(dt.Rows[4][1].ToString()) / Convert.ToDouble(dt.Rows[0][1].ToString()) * 100;

                                if (Convert.ToDouble(dt.Rows[0][2].ToString()) == 0)
                                    b = 0;  // 分母為零
                                else
                                    b = Convert.ToDouble(dt.Rows[4][2].ToString()) / Convert.ToDouble(dt.Rows[0][2].ToString()) * 100;
                                dt.Rows.Add(SetRow(dt, "當月消耗金額佔醫療收入之百分比(H)=(E/A)*100%", a.ToString("f2") + "%", b.ToString("f2") + "%", ""));
                            }
                            else if (i == 9)
                                AddRow(ref dt, flag, "當月低週轉率品項結存金額(I)", item.MAT_CLASS, item.LOWTURN_INV_AMT);
                            else if (i == 10)
                            {
                                double a = 0, b = 0;
                                if (Convert.ToDouble(dt.Rows[4][1].ToString()) == 0)
                                    a = 0;  // 分母為零
                                else
                                    a = Convert.ToDouble(dt.Rows[8][1].ToString()) / Convert.ToDouble(dt.Rows[4][1].ToString()) * 100;

                                if (Convert.ToDouble(dt.Rows[4][2].ToString()) == 0)
                                    b = 0;  // 分母為零
                                else
                                    b = Convert.ToDouble(dt.Rows[8][2].ToString()) / Convert.ToDouble(dt.Rows[4][2].ToString()) * 100;
                                dt.Rows.Add(SetRow(dt, "當月低週轉率品項之百分比(J)=(I/E)*100%", a.ToString("f2") + "%", b.ToString("f2") + "%", ""));

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
                dt.Rows.Add(SetRow(dt, item_name, tmp[0], tmp[1], ""));
            }
            else
            {
                if (mat_class == "01")
                    dt.Rows.Add(SetRow(dt, item_name, value, "0", ""));
                else
                    dt.Rows.Add(SetRow(dt, item_name, "0", value, ""));
            }
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