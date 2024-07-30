using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Web;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace WebApp.Controllers.AB
{
    public class AB0085Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public void GetExcel(FormDataCollection form)
        {
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            string wh_no = form.Get("wh_no");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0085Repository repo = new AB0085Repository(DBWork);
                    var ts = form.Get("TS");
                    var ss = ts.Split(',');
                    var apptime_bg = ss[0];
                    var apptime_ed = ss[1];
                    var fileName = form.Get("FN");
                    string hospCode = repo.GetHospCode();
                    var data = repo.GetMTotal(form.Get("TS"), wh_no, hospCode == "0");
                    if (data.Count() > 0)
                    {
                        data.ElementAt(0).日期起 = apptime_bg;
                        data.ElementAt(0).日期迄 = apptime_ed;
                        var btime = repo.SET_BTIME(apptime_bg);
                        var etime = repo.SET_ETIME(apptime_ed);
                        if (btime.Count() > 0)
                        {
                            data.ElementAt(0).開帳日期 = btime.Take(1).FirstOrDefault();
                        }
                        else
                        {
                            data.ElementAt(0).開帳日期 = "";
                        }
                        if (etime.Count() > 0)
                        {
                            data.ElementAt(0).關帳日期 = etime.Take(1).FirstOrDefault();
                        }
                        else
                        {
                            data.ElementAt(0).關帳日期 = "";
                        }
                        var workbook = ExoprtToExcel(data);

                        //output
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            workbook.Write(memoryStream);
                            JCLib.Export.OutputFile(memoryStream, fileName);
                            workbook.Close();
                        }
                    }

                    //JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(form.Get("TS")));
                    //JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4, p5));
                    //session.Result.etts = repo.GetExcel(p0, p1, p2, p3, p4,p5);
                }
                catch
                {
                    throw;
                }
            }
        }

        [HttpPost]

        public ApiResponse GetExcel2(FormDataCollection form)
        {
            var ts = form.Get("TS");
            string wh_no = form.Get("wh_no");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0085Repository repo = new AB0085Repository(DBWork);
                    JCLib.Excel.Export("酒精用量統計月報表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(ts, wh_no ,false));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public XSSFWorkbook ExoprtToExcel(IEnumerable<AB0085Repository.M> data)
        {
            //var ss = s.Split(',');
            //var apptime_bg = ss[0];
            //var apptime_ed = ss[1];
            var wb = new XSSFWorkbook();
            var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");


            var headers = data.Select(d => new { d.英文品名, d.院內碼, d.廠商英文名稱 }).Distinct();
            IRow row = sheet.CreateRow(0);
            row.CreateCell(1).SetCellValue("酒精用量統計月報表");
            row.CreateCell(2).SetCellValue("日期" + data.ElementAt(0).日期起 + "~" + data.ElementAt(0).日期迄);
            row.CreateCell(3).SetCellValue("起始日期" + data.ElementAt(0).開帳日期 + "  結束日期" + data.ElementAt(0).關帳日期);

            row = sheet.CreateRow(1);
            row.CreateCell(0).SetCellValue("單位");
            for (int i = 0; i < headers.Count(); i++)
            {
                row.CreateCell(i + 1).SetCellValue(headers.ElementAt(i).英文品名);
            }


            row = sheet.CreateRow(2);
            for (int i = 0; i < headers.Count(); i++)
            {
                row.CreateCell(i + 1).SetCellValue(headers.ElementAt(i).院內碼);
            }


            row = sheet.CreateRow(3);
            for (int i = 0; i < headers.Count(); i++)
            {
                row.CreateCell(i + 1).SetCellValue(headers.ElementAt(i).廠商英文名稱);
            }


            var groups = data.GroupBy(d => d.庫房名稱);
            for (int i = 0; i < groups.Count(); i++)
            {
                var group = groups.ElementAt(i);

                row = sheet.CreateRow(4 + i);
                row.CreateCell(0).SetCellValue(group.Key);

                for (int j = 0; j < headers.Count(); j++)
                {
                    var header = headers.ElementAt(j);
                    var value = group.Where(m => m.院內碼 == header.院內碼 && m.廠商英文名稱 == header.廠商英文名稱).Sum(m => m.核發數量);
                    row.CreateCell(j + 1).SetCellValue(value);
                }
            }
            return wb;
        }

        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {

            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var wh_no = form.Get("wh_no")== null ? string.Empty : form.Get("wh_no");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0085Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetAll(p1, p2, wh_no, page, limit, sorters, User.Identity.Name, hospCode == "0");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        // GET: AB0085


        #region combo
        public ApiResponse Whnos(FormDataCollection form)
        {

            string queryString = form.Get("queryString");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0085Repository(DBWork);
                    session.Result.etts = repo.GetWhnos(queryString).Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        public ApiResponse GetHospCode()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0085Repository(DBWork);
                    session.Result.msg = repo.GetHospCode();
                    //session.Result.etts = repo.GetWhnos(queryString).Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });

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