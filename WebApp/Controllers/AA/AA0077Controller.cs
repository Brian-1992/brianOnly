using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Web;
using NPOI.SS.UserModel;

namespace WebApp.Controllers.AA
{
    public class AA0077Controller : ApiController
    {
        [HttpPost]
        public void GetExcel(FormDataCollection form)
        {
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0077Repository repo = new AA0077Repository(DBWork);
                    var ts = form.Get("TS");
                    var ss = ts.Split(',');
                    var apptime_bg = ss[2];
                    var apptime_ed = ss[3];

                    string hospCode = repo.GetHospCode();

                    var fileName = form.Get("FN");
                    var data = repo.GetMTotal(form.Get("TS"), hospCode=="0");
                    var data1 = repo.GetMSum(form.Get("TS"), hospCode == "0");
                    var data2 = repo.GetMSumpack(form.Get("TS"), hospCode == "0");
                    if (data.Count() > 0)
                    {
                        data.ElementAt(0).日期起 = apptime_bg;
                        data.ElementAt(0).日期迄 = apptime_ed;
                        var workbook = ExoprtToExcel(data, data1, data2);

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

        public static HSSFWorkbook ExoprtToExcel(IEnumerable<AA0077Repository.M> data, IEnumerable<AA0077Repository.M> data1, IEnumerable<AA0077Repository.M> data2)
        {
            //var ss = s.Split(',');
            //var apptime_bg = ss[0];
            //var apptime_ed = ss[1];
            var wb = new HSSFWorkbook();
            var sheet = (HSSFSheet)wb.CreateSheet("Sheet1");


            var headers = data.Select(d => new { d.英文品名, d.院內碼, d.廠商英文名稱 }).Distinct();
            var headers1 = data1.Select(d => new { d.院內碼 }).Distinct();
            var headers2 = data2.Select(d => new { d.院內碼 }).Distinct();
            IRow row = sheet.CreateRow(0);
            row.CreateCell(1).SetCellValue("大瓶點滴之申請量總表");
            row.CreateCell(2).SetCellValue("日期" + data.ElementAt(0).日期起 + "~" + data.ElementAt(0).日期迄);

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
            var groups1 = data1.GroupBy(d => d.院內碼);
            var groups2 = data.GroupBy(d => d.院內碼);
            var groups3 = data2.GroupBy(d => d.院內碼);
            for (int i = 0; i < groups.Count(); i++)
            {
                var group = groups.ElementAt(i);

                row = sheet.CreateRow(4 + i);
                row.CreateCell(0).SetCellValue(group.Key);

                for (int j = 0; j < headers.Count(); j++)
                {
                    var header = headers.ElementAt(j);
                    var value = group.Where(m => m.院內碼 == header.院內碼 && m.廠商英文名稱 == header.廠商英文名稱).Sum(m => m.核發數量);
                    if (value > 0)
                    {
                        row.CreateCell(j + 1).SetCellValue(value);
                    }
                }
            }
            row = sheet.CreateRow(4 + groups.Count());
            row.CreateCell(0).SetCellValue("PH1S-藥庫");


            for (int i = 0; i < groups1.Count(); i++)
            {

                var group2 = groups2.ElementAt(i);

                if (i == 0)
                {
                    row = sheet.CreateRow(5 + groups.Count() + i);
                    row.CreateCell(0).SetCellValue("合計");
                }


                for (int j = 0; j < headers1.Count(); j++)
                {
                    var header = headers1.ElementAt(j);
                    var group1 = groups1.ElementAt(j);
                    if (group2.Key == group1.Key)
                    {
                        var value = group1.Where(m => m.院內碼 == header.院內碼).Sum(m => m.合計數量);
                        row.CreateCell(i + 1).SetCellValue(value);
                        j = j + 1;
                    }
                    //var value = group1.Where(m => m.院內碼 == header.院內碼 ).Sum(m => m.合計數量);
                    //row.CreateCell(j + 1).SetCellValue(value);
                }
            }

            for (int i = 0; i < groups2.Count(); i++)
            {

                var group2 = groups2.ElementAt(i);

                if (i == 0)
                {
                    row = sheet.CreateRow(6 + groups.Count() + i);
                    row.CreateCell(0).SetCellValue("箱數");
                }


                for (int j = 0; j < headers2.Count(); j++)
                {
                    var header = headers2.ElementAt(j);
                    var group3 = groups3.ElementAt(j);
                    if (group2.Key == group3.Key)
                    {
                        var value = group3.Where(m => m.院內碼 == header.院內碼).Sum(m => m.箱數);
                        row.CreateCell(i + 1).SetCellValue(value);
                        j = j + 1;
                    }
                    //var value = group1.Where(m => m.院內碼 == header.院內碼 ).Sum(m => m.合計數量);
                    //row.CreateCell(j + 1).SetCellValue(value);
                }
            }

            return wb;
        }

        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {

            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0077Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetAll(p1, p2, page, limit, sorters, User.Identity.Name, hospCode=="0");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        // GET: AA0077
    }
}