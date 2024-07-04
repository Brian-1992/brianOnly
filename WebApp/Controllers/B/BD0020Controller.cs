using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Web;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace WebApp.Controllers.B
{
    public class BD0020Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0020Repository(DBWork);
                    session.Result.etts = repo.GetM(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetD(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    BD0020Repository repo = new BD0020Repository(DBWork);
                    session.Result.etts = repo.GetD(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterCreate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0020Repository repo = new BD0020Repository(DBWork);
                    MM_PR01_M mm_pr01_m = new MM_PR01_M();

                    mm_pr01_m.PR_USER = User.Identity.Name;
                    mm_pr01_m.CREATE_USER = User.Identity.Name;
                    mm_pr01_m.UPDATE_USER = User.Identity.Name;
                    mm_pr01_m.UPDATE_IP = DBWork.ProcIP;
                    mm_pr01_m.MEMO = form.Get("MEMO");

                    session.Result.msg = repo.MasterCreate(mm_pr01_m);

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
        public ApiResponse MasterDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0020Repository repo = new BD0020Repository(DBWork);
                    if (form.Get("PR_NO") != "")
                    {
                        string pr_no = form.Get("PR_NO").Replace("<br>", "^");
                        // 移除最後面的^
                        pr_no = pr_no.Substring(0, pr_no.Length - 1);
                        string[] pr_no_list = pr_no.Split('^');
                        for (int i = 0; i < pr_no_list.Length; i++)
                        {
                            string chkMsg = chkPrStatus(pr_no_list[i]);
                            if (chkMsg == "")
                            {
                                repo.DetailDeleteAll(pr_no_list[i]);
                                session.Result.afrs = repo.MasterDelete(pr_no_list[i]);
                            }
                            else
                            {
                                DBWork.Rollback();
                                session.Result.msg = chkMsg;
                                session.Result.success = false;
                                return session.Result;
                            }

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

        [HttpPost]
        public ApiResponse MasterUpdate(MM_PR01_M mm_pr01_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0020Repository repo = new BD0020Repository(DBWork);
                    mm_pr01_m.UPDATE_USER = User.Identity.Name;
                    mm_pr01_m.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.MasterUpdate(mm_pr01_m);
                    //session.Result.etts = repo.Get(me_docm.DOCNO);

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
        public ApiResponse MasterTrans(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0020Repository repo = new BD0020Repository(DBWork);
                    if (form.Get("PR_NO") != "")
                    {
                        string pr_no = form.Get("PR_NO").Replace("<br>", "^");
                        // 移除最後面的^
                        pr_no = pr_no.Substring(0, pr_no.Length - 1);
                        string[] temp_pr_no_list = pr_no.Split('^');

                        string pr_no_list = string.Empty;
                        for (int i = 0; i < temp_pr_no_list.Length; i++)
                        {
                            if (string.IsNullOrEmpty(pr_no_list) == false)
                            {
                                pr_no_list += ",";
                            }
                            pr_no_list += string.Format("'{0}'", temp_pr_no_list[i]);
                        }

                        // 取得 mat_class, agen_no, m_contid
                        IEnumerable<MM_PR01_D> temps = repo.GetAgaennoMContids(pr_no_list);

                        // 逐筆處理
                        foreach (MM_PR01_D temp in temps)
                        {
                            // 取得單號
                            string v_po_no = repo.GetPono();
                            // 取得v_memo
                            string v_memo = repo.GetMmPr01MMemos(pr_no_list, temp.AGEN_NO, temp.M_CONTID);
                            // 新增MM_PO_M
                            repo.CreatPoM(v_po_no, temp.AGEN_NO, temp.M_CONTID, temp.AGEN_TEL, v_memo, User.Identity.Name, DBWork.ProcIP);
                            // 新增MM_PO_D
                            repo.CreatPoD(v_po_no, pr_no_list, temp.AGEN_NO, temp.M_CONTID, User.Identity.Name, DBWork.ProcIP);
                            // 更新MM_PR01_D(將PO_NO回寫到申購資料以利後續從採購單追蹤)
                            repo.UpdatePr01D(v_po_no, pr_no_list, temp.AGEN_NO, temp.M_CONTID);
                            // 新增PH_INVOICE
                            repo.CreatPhInvoice(v_po_no, pr_no_list, temp.AGEN_NO, temp.M_CONTID, User.Identity.Name, DBWork.ProcIP);
                            // 新增MM_PO_INREC
                            repo.CreatPoInrec(v_po_no, pr_no_list, temp.AGEN_NO, temp.M_CONTID, User.Identity.Name, DBWork.ProcIP);
                        }


                        // 更新MM_PR01_M狀態
                        session.Result.afrs = repo.UpdateMM_PR01_M_STATUS(pr_no_list, User.Identity.Name, DBWork.ProcIP);
                    }
                    DBWork.Commit();
                }
                catch (Exception ex)
                {
                    session.Result.msg = ex.Message;
                    session.Result.success = false;
                    DBWork.Rollback();
                    //throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailCreate(MM_PR01_D mm_pr01_d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0020Repository repo = new BD0020Repository(DBWork);

                    mm_pr01_d.CREATE_USER = User.Identity.Name;
                    mm_pr01_d.UPDATE_IP = DBWork.ProcIP;

                    int unitRate = repo.GetUnitRate(mm_pr01_d.MMCODE);
                    int i;
                    if (int.TryParse(mm_pr01_d.PR_QTY, out i) == false)
                    {
                        session.Result.success = false;
                        session.Result.msg = "申購量需為數字，請重新確認";
                        return session.Result;
                    }

                    if (int.Parse(mm_pr01_d.PR_QTY) % unitRate != 0)
                    {
                        session.Result.success = false;
                        session.Result.msg = "申購量不為出貨單位倍數，請重新確認";
                        return session.Result;
                    }

                    //if (repo.CheckFlagMMCODE(mm_pr01_d.MMCODE) == false)
                    //{
                    //    session.Result.success = false;
                    //    session.Result.msg = "院內碼已全院停用，請重新確認";
                    //    return session.Result;
                    //}

                    if (repo.CheckDetailMmcodedExists(mm_pr01_d.PR_NO, mm_pr01_d.MMCODE))
                    {
                        string chkMmcodeValidRtn = chkMmcodeValid(mm_pr01_d.MMCODE, mm_pr01_d.PR_QTY);
                        if (chkMmcodeValidRtn == "")
                        {
                            session.Result.afrs = repo.DetailCreate(mm_pr01_d);
                            DBWork.Commit();
                        }
                        else
                        {
                            DBWork.Rollback();
                            session.Result.success = false;
                            session.Result.msg = chkMmcodeValidRtn;
                        }
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = mm_pr01_d.PR_NO + "中已有" + mm_pr01_d.MMCODE + "，請重新確認";
                    }

                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailDelete(MM_PR01_D mm_pr01_d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0020Repository repo = new BD0020Repository(DBWork);
                    string chkMsg = chkPrStatus(mm_pr01_d.PR_NO);

                    if (chkMsg == "")
                    {
                        session.Result.afrs = repo.DetailDelete(mm_pr01_d.PR_NO, mm_pr01_d.MMCODE);
                        DBWork.Commit();
                    }
                    else
                    {
                        DBWork.Rollback();
                        session.Result.msg = chkMsg;
                        session.Result.success = false;
                        return session.Result;
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
        public ApiResponse DetailUpdate(MM_PR01_D mm_pr01_d)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0020Repository repo = new BD0020Repository(DBWork);
                    string chkMmcodeValidRtn = chkMmcodeValid(mm_pr01_d.MMCODE, mm_pr01_d.PR_QTY);

                    MM_PR01_M prData = repo.GetChkPrStatus(mm_pr01_d.PR_NO);

                    int unitRate = repo.GetUnitRate(mm_pr01_d.MMCODE);
                    int i;
                    if (int.TryParse(mm_pr01_d.PR_QTY, out i) == false)
                    {
                        session.Result.success = false;
                        session.Result.msg = "申購量需為數字，請重新確認";
                        return session.Result;
                    }

                    if (prData.ISFROMDOCM == "Y" && Convert.ToInt32(mm_pr01_d.PR_QTY) < mm_pr01_d.ORI_PR_QTY)
                    {
                        chkMmcodeValidRtn = "轉申購之單據數量不可小於原需求量";
                    }

                    if (int.Parse(mm_pr01_d.PR_QTY) % unitRate != 0)
                    {
                        session.Result.success = false;
                        session.Result.msg = "申購量不為出貨單位倍數，請重新確認";
                        return session.Result;
                    }

                    if (chkMmcodeValidRtn == "")
                    {
                        mm_pr01_d.UPDATE_USER = User.Identity.Name;
                        mm_pr01_d.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.DetailUpdate(mm_pr01_d);

                        DBWork.Commit();
                    }
                    else
                    {
                        DBWork.Rollback();
                        session.Result.success = false;
                        session.Result.msg = chkMmcodeValidRtn;
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
        //檢查轉訂單是否有明細資料
        public ApiResponse GetDetailDataForT1TransOrders(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = 1;
            var limit = 100;
            var sorters = "";

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    BD0020Repository repo = new BD0020Repository(DBWork);
                    session.Result.etts = repo.GetD(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMATCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0020Repository(DBWork);
                    session.Result.etts = repo.GetMATCombo();
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
            var p0 = form.Get("p0");
            var MAT_CLASS = form.Get("MAT_CLASS");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0020Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo(MAT_CLASS, p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetSelectMmcodeDetail(string MMCODE, string MAT_CLASS)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0020Repository(DBWork);
                    session.Result.etts = repo.GetSelectMmcodeDetail(MMCODE, MAT_CLASS);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetWh_noCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0020Repository(DBWork);
                    session.Result.etts = repo.GetWh_noCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse GetMasterExceedAmtMMCodes(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0020Repository(DBWork);

                    string[] strSeparating = { "<br>" };
                    string[] strProNoList = form.Get("PR_NO").Split(strSeparating, System.StringSplitOptions.RemoveEmptyEntries);

                    foreach (string strPrNo in strProNoList)
                    {
                        IEnumerable<MM_PR_D> prnoList = repo.GetExceedAmtMMCodes(strPrNo);
                        if (prnoList.Count() > 0)
                        {
                            string strMMcodes = string.Join(", ", prnoList.Select(obj => obj.MMCODE));
                            string chkMsg = $"{strPrNo} = {strMMcodes} = 非合約累計採購金額預計超過(含)十五萬元，是否仍要產生訂單 ?";

                            session.Result.msg = chkMsg;
                            session.Result.etts = prnoList;
                            session.Result.success = true;
                            return session.Result;
                        }
                    }
                }
                catch (Exception e)
                {
                    throw;
                }

                session.Result.success = true;
                return session.Result;
            }
        }

        //匯入檢核
        [HttpPost]
        public ApiResponse SendExcel()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<MM_PR01_D> list = new List<MM_PR01_D>();
                List<MM_PR01_D> ori_list = new List<MM_PR01_D>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                var v_prno = HttpContext.Current.Request.Form["pr_no"];
                IWorkbook workBook;
                try
                {
                    BD0020Repository repo = new BD0020Repository(DBWork);
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

                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("申購數量", "PR_QTY"),
                        new HeaderItem("備註", "MEMO")
                    };
                    List<HeaderItem> headerItems2 = new List<HeaderItem>() {  //20231205 已統一格式將永遠不會跑到 headerItems2
                        new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("申購數量", "PR_QTY"),
                        new HeaderItem("備註", "MEMO"),
                    };

                    #region excel 欄位檢查

                    headerItems = SetHeaderIndex(headerItems, headerRow);
                    headerItems2 = SetHeaderIndex(headerItems2, headerRow);

                    // 確定該有的欄位都有，沒有的回傳錯誤訊息
                    string errMsg = string.Empty;
                    string errMsg2 = string.Empty;

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

                    //如果範本不符合 使用廠商提供格式
                    if (errMsg != string.Empty)
                    {
                        foreach (HeaderItem item in headerItems2)
                        {
                            if (item.Index == -1)
                            {
                                if (errMsg2 == string.Empty)
                                {
                                    errMsg2 += item.Name;
                                }
                                else
                                {
                                    errMsg2 += string.Format("、{0}", item.Name);
                                }
                            }
                        }
                        if (errMsg2 != string.Empty)
                        {
                            session.Result.success = false;
                            session.Result.msg = string.Format("欄位錯誤，缺：{0}", errMsg2);
                            return session.Result;
                        }

                    }

                    #endregion

                    #endregion

                    #region 資料轉成list
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        MM_PR01_D temp = new MM_PR01_D();
                        if (errMsg == string.Empty)
                        {
                            foreach (HeaderItem item in headerItems)
                            {
                                string value = "";

                                if (row.GetCell(item.Index) == null)
                                {
                                    continue;
                                }

                                if (item.Name == "申購數量")
                                {
                                    if (row.GetCell(item.Index).CellType == CellType.Error)
                                    {
                                        value = null;
                                    }
                                    else if (row.GetCell(item.Index).CellType == CellType.Formula)
                                    {
                                        if (row.GetCell(item.Index).CachedFormulaResultType == CellType.Error)
                                        {
                                            value = null;
                                        }
                                        else
                                        {
                                            value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).NumericCellValue.ToString();
                                        }
                                    }
                                    else
                                    {
                                        value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).NumericCellValue.ToString();
                                    }
                                }
                                else
                                {
                                    value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();

                                }
                                temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                            }
                        }
                        else
                        {
                            foreach (HeaderItem item in headerItems2)
                            {

                                string value = "";

                                if (row.GetCell(item.Index) == null)
                                {
                                    continue;
                                }

                                if (item.Name == "申購數量")
                                {
                                    if (row.GetCell(item.Index).CellType == CellType.Error)
                                    {
                                        value = null;
                                    }
                                    else if (row.GetCell(item.Index).CellType == CellType.Formula)
                                    {
                                        if (row.GetCell(item.Index).CachedFormulaResultType == CellType.Error)
                                        {
                                            value = null;
                                        }
                                        else
                                        {
                                            value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).NumericCellValue.ToString();
                                        }
                                    }
                                    else
                                    {
                                        value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).NumericCellValue.ToString();
                                    }
                                }
                                else
                                {
                                    value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();

                                }
                                temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                            }
                        }
                        temp.Seq = i - 1;
                        ori_list.Add(temp);
                    }
                    #endregion



                    DataTable dtTable = new DataTable();
                    DataRow datarow = dtTable.NewRow();

                    bool flowIdValid = repo.ChceckPrStatus(v_prno);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    foreach (MM_PR01_D mm_pr01_d in ori_list)
                    {
                        mm_pr01_d.PR_NO = v_prno;
                        mm_pr01_d.CHECK_RESULT = "OK";

                        //資料是否被使用者填入更新值
                        bool dataUpdated = false;

                        //如果有任何一格不是空的
                        if (string.IsNullOrEmpty(mm_pr01_d.MMCODE) == false || string.IsNullOrEmpty(mm_pr01_d.PR_QTY) == false)
                        {
                            //表示有效資料
                            dataUpdated = true;
                        }
                        //整理資料
                        if (string.IsNullOrEmpty(mm_pr01_d.PR_QTY))
                        {
                            mm_pr01_d.PR_QTY = "0";
                        }
                        if (string.IsNullOrEmpty(mm_pr01_d.MEMO))
                        {
                            mm_pr01_d.MEMO = "";
                        }

                        //若有待匯入資料
                        if (mm_pr01_d.MMCODE != "" && dataUpdated == true)
                        {
                            if (mm_pr01_d.PR_NO != v_prno)
                            {
                                mm_pr01_d.CHECK_RESULT = "此申購單號與匯入檢核申購單號不同";
                            }
                            else if (repo.CheckExistsPR_NO(mm_pr01_d.PR_NO) != true)
                            {
                                mm_pr01_d.CHECK_RESULT = "申購單號錯誤";
                            }
                            else if (repo.CheckExistsMMCODE(mm_pr01_d.MMCODE) != true)
                            {
                                mm_pr01_d.CHECK_RESULT = "院內碼不存在";
                            }
                            else if (repo.CheckFlagMMCODE(mm_pr01_d.MMCODE) != true)
                            {
                                mm_pr01_d.CHECK_RESULT = "院內碼已全院停用";
                            }
                            else if (repo.CheckPrExistsMMCODE(v_prno, mm_pr01_d.MMCODE) == true)
                            {
                                mm_pr01_d.CHECK_RESULT = "此單已有重複院內碼";
                            }
                            else if (CheckListDupMMCODE(mm_pr01_d.PR_NO, mm_pr01_d.MMCODE, mm_pr01_d.Seq, ori_list) != true)
                            {
                                mm_pr01_d.CHECK_RESULT = "匯入院內碼重複";
                            }
                            else if (repo.CheckExistsAGENNO(mm_pr01_d.MMCODE) != true)
                            {
                                mm_pr01_d.CHECK_RESULT = "未設定廠商代碼";
                            }
                            else if (mm_pr01_d.MEMO.Length > 200)
                            {
                                mm_pr01_d.CHECK_RESULT = "備註不可超過200字";
                            }
                            else
                            {
                                int unitRate = repo.GetUnitRate(mm_pr01_d.MMCODE);
                                int t;
                                if (int.TryParse(mm_pr01_d.PR_QTY, out t) == false)
                                {
                                    mm_pr01_d.CHECK_RESULT = string.Format("申購量 {0} 需為數字", mm_pr01_d.PR_QTY);
                                }
                                else
                                {
                                    t = int.Parse(mm_pr01_d.PR_QTY);

                                    if (t <= 0) // 申購單匯入時，數量小於等於0跳過不處理
                                    {
                                        //mm_pr01_d.CHECK_RESULT = string.Format("申購量 {0} 需為整數且大於0", mm_pr01_d.PR_QTY);
                                        continue;
                                    }

                                    if (t % unitRate != 0)
                                    {
                                        mm_pr01_d.CHECK_RESULT = string.Format("申購量 {0} 不為出貨單位 {1} 倍數", mm_pr01_d.PR_QTY, unitRate);
                                    }
                                }
                            }
                            if (mm_pr01_d.CHECK_RESULT == "OK")
                            {
                                mm_pr01_d.CHECK_RESULT = "通過";
                            };

                            if (mm_pr01_d.CHECK_RESULT != "通過")
                            {
                                checkPassed = false;
                            }
                            //產生一筆資料
                            list.Add(mm_pr01_d);
                        }
                    }
                    bool IsTotalPriceReached = false;
                    if (repo.GetTotalPrice(ori_list) > 150000)
                    {
                        IsTotalPriceReached = true;
                    }

                    session.Result.etts = list;
                    session.Result.msg = checkPassed.ToString() + "," + IsTotalPriceReached.ToString();
                }
                catch (Exception e)
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
            string PR_NO = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0020Repository repo = new BD0020Repository(DBWork);
                    JCLib.Excel.Export("BD0020.xls", repo.GetExcel(PR_NO));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 確認更新
        [HttpPost]
        public ApiResponse Insert(FormDataCollection formData)
        {
            var v_prno = formData.Get("pr_no");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<MM_PR01_D> mm_pr01_d = JsonConvert.DeserializeObject<IEnumerable<MM_PR01_D>>(formData["data"]);

                List<string> checkDuplicate = new List<string>();

                List<MM_PR01_D> mm_pr01_d_list = new List<MM_PR01_D>();
                try
                {
                    var repo = new BD0020Repository(DBWork);
                    bool isDuplicate = false;

                    foreach (MM_PR01_D data in mm_pr01_d)
                    {
                        bool flowIdValid = repo.ChceckPrStatus(v_prno);
                        if (flowIdValid == false)
                        {
                            session.Result.msg = "申請單狀態已變更，請重新查詢";
                            session.Result.success = false;
                            return session.Result;
                        }

                        if (int.Parse(data.PR_QTY) <= 0) // 申購單匯入時，數量小於等於0跳過不處理
                        {
                            continue;
                        }

                        data.PR_NO = v_prno;
                        if (checkDuplicate.Contains(data.MMCODE)) //檢查list有沒有已經insert過的MMCODE
                        {
                            isDuplicate = true;
                            session.Result.msg = isDuplicate.ToString();
                            break;
                        }
                        else
                        {
                            checkDuplicate.Add(data.MMCODE);

                            try
                            {
                                data.CREATE_USER = User.Identity.Name;
                                data.UPDATE_IP = DBWork.ProcIP;
                                session.Result.afrs = repo.DetailCreate(data);
                            }
                            catch
                            {
                                throw;
                            }
                            mm_pr01_d_list.Add(data);
                        }
                    }

                    session.Result.etts = mm_pr01_d_list;

                    if (isDuplicate == false)
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        DBWork.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        public string chkMmcodeValid(string mmcode, string pr_qty)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                BD0020Repository repo = new BD0020Repository(DBWork);
                string rtnStr = "";
                if (repo.CheckMastMmcodedExists(mmcode))
                {
                    if (repo.CheckMastAgennoExists(mmcode))
                    {
                        if (Convert.ToInt32(pr_qty) > 0)
                        {
                            rtnStr = "";
                        }
                        else
                        {
                            rtnStr = "申購數量需為整數且大於0，請重新確認";
                        }
                    }
                    else
                    {
                        rtnStr = mmcode + "廠商碼未更新或無此廠商碼，請重新確認";
                    }
                }
                else
                {
                    rtnStr = mmcode + "不存在或已全院停用，請重新確認";
                }
                return rtnStr;
            }

        }
        public string chkPrStatus(string pr_no)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                BD0020Repository repo = new BD0020Repository(DBWork);
                MM_PR01_M prData = repo.GetChkPrStatus(pr_no);

                if (prData.PR_STATUS != "35")
                    return pr_no + "狀態已變更，請重新確認";
                else if (prData.ISFROMDOCM == "Y")
                    return pr_no + "為申請轉申購，不可刪除";
                else
                    return "";
            }

        }

        public bool CheckListDupMMCODE(string pr_no, string mmcode, int? chkIdx, IEnumerable<MM_PR01_D> dt)//DataTable dt)
        {
            MM_PR01_D temp = null;
            return dt.Where(x => x.MMCODE == mmcode).Where(x => x.Seq != chkIdx).Select(x => x).Any() == false;

            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    if (dt.Rows[i]["申購單號"].ToString() == pr_no && dt.Rows[i]["院內碼"].ToString() == mmcode && i != chkIdx)
            //        return false;
            //}
            //return true;
        }

        #region 匯入比對欄位設定
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
                if (headerRow.GetCell(i) == null)
                {
                    continue;
                }

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
        #endregion
    }


}
