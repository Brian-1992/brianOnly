using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models.AA;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using Newtonsoft.Json;

namespace WebApp.Controllers.AA
{
    public class AA0175Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
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
                    var repo = new AA0175Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, page, limit, sorters);
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
                    AA0175Repository repo = new AA0175Repository(DBWork);
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
                    AA0175Repository repo = new AA0175Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        //庫房代碼
        [HttpPost]
        public ApiResponse GetWhnoCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0175Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMatClassSubCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AA0175Repository repo = new AA0175Repository(DBWork);
                    session.Result.etts = repo.GetMatClassSubCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterCreate(AA0175 input)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0175Repository repo = new AA0175Repository(DBWork);
                    input.CREATE_USER = User.Identity.Name;

                    if (repo.CheckMasterExists(input.WH_NO, input.MMCODE, input.STORE_LOC))
                        session.Result.msg = "已有重複的資料";
                    else
                        session.Result.msg = repo.MasterCreate(input);

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
        public ApiResponse MasterUpdate(AA0175 input)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0175Repository repo = new AA0175Repository(DBWork);
                    input.UPDATE_USER = User.Identity.Name;
                    input.UPDATE_IP = DBWork.ProcIP;

                    if (repo.CheckMasterExists(input.WH_NO, input.MMCODE, input.STORE_LOC))
                        session.Result.msg = "已有重複的資料";
                    else {
                        session.Result.afrs = repo.MasterDeleteOri(input);
                        session.Result.msg = repo.MasterCreate(input);
                    }
                        

                    //session.Result.afrs = repo.MasterUpdate(input);

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
        public ApiResponse MasterDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0175Repository repo = new AA0175Repository(DBWork);
                    if (form.Get("item") != "")
                    {
                        AA0175 input = new AA0175();
                        string tmp = form.Get("item").Replace("<br>", "^");
                        // 移除最後面的^
                        tmp = tmp.Substring(0, tmp.Length - 1);

                        string[] tmpList = tmp.Split('^');
                        for (int i = 0; i < tmpList.Length; i++)
                        {
                            string[] param = tmpList[i].Split(',');
                            input.WH_NO = param[0];
                            input.MMCODE = param[1];
                            input.STORE_LOC = param[2];
                            session.Result.afrs = repo.MasterDelete(input);
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

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0175Repository repo = new AA0175Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4));
                }
                catch(Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Upload() //[匯入]呼叫
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<AA0175> list = new List<AA0175>();
                UnitOfWork DBWork = session.UnitOfWork;

                try
                {
                    AA0175Repository repo = new AA0175Repository(DBWork);
                    AA0140Repository repo1 = new AA0140Repository(DBWork);

                    #region data upload
                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                        new HeaderItem("庫房代碼", "WH_NO"),
                        new HeaderItem("藥材代碼", "MMCODE"),
                        new HeaderItem("儲位位置", "STORE_LOC"),
                       // new HeaderItem("現存數量", "INV_QTY"),
                        new HeaderItem("備註", "LOC_NOTE")
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
                    // 確定該有的欄位都有，沒有的回傳錯誤訊息
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

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        AA0175 temp = new AA0175();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            if (item.FieldName == "INV_QTY")
                                temp.GetType().GetProperty(item.FieldName).SetValue(temp, Convert.ToInt32(value), null);
                            else
                                temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                        }
                        //temp.Seq = i - 1;
                        list.Add(temp);
                    }
                    #endregion

                    foreach (AA0175 item in list)
                    {
                        string msg = string.Empty;
                        string temp_msg = string.Empty;

                        item.SaveStatus = "Y";  // N:Error Y:Insert

                        // 確認院內碼是否存在於藥品基本檔
                        if (repo1.CheckExists_Mimast(item.MMCODE))
                        {
                            temp_msg = "院內碼不存在於藥品基本檔";
                            item.SaveStatus = "N";
                        }

                        if (repo.CheckExists_WHmast(item.WH_NO, item.MMCODE))
                        {
                            temp_msg = "庫房代碼不存在於庫房基本檔";
                            item.SaveStatus = "N";
                        }


                        //// 確認是否有數值欄位放文字或負值，有的話改0
                        //float x = 0;
                        //if (float.TryParse(item.INV_QTY.ToString(), out x) == false)
                        //{
                        //    temp_msg = "現存數量不得小於0";
                        //    msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                        //    item.INV_QTY = 0;
                        //}

                        item.UploadMsg = msg;
                    }

                    session.Result.etts = list;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UploadConfirm(FormDataCollection form)  //[確定上傳]呼叫
        {
            string itemString = form.Get("data");
            IEnumerable<AA0175> list = JsonConvert.DeserializeObject<IEnumerable<AA0175>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0175Repository repo = new AA0175Repository(DBWork);

                    foreach (AA0175 item in list)
                    {
                        item.CREATE_USER = User.Identity.Name;
                        item.UPDATE_USER = User.Identity.Name;
                        item.UPDATE_IP = DBWork.ProcIP;

                        if (repo.CheckMasterExists(item.WH_NO, item.MMCODE, item.STORE_LOC))
                            session.Result.afrs = repo.MasterUpdate(item);
                        else
                            session.Result.afrs = repo.CreateImport(item);
                    }

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
    }
}
