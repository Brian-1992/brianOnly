using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Data;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.Web;
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0072Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetMatClassCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0072Repository repo = new AA0072Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH(FormDataCollection form)
        {
            var v_tpe = "N";
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0072Repository repo = new AA0072Repository(DBWork);
                    var v_inid = repo.GetUridInid(User.Identity.Name);
                    if (repo.Checkwhno(v_inid))
                    {
                        v_tpe = "Y";
                    }
                    session.Result.etts = repo.GetWH(v_tpe).Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢
        [HttpPost]
        public ApiResponse GetQueryData(FormDataCollection form)
        {
            var matClass = form.Get("matClass");
            var fromYM = form.Get("dataYMFrom");
            var toYM = form.Get("dataYMTo");
            var whNo = form.Get("whNo");
            var mmCode = "";
            var docType = form.Get("docType");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0072Repository(DBWork);
                    session.Result.etts = repo.GetQueryData("js", matClass, fromYM, toYM, whNo, mmCode, docType, page, limit, sorters, "AA0072"); //撈出object
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
            string matClass = form.Get("matClass");
            string fromYM = form.Get("dataYMFrom");
            string toYM = form.Get("dataYMTo");
            string whNo = form.Get("whNo");
            string mmCode = form.Get("mmCode");
            string docType = form.Get("docType");
            string printTitle = form.Get("printTitle");
            string frompgm = form.Get("frompgm");
            string excelName = form.Get("FN") + "_" + (Convert.ToInt32(DateTime.Now.ToString("yyyy")) - 1911).ToString() + DateTime.Now.ToString("MMddhhmm") + ".xlsx";

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0072Repository repo = new AA0072Repository(DBWork);

                    DataTable data = repo.GetExcelData("xls", matClass, fromYM, toYM, whNo, mmCode, docType, printTitle, DBWork.UserInfo.UserId, frompgm);

                    if (data.Rows.Count > 0)
                    {
                        string header = repo.getInidName(DBWork.UserInfo.UserId) + " " + printTitle + "單位申領明細報表";
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
                                    "attachment; filename=" + excelName);
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