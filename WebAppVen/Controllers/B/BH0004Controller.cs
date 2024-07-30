using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebAppVen.Repository.BH;
using WebAppVen.Models;
using System;
using Newtonsoft.Json;
using System.Web;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;

namespace WebAppVen.Controllers.BH
{
    public class BH0004Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BH0004Repository(DBWork);
                    session.Result.etts = repo.GetAll(DBWork.ProcUser, p1, p2, p3, p4, p5, p6, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //新增
        [HttpPost]
        public ApiResponse Create(WB_AIRHIS wb_airhis)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BH0004Repository(DBWork);

                    wb_airhis.CREATE_USER = User.Identity.Name;
                    wb_airhis.UPDATE_IP = DBWork.ProcIP;
                    wb_airhis.AGEN_NO = DBWork.ProcUser;
                    wb_airhis.UPDATE_USER = DBWork.ProcUser;
                    object[] Valid = CreateValid(wb_airhis, DBWork); //新增前檢查


                    if ((bool)Valid[0])
                    {
                        session.Result.afrs = repo.Create(wb_airhis);
                        session.Result.etts = repo.Get(wb_airhis);
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = Valid[1].ToString();
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

        public object[] CreateValid(WB_AIRHIS wb_airhis, IUnitOfWork DBWork)
        {
            #region 資料驗證
            var repo = new BH0004Repository(DBWork);
            object[] result = new object[2];

            result[0] = true;

            //// 檢查院內碼是否已存在
            ////if (!repo.CheckMmcodeExists(wb_airhis.MMCODE))
            ////{
            ////    result[0] = false;
            ////    result[1] = "[院內碼]" + wb_airhis.MMCODE + "不存在";
            ////    return result;
            ////}

            ////放置地點
            //if (!repo.CheckDeptExists(wb_airhis.DEPT))
            //{
            //    result[0] = false;
            //    result[1] = "[放置地點]" + wb_airhis.DEPT + "不存在";
            //    return result;
            //}

            //瓶號

            bool FBNO_exists = repo.CheckFbnoExists(wb_airhis.FBNO);
            switch (wb_airhis.EXTYPE)
            {
                case "GO":
                    if (!FBNO_exists)
                    {
                        result[0] = false;
                        result[1] = "[瓶號]" + wb_airhis.FBNO + "目前沒有放置於三總院內,請修改";
                        return result;
                    }
                    break;
                case "GI":
                    if (FBNO_exists)
                    {
                        result[0] = false;
                        result[1] = "[瓶號]" + wb_airhis.FBNO + "目前已放置放置於三總院內,請修改";
                        return result;
                    }
                    break;
            }
            #endregion

            return result;
        }

        // 修改
        [HttpPost]
        public ApiResponse Update(WB_AIRHIS wb_airhis)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BH0004Repository(DBWork);
                    wb_airhis.UPDATE_USER = User.Identity.Name;
                    wb_airhis.AGEN_NO = DBWork.ProcUser;
                    wb_airhis.UPDATE_IP = DBWork.ProcIP;

                    object[] Valid = CreateValid(wb_airhis, DBWork); //新增前檢查


                    if ((bool)Valid[0])
                    {
                        session.Result.afrs = repo.Update(wb_airhis);
                        session.Result.etts = repo.Get_U(wb_airhis);
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = Valid[1].ToString();
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

        // 刪除
        [HttpPost]
        public ApiResponse Delete(WB_AIRHIS wb_airhis)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                wb_airhis.AGEN_NO = DBWork.ProcUser;
                try
                {
                    var repo = new BH0004Repository(DBWork);
                    if (repo.CheckExists(wb_airhis))
                    {
                        session.Result.afrs = repo.Delete(wb_airhis);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此筆資料</span>不存在。";
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

        public ApiResponse GetDeptCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BH0004Repository(DBWork);
                    session.Result.etts = repo.GetDeptCombo();
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
                    BH0004Repository repo = new BH0004Repository(DBWork);

                    HttpResponse res = HttpContext.Current.Response;

                    FileStream fs = new FileStream(HttpContext.Current.Server.MapPath("../../Scripts/B/廠商氣體鋼瓶上傳範本檔.xlsx"), FileMode.Open);

                    byte[] bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    fs.Close();

                    res.BufferOutput = false;

                    res.Clear();
                    res.ClearHeaders();
                    res.HeaderEncoding = System.Text.Encoding.Default;
                    res.ContentType = "application/octet-stream";
                    res.AddHeader("Content-Disposition",
                                "attachment; filename=" + HttpUtility.UrlEncode("廠商氣體鋼瓶上傳範本檔.xlsx", System.Text.Encoding.UTF8));
                    res.BinaryWrite(bytes);



                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                return session.Result;
            }
        }


        // 確認回傳
        [HttpPost]
        public ApiResponse confirmData(FormDataCollection formData)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<WB_AIRHIS> wb_airhis = JsonConvert.DeserializeObject<IEnumerable<WB_AIRHIS>>(formData["data"]);

                List<WB_AIRHIS> wb_airhis_list = new List<WB_AIRHIS>();
                try
                {
                    var repo = new BH0004Repository(DBWork);

                    foreach (WB_AIRHIS data in wb_airhis)
                    {
                        object[] Valid = CreateValid(data, DBWork); //新增前檢查
                        data.UPDATE_USER = User.Identity.Name;
                        data.AGEN_NO = DBWork.ProcUser;
                        data.UPDATE_IP = DBWork.ProcIP;

                        if ((bool)Valid[0])
                        {
                            try
                            {
                                repo.confirmData(data);
                                data.CHECK_RESULT = "傳入成功";
                                data.STATUS = "B";
                            }
                            catch
                            {
                                data.CHECK_RESULT = "<span style='color:red'>傳入失敗</span>";
                            }

                        }
                        else
                        {
                            data.CHECK_RESULT = "<span style='color:red'>" + Valid[1].ToString() + "</span>";
                        }

                        wb_airhis_list.Add(data);
                    }

                    session.Result.etts = wb_airhis_list;

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
        public ApiResponse SendExcel()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<WB_AIRHIS> list = new List<WB_AIRHIS>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                IWorkbook workBook;

                try
                {
                    BH0004Repository repo = new BH0004Repository(DBWork);


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
                    int i, j;
                    DateTime dtDate;
                    bool isValid = true;
                    string[] arr = { "廠商代碼", "更換日期", "品名", "瓶號", "更換類別", "尺寸" };
                    int[] dataLen = { 6, 8, 200,20,2,20 };

                    #region 檢查檔案格式
                    for (j = 0; j < cellCount; j++)
                    {
                        isValid = headerRow.GetCell(j) == null ? false : true;
                        if (!isValid)
                        {
                            session.Result.msg = "檔案格式不同, 請下載範本更新";
                            break;
                        }
                    }


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


                    if (isValid)
                    {
                        //略過第零列(標題列)，一直處理至最後一列
                        for (i = 1; i <= sheet.LastRowNum; i++)
                        {
                            WB_AIRHIS wB_AIRHIS = new WB_AIRHIS();
                            string[] arrCheckResult = new string[cellCount];
                            string[] arrCellString = new string[cellCount];

                            IRow row = sheet.GetRow(i);
                            if (row is null) { continue; }
                            #region 資料驗證
                            //是否為空白
                            for (j = 0; j < cellCount; j++)
                            {
                                arrCellString[j] = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();

                                if (string.IsNullOrWhiteSpace(arrCellString[j]))
                                {
                                    arrCheckResult[j] = "[" + arr[j] + "]不可空白";
                                }
                            }
                            //是否超過資料長度
                            for (j = 0; j < cellCount; j++)
                            {
                                if (arrCellString[j].Length > dataLen[j])
                                {
                                    arrCheckResult[j] = "[" + arr[j] + "]長度超出，最多" + dataLen[j] + "字";
                                }
                            }

                            //廠商代碼
                            if (arrCellString[0] != DBWork.ProcUser)
                            {
                                arrCheckResult[0] = !string.IsNullOrEmpty(arrCheckResult[0]) ? arrCheckResult[0] : "[廠商代碼]" + arrCellString[0] + "錯誤";
                            }

                            //更換日期
                            if (!DateTime.TryParseExact(arrCellString[1], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out dtDate))
                            {
                                arrCheckResult[1] = !string.IsNullOrEmpty(arrCheckResult[1]) ? arrCheckResult[1] : "[更換日期]" + arrCellString[1] + "不符合西元年 yyyymmdd";
                            }


                            //更換類別

                            string Extype = arrCellString[4];
                            if (Extype != "取走" && Extype != "換入")
                            {
                                arrCheckResult[4] = "[更換類別]不為[取走] 或 [換入]";
                            }

                            //瓶號
                            if (!string.IsNullOrEmpty(arrCellString[3])) //瓶號錯誤則跳過
                            {
                                bool FBNO_exists = repo.CheckFbnoExists(arrCellString[3].ToString());
                                switch (arrCellString[5])
                                {
                                    case "取走":
                                        if (!FBNO_exists)
                                        {
                                            arrCheckResult[5] = "[瓶號]" + arrCellString[3] + "目前沒有放置於三總院內,請修改";
                                        }
                                        break;
                                    case "換入":
                                        if (FBNO_exists)
                                        {
                                            arrCheckResult[5] = "[瓶號]" + arrCellString[3] + "目前已放置於三總院內,請修改";
                                        }
                                        break;
                                }
                            }
                            #endregion


                            #region 備註
                            //FBNO 瓶號
                            //SEQ 交易流水號
                            //TXTDAY 更換日期
                            //AGEN_NO 廠商碼
                            //EXTYPE 更換類別
                            //STATUS 狀態
                            //XSIZE 容量
                            #endregion
                            DateTime defaultday = new DateTime(1912, 1, 1);
                            wB_AIRHIS.AGEN_NO = arrCellString[0].ToString();
                            wB_AIRHIS.TXTDAY = string.IsNullOrEmpty(arrCheckResult[1]) ? DateTime.ParseExact(arrCellString[1].ToString(), "yyyyMMdd", null) : defaultday;
                            wB_AIRHIS.NAMEC = arrCellString[2] == null ? "" : arrCellString[2].ToString();
                            wB_AIRHIS.FBNO = arrCellString[3].ToString();
                            string EXTYPE = "";
                            switch (arrCellString[4].ToString())
                            {
                                case "取走":
                                    EXTYPE = "GO";
                                    break;
                                case "換入":
                                    EXTYPE = "GI";
                                    break;
                            }

                            wB_AIRHIS.EXTYPE = EXTYPE;
                            wB_AIRHIS.XSIZE = arrCellString[5].ToString();

                            string checkResultStr = "";
                            foreach (string result in arrCheckResult)
                            {
                                if (!string.IsNullOrEmpty(result))
                                    checkResultStr += result + "</br>";
                            }
                            if (checkResultStr.Length > 0)
                            {
                                //刪除最後的換行符號+紅色
                                checkResultStr = "<span style=color:red >" + checkResultStr.Substring(0, checkResultStr.Length - 5) + "</span>";
                            }
                            else
                            {
                                checkResultStr = "OK";
                            }

                            wB_AIRHIS.CHECK_RESULT = checkResultStr;
                            list.Add(wB_AIRHIS);
                        }
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
        
        //匯入
        [HttpPost]
        public ApiResponse Import(FormDataCollection formData)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                IEnumerable<WB_AIRHIS> wB_AIRHIS = JsonConvert.DeserializeObject<IEnumerable<WB_AIRHIS>>(formData["data"]);
                List<WB_AIRHIS> list_wb_airhis = new List<WB_AIRHIS>();
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BH0004Repository(DBWork);
                    foreach (WB_AIRHIS wb_airhis in wB_AIRHIS)
                    {
                        wb_airhis.CREATE_USER = User.Identity.Name;
                        wb_airhis.UPDATE_IP = DBWork.ProcIP;
                        wb_airhis.UPDATE_USER = DBWork.ProcUser;
                        WB_AIRHIS new_wb_airhis = new WB_AIRHIS();
                        new_wb_airhis = wb_airhis;

                        //檢查結果是否OK
                        if (wb_airhis.CHECK_RESULT == "OK")
                        {
                            wb_airhis.STATUS = "B";

                            try
                            {
                                object[] Valid = CreateValid(wb_airhis, DBWork); //新增前檢查

                                if ((bool)Valid[0])
                                {
                                    repo.Create(wb_airhis);
                                    new_wb_airhis.IMPORT_RESULT = "匯入成功";
                                }
                                else
                                {
                                    new_wb_airhis.IMPORT_RESULT = Valid[1].ToString();
                                    wb_airhis.STATUS = "";
                                }

                            }
                            catch (Exception ex)
                            {
                                new_wb_airhis.IMPORT_RESULT = "匯入失敗";
                                wb_airhis.STATUS = "";
                            }
                        }
                        else
                        {
                            new_wb_airhis.IMPORT_RESULT = "資料未上傳";
                        }

                        list_wb_airhis.Add(new_wb_airhis);

                    }

                    DBWork.Commit();
                    session.Result.etts = list_wb_airhis;

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
                    BH0004Repository repo = new BH0004Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public string GetDataTime()
        {
            string str = "";
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BH0004Repository(DBWork);
                    str = repo.GetDataTime();
                }
                catch
                {
                    throw;
                }
                return str;
            }
        }
    }


}