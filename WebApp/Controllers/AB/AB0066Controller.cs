using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json;

using System.Data;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using System.Web;
using System.Text;

namespace WebApp.Controllers.AB
{
    public class AB0066Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = "";
            var p1 = "";
            var p2 = "";
            var p3 = "";
            var p4 = "";
            var p5 = "";
            var p6 = "";

            p0 = form.Get("p0");    //查詢月份(起)
            p1 = form.Get("p1");    //查詢月份(迄)
            p2 = form.Get("p2");    //院內碼
            p3 = form.Get("p3");    //庫別代碼
            p4 = form.Get("p4");    //庫存類別歸屬
            p5 = form.Get("p5");    //管制用藥代碼
            p6 = form.Get("p6");    //停用碼

            string[] arr_p4 = { };
            if (!string.IsNullOrEmpty(p4))
            {
                arr_p4 = p4.Trim().Split(','); //用,分割
            }
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0066Repository repo = new AB0066Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, arr_p4, p5, p6);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var limit = int.Parse(form.Get("limit"));

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0066Repository repo = new AB0066Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse WH_NOCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0066Repository repo = new AB0066Repository(DBWork);
                    session.Result.etts = repo.GetWH_NOCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse WH_GRADECombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0066Repository repo = new AB0066Repository(DBWork);
                    session.Result.etts = repo.GetWH_GRADECombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse E_RestrictCodeCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0066Repository repo = new AB0066Repository(DBWork);
                    session.Result.etts = repo.GetE_RestrictCodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form) {

            var p0 = "";
            var p1 = "";
            var p2 = "";
            var p3 = "";
            var p4 = "";
            var p5 = "";
            var p6 = "";

            p0 = form.Get("p0");    //查詢月份(起)
            p1 = form.Get("p1");    //查詢月份(迄)
            p2 = form.Get("p2");    //院內碼
            p3 = form.Get("p3");    //庫別代碼
            p4 = form.Get("p4");    //庫存類別歸屬
            p5 = form.Get("p5");    //管制用藥代碼
            p6 = form.Get("p6");    //停用碼
            string fn = form.Get("fn");

            string[] arr_p4 = { };
            if (!string.IsNullOrEmpty(p4))
            {
                arr_p4 = p4.Trim().Split(','); //用,分割
            }

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0066Repository(DBWork);

                    DataTable data = null;

                    data = repo.GetExcel(p0, p1, p2, p3, arr_p4, p5, p6);

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
                                    "attachment; filename=" + fn);
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
    }
}
