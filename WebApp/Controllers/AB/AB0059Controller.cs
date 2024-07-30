using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.Web;
using System;

namespace WebApp.Controllers.AB
{
    public class AB0059Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1");    //成立年月  SET_YM
            var p3 = form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL
            var p5 = form.Get("p5");    //庫房別
            bool notZeroOnly = form.Get("p6") == "true";
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p4 == true)
                    {
                        var repo = new AB0059Repository(DBWork);
                        string _p0 = p0.Substring(0, p0.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        session.Result.etts = repo.GetAll(p0, p1, p3, p4, p5, notZeroOnly, page, limit, sorters);
                    }
                    else
                    {
                        var repo = new AB0059Repository(DBWork);
                        session.Result.etts = repo.GetAll(p0, p1, p3, p4, p5, notZeroOnly, page, limit, sorters);
                    }
                }
                catch(Exception e)
                {
                    throw;
                }

                return session.Result;
            }
        }

        public ApiResponse Print(FormDataCollection form)
        {
            var p0 = form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1");    //成立年月  SET_YM
            var p3 = form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL
            var p5 = form.Get("p5");    //庫房別
            bool notZeroOnly = form.Get("p6") == "true";
            
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p4 == true)
                    {
                        var repo = new AB0059Repository(DBWork);
                        string _p0 = p0.Substring(0, p0.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        session.Result.etts = repo.Print(p0, p1, p3, p4, p5, notZeroOnly);
                    }
                    else
                    {
                        var repo = new AB0059Repository(DBWork);
                        session.Result.etts = repo.Print(p0, p1, p3, p4, p5, notZeroOnly);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1");    //成立年月  SET_YM
            var p3 = form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL
            var p5 = form.Get("p5");    //庫房別
            bool notZeroOnly = form.Get("p6") == "true";

            DataTable data;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0059Repository(DBWork);
                    if (p4 == true)
                    {
                        string _p0 = p0.Substring(0, p0.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                    }
                    //JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p3, p4, p5));
                    data = repo.GetExcel(p0, p1, p3, p4, p5, notZeroOnly);
                    if (data.Rows.Count > 0)
                    {
                        var workbook = ExoprtToExcel(data);

                        //output
                        var ms = new MemoryStream();
                        workbook.Write(ms);
                        var res = HttpContext.Current.Response;
                        res.BufferOutput = false;

                        res.Clear();
                        res.ClearHeaders();
                        res.HeaderEncoding = System.Text.Encoding.Default;
                        res.ContentType = "application/octet-stream";
                        res.AddHeader("Content-Disposition",
                                    "attachment; filename=" + form.Get("FN"));
                        res.BinaryWrite(ms.ToArray());

                        ms.Close();
                        ms.Dispose();
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //製造EXCEL的內容
        public XSSFWorkbook ExoprtToExcel(DataTable data)
        {
            var wb = new XSSFWorkbook();
            var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");

            IRow row = sheet.CreateRow(0);
            for (int i = 0; i < data.Columns.Count; i++)
            {
                row.CreateCell(i).SetCellValue(data.Columns[i].ToString());
            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                row = sheet.CreateRow(1 + i);
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(data.Rows[i].ItemArray[j].ToString());
                }
            }
            return wb;
        }

        public ApiResponse GetMatCombo(string wh_no)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0059Repository(DBWork);
                    var wh_kind = repo.GetWH_KIND(wh_no);
                    session.Result.etts = repo.GetMatCombo(wh_kind);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetYMCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0059Repository(DBWork);
                    session.Result.etts = repo.GetYMCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //public ApiResponse getWH_NO()
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new AB0059Repository(DBWork);
        //            session.Result.etts = repo.getWH_NO();
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var p0 = form.Get("MAT_CLASS");
                    var p1 = form.Get("MMCODE");
                    var clsALL = bool.Parse(form.Get("clsALL"));
                    //var wh_no = form.Get("WH_NO");

                    if (clsALL == true)
                    {
                        var repo = new AB0059Repository(DBWork);
                        string _p0 = p0.Substring(0, p0.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        session.Result.etts = repo.GetMMCodeCombo(p0, p1, clsALL, page, limit, sorters);
                    }
                    else
                    {
                        var repo = new AB0059Repository(DBWork);
                        session.Result.etts = repo.GetMMCodeCombo(p0, p1, clsALL, page, limit, sorters);
                    }

                    //AB0059Repository repo = new AB0059Repository(DBWork);
                    //session.Result.etts = repo.GetMMCodeCombo(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetWHCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0059Repository(DBWork);
                    session.Result.etts = repo.GetWHCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
    }
}
