using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AB;
using JCLib.DB;
using WebApp.Models;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;
using WebApp.Repository.AA;

namespace WebApp.Report.A
{
    public partial class AB0012 : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                report1Bind();
                //this.ReportViewer1.PageCountMode = PageCountMode.Actual;
            }
        }

        protected void report1Bind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    AB0012Repository.ME_DOCM_QUERY_PARAMS query = new AB0012Repository.ME_DOCM_QUERY_PARAMS();
                    AA0015Repository repo_rdlc = new AA0015Repository(DBWork);
                    string hospName = repo_rdlc.GetHospName();
                    string hospFullName = repo_rdlc.GetHospFullName();
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospName", hospName) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("HospFullName", hospFullName) });

                    ReportViewer1.EnableTelemetry = false;
                    query.DOCNO = Request.QueryString["docno"].ToString().Replace("null", "");

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Towh", repo.GetThisTowh(query.DOCNO)) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Frwh", repo.GetThisFrwh(query.DOCNO)) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Docno", query.DOCNO) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Apptime", repo.GetThisApptime(query.DOCNO)) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("User", DBWork.UserInfo.UserId + " " + DBWork.UserInfo.UserName) });

                    //================================產生BarCode的資料=======================================
                    Barcode tmp_BarCode = new Barcode();
                    TYPE type = TYPE.CODE128;

                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        try
                        {
                            Bitmap img_BarCode = (Bitmap)tmp_BarCode.Encode(type, query.DOCNO, 550, 45);

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
                    ReportViewer1.LocalReport.DisplayName = "藥品請領單";
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.GetPrintData(query)));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }


        }
    }
}