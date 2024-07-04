using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.F;
using WebApp.Models;
using JCLib.DB;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using WebApp.Repository.AA;

namespace WebApp.Report.F
{
    public partial class FA0017_1 : SiteBase.BasePage
    {
        string P0; //物料分類
        string P1; //進貨日期起
        string P2; //進貨日期迄
        string P3; //中信或院內
        string P0_NAME; //物料分類名稱

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //RDLC本身的列印功能鍵
                //ReportViewer1.ShowPrintButton = false;
                //ReportViewer1.ShowExportControls = false;
                P0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
                P0_NAME = (Request.QueryString["p0_Name"] is null) ? "" : Request.QueryString["p0_Name"].ToString().Replace("null", "");
                P1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = (Request.QueryString["p2"] is null) ? "" : Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = (Request.QueryString["p3"] is null) ? "" : Request.QueryString["p3"].ToString().Replace("null", "");

                report1Bind();
            }
        }
        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0017Repository repo = new FA0017Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    //停用收集使用方式資料，加快報表顯示
                    ReportViewer1.EnableTelemetry = false;

                    //將IEnumerable轉DataTable
                    //也可將DataTable當作LocalReport的DataSources
                    //tmpdt_FA0017 = LinqQueryToDataTable(repo.GetPrintData_All_2(P0, P1, P2, P3));
                    //tmpdt_FA0017_D = LinqQueryToDataTable(repo.GetPrintData_Detail(P1, P2, P0));

                    //CombineDataTable(dt_FA0017, tmpdt_FA0017, tmpdt_FA0017_D);

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    string P3_C = (P3 == "Y") ? "(中信平台)" : "(院內購案)";
                    string str_ReportTittle = P1.Substring(0, 3) + "年" + P1.Substring(3, 2) + "月" + P0_NAME + "類採購結報月報表" + P3_C;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ReportTittle", str_ReportTittle) });

                    string str_UserDept = DBWork.UserInfo.InidName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserDept", str_UserDept) });

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("FA0017", repo.GetPrintData_All_2(P0, P1, P2, P3)));
                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }
        }

        /// <summary>
        /// IEnumerable To DataTable(參考網路)
        /// </summary>
        /// <typeparam name="T">資料型態</typeparam>
        /// <param name="query">IEnumerable資料</param>
        /// <returns></returns>
        public static DataTable LinqQueryToDataTable<T>(IEnumerable<T> query)
        {
            DataTable tbl = new DataTable();
            PropertyInfo[] props = null;
            foreach (T item in query)
            {
                if (props == null) //尚未初始化
                {
                    Type t = item.GetType();
                    props = t.GetProperties();
                    foreach (PropertyInfo pi in props)
                    {
                        Type colType = pi.PropertyType;
                        //針對Nullable<>特別處理
                        if (colType.IsGenericType && colType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }
                        //建立欄位
                        tbl.Columns.Add(pi.Name, colType);
                    }
                }
                DataRow row = tbl.NewRow();
                foreach (PropertyInfo pi in props)
                {
                    row[pi.Name] = pi.GetValue(item, null) ?? DBNull.Value;
                }
                tbl.Rows.Add(row);
            }
            return tbl;
        }

        public void CreatDataTable(DataTable dt)
        {
            dt.Clear();
            dt.Columns.Add("項次");
            dt.Columns["項次"].AutoIncrement = true;
            dt.Columns["項次"].AutoIncrementSeed = 1;
            dt.Columns["項次"].AutoIncrementStep = 1;
            dt.Columns.Add("AGEN_NO");
            dt.Columns.Add("AGEN_NAMEC");
            dt.Columns.Add("FULLSUM", Type.GetType("System.Decimal"));
            dt.Columns.Add("DISCSUM", Type.GetType("System.Decimal"));
            dt.Columns.Add("M_DISCPERC");
            dt.Columns.Add("PAYSUM", Type.GetType("System.Decimal"));
            dt.Columns.Add("TXFEE", Type.GetType("System.Decimal"));
            dt.Columns.Add("PAY", Type.GetType("System.Decimal"));
            dt.Columns.Add("IsBANK");
            dt.Columns.Add("AGEN_BANK");
            dt.Columns.Add("AGEN_SUB");
            dt.Columns.Add("AGEN_ACC");
        }

        public void CombineDataTable(DataTable dt_Target, DataTable dt_Source1, DataTable dt_Source2)
        {
            int TargetIndex = 0;
            //建立資料欄位
            CreatDataTable(dt_Target);

            //用一個欄位一個欄位塞資料
            //一筆一筆的讀取dt_Source1的資料
            foreach (DataRow tmp_dr_Source1 in dt_Source1.Rows)
            {
                //讀取廠商碼(AGEN_NO)，並用來搜尋dt_Source2是否有符合的細項資料
                string tmp_AGEN_NO = tmp_dr_Source1["AGEN_NO"].ToString();
                DataRow[] tmp_dt_Source2 = dt_Source2.Select("AGEN_NO=" + tmp_AGEN_NO);

                //手動塞入大項資料 FROM dt_Source1，並將Index+1
                dt_Target.Rows.Add();
                dt_Target.Rows[TargetIndex]["AGEN_NO"] = tmp_dr_Source1["AGEN_NO"];
                dt_Target.Rows[TargetIndex]["AGEN_NAMEC"] = tmp_dr_Source1["AGEN_NAMEC"];
                dt_Target.Rows[TargetIndex]["FULLSUM"] = tmp_dr_Source1["FULLSUM"];
                dt_Target.Rows[TargetIndex]["DISCSUM"] = tmp_dr_Source1["DISCSUM"];
                dt_Target.Rows[TargetIndex]["PAYSUM"] = tmp_dr_Source1["PAYSUM"];
                dt_Target.Rows[TargetIndex]["TXFEE"] = tmp_dr_Source1["TXFEE"];
                dt_Target.Rows[TargetIndex]["PAY"] = tmp_dr_Source1["PAY"];
                dt_Target.Rows[TargetIndex]["AGEN_BANK"] = tmp_dr_Source1["AGEN_BANK"];
                dt_Target.Rows[TargetIndex]["AGEN_SUB"] = tmp_dr_Source1["AGEN_SUB"];
                dt_Target.Rows[TargetIndex]["AGEN_ACC"] = tmp_dr_Source1["AGEN_ACC"];

                TargetIndex++;

                //將搜尋出來的細項結果手動塞入dt_Target
                //一個一個的指定欄位名稱填入資料，並將Index+1
                foreach (DataRow tmp_dr_Source2 in tmp_dt_Source2)
                {
                    dt_Target.Rows.Add();
                    dt_Target.Rows[TargetIndex]["FULLSUM"] = tmp_dr_Source2["FULLSUM"];
                    dt_Target.Rows[TargetIndex]["DISCSUM"] = tmp_dr_Source2["DISCSUM"];
                    dt_Target.Rows[TargetIndex]["M_DISCPERC"] = tmp_dr_Source2["M_DISCPERC"];
                    dt_Target.Rows[TargetIndex]["PAYSUM"] = tmp_dr_Source2["PAYSUM"];

                    TargetIndex++;
                }
            }
        }
    }
}