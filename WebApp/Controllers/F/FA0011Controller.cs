using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
using NPOI.XSSF.UserModel;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;
using System.Web;

namespace WebApp.Controllers.F
{
    public class FA0011Controller : SiteBase.BaseApiController
    {
        //查詢,匯出,列印 參考AA0061

        // FA0011 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var showopt = form.Get("showopt");
            var showdata = form.Get("showdata");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0011Repository(DBWork);
                    session.Result.etts = repo.GetAll(User.Identity.Name, d0, d1, p0, p1, showopt, showdata, page, limit, sorters); //撈出object
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0011Repository repo = new FA0011Repository(DBWork);
                    session.Result.etts = repo.GetMatClass();

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
            var p0 = form.Get("p0"); //動態mmcode
            //var mat_class = form.Get("mat_class");
            //var store_id = form.Get("store_id");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0011Repository repo = new FA0011Repository(DBWork);
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
        public ApiResponse Excel(FormDataCollection form)
        {
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var showopt = form.Get("p2");
            var showdata = form.Get("p3");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0011Repository repo = new FA0011Repository(DBWork);

                    DataTable data = repo.GetExcel(User.Identity.Name, d0, d1, p0, p1, showopt, showdata);
                    string fileName = form.Get("FN") + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xlsx";
                    string header = string.Format(repo.GetExcelTitle(User.Identity.Name, d0, d1, showdata));
                    if (data.Rows.Count > 0)
                    {
                        var workbook = ExoprtToExcel(data, header);

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
                                    "attachment; filename=" + fileName);
                        res.BinaryWrite(ms.ToArray());

                        ms.Close();
                        ms.Dispose();
                    }

                    using (var dataTable1 = repo.GetExcel(User.Identity.Name, d0, d1, p0, p1, showopt, showdata))
                    {
                        JCLib.Excel.Export(form.Get("FN") + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", dataTable1,
                            (dt) => {
                                //headerHandler: 返回Excel表首要顯示的文字
                                return string.Format(repo.GetExcelTitle(User.Identity.Name, d0, d1, showdata));
                            });
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public XSSFWorkbook ExoprtToExcel(DataTable data, string header)
        {
            var wb = new XSSFWorkbook();
            var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");

            IRow row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue(header);

            row = sheet.CreateRow(1);
            for (int i = 0; i < data.Columns.Count; i++)
            {
                row.CreateCell(i).SetCellValue(data.Columns[i].ToString());
            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                row = sheet.CreateRow(2 + i);
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(data.Rows[i].ItemArray[j].ToString());
                }
            }
            return wb;
        }
    }
}