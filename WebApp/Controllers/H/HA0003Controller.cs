using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.H;
using WebApp.Models;
using System.Collections.Generic;
using System.Data;
using WebApp.Models.H;
using System;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using System.Web;
using System.Linq;
using Newtonsoft.Json;

namespace WebApp.Controllers.H
{
    public class HA0003Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var agen_bank_14 = form.Get("p0");
            var bankname = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new HA0003Repository(DBWork);
                    session.Result.etts = repo.GetAll(agen_bank_14, bankname, page, limit, sorters);
                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        // 新增
        [HttpPost]
        public ApiResponse Create(PH_BANK_AF af)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new HA0003Repository(DBWork);
                    if (repo.CheckExists(af.AGEN_BANK_14))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>銀行代碼</span>重複，請重新輸入。";

                        return session.Result;
                    }

                    af.CREATE_USER = DBWork.UserInfo.UserId;
                    af.UPDATE_USER = DBWork.UserInfo.UserId;
                    af.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Create(af);

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        // 修改
        [HttpPost]
        public ApiResponse Update(PH_BANK_AF af)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new HA0003Repository(DBWork);
                    af.UPDATE_USER = DBWork.UserInfo.UserId;
                    af.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(af);

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        // 刪除
        [HttpPost]
        public ApiResponse Delete(PH_BANK_AF af)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new HA0003Repository(DBWork);
                    if (repo.CheckPhVenderExists(af.AGEN_BANK_14))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>銀行代碼</span>存在於廠商基本檔，無法刪除。";
                        return session.Result;
                    }
                    if (repo.CheckExists(af.AGEN_BANK_14) == false)
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>銀行代碼</span>不存在，無法刪除。";
                        return session.Result;
                    }

                    session.Result.afrs = repo.Delete(af.AGEN_BANK_14);

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetExcelExample(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    HA0003Repository repo = new HA0003Repository(DBWork);
                    JCLib.Excel.Export("銀行資料維護作業.xls", repo.GetExcelExample());
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //檢核用Class
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

        //匯入檢核
        [HttpPost]
        public ApiResponse SendExcel()
        {
            /**
             * 
             * 以下為 匯入彈出視窗用到的 Controllers
             * 
             */
            List<HeaderItem> headerItems = new List<HeaderItem>() {
                new HeaderItem("銀行代碼", "AGEN_BANK_14"),
                new HeaderItem("銀行名稱", "BANKNAME"),
            };

            using (WorkSession session = new WorkSession(this))
            {
                List<PH_BANK_AF> list = new List<PH_BANK_AF>();
                List<PH_BANK_AF> final_list = new List<PH_BANK_AF>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                IWorkbook workBook;
                try
                {
                    bool checkPassed = true; //檢核有沒有通過 有通過就true 沒通過就false

                    #region 檢查檔案格式
                    if (Path.GetExtension(HttpPostedFile.FileName).ToLower() == ".xls")
                    {
                        workBook = new HSSFWorkbook(HttpPostedFile.InputStream); //讀取xls檔
                    }
                    else
                    {
                        workBook = new XSSFWorkbook(HttpPostedFile.InputStream); //讀取xlsx檔
                    }

                    var sheet = workBook.GetSheetAt(0); //讀取EXCEL的第一個分頁
                    IRow headerRow = sheet.GetRow(0);   //由第一列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum; //欄位數目
                    //int i, j;

                    #endregion

                    // 確定該有的欄位都有，沒有的回傳錯誤訊息
                    #region check excel header
                    headerItems = SetHeaderIndex(headerItems, headerRow);
                    string errMsg = string.Empty;
                    foreach (HeaderItem item in headerItems)
                    {
                        if (item.Index == -1)
                        {
                            if (errMsg == string.Empty)
                            {
                                errMsg += item.Name;
                            }
                            else
                            {
                                errMsg += string.Format("、{0}", item.Name);
                            }
                        }
                    }

                    if (errMsg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("欄位錯誤，缺：{0}", errMsg);
                        return session.Result;
                    }
                    # endregion

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        PH_BANK_AF temp = new PH_BANK_AF();
                        foreach (HeaderItem item in headerItems)
                        {

                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();

                            if (temp.GetType().GetProperty(item.FieldName).PropertyType == typeof(string))
                            {
                                temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                            }
                        }
                        list.Add(temp);
                    }

                    int j;
                    foreach (PH_BANK_AF item in list)
                    {
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        bool breakloop = false;

                        #region 檢查必填欄位
                        Dictionary<string, string> requiredCols;
                        // 必填欄位要有值
                        breakloop = false;
                        requiredCols = new Dictionary<string, string>{
                                { "AGEN_BANK_14", "銀行代碼" }, { "BANKNAME", "銀行名稱" }
                            };

                        foreach (KeyValuePair<string, string> mapItem in requiredCols)
                        {
                            var value = item.GetType().GetProperty(mapItem.Key).GetValue(item, null).ToString();

                            if (string.IsNullOrEmpty(value)) //檢查空白
                            {
                                item.CHECK_RESULT = mapItem.Value + " 為必填欄位";
                                checkPassed = false;
                                breakloop = true;
                                final_list.Add(item);
                                break;
                            }

                            if (mapItem.Key == "AGEN_BANK_14" && (value.Length == 4 || value.Length > 7 || !value.All(char.IsDigit))) //檢查銀行代碼
                            {
                                item.CHECK_RESULT = mapItem.Value + " 格式有誤";
                                checkPassed = false;
                                breakloop = true;
                                final_list.Add(item);
                                break;
                            }

                            //如果銀行代碼輸入值自動幫忙補0
                            if (mapItem.Key == "AGEN_BANK_14" && value.Length <= 3){
                                item.AGEN_BANK_14 = String.Format("{0:000}", Convert.ToInt32(value));
                            }
                            else if (mapItem.Key == "AGEN_BANK_14")
                            {
                                item.AGEN_BANK_14 = String.Format("{0:0000000}", Convert.ToInt32(value));
                            }
                        }
                        if (breakloop) continue;
                        #endregion


                        // 若無問題補上 OK
                        if (string.IsNullOrEmpty(item.CHECK_RESULT))
                        {
                            item.CHECK_RESULT = "OK";
                        }

                        final_list.Add(item);
                    }

                    session.Result.etts = final_list;
                    session.Result.msg = checkPassed.ToString();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //確認更新
        [HttpPost]
        public ApiResponse InsertFromXls(FormDataCollection formData)
        {
            /**
             * 
             * 以下為 匯入彈出視窗用到的 Controllers
             * 
             */
            List<HeaderItem> headerItems = new List<HeaderItem>() {
                 new HeaderItem("銀行代碼", "AGEN_BANK_14"),
                new HeaderItem("銀行名稱", "BANKNAME"),
            };

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<PH_BANK_AF> af_historys = JsonConvert.DeserializeObject<IEnumerable<PH_BANK_AF>>(formData["data"]);

                try
                {
                    var repo = new HA0003Repository(DBWork);

                    foreach (PH_BANK_AF af in af_historys)
                    {
                        af.CREATE_USER = User.Identity.Name;
                        af.UPDATE_USER = User.Identity.Name;
                        af.UPDATE_IP = DBWork.ProcIP;

                        //判斷資料要進行新增還是更新
                        if (repo.CheckExists(af.AGEN_BANK_14))
                        {
                            //存在則進行更新
                            af.UPDATE_USER = DBWork.UserInfo.UserId;
                            af.UPDATE_IP = DBWork.ProcIP;
                            repo.Update(af);
                        }
                        else
                        {
                            //不存在進行新增
                            af.CREATE_USER = DBWork.UserInfo.UserId;
                            af.UPDATE_USER = DBWork.UserInfo.UserId;
                            af.UPDATE_IP = DBWork.ProcIP;
                            repo.Create(af);
                        }
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