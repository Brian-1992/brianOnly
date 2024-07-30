using JCLib.DB;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
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
using WebApp.Models;
using WebApp.Models.MI;
using WebApp.Repository.AA;

namespace WebApp.Controllers.AA
{
    public class AA0178Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //物料類別
            var p1 = form.Get("p1"); //院內碼
            var p2 = form.Get("p2"); // 庫房類別
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0178Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, page, limit, sorters);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
        //匯出
        [HttpPost]
        public ApiResponse GetExcel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            string fileName = string.Format("藥局基準量維護_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0178Repository repo = new AA0178Repository(DBWork);
                    JCLib.Excel.Export(fileName, repo.GetExcel(p0, p1, p2));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //物料類別combo
        [HttpPost]
        public ApiResponse GetMatClassSubCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0178Repository repo = new AA0178Repository(DBWork);
                    session.Result.etts = repo.GetMatClassSubCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //院內碼combo
        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //院內碼smartquery
            var p1 = form.Get("p1"); //物料類別
            var page = int.Parse(form.Get("page"));
            var limit = int.Parse(form.Get("limit"));

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0178Repository repo = new AA0178Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH_NoCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0178Repository repo = new AA0178Repository(DBWork);
                    string USER_ID = DBWork.UserInfo.UserId;
                    session.Result.etts = repo.GetWH_NoCombo(USER_ID);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

        }

        //修改
        [HttpPost]
        public ApiResponse Update(FormDataCollection form)
        {
            var MMCODE = form.Get("MMCODE");
            var NOW_RO = form.Get("NOW_RO");
            var DIFF_PERC = form.Get("DIFF_PERC");
            var DAY_RO = form.Get("DAY_RO");
            var SAFE_QTY = form.Get("SAFE_QTY");
            var NORMAL_QTY = form.Get("NORMAL_QTY");
            var G34_MAX_APPQTY = form.Get("G34_MAX_APPQTY");
            var RO_TYPE = form.Get("RO_TYPE");
            var WH_NO = form.Get("WH_NO");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0178Repository(DBWork);
                    AA0178 AA0178 = new AA0178();
                    AA0178.MMCODE = MMCODE;
                    AA0178.NOW_RO = NOW_RO;
                    AA0178.DIFF_PERC = DIFF_PERC;
                    AA0178.DAY_RO = DAY_RO;
                    AA0178.SAFE_QTY = SAFE_QTY;
                    AA0178.NORMAL_QTY = NORMAL_QTY;
                    AA0178.G34_MAX_APPQTY = G34_MAX_APPQTY;
                    AA0178.RO_TYPE = RO_TYPE;
                    AA0178.WH_NO = WH_NO;

                    session.Result.afrs = repo.Update(AA0178);
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
        public ApiResponse UpdatePerc(FormDataCollection form)
        {
            var SAFE_PERC = form.Get("SAFE_PERC");
            var NORMAL_PERC = form.Get("NORMAL_PERC");
            var G34_PERC = form.Get("G34_PERC");
            var SAFE_QTY = form.Get("SAFE_QTY");
            var NORMAL_QTY = form.Get("NORMAL_QTY");
            var G34_MAX_APPQTY = form.Get("G34_MAX_APPQTY");

            var MMCODE = form.Get("MMCODE");
            var WH_NO = form.Get("WH_NO");
            var RO_TYPE = form.Get("RO_TYPE");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0178Repository(DBWork);
                    AA0178 AA0178 = new AA0178();
                    AA0178.SAFE_PERC = SAFE_PERC;
                    AA0178.NORMAL_PERC = NORMAL_PERC;
                    AA0178.G34_PERC = G34_PERC;
                    AA0178.SAFE_QTY = SAFE_QTY;
                    AA0178.NORMAL_QTY = NORMAL_QTY;
                    AA0178.G34_MAX_APPQTY = G34_MAX_APPQTY;

                    AA0178.MMCODE = MMCODE;
                    AA0178.WH_NO = WH_NO;
                    AA0178.RO_TYPE = RO_TYPE;

                    AA0178.UPDATE_USER = DBWork.UserInfo.UserId;
                    AA0178.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.UpdatePerc(AA0178);
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

        /**
          * ToolBar 匯入功能
          */
        [HttpPost]
        public ApiResponse GetExcelExample(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    string fileName = string.Format("匯入藥局基準量維護_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    AA0178Repository repo = new AA0178Repository(DBWork);
                    JCLib.Excel.Export(fileName, repo.GetExcelExample());
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
        public ApiResponse SendExcel()
        {
            using (WorkSession session = new WorkSession(this))
            {
                List<MI_BASERO_14> list = new List<MI_BASERO_14>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                IWorkbook workBook;

                try
                {
                    AA0178Repository repo = new AA0178Repository(DBWork);
                    bool checkPassed = true; //檢核有沒有通過 有通過就true 沒通過就false

                    #region 1. 檢查檔案格式
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
                    int i, j;

                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                        new HeaderItem("院內碼", "MMCODE"),
                        //new HeaderItem("設定庫房類別", "RO_WHTYPE"),
                        //new HeaderItem("基準量模式", "RO_TYPE"), 預設為 1
                        new HeaderItem("現用基準量", "NOW_RO"),
                        //new HeaderItem("日平均消耗10天", "DAY_USE_10"),
                        //new HeaderItem("日平均消耗14天", "DAY_USE_14"),
                        //new HeaderItem("日平均消耗90天", "DAY_USE_90"),
                        //new HeaderItem("前第一個月消耗", "MON_USE_1"),
                        //new HeaderItem("前第二個月消耗", "MON_USE_2"),
                        //new HeaderItem("前第三個月消耗", "MON_USE_3"),
                        //new HeaderItem("前第四個月消耗", "MON_USE_4"),
                        //new HeaderItem("前第五個月消耗", "MON_USE_5"),
                        //new HeaderItem("前第六個月消耗", "MON_USE_6"),
                        //new HeaderItem("三個月平均消耗量", "MON_AVG_USE_3"),
                        //new HeaderItem("六個月平均消耗量", "MON_AVG_USE_6"),
                        new HeaderItem("護理病房最大請領量", "G34_MAX_APPQTY"),
                        //new HeaderItem("供應中心最大請領量", "SUPPLY_MAX_APPQTY"),
                        //new HeaderItem("藥局請領最大量", "PHR_MAX_APPQTY"),
                        //new HeaderItem("戰備存量", "WAR_QTY"),
                        new HeaderItem("安全庫存量", "SAFE_QTY"),
                        new HeaderItem("正常庫存量", "NORMAL_QTY"),
                        new HeaderItem("誤差百分比", "DIFF_PERC"),
                        //new HeaderItem("安全存量比值百分比", "SAFE_PERC"),
                        //new HeaderItem("日基準量", "DAY_RO"),
                        //new HeaderItem("月基準量", "MON_RO"),
                        //new HeaderItem("護理病房最大請領RO比值", "G34_PERC"),
                        //new HeaderItem("供應中心最大請領RO比值", "SUPPLY_PERC"),
                        //new HeaderItem("藥局最大請領RO比值", "PHR_PERC"),
                        //new HeaderItem("正常存量RO比值", "NORMAL_PERC"),
                        //new HeaderItem("戰備存量RO比值", "WAR_PERC"),
                        //new HeaderItem("庫房代碼", "WH_NO"),
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

                    if (errMsg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("欄位錯誤，缺：{0}", errMsg);
                        return session.Result;
                    }
                    #endregion

                    DataTable dtTable = new DataTable();
                    DataRow datarow = dtTable.NewRow();
                    string arrCheckResult = "";
                    int nullnum = 0; //判斷是否整列為空

                    #region 2. 建立DataTable
                    for (i = 0; i < cellCount; i++)
                    //以欄位文字為名新增欄位，此處全視為字串型別以求簡化
                    {
                        dtTable.Columns.Add(
                              new DataColumn(headerRow.GetCell(i).StringCellValue));
                    }
                    dtTable.Columns.Add("檢核結果");

                    //略過第0列(標題列)，一直處理至最後一列
                    //for (i = 1; i <= sheet.LastRowNum; i++)
                    //略過第0列(說明列)和第1列(標題列)，一直處理至最後一列
                    for (i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        datarow = dtTable.NewRow();
                        arrCheckResult = "OK";
                        nullnum = 0;
                        //依先前取得的欄位數逐一設定欄位內容
                        for (j = 0; j < cellCount; j++)
                        {
                            if (row == null)
                            {
                                nullnum = cellCount;
                                break;
                            }
                            datarow[j] = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();

                            if (string.IsNullOrWhiteSpace(datarow[j].ToString()))
                            {
                                nullnum++;
                                //if ((j != 1) && (j != 3) && (j != 4) && (j != 7) && (arrCheckResult == "OK"))
                                //{
                                //    if (arrCheckResult == "OK")
                                //    {
                                //        arrCheckResult = "[" + arr[j] + "]不可空白;";
                                //    }
                                //    else
                                //    {
                                //        arrCheckResult = arrCheckResult + "[" + arr[j] + "]不可空白;";
                                //    }
                                //}
                            }
                        }
                        // 如果 nullnum 等於 cellCount 代表 excel 有隱藏的行
                        if (nullnum != cellCount)
                        {
                            datarow[cellCount] = arrCheckResult;
                            dtTable.Rows.Add(datarow);
                        }
                    }

                    dtTable.DefaultView.Sort = "院內碼";
                    dtTable = dtTable.DefaultView.ToTable();
                    #endregion

                    DataTable newTable = dtTable.Clone();
                    // 儲存 院內碼 的字串，如果 匯入時，此字串重複，直接過濾
                    List<string> checkList = new List<string>();

                    #region 3. 檢核每個 row 裡面 column 的條件
                    for (i = 0; i < dtTable.Rows.Count; i++)
                    {
                        string strCheck = dtTable.Rows[i]["院內碼"].ToString();

                        if (dtTable.Rows[i]["檢核結果"].ToString() == "OK")
                        {
                            // 檢查 院內碼 及 庫房
                            if (checkList.Contains(strCheck))
                            {
                                dtTable.Rows[i]["檢核結果"] = "該院內碼資料重複";
                                checkPassed = false;
                                //dtTable.Rows[i]["中文品名"] = "";
                                //dtTable.Rows[i]["英文品名"] = "";
                                newTable.ImportRow(dtTable.Rows[i]);
                                continue;
                            }
                            else
                            {
                                checkList.Add(strCheck);
                            }

                            //檢查該院內碼是否存在
                            if (!repo.CheckExistsMMCODE(dtTable.Rows[i]["院內碼"].ToString()))
                            {
                                dtTable.Rows[i]["檢核結果"] = "該院內碼: " + dtTable.Rows[i]["院內碼"].ToString() + " 不存在或有誤";
                                checkPassed = false;
                                //dtTable.Rows[i]["中文品名"] = "";
                                //dtTable.Rows[i]["英文品名"] = "";
                                newTable.ImportRow(dtTable.Rows[i]);
                                continue;
                            }

                            ////檢查該庫房代碼是否存在
                            //if (!repo.CheckExistsWH_NO(dtTable.Rows[i]["庫房代碼"].ToString()))
                            //{
                            //    dtTable.Rows[i]["檢核結果"] = "該庫房代碼" + dtTable.Rows[i]["庫房代碼"].ToString() + " 不存在或有誤";
                            //    checkPassed = false;
                            //    //dtTable.Rows[i]["庫房名稱"] = "";
                            //    newTable.ImportRow(dtTable.Rows[i]);
                            //    continue;

                            //}

                            // 檢查基準量模式
                            Dictionary<string, string> map = new Dictionary<string, string>{
                                { "日基準", "1" },
                                { "月基準", "2" },
                                { "自訂基準", "3" }
                            };
                            if (map.TryGetValue(dtTable.Rows[i]["基準量模式"].ToString(), out string strRo_type))
                            {
                                if (!repo.CheckExistsPARAM_D(strRo_type, "RO_TYPE"))
                                {
                                    dtTable.Rows[i]["檢核結果"] = "該基準量模式: " + dtTable.Rows[i]["基準量模式"].ToString() + " 不存在或有誤";
                                    checkPassed = false;
                                    newTable.ImportRow(dtTable.Rows[i]);
                                    continue;
                                }
                            }
                            else
                            {
                                dtTable.Rows[i]["檢核結果"] = "該基準量模式: " + dtTable.Rows[i]["基準量模式"].ToString() + " 不存在或有誤";
                                checkPassed = false;
                                newTable.ImportRow(dtTable.Rows[i]);
                                continue;
                            }

                            // check number columns
                            string[] colNames = {
                                "現用基準量",
                                //"日平均消耗10天", "日平均消耗14天", "日平均消耗90天", "前第一個月消耗", "前第二個月消耗",
                                //"前第三個月消耗", "前第四個月消耗", "前第五個月消耗", "前第六個月消耗", "三個月平均消耗量",
                                //"六個月平均消耗量",
                                "護理病房最大請領量",
                                //"供應中心最大請領量", "藥局請領最大量", "戰備存量",
                                "安全庫存量", "正常庫存量", "誤差百分比", 
                                // "安全存量比值百分比", "日基準量", "月基準量",
                                //"護理病房最大請領RO比值", "供應中心最大請領RO比值", "藥局最大請領RO比值", "正常存量RO比值", "戰備存量RO比值"
                            };

                            bool breakloop = false;
                            foreach (string strColName in colNames)
                            {
                                if (float.TryParse(dtTable.Rows[i][strColName].ToString(), out float fValue))
                                {
                                    dtTable.Rows[i][strColName] = fValue.ToString("0.00");
                                    if (fValue < 0)
                                    {
                                        dtTable.Rows[i]["檢核結果"] = "該" + strColName + ": " + dtTable.Rows[i][strColName].ToString() + " 不得小於0";
                                        checkPassed = false;
                                        breakloop = true;
                                    }
                                }
                                else
                                {
                                    dtTable.Rows[i]["檢核結果"] = "該" + strColName + ": " + dtTable.Rows[i][strColName].ToString() + " 不是數值";
                                    checkPassed = false;
                                    breakloop = true;
                                }

                                if (breakloop)
                                {
                                    newTable.ImportRow(dtTable.Rows[i]);
                                    continue;
                                }
                            }

                            newTable.ImportRow(dtTable.Rows[i]);
                        }
                        else
                        {
                            newTable.ImportRow(dtTable.Rows[i]);
                        }
                    }
                    #endregion

                    newTable.DefaultView.Sort = "院內碼";
                    newTable = newTable.DefaultView.ToTable();

                    //加入至 MI_BASERO_14 中
                    #region 4. 加入至 MI_BASERO_14 中
                    for (i = 0; i < newTable.Rows.Count; i++)
                    {
                        MI_BASERO_14 mi_base_14 = new MI_BASERO_14();

                        mi_base_14.MMCODE = newTable.Rows[i]["院內碼"].ToString();
                        mi_base_14.RO_TYPE = newTable.Rows[i]["基準量模式"].ToString();
                        mi_base_14.NOW_RO = newTable.Rows[i]["現用基準量"].ToString();
                        //mi_base_14.DAY_USE_10 = newTable.Rows[i]["日平均消耗10天"].ToString();
                        //mi_base_14.DAY_USE_14 = newTable.Rows[i]["日平均消耗14天"].ToString();
                        //mi_base_14.DAY_USE_90 = newTable.Rows[i]["日平均消耗90天"].ToString();
                        //mi_base_14.MON_USE_1 = newTable.Rows[i]["前第一個月消耗"].ToString();
                        //mi_base_14.MON_USE_2 = newTable.Rows[i]["前第二個月消耗"].ToString();
                        //mi_base_14.MON_USE_3 = newTable.Rows[i]["前第三個月消耗"].ToString();
                        //mi_base_14.MON_USE_4 = newTable.Rows[i]["前第四個月消耗"].ToString();
                        //mi_base_14.MON_USE_5 = newTable.Rows[i]["前第五個月消耗"].ToString();
                        //mi_base_14.MON_USE_6 = newTable.Rows[i]["前第六個月消耗"].ToString();
                        //mi_base_14.MON_AVG_USE_3 = newTable.Rows[i]["三個月平均消耗量"].ToString();
                        //mi_base_14.MON_AVG_USE_6 = newTable.Rows[i]["六個月平均消耗量"].ToString();
                        mi_base_14.G34_MAX_APPQTY = newTable.Rows[i]["護理病房最大請領量"].ToString();
                        //mi_base_14.SUPPLY_MAX_APPQTY = newTable.Rows[i]["供應中心最大請領量"].ToString();
                        //mi_base_14.PHR_MAX_APPQTY = newTable.Rows[i]["藥局請領最大量"].ToString();
                        //mi_base_14.WAR_QTY = newTable.Rows[i]["戰備存量"].ToString();
                        mi_base_14.SAFE_QTY = newTable.Rows[i]["安全庫存量"].ToString();
                        mi_base_14.NORMAL_QTY = newTable.Rows[i]["正常庫存量"].ToString();
                        mi_base_14.DIFF_PERC = newTable.Rows[i]["誤差百分比"].ToString();
                        //mi_base_14.SAFE_PERC = newTable.Rows[i]["安全存量比值百分比"].ToString();
                        //mi_base_14.DAY_RO = newTable.Rows[i]["日基準量"].ToString();
                        //mi_base_14.MON_RO = newTable.Rows[i]["月基準量"].ToString();
                        //mi_base_14.G34_PERC = newTable.Rows[i]["護理病房最大請領RO比值"].ToString();
                        //mi_base_14.SUPPLY_PERC = newTable.Rows[i]["供應中心最大請領RO比值"].ToString();
                        //mi_base_14.PHR_PERC = newTable.Rows[i]["藥局最大請領RO比值"].ToString();
                        //mi_base_14.NORMAL_PERC = newTable.Rows[i]["正常存量RO比值"].ToString();
                        //mi_base_14.WAR_PERC = newTable.Rows[i]["戰備存量RO比值"].ToString();
                        //mi_base_14.WH_NO = newTable.Rows[i]["庫房代碼"].ToString();

                        mi_base_14.CHECK_RESULT = newTable.Rows[i]["檢核結果"].ToString();

                        //產生一筆資料
                        list.Add(mi_base_14);
                    }
                    #endregion

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

        //確認更新
        [HttpPost]
        public ApiResponse InsertFromXls(FormDataCollection formData)
        {
            string P2 = formData.Get("P2");// 庫房代碼
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                IEnumerable<MI_BASERO_14> mi_basero_14List = JsonConvert.DeserializeObject<IEnumerable<MI_BASERO_14>>(formData["data"]);
                List<MI_BASERO_14> mi_hist_list = new List<MI_BASERO_14>();
                Dictionary<string, string> ro_typeMap = new Dictionary<string, string>{
                                { "日基準", "1" },
                                { "月基準", "2" },
                                { "自訂基準", "3" }
                            };

                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0178Repository(DBWork);
                    foreach (MI_BASERO_14 mi_basero_14 in mi_basero_14List)
                    {
                        mi_basero_14.RO_WHTYPE = "2";
                        mi_basero_14.WH_NO = P2;
                        mi_basero_14.CREATE_USER = User.Identity.Name;
                        mi_basero_14.UPDATE_USER = User.Identity.Name;
                        mi_basero_14.UPDATE_IP = DBWork.ProcIP;

                        // 檢查 院內碼 庫房 複合鍵 是否存在
                        if (repo.CheckExistsCompundKey(mi_basero_14.MMCODE, mi_basero_14.WH_NO))
                        {
                            MI_BASERO_14 oldMI_BASE_RO = repo.GetMI_BASERO_14(mi_basero_14.MMCODE, mi_basero_14.WH_NO);

                            oldMI_BASE_RO.NOW_RO = mi_basero_14.NOW_RO;
                            //oldMI_BASE_RO.DAY_USE_10 = mi_basero_14.DAY_USE_10;
                            //oldMI_BASE_RO.DAY_USE_14 = mi_basero_14.DAY_USE_14;
                            //oldMI_BASE_RO.DAY_USE_90 = mi_basero_14.DAY_USE_90;
                            //oldMI_BASE_RO.MON_USE_1 = mi_basero_14.MON_USE_1;
                            //oldMI_BASE_RO.MON_USE_2 = mi_basero_14.MON_USE_2;
                            //oldMI_BASE_RO.MON_USE_3 = mi_basero_14.MON_USE_3;
                            //oldMI_BASE_RO.MON_USE_4 = mi_basero_14.MON_USE_4;
                            //oldMI_BASE_RO.MON_USE_5 = mi_basero_14.MON_USE_5;
                            //oldMI_BASE_RO.MON_USE_6 = mi_basero_14.MON_USE_6;
                            //oldMI_BASE_RO.MON_AVG_USE_3 = mi_basero_14.MON_AVG_USE_3;
                            //oldMI_BASE_RO.MON_AVG_USE_6 = mi_basero_14.MON_AVG_USE_6;
                            //oldMI_BASE_RO.SUPPLY_MAX_APPQTY = mi_basero_14.SUPPLY_MAX_APPQTY;
                            //oldMI_BASE_RO.PHR_MAX_APPQTY = mi_basero_14.PHR_MAX_APPQTY;
                            //oldMI_BASE_RO.WAR_QTY = mi_basero_14.WAR_QTY;
                            if (double.TryParse(oldMI_BASE_RO.SAFE_PERC, out double dSAFE_PERC) && dSAFE_PERC > 0)
                            {
                                oldMI_BASE_RO.SAFE_QTY = (double.Parse(mi_basero_14.NOW_RO) * dSAFE_PERC).ToString();
                            }
                            else
                            {
                                oldMI_BASE_RO.SAFE_QTY = mi_basero_14.SAFE_QTY;
                            }

                            if (double.TryParse(oldMI_BASE_RO.NORMAL_PERC, out double dNORMAL_PERC) && dNORMAL_PERC > 0)
                            {
                                oldMI_BASE_RO.NORMAL_QTY = (double.Parse(mi_basero_14.NOW_RO) * dNORMAL_PERC).ToString();
                            }
                            else
                            {
                                oldMI_BASE_RO.NORMAL_QTY = mi_basero_14.NORMAL_QTY;
                            }

                            if (double.TryParse(oldMI_BASE_RO.G34_PERC, out double dG34_PERC) && dG34_PERC > 0)
                            {
                                oldMI_BASE_RO.G34_MAX_APPQTY = (double.Parse(mi_basero_14.NOW_RO) * dG34_PERC).ToString();
                            }
                            else
                            {
                                oldMI_BASE_RO.G34_MAX_APPQTY = mi_basero_14.G34_MAX_APPQTY;
                            }

                            oldMI_BASE_RO.DIFF_PERC = mi_basero_14.DIFF_PERC;
                            //oldMI_BASE_RO.SAFE_PERC = mi_basero_14.SAFE_PERC;
                            oldMI_BASE_RO.UPDATE_USER = User.Identity.Name;
                            oldMI_BASE_RO.UPDATE_IP = DBWork.ProcIP;

                            repo.UpdateFromXLS(oldMI_BASE_RO);
                        }
                        else
                        {
                            mi_basero_14.RO_TYPE = ro_typeMap[mi_basero_14.RO_TYPE];
                            mi_basero_14.SUPPLY_MAX_APPQTY = "0";
                            mi_basero_14.PHR_MAX_APPQTY = "0";
                            mi_basero_14.WAR_QTY = "0";
                            mi_basero_14.SAFE_PERC = "0";
                            mi_basero_14.DAY_RO = "0";
                            mi_basero_14.G34_PERC = "0";
                            mi_basero_14.MON_RO = "0";
                            mi_basero_14.NORMAL_PERC = "0";
                            mi_basero_14.PHR_PERC = "0";
                            mi_basero_14.SUPPLY_PERC = "0";
                            mi_basero_14.WAR_PERC = "0";

                            repo.InsertFromXLS(mi_basero_14);
                        }

                        mi_hist_list.Add(mi_basero_14);
                    }

                    session.Result.etts = mi_hist_list;

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