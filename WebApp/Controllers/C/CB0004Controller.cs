using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.CB;
using WebApp.Models;
using System;
using Newtonsoft.Json;
using System.Web;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;

namespace WebApp.Controllers.CB
{
    public class CB0004Controller : SiteBase.BaseApiController
    {

        [HttpPost]
        public ApiResponse DownloadExcel(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    HttpResponse res = HttpContext.Current.Response;

                    FileStream fs = new FileStream(HttpContext.Current.Server.MapPath("~/Scripts/C/CB0004-上傳品項條碼資料範本.xls"), FileMode.Open);

                    byte[] bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    fs.Close();

                    res.BufferOutput = false;

                    res.Clear();
                    res.ClearHeaders();
                    res.HeaderEncoding = System.Text.Encoding.Default;
                    res.ContentType = "application/octet-stream";
                    res.AddHeader("Content-Disposition",
                                "attachment; filename=" + HttpUtility.UrlEncode("CB0004-上傳品項條碼資料範本.xls", System.Text.Encoding.UTF8));
                    res.BinaryWrite(bytes);



                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }



        [HttpPost]
        public ApiResponse CheckExcel()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<BC_BARCODE> list = new List<BC_BARCODE>();
                UnitOfWork DBWork = session.UnitOfWork;

                string[] arr = { "院內碼", "國際條碼", "條碼類別代碼", "條碼類別敘述" };

                try
                {
                    CB0004Repository repo = new CB0004Repository(DBWork);

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
                    int cellCount =0;
                    bool isValid = true;

                    if (headerRow != null)
                    {
                        cellCount = headerRow.LastCellNum;
                    }
                    else
                    {
                        isValid = false;
                        session.Result.msg = "檔案格式不同, 請下載範本更新";
                    }

                    int i, j;
                    string cellstring = "";
                    int[] dataLen = { 13, 200, 20 };
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
                        isValid = headerRow.GetCell(0).ToString() == "院內碼" ? true : false;
                        if (isValid)
                        {
                            isValid = headerRow.GetCell(1).ToString() == "國際條碼" ? true : false;
                        }
                        if (isValid)
                        {
                            isValid = headerRow.GetCell(2).ToString() == "條碼類別代碼" ? true : false;
                        }
                        if (isValid)
                        {
                            isValid = headerRow.GetCell(3).ToString() == "條碼類別敘述" ? true : false;
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
                            string[] rowErrStr = new string[3];

                            IRow row = sheet.GetRow(i);
                            if (row is null) { continue; }
                            #region 資料驗證
                            //是否為空白
                            for (j = 0; j < cellCount; j++)
                            {
                                if (j == 3) continue;

                                cellstring = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();

                                if (string.IsNullOrWhiteSpace(cellstring))
                                {
                                    rowErrStr[j] = "[" + arr[j] + "]不可空白";
                                }
                            }


                            //檢查資料長度
                            for (j = 0; j < cellCount; j++)
                            {
                                if (j == 3) continue;

                                cellstring = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();

                                if (cellstring.Length > dataLen[j])
                                {
                                    rowErrStr[j] = "[" + arr[j] + "]長度超出，最大長度" + dataLen[j].ToString();
                                }
                            }

                            // 檢查院內碼是否已存在
                            if (!repo.CheckMmcodeExists(row.GetCell(0).ToString()))
                            {
                                rowErrStr[0] = "[院內碼]" + row.GetCell(0).ToString() + "院內碼不存在基本檔";
                            }
                            // 檢查國際條碼是否已存在
                            if (repo.CheckBarcodeExists(row.GetCell(0).ToString(), row.GetCell(1).ToString()))
                            {
                                rowErrStr[1] = "[國際條碼]" + row.GetCell(1).ToString() + "已存在";
                            }
                            // 檢查條碼類別代碼是否已存在
                            if (!repo.CheckXcategoryExists(row.GetCell(2).ToString()))
                            {
                                rowErrStr[2] = "[條碼類別代碼]" + row.GetCell(2).ToString() + "不存在條碼分類檔";
                            }
                            #endregion

                            #region 處理輸出資料
                            string[] datatStr = new string[4];
                            string resultMsg = "";
                            string rowDataStr = "";
                            for (j = 0; j < cellCount; j++)
                            {
                                datatStr[j] = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();
                                rowDataStr += datatStr[j] == "" ? "" : datatStr[j] + ",";
                            }
                            //刪除最後的逗號
                            rowDataStr = rowDataStr.Length > 0 ? rowDataStr.Substring(0, rowDataStr.Length - 1) : "";
                            foreach (string errStr in rowErrStr)
                            {
                                if (!string.IsNullOrEmpty(errStr))
                                    resultMsg += errStr + "</br>";
                            }
                            if (resultMsg.Length > 0)
                            {
                                //刪除最後的換行符號
                                resultMsg = resultMsg.Substring(0, resultMsg.Length - 5);
                            }
                            else
                            {
                                resultMsg = "OK";
                            }
                            #endregion
                            BC_BARCODE bC_BARCODE = new BC_BARCODE();
                            //原始資料
                            bC_BARCODE.BARCODE_TEXT = rowDataStr;
                            //檢核結果
                            bC_BARCODE.STATUS_DISPLAY = resultMsg;
                            //條碼類別敘述
                            bC_BARCODE.XCATEGORY_DISPLAY = datatStr[3];
                            bC_BARCODE.MMCODE = datatStr[0];
                            bC_BARCODE.BARCODE = datatStr[1];
                            bC_BARCODE.XCATEGORY = datatStr[2];

                            list.Add(bC_BARCODE);
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

        
        [HttpPost]
        public ApiResponse Import(FormDataCollection formData)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                IEnumerable<BC_BARCODE> bC_BARCODE = JsonConvert.DeserializeObject<IEnumerable<BC_BARCODE>>(formData["data"]);
                List<BC_BARCODE> list_bc_barcode = new List<BC_BARCODE>();
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CB0004Repository(DBWork);
                    foreach (BC_BARCODE bc_barcode in bC_BARCODE)
                    {
                        bc_barcode.CREATE_USER = User.Identity.Name;
                        bc_barcode.UPDATE_IP = DBWork.ProcIP;
                        bc_barcode.UPDATE_USER = DBWork.ProcUser;

                        BC_BARCODE new_bc_barcode = new BC_BARCODE();
                        new_bc_barcode = bc_barcode;

                        //檢查結果是否OK
                        if (!repo.CheckPKDataExists(bc_barcode)&& bc_barcode.STATUS_DISPLAY=="OK")
                        {
                            try
                            {
                                repo.Create(bc_barcode);
                                //狀態
                                new_bc_barcode.STATUS = "匯入成功";
                            }
                            catch(Exception ex)
                            {
                                new_bc_barcode.STATUS = "匯入失敗";
                            }
                        }
                        else
                        {
                            new_bc_barcode.STATUS = "資料未上傳";
                        }

                        list_bc_barcode.Add(bc_barcode);
                    }

                    DBWork.Commit();
                    session.Result.etts = list_bc_barcode;
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