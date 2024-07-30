using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.AA;
using JCLib.DB;
using WebApp.Models;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebApp.Report.A
{
    public partial class AA0015_NON : SiteBase.BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //report1Bind();
                //AA0015,AB0010 USED
                reportmBind();
                this.ReportViewer1.PageCountMode = PageCountMode.Actual;
            }
        }

        protected void report1Bind() // NOT USE
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    AA0015Repository.ME_DOCM_QUERY_PARAMS query = new AA0015Repository.ME_DOCM_QUERY_PARAMS();

                    query.DOCNO = Request.QueryString["docno"].ToString().Replace("null", "");
                    string frwh = repo.GetThisFrwh(query.DOCNO);
                    query.FRWH = frwh;

                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Towh", repo.GetThisTowh(query.DOCNO)) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Frwh", frwh) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Apptime", repo.GetThisApptime(query.DOCNO)) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("Docno", query.DOCNO) });
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



                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.GetPrintData_NON(query)));

                    ReportViewer1.LocalReport.Refresh();
                }
                catch
                {
                    throw;
                }
                //return session.Result;
            }
        }

        protected void reportmBind()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0015Repository repo = new AA0015Repository(DBWork);
                    AA0015Repository.ME_DOCM_QUERY_PARAMS query = new AA0015Repository.ME_DOCM_QUERY_PARAMS();

                    //query.DOCNO = Request.QueryString["docno"].ToString().Replace("null", "");
                    //string frwh = repo.GetThisFrwh(query.DOCNO);
                    //query.FRWH = frwh;
                    string fr = Request.QueryString["fr"].ToString().Replace("null", ""); //AA0015, AB0010
                    query.DOCNO_S = Request.QueryString["p0"].ToString().Replace("null", "");
                    query.DOCNO_E = Request.QueryString["p1"].ToString().Replace("null", "");
                    query.APPTIME_S = Request.QueryString["p2"].ToString().Replace("null", "");
                    query.APPTIME_E = Request.QueryString["p3"].ToString().Replace("null", "");
                    query.WH_NO = Request.QueryString["p4"].ToString().Replace("null", "");
                    query.SORT = Request.QueryString["p5"].ToString().Replace("null", "");
                    if (Request.QueryString["p6"] != null)
                        query.FRWH = Request.QueryString["p6"].ToString().Replace("null", "");

                    if (fr == "AB0010")
                    {
                        query.FLOWID = "0102";
                        //query.APPID = DBWork.UserInfo.UserId;
                        query.APPID = "";
                        query.APPDEPT = DBWork.UserInfo.Inid;
                    }
                    else
                    {
                        query.FLOWID = "";
                        query.APPID = "";
                        query.APPDEPT = "";
                    }
                    //產生當下列印時間(民國格式)，將時間寫到報表中顯示(固定)
                    string str_PrintTime = (DateTime.Now.Year - 1911).ToString() + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + " " + DateTime.Now.ToString("HH:mm:ss");
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("PrintTime", str_PrintTime) });
                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { new ReportParameter("User", DBWork.UserInfo.UserId + " " + DBWork.UserInfo.UserName) });
                    

                    ReportViewer1.EnableTelemetry = false;
                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", repo.GetPrintDataM_NON(query)));

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