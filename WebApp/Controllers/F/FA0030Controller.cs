using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using NPOI.HSSF.UserModel;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;
using System.Web;

namespace WebApp.Controllers.F
{
    public class FA0030Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0030Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //取得使用者單位
        public string GetDept()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0030Repository(DBWork);
                    var Dept = repo.GetDept(User.Identity.Name);
                    return Dept;
                }
                catch
                {
                    throw;
                }
            }
        }

        //匯出EXCEL
        [HttpPost]
        public void Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0030Repository repo = new FA0030Repository(DBWork);
                    var fileName = form.Get("FN");
                    var data = repo.GetExcel(form.Get("p0"));
                    if (data.Rows.Count > 0)
                    {
                        var workbook = ExoprtToExcel(data, p0);

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
                }
                catch
                {
                    throw;
                }
            }
        }

        //製造EXCEL的內容
        public HSSFWorkbook ExoprtToExcel(DataTable data, string data_ym)
        {
            var wb = new HSSFWorkbook();
            var sheet = (HSSFSheet)wb.CreateSheet("Sheet1");

            IRow row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue(GetDept() + data_ym + "總材積明細");

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