using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.B;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Report.B
{
    public partial class BG0010 : System.Web.UI.Page
    {
        string rdlc;
        string invoice_dt;
        string agenNo1, agenNo2;
        string matClass;
        string[] arr_matClass = { };
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                rdlc = Request.QueryString["rdlc"].ToString();
                invoice_dt = Request.QueryString["P0"].ToString();
                matClass = Request.QueryString["P1"].ToString();
                agenNo1 = Request.QueryString["P2"].ToString();
                agenNo2 = Request.QueryString["P3"].ToString();

                if (matClass.ToUpper() == "NULL")
                    matClass = "";

                if (!string.IsNullOrEmpty(matClass))
                {
                    arr_matClass = matClass.Trim().Split(','); //用,分割
                }
                if (rdlc == "1")
                    ReportBind();
                else if (rdlc == "2" || rdlc == "3" || rdlc == "4")
                    ReportBind_2();
            }
        }

        protected void ReportBind()
        {
            ReportViewer1.Visible = true;
            ReportViewer2.Visible = false;

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    string tmpInvoicdeDt = "";

                    BG0010Repository repo_rdlc = new BG0010Repository(DBWork);
                    string hospFullName = repo_rdlc.GetHospFullName();

                    if (invoice_dt.Length == 5)
                    {
                        tmpInvoicdeDt = GetChineseNumber(invoice_dt.Substring(0, 3));
                        tmpInvoicdeDt += "年度";
                        if (invoice_dt.Substring(3, 1) == "0")
                            tmpInvoicdeDt += GetChineseNumber(invoice_dt.Substring(4, 1));
                        else
                            tmpInvoicdeDt += GetChineseNumber(invoice_dt.Substring(3, 2));
                        tmpInvoicdeDt += "月份";
                    }

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Title", hospFullName + tmpInvoicdeDt + "支付廠商明細表") });

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    BG0010Repository repo = new BG0010Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;

                    IEnumerable<WebApp.Models.BG0010> temp = repo.Report(invoice_dt, arr_matClass, agenNo1, agenNo2); ;

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BG0010", temp));
                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
            }
        }

        protected void ReportBind_2()
        {
            ReportViewer1.Visible = false;
            ReportViewer2.Visible = true;

            DataTable dt = new DataTable();
            dt.Columns.Add("F1");
            dt.Columns.Add("F2");
            dt.Columns.Add("F3");
            dt.Columns.Add("F4");
            dt.Columns.Add("F5");
            dt.Columns.Add("F6");
            dt.Columns.Add("F7");
            dt.Columns.Add("F8");
            dt.Columns.Add("F9");
            dt.Columns.Add("F10");
            dt.Columns.Add("F11");
            dt.Columns.Add("F12");
            dt.Columns.Add("F13");
            dt.Columns.Add("F14");
            dt.Columns.Add("F15");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    string tmpInvoicdeDt = "";
                    if (invoice_dt.Length == 5)
                    {
                        tmpInvoicdeDt = GetChineseNumber(invoice_dt.Substring(0, 3));
                        tmpInvoicdeDt += "年度";
                        if (invoice_dt.Substring(3, 1) == "0")
                            tmpInvoicdeDt += GetChineseNumber(invoice_dt.Substring(4, 1));
                        else
                            tmpInvoicdeDt += GetChineseNumber(invoice_dt.Substring(3, 2));
                        tmpInvoicdeDt += "月份";
                    }

                    BG0010Repository repo_rdlc = new BG0010Repository(DBWork);
                    string hospFullName = repo_rdlc.GetHospFullName();

                    if (rdlc == "2")
                        ReportViewer2.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Title", hospFullName + tmpInvoicdeDt + "民眾實付廠商明細表") });
                    else if (rdlc == "3")
                        ReportViewer2.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Title", hospFullName + tmpInvoicdeDt + "廠商聯標契約優惠明細表") });
                    else
                        ReportViewer2.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Title", hospFullName + tmpInvoicdeDt + "民眾應付廠商明細表") });

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day;
                    ReportViewer2.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    
                    BG0010Repository repo = new BG0010Repository(DBWork);
                    ReportViewer2.EnableTelemetry = false;
                    int i = 0;
                    double sum = 0;
                    DataRow dr = dt.NewRow();
                    IEnumerable<WebApp.Models.BG0010> temp;
                    
                    if (rdlc == "2")
                        temp = repo.Report_2(2, invoice_dt, arr_matClass, agenNo1, agenNo2);
                    else if (rdlc == "3")
                        temp = repo.Report_2(3, invoice_dt, arr_matClass, agenNo1, agenNo2);
                    else
                        temp = repo.Report_2(4, invoice_dt, arr_matClass, agenNo1, agenNo2);
                    foreach (var item in temp)
                    {
                        dr[i % 5 * 3] = item.F1; // 廠商代碼
                        dr[i % 5 * 3 + 1] = item.F2; // 廠商名稱
                        dr[i % 5 * 3 + 2] = item.F3; // 金額
                        sum += Convert.ToDouble(item.F3);

                        if (i != 0 && (i + 1) % 5 == 0)
                        {
                            dt.Rows.Add(dr);
                            dr = dt.NewRow();
                        }
                        i++;
                    }

                    if (dr[0].ToString() != "")
                        dt.Rows.Add(dr);

                    ReportViewer2.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("SumMoney", sum.ToString()) });

                    ReportViewer2.LocalReport.DataSources.Add(new ReportDataSource("BG0010", dt));
                    ReportViewer2.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
            }
        }

        protected string GetChineseNumber(string number)
        {
            string[] chineseNumber = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            string[] unit = { "", "十", "百", "千", "萬", "十萬", "百萬", "千萬", "億", "十億", "百億", "千億", "兆", "十兆", "百兆", "千兆" };
            System.Text.StringBuilder ret = new System.Text.StringBuilder();
            string inputNumber = number.ToString();
            int idx = inputNumber.Length;
            bool needAppendZero = false;
            if (Convert.ToInt32(number) == 10)
            {
                // 數字10
                ret.Append("十");
            }
            else if (Convert.ToInt32(number) >= 10 && Convert.ToInt32(number) <= 19)
            {
                // 數字11 ~ 19特別處理
                ret.Append("十");
                char c = inputNumber[1];
                ret.Append(chineseNumber[(int)(c - '0')]);
            }
            else
            {
                foreach (char c in inputNumber)
                {
                    idx--;
                    if (c > '0')
                    {
                        if (needAppendZero)
                        {
                            ret.Append(chineseNumber[0]);
                            needAppendZero = false;
                        }
                        ret.Append(chineseNumber[(int)(c - '0')] + unit[idx]);
                    }
                    else
                        needAppendZero = true;
                }
            }
            
            return ret.Length == 0 ? chineseNumber[0] : ret.ToString();
        }
    }
}