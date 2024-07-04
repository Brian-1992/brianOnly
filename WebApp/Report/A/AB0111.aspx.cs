using System;
using WebApp.Repository.AB;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using JCLib.DB;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.Reflection;
using WebApp.Models;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebApp.Report.A
{
    public partial class AB0111 : SiteBase.BasePage
    {
        string crdocno;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                crdocno = (Request.QueryString["crdocno"] is null) ? "" : Request.QueryString["crdocno"].ToString().Replace("null", "");
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
                    var repo = new AB0111Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;

                    IEnumerable<Models.AB.AB0111> data = repo.GetPrint(crdocno);

                    Barcode tmp_BarCode = new Barcode();
                    TYPE type = TYPE.CODE128;

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("TODAY", DateTime.Now.ToString("yyyy-MM-dd")) });

                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        try
                        {
                            Bitmap img_BarCode = (Bitmap)tmp_BarCode.Encode(type, crdocno, 550, 250);

                            img_BarCode.Save(ms, ImageFormat.Jpeg);
                            byte[] byteImage = new Byte[ms.Length];
                            byteImage = ms.ToArray();
                            string strB64 = Convert.ToBase64String(byteImage);
                            ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Barcode", strB64) });
                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("AB0111", data));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch(Exception e)
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