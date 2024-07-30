using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using System.Web;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;

namespace WebApp.Controllers.AA
{
    public class AA0076Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0076Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p5, p6, page, limit, sorters, User.Identity.Name);
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
        public ApiResponse Create(MI_WEXPINV mi_wexpinv)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0076Repository(DBWork);

                    if (!repo.CheckWhnoExists(mi_wexpinv.WH_NO))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房代碼</span>不存在，請重新輸入。";

                        return session.Result;
                    }

                    if (!repo.CheckMmcodeExists(mi_wexpinv.MMCODE))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼</span>不存在，請重新輸入。";

                        return session.Result;
                    }

                    //if (!repo.CheckMeexpmMatch(mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, mi_wexpinv.EXP_DATE))
                    //{
                    //    session.Result.afrs = 0;
                    //    session.Result.success = false;
                    //    session.Result.msg = "<span style='color:red'>不曾進貨過此效期的院內碼</span>，請重新輸入。";

                    //    return session.Result;
                    //}

                    var curr_EXP_DATE = DateTime.Parse(mi_wexpinv.EXP_DATE).ToString("yyyy-MM-dd");
                    if (!repo.CheckPH_LOTNOExists(mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, curr_EXP_DATE) &&
                        !repo.CheckPH1SExpExists(mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, curr_EXP_DATE))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "此筆資料不存在基本檔";

                        return session.Result;
                    }

                    if (!repo.CheckExists(mi_wexpinv.WH_NO, mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, mi_wexpinv.EXP_DATE)) // 新增前檢查主鍵是否已存在
                    {
                        //var utility = new UtilityController();
                        //string dateNow = utility.convertEYtoTWY(System.DateTime.Now.ToString("yyyy-MM-dd"), "01");
                        mi_wexpinv.EXP_DATE = DateTime.Parse(mi_wexpinv.EXP_DATE).ToString("yyyy-MM-dd");
                        mi_wexpinv.CREATE_USER = User.Identity.Name;
                        mi_wexpinv.UPDATE_USER = User.Identity.Name;
                        mi_wexpinv.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(mi_wexpinv);
                        session.Result.etts = repo.Get(mi_wexpinv.WH_NO, mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, mi_wexpinv.EXP_DATE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "項目重複，請重新輸入。";
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
        public ApiResponse Update(MI_WEXPINV mi_wexpinv)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0076Repository(DBWork);

                    //if (!repo.CheckMeexpmMatch(mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, mi_wexpinv.EXP_DATE))
                    //{
                    //    session.Result.afrs = 0;
                    //    session.Result.success = false;
                    //    session.Result.msg = "<span style='color:red'>不曾進貨過此效期的院內碼</span>，請重新輸入。";

                    //    return session.Result;
                    //}
                    var curr_EXP_DATE = DateTime.Parse(mi_wexpinv.EXP_DATE).ToString("yyyy-MM-dd");

                    if (repo.CheckPH_LOTNOExists(mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, curr_EXP_DATE) ||
                        repo.CheckPH1SExpExists(mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, curr_EXP_DATE))
                    {
                        mi_wexpinv.EXP_DATE = DateTime.Parse(mi_wexpinv.EXP_DATE).ToString("yyyy-MM-dd");
                        mi_wexpinv.UPDATE_USER = User.Identity.Name;
                        mi_wexpinv.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Update(mi_wexpinv);
                        session.Result.etts = repo.Get(mi_wexpinv.WH_NO, mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, mi_wexpinv.EXP_DATE);

                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "此筆資料不存在基本檔";
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
        public ApiResponse Delete(MI_WEXPINV mi_wexpinv)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0076Repository(DBWork);
                    if (repo.CheckExists(mi_wexpinv.WH_NO, mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, mi_wexpinv.EXP_DATE))
                    {
                        session.Result.afrs = repo.Delete(mi_wexpinv.WH_NO, mi_wexpinv.MMCODE, mi_wexpinv.LOT_NO, mi_wexpinv.EXP_DATE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>記錄</span>不存在。";
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

        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0076Repository(DBWork);
                    if (repo.GetUserKind(User.Identity.Name).Contains("S"))
                    {
                        session.Result.etts = repo.GetWhnoCombo_S(User.Identity.Name);
                    }else if (repo.GetUserKind(User.Identity.Name).Contains("1"))
                    {
                        session.Result.etts = repo.GetWhnoCombo_1OrElse(User.Identity.Name);
                    }
                    else
                    {
                        session.Result.etts = repo.GetWhnoCombo_1OrElse(User.Identity.Name);
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
        public ApiResponse Getst_LOT_NO(FormDataCollection form)
        {
            var mmcode = form.Get("mmcode"); //mmcode


            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0076Repository repo = new AA0076Repository(DBWork);
                    session.Result.etts = repo.Getst_LOT_NO(mmcode);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Getst_EXP_DATE(FormDataCollection form)
        {
            var mmcode = form.Get("mmcode"); //mmcode
            var lot_no = form.Get("lot_no"); //lot_no


            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0076Repository repo = new AA0076Repository(DBWork);
                    session.Result.etts = repo.Getst_EXP_DATE(mmcode, lot_no);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
            var wh_no = form.Get("wh_no");
            var page = int.Parse(form.Get("page"));

            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0076Repository repo = new AA0076Repository(DBWork);
                    if (repo.GetUserKind(User.Identity.Name).Contains("S") ||
                        repo.GetUserKind(User.Identity.Name).Contains("1"))
                    {
                        session.Result.etts = repo.GetMMCODECombo(p0, wh_no, page, limit, "");
                    }
                    else
                    {
                        session.Result.etts = repo.GetMMCODECombo_else(p0, wh_no,User.Identity.Name, page, limit, "");
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
        public ApiResponse GetMmdataByMmcode(FormDataCollection form)
        {
            string mmcode = form.Get("MMCODE");
            string wh_no = form.Get("WH_NO");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0076Repository repo = new AA0076Repository(DBWork);
                    if (!repo.CheckWhmmExist(mmcode, wh_no))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此院內碼不存在於此庫房中</span>，請重新輸入。";

                        return session.Result;
                    }

                    session.Result.etts = repo.GetMmdataByMmcode(mmcode);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出Excel
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0076Repository repo = new AA0076Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN") + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(p0, User.Identity.Name));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //根據Excel匯入，與資料庫比對，是否要更新

        [HttpPost]
        public ApiResponse CheckExcel()
        {
            Boolean ifallOk = true; //若上傳筆數全數可以,則須更新,並寫出更新成功
            using (WorkSession session = new WorkSession(this))
            {

                List<MI_WEXPINV> listWithmsg = new List<MI_WEXPINV>();

                UnitOfWork DBWork = session.UnitOfWork;
                //string[] arr = { "WH_NO", "MMCODE", "EXP_DATE", "LOT_NO", "INV_QTY" };
                DBWork.BeginTransaction();
                try
                {
                    AA0076Repository repo = new AA0076Repository(DBWork);

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
                    int cellCount = headerRow.LastCellNum;
                    int i, j;
                    bool isValid = true;
                    bool isFirst = true;
                    string premmcode = "";
                    double sumInv_qty = 0.0;
                    #region 檢查檔案格式
                    for (j = 0; j < cellCount; j++)
                    {
                        isValid = headerRow.GetCell(j) == null ? false : true;  //如果檔案head格式為null，則isValid為false
                        if (!isValid)
                        {
                            session.Result.msg = "標頭有空白, 請確認格式";
                            break;
                        }
                    }


                    if (isValid)
                    {
                        isValid = headerRow.GetCell(0).ToString() == "庫房代碼" ? true : false;
                        if (isValid)
                        {
                            isValid = headerRow.GetCell(1).ToString() == "院內碼" ? true : false;
                        }
                        if (isValid)
                        {
                            isValid = headerRow.GetCell(2).ToString() == "效期" ? true : false;
                        }
                        if (isValid)
                        {
                            isValid = headerRow.GetCell(3).ToString() == "批號" ? true : false;
                        }
                        if (isValid)
                        {
                            isValid = headerRow.GetCell(4).ToString() == "效期批號庫存數量" ? true : false;
                        }
                    }

                    if (!isValid)
                    {
                        session.Result.msg = "檔案格式不同, 請確認格式";
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
                        for (i = 1; i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            datarow = dtTable.NewRow();
                            arrCheckResult = "OK"; //預設是OK
                            nullnum = 0; //有可能整列為空白，可以忽略
                            //依先前取得的欄位數逐一設定欄位內容
                            for (j = 0; j < cellCount; j++)
                            {
                                datarow[j] = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();

                                if (string.IsNullOrWhiteSpace(datarow[j].ToString()))
                                {
                                    nullnum++;
                                    arrCheckResult = "[" + headerRow.GetCell(j).StringCellValue + "]不可空白";
                                }
                            }
                            if (nullnum != cellCount)
                            {
                                //cellCount+1是目前 檢核結果 欄位
                                datarow[cellCount] = arrCheckResult;
                                dtTable.Rows.Add(datarow);
                                ifallOk = false;
                            }
                        }

                        dtTable.DefaultView.Sort = "院內碼";
                        dtTable = dtTable.DefaultView.ToTable();
                        #endregion

                        Boolean if_check = false;
                        #region 資料比對 INV_QTY
                        for (i = 0; i < dtTable.Rows.Count; i++)
                        {
                            if (dtTable.Rows[i]["檢核結果"].ToString() == "OK")
                            {
                                //是否存在TABLE PH_LOTNO 或 PH1S是否有此批號效期
                                if (!repo.CheckPH_LOTNOExists(dtTable.Rows[i]["院內碼"].ToString(), dtTable.Rows[i]["批號"].ToString(), dtTable.Rows[i]["效期"].ToString()) &&
                                    !repo.CheckPH1SExpExists(dtTable.Rows[i]["院內碼"].ToString(), dtTable.Rows[i]["批號"].ToString(), dtTable.Rows[i]["效期"].ToString()))
                                {
                                    ifallOk = false;
                                    dtTable.Rows[i]["檢核結果"] = "此筆資料不存在批號,效期,基本檔";

                                    //上一個與目前是不一樣(EX:狀態: 更新成功/不符 =>此資料不存在基本檔)
                                    if (if_check == true)
                                    {
                                        if (!premmcode.Equals(dtTable.Rows[i]["院內碼"].ToString()))
                                        {
                                            ifallOk = ComputeStatus(repo, dtTable, premmcode, sumInv_qty, i);

                                            isFirst = true; //狀態: 之後不是 此資料不存在基本檔 需要改變isFirst 值
                                            if_check = false;
                                        }
                                    }

                                }
                                else
                                {
                                    //總庫存數量判斷, insert delete MI_WEXPINV
                                    if_check = true;
                                    if (isFirst == true)
                                    {
                                        sumInv_qty = Convert.ToDouble(dtTable.Rows[i]["效期批號庫存數量"].ToString());
                                        isFirst = false;
                                    }
                                    else if (isFirst == false)
                                    {
                                        if (premmcode.Equals(dtTable.Rows[i]["院內碼"].ToString())) //上一個與目前是一樣
                                        {
                                            sumInv_qty = sumInv_qty + Convert.ToDouble(dtTable.Rows[i]["效期批號庫存數量"].ToString());
                                        }
                                        else if (!premmcode.Equals(dtTable.Rows[i]["院內碼"].ToString())) //上一個與目前是不一樣
                                        {
                                            ifallOk = ComputeStatus(repo, dtTable, premmcode, sumInv_qty, i);
                                            sumInv_qty = Convert.ToDouble(dtTable.Rows[i]["效期批號庫存數量"].ToString());
                                        }
                                    }

                                    //如果是最後一筆，檢核結果也要塞值
                                    if(dtTable.Rows.Count == (i + 1))
                                    {
                                        //抓目前的mmcode
                                        ifallOk = ComputeStatus(repo, dtTable, dtTable.Rows[i]["院內碼"].ToString(), sumInv_qty, i);
                                    }

                                }
                                premmcode = dtTable.Rows[i]["院內碼"].ToString();
                                //preinv_qty = dtTable.Rows[i]["效期批號庫存數量"].ToString();
                            }
                        }
                        #endregion
                        #region 如果都檢驗合格則一次更新
                        if (ifallOk == true)
                        {
                            for (int k = 0; k < dtTable.Rows.Count; k++)
                            {
                                //有,才刪除
                                if(repo.SelectMI_WEXPINV(dtTable.Rows[k]["庫房代碼"].ToString(), dtTable.Rows[k]["院內碼"].ToString()) >=1)
                                {
                                    repo.DeleteMI_WEXPINV(dtTable.Rows[k]["庫房代碼"].ToString(), dtTable.Rows[k]["院內碼"].ToString());
                                }

                                //在新增
                                var CREATE_USER = User.Identity.Name;
                                var UPDATE_USER = User.Identity.Name;
                                var UPDATE_IP = DBWork.ProcIP;
                                if (repo.SelectMI_WEXPINV2(dtTable.Rows[k]["庫房代碼"].ToString(), dtTable.Rows[k]["院內碼"].ToString(), dtTable.Rows[k]["效期"].ToString(), dtTable.Rows[k]["批號"].ToString(), dtTable.Rows[k]["效期批號庫存數量"].ToString()) == 0)
                                {
                                    repo.InsertMI_WEXPINV(dtTable.Rows[k]["庫房代碼"].ToString(), dtTable.Rows[k]["院內碼"].ToString(), dtTable.Rows[k]["效期"].ToString(), dtTable.Rows[k]["批號"].ToString(), dtTable.Rows[k]["效期批號庫存數量"].ToString(), CREATE_USER, UPDATE_USER, UPDATE_IP);
                                    dtTable.Rows[k]["檢核結果"] = "更新成功";
                                }
                                else //已經更新過
                                {
                                    dtTable.Rows[k]["檢核結果"] = "已經更新過";
                                }

                            }
                            DBWork.Commit();
                        }
                        #endregion
                        #region 輸出到前端
                        for (i = 0; i < dtTable.Rows.Count; i++)
                        {
                            MI_WEXPINV mi_WEXPINV = new MI_WEXPINV();
                            //顯示資料到GRID
                            mi_WEXPINV.WH_NO = dtTable.Rows[i]["庫房代碼"].ToString();
                            mi_WEXPINV.WH_NAME = repo.selectWH_NAME(dtTable.Rows[i]["庫房代碼"].ToString());
                            mi_WEXPINV.MMCODE = dtTable.Rows[i]["院內碼"].ToString();
                            mi_WEXPINV.MMNAME_C = repo.selectMMNAME_C(dtTable.Rows[i]["院內碼"].ToString());
                            mi_WEXPINV.MMNAME_E = repo.selectMMNAME_E(dtTable.Rows[i]["院內碼"].ToString());


                            //轉成民國年 2019-07-02  => 1080702 ; 如果是格式不正確會有錯誤
                            if(dtTable.Rows[i]["效期"].ToString().IndexOf("-") >1)
                            {
                                mi_WEXPINV.EXP_DATE = Convert.ToString(Convert.ToInt32(dtTable.Rows[i]["效期"].ToString().Substring(0, 4)) - 1911) + dtTable.Rows[i]["效期"].ToString().Substring(5, 2) + dtTable.Rows[i]["效期"].ToString().Substring(8, 2);
                                mi_WEXPINV.STATUS_DISPLAY = dtTable.Rows[i]["檢核結果"].ToString();
                            }
                            else
                            {
                                mi_WEXPINV.EXP_DATE = dtTable.Rows[i]["效期"].ToString();
                                mi_WEXPINV.STATUS_DISPLAY = dtTable.Rows[i]["檢核結果"].ToString()+ "</br>" + "日期格式有誤";

                            }
                            mi_WEXPINV.LOT_NO = dtTable.Rows[i]["批號"].ToString();
                            mi_WEXPINV.INV_QTY = dtTable.Rows[i]["效期批號庫存數量"].ToString();
                            listWithmsg.Add(mi_WEXPINV);
                        }
                        #endregion




                    }


                    if (!isValid)
                    {
                        session.Result.success = false;
                    }
                    else
                    {
                        session.Result.etts = listWithmsg;
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

        //檢驗是否通過
        public Boolean ComputeStatus(AA0076Repository repo, DataTable dtTable, string mmcode, Double sumInv_qty, int i)
        {
            Boolean ifallOk = true;
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;

                if (!repo.CheckInv_qty(dtTable.Rows[i]["庫房代碼"].ToString(), mmcode, sumInv_qty)) //上一個的檢核
                {
                    DataRow[] arrRows = dtTable.Select("庫房代碼='" + dtTable.Rows[i]["庫房代碼"].ToString() + "'" + " and 院內碼='" + mmcode + "'");
                    foreach (DataRow row in arrRows)
                    {
                        row["檢核結果"] = "效期批號庫存數量累計" + "</br>" + "與總庫存數量不符";
                        ifallOk = false;
                    }
                }
                else
                {
                    //表示數量正確,需要顯示更新成功(insert delete MI_WEXPINV)
                    DataRow[] arrRows = dtTable.Select("庫房代碼='" + dtTable.Rows[i]["庫房代碼"].ToString() + "'" + " and 院內碼='" + mmcode + "'");

                    foreach (DataRow row in arrRows)
                    {
                        row["檢核結果"] = "檢核通過";
                    }
                }

            }

            return ifallOk;

        }
    }
}