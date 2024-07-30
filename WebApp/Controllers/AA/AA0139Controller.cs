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
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0139Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0139Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2);
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

                List<ME_DOCD> list = new List<ME_DOCD>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                var win1P0 = HttpContext.Current.Request.Form["win1P0"]; // 聯標生效起年
                var win1P1 = HttpContext.Current.Request.Form["win1P1"]; // 聯標生效迄年
                var win1P2 = HttpContext.Current.Request.Form["win1P2"]; // 修改年月日
                IWorkbook workBook;

                DBWork.BeginTransaction();
                try
                {
                    AA0139Repository repo = new AA0139Repository(DBWork);
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
                    IRow headerRow = sheet.GetRow(0); //由第一列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum; //欄位數目
                    int i, j;


                    bool isValid = true;
                    string[] arr = { "投標項次", "招標成分", "成分含量", "規格量", "劑型", "英文品名", "中文品名", "包裝",
                        "原廠牌", "許可證字號", "單次訂購達優惠數量折讓意願", "單次採購優惠數量", "健保代碼", "健保價(健保品項)/上月預算單價(非健保品項)",
                        "決標契約單價", "決標成本單價", "單次訂購達優惠數量成本價", "廠商統編", "廠商名稱" };

                    for (j = 0; j < cellCount; j++)
                    {
                        isValid = headerRow.GetCell(j) == null ? false : true;
                        if (!isValid)
                        {
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
                        session.Result.msg = "檔案格式不同。";
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

                        //略過第0列(標題列)，一直處理至最後一列
                        //for (i = 1; i <= sheet.LastRowNum; i++)
                        //略過第0列(說明列)和第1列(標題列)，一直處理至最後一列
                        for (i = 1; i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            datarow = dtTable.NewRow();
                            //arrCheckResult = "OK";
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

                        dtTable.DefaultView.Sort = "投標項次";
                        dtTable = dtTable.DefaultView.ToTable();
                        #endregion

                        DataTable newTable = dtTable.Clone();
                        newTable = dtTable;

                        // 清空工作檔
                        int delRowsWK = repo.DeleteWK();

                        // table MILMED_JBID_LIST資料搬到工作檔(MILMED_JBID_LISTWK)
                        int transRows = repo.TransM(win1P0, win1P1);

                        // 清空符合條件的資料
                        int delRowsM = repo.DeleteM(win1P0, win1P1);

                        //將excel內容寫入MILMED_JBID_LIST
                        #region 加入至MILMED_JBID_LIST中
                        for (i = 0; i < newTable.Rows.Count; i++)
                        {
                            AA0139 aa0139 = new AA0139();

                            aa0139.JBID_STYR = win1P0;
                            aa0139.JBID_EDYR = win1P1;
                            aa0139.BID_NO = newTable.Rows[i]["投標項次"].ToString().Trim();
                            aa0139.INGR = newTable.Rows[i]["招標成分"].ToString().Trim();
                            aa0139.INGR_CONTENT = newTable.Rows[i]["成分含量"].ToString().Trim();
                            aa0139.SPEC = newTable.Rows[i]["規格量"].ToString().Trim();
                            aa0139.DOSAGE_FORM = newTable.Rows[i]["劑型"].ToString().Trim();
                            aa0139.MMNAME_E = newTable.Rows[i]["英文品名"].ToString().Trim();
                            aa0139.MMNAME_C = newTable.Rows[i]["中文品名"].ToString().Trim();
                            aa0139.PACKQTY = newTable.Rows[i]["包裝"].ToString().Trim();
                            aa0139.ORIG_BRAND = newTable.Rows[i]["原廠牌"].ToString().Trim();
                            aa0139.LICENSE_NO = newTable.Rows[i]["許可證字號"].ToString().Trim();
                            aa0139.ISWILLING = newTable.Rows[i]["單次訂購達優惠數量折讓意願"].ToString().Trim();
                            aa0139.DISCOUNT_QTY = newTable.Rows[i]["單次採購優惠數量"].ToString().Trim();
                            aa0139.INSU_CODE = newTable.Rows[i]["健保代碼"].ToString().Trim();
                            aa0139.INSU_RATIO = newTable.Rows[i]["健保價(健保品項)/上月預算單價(非健保品項)"].ToString().Trim();
                            aa0139.K_UPRICE = newTable.Rows[i]["決標契約單價"].ToString().Trim();
                            aa0139.COST_UPRICE = newTable.Rows[i]["決標成本單價"].ToString().Trim();
                            aa0139.DISC_COST_UPRICE = newTable.Rows[i]["單次訂購達優惠數量成本價"].ToString().Trim();
                            aa0139.UNIFORM_NO = newTable.Rows[i]["廠商統編"].ToString().Trim();
                            aa0139.AGEN_NAME = newTable.Rows[i]["廠商名稱"].ToString().Trim();
                            aa0139.UPDATE_YMD = win1P2;
                            aa0139.UPADTEUSER = User.Identity.Name;
                            aa0139.UPDATEIP = DBWork.ProcIP;

                            if (aa0139.INSU_CODE.Length > 10) {
                                int temp = 0;
                            }

                            //產生一筆資料
                            repo.CreateM(aa0139);
                        }
                        #endregion

                        // MILMED_JBID_LOG資料只保留上月及本月，例如 sysdate=2021/04/23，取上月2021/03，轉為民國年11003
                        DateTime lastMN = DateTime.Now.AddMonths(-1);
                        string lastTWMN = (lastMN.Year - 1911).ToString() + lastMN.Month.ToString().PadLeft(2, '0');
                        int delRowsLog = repo.DeleteLog(lastTWMN);

                        // 比對本次匯入資料是否有異動，若有異動，將異動前及異動後資料，寫到MILMED_JBID_LOG
                        repo.CreateLogD();
                        repo.CreateLogI();
                    }

                    if (!isValid)
                    {
                        session.Result.success = false;
                        DBWork.Rollback();
                    }
                    else
                    {
                        DBWork.Commit();
                        // session.Result.etts = list;
                        session.Result.msg = checkPassed.ToString();
                    }
                }
                catch(Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢
        [HttpPost]
        public ApiResponse GetWin2All_1(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0139Repository(DBWork);
                    session.Result.etts = repo.GetWin2All_1(p0);
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
        public ApiResponse ExcelT1(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0139Repository repo = new AA0139Repository(DBWork);
                    JCLib.Excel.Export("AA0139匯出.xls", repo.GetExcelT1(p0));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse ExcelT2(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0139Repository repo = new AA0139Repository(DBWork);
                    JCLib.Excel.Export("AA0139匯出(多筆).xls", repo.GetExcelT2(p0));
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