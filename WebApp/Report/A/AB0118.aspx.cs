using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using WebApp.Models;
using JCLib.DB;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Linq;
using WebApp.Repository.AA;

namespace WebApp.Report.AB
{
    public partial class AB0118 : System.Web.UI.Page
    {
        /// <summary>
        /// 申請單號
        /// </summary>
        string DOCNO;

        string Order;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //隱藏RDLC本身的列印功能鍵
                ReportViewer1.ShowPrintButton = false;
                ReportViewer1.ShowExportControls = false;
                DOCNO = (Request.QueryString["docno"] is null) ? "" : Request.QueryString["docno"].ToString().Replace("null", "");
                Order = (Request.QueryString["Order"] is null) ? "0" : Request.QueryString["Order"].ToString();
                ReportViewer1.ShowPrintButton = true;
                ReportViewer1.ShowExportControls = true;

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
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();

                    string[] arr_docno = { };
                    if (!string.IsNullOrEmpty(DOCNO))
                    {
                        arr_docno = DOCNO.Trim().Split(','); //用,分割
                    }

                    if (arr_docno.Count() > 1)
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\A\AB0118_2.rdlc";
                    }

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });
                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_TWNToday = (DateTime.Now.Year - 1911).ToString() + "年" + DateTime.Now.Month + "月" + DateTime.Now.Day + "日";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TWNToday", str_TWNToday) });

                    //停用收集使用方式資料，加快報表顯示
                    ReportViewer1.EnableTelemetry = false;



                    //將IEnumerable轉DataTable
                    //也可將DataTable當作LocalReport的DataSources
                    DataTable dt = LinqQueryToDataTable<AB0118Report_MODEL>(repo.GetPrintData(arr_docno, User.Identity.Name, Order));

                    //判斷資料筆數，如果沒有大於0則將s_IsAnyData填入無資料(報表顯示用)
                    string s_IsAnyData = dt.Rows.Count > 0 ? " " : "無資料";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("IsAnyData", s_IsAnyData) });


                    string apptime = dt.Rows[0]["APPTIME_T"].ToString();
                    string apptime_str = apptime.Substring(0,3)+ "年" + apptime.Substring(3, 2) + "月" + apptime.Substring(5, 2) + "日"+apptime.Substring(7,2)+"時"+ apptime.Substring(9, 2)+"分";

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DOCNO", arr_docno) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APPTIME", apptime_str) }); 
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("FRWH_N", dt.Rows[0]["FRWH_N"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APPLY_NOTE", dt.Rows[0]["APPLY_NOTE"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("ISARMY_N", dt.Rows[0]["ISARMY_N"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APPDEPT_N", dt.Rows[0]["APPDEPT_N"].ToString().Trim()) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APPID", dt.Rows[0]["APPID"].ToString().Trim()) });
                    
                    //ReportViewer1.PageCountMode = PageCountMode.Actual;
                    //先清空報表的DataSet，再將讀到的AB0118 DataTable放到DataSources(對應到AB0118.xsd)
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0118", dt));

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