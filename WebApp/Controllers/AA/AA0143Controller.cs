using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using System.Data;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.Web;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0143Controller : SiteBase.BaseApiController
    {
        #region GetComboBox
        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
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
                    AA0143Repository repo = new AA0143Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTOWHcombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0143Repository repo = new AA0143Repository(DBWork);
                    session.Result.etts = repo.GetTOWHcombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetStatusCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0143Repository repo = new AA0143Repository(DBWork);
                    session.Result.etts = repo.GetStatusCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAgenCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0143Repository repo = new AA0143Repository(DBWork);
                    session.Result.etts = repo.GetAgenCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        //查詢
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //mmcode
            var p1 = form.Get("p1"); //agen_no
            var p2 = form.Get("p2"); //towh
            var p3 = form.Get("p3"); //apptime_start
            var p4 = form.Get("p4"); //apptime_end
            var p5 = form.Get("p5"); //status
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0143Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }      
        //產生訂單
        [HttpPost]
        public ApiResponse Order(FormDataCollection form)
        {
            var crdocno = form.Get("crdocno"); //醫緊急療出貨單編號           

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0143Repository(DBWork);                    
                    session.Result.afrs = repo.Order(crdocno, User.Identity.Name);
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
        //重新讀取廠商EMAL
        [HttpPost]
        public ApiResponse ReadEmail(FormDataCollection form)
        {
            var crdocno = form.Get("crdocno"); //醫緊急療出貨單編號  
            var update_user = "";
            var update_ip = "";

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0143Repository(DBWork);
                    update_user = User.Identity.Name;
                    update_ip = DBWork.ProcIP;
                    session.Result.afrs = repo.ReadEmail(crdocno, update_user, update_ip);
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
        //重新寄信
        [HttpPost]
        public ApiResponse SendMail(FormDataCollection form)
        {
            var crdocno = form.Get("crdocno"); //醫緊急療出貨單編號  
            var update_user = "";
            var update_ip = "";

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0143Repository(DBWork);
                    update_user = User.Identity.Name;
                    update_ip = DBWork.ProcIP;
                    session.Result.afrs = repo.SendMail(crdocno, update_user, update_ip);
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
        //日報表
        [HttpPost]
        public ApiResponse GetReport(FormDataCollection form)
        {
            var p0 = form.Get("p6"); //apptime_start
            var p1 = form.Get("p7"); //apptime_end
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0143Repository(DBWork);
                    session.Result.etts = repo.GetReport(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region Excel
        [HttpPost]
        public ApiResponse ReportExcel(FormDataCollection form)
        {
            var apptime_start = form.Get("start"); //申請時間起
            var apptime_end = form.Get("end"); //申請時間迄

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {                   
                    DataTable data;
                    var repo = new AA0143Repository(DBWork);
                    data = repo.GetExcel(apptime_start, apptime_end);
                    if (data.Rows.Count > 0)
                    {
                        var workbook = ExoprtToExcel_MuitlSheet(data);

                        //output
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            workbook.Write(memoryStream);
                            JCLib.Export.OutputFile(memoryStream, form.Get("FN"));
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
        //製造EXCEL的內容(單一Sheet)
        //public XSSFWorkbook ExoprtToExcel_SingSheet(DataTable data)
        //{
        //    var wb = new XSSFWorkbook();
        //    var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");

        //    IRow row = sheet.CreateRow(0);
        //    for (int i = 0; i < data.Columns.Count; i++)
        //    {
        //        row.CreateCell(i).SetCellValue(data.Columns[i].ToString());
        //    }

        //    for (int i = 0; i < data.Rows.Count; i++)
        //    {
        //        row = sheet.CreateRow(1 + i);
        //        for (int j = 0; j < data.Columns.Count; j++)
        //        {
        //            row.CreateCell(j).SetCellValue(data.Rows[i].ItemArray[j].ToString());
        //        }
        //    }
        //    return wb;
        //}

        //製造EXCEL的內容(多Sheet)
        public XSSFWorkbook ExoprtToExcel_MuitlSheet(DataTable data)
        {
            IEnumerable<string> yyymms = data.AsEnumerable().Select(x => x.Field<string>("報表日期")).OrderBy(x => x).Distinct();

            var wb = new XSSFWorkbook();

            if (yyymms.Any() == false)
            {
                var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");
                IRow row = sheet.CreateRow(0);
                row.CreateCell(0).SetCellValue("無資料");

                return wb;
            }

            foreach (string yyymm in yyymms)
            {
                var sheet = (XSSFSheet)wb.CreateSheet(yyymm);

                IRow row = sheet.CreateRow(0);
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(data.Columns[i].ToString());
                }

                DataTable temp = data.AsEnumerable().Where(x => x.Field<string>("報表日期") == yyymm)  
                                    .CopyToDataTable();

                for (int i = 0; i < temp.Rows.Count; i++)
                {
                    row = sheet.CreateRow(1 + i);
                    for (int j = 0; j < temp.Columns.Count; j++)
                    {
                        row.CreateCell(j).SetCellValue(temp.Rows[i].ItemArray[j].ToString());
                    }
                }
            }


            return wb;
        }

        #endregion
    }
}
