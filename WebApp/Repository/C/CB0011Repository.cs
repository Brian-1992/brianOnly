using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Dapper;
using JCLib.DB;
using WebApp.Models.C;
using BarcodeLib;
using System.Drawing.Imaging;
using System.Drawing;

namespace WebApp.Repository.C
{
    public class CB0011Repository : JCLib.Mvc.BaseRepository
    {
        public CB0011Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CB0011> GetBox(int start,int limit,string BOXNO, string BARCODE, string STATUS, string sort)
        {
            var p = new DynamicParameters();
            String sql = @"select BOXNO,BARCODE,XCATEGORY,DESCRIPT,STATUS from BC_BOX WHERE 1=1";
            if (!string.IsNullOrEmpty(BOXNO))
            {
                sql += " and BOXNO like :BOXNO";
                p.Add(":BOXNO", BOXNO + "%");
            }
            if (BARCODE != "BOX")
            {
                sql += " and BARCODE like :BARCODE";
                p.Add(":BARCODE", BARCODE + "%");
            }
            if (STATUS != "all")
            {
                sql += " and STATUS = :STATUS";
                p.Add(":STATUS", STATUS);
            }
            p.Add("OFFSET", start);
            p.Add("PAGE_SIZE", limit);
            return DBWork.Connection.Query<CB0011>(GetPagingStatement(sql, sort), p, DBWork.Transaction);
        }
        public IEnumerable<CB0011> Print(string BOXNO, string BARCODE, string STATUS)
        {
            var p = new DynamicParameters();
            String sql = @"select ROWNUM NO,BOXNO,BARCODE,XCATEGORY,DESCRIPT,STATUS from BC_BOX WHERE 1=1";
            if (!string.IsNullOrEmpty(BOXNO))
            {
                sql += " and BOXNO like :BOXNO";
                p.Add(":BOXNO", BOXNO + "%");
            }
            if (BARCODE != "BOX")
            {
                sql += " and BARCODE like :BARCODE";
                p.Add(":BARCODE", BARCODE + "%");
            }
            if (STATUS != "all")
            {
                sql += " and STATUS = :STATUS";
                p.Add(":STATUS", STATUS);
            }
            //return DBWork.Connection.Query<CB0011>(sql, p, DBWork.Transaction);
            Barcode b = new Barcode();

            bool isTypeError = false;

            List<CB0011> list = new List<CB0011>();
            foreach (CB0011 CB0011 in DBWork.Connection.Query<CB0011>(sql, p, DBWork.Transaction))
            {
                BarcodeLib.TYPE type = BarcodeLib.TYPE.UNSPECIFIED;
                switch (CB0011.XCATEGORY)
                {
                    case "UPCA": type = BarcodeLib.TYPE.UPCA; break;
                    case "UPCE": type = BarcodeLib.TYPE.UPCE; break;
                    case "UPC 2 Digit Ext.": type = BarcodeLib.TYPE.UPC_SUPPLEMENTAL_2DIGIT; break;
                    case "UPC 5 Digit Ext.": type = BarcodeLib.TYPE.UPC_SUPPLEMENTAL_5DIGIT; break;
                    case "EAN13": type = BarcodeLib.TYPE.EAN13; break;
                    case "JAN-13": type = BarcodeLib.TYPE.JAN13; break;
                    case "EAN8": type = BarcodeLib.TYPE.EAN8; break;
                    case "ITF-14": type = BarcodeLib.TYPE.ITF14; break;
                    case "Codabar": type = BarcodeLib.TYPE.Codabar; break;
                    case "PostNet": type = BarcodeLib.TYPE.PostNet; break;
                    case "Bookland/ISBN": type = BarcodeLib.TYPE.BOOKLAND; break;
                    case "Code 11": type = BarcodeLib.TYPE.CODE11; break;
                    case "CODE39": type = BarcodeLib.TYPE.CODE39; break;
                    case "Code 39 Extended": type = BarcodeLib.TYPE.CODE39Extended; break;
                    case "Code 39 Mod 43": type = BarcodeLib.TYPE.CODE39_Mod43; break;
                    case "CODE93": type = BarcodeLib.TYPE.CODE93; break;
                    case "LOGMARS": type = BarcodeLib.TYPE.LOGMARS; break;
                    case "MSI": type = BarcodeLib.TYPE.MSI_Mod10; break;
                    case "Interleaved 2 of 5": type = BarcodeLib.TYPE.Interleaved2of5; break;
                    case "Standard 2 of 5": type = BarcodeLib.TYPE.Standard2of5; break;
                    case "CODE128": type = BarcodeLib.TYPE.CODE128; break;
                    case "Code 128-A": type = BarcodeLib.TYPE.CODE128A; break;
                    case "Code 128-B": type = BarcodeLib.TYPE.CODE128B; break;
                    case "Code 128-C": type = BarcodeLib.TYPE.CODE128C; break;
                    case "Telepen": type = BarcodeLib.TYPE.TELEPEN; break;
                    case "FIM": type = BarcodeLib.TYPE.FIM; break;
                    case "Pharmacode": type = BarcodeLib.TYPE.PHARMACODE; break;
                    default: isTypeError = true; break;
                }

                if (!isTypeError)
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        try
                        {
                            Bitmap image = (Bitmap)b.Encode(type, CB0011.BARCODE, 150, 50);
                            image.Save(ms, ImageFormat.Bmp);
                            byte[] byteImage = new Byte[ms.Length];
                            byteImage = ms.ToArray();
                            string strB64 = Convert.ToBase64String(byteImage);
                            CB0011.BARCODE_IMAGE_STR = strB64;
                        }
                        catch (FormatException ex)
                        {
                            CB0011.BARCODE_IMAGE_STR = null;
                        }
                    }
                }
                else
                {
                    CB0011.BARCODE_IMAGE_STR = "";
                }
                list.Add(CB0011);
                isTypeError = false;
            }

            return list;
        }
        public int AddBox(CB0011 CB0011)
        {
            string sql = @"insert into BC_BOX(BOXNO,BARCODE,DESCRIPT,XCATEGORY,STATUS,CREATE_TIME,CREATE_USER,UPDATE_IP) VALUES(:BOXNO,:BARCODE,:DESCRIPT,:XCATEGORY,:STATUS,sysdate,:CREATE_USER,:UPDATE_IP)";
          return  DBWork.Connection.Execute(sql, CB0011, DBWork.Transaction);
        }
        public int DeleteBox(CB0011 CB0011)
        {
            string sql = @"delete from BC_BOX where BOXNO=:BOXNO";
            return DBWork.Connection.Execute(sql, CB0011, DBWork.Transaction);

        }
        public int UpdateBox(CB0011 CB0011)
        {
            string sql = @"update BC_BOX set BARCODE=:BARCODE,DESCRIPT=:DESCRIPT,XCATEGORY=:XCATEGORY,STATUS=:STATUS,UPDATE_TIME=SYSDATE,UPDATE_IP=:UPDATE_IP,UPDATE_USER=:UPDATE_USER where BOXNO=:BOXNO";
            return DBWork.Connection.Execute(sql, CB0011, DBWork.Transaction);
        }
        public IEnumerable<CB0011> GetXcategory()
        {
            String sql = @"select XCATEGORY,XCATEGORY || ' ' || DESCRIPT as DESCRIPT from bc_category";
            return DBWork.Connection.Query<CB0011>(sql, DBWork.Transaction);
        }
    }
    
}