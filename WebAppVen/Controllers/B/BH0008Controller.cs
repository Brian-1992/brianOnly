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
    public class BH0008Controller : SiteBase.BaseApiController
    {
        //for 三總使用者
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p2 = form.Get("p2");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0008Repository repo = new BH0008Repository(DBWork);
                    string str_UserID = DBWork.UserInfo.UserId;
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(str_UserID, p0, p2));
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
                IWorkbook workBook;

                try
                {
                    BH0008Repository repo = new BH0008Repository(DBWork);


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
                    string[] arr = { "廠商編號", "交貨批次", "藥品中文名稱", "藥品英文名稱", "預計交貨日", "交貨數量", "三總院內碼",   "批號", "效期", "發票號碼", "發票日期", "備註" };
                    int[] dataLen = { 6, 8,0,0, 8, 8, 13, 20, 8, 10, 8,  150 };

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
                            //是否為空白
                            for (j = 0; j < cellCount; j++)
                            {
                                arrCellString[j] = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();
                                //string[] arr = { "廠商編號", "交貨批次", "藥品中文名稱", "藥品英文名稱", "預計交貨日", "交貨數量", "三總院內碼", "批號", "效期", "發票號碼", "發票日期", "備註" };

                                if (j == 0 || j == 1 || j == 5 || j == 6 )
                                {
                                    if (string.IsNullOrWhiteSpace(arrCellString[j]))
                                    {
                                        arrCheckResult[j] = "[" + arr[j] + "]不可空白";
                                    }
                                }
                            }
                            //是否超過資料長度
                            for (j = 0; j < cellCount; j++)
                            {
                                if (j == 2 || j == 3)
                                { continue; }
                                if (arrCellString[j].Length > dataLen[j])
                                {
                                    arrCheckResult[j] = "[" + arr[j] + "]長度超出，最多" + dataLen[j] + "字";
                                }
                            }

                            //廠商代碼
                            if (arrCellString[0] != DBWork.ProcUser)
                            {
                                arrCheckResult[0] = !string.IsNullOrEmpty(arrCheckResult[0]) ? arrCheckResult[0] : "[廠商代碼]" + arrCellString[0] + "與登入帳號不同";
                            }

                            //三總院內碼 是否存在
                            if (!repo.CheckMmcodeExists(po_no,  arrCellString[6]))
                            {
                                arrCheckResult[3] = !string.IsNullOrEmpty(arrCheckResult[6]) ? arrCheckResult[6] : "[三總院內碼]" + arrCellString[6] + "不是此訂單品項";
                            }


                            //交貨數量
                            if (int.TryParse(arrCellString[5], out int a))
                            {
                                if (int.Parse(arrCellString[5]) <= 0)
                                {
                                    arrCheckResult[5] = !string.IsNullOrEmpty(arrCheckResult[5]) ? arrCheckResult[4] : "[交貨數量]" + arrCellString[5] + "不可小於0";
                                }
                            }
                            else
                            {
                                arrCheckResult[5] = !string.IsNullOrEmpty(arrCheckResult[4]) ? arrCheckResult[5] : "[交貨數量]" + arrCellString[5] + "不可小於0";
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

                            //檢查是否要輸入[效期]及[批號]
                            if (repo.CheckStatus(po_no, DBWork.ProcUser) && (arrCellString[7]==""|| arrCellString[8]==""))
                            {
                                arrCheckResult[7] = !string.IsNullOrEmpty(arrCheckResult[7]) ? arrCheckResult[7] : "[批號],[效期]管制" + arrCellString[8] + "[批號],[效期]管制欄不可空白";
                            }

                            //檢查[效期]是否空白及符合日期格式yyyymmdd
                            if (!DateTime.TryParseExact(arrCellString[8], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out dtDate))
                            {
                                arrCheckResult[8] = !string.IsNullOrEmpty(arrCheckResult[8]) ? arrCheckResult[8] : "[效期]" + arrCellString[9] + "不符合西元年yyyymmdd格式";
                            }

                            //檢查[發票號碼]
                            if (arrCellString[9].Length == 10)
                            {
                                if (!char.IsUpper(char.Parse(arrCellString[9].Substring(0, 1))) || !char.IsUpper(char.Parse(arrCellString[9].Substring(1, 1))) || !int.TryParse(arrCellString[9].Substring(2, 8), out int c))
                                { arrCheckResult[9] = !string.IsNullOrEmpty(arrCheckResult[9]) ? arrCheckResult[9] : "[發票號碼]" + arrCellString[9] + "格式錯誤"; }
                            }
                            else
                            {
                                arrCheckResult[9] = !string.IsNullOrEmpty(arrCheckResult[9]) ? arrCheckResult[9] : "[發票號碼]" + arrCellString[9] + "格式錯誤";
                            }

                            #endregion


                            #region 備註
                            //廠商編號 AGEN_NO
                            //交貨批次 DNO 
                            //藥品中文名稱
                            //藥品英文名稱
                            //預計交貨日 DELI_DT
                            //交貨數量 INQTY
                            //三總院內碼 MMCODE
                            //借貨量 BW_SQTY
                            //批號 LOT_NO
                            //效期 EXP_DATE
                            //發票號碼 INVOICE
                            //發票日期 INVOICE_DT
                            //備註 MEMO
                            #endregion
                            //DateTime defaultday = new DateTime(1912, 1, 1);
                            wb_reply.AGEN_NO = arrCellString[0];
                            wb_reply.DNO =            arrCellString[1];
                            wb_reply.DELI_DT     =    arrCellString[4];
                            wb_reply.INQTY =          arrCellString[5];
                            wb_reply.MMCODE      =    arrCellString[6];                                       
                            //wb_reply.BW_SQTY     =    arrCellString[7];
                            wb_reply.LOT_NO      =    arrCellString[7];
                            wb_reply.EXP_DATE    =    arrCellString[8];
                            wb_reply.INVOICE     =    arrCellString[9];
                            wb_reply.INVOICE_DT  =    arrCellString[10];    
                            wb_reply.MEMO = arrCellString[11];
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
                     
                    var repo = new BH0008Repository(DBWork);
                    foreach (WB_REPLY data in wb_reply)
                    {
                        data.CREATE_USER = User.Identity.Name;
                        data.UPDATE_IP = DBWork.ProcIP;
                        data.UPDATE_USER = DBWork.ProcUser;

                        //檢查結果是否OK
                        if (data.CHECK_RESULT == "OK")
                        {

                            try
                            {
                                data.STATUS = "A";
                                data.FLAG = "A";
                                repo.ImportCreate(data);
                                data.IMPORT_RESULT = "匯入成功";
                                data.STATUS = "處理中";
                            }
                            catch (Exception ex)
                            {
                                data.IMPORT_RESULT = "匯入失敗";
                                data.STATUS = "";
                            }
                        }
                        else
                        {
                            data.IMPORT_RESULT = "資料未上傳";
                        }

                        list_wb_reply.Add(data);

                    }

                    DBWork.Commit();
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
                    BH0008Repository repo = new BH0008Repository(DBWork);
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
                    BH0008Repository repo = new BH0008Repository(DBWork);
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
                    BH0008Repository repo = new BH0008Repository(DBWork);
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

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BH0008Repository repo = new BH0008Repository(DBWork);
                    session.Result.etts = repo.GetPoDetail(form.Get("PO_NO"), form.Get("MMCODE"), User.Identity.Name, form.Get("DNO"), page, limit, sorters);
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
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BH0008Repository repo = new BH0008Repository(DBWork);
                    WB_REPLY wb_reply = new WB_REPLY();
                    wb_reply.PO_NO = form.Get("PO");
                    wb_reply.DNO = repo.GetMaxDno(wb_reply.PO_NO);
                    wb_reply.AGEN_NO = User.Identity.Name;
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
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BH0008Repository repo = new BH0008Repository(DBWork);
                    WB_REPLY wb_reply = new WB_REPLY();
                    wb_reply.PO_NO = form.Get("PO_NO");
                    wb_reply.DNO = form.Get("DNO");
                    wb_reply.MMCODE = form.Get("MMCODE");
                    wb_reply.DELI_DT = string.IsNullOrEmpty(form.Get("DELI_DT")) ? null : form.Get("DELI_DT");
                    wb_reply.LOT_NO = form.Get("LOT_NO");
                    wb_reply.EXP_DATE = string.IsNullOrEmpty(form.Get("EXP_DATE")) ? null : form.Get("DELI_DT");
                    wb_reply.INQTY = form.Get("INQTY");
                    //wb_reply.BW_SQTY = form.Get("BW_SQTY");
                    wb_reply.INVOICE = form.Get("INVOICE");
                    wb_reply.INVOICE_DT = string.IsNullOrEmpty(form.Get("INVOICE_DT")) ? null : form.Get("DELI_DT");
                    //wb_reply.BARCODE = form.Get("BARCODE");
                    wb_reply.MEMO = form.Get("MEMO");
                    wb_reply.AGEN_NO = User.Identity.Name;
                    wb_reply.CREATE_USER = User.Identity.Name;
                    wb_reply.UPDATE_USER = User.Identity.Name;
                    wb_reply.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.CreateOne(wb_reply);

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
        public ApiResponse DetailUpdate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BH0008Repository repo = new BH0008Repository(DBWork);
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
                    BH0008Repository repo = new BH0008Repository(DBWork);
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
                    BH0008Repository repo = new BH0008Repository(DBWork);

                    string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] tmp_seq = seq.Split(',');
                    for (int i = 0; i < tmp_seq.Length; i++)
                    {
                        WB_REPLY wb_reply = new WB_REPLY();
                        wb_reply.UPDATE_USER = User.Identity.Name;
                        wb_reply.UPDATE_IP = DBWork.ProcIP;
                        wb_reply.SEQ = tmp_seq[i];
                        if (repo.ChkINQTY(tmp_seq[i]) == "Y")
                        {
                            session.Result.afrs = repo.DetailSubmit(wb_reply);
                        }
                        else
                        {
                            session.Result.msg = "[交貨數量]累計超過[訂單數量],請檢查!";
                            break;
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
    }
}
