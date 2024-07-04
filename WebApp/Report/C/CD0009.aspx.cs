using System;
using System.Web.UI;
using System.Data;
using Microsoft.Reporting.WebForms;
using WebApp.Repository.C;
using WebApp.Models;
using JCLib.DB;
using System.Collections.Generic;
using System.Drawing;
using BarcodeLib;
using System.Drawing.Imaging;

namespace WebApp.Report.C
{
    public partial class CD0009 : Page
    {
        string wh_no;
        string pick_date;
        string lot_no;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                wh_no = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                pick_date = Request.QueryString["PICK_DATE"].ToString().Replace("null", "");
                lot_no = Request.QueryString["LOT_NO"].ToString().Replace("null", "0");
                
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
                    CD0009Repository repo = new CD0009Repository(DBWork);
                    ReportViewer1.EnableTelemetry = false;
                    IEnumerable<Models.CD0009> list = repo.Print(wh_no, pick_date, int.Parse(lot_no));

                    Barcode b = new Barcode();
                    b.IncludeLabel = true;

                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        foreach (Models.CD0009 item in list)
                        {
                            try
                            {

                                BarcodeLib.TYPE type = BarcodeLib.TYPE.UNSPECIFIED;
                                type = BarcodeLib.TYPE.CODE128;

                                Bitmap image = (Bitmap)b.Encode(type, item.DOCNO);


                                image.Save(ms, ImageFormat.Bmp);
                                byte[] byteImage = new Byte[ms.Length];
                                byteImage = ms.ToArray();
                                string strB64 = Convert.ToBase64String(byteImage);
                                item.Barcode = strB64;

                            }
                            catch (Exception ex)
                            {
                                item.Barcode = "";
                            }
                        }
                    }
 
                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CD0009", list));

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