using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebAppVen.Repository.B;
using WebAppVen.Models;
using JCLib.DB;
using NPOI.SS.UserModel;
using System.Web;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using Newtonsoft.Json;

namespace WebAppVen.Controllers.B
{
    public class BH0002Controller : SiteBase.BaseApiController
    {

        [HttpPost]
        public ApiResponse Excel_1(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var TSGH = "";
            var AGEN_NO = "";
            if (form.Get("TSGH") != null) TSGH = form.Get("TSGH");
            if (form.Get("AGEN_NO") != null) AGEN_NO = form.Get("AGEN_NO");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    string str_UserID = DBWork.UserInfo.UserId;
                    if (TSGH == "Y")
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel_1(AGEN_NO, p0));
                    else
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel_1(str_UserID, p0));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse Excel_2(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p2 = form.Get("p2");
            var TSGH = "";
            var AGEN_NO = "";
            if (form.Get("TSGH") != null) TSGH = form.Get("TSGH");
            if (form.Get("AGEN_NO") != null) AGEN_NO = form.Get("AGEN_NO");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    string str_UserID = DBWork.UserInfo.UserId;
                    if (TSGH == "Y")
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel_2(AGEN_NO, p0));
                    else
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel_2(str_UserID, p0));
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

                List<WB_REPLY> list = new List<WB_REPLY>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                var po_no = HttpContext.Current.Request.Form["po_no"];
                var TSGH = "";
                if (HttpContext.Current.Request.Form["TSGH"] != null)
                    TSGH = HttpContext.Current.Request.Form["TSGH"];
                var AGEN_NO = "";
                if (HttpContext.Current.Request.Form["AGEN_NO"] != null)
                    AGEN_NO = HttpContext.Current.Request.Form["AGEN_NO"];
                IWorkbook workBook;
                var GEN = "N";
                if (po_no.IndexOf("GEN") > -1) GEN = "Y";
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
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
                    string[] arr = { "廠商編號", "藥品中文名稱", "藥品英文名稱", "預計交貨日", "交貨數量", "三總院內碼", "批號", "效期", "發票號碼", "發票日期", "備註" };
                    int[] dataLen = { 6, 0, 0, 8, 8, 13, 20, 8, 10, 8, 150 };

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
                            WB_REPLY wb_reply = new WB_REPLY();
                            string[] arrCheckResult = new string[cellCount];
                            string[] arrCellString = new string[cellCount];

                            IRow row = sheet.GetRow(i);
                            if (row is null) { continue; }
                            #region 資料驗證
                            string chkstr = "";
                            for (j = 0; j < cellCount; j++)
                            {
                                arrCellString[j] = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();
                                //string[] arr = { "廠商編號", "藥品中文名稱", "藥品英文名稱", "預計交貨日", "交貨數量", "三總院內碼", "批號", "效期", "發票號碼", "發票日期", "備註" };
                                if (j == 0 || j == 4 || j == 5 || j == 6 || j == 7)
                                {
                                    //    if (string.IsNullOrWhiteSpace(arrCellString[j]))
                                    //    {
                                    //        if (GEN == "N")
                                    //            arrCheckResult[j] = "[" + arr[j] + "]不可空白";
                                    //        else if (j == 0 || j == 4 || j == 5)
                                    //            arrCheckResult[j] = "[" + arr[j] + "]不可空白";
                                    //    }
                                    chkstr += arrCellString[j];
                                }
                            }
                            //過濾"廠商編號", "預計交貨日", "交貨數量", "交貨數量", "三總院內碼"都空白資料列
                            if (chkstr == "") { continue; }
                            //是否超過資料長度
                            for (j = 0; j < cellCount; j++)
                            {
                                if (j == 1 || j == 2)
                                { continue; }
                                if (arrCellString[j].Length > dataLen[j])
                                {
                                    arrCheckResult[j] = "[" + arr[j] + "]長度超出，最多" + dataLen[j] + "字";
                                }
                            }

                            //廠商代碼
                            if (TSGH != "Y")
                                AGEN_NO = DBWork.ProcUser;
                            if (arrCellString[0] != AGEN_NO)
                            {
                                arrCheckResult[0] = !string.IsNullOrEmpty(arrCheckResult[0]) ? arrCheckResult[0] : "[廠商代碼]" + arrCellString[0] + "與登入帳號不同";
                            }

                            //三總院內碼 是否存在
                            if (!repo.CheckMmcodeExists(po_no, arrCellString[5]))
                            {
                                arrCheckResult[2] = !string.IsNullOrEmpty(arrCheckResult[5]) ? arrCheckResult[5] : "[三總院內碼]" + arrCellString[5] + "不是此訂單品項";
                            }
                            //檢查[預計交貨日]是否空白及符合日期格式yyyymmdd
                            if (!DateTime.TryParseExact(arrCellString[3], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out dtDate))
                            {
                                arrCheckResult[3] = !string.IsNullOrEmpty(arrCheckResult[3]) ? arrCheckResult[3] : "[預計交貨日]" + arrCellString[3] + "不符合西元年yyyymmdd格式";
                            }
                            //交貨數量 0 時檢查[發票號碼][8],[發票日期][9]不可空白
                            if (int.TryParse(arrCellString[4], out int a))
                            {
                                if (int.Parse(arrCellString[4]) < 0)
                                {
                                    arrCheckResult[4] = !string.IsNullOrEmpty(arrCheckResult[4]) ? arrCheckResult[3] : "[交貨數量]" + arrCellString[4] + "不可小於0";
                                }
                                else if (int.Parse(arrCellString[4]) == 0 && (string.IsNullOrEmpty(arrCellString[8]) || string.IsNullOrEmpty(arrCellString[9])))
                                {
                                    arrCheckResult[4] = !string.IsNullOrEmpty(arrCheckResult[4]) ? arrCheckResult[3] : "[交貨數量]" + arrCellString[4] + "=0時，[發票號碼]、[發票日期]不可空白!";
                                }
                            }
                            else
                            {
                                arrCheckResult[4] = !string.IsNullOrEmpty(arrCheckResult[3]) ? arrCheckResult[4] : "[交貨數量]" + arrCellString[4] + "不是數字 !";
                            }

                            //借貨量
                            //if (int.TryParse(arrCellString[7], out int b))
                            //{
                            //    if (int.Parse(arrCellString[7]) <= 0)
                            //    {
                            //        arrCheckResult[7] = !string.IsNullOrEmpty(arrCheckResult[7]) ? arrCheckResult[7] : "[借貨量]" + arrCellString[7] + "不可小於0";
                            //    }
                            //}
                            //else
                            //{
                            //    arrCheckResult[7] = !string.IsNullOrEmpty(arrCheckResult[7]) ? arrCheckResult[7] : "[借貨量]" + arrCellString[7] + "不可小於0";
                            //}

                            //檢查是否要輸入[效期]及[批號], 交貨數量arrCellString[4]=0，一般物品 時可不輸入
                            if (arrCellString[4] != "0" && GEN == "N" && (arrCellString[6] == "" || arrCellString[7] == ""))
                            {
                                arrCheckResult[6] = !string.IsNullOrEmpty(arrCheckResult[6]) ? arrCheckResult[6] : "[批號],[效期]欄不可空白";
                            }

                            //檢查[效期]是否空白及符合日期格式yyyymmdd
                            if (!DateTime.TryParseExact(arrCellString[7], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out dtDate))
                            {
                                if (GEN == "N" && arrCellString[4] != "0")
                                    arrCheckResult[7] = !string.IsNullOrEmpty(arrCheckResult[7]) ? arrCheckResult[7] : "[效期]" + arrCellString[7] + "不符合西元年yyyymmdd格式";
                                else if (string.IsNullOrEmpty(arrCellString[7]) == false)  // 一般物品, 效期 not null
                                    arrCheckResult[7] = !string.IsNullOrEmpty(arrCheckResult[7]) ? arrCheckResult[7] : "[效期]" + arrCellString[7] + "不符合西元年yyyymmdd格式";

                            }

                            //檢查[發票號碼] 藥品需求可以空白
                            if (arrCellString[8].Length == 10)
                            {
                                if (!char.IsUpper(char.Parse(arrCellString[8].Substring(0, 1))) || !char.IsUpper(char.Parse(arrCellString[8].Substring(1, 1))) || !int.TryParse(arrCellString[8].Substring(2, 8), out int c))
                                { arrCheckResult[8] = !string.IsNullOrEmpty(arrCheckResult[8]) ? arrCheckResult[8] : "[發票號碼]" + arrCellString[8] + "格式錯誤"; }
                            }
                            else
                            {
                                arrCheckResult[8] = string.IsNullOrEmpty(arrCellString[8]) ? "" : "[發票號碼]" + arrCellString[8] + "格式錯誤";
                            }
                            //檢查[發票日期] 藥品需求可以空白
                            if (!string.IsNullOrEmpty(arrCellString[9]))
                            {
                                if (!DateTime.TryParseExact(arrCellString[9], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out dtDate))
                                {
                                    arrCheckResult[9] = !string.IsNullOrEmpty(arrCellString[9]) ? "" : "[發票日期]" + arrCellString[9] + "不符合西元年yyyymmdd格式";
                                }
                            }
                            //檢查是否為空白
                            for (j = 0; j < cellCount; j++)
                            {
                                //string[] arr = { "廠商編號", "藥品中文名稱", "藥品英文名稱", "預計交貨日", "交貨數量", "三總院內碼", "批號", "效期", "發票號碼", "發票日期", "備註" };
                                if (j == 0 || j == 3 || j == 4 || j == 5 || j == 6 || j == 7)
                                {
                                    if (string.IsNullOrWhiteSpace(arrCellString[j]))
                                    {
                                        if (GEN == "N")
                                        {
                                            if (arrCellString[4] == "0" && j != 6 && j != 7) //交貨數量 0 時檢查[批號][6],[批號][7]可空白
                                                arrCheckResult[j] += "[" + arr[j] + "]不可空白";
                                        }
                                        else if (j == 0 || j == 3 || j == 4 || j == 5)
                                            arrCheckResult[j] += "[" + arr[j] + "]不可空白";
                                    }
                                    chkstr += arrCellString[j];
                                }
                            }
                            #endregion


                            #region 備註
                            //廠商編號 AGEN_NO
                            //交貨批次 DNO ---1090723 刪除
                            //藥品中文名稱
                            //藥品英文名稱
                            //預計交貨日 DELI_DT
                            //交貨數量 INQTY
                            //三總院內碼 MMCODE
                            //借貨量 BW_SQTY  ---1090723 刪除
                            //批號 LOT_NO 
                            //效期 EXP_DATE
                            //發票號碼 INVOICE
                            //發票日期 INVOICE_DT
                            //備註 MEMO
                            #endregion
                            //DateTime defaultday = new DateTime(1912, 1, 1);
                            wb_reply.AGEN_NO = arrCellString[0];
                            //wb_reply.DNO = arrCellString[1];
                            wb_reply.DELI_DT = arrCellString[3];
                            wb_reply.INQTY = arrCellString[4];
                            wb_reply.MMCODE = arrCellString[5];
                            //wb_reply.BW_SQTY     =    arrCellString[7];
                            wb_reply.LOT_NO = arrCellString[6];
                            wb_reply.EXP_DATE = arrCellString[7];
                            wb_reply.INVOICE = arrCellString[8];
                            wb_reply.INVOICE_DT = arrCellString[9];
                            wb_reply.MEMO = arrCellString[10];
                            DateTime defaultday = new DateTime(1912, 1, 1);

                            wb_reply.SEQ = "0";
                            wb_reply.CREATE_TIME = defaultday.ToString();
                            wb_reply.UPDATE_TIME = defaultday.ToString();
                            wb_reply.PO_NO = po_no;


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

                            if (checkResultStr == "OK") {
                                DateTime d = new DateTime();
                                if (DateTime.TryParseExact(wb_reply.EXP_DATE, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out d))
                                {
                                    string maxExpDate = DateTime.Now.AddYears(10).ToString("yyyyMMdd");
                                    if (DateTime.ParseExact(wb_reply.EXP_DATE, "yyyyMMdd", null) > DateTime.ParseExact(maxExpDate, "yyyyMMdd", null))
                                    {
                                        wb_reply.EXP_DATE = maxExpDate;
                                        checkResultStr += "(效期已改為最大值)";
                                    }
                                }
                            }

                            wb_reply.CHECK_RESULT = checkResultStr;
                            list.Add(wb_reply);
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
                IEnumerable<WB_REPLY> wb_reply = JsonConvert.DeserializeObject<IEnumerable<WB_REPLY>>(formData["data"]);
                List<WB_REPLY> list_wb_reply = new List<WB_REPLY>();
                DBWork.BeginTransaction();
                try
                {
                    //匯入前先取得[交貨批次]
                    var repo = new BH0002Repository(DBWork);

                    // 檢查訂單是否為目前登入廠商
                    // formData.Get("PO_NO")
                    if (repo.CheckPonoAgaenno(formData.Get("PO_NO"), DBWork.UserInfo.UserId) == false) {
                        session.Result.success = false;
                        session.Result.msg = "此訂單號碼非您的訂單，請重新確認";
                        return session.Result;
                    }

                    var DNO = repo.GetMaxDno(formData.Get("PO_NO"));
                    var IMPORT_ERROR = "";
                    foreach (WB_REPLY data in wb_reply)
                    {
                        data.CREATE_USER = User.Identity.Name;
                        data.UPDATE_IP = DBWork.ProcIP;
                        data.UPDATE_USER = DBWork.ProcUser;
                        data.IMPORT_RESULT = "";
                        data.STATUS = "";
                        data.FLAG = "";
                        data.DNO = DNO;
                        //檢查結果是否OK
                        if (data.CHECK_RESULT.Substring(0, 2) == "OK")
                        {
                            try
                            {
                                data.STATUS = "A";
                                data.FLAG = "A";
                                if (repo.IsExist(data) == 0)
                                {
                                    if (data.INVOICE_DT == "") data.INVOICE_DT = null;
                                    repo.ImportCreate(data);
                                    data.IMPORT_RESULT = "匯入成功";
                                    data.STATUS = "處理中";
                                }
                                else
                                {
                                    IMPORT_ERROR += data.MMCODE + "-" + data.LOT_NO + ":批號/效期資料重複,未上傳</br>";
                                    data.IMPORT_RESULT += data.MMCODE + "-" + data.LOT_NO + ":批號/效期資料重複,未上傳";
                                    data.STATUS = "";
                                }
                            }
                            catch (Exception ex)
                            {
                                IMPORT_ERROR += data.MMCODE + ":匯入失敗</br>";
                                data.IMPORT_RESULT = "匯入失敗";
                                data.STATUS = "";
                            }
                        }
                        else
                        {
                            IMPORT_ERROR += data.MMCODE + ":資料未上傳</br>";
                            data.IMPORT_RESULT = "資料未上傳";
                        }

                        list_wb_reply.Add(data);

                    }

                    DBWork.Commit();
                    session.Result.msg = DNO + "@" + IMPORT_ERROR;
                    session.Result.etts = list_wb_reply;

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
        public ApiResponse GetPoCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    session.Result.etts = repo.GetPoCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmcodeCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo(form.Get("PO_NO"));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetPoMaster(FormDataCollection form)
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
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    session.Result.etts = repo.GetPoMaster(form.Get("PO_NO"), User.Identity.Name, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetPoDetail(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            var TSGH = "";
            var AGEN_NO = "";
            if (form.Get("TSGH") != null) TSGH = form.Get("TSGH");
            if (form.Get("AGEN_NO") != null) AGEN_NO = form.Get("AGEN_NO");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    if (TSGH != "Y")
                        AGEN_NO = User.Identity.Name;
                    session.Result.etts = repo.GetPoDetail(form.Get("PO_NO"), form.Get("MMCODE"), AGEN_NO, form.Get("DNO"), page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 交貨新增
        [HttpPost]
        public ApiResponse CreateAll(FormDataCollection form)
        {
            var TSGH = "";
            var AGEN_NO = "";
            if (form.Get("TSGH") != null) TSGH = form.Get("TSGH");
            if (form.Get("AGEN_NO") != null) AGEN_NO = form.Get("AGEN_NO");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    WB_REPLY wb_reply = new WB_REPLY();
                    wb_reply.PO_NO = form.Get("PO");
                    wb_reply.DNO = repo.GetMaxDno(wb_reply.PO_NO);
                    if (TSGH != "Y")
                        wb_reply.AGEN_NO = User.Identity.Name;
                    else
                        wb_reply.AGEN_NO = AGEN_NO;
                    wb_reply.CREATE_USER = User.Identity.Name;
                    wb_reply.UPDATE_USER = User.Identity.Name;
                    wb_reply.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.CreateAll(wb_reply);

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
        public ApiResponse CreateOne(FormDataCollection form)
        {
            var TSGH = "";
            var AGEN_NO = "";
            if (form.Get("TSGH") != null) TSGH = form.Get("TSGH");
            if (form.Get("AGEN_NO") != null) AGEN_NO = form.Get("AGEN_NO");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    WB_REPLY wb_reply = new WB_REPLY();
                    wb_reply.PO_NO = form.Get("PO_NO");
                    wb_reply.DNO = form.Get("DNO");
                    wb_reply.MMCODE = form.Get("MMCODE");
                    wb_reply.DELI_DT = form.Get("DELI_DT");
                    wb_reply.LOT_NO = form.Get("LOT_NO");
                    wb_reply.EXP_DATE = form.Get("EXP_DATE") == "" ? null : form.Get("EXP_DATE");
                    wb_reply.INQTY = form.Get("INQTY");
                    //wb_reply.BW_SQTY = form.Get("BW_SQTY");
                    wb_reply.INVOICE = form.Get("INVOICE");
                    wb_reply.INVOICE_DT = form.Get("INVOICE_DT") == "" ? null : form.Get("INVOICE_DT");
                    wb_reply.INVOICE_OLD = form.Get("INVOICE_OLD");
                    if (wb_reply.INVOICE_OLD == "") wb_reply.INVOICE_OLD = null;
                    //wb_reply.BARCODE = form.Get("BARCODE");
                    wb_reply.MEMO = form.Get("MEMO");
                    if (TSGH != "Y")
                        wb_reply.AGEN_NO = User.Identity.Name;
                    else
                        wb_reply.AGEN_NO = AGEN_NO;
                    wb_reply.CREATE_USER = User.Identity.Name;
                    wb_reply.UPDATE_USER = User.Identity.Name;
                    wb_reply.UPDATE_IP = DBWork.ProcIP;
                    //檢查[訂單數量]是否大於等於[交貨數量]
                    if (repo.ChkINQTY(wb_reply.PO_NO, wb_reply.MMCODE, Int32.Parse(wb_reply.INQTY)) == "Y")
                    {
                        session.Result.afrs = repo.CreateOne(wb_reply);
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.msg = "[交貨數量]累計超過[訂單數量],請檢查!";
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

        [HttpPost]
        public ApiResponse DetailUpdate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    WB_REPLY wb_reply = new WB_REPLY();
                    //wb_reply.PO_NO = form.Get("PO");
                    //wb_reply.DNO = repo.GetMaxDno(wb_reply.PO_NO);
                    //wb_reply.AGEN_NO = User.Identity.Name;
                    if (form.Get("DELI_DT") != null && form.Get("DELI_DT") != "")
                        wb_reply.DELI_DT = form.Get("DELI_DT").Split('T')[0];  // yyyy-mm-ddT00:00:00

                    if (form.Get("EXP_DATE") != null && form.Get("EXP_DATE") != "")
                        wb_reply.EXP_DATE = form.Get("EXP_DATE").Split('T')[0];  // yyyy-mm-ddT00:00:00
                    

                    if (form.Get("INVOICE_DT") != null && form.Get("INVOICE_DT") != "")
                        wb_reply.INVOICE_DT = form.Get("INVOICE_DT").Split('T')[0];  // yyyy-mm-ddT00:00:00

                    wb_reply.SEQ = form.Get("SEQ");
                    wb_reply.LOT_NO = form.Get("LOT_NO");
                    wb_reply.INQTY = form.Get("INQTY");
                    //wb_reply.BW_SQTY = form.Get("BW_SQTY");
                    wb_reply.INVOICE = form.Get("INVOICE");
                    wb_reply.INVOICE_OLD = form.Get("INVOICE_OLD");
                    if (wb_reply.INVOICE_OLD == "") wb_reply.INVOICE_OLD = null;
                    //wb_reply.BARCODE = form.Get("BARCODE");
                    wb_reply.MEMO = form.Get("MEMO");

                    wb_reply.UPDATE_USER = User.Identity.Name;
                    wb_reply.UPDATE_IP = DBWork.ProcIP;


                    session.Result.afrs = repo.DetailUpdate(wb_reply);

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
        public ApiResponse DetailDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] tmp_seq = seq.Split(',');
                    for (int i = 0; i < tmp_seq.Length; i++)
                        session.Result.afrs = repo.DetailDelete(tmp_seq[i]);
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
        public ApiResponse DetailSubmit(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {

                    BH0002Repository repo = new BH0002Repository(DBWork);

                    string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                    string inqty = form.Get("INQTY").Substring(0, form.Get("INQTY").Length - 1);
                    string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1);
                    string po_no = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1);
                    string deli_dt = form.Get("DELI_DT").Substring(0, form.Get("DELI_DT").Length - 1);
                    string[] tmp_seq = seq.Split(',');
                    string[] tmp_inqty = inqty.Split(',');
                    string[] tmp_mmcode = mmcode.Split(',');
                    string[] tmp_po_no = po_no.Split(',');
                    string[] tmp_deli_dt = deli_dt.Split(',');
                    int ChkInvoice = 1;
                    string ChkINQTY = "";
                    for (int i = 0; i < tmp_seq.Length; i++)
                    {
                        if (tmp_deli_dt[i] != "")
                        {
                            WB_REPLY wb_reply = new WB_REPLY();
                            wb_reply.UPDATE_USER = User.Identity.Name;
                            wb_reply.UPDATE_IP = DBWork.ProcIP;
                            wb_reply.SEQ = tmp_seq[i];
                            ChkINQTY = repo.ChkINQTY(tmp_seq[i]); //檢查[訂單數量]是否大於等於[交貨數量]
                            if (tmp_inqty[i] == "0")
                                ChkInvoice = repo.ChkInvoice(tmp_seq[i], tmp_po_no[i], tmp_mmcode[i]);
                            else
                                ChkInvoice = 1;
                            if (ChkInvoice > 0 && ChkINQTY == "Y")
                            {
                                if (tmp_inqty[i] == "0")
                                    session.Result.afrs = repo.DetailSubmit_INQTY0(wb_reply);
                                else if (tmp_po_no[i].IndexOf("INV") < 0 && tmp_po_no[i].IndexOf("GEN") < 0)  // 藥品一定要 批號、效期
                                    session.Result.afrs = repo.DetailSubmit_NONull(wb_reply);
                                else
                                    session.Result.afrs = repo.DetailSubmit(wb_reply);
                            }
                            else
                            {
                                if (ChkInvoice == 0)
                                    session.Result.msg += tmp_mmcode[i] + ":[舊發票資料]輸入錯誤!</br>";
                                if (ChkINQTY != "Y")
                                    session.Result.msg += tmp_mmcode[i] + ":[交貨數量]累計超過[訂單數量],請檢查!</br>";
                            }
                        }
                        else
                        {
                            session.Result.msg += tmp_mmcode[i] + ":[交貨日期]空白,請檢查!</br>";
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
        // for 三總使用者 TSGH
        [HttpPost]
        public ApiResponse GetAgennoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = 2000;
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetPoCombo_TSGH(FormDataCollection form)
        {
            var AGEN_NO = form.Get("AGEN_NO");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    session.Result.etts = repo.GetPoCombo_TSGH(AGEN_NO);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetPoMaster_TSGH(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            var AGEN_NO = form.Get("AGEN_NO");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0002Repository repo = new BH0002Repository(DBWork);
                    session.Result.etts = repo.GetPoMaster_TSGH(form.Get("PO_NO"), AGEN_NO, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region 2022-03-18 新增：取得效期最大值

        [HttpPost]
        public ApiResponse GetMaxExpDate()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    string max_date = DateTime.Now.AddYears(10).ToString("yyyy/MM/dd");
                    session.Result.msg = max_date;
                    session.Result.success = true;
                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }
        }
        #endregion
    }
}
