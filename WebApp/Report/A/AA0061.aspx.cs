using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;
using System.Reflection;

namespace WebApp.Report.A
{
    public partial class AA0061 : Page
    {
        string userID, APPTIME1, APPTIME2, task_id, mmcode, showopt, showdata,type;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                userID = User.Identity.Name;
                APPTIME1 = Request.QueryString["APPTIME1"].ToString().Replace("null", "");
                APPTIME2 = Request.QueryString["APPTIME2"].ToString().Replace("null", "");
                task_id = Request.QueryString["task_id"].ToString().Replace("null", "");
                mmcode = Request.QueryString["mmcode"].ToString().Replace("null", "");
                showopt = Request.QueryString["showopt"].ToString().Replace("null", "");
                showdata = Request.QueryString["showdata"].ToString().Replace("null", "");
                type = Request.QueryString["type"].ToString().Replace("null", ""); //分辨是AA0061 or AB0056


                report1Bind();
                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
            }
        }
        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0061Repository repo = new AA0061Repository(DBWork);
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });

                    string tmpTIME1 = "", tmpTIME2 = "";
                    int Intyear1 = 0, Intyear2 = 0;
                    if (APPTIME1 != "" && APPTIME1 != null)
                    {
                        tmpTIME1 = Convert.ToDateTime(APPTIME1.Substring(4, 11)).ToString("yyyyMM");
                        Intyear1 = Convert.ToInt32(tmpTIME1.Substring(0, 4)) - 1911;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APPTIME1", Convert.ToString(Intyear1) + tmpTIME1.Substring(4, 2)) });
                    }

                    if (APPTIME2 != "" && APPTIME2 != null)
                    {
                        tmpTIME2 = Convert.ToDateTime(APPTIME2.Substring(4, 11)).ToString("yyyyMM");
                        Intyear2 = Convert.ToInt32(tmpTIME2.Substring(0, 4)) - 1911;
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("APPTIME2", Convert.ToString(Intyear2) + tmpTIME2.Substring(4, 2)) });
                    }



                    IEnumerable<string> depts = repo.GetUserWhnos(User.Identity.Name);
                    string deptString = GetDeptStrings(depts);


                    if (showopt == "0") //非庫備品
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DATA_DESC", "非庫備品") });
                    }
                    else if (showopt == "1")  //庫備品
                    {
                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("DATA_DESC", "庫備品") });
                    }


                    ReportViewer1.LocalReport.DataSources.Clear();

                    DataTable tmpdt_AA0061 = new DataTable();
                    if (type == "AA0061")
                    {
                        tmpdt_AA0061 = LinqQueryToDataTable<AA0061_MODEL>(repo.GetPrintData(userID, APPTIME1, APPTIME2, task_id, mmcode, showopt, showdata));
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", tmpdt_AA0061));
                    }
                    else if(type == "AB0056")
                    {
                        tmpdt_AA0061 = LinqQueryToDataTable<AA0061_MODEL>(repo.GetPrintDataByDept(userID, APPTIME1, APPTIME2, task_id, mmcode, showopt, showdata, deptString));
                        ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", tmpdt_AA0061));
                    }

                    //判斷資料筆數，如果沒有大於0則將s_IsAnyData填入無資料(報表顯示用)
                    string s_IsAnyData = tmpdt_AA0061.Rows.Count > 0 ? " " : "無資料";
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("IsAnyData", s_IsAnyData) });
                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }

            
        }

        public string GetDeptStrings(IEnumerable<string> depts)
        {
            string result = string.Empty;
            foreach (string dept in depts)
            {
                if (result != string.Empty)
                {
                    result += ", ";
                }
                result += string.Format("'{0}'", dept);
            }
            return result;
        }


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