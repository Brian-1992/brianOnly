using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json.Linq;
using WebApp.Models.AB;
using System.Web.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using System.Net.Http;
using System.Data;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;

namespace WebApp.Controllers.AB
{
    public class AB0111Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form) {
            string crdocno = form.Get("crdocno") == null ? string.Empty : form.Get("crdocno");
            string mmcode = form.Get("mmcode") == null ? string.Empty : form.Get("mmcode");
            string agen_no = form.Get("agen_no") == null ? string.Empty : form.Get("agen_no");
            string start_date = form.Get("start_date")==null ? string.Empty : form.Get("start_date");
            string end_date = form.Get("end_date")== null ? string.Empty : form.Get("end_date");
            string status_string = form.Get("status") == null ? string.Empty : form.Get("status");
            bool isAA = form.Get("isAA") == "Y";

            string status = string.Empty;
            IEnumerable<string> status_list = status_string.Split(',').ToList<string>();
            foreach (string temp in status_list) {

                if (string.IsNullOrEmpty(temp)) {
                    continue;
                }

                if (string.IsNullOrEmpty(status) == false) {
                    status += ", ";
                }
                status += string.Format("'{0}'", temp);
            }

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0111Repository(DBWork);
                    session.Result.etts = repo.GetAll(crdocno, mmcode, agen_no,
                                                      start_date, end_date, status,
                                                      DBWork.UserInfo.UserId, isAA);
                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Details(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0111Repository(DBWork);
                    session.Result.etts = repo.GetDetails(crdocno);

                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse ChangePtName(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            string drName = form.Get("drName");
            string ptName = form.Get("ptName");
            string chartNo = form.Get("chartNo");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0111Repository(DBWork);
                    session.Result.afrs = repo.UpdatePtName(crdocno, drName, ptName, chartNo, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    session.Result.success = true;

                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            string mmcode = form.Get("mmcode");
            string agen_no = form.Get("agen_no");
            string start_date = form.Get("start_date");
            string end_date = form.Get("end_date");
            string status_string = form.Get("status") == null ? string.Empty : form.Get("status");
            bool isAA = form.Get("isAA") == "Y";

            string status = string.Empty;
            IEnumerable<string> status_list = status_string.Split(',').ToList<string>();
            foreach (string temp in status_list)
            {
                if (string.IsNullOrEmpty(temp))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(status) == false)
                {
                    status += ", ";
                }
                status += string.Format("'{0}'", temp);
            }

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0111Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;

                    result = repo.GetExcel(crdocno, mmcode, agen_no, start_date, end_date, status,
                        DBWork.UserInfo.UserId, isAA);

                    var workbook = ExoprtToExcel(result);

                    //output
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        workbook.Write(memoryStream);
                        JCLib.Export.OutputFile(memoryStream, string.Format("緊急醫療通知單查詢_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss")));
                        workbook.Close();
                    }

                    return session.Result;
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
        [HttpGet]
        public ApiResponse GetAgenCombo() {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0111Repository(DBWork);
                    session.Result.etts = repo.GetAgens();

                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpGet]
        public ApiResponse GetStatusCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0111Repository(DBWork);
                    session.Result.etts = repo.GetStatus();

                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        #endregion
    }
}