using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WebApp.Repository.F;
using System.IO;
using System.Web;
using System.Data;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace WebApp.Controllers.F
{
    public class FA0069Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1");    //成立年月  SET_YM
            var p2 = form.Get("p2");    //單位類別  INID_FLAG
            var p3 = form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL
            var p5 = form.Get("p5") == null ? string.Empty : form.Get("p5").Trim();    //單位科別庫房代碼 
            var p6 = form.Get("p6") == null ? string.Empty : form.Get("p6");    //庫存量<0
            var p7 = form.Get("p7") == null ? string.Empty : form.Get("p7");    //MMCODE
            var p8 = form.Get("p8") == null ? string.Empty : form.Get("p8");    //消耗量<0
            var p9 = form.Get("p9") == null ? string.Empty : form.Get("p9");    //期初<0
            var p10 = form.Get("p10") == null ? string.Empty : form.Get("p10");    //院內碼是否作廢
            var p11 = form.Get("p11") == null ? string.Empty : form.Get("p11");    //是否寄售
            var p12 = form.Get("p12") == null ? string.Empty : form.Get("p12");    //庫房是否作廢

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p2 != "4")  //只有是否庫備Radio選擇單位科別才會抓取庫房代碼
                    {
                        p5 = null;
                    }

                    var repo = new FA0069Repository(DBWork);
                    session.Result.etts = repo.GetFa0069(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
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
            var p2 = form.Get("p2");    //單位種類  INID_FLAG
            var p3 = form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL
            var p5 = form.Get("p5");    //單位科別庫房代碼 
            var p6 = form.Get("p6") == null ? string.Empty : form.Get("p6");
            var p7 = form.Get("p7") == null ? string.Empty : form.Get("p7");
            var p8 = form.Get("p8") == null ? string.Empty : form.Get("p8");
            var p9 = form.Get("p9") == null ? string.Empty : form.Get("p9");
            var p10 = form.Get("p10") == null ? string.Empty : form.Get("p10");    //院內碼是否作廢
            var p11 = form.Get("p11") == null ? string.Empty : form.Get("p11");    //是否寄售
            var p12 = form.Get("p12") == null ? string.Empty : form.Get("p12");    //庫房是否作廢

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DataTable data;
                string fileName = form.Get("FN");
                try
                {
                    if (p2 != "4")  //只有是否庫備Radio選擇單位科別才會抓取庫房代碼
                    {
                        p5 = string.Empty;
                    }

                    var repo = new FA0069Repository(DBWork);
                    data = repo.GetExcel(p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
                    if (data.Rows.Count > 0)
                    {
                        var workbook = ExoprtToExcel(data);

                        //output
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            workbook.Write(memoryStream);
                            JCLib.Export.OutputFile(memoryStream, fileName);
                            workbook.Close();
                        }
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

        public ApiResponse GetMatCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0069Repository(DBWork);
                    session.Result.etts = repo.GetMatCombo();
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
                    var repo = new FA0069Repository(DBWork);
                    session.Result.etts = repo.GetYMCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

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

                    if (clsALL == true)
                    {
                        var repo = new FA0069Repository(DBWork);
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
                        var repo = new FA0069Repository(DBWork);
                        session.Result.etts = repo.GetMMCodeCombo(p0, p1, clsALL, page, limit, sorters);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH_NoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0069Repository repo = new FA0069Repository(DBWork);
                    session.Result.etts = repo.GetWH_NoCombo(p0, page, limit, "")
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_GRADE = w.WH_GRADE });
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
