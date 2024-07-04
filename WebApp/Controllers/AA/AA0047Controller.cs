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
    public class AA0047Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            //var p1 = form.Get("p1");
            //var p2 = form.Get("p2");
            //var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0047Repository(DBWork);
                    //session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, page, limit, sorters);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters);
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
        public ApiResponse Create(MM_WHAPLDT mm_WHAPLDT)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0047Repository(DBWork);
                    var str_APPLY_DATE = mm_WHAPLDT.APPLY_YEAR_MONTH + mm_WHAPLDT.APPLY_DAY;

                    if (!repo.CheckExists(str_APPLY_DATE, mm_WHAPLDT.WH_NO)) // 新增前檢查主鍵是否已存在
                    {
                        mm_WHAPLDT.CREATE_USER = User.Identity.Name;
                        mm_WHAPLDT.UPDATE_USER = User.Identity.Name;
                        mm_WHAPLDT.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(mm_WHAPLDT);
                        session.Result.etts = repo.Get(str_APPLY_DATE, mm_WHAPLDT.WH_NO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>撥發日期與庫房代碼</span>重複，請重新輸入。";
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
        public ApiResponse Update(MM_WHAPLDT mm_WHAPLDT)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0047Repository(DBWork);
                    var str_APPLY_DATE = mm_WHAPLDT.APPLY_YEAR_MONTH + mm_WHAPLDT.APPLY_DAY;

                    if (!repo.CheckExists(str_APPLY_DATE, mm_WHAPLDT.WH_NO)) // 新增前檢查主鍵是否已存在
                    {
                        mm_WHAPLDT.UPDATE_USER = User.Identity.Name;
                        mm_WHAPLDT.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Update(mm_WHAPLDT);
                        session.Result.etts = repo.Get(str_APPLY_DATE, mm_WHAPLDT.WH_NO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>撥發日期與庫房代碼</span>重複，請重新輸入。";
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

        //刪除
        [HttpPost]
        public ApiResponse Delete(MM_WHAPLDT mm_WHAPLDT)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0047Repository(DBWork);
                    var str_APPLY_DATE = mm_WHAPLDT.APPLY_YEAR_MONTH + mm_WHAPLDT.APPLY_DAY;
                    if (repo.CheckExists(str_APPLY_DATE, mm_WHAPLDT.WH_NO))
                    {
                        session.Result.afrs = repo.Delete(str_APPLY_DATE, mm_WHAPLDT.WH_NO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>單位申請時間</span>不存在。";
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

        [HttpPost]
        public ApiResponse GetmiWhmastCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0047Repository repo = new AA0047Repository(DBWork);
                    session.Result.etts = repo.GetmiWhmastCombo();
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

                List<AA0047M> list = new List<AA0047M>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                IWorkbook workBook;

                try
                {
                    AA0047Repository repo = new AA0047Repository(DBWork);
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
                    int i, j;


                    bool isValid = true;
                    string[] arr = { "撥發年月", "撥發日", "庫房代碼" };

                    for (j = 0; j < cellCount; j++)
                    {
                        isValid = headerRow.GetCell(j) == null ? false : true;
                        if (!isValid)
                        {
                            session.Result.msg = "檔案格式不同，請下載範本來更新。";
                            break;
                        }
                    }

                    //檢查檔案中欄位名稱是否符合
                    if (isValid)
                    {
                        for (i = 0; i < cellCount; i++)
                        {
                            isValid = headerRow.GetCell(i).ToString() == arr[i] ? true : false;
                            if (!isValid)
                            {
                                break;
                            }
                        }
                    }

                    if (!isValid)
                    {
                        session.Result.msg = "檔案格式不同，請下載範本來更新。";
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
                        //dtTable.Columns.Add("檢查庫房代碼");

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
                            }
                            if (nullnum != cellCount)
                            {
                                datarow[cellCount] = arrCheckResult;
                                dtTable.Rows.Add(datarow);
                            }
                        }

                        dtTable.DefaultView.Sort = "撥發年月";
                        dtTable = dtTable.DefaultView.ToTable();
                        #endregion

                        DataTable newTable = dtTable.Clone();
                        newTable = dtTable;

                        //加入至AA0047M中
                        #region 加入至AA0047M中
                        for (i = 0; i < newTable.Rows.Count; i++)
                        {
                            AA0047M AA0047M = new AA0047M();

                            AA0047M.APPLY_YEAR_MONTH = newTable.Rows[i]["撥發年月"].ToString();
                            AA0047M.APPLY_DAY = newTable.Rows[i]["撥發日"].ToString();
                            AA0047M.WH_NO = newTable.Rows[i]["庫房代碼"].ToString();
                            AA0047M.APPLY_DATE = AA0047M.APPLY_YEAR_MONTH + AA0047M.APPLY_DAY;
                            //AA0047M.INV_POSTID_N = newTable.Rows[i]["是否盤盈虧(Y盤盈虧,N消耗)(新)"].ToString() == "" ? "" : newTable.Rows[i]["是否盤盈虧(Y盤盈虧,N消耗)(新)"].ToString();
                            AA0047M.CHECK_RESULT = newTable.Rows[i]["檢核結果"].ToString();
                            //AA0047M.IMPORT_RESULT = "尚未上傳更新";

                            //資料是否被使用者填入更新值
                            bool dataUpdated = false;

                            //如果有任何一格不是空的
                            if (
                                AA0047M.APPLY_YEAR_MONTH != "" ||
                                AA0047M.APPLY_DAY != "" ||
                                AA0047M.WH_NO != ""
                                )
                            {
                                //表示資料有被更新
                                dataUpdated = true;
                            }

                            //若庫房代碼不是空的且資料有更新過
                            if (newTable.Rows[i]["撥發年月"].ToString() != "" && dataUpdated == true)
                            {
                                //檢核庫房代碼
                                if (repo.CheckExistsWH_NO(AA0047M.WH_NO) != true)
                                {
                                    AA0047M.CHECK_RESULT = "庫房代碼不存在";
                                }
                                else if (repo.CheckExistsTWNDate(AA0047M.APPLY_YEAR_MONTH, AA0047M.APPLY_DAY) != true)
                                {
                                    AA0047M.CHECK_RESULT = "撥發日錯誤";
                                }
                                else if (repo.CheckExistsTWNMonth(AA0047M.APPLY_YEAR_MONTH, AA0047M.WH_NO) == true)
                                {
                                    AA0047M.CHECK_RESULT = "此庫房的撥發年月已存在";
                                }
                                else
                                {
                                    if (AA0047M.CHECK_RESULT == "OK")
                                    {
                                        AA0047M.CHECK_RESULT = "";
                                        if (AA0047M.APPLY_YEAR_MONTH != "")
                                        {
                                            if (AA0047M.APPLY_YEAR_MONTH.Length > 5)
                                            {
                                                AA0047M.CHECK_RESULT += "撥發年月太長";
                                                AA0047M.CHECK_RESULT += ", ";
                                            }
                                        }
                                        if (AA0047M.APPLY_DAY != "")
                                        {
                                            if (AA0047M.APPLY_DAY.Length > 2)
                                            {
                                                AA0047M.CHECK_RESULT += "撥發日太長";
                                                AA0047M.CHECK_RESULT += ", ";
                                            }
                                        }

                                        //刪除最後的逗點
                                        if (AA0047M.CHECK_RESULT == "")
                                        {
                                            AA0047M.CHECK_RESULT = "OK";
                                        }
                                        else
                                        {
                                            AA0047M.CHECK_RESULT = AA0047M.CHECK_RESULT.Substring(0, AA0047M.CHECK_RESULT.Length - 2);
                                        }
                                    };

                                }
                                if (AA0047M.CHECK_RESULT != "OK")
                                {
                                    checkPassed = false;
                                }
                                //產生一筆資料
                                list.Add(AA0047M);
                            }
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
                        session.Result.msg = checkPassed.ToString();
                    }
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
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0047Repository repo = new AA0047Repository(DBWork);
                    JCLib.Excel.Export("SAMPLE.xls", repo.GetExcel());
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
        public ApiResponse Insert(FormDataCollection formData)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<AA0047M> AA0047m = JsonConvert.DeserializeObject<IEnumerable<AA0047M>>(formData["data"]);

                List<string> checkDuplicate = new List<string>();

                List<AA0047M> AA0047m_list = new List<AA0047M>();
                try
                {
                    var repo = new AA0047Repository(DBWork);
                    bool isDuplicate = false;

                    foreach (AA0047M data in AA0047m)
                    {

                        data.UPDATE_USER = User.Identity.Name;
                        data.UPDATE_TIME = DateTime.Now;
                        data.UPDATE_IP = DBWork.ProcIP;

                        try
                        {
                            if (!repo.CheckExistsTWNMonth(data.APPLY_YEAR_MONTH, data.WH_NO))
                            {
                                repo.Insert2(data);
                            }
                            else
                            {
                                //repo.Update2(data);
                            }

                        }
                        catch
                        {
                            throw;
                        }
                        AA0047m_list.Add(data);
                    }

                    session.Result.etts = AA0047m_list;

                    if (isDuplicate == false)
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        DBWork.Rollback();
                    }
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

    }
}
