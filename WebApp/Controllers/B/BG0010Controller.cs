using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.Data;

namespace WebApp.Controllers.B
{
    public class BG0010Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var user = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            string[] arr_p1 = { };
            if (!string.IsNullOrEmpty(p1))
            {
                arr_p1 = p1.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0010Repository(DBWork);
                    string hospCode = repo.GetHospCode();

                    session.Result.etts = repo.GetAll(p0, arr_p1, p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse ReCalulate(FormDataCollection form)
        {
            var p0 = form.Get("set_ym");
            var user = User.Identity.Name;

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0010Repository(DBWork);
                    DBWork.BeginTransaction();
                    if (repo.ChkSetYM(p0))  
                    {
                        session.Result.success = false;
                        session.Result.msg = "輸入年月尚未月結，請重新選擇";
                        return session.Result;
                    }
                    string hospCode = repo.GetHospCode();
                    //刪除資料
                    repo.deleteData(p0);
                    //重新計算
                    repo.ImportData(p0, hospCode, User.Identity.Name, DBWork.ProcIP);

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse SumAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var user = User.Identity.Name;
            string[] arr_p1 = { };
            if (!string.IsNullOrEmpty(p1))
            {
                arr_p1 = p1.Trim().Split(','); //用,分割
            }

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0010Repository(DBWork);
                    session.Result.etts = repo.GetSumAll(p0, arr_p1, p2, p3);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetYm()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0010Repository(DBWork);
                    session.Result.afrs = repo.GetYm();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMatClassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    BG0010Repository repo = new BG0010Repository(DBWork);
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
        public ApiResponse GetAgenNoCombo(FormDataCollection form)
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
                    BG0010Repository repo = new BG0010Repository(DBWork);
                    session.Result.etts = repo.GetAgenNoCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAgenNoCombo_1(FormDataCollection form)
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
                    BG0010Repository repo = new BG0010Repository(DBWork);
                    session.Result.etts = repo.GetAgenNoCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //修改
        [HttpPost]
        public ApiResponse Update(BG0010 bg0010)
        {

            IEnumerable<BG0010> invchks = JsonConvert.DeserializeObject<IEnumerable<BG0010>>(bg0010.ITEM_STRING);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BG0010Repository repo = new BG0010Repository(DBWork);
                    string msg = string.Empty;
                    foreach (BG0010 invchk in invchks)
                    {
                        invchk.UPDATE_USER = User.Identity.Name;
                        invchk.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Update(invchk);
                    }
                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMiMnset(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BG0010Repository(DBWork);
                    session.Result.etts = repo.GetMiMnset(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse ChkSetYM(FormDataCollection form)
        {
            string set_ym = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0010Repository repo = new BG0010Repository(DBWork);
                    if (repo.ChkSetYM(set_ym))
                        session.Result.msg = "N";
                    else
                        session.Result.msg = "Y";
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            string[] arr_p1 = { };
            if (!string.IsNullOrEmpty(p1))
            {
                arr_p1 = p1.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0010Repository repo = new BG0010Repository(DBWork);

                   var workbook=  CreatWorkbook(repo.GetExcel(p0, arr_p1, p2, p3));

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        workbook.Write(memoryStream);
                        JCLib.Export.OutputFile(memoryStream, "BG0010.xls");
                        workbook.Close();
                    }
                    //JCLib.Excel.Export("BG0010.xls", repo.GetExcel(p0, arr_p1, p2, p3));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel2(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            string[] arr_p1 = { };
            if (!string.IsNullOrEmpty(p1))
            {
                arr_p1 = p1.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BG0010Repository repo = new BG0010Repository(DBWork);

                    //var workbook = CreatWorkbook(repo.GetExcel(p0, arr_p1, p2, p3));

                    //using (MemoryStream memoryStream = new MemoryStream())
                    //{
                    //    workbook.Write(memoryStream);
                    //    JCLib.Export.OutputFile(memoryStream, "BG0010.xls");
                    //    workbook.Close();
                    //}
                    JCLib.Excel.Export("支付明細.xls", repo.GetExcel2(p0, arr_p1, p2, p3));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        //製造EXCEL的內容
        public XSSFWorkbook CreatWorkbook(DataTable data)
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
                    if ((j > 2 && j < 13) || (j ==15))
                    {
                        var cell = row.CreateCell(j);
                        cell.SetCellType(CellType.Numeric);
                        var a = data.Rows[i].ItemArray[j];
                        a = a == DBNull.Value ? 0 : a;
                        cell.SetCellValue(Convert.ToDouble(a));
                    }
                    else
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i].ItemArray[j].ToString());
                    }

                }
            }
            return wb;
        }

    }
}