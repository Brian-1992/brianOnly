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
using WebApp.Repository.AA;

namespace WebApp.Controllers.AA
{
    public class AA0152Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");    // 庫房代碼
            var p1 = form.Get("p1");    // 物料分類
            var p2 = form.Get("p2");    // 院內碼
            var d0 = form.Get("d0");    // 效期起
            var d1 = form.Get("d1");    // 效期訖
            var isab = form.Get("isab");
            var p3 = form.Get("p3");  //效期9991231 是否顯示


            //var showopt = form.Get("showopt");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");
            var tuser = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0152Repository(DBWork);
                    //session.Result.etts = repo.GetAll(d0, d1, p0, p1, showopt, page, limit, sorters); //撈出object
                    session.Result.etts = repo.GetAll(p0, p1, p2, d0, d1, isab,p3, tuser, page, limit, sorters); //撈出object
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
                    AA0152Repository repo = new AA0152Repository(DBWork);
                    session.Result.etts = repo.GetWhCombo(User.Identity.Name);
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
                    AA0152Repository repo = new AA0152Repository(DBWork);
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
                    AA0152Repository repo = new AA0152Repository(DBWork);
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
        public ApiResponse GetExpDate()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0152Repository repo = new AA0152Repository(DBWork);
                    session.Result.etts = repo.GetExpDate();
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
            var d0 = form.Get("d0");    // 效期起
            var d1 = form.Get("d1");    // 效期訖
            var p3 = form.Get("p3");  //效期9991231 是否顯示

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0152Repository repo = new AA0152Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;
                    result = repo.GetExcel(p0, p1, p2, d0, d1,p3);
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
            }
            catch (Exception e)
            {
                throw;
            }
            return wb;
        }
    }
}