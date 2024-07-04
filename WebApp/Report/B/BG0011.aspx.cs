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
    public partial class BG0011 : System.Web.UI.Page
    {
        string rdlc;
        string invoice_dt;
        string category;
        string agenNo1, agenNo2;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                rdlc = Request.QueryString["rdlc"].ToString();
                invoice_dt = Request.QueryString["P0"].ToString();
                category = Request.QueryString["P2"].ToString();
                agenNo1= Request.QueryString["P4"].ToString();
                agenNo2 = Request.QueryString["P5"].ToString();

                if (rdlc == "1")
                    ReportBind_1();
                else
                    ReportBind_2();
            }
        }

        protected void ReportBind_1()
        {
            ReportViewer1.Visible = true;
            ReportViewer2.Visible = false;
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
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("INVOICE_DT", tmpInvoicdeDt) });

                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    BG0011Repository repo = new BG0011Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    IEnumerable<WebApp.Models.BG0011> temp = repo.Report_1(invoice_dt, category, agenNo1, agenNo2);
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BG0011", temp));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }
        }

        protected void ReportBind_2()
        {
            ReportViewer1.Visible = false;
            ReportViewer2.Visible = true;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    string tmpInvoicdeDt = "";

                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();

                    if (rdlc == "3")
                    {
                        ReportViewer2.LocalReport.ReportPath = @"Report\B\BG0011_2.rdlc";
                    }

                    ReportViewer2.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer2.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

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
                    ReportViewer2.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("INVOICE_DT", tmpInvoicdeDt) });

                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer2.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    BG0011Repository repo = new BG0011Repository(DBWork);
                    ReportViewer2.EnableTelemetry = false;
                    IEnumerable<WebApp.Models.BG0011> temp = repo.Report_1(invoice_dt, category, agenNo1, agenNo2);
                    ReportViewer2.LocalReport.DataSources.Add(new ReportDataSource("BG0011", temp));

                    ReportViewer2.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
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
            return ret.Length == 0 ? chineseNumber[0] : ret.ToString();
        }
    }
}