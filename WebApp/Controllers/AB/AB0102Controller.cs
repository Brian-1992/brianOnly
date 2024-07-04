using JCLib.DB;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AB
{
    public class AB0102Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            string date1 = form.Get("p0");
            string date2 = form.Get("p1");
            string wh_no = form.Get("p2");
            string mmcode = form.Get("p3");
            bool failOnly = form.Get("p4") == "Y";
            string proc_msg = form.Get("p5");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AB0102Repository repo = new AB0102Repository(DBWork);
                    session.Result.etts = repo.GetAll(date1, date2, wh_no, mmcode, failOnly, proc_msg);

                }
                catch (Exception ex)
                {
                    throw;
                }

                return session.Result;
            }

        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            string date1 = form.Get("p0");
            string date2 = form.Get("p1");
            string wh_no = form.Get("p2");
            string mmcode = form.Get("p3");
            bool failOnly = form.Get("p4") == "Y";
            string proc_msg = form.Get("p5");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0102Repository repo = new AB0102Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;

                    result = repo.GetExcel(date1, date2, wh_no, mmcode, failOnly, proc_msg);

                    var workbook = ExoprtToExcel(result);


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
                                "attachment; filename=" + string.Format("{0}.xlsx", form.Get("FN")));
                    res.BinaryWrite(ms.ToArray());

                    ms.Close();
                    ms.Dispose();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetLatestData()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AB0102Repository repo = new AB0102Repository(DBWork);
                    session.Result.etts = repo.GetLatestData();

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region combo
        public ApiResponse Whnos(FormDataCollection form)
        {

            string query = form.Get("query");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AB0102Repository repo = new AB0102Repository(DBWork);
                    session.Result.etts = repo
                        .GetWhnos(query)
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_GRADE = w.WH_GRADE });

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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0102Repository repo = new AB0102Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetProcMsgCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0102Repository repo = new AB0102Repository(DBWork);
                    session.Result.etts = repo.GetProcMsg();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        #region
        [HttpPost]
        public ApiResponse Details(FormDataCollection form) {
            string date1 = form.Get("p0");
            string date2 = form.Get("p1");
            string wh_no = form.Get("p2");
            string mmcode = form.Get("p3");
            bool failOnly = form.Get("p4") == "Y";
            string proc_msg = form.Get("p5");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    AB0102Repository repo = new AB0102Repository(DBWork);
                    session.Result.etts = repo.GetDetails(date1, date2, wh_no, mmcode, failOnly, proc_msg);

                }
                catch (Exception ex)
                {
                    throw;
                }

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailExcel(FormDataCollection form)
        {
            string date1 = form.Get("p0");
            string date2 = form.Get("p1");
            string wh_no = form.Get("p2");
            string mmcode = form.Get("p3");
            bool failOnly = form.Get("p4") == "Y";
            string proc_msg = form.Get("p5") == null ? string.Empty : proc_msg = form.Get("p5");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0102Repository repo = new AB0102Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;

                    result = repo.GetDetailExcel(date1, date2, wh_no, mmcode, failOnly, proc_msg);

                    var workbook = ExoprtToExcel(result);


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
                                "attachment; filename=" + string.Format("{0}.xlsx", form.Get("FN")));
                    res.BinaryWrite(ms.ToArray());

                    ms.Close();
                    ms.Dispose();

                    


                    //dtItems.Merge(result);

                    //JCLib.Excel.Export(form.Get("FN"), dtItems);

                    //DBWork.Commit();
                }
                catch
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
        #endregion
    }
}