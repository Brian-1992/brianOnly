using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Collections.Generic;
using System.Web;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using Newtonsoft.Json;

namespace WebApp.Controllers.AA
{
    public class AA0043Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0043Repository(DBWork);
                    var user_kind = repo.USER_KIND(User.Identity.Name);
                    if (user_kind == "S")
                    {
                        session.Result.etts = repo.GetAll_S(p0, p1, p2, p3, page, limit, sorters, User.Identity.Name);
                    }
                    else if (user_kind == "1")
                    {
                        session.Result.etts = repo.GetAll_1(p0, p1, p2, p3, page, limit, sorters, User.Identity.Name);
                    }
                    else
                    {
                        session.Result.etts = repo.GetAll(p0, p1, p2, p3, page, limit, sorters, User.Identity.Name);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 新增
        [HttpPost]
        public ApiResponse Create(MI_WLOCINV mi_wlocinv)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0043Repository(DBWork);
                    if (!repo.CheckExists(mi_wlocinv.WH_NO, mi_wlocinv.MMCODE, mi_wlocinv.STORE_LOC)) // 新增前檢查主鍵是否已存在
                    {
                        mi_wlocinv.CREATE_USER = User.Identity.Name;
                        mi_wlocinv.UPDATE_USER = User.Identity.Name;
                        mi_wlocinv.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(mi_wlocinv);
                        session.Result.etts = repo.Get(mi_wlocinv.WH_NO, mi_wlocinv.MMCODE, mi_wlocinv.STORE_LOC);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房代碼、院內碼及儲位代碼</span>重複，請重新輸入。";
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

        // 修改
        [HttpPost]
        public ApiResponse Update(MI_WLOCINV mi_wlocinv)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0043Repository(DBWork);
                    mi_wlocinv.UPDATE_USER = User.Identity.Name;
                    mi_wlocinv.UPDATE_IP = DBWork.ProcIP;
                    if (mi_wlocinv.STORE_LOC == mi_wlocinv.STORE_LOC_DISPLAY)
                    {
                        session.Result.afrs = repo.Update(mi_wlocinv);
                        session.Result.etts = repo.Get(mi_wlocinv.WH_NO, mi_wlocinv.MMCODE, mi_wlocinv.STORE_LOC);
                    }
                    else
                    {
                        if (!repo.CheckExists(mi_wlocinv.WH_NO, mi_wlocinv.MMCODE, mi_wlocinv.STORE_LOC)) // 新增前檢查主鍵是否已存在
                        {
                            session.Result.afrs = repo.Update(mi_wlocinv);
                            session.Result.etts = repo.Get(mi_wlocinv.WH_NO, mi_wlocinv.MMCODE, mi_wlocinv.STORE_LOC);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>庫房代碼、院內碼及儲位代碼</span>重複，請重新輸入。";
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

        // 刪除
        [HttpPost]
        public ApiResponse Delete(MI_WLOCINV mi_wlocinv)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0043Repository(DBWork);
                    if (repo.CheckExists(mi_wlocinv.WH_NO, mi_wlocinv.MMCODE, mi_wlocinv.STORE_LOC))
                    {
                        session.Result.afrs = repo.Delete(mi_wlocinv.WH_NO, mi_wlocinv.MMCODE, mi_wlocinv.STORE_LOC);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房儲位碼</span>不存在。";
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

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0043Repository repo = new AA0043Repository(DBWork);
                    var user_kind = repo.USER_KIND(User.Identity.Name);
                    if (user_kind == "S")
                    {
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel_S(p0, p1, User.Identity.Name));
                    }
                    else if (user_kind == "1")
                    {
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel_1(p0, p1, User.Identity.Name));
                    }
                    else
                    {
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, User.Identity.Name));
                    }

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

                List<MI_WLOCINV> list = new List<MI_WLOCINV>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                IWorkbook workBook;

                try
                {
                    AA0043Repository repo = new AA0043Repository(DBWork);
                    var user_kind = repo.USER_KIND(User.Identity.Name);

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
                    IRow headerRow = sheet.GetRow(0); //由第一列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum; //欄位數目
                    int i, j;


                    bool isValid = true;
                    string[] arr = { "庫房代碼", "庫房名稱", "院內碼", "中文品名", "英文品名", "儲位代碼", "儲位庫存數量", "總庫存數量", "備註" };

                    for (j = 0; j < cellCount; j++)
                    {
                        isValid = headerRow.GetCell(j) == null ? false : true;
                        if (!isValid)
                        {
                            session.Result.msg = "檔案格式不同, 請下載範本更新";
                            break;
                        }
                    }

                    //檢查檔案中欄位名稱是否符合
                    if (isValid)
                    {
                        for (i = 0; i < cellCount; i++)
                        {
                            isValid = headerRow.GetCell(i).ToString() == arr[i] ? true : false;
                            if (!isValid) { break; }
                        }
                    }

                    if (!isValid)
                    {
                        session.Result.msg = "檔案格式不同, 請下載範本更新";
                    }
                    #endregion


                    DataTable dtTable = new DataTable();
                    DataRow datarow = dtTable.NewRow();
                    string arrCheckResult = "";
                    int nullnum = 0; //判斷是否整列為空

                    if (isValid)
                    {
                        #region 建立DataTable
                        for (i = 0; i < cellCount; i++)
                        //以欄位文字為名新增欄位，此處全視為字串型別以求簡化
                        {
                            dtTable.Columns.Add(
                                  new DataColumn(headerRow.GetCell(i).StringCellValue));
                        }

                        dtTable.Columns.Add("檢核結果");
                        // dtTable.Columns.Add("檢查庫房代碼");

                        //略過第0列(標題列)，一直處理至最後一列
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
                                    if ((j != 1) && (j != 3) && (j != 4) && (j != 7) && (arrCheckResult == "OK"))
                                    {
                                        if (arrCheckResult == "OK")
                                        {
                                            arrCheckResult = "[" + arr[j] + "]不可空白;";
                                        }
                                        else
                                        {
                                            arrCheckResult = arrCheckResult + "[" + arr[j] + "]不可空白;";
                                        }
                                    }
                                }
                            }
                            if (nullnum != cellCount)
                            {
                                datarow[cellCount] = arrCheckResult;
                                dtTable.Rows.Add(datarow);
                            }
                        }

                        dtTable.DefaultView.Sort = "庫房代碼,院內碼,儲位代碼";
                        dtTable = dtTable.DefaultView.ToTable();
                        #endregion

                        # region 檢核每組庫房代碼+院內碼各條件
                        string wh_no_temp = "";
                        string mmcode_temp = "";
                        string store_loc_temp = "";
                        DataTable newTable = dtTable.Clone();
                        DataTable tempTable = dtTable.Clone();
                        double inv_qty_temp = 0;
                        double total_inv_qty = 0;

                        for (i = 0; i < dtTable.Rows.Count; i++)
                        {
                            bool check_wh_no;
                            bool check_mmcode;
                            if (dtTable.Rows[i]["檢核結果"].ToString() == "OK")
                            {
                                dtTable.Rows[i]["檢核結果"] = "";

                                #region 檢核各代碼是否存在(WH_NO/MMCODE/STORE_LOC)
                                //檢查該庫房代碼是否存在
                                if (!repo.CheckExistsWH_NO(dtTable.Rows[i]["庫房代碼"].ToString()))
                                {
                                    dtTable.Rows[i]["檢核結果"] = "該庫房代碼" + dtTable.Rows[i]["庫房代碼"].ToString() + "不存在";
                                    dtTable.Rows[i]["庫房名稱"] = "";
                                    newTable.ImportRow(dtTable.Rows[i]);
                                    if (i == (dtTable.Rows.Count - 1))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    dtTable.Rows[i]["庫房名稱"] = repo.GetWH_NAME(dtTable.Rows[i]["庫房代碼"].ToString());
                                }

                                //檢查該院內碼是否存在
                                if (!repo.CheckExistsMMCODE(dtTable.Rows[i]["院內碼"].ToString()))
                                {
                                    dtTable.Rows[i]["檢核結果"] = "該院內碼" + dtTable.Rows[i]["院內碼"].ToString() + "不存在";
                                    dtTable.Rows[i]["中文品名"] = "";
                                    dtTable.Rows[i]["英文品名"] = "";
                                    newTable.ImportRow(dtTable.Rows[i]);
                                    if (i == (dtTable.Rows.Count - 1))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    dtTable.Rows[i]["中文品名"] = repo.GetMMNAME_C(dtTable.Rows[i]["院內碼"].ToString());
                                    dtTable.Rows[i]["英文品名"] = repo.GetMMNAME_E(dtTable.Rows[i]["院內碼"].ToString());
                                }

                                //檢查該儲位代碼是否存在
                                if (!repo.CheckExistsSTORE_LOC(dtTable.Rows[i]["儲位代碼"].ToString()))
                                {
                                    dtTable.Rows[i]["檢核結果"] = "該儲位代碼" + dtTable.Rows[i]["儲位代碼"].ToString() + "不存在";
                                    newTable.ImportRow(dtTable.Rows[i]);
                                    if (i == (dtTable.Rows.Count - 1))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                #endregion

                                #region 檢核是否符合對應代碼(WH_NO/MMCODE/STORE_LOC)
                                //檢查此帳號是否存在此庫房代碼
                                if (user_kind == "S")
                                {
                                    check_wh_no = repo.CheckWH_NO_S(dtTable.Rows[i]["庫房代碼"].ToString(), User.Identity.Name);
                                }
                                else
                                {
                                    check_wh_no = repo.CheckWH_NO_1(dtTable.Rows[i]["庫房代碼"].ToString(), User.Identity.Name);
                                }
                                if (!check_wh_no)
                                {
                                    dtTable.Rows[i]["檢核結果"] = "此帳號(" + User.Identity.Name + ")無管理庫房代碼" + dtTable.Rows[i]["庫房代碼"].ToString() + "的權限";
                                    newTable.ImportRow(dtTable.Rows[i]);
                                    if (i == (dtTable.Rows.Count - 1))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                //檢查此庫房代碼是否存在此院內碼
                                if (user_kind == "S" || user_kind == "1")
                                {
                                    check_mmcode = repo.CheckMMCODE_S(dtTable.Rows[i]["庫房代碼"].ToString(), dtTable.Rows[i]["院內碼"].ToString());
                                }
                                else
                                {
                                    check_mmcode = repo.CheckMMCODE(dtTable.Rows[i]["庫房代碼"].ToString(), dtTable.Rows[i]["院內碼"].ToString(), User.Identity.Name);
                                }
                                if (!check_mmcode)
                                {
                                    dtTable.Rows[i]["檢核結果"] = "此庫房" + dtTable.Rows[i]["庫房代碼"].ToString() + "中無此院內碼" + dtTable.Rows[i]["院內碼"].ToString();
                                    newTable.ImportRow(dtTable.Rows[i]);
                                    if (i == (dtTable.Rows.Count - 1))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                //檢查此庫房代碼是否存在此儲位代碼
                                if (!repo.CheckSTORE_LOC(dtTable.Rows[i]["庫房代碼"].ToString(), dtTable.Rows[i]["儲位代碼"].ToString()))
                                {
                                    dtTable.Rows[i]["檢核結果"] = "此庫房" + dtTable.Rows[i]["庫房代碼"].ToString() + "中無此儲位代碼" + dtTable.Rows[i]["儲位代碼"].ToString();
                                    newTable.ImportRow(dtTable.Rows[i]);
                                    if (i == (dtTable.Rows.Count - 1))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                #endregion

                                if (wh_no_temp == "") //第一筆加入暫存datatable
                                {
                                    tempTable.ImportRow(dtTable.Rows[i]);
                                    wh_no_temp = tempTable.Rows[0]["庫房代碼"].ToString();
                                    mmcode_temp = tempTable.Rows[0]["院內碼"].ToString();
                                }
                                else //第二筆之後
                                {
                                    //若跟上筆為同一組庫房代碼+院內碼，加入暫存datatable
                                    if ((wh_no_temp == dtTable.Rows[i]["庫房代碼"].ToString()) &&
                                        (mmcode_temp == dtTable.Rows[i]["院內碼"].ToString()))
                                    {
                                        tempTable.ImportRow(dtTable.Rows[i]);
                                    }
                                    else //若為不同，先處理暫存table中的資料檢核，再清空後將新的一組加入
                                    {
                                        # region 分別檢核每組tempTable
                                        total_inv_qty = repo.GetQtySum(wh_no_temp, mmcode_temp);
                                        inv_qty_temp = 0;
                                        store_loc_temp = "";
                                        for (j = 0; j < tempTable.Rows.Count; j++)
                                        {
                                            if (store_loc_temp == tempTable.Rows[j]["儲位代碼"].ToString())
                                            {
                                                tempTable.Rows[j]["檢核結果"] = "此組庫房代碼、院內碼及儲位代碼已重複";
                                                newTable.ImportRow(tempTable.Rows[j]);
                                                tempTable.Rows.Remove(tempTable.Rows[j]);
                                                j = j - 1;
                                                if (j == (tempTable.Rows.Count - 1))
                                                {
                                                    break;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                inv_qty_temp = inv_qty_temp + double.Parse(tempTable.Rows[j]["儲位庫存數量"].ToString());
                                            }
                                            store_loc_temp = tempTable.Rows[j]["儲位代碼"].ToString();
                                        }
                                        if (total_inv_qty == inv_qty_temp)
                                        {
                                            for (j = 0; j < tempTable.Rows.Count; j++)
                                            {
                                                tempTable.Rows[j]["檢核結果"] = "檢核通過";
                                                newTable.ImportRow(tempTable.Rows[j]);
                                            }
                                        }
                                        else
                                        {
                                            for (j = 0; j < tempTable.Rows.Count; j++)
                                            {
                                                tempTable.Rows[j]["檢核結果"] = "儲位庫存數量累計 " + inv_qty_temp + " 與總庫存數量 " + total_inv_qty + " 不符";
                                                newTable.ImportRow(tempTable.Rows[j]);
                                            }
                                        }
                                        tempTable.Clear();
                                        tempTable.ImportRow(dtTable.Rows[i]);
                                        wh_no_temp = tempTable.Rows[0]["庫房代碼"].ToString();
                                        mmcode_temp = tempTable.Rows[0]["院內碼"].ToString();
                                        #endregion

                                    }
                                }
                            }
                            else
                            {
                                newTable.ImportRow(dtTable.Rows[i]);
                            }
                        }
                        #region 檢核最後一組tempTable
                        inv_qty_temp = 0;
                        store_loc_temp = "";
                        total_inv_qty = repo.GetQtySum(wh_no_temp, mmcode_temp);
                        for (j = 0; j < tempTable.Rows.Count; j++)
                        {
                            if (store_loc_temp == tempTable.Rows[j]["儲位代碼"].ToString())
                            {
                                tempTable.Rows[j]["檢核結果"] = "此組庫房代碼、院內碼及儲位代碼已重複";
                                newTable.ImportRow(tempTable.Rows[j]);
                                tempTable.Rows.Remove(tempTable.Rows[j]);
                                if (j == (tempTable.Rows.Count - 1))
                                {
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                inv_qty_temp = inv_qty_temp + double.Parse(tempTable.Rows[j]["儲位庫存數量"].ToString());
                            }
                            store_loc_temp = tempTable.Rows[j]["儲位代碼"].ToString();
                        }
                        if (total_inv_qty == inv_qty_temp)
                        {
                            for (j = 0; j < tempTable.Rows.Count; j++)
                            {
                                tempTable.Rows[j]["檢核結果"] = "檢核通過";
                                newTable.ImportRow(tempTable.Rows[j]);
                            }
                        }
                        else
                        {
                            for (j = 0; j < tempTable.Rows.Count; j++)
                            {
                                tempTable.Rows[j]["檢核結果"] = "儲位庫存數量累計 " + inv_qty_temp + " 與總庫存數量 " + total_inv_qty + " 不符";
                                newTable.ImportRow(tempTable.Rows[j]);
                            }
                        }
                        #endregion
                        #endregion

                        newTable.DefaultView.Sort = "庫房代碼,院內碼,儲位代碼";
                        newTable = newTable.DefaultView.ToTable();

                        //加入至MI_WLOCINV中
                        #region 加入至MI_WLOCINV中
                        for (i = 0; i < newTable.Rows.Count; i++)
                        {
                            MI_WLOCINV MI_WLOCINV = new MI_WLOCINV();
                            MI_WLOCINV.WH_NO = newTable.Rows[i]["庫房代碼"].ToString();
                            MI_WLOCINV.WH_NAME_DISPLAY = newTable.Rows[i]["庫房代碼"].ToString();
                            MI_WLOCINV.WH_NAME_TEXT = newTable.Rows[i]["庫房代碼"].ToString();
                            MI_WLOCINV.WH_NAME = newTable.Rows[i]["庫房名稱"].ToString();
                            MI_WLOCINV.MMCODE = newTable.Rows[i]["院內碼"].ToString();
                            MI_WLOCINV.MMCODE_TEXT = newTable.Rows[i]["院內碼"].ToString();
                            MI_WLOCINV.MMCODE_DISPLAY = newTable.Rows[i]["院內碼"].ToString();
                            MI_WLOCINV.MMNAME_C = newTable.Rows[i]["中文品名"].ToString();
                            MI_WLOCINV.MMNAME_E = newTable.Rows[i]["英文品名"].ToString();
                            MI_WLOCINV.STORE_LOC = newTable.Rows[i]["儲位代碼"].ToString();
                            MI_WLOCINV.STORE_LOC_DISPLAY = newTable.Rows[i]["儲位代碼"].ToString();
                            MI_WLOCINV.STORE_LOC_TEXT = newTable.Rows[i]["儲位代碼"].ToString();
                            MI_WLOCINV.INV_QTY = double.Parse(newTable.Rows[i]["儲位庫存數量"].ToString() == "" ? "0" : newTable.Rows[i]["儲位庫存數量"].ToString());
                            MI_WLOCINV.LOC_NOTE = newTable.Rows[i]["備註"].ToString();
                            MI_WLOCINV.CHECK_RESULT = newTable.Rows[i]["檢核結果"].ToString();
                            MI_WLOCINV.IMPORT_RESULT = "尚未上傳更新";
                            list.Add(MI_WLOCINV);
                        }
                        #endregion
                    }
                    if (!isValid)
                    {
                        session.Result.success = false;
                    }
                    else
                    {
                        session.Result.etts = list;
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //上傳更新
        [HttpPost]
        public ApiResponse Import(FormDataCollection formData)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                IEnumerable<MI_WLOCINV> MI_WLOCINV = JsonConvert.DeserializeObject<IEnumerable<MI_WLOCINV>>(formData["data"]);
                List<MI_WLOCINV> list_MI_WLOCINV = new List<MI_WLOCINV>();
                var wh_no_temp = "";
                var mmcode_temp = "";
                DBWork.BeginTransaction();

                try
                {
                    var repo = new AA0043Repository(DBWork);

                    foreach (MI_WLOCINV MI_WLOCINV_1 in MI_WLOCINV)
                    {
                        MI_WLOCINV_1.CREATE_USER = User.Identity.Name;
                        MI_WLOCINV_1.UPDATE_IP = DBWork.ProcIP;
                        MI_WLOCINV_1.UPDATE_USER = DBWork.ProcUser;
                        MI_WLOCINV new_MI_WLOCINV = new MI_WLOCINV();
                        new_MI_WLOCINV = MI_WLOCINV_1;

                        if (MI_WLOCINV_1.CHECK_RESULT == "檢核通過")
                        {
                            if (wh_no_temp == "")
                            {
                                wh_no_temp = MI_WLOCINV_1.WH_NO;
                                mmcode_temp = MI_WLOCINV_1.MMCODE;
                                repo.Import_Delete(wh_no_temp, mmcode_temp);
                            }

                            //檢查結果是否OK

                            try
                            {
                                if ((MI_WLOCINV_1.WH_NO != wh_no_temp) || (MI_WLOCINV_1.MMCODE != mmcode_temp))
                                {
                                    wh_no_temp = MI_WLOCINV_1.WH_NO;
                                    mmcode_temp = MI_WLOCINV_1.MMCODE;
                                    repo.Import_Delete(wh_no_temp, mmcode_temp);
                                }
                                repo.Import_Create(MI_WLOCINV_1);
                                //狀態
                                new_MI_WLOCINV.IMPORT_RESULT = "上傳更新成功";
                            }
                            catch
                            {
                                new_MI_WLOCINV.IMPORT_RESULT = "上傳更新失敗";
                            }
                        }
                        else
                        {
                            new_MI_WLOCINV.IMPORT_RESULT = "資料未上傳";
                        }

                        list_MI_WLOCINV.Add(new_MI_WLOCINV);

                    }

                    DBWork.Commit();
                    session.Result.etts = list_MI_WLOCINV;

                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        //庫房代碼下拉式選單
        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0043Repository(DBWork);
                    var user_kind = repo.USER_KIND(User.Identity.Name);
                    if (user_kind == "S")
                    {
                        session.Result.etts = repo.GetWhnoCombo_S(User.Identity.Name);
                    }
                    else if (user_kind == "1")
                    {
                        session.Result.etts = repo.GetWhnoCombo_1(User.Identity.Name);
                    }
                    else
                    {
                        session.Result.etts = repo.GetWhnoCombo_1(User.Identity.Name);
                    }

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {
            var WH_KIND = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0043Repository repo = new AA0043Repository(DBWork);
                    if (WH_KIND == "0")
                    {
                        session.Result.etts = repo.GetMatclass1Combo();
                    }
                    else
                    {
                        session.Result.etts = repo.GetMatclass23Combo();
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        //儲位代碼下拉式選單
        [HttpPost]
        public ApiResponse GetSTORE_LOC(FormDataCollection form)
        {
            var WH_NO = form.Get("WH_NO");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0043Repository(DBWork);
                    session.Result.etts = repo.GetSTORE_LOC(WH_NO);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //院內碼下拉式選單(搜尋)
        [HttpPost]
        public ApiResponse GetMMCodeCombo_Q(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0043Repository repo = new AA0043Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo_Q(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //院內碼下拉式選單(新增)
        [HttpPost]
        public ApiResponse GetMMCodeCombo_I(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0043Repository repo = new AA0043Repository(DBWork);
                    var user_kind = repo.USER_KIND(User.Identity.Name);
                    if (user_kind == "S")
                    {
                        session.Result.etts = repo.GetMMCodeCombo_I_S(p0, p1, page, limit, "");
                    }
                    else if (user_kind == "1")
                    {
                        session.Result.etts = repo.GetMMCodeCombo_I_S(p0, p1, page, limit, "");
                    }
                    else
                    {
                        session.Result.etts = repo.GetMMCodeCombo_I(p0, p1, User.Identity.Name, page, limit, "");
                    }

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //院內碼彈出式視窗(搜尋)
        [HttpPost]
        public ApiResponse GetMmcode_Q(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0043Repository repo = new AA0043Repository(DBWork);
                    AA0043Repository.MI_MAST_QUERY_PARAMS query = new AA0043Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    session.Result.etts = repo.GetMmcode_Q(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //院內碼彈出式視窗(新增)
        [HttpPost]
        public ApiResponse GetMmcode_I(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0043Repository repo = new AA0043Repository(DBWork);
                    AA0043Repository.MI_MAST_QUERY_PARAMS query = new AA0043Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    query.USER_NAME = User.Identity.Name;

                    var user_kind = repo.USER_KIND(User.Identity.Name);
                    if (user_kind == "S")
                    {
                        session.Result.etts = repo.GetMmcode_I_S(query, page, limit, sorters);
                    }
                    else if (user_kind == "1")
                    {
                        session.Result.etts = repo.GetMmcode_I_S(query, page, limit, sorters);
                    }
                    else
                    {
                        session.Result.etts = repo.GetMmcode_I(query, page, limit, sorters);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkMmcode(FormDataCollection form)
        {
            var p0 = form.Get("mmcode");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0043Repository(DBWork);
                    if (repo.chkMmcode(p0) > 0)
                        session.Result.msg = "T"; // 有找到院內碼
                    else
                        session.Result.msg = "F";
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