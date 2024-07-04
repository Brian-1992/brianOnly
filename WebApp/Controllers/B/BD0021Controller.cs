using JCLib.DB;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web;
using WebApp.Models;
using WebApp.Repository.B;

namespace WebApp.Controllers.B
{
    public class BD0021Controller : SiteBase.BaseApiController
    {
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");    // 庫房代碼
            var p1 = form.Get("p1");    // 物料分類
            var p2 = form.Get("p2");    // 院內碼
            var p3 = form.Get("p3");    // 是否合約
            var p4 = form.Get("p4");    // 訂單編號
            var d0 = form.Get("d0");    // 訂單日期起
            var d1 = form.Get("d1");    // 訂單日期訖
            var p5 = form.Get("p5");    // 廠商代碼


            //var showopt = form.Get("showopt");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0021Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, p3, p4, d0, d1,p5, page, limit, sorters); //撈出object
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse AllD(FormDataCollection form)
        {
            var p0 = form.Get("p0");    // 訂單編號
            var d1 = form.Get("d1");    // 訂單日期訖

            //var showopt = form.Get("showopt");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0021Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters); //撈出object
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWhCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0021Repository repo = new BD0021Repository(DBWork);
                    session.Result.etts = repo.GetWhCombo();
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
            var p0 = form.Get("p0");    // 庫房代碼

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0021Repository repo = new BD0021Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo(p0);
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
            var p0 = form.Get("p0");
            var mat_class = form.Get("MAT_CLASS");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0021Repository repo = new BD0021Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, mat_class, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetPoTime()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0021Repository repo = new BD0021Repository(DBWork);
                    session.Result.etts = repo.GetPoTime();
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");    // 庫房代碼
            var p1 = form.Get("p1");    // 物料分類
            var p2 = form.Get("p2");    // 院內碼
            var p3 = form.Get("p3");    // 是否合約
            var p4 = form.Get("p4");    // 是否合約
            var d0 = form.Get("d0");    // 訂單日期起
            var d1 = form.Get("d1");    // 訂單日期訖

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0021Repository repo = new BD0021Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;
                    result = repo.GetExcel(p0, p1, p2, p3, p4, d0, d1);
                    var workbook = ExoprtToExcel(result);

                    //output
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        workbook.Write(memoryStream);
                        JCLib.Export.OutputFile(memoryStream, string.Format("{0}.xlsx", form.Get("FN")));
                        workbook.Close();
                    }
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        public XSSFWorkbook ExoprtToExcel(DataTable data)
        {
            var wb = new XSSFWorkbook();
            var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");
            try
            {
                IRow row = sheet.CreateRow(0);
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    if (data.Columns[i].ToString().ToLower() == "base_unit")
                    {
                        row.CreateCell(i).SetCellValue("院內最小計量單位");
                    }
                    else {
                        row.CreateCell(i).SetCellValue(data.Columns[i].ToString());
                    }
                    
                }

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    row = sheet.CreateRow(1 + i);
                    for (int j = 0; j < data.Columns.Count; j++)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i].ItemArray[j].ToString());
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return wb;
        }

        [HttpPost]
        public ApiResponse GetAgennoCombo(FormDataCollection form)
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
                    BD0021Repository repo = new BD0021Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
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