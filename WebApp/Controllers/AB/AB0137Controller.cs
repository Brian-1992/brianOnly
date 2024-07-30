using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Web;
using NPOI.SS.UserModel;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using WebApp.Repository.AB;
using WebApp.Models.AB;


namespace WebApp.Controllers.AB
{
    public class AB0137Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0137Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            string PR_NO = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("院內碼");
                    dt.Columns.Add("本月消耗");

                    JCLib.Excel.Export("AB0137.xls", dt);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        public ApiResponse GetWh_noCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0137Repository(DBWork);
                    session.Result.etts = repo.GetWh_noCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SendExcel()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<AB0137> list = new List<AB0137>();
                List<AB0137> ori_list = new List<AB0137>();

                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                IWorkbook workBook;

                try
                {
                    AB0137Repository repo = new AB0137Repository(DBWork);
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
                    //IRow headerRow = sheet.GetRow(0); //由第一列取標題做為欄位名稱
                    IRow headerRow = sheet.GetRow(0); //由第二列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum; //欄位數目
                    //int i, j;

                    #region excel欄位檢查
                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("本月消耗", "USEQTY"),
                    };

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
                    #endregion
                    #region 資料轉成list
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        AB0137 temp = new AB0137();

                        if (errMsg == string.Empty)
                        {
                            foreach (HeaderItem item in headerItems)
                            {
                                string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                                temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                            }
                        }

                        temp.Seq = i - 1;
                        ori_list.Add(temp);
                    }
                    #endregion

                    #endregion





                    DataTable dtTable = new DataTable();
                    DataRow datarow = dtTable.NewRow();
                    string arrCheckResult = "";
                    int nullnum = 0; //判斷是否整列為空

                    //bool flowIdValid = repo.ChceckPrStatus(v_prno);
                    //if (flowIdValid == false)
                    //{
                    //    session.Result.msg = "申請單狀態已變更，請重新查詢";
                    //    session.Result.success = false;
                    //    return session.Result;
                    //}

                    var mappingData = repo.GetMmcodeData(ori_list);
                    Dictionary<string, Tuple<string, string, string>> dictionary = mappingData
                        .ToDictionary(
                            obj => obj.GetType().GetProperty("MMCODE").GetValue(obj).ToString(),
                            obj => new Tuple<string, string, string>(
                                obj.GetType().GetProperty("MMNAME_E").GetValue(obj).ToString(),
                                obj.GetType().GetProperty("MMNAME_C").GetValue(obj).ToString(),
                                obj.GetType().GetProperty("MAT_CLASS").GetValue(obj).ToString()
                            )
                        );

                    foreach (AB0137 data in ori_list)
                    {
                        if (dictionary.TryGetValue(data.MMCODE, out var value))
                        {
                            data.MMNAME_E = value.Item1;
                            data.MMNAME_C = value.Item2;
                            data.MAT_CLASS = value.Item3;
                        }
                    }

                    foreach (AB0137 data in ori_list)
                    {
                        data.CHECK_RESULT = "OK";

                        //資料是否被使用者填入更新值
                        bool dataUpdated = false;

                        //如果有任何一格不是空的
                        if (string.IsNullOrEmpty(data.MMCODE) == false /*&& string.IsNullOrEmpty(data.USEQTY) == false*/)
                        {
                            //表示有效資料
                            dataUpdated = true;
                        }
                        //整理資料
                        if (string.IsNullOrEmpty(data.USEQTY))
                        {
                            data.USEQTY = "0";
                        }


                        //若有待匯入資料
                        if (data.MMCODE.ToString() != "" && dataUpdated == true)
                        {

                            if (repo.CheckExistsMMCODE(data.MMCODE) != true)
                            {
                                data.CHECK_RESULT = "院內碼必須存在於藥材主檔";
                            }
                            else if (data.MAT_CLASS != "01")
                            {
                                data.CHECK_RESULT = "物料類別必須為01藥品";

                            }
                            else if (repo.CheckFlagMMCODE(data.MMCODE) != true)
                            {
                                data.CHECK_RESULT = "院內碼已全院停用";
                            }
                            else if (ori_list.Where(x => x.MMCODE == data.MMCODE).Select(x => x).Count() > 1)
                            {
                                data.CHECK_RESULT = "匯入院內碼重複";
                            }
                            else
                            {
                                int unitRate = repo.GetUnitRate(data.MMCODE);
                                int t;
                                if (int.TryParse(data.USEQTY, out t) == false)
                                {
                                    data.CHECK_RESULT = string.Format("本月消耗 {0} 需為數字", data.USEQTY);
                                }
                                else
                                {
                                    //t = int.Parse(data.USEQTY);
                                    //if (t <= 0)  // 申購單匯入時，數量小於等於0跳過不處理
                                    //{
                                    //    // mm_pr01_d.CHECK_RESULT = string.Format("申購量 {0} 需為整數且大於0", mm_pr01_d.PR_QTY);
                                    //    continue;
                                    //}
                                    //1121009新竹 檢查出貨單位 不卡關改提示(匯入無法)
                                    /*
                                    if (t % unitRate != 0)
                                    {
                                        mm_pr01_d.CHECK_RESULT = string.Format("申購量 {0} 不為出貨單位 {1} 倍數", mm_pr01_d.PR_QTY, unitRate);
                                    }*/
                                }


                                if (data.CHECK_RESULT == "OK")
                                {
                                    data.CHECK_RESULT = "通過";
                                };
                            }

                            if (data.CHECK_RESULT == "OK")
                            {
                                data.CHECK_RESULT = "通過";
                            }

                            if (data.CHECK_RESULT != "通過")
                            {
                                checkPassed = false;
                            }
                            //產生一筆資料
                            list.Add(data);
                        }
                    }

                    session.Result.etts = list;
                    session.Result.msg = checkPassed.ToString();

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 確認更新
        [HttpPost]
        public ApiResponse Insert(FormDataCollection formData)
        {
            var STOCKCODE = formData.Get("STOCKCODE");


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<AB0137> AB0137Data = JsonConvert.DeserializeObject<IEnumerable<AB0137>>(formData["data"]);


                List<AB0137> AB0137_list = new List<AB0137>();
                try
                {
                    var repo = new AB0137Repository(DBWork);

                    foreach (AB0137 data in AB0137Data)
                    {
                        try
                        {
                            data.STOCKCODE = STOCKCODE;
                            data.CREATE_USER = User.Identity.Name;
                            data.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.Insert(data);
                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                        AB0137_list.Add(data);
                        //}
                    }

                    session.Result.etts = AB0137_list;

                    DBWork.Commit();

                }
                catch (Exception ex)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        #region 匯入比對欄位設定
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
        #endregion


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
                    AB0137Repository repo = new AB0137Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
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
