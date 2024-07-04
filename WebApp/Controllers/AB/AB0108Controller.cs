using JCLib.DB;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AB
{
    public class AB0108Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetData(FormDataCollection form) {
            string start_date = form.Get("p0");
            string end_date = form.Get("p1");
            string status = form.Get("p2");
            string wh_no = form.Get("p3");
            string mmcode = form.Get("p4");
            bool view_all = form.Get("view_all") == "Y";

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0108Repository(DBWork);

                    if (string.IsNullOrEmpty(wh_no) == false && view_all == false) {
                        if (repo.CheckWhnoValid(wh_no, DBWork.UserInfo.UserId) == false) {
                            session.Result.success = false;
                            session.Result.msg = "無此庫房權限，請重新選擇";
                            return session.Result;
                        }
                    }

                    session.Result.etts = repo.GetData(start_date, end_date, status, wh_no, mmcode, view_all, DBWork.UserInfo.UserId);
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public void Excel(FormDataCollection form) {
            string start_date = form.Get("start_date");
            string end_date = form.Get("end_date");
            string status = form.Get("status");
            string wh_no = form.Get("wh_no");
            string mmcode = form.Get("mmcode");
            bool view_all = form.Get("view_all") == "Y";

            DataTable data = null;

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0108Repository(DBWork);

                    if (string.IsNullOrEmpty(wh_no) == false && view_all == false)
                    {
                        if (repo.CheckWhnoValid(wh_no, DBWork.UserInfo.UserId) == false)
                        {
                            session.Result.success = false;
                            session.Result.msg = "無此庫房權限，請重新選擇";
                        }
                    }

                    data = repo.GetExcel(start_date, end_date, status, wh_no, mmcode, view_all, DBWork.UserInfo.UserId);

                    string fileName = string.Format("衛星庫房條碼扣庫異動記錄_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));

                    if (data.Rows.Count > 0)
                    {
                        var workbook = ExoprtToExcel(data);

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
                                    "attachment; filename=" + fileName);
                        res.BinaryWrite(ms.ToArray());

                        ms.Close();
                        ms.Dispose();
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
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

        #region combo

        [HttpPost]
        public ApiResponse GetWhnoCombo(FormDataCollection form) {
            bool view_all = form.Get("view_all") == "Y";
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0108Repository(DBWork);
                    if (view_all)
                    {
                        session.Result.etts = repo.GetViewAllWhnos();
                    }
                    else {
                        session.Result.etts = repo.GetWhnoCombo(DBWork.UserInfo.UserId);
                    }
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse GetStatusCombo() {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0108Repository(DBWork);
                    session.Result.etts = repo.GetStatusCombo();
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }

        #endregion


    }
}