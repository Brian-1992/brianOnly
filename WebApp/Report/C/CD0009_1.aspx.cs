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
    public partial class CD0009_1 : Page
    {
        string wh_no;
        string pick_date;
        // string docnosString = string.Empty;
        string inputDocnosString;
        string[] docnos = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                wh_no = Request.QueryString["WH_NO"].ToString().Replace("null", "");
                pick_date = Request.QueryString["PICK_DATE"].ToString().Replace("null", "");
                inputDocnosString = Request.QueryString["DOCNOS"].ToString().Replace("null", "0");

                //string[] temps = inputDocnosString.Split('|');
                //foreach (string temp in temps)
                //{
                //    if (docnosString != string.Empty)
                //    {
                //        docnosString += ",";
                //    }
                //    docnosString += string.Format("'{0}'", temp);
                //}

                docnos = inputDocnosString.Split('|');

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
                    IEnumerable<Models.CD0009> list = repo.PrintNoUser(wh_no, pick_date, docnos);

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

                    ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("CD0009_1", list));

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