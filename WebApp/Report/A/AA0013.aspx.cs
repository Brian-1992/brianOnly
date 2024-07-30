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
using System.Linq;

namespace WebApp.Report.A
{
    public partial class AA0013 : System.Web.UI.Page
    {
        /// <summary>
        /// 申請單位
        /// </summary>
        string P0;

        /// <summary>
        /// 物料分類
        /// </summary>
        string P1;

        /// <summary>
        /// 申請日期_起
        /// </summary>
        string P2;

        /// <summary>
        /// 申請日期_迄
        /// </summary>
        string P3;

        /// <summary>
        /// 申請單狀態
        /// </summary>
        string P4;

        /// <summary>
        /// 申請單號
        /// </summary>
        string P5;

        /// <summary>
        /// 核撥日期_起
        /// </summary>
        string P6;

        /// <summary>
        /// 核撥日期_迄
        /// </summary>
        string P7;

        /// <summary>
        /// 執行動作。0 = 查詢顯示  1 = 列印
        /// </summary>
        string Action;

        bool isGas;

        string Order;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //隱藏RDLC本身的列印功能鍵
                ReportViewer1.ShowPrintButton = false;
                ReportViewer1.ShowExportControls = false;
                P0 = (Request.QueryString["p0"] is null) ? "" : Request.QueryString["p0"].ToString().Replace("null", "");
                P1 = (Request.QueryString["p1"] is null) ? "" : Request.QueryString["p1"].ToString().Replace("null", "");
                P2 = (Request.QueryString["p2"] is null) ? "" : Request.QueryString["p2"].ToString().Replace("null", "");
                P3 = (Request.QueryString["p3"] is null) ? "" : Request.QueryString["p3"].ToString().Replace("null", "");
                P4 = (Request.QueryString["p4"] is null) ? "" : Request.QueryString["p4"].ToString().Replace("null", "");
                P5 = (Request.QueryString["p5"] is null) ? "" : Request.QueryString["p5"].ToString().Replace("null", "");
                P6 = (Request.QueryString["p6"] is null) ? "" : Request.QueryString["p6"].ToString().Replace("null", "");
                P7 = (Request.QueryString["p7"] is null) ? "" : Request.QueryString["p7"].ToString().Replace("null", "");
                isGas = (Request.QueryString["isGas"] is null) ? false : Request.QueryString["isGas"].ToString() == "Y";
                Action = (Request.QueryString["Action"] is null) ? "0" : Request.QueryString["Action"].ToString();
                Order = (Request.QueryString["Order"] is null) ? "0" : Request.QueryString["Order"].ToString();
                if (Action == "3" || Action == "4")
                {
                    ReportViewer1.ShowPrintButton = true;
                    ReportViewer1.ShowExportControls = true;
                }
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
                    AA0013Repository repo = new AA0013Repository(DBWork);

                    //停用收集使用方式資料，加快報表顯示
                    ReportViewer1.EnableTelemetry = false;

                    string TASK_ID = repo.GetTASK_ID(User.Identity.Name);
                    List<string> taskids = repo.GetTaskids(User.Identity.Name).ToList<string>();
                    bool IsUserWHNO_Equal_WHNOMM1 = repo.GetIsUserWHNO_Equal_WHNOMM1(User.Identity.Name);
                    string[] arr_p0 = { };
                    string[] arr_p1 = { };
                    string[] arr_p4 = { };
                    string[] arr_p5 = { };
                    if (!string.IsNullOrEmpty(P0))
                    {
                        arr_p0 = P0.Trim().Split(','); //用,分割
                    }
                    if (!string.IsNullOrEmpty(P1))
                    {
                        arr_p1 = P1.Trim().Split(','); //用,分割
                    }
                    if (!string.IsNullOrEmpty(P4))
                    {
                        arr_p4 = P4.Trim().Split(','); //用,分割
                    }
                    if (!string.IsNullOrEmpty(P5))
                    {
                        arr_p5 = P5.Trim().Split(','); //用,分割
                    }

                    //將IEnumerable轉DataTable
                    //也可將DataTable當作LocalReport的DataSources
                    DataTable tmpdt_AA0013 = LinqQueryToDataTable<AA0013Report_MODEL>(repo.GetPrintData(isGas, arr_p0, arr_p1, P2, P3, arr_p4, arr_p5, P6, P7, User.Identity.Name, Order));

                    //判斷資料筆數，如果沒有大於0則將s_IsAnyData填入無資料(報表顯示用)
                    string s_IsAnyData = tmpdt_AA0013.Rows.Count > 0 ? " " : "無資料";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("IsAnyData", s_IsAnyData) });

                    //判斷資料筆數，如果大於0才計算核撥量總合
                    //計算APVQTY總合(APVQTY總和為0改為計算EXPT_DISTQTY總合)，
                    //直接以固定參數的方式寫到RDLC
                    //20191001 暫時先不顯示
                    string s_TOTAL_APVQTY = "0";
                    if (tmpdt_AA0013.Rows.Count > 0)
                    {
                        s_TOTAL_APVQTY = tmpdt_AA0013.Compute("Sum(APVQTY)", string.Empty).ToString();
                        //if (s_TOTAL_APVQTY == "0")
                        //{
                        //    tmpdt_AA0013.Compute("Sum(EXPT_DISTQTY)", string.Empty).ToString();
                        //}
                    }
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TOTAL_APVQTY", s_TOTAL_APVQTY) });

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    //string str_title = "衛材";
                    //if (Action == "3" )
                    //{
                    //    str_title = "一般物品";
                    //}
                    //ReportViewer1.PageCountMode = PageCountMode.Actual;
                    //ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Title", str_title) });
                    //先清空報表的DataSet，再將讀到的AA0013 DataTable放到DataSources(對應到AA0013.xsd)
                    ReportViewer1.LocalReport.DataSources.Clear();
                    //ReportViewer1.LocalReport.DisplayName = parDN;
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AA0013", tmpdt_AA0013));

                    ReportViewer1.LocalReport.Refresh();

                    //如果按列印
                    if (Action == "1")
                    {
                        //用Dictionary傳遞ReportParameter，丟到GetPDF.aspx再作解析
                        Dictionary<string, string> Dict1 = new Dictionary<string, string>();
                        Dict1.Add("IsAnyData", s_IsAnyData);
                        Dict1.Add("TOTAL_APVQTY", s_TOTAL_APVQTY);
                        Dict1.Add("PrintTime", str_PrintTime);
                        //Dict1.Add("Title", str_title);

                        Page.Session["RDLC_Path"] = "AA0013.rdlc";
                        Page.Session["DataTableName"] = "AA0013";
                        Page.Session["Dict1"] = Dict1;
                        Page.Session["DTTable"] = tmpdt_AA0013;

                        //送出直接列印
                        iframe_PrintReport.Attributes["src"] = "GetPDF.aspx";
                        Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "PrintJs", "PrintPdf()", true);

                        //依據列印單號更新LIST_ID為Y
                        //將每一筆申請單號先用逗號串起來，再切割為陣列當參數丟入Update_ME_DOCM_ListID_To_Y
                        string tmp_DOCNO = "";
                        string[] Str_DOCNO = { };
                        foreach (DataRow tmp_dr in tmpdt_AA0013.Rows)
                        {
                            if (string.IsNullOrEmpty(tmp_DOCNO))
                            {
                                tmp_DOCNO += tmp_dr["DOCNO"].ToString();
                            }
                            else
                            {
                                tmp_DOCNO += "," + tmp_dr["DOCNO"].ToString();
                            }
                        }
                        Str_DOCNO = tmp_DOCNO.Split(',');
                        repo.Update_ME_DOCM_ListID_To_Y(Str_DOCNO);
                    }
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