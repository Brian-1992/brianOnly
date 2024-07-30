using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using static WebApp.Repository.C.CE0046Repository;
using System.Linq;
using System.Web;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using WebApp.Models.D;

namespace WebApp.Controllers.C
{
    public class CE0046Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("set_ym"); // chk_ym盤點年月
            var p1 = form.Get("ur_inid"); // ur_inid 責任中心
            var user = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0046Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, user, page, limit, sorters);
                }
                catch (Exception e)
                {
                    var a = e.Message.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        /// <summary>
        /// 月結年月
        /// </summary>
        [HttpPost]
        public ApiResponse GetYmCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0046Repository repo = new CE0046Repository(DBWork);
                    session.Result.etts = repo.GetYmCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        /// <summary>
        /// 責任中心代碼
        /// </summary>
        [HttpPost]
        public ApiResponse GetUrInidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0046Repository repo = new CE0046Repository(DBWork);
                    session.Result.etts = repo.GetUrInidCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        /// <summary>
        /// 盤點資訊
        /// </summary>
        [HttpPost]
        public ApiResponse GetChkStatus(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var p0 = form.Get("chk_ym"); // chk_ym盤點年月
                    CE0046Repository repo = new CE0046Repository(DBWork);
                    session.Result.etts = repo.GetChkStatus(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result; ;
            }
        }

        /// <summary>
        /// 匯出
        /// </summary>
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        { // 【匯出】按鈕
            string p0 = form.Get("p0"); // 年月
            string p1 = form.Get("p1"); // 責任中心
            string excelName = p1 + "_" + (Convert.ToInt32(DateTime.Now.ToString("yyyy")) - 1911).ToString() + DateTime.Now.ToString("MMddhhmmss") + ".xlsx";

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    using (var dataSet = new System.Data.DataSet())
                    {
                        CE0046Repository repo = new CE0046Repository(DBWork);
                        var dataTable1 = repo.GetExcelListSheet(p0, p1);
                        dataTable1.TableName = "查詢資料";
                        dataSet.Tables.Add(dataTable1);

                        var dataTable2 = repo.GetExcelSummarySheet(p0, p1);
                        dataTable2.TableName = "加總資料";
                        dataSet.Tables.Add(dataTable2);

                        JCLib.Excel.Export(excelName, dataSet);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public class HeaderItem
        {
            public string Name { get; set; }
            public string FieldName { get; set; }
            public int Index { get; set; }
            public HeaderItem()
            {
                Name = string.Empty;
                Index = -1;
                FieldName = string.Empty;
            }
            public HeaderItem(string name, int index, string fieldName)
            {
                Name = name;
                Index = index;
                FieldName = fieldName;
            }
            public HeaderItem(string name, string fieldName)
            {
                Name = name;
                Index = -1;
                FieldName = fieldName;
            }
        }

        public List<HeaderItem> SetHeaderIndex(List<HeaderItem> list, IRow headerRow)
        {
            int cellCounts = headerRow.LastCellNum;
            for (int i = 0; i < cellCounts; i++)
            {
                foreach (HeaderItem item in list)
                {
                    if (headerRow.GetCell(i).ToString() == item.Name)
                    {
                        item.Index = i;
                    }
                }
            }

            return list;
        }
    }
}
