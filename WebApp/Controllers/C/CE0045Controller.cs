using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using static WebApp.Repository.C.CE0045Repository;
using System.Linq;
using System.Web;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using WebApp.Models.D;

namespace WebApp.Controllers.C
{
    public class CE0045Controller : SiteBase.BaseApiController
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
                    var repo = new CE0045Repository(DBWork);
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
                    CE0045Repository repo = new CE0045Repository(DBWork);
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
                    CE0045Repository repo = new CE0045Repository(DBWork);
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
                    var p1 = form.Get("ur_inid"); // ur_inid 責任中心
                    CE0045Repository repo = new CE0045Repository(DBWork);
                    session.Result.etts = repo.GetChkStatus(p0, p1);
                }
                catch
                {
                    throw;
                }
                return session.Result; ;
            }
        }

        /// <summary>
        /// 儲存 grid 內容
        /// </summary>
        [HttpPost]
        public ApiResponse UpdateDetails(FormDataCollection form)
        {
            var itemString = form.Get("list");
            string strSetYm = form.Get("set_ym");
            string strUrInid = form.Get("ur_inid");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;

                try
                {
                    var repo = new CE0045Repository(DBWork);
                    IEnumerable<DGMISS_CHK> items = JsonConvert.DeserializeObject<IEnumerable<DGMISS_CHK>>(itemString);

                    // a.檢查盤點當狀態是否已變更，若盤點單狀態不為1則跳出錯誤訊息「盤點單狀態已變更，請重新查詢」
                    IEnumerable<CHK_ST> rec = repo.GetChkStatus(strSetYm, strUrInid);

                    string strChkStatus = rec.First().CHK_STATUS;
                    if (
                        rec.Any() &&
                        !strChkStatus.Equals("盤中"))
                    {
                        session.Result.msg = "盤點單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    if (items.Any())
                    {
                        foreach (DGMISS_CHK item in items)
                        {
                            item.DATA_YM = strSetYm;
                            item.INID = strUrInid;
                            item.CHK_UID = DBWork.UserInfo.UserId;
                            item.UPDATE_USER = DBWork.UserInfo.UserId;
                            item.UPDATE_IP = DBWork.ProcIP;
                            item.CHK_STATUS = "1";

                            session.Result.afrs = repo.UpdateDgmissChkDetail(item); // b.更新CHK_DETAIL盤點量、調整消耗
                        }
                    }

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
            string strSetYm = form.Get("p0"); // 年月
            string strUrInid = form.Get("p1"); // 責任中心
            string excelName = strUrInid + "_" + (Convert.ToInt32(DateTime.Now.ToString("yyyy")) - 1911).ToString() + DateTime.Now.ToString("MMddhhmmss") + ".xlsx";

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CE0045Repository repo = new CE0045Repository(DBWork);
                    JCLib.Excel.Export(excelName, repo.GetExcel(
                            strSetYm, // 年月
                            strUrInid // 責任中心
                        )
                    );
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯入檢核
        [HttpPost]
        public ApiResponse Import()
        {
            using (WorkSession session = new WorkSession(this))
            {
                List<DGMISS_CHK> list = new List<DGMISS_CHK>();
                UnitOfWork DBWork = session.UnitOfWork;

                string strSetYm = HttpContext.Current.Request.Form["set_ym"];
                string strUrInid = HttpContext.Current.Request.Form["ur_inid"];

                DBWork.BeginTransaction();
                try
                {
                    CE0045Repository repo = new CE0045Repository(DBWork);

                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                        new HeaderItem("資料年月", "DATA_YM"),
                        new HeaderItem("責任中心", "INID"),
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("盤點量", "CHK_QTY"),
                        new HeaderItem("備註", "MEMO"),
                    };

                    IWorkbook workBook;
                    var HttpPostedFile = HttpContext.Current.Request.Files["file"];

                    if (Path.GetExtension(HttpPostedFile.FileName).ToLower() == ".xls")
                    {
                        workBook = new HSSFWorkbook(HttpPostedFile.InputStream);
                    }
                    else
                    {
                        workBook = new XSSFWorkbook(HttpPostedFile.InputStream);
                    }
                    var sheet = workBook.GetSheetAt(0);
                    IRow headerRow = sheet.GetRow(0);//由第一列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum;

                    headerItems = SetHeaderIndex(headerItems, headerRow);

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }


                        DGMISS_CHK model = repo.GetDGMISS_CHK(new DGMISS_CHK
                        {
                            DATA_YM = row.GetCell(0).ToString(),
                            INID = row.GetCell(1).ToString(),
                            MMCODE = row.GetCell(2).ToString()
                        });


                        foreach (HeaderItem item in headerItems)
                        {
                            string strTmp = item.FieldName;
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            string strFieldName = item.FieldName;
                            model.GetType().GetProperty(item.FieldName).SetValue(model, value, null);
                        }

                        list.Add(model);
                    }

                    // 更新 DGMISS_CHK 盤點量
                    session.Result.afrs = UpdateDirtyData(list, DBWork);

                    DBWork.Commit();
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

        /// <summary>
        /// 更新 有填 盤點量的資料
        /// </summary>
        /// <param name="list"></param>
        /// <param name="DBWork"></param>
        /// <returns></returns>
        public int UpdateDirtyData(IEnumerable<DGMISS_CHK> list, UnitOfWork DBWork)
        {
            try
            {
                int afrs = 0;
                CE0045Repository repo = new CE0045Repository(DBWork);

                foreach (DGMISS_CHK item in list)
                {
                    item.DATA_YM = item.DATA_YM.Replace("/", string.Empty);

                    if (!string.IsNullOrEmpty(item.CHK_QTY) && item.CHK_QTY != "0")
                    {
                        item.CREATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        repo.UpdateDgmissChkDetail(item);
                    }
                }
                return afrs;
            }
            catch
            {
                DBWork.Rollback();
                throw;
            }
        }
    }
}
