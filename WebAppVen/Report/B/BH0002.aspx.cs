using System;
using WebAppVen.Repository.B;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using JCLib.DB;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.Reflection;
using WebAppVen.Models;

namespace WebAppVen.Report.B
{
    public partial class BH0002 : SiteBase.BasePage
    {
        string P1;
        string P2;
        string P3;
        string TSGH;
        string AGEN_NO;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                P1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = (Request.QueryString["p2"] is null) ? "" : Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = (Request.QueryString["p3"] is null) ? "" : Request.QueryString["p3"].ToString().Replace("null", "");
                TSGH = (Request.QueryString["TSGH"] is null) ? "" : Request.QueryString["TSGH"].ToString().Replace("null", "");
                AGEN_NO = (Request.QueryString["TSGH"] is null) ? "" : Request.QueryString["AGEN_NO"].ToString().Replace("null", "");
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
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    bool IsPO_OverLenth = P1.Length > 11 ? true : false;

                    //將IEnumerable轉DataTable
                    //也可將DataTable當作LocalReport的DataSources
                    //
                    var P0 = DBWork.UserInfo.UserId;
                    if (TSGH == "Y")
                        P0 = AGEN_NO;

                    //
                    DataTable tmpdt_BH0002 = LinqQueryToDataTable<WB_REPLY>(repo.SearchReportData(P1, P2, P0, IsPO_OverLenth));

                    //報表路徑設定
                    if (P3 == "true")
                    {
                        ReportViewer1.LocalReport.ReportPath = @"Report\B\BH0002_OverLenth.rdlc";
                    }
                    else
                    {
                        ReportViewer1.LocalReport.ReportPath = tmpdt_BH0002.Rows.Count > 7 ? @"Report\B\BH0002_OverLenth.rdlc" : @"Report\B\BH0002.rdlc";
                    }

                    string sUserName = DBWork.UserInfo.UserName;
                    if (TSGH == "Y")
                        sUserName = AGEN_NO;
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("UserName", sUserName) });

                    //大寫金額
                    Int32 iTotal_Price = Convert.ToInt32(tmpdt_BH0002.Compute("SUM(tot)", string.Empty));
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("CHT_Total_Price", JCLib.ChtNumber.Transform(iTotal_Price)) });

                    //購案及契約編號
                    string sPO_CON_NO = "";
                    if (IsPO_OverLenth)
                    {
                        sPO_CON_NO = P1;
                    }
                    else
                    {
                        sPO_CON_NO = P1 + repo.GetPO_CON_NO(P1);
                    }
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PO_CON_NO", sPO_CON_NO) });

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    //string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    //string str_AGEN_NO = DBWork.UserInfo.UserId;
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_NO", str_AGEN_NO) });

                    //string str_AGEN_NAME = DBWork.UserInfo.UserName;
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("AGEN_NAME", str_AGEN_NAME) });

                    ReportViewer1.PageCountMode = PageCountMode.Actual;

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("BH0002", tmpdt_BH0002));

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