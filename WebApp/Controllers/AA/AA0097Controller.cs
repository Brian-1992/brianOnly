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
    public class AA0097Controller : SiteBase.BaseApiController
    {
        //匯入檢核
        [HttpPost]
        public ApiResponse SendExcel()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<AA0097M> list = new List<AA0097M>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                IWorkbook workBook;

                try
                {
                    AA0097Repository repo = new AA0097Repository(DBWork);
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
                    IRow headerRow = sheet.GetRow(1); //由第二列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum; //欄位數目
                    int i, j;


                    bool isValid = true;
                    //string[] arr = { "院內碼", "庫備識別碼", "優惠最小單價(計量單位單價)", "申購計量單位(包裝單位)", "包裝轉換率", "長度(CM)", "寬度(CM)", "高度(CM)", "圓周", "材積轉換率", "ID碼", "衛材料號碼", "行政院碼", "消耗屬性(1消耗,2半消耗)", "給付類別(1自費,2健保,3醫院吸收)", "計費方式(1計價,2不計價)", "扣庫方式(1扣庫2不扣庫)", "是否盤盈虧(Y盤盈虧,N消耗)" };
                    string[] arr = { "院內碼", "庫備識別碼(0非庫備,1庫備)", "庫備識別碼(0非庫備,1庫備)(新)", "優惠最小單價(計量單位單價)", "優惠最小單價(計量單位單價)(新)", "申購計量單位(包裝單位)", "申購計量單位(包裝單位)(新)", "包裝轉換率", "包裝轉換率(新)", "長度(CM)", "長度(CM)(新)", "寬度(CM)", "寬度(CM)(新)", "高度(CM)", "高度(CM)(新)", "圓周", "圓周(新)", "材積轉換率", "材積轉換率(新)", "ID碼", "ID碼(新)", "衛材料號碼", "衛材料號碼(新)", "行政院碼", "行政院碼(新)", "消耗屬性(1消耗,2半消耗)", "消耗屬性(1消耗,2半消耗)(新)", "給付類別(1自費,2健保,3醫院吸收)", "給付類別(1自費,2健保,3醫院吸收)(新)", "計費方式(1計價,2不計價)", "計費方式(1計價,2不計價)(新)", "扣庫方式(1扣庫2不扣庫)", "扣庫方式(1扣庫2不扣庫)(新)" };

                    for (j = 0; j < cellCount; j++)
                    {
                        isValid = headerRow.GetCell(j) == null ? false : true;
                        if (!isValid)
                        {
                            session.Result.msg = "檔案格式不同，請下載匯出的物料主檔來更新。";
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
                        session.Result.msg = "檔案格式不同，請下載匯出的物料主檔來更新。";
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
                        for (i = 2; i <= sheet.LastRowNum; i++)
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

                        dtTable.DefaultView.Sort = "院內碼";
                        dtTable = dtTable.DefaultView.ToTable();
                        #endregion

                        DataTable newTable = dtTable.Clone();
                        newTable = dtTable;

                        //加入至AA0097M中
                        #region 加入至AA0097M中
                        for (i = 0; i < newTable.Rows.Count; i++)
                        {
                            AA0097M AA0097M = new AA0097M();
                            DataTable dt = new DataTable();
                            dt = repo.GetAll(newTable.Rows[i]["院內碼"].ToString());

                            AA0097M.MMCODE = newTable.Rows[i]["院內碼"].ToString();
                            AA0097M.M_STOREID_N = newTable.Rows[i]["庫備識別碼(0非庫備,1庫備)(新)"].ToString() == "" ? "" : newTable.Rows[i]["庫備識別碼(0非庫備,1庫備)(新)"].ToString();
                            AA0097M.DISC_UPRICE_N = newTable.Rows[i]["優惠最小單價(計量單位單價)(新)"].ToString() == "" ? "" : newTable.Rows[i]["優惠最小單價(計量單位單價)(新)"].ToString();
                            AA0097M.M_PURUN_N = newTable.Rows[i]["申購計量單位(包裝單位)(新)"].ToString() == "" ? "" : newTable.Rows[i]["申購計量單位(包裝單位)(新)"].ToString();
                            AA0097M.EXCH_RATIO_N = newTable.Rows[i]["包裝轉換率(新)"].ToString() == "" ? "" : newTable.Rows[i]["包裝轉換率(新)"].ToString();
                            AA0097M.M_VOLL_N = newTable.Rows[i]["長度(CM)(新)"].ToString() == "" ? "" : newTable.Rows[i]["長度(CM)(新)"].ToString();
                            AA0097M.M_VOLW_N = newTable.Rows[i]["寬度(CM)(新)"].ToString() == "" ? "" : newTable.Rows[i]["寬度(CM)(新)"].ToString();
                            AA0097M.M_VOLH_N = newTable.Rows[i]["高度(CM)(新)"].ToString() == "" ? "" : newTable.Rows[i]["高度(CM)(新)"].ToString();
                            AA0097M.M_VOLC_N = newTable.Rows[i]["圓周(新)"].ToString() == "" ? "" : newTable.Rows[i]["圓周(新)"].ToString();
                            AA0097M.M_SWAP_N = newTable.Rows[i]["材積轉換率(新)"].ToString() == "" ? "" : newTable.Rows[i]["材積轉換率(新)"].ToString();
                            AA0097M.M_IDKEY_N = newTable.Rows[i]["ID碼(新)"].ToString() == "" ? "" : newTable.Rows[i]["ID碼(新)"].ToString();
                            AA0097M.M_INVKEY_N = newTable.Rows[i]["衛材料號碼(新)"].ToString() == "" ? "" : newTable.Rows[i]["衛材料號碼(新)"].ToString();
                            AA0097M.M_GOVKEY_N = newTable.Rows[i]["行政院碼(新)"].ToString() == "" ? "" : newTable.Rows[i]["行政院碼(新)"].ToString();
                            AA0097M.M_CONSUMID_N = newTable.Rows[i]["消耗屬性(1消耗,2半消耗)(新)"].ToString() == "" ? "" : newTable.Rows[i]["消耗屬性(1消耗,2半消耗)(新)"].ToString();
                            AA0097M.M_PAYKIND_N = newTable.Rows[i]["給付類別(1自費,2健保,3醫院吸收)(新)"].ToString() == "" ? "" : newTable.Rows[i]["給付類別(1自費,2健保,3醫院吸收)(新)"].ToString();
                            AA0097M.M_PAYID_N = newTable.Rows[i]["計費方式(1計價,2不計價)(新)"].ToString() == "" ? "" : newTable.Rows[i]["計費方式(1計價,2不計價)(新)"].ToString();
                            AA0097M.M_TRNID_N = newTable.Rows[i]["扣庫方式(1扣庫2不扣庫)(新)"].ToString() == "" ? "" : newTable.Rows[i]["扣庫方式(1扣庫2不扣庫)(新)"].ToString();
                            //AA0097M.INV_POSTID_N = newTable.Rows[i]["是否盤盈虧(Y盤盈虧,N消耗)(新)"].ToString() == "" ? "" : newTable.Rows[i]["是否盤盈虧(Y盤盈虧,N消耗)(新)"].ToString();
                            AA0097M.CHECK_RESULT = newTable.Rows[i]["檢核結果"].ToString();
                            //AA0097M.IMPORT_RESULT = "尚未上傳更新";

                            //資料是否被使用者填入更新值
                            bool dataUpdated = false;

                            //如果有任何一格不是空的
                            if (
                                AA0097M.M_STOREID_N != "" ||
                                AA0097M.DISC_UPRICE_N != "" ||
                                AA0097M.M_PURUN_N != "" ||
                                AA0097M.EXCH_RATIO_N != "" ||
                                AA0097M.M_VOLL_N != "" ||
                                AA0097M.M_VOLW_N != "" ||
                                AA0097M.M_VOLH_N != "" ||
                                AA0097M.M_VOLC_N != "" ||
                                AA0097M.M_SWAP_N != "" ||
                                AA0097M.M_IDKEY_N != "" ||
                                AA0097M.M_INVKEY_N != "" ||
                                AA0097M.M_GOVKEY_N != "" ||
                                AA0097M.M_CONSUMID_N != "" ||
                                AA0097M.M_PAYKIND_N != "" ||
                                AA0097M.M_PAYID_N != "" ||
                                AA0097M.M_TRNID_N != "" 
                                //AA0097M.INV_POSTID_N != ""
                                )
                            {
                                //表示資料有被更新
                                dataUpdated = true;
                            }

                            //若院內碼不是空的且資料有更新過
                            if (newTable.Rows[i]["院內碼"].ToString() != "" && dataUpdated == true)
                            {
                                //檢核院內碼
                                if (repo.CheckExistsMMCODE(newTable.Rows[i]["院內碼"].ToString()) != true)
                                {
                                    AA0097M.CHECK_RESULT = "院內碼不存在";
                                }
                                else
                                {
                                    //檢核物料類別
                                    if (repo.CheckExistsMMCODEMAT(newTable.Rows[i]["院內碼"].ToString()) != true)
                                    {
                                        AA0097M.CHECK_RESULT = "本批次上傳作業僅適用於 '02'-衛材 類別";
                                    }
                                    else
                                    {
                                        AA0097M.MMNAME_C = dt.Rows[0]["MMNAME_C"].ToString();
                                        AA0097M.MMNAME_E = dt.Rows[0]["MMNAME_E"].ToString();
                                        AA0097M.M_STOREID = newTable.Rows[i]["庫備識別碼(0非庫備,1庫備)(新)"].ToString() == "" ? "" : dt.Rows[0]["M_STOREID"].ToString();
                                        AA0097M.DISC_UPRICE = newTable.Rows[i]["優惠最小單價(計量單位單價)(新)"].ToString() == "" ? "" : dt.Rows[0]["DISC_UPRICE"].ToString();
                                        AA0097M.M_PURUN = newTable.Rows[i]["申購計量單位(包裝單位)(新)"].ToString() == "" ? "" : dt.Rows[0]["M_PURUN"].ToString();
                                        AA0097M.EXCH_RATIO = newTable.Rows[i]["包裝轉換率(新)"].ToString() == "" ? "" : dt.Rows[0]["EXCH_RATIO"].ToString();
                                        AA0097M.M_VOLL = newTable.Rows[i]["長度(CM)(新)"].ToString() == "" ? "" : dt.Rows[0]["M_VOLL"].ToString();
                                        AA0097M.M_VOLW = newTable.Rows[i]["寬度(CM)(新)"].ToString() == "" ? "" : dt.Rows[0]["M_VOLW"].ToString();
                                        AA0097M.M_VOLH = newTable.Rows[i]["高度(CM)(新)"].ToString() == "" ? "" : dt.Rows[0]["M_VOLH"].ToString();
                                        AA0097M.M_VOLC = newTable.Rows[i]["圓周(新)"].ToString() == "" ? "" : dt.Rows[0]["M_VOLC"].ToString();
                                        AA0097M.M_SWAP = newTable.Rows[i]["材積轉換率(新)"].ToString() == "" ? "" : dt.Rows[0]["M_SWAP"].ToString();
                                        AA0097M.M_IDKEY = newTable.Rows[i]["ID碼(新)"].ToString() == "" ? "" : dt.Rows[0]["M_IDKEY"].ToString();
                                        AA0097M.M_INVKEY = newTable.Rows[i]["衛材料號碼(新)"].ToString() == "" ? "" : dt.Rows[0]["M_INVKEY"].ToString();
                                        AA0097M.M_GOVKEY = newTable.Rows[i]["行政院碼(新)"].ToString() == "" ? "" : dt.Rows[0]["M_GOVKEY"].ToString();
                                        AA0097M.M_CONSUMID = newTable.Rows[i]["消耗屬性(1消耗,2半消耗)(新)"].ToString() == "" ? "" : dt.Rows[0]["M_CONSUMID"].ToString();
                                        AA0097M.M_PAYKIND = newTable.Rows[i]["給付類別(1自費,2健保,3醫院吸收)(新)"].ToString() == "" ? "" : dt.Rows[0]["M_PAYKIND"].ToString();
                                        AA0097M.M_PAYID = newTable.Rows[i]["計費方式(1計價,2不計價)(新)"].ToString() == "" ? "" : dt.Rows[0]["M_PAYID"].ToString();
                                        AA0097M.M_TRNID = newTable.Rows[i]["扣庫方式(1扣庫2不扣庫)(新)"].ToString() == "" ? "" : dt.Rows[0]["M_TRNID"].ToString();
                                        //AA0097M.INV_POSTID = newTable.Rows[i]["是否盤盈虧(Y盤盈虧,N消耗)(新)"].ToString() == "" ? "" : dt.Rows[0]["INV_POSTID"].ToString();

                                        if (AA0097M.CHECK_RESULT == "OK")
                                        {
                                            AA0097M.CHECK_RESULT = "";
                                            double d; //TryParse用容器

                                            //檢核庫備識別碼 M_STOREID
                                            if (AA0097M.M_STOREID_N != "")
                                            {
                                                if (AA0097M.M_STOREID_N != "0" && AA0097M.M_STOREID_N != "1")
                                                {
                                                    AA0097M.CHECK_RESULT += "庫備識別碼未定義";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核優惠最小單價(計量單位單價) DISC_UPRICE
                                            if (AA0097M.DISC_UPRICE_N != "")
                                            {
                                                if (double.TryParse(AA0097M.DISC_UPRICE_N, out d) != true)
                                                {
                                                    AA0097M.CHECK_RESULT += "最小優惠單價必須為數字";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                                if (double.Parse(AA0097M.DISC_UPRICE_N) < 0)
                                                {
                                                    AA0097M.CHECK_RESULT += "最小優惠單價不可為負值";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核申購計量單位(包裝單位) M_PURUN
                                            if (AA0097M.M_PURUN_N != "")
                                            {
                                                if (AA0097M.EXCH_RATIO_N == "")
                                                {
                                                    AA0097M.CHECK_RESULT += "新申購計量單位與新包裝轉換率需同時訂定";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                                else
                                                {
                                                    if (repo.CheckExistsPURUN(AA0097M.M_PURUN_N) != true)
                                                    {
                                                        AA0097M.CHECK_RESULT += "新申購計量單位不存在於計量單位檔";
                                                        AA0097M.CHECK_RESULT += ", ";
                                                    }
                                                }
                                            }

                                            //檢核包裝轉換率 EXCH_RATIO
                                            if (AA0097M.EXCH_RATIO_N != "")
                                            {
                                                if (AA0097M.M_PURUN_N == "")
                                                {
                                                    AA0097M.CHECK_RESULT += "新申購計量單位與新包裝轉換率需同時訂定";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                                else
                                                {
                                                    //院內碼與計量單位須同時符合MI_UNITEXCH內的資料
                                                    //if (repo.CheckExistsEXCH_RATIO(AA0097M.MMCODE, AA0097M.M_PURUN_N) != true)
                                                    //{
                                                    //    AA0097M.CHECK_RESULT += "包裝轉換率無法新增只能修改";
                                                    //    AA0097M.CHECK_RESULT += ", ";
                                                    //}
                                                    if (double.TryParse(AA0097M.EXCH_RATIO_N, out d) != true)
                                                    {
                                                        AA0097M.CHECK_RESULT += "包裝轉換率必須為數字";
                                                        AA0097M.CHECK_RESULT += ", ";
                                                    }
                                                    if (double.Parse(AA0097M.EXCH_RATIO_N) < 0)
                                                    {
                                                        AA0097M.CHECK_RESULT += "包裝轉換率不可為負值";
                                                        AA0097M.CHECK_RESULT += ", ";
                                                    }
                                                }
                                            }

                                            //檢核長度 M_VOLL
                                            if (AA0097M.M_VOLL_N != "")
                                            {
                                                if (double.TryParse(AA0097M.M_VOLL_N, out d) != true)
                                                {
                                                    AA0097M.CHECK_RESULT += "長度必須為數字";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                                if (double.Parse(AA0097M.M_VOLL_N) < 0)
                                                {
                                                    AA0097M.CHECK_RESULT += "長度不可為負值";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核寬度 M_VOLW
                                            if (AA0097M.M_VOLW_N != "")
                                            {
                                                if (double.TryParse(AA0097M.M_VOLW_N, out d) != true)
                                                {
                                                    AA0097M.CHECK_RESULT += "寬度必須為數字";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                                if (double.Parse(AA0097M.M_VOLW_N) < 0)
                                                {
                                                    AA0097M.CHECK_RESULT += "寬度不可為負值";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核高度 M_VOLH
                                            if (AA0097M.M_VOLH_N != "")
                                            {
                                                if (double.TryParse(AA0097M.M_VOLH_N, out d) != true)
                                                {
                                                    AA0097M.CHECK_RESULT += "高度必須為數字";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                                if (double.Parse(AA0097M.M_VOLH_N) < 0)
                                                {
                                                    AA0097M.CHECK_RESULT += "高度不可為負值";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核圓周 M_VOLC
                                            if (AA0097M.M_VOLC_N != "")
                                            {
                                                if (double.TryParse(AA0097M.M_VOLC_N, out d) != true)
                                                {
                                                    AA0097M.CHECK_RESULT += "圓周必須為數字";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                                if (double.Parse(AA0097M.M_VOLC_N) < 0)
                                                {
                                                    AA0097M.CHECK_RESULT += "圓周不可為負值";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核材積轉換率 M_SWAP
                                            if (AA0097M.M_SWAP_N != "")
                                            {
                                                if (double.TryParse(AA0097M.M_SWAP_N, out d) != true)
                                                {
                                                    AA0097M.CHECK_RESULT += "材積轉換率必須為數字";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                                if (double.Parse(AA0097M.M_SWAP_N) < 0)
                                                {
                                                    AA0097M.CHECK_RESULT += "材積轉換率不可為負值";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核ID碼 M_IDKEY
                                            if (AA0097M.M_IDKEY_N != "")
                                            {
                                                if (AA0097M.M_IDKEY_N.Length > 8)
                                                {
                                                    AA0097M.CHECK_RESULT += "ID碼太長";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核衛材料號碼 M_INVKEY
                                            if (AA0097M.M_INVKEY_N != "")
                                            {
                                                if (AA0097M.M_IDKEY_N.Length > 16)
                                                {
                                                    AA0097M.CHECK_RESULT += "衛材料號碼太長";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核行政院碼 M_GOVKEY
                                            if (AA0097M.M_GOVKEY_N != "")
                                            {
                                                if (AA0097M.M_IDKEY_N.Length > 12)
                                                {
                                                    AA0097M.CHECK_RESULT += "行政院碼太長";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核消耗屬性 M_CONSUMID
                                            if (AA0097M.M_CONSUMID_N != "")
                                            {
                                                if (AA0097M.M_CONSUMID_N != "1" && AA0097M.M_CONSUMID_N != "2")
                                                {
                                                    AA0097M.CHECK_RESULT += "消耗屬性未定義";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核給付類別 M_PAYKIND
                                            if (AA0097M.M_PAYKIND_N != "")
                                            {
                                                if (AA0097M.M_PAYKIND_N != "1" && AA0097M.M_PAYKIND_N != "2" && AA0097M.M_PAYKIND_N != "3")
                                                {
                                                    AA0097M.CHECK_RESULT += "給付類別未定義";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核計費方式 M_PAYID
                                            if (AA0097M.M_PAYID_N != "")
                                            {
                                                if (AA0097M.M_PAYID_N != "1" && AA0097M.M_PAYID_N != "2")
                                                {
                                                    AA0097M.CHECK_RESULT += "計費方式未定義";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核扣庫方式 M_TRNID
                                            if (AA0097M.M_TRNID_N != "")
                                            {
                                                if (AA0097M.M_TRNID_N != "1" && AA0097M.M_TRNID_N != "2")
                                                {
                                                    AA0097M.CHECK_RESULT += "扣庫方式未定義";
                                                    AA0097M.CHECK_RESULT += ", ";
                                                }
                                            }

                                            //檢核是否盤盈虧 INV_POSTID
                                            //if (AA0097M.INV_POSTID_N != "")
                                            //{
                                            //    if (AA0097M.INV_POSTID_N != "Y" && AA0097M.INV_POSTID_N != "N")
                                            //    {
                                            //        AA0097M.CHECK_RESULT += "是否盤盈虧未定義";
                                            //        AA0097M.CHECK_RESULT += ", ";
                                            //    }
                                            //}

                                            //刪除最後的逗點
                                            if (AA0097M.CHECK_RESULT == "")
                                            {
                                                AA0097M.CHECK_RESULT = "OK";
                                            }
                                            else
                                            {
                                                AA0097M.CHECK_RESULT = AA0097M.CHECK_RESULT.Substring(0, AA0097M.CHECK_RESULT.Length - 2);
                                            }
                                        };
                                    }
                                }
                                if (AA0097M.CHECK_RESULT != "OK")
                                {
                                    checkPassed = false;
                                }
                                //產生一筆資料
                                list.Add(AA0097M);
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

        //確認更新
        [HttpPost]
        public ApiResponse Insert(FormDataCollection formData)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<AA0097M> aa0097m = JsonConvert.DeserializeObject<IEnumerable<AA0097M>>(formData["data"]);

                List<string> checkDuplicate = new List<string>();

                List<AA0097M> aa0097m_list = new List<AA0097M>();
                try
                {
                    var repo = new AA0097Repository(DBWork);
                    bool isDuplicate = false;

                    foreach (AA0097M data in aa0097m)
                    {

                        if (checkDuplicate.Contains(data.MMCODE)) //檢查list有沒有已經insert過的MMCODE
                        {
                            isDuplicate = true;
                            session.Result.msg = isDuplicate.ToString();
                            break;
                        }
                        else
                        {
                            data.UPDATE_USER = User.Identity.Name;
                            data.UPDATE_TIME = DateTime.Now;
                            data.UPDATE_IP = DBWork.ProcIP;
                            checkDuplicate.Add(data.MMCODE); //每次insert都把MMCODE寫入這個list

                            try
                            {
                                repo.Insert(data);
                                repo.Update(data);
                                if (!string.IsNullOrWhiteSpace(data.EXCH_RATIO_N))
                                {
                                    if (repo.CheckExistsEXCH_RATIO(data.MMCODE, data.M_PURUN_N) == true)
                                    {
                                        //如果在轉換率檔 MI_UNITEXCH 內已經有該筆院內碼+計量單位則用更新的
                                        repo.UpdateEXCH_RATIO(data.MMCODE, data.M_PURUN_N, data.EXCH_RATIO_N);
                                    }
                                    else
                                    {
                                        //如果在轉換率檔 MI_UNITEXCH 內還沒有該筆院內碼+計量單位則用插入的
                                        repo.InsertEXCH_RATIO(data.MMCODE, data.M_PURUN_N, data.EXCH_RATIO_N);
                                    }
                                }
                            }
                            catch
                            {
                                throw;
                            }
                            aa0097m_list.Add(data);
                        }
                    }

                    session.Result.etts = aa0097m_list;

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

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {

                    AA0097Repository repo = new AA0097Repository(DBWork);
                    using (var dataTable1 = repo.GetExcel())
                    {
                        string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MMdd");
                        JCLib.Excel.Export(dateTime + "_" + "新合約品項批次更新_物料主檔" + ".xls", dataTable1,
                            (dt) =>
                            {
                                return string.Format("說明：請輸入院內碼和欲更新的欄位，只有欄位名稱標明(新)的欄位修改有效，其餘僅供參考；不需輸入所有欄位，僅輸入欲更新欄位即可。");
                            }
                            );
                    }
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