using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Reflection;
using System.Web;

namespace WebApp.Report.A
{
    public partial class AA0095 :SiteBase.BasePage
    {
        /// <summary>
        /// 庫別代碼
        /// </summary>
        string P0;

        /// <summary>
        /// 月份別
        /// </summary>
        string P1;

        /// <summary>
        /// 藥品代碼_FROM
        /// </summary>
        string P2;

        /// <summary>
        /// 藥品代碼_TO
        /// </summary>
        string P3;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //RDLC本身的列印功能鍵
                //ReportViewer1.ShowPrintButton = false;
                //ReportViewer1.ShowExportControls = false;
                P0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
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
                    AA0095Repository repo = new AA0095Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    //停用收集使用方式資料，加快報表顯示
                    ReportViewer1.EnableTelemetry = false;

                    //將IEnumerable轉DataTable
                    //也可將DataTable當作LocalReport的DataSources
                    //DataTable tmpdt_AA0095 = LinqQueryToDataTable<AA0095Report_MODEL>(repo.SearchReportData(P0, P1, P2, P3));

                    ////判斷資料筆數，如果沒有大於0則將s_IsAnyData填入無資料(報表顯示用)
                    //string s_IsAnyData = tmpdt_AA0095.Rows.Count > 0 ? " " : "無資料";
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("IsAnyData", s_IsAnyData) });

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    string str_YearMonth = P1;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("YearMonth", str_YearMonth) });

                    string str_UserName = DBWork.UserInfo.UserName;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintUser", str_UserName) });

                    ReportViewer1.PageCountMode = PageCountMode.Actual;

                    //先清空報表的DataSet，再將讀到的AA0095 DataTable放到DataSources(對應到AA0095.xsd)
                    ReportViewer1.LocalReport.DataSources.Clear();
                    //ReportViewer1.LocalReport.DisplayName = "AA0095";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0095", repo.SearchReportData(P0, P1, P2, P3)));

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
    }
}