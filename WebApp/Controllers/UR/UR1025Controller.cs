using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;
using NPOI.XSSF.UserModel;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;
using System.Web;

namespace WebApp.Controllers.UR
{
    public class UR1025Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 帳號
            var p1 = form.Get("p1"); // 姓名
            var p2 = form.Get("p2"); // 程式編號
            var p3 = form.Get("p3"); // 群組代碼
            var p4 = form.Get("p4"); // 查詢
            var p5 = form.Get("p5"); // 維護
            var p6 = form.Get("p6"); // 列印
            var p7 = form.Get("p7"); // 責任中心代碼
            var p8 = form.Get("p8"); // AD帳號

            if (p4 == "true")
                p4 = "1";
            else
                p4 = null;
            if (p5 == "true")
                p5 = "1";
            else
                p5 = null;
            if (p6 == "true")
                p6 = "1";
            else
                p6 = null;

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_TACLRepository(DBWork);
                    session.Result.etts = repo.GetTaclAll(p0, p1, p2, p3, p4, p5, p6, p7, p8);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UserDetail(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 帳號
            
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_UIRRepository(DBWork);
                    session.Result.etts = repo.GetUserDetail(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse FgDetail(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 程式代碼

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_UIRRepository(DBWork);
                    session.Result.etts = repo.GetFgDetail(p0);
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
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");

            if (p4 == "true")
                p4 = "1";
            else
                p4 = null;
            if (p5 == "true")
                p5 = "1";
            else
                p5 = null;
            if (p6 == "true")
                p6 = "1";
            else
                p6 = null;

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_TACLRepository repo = new UR_TACLRepository(DBWork);
                    DataTable data = repo.GetTaclAllExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8);
                    if (data.Rows.Count > 0)
                    {
                        var workbook = ExoprtToExcel(data);

                        //output
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            workbook.Write(memoryStream);
                            JCLib.Export.OutputFile(memoryStream, form.Get("FN"));
                            workbook.Close();
                        }
                    }

                    //JCLib.Excel.Export(form.Get("FN"), repo.GetTaclAllExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8));
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