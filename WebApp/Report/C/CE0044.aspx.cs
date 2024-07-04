using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.C;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;
using WebApp.Repository.AA;
using System.Linq;

namespace WebApp.Report.C
{
    public partial class CE0044 : Page
    {
        string p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, wh_name, report, p20, p21;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                p0 = Request.QueryString["p0"].ToString().Replace("null", ""); // 月結年月
                p1 = Request.QueryString["p1"].ToString().Replace("null", ""); // 盤存單位
                p2 = Request.QueryString["p2"].ToString().Replace("null", ""); // 庫房類別
                p3 = Request.QueryString["p3"].ToString().Replace("null", ""); // 庫房代碼
                p4 = Request.QueryString["p4"].ToString().Replace("null", ""); // 責任中心代碼
                p5 = Request.QueryString["p5"].ToString().Replace("null", ""); // 藥材類別
                p6 = Request.QueryString["p6"].ToString().Replace("null", ""); // 藥材代碼
                p7 = Request.QueryString["p7"].ToString().Replace("null", ""); // 是否合約
                p8 = Request.QueryString["p8"].ToString().Replace("null", ""); // 買斷寄庫
                p9 = Request.QueryString["p9"].ToString().Replace("null", ""); // 是否戰備
                p10 = Request.QueryString["p10"].ToString().Replace("null", ""); // 管制品項
                p11 = Request.QueryString["p11"].ToString().Replace("null", ""); // 是否常用品項
                p12 = Request.QueryString["p12"].ToString().Replace("null", ""); // 急救品項
                p13 = Request.QueryString["p13"].ToString().Replace("null", ""); // 中西藥別
                p14 = Request.QueryString["p14"].ToString().Replace("null", ""); // 合約類別
                p15 = Request.QueryString["p15"].ToString().Replace("null", ""); // 採購類別
                p16 = Request.QueryString["p16"].ToString().Replace("null", ""); // 特殊品項
                p17 = Request.QueryString["p17"].ToString().Replace("null", ""); // 差異量
                p18 = Request.QueryString["p18"].ToString().Replace("null", ""); // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
                p19 = Request.QueryString["p19"].ToString().Replace("null", ""); // (期初庫存<>0)或(期初=0但有進出)
                p20 = Request.QueryString["p20"].ToString().Replace("null", ""); // 結存不含戰備量
                p21 = Request.QueryString["p21"].ToString().Replace("null", ""); // 結存不足戰備量
                wh_name = Request.QueryString["wh_name"].ToString().Replace("null", ""); // 庫房名稱
                report = Request.QueryString["report"].ToString().Replace("null", ""); // 報表選擇

                if (report == "1")
                {
                    report1Bind();
                }
                else if (report == "2")
                {
                    ReportViewer1.LocalReport.ReportPath = @"Report\C\CE0044_2.rdlc";
                    report1Bind2();
                }
                else if (report == "3")
                {
                    ReportViewer1.LocalReport.ReportPath = @"Report\C\CE0044_3.rdlc";
                    report1Bind3();
                }
            }
        }
        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;

                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });
                    string str_PrintChtYM = p0.Substring(0, 3) + "年" + p0.Substring(3, 2) + "月份";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ChtYm", str_PrintChtYM) });
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WhName", wh_name) });
                    IEnumerable<CE0044D> list = repo.GetAllD(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, User.Identity.Name, p20, p21,
                                                1, 20, "[{\"property\":\"F1\",\"direction\":\"ASC\"}]");
                    CE0044D result_data = list.First();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F15", result_data.F15) }); // 買斷結存金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F19", result_data.F19) }); // 寄庫結存金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F14", result_data.F14) }); // 本月結存總金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F5", result_data.F5) }); // 本月消耗金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F24", result_data.F24) }); // 本月正差異金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F25", result_data.F25) }); // 本月負差異金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F26", result_data.F26) }); // 本月差異金額
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F27", result_data.F27) }); // 本月單價5000元以上總消耗
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F28", result_data.F28) }); // 本月單價未滿5000元總消耗

                    ReportViewer1.LocalReport.DisplayName = "CE0044";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0044", repo.Print(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, User.Identity.Name, p20, p21)));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception e)
                {
                    var a = e.Message.ToString();
                    throw;
                }
            }
        }

        protected void report1Bind2()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;

                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });
                    string str_PrintChtYM = p0.Substring(0, 3) + "年" + p0.Substring(3, 2) + "月份";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ChtYm", str_PrintChtYM) });
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WhName", wh_name) });
                    //IEnumerable<CE0044D> list = repo.GetAllD(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, User.Identity.Name, 1, 20, "[{\"property\":\"F1\",\"direction\":\"ASC\"}]");
                    //CE0044D result_data = list.First();
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F15", result_data.F15) }); // 買斷結存金額
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F19", result_data.F19) }); // 寄庫結存金額
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F14", result_data.F14) }); // 本月結存總金額
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F5", result_data.F5) }); // 本月消耗金額
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F24", result_data.F24) }); // 本月正差異金額
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F25", result_data.F25) }); // 本月負差異金額
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F26", result_data.F26) }); // 本月差異金額
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F27", result_data.F27) }); // 本月單價5000元以上總消耗
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Result_F28", result_data.F28) }); // 本月單價未滿5000元總消耗

                    ReportViewer1.LocalReport.DisplayName = "CE0044";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0044", repo.Print2(p0, User.Identity.Name, p20)));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception e)
                {
                    var a = e.Message.ToString();
                    throw;
                }
            }
        }

        protected void report1Bind3()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0044Repository repo = new CE0044Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;

                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });
                    string str_PrintChtYM = p0.Substring(0, 3) + "年" + p0.Substring(3, 2) + "月份";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ChtYm", str_PrintChtYM) });
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    var printWhName = wh_name;
                    if (printWhName != "")
                    {
                        printWhName = "("+ printWhName + ")";
                    }
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("WhName", printWhName) });

                    ReportViewer1.LocalReport.DisplayName = "CE0044";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CE0044", repo.Print3(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, User.Identity.Name, p20, p21)));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch (Exception e)
                {
                    var a = e.Message.ToString();
                    throw;
                }
            }
        }
    }
}