using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
using System.Data;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using System.Web;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Linq;
using Newtonsoft.Json;

namespace WebApp.Controllers.AA
{
    public class AA0125Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
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
                    var repo = new AA0125Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Get(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var loadType = form.Get("loadType");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0125Repository(DBWork);
                    session.Result.etts = repo.Get(p0, loadType);
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
        public ApiResponse Create(MI_MAST mi_mast)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0125Repository(DBWork);
                    if (repo.CheckMmcodeMatClass(mi_mast.MMCODE) == "02") {
                        session.Result.success = false;
                        session.Result.msg = "衛材新增請透過匯入或從HIS端修正";
                        return session.Result;
                    }
                    if (!repo.CheckExists(mi_mast.MMCODE))
                    {
                        mi_mast.CREATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_IP = DBWork.ProcIP;

                        var repo2 = new AA0038Repository(DBWork);
                        string mat_clsid = repo2.GetMatClsid(mi_mast.MAT_CLASS);


                        if (mi_mast.MIN_ORDQTY == "" || mi_mast.MIN_ORDQTY == null)
                            mi_mast.MIN_ORDQTY = "0";
                        if (mi_mast.EXCH_RATIO == "" || mi_mast.EXCH_RATIO == null)
                            mi_mast.EXCH_RATIO = "1";

                        // 2022-03-31註解，已於INF_SET.HIS_MAST_N中處理
                        ////2021-02-22: 檢查MI_UNITEXCH是否有資料，不存在新增，有存在更新
                        //if (repo.CheckUnitExchExists(mi_mast.MMCODE, mi_mast.M_AGENNO, mi_mast.M_PURUN) == false)
                        //{
                        //    repo.InsertUnitExch(mi_mast.MMCODE, mi_mast.M_AGENNO, mi_mast.M_PURUN, mi_mast.EXCH_RATIO, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        //}
                        //else
                        //{
                        //    // 檢查現有exch_ratio，相同不更新
                        //    string current_exch_ratio = repo.GetExchRatio(mi_mast.MMCODE, mi_mast.M_AGENNO, mi_mast.M_PURUN);
                        //    if (current_exch_ratio != mi_mast.EXCH_RATIO)
                        //    {
                        //        repo.UpdateUnitExch(mi_mast.MMCODE, mi_mast.M_AGENNO, mi_mast.M_PURUN, mi_mast.EXCH_RATIO, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        //    }
                        //}

                        // 新增及修改時,若為一般物品,能設,通信則計算最小單價,優惠合約單價,優惠最小單價
                        // 衛材不可在此新增,故對衛材的計算只在修改才有
                        if (mat_clsid == "3" || mat_clsid == "4" || mat_clsid == "5")
                        {
                            if (mi_mast.M_CONTPRICE == "" || mi_mast.M_CONTPRICE == null)
                                mi_mast.M_CONTPRICE = "0";
                            if (mi_mast.M_DISCPERC == "" || mi_mast.M_DISCPERC == null)
                                mi_mast.M_DISCPERC = "0";
                            else if (Convert.ToDouble(mi_mast.M_DISCPERC) > 100)
                                mi_mast.M_DISCPERC = "100";
                            mi_mast.UPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) / Convert.ToDouble(mi_mast.EXCH_RATIO), 4).ToString(); //最小單價=合約單價/廠商包裝轉換率 (4捨5入到小數4位)
                            mi_mast.DISC_CPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠合約單價=合約單價*(1-(折讓比/100)) (4捨5入到小數4位)
                            mi_mast.DISC_UPRICE = Math.Round(Convert.ToDouble(mi_mast.UPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠最小單價=最小單價*(1-(折讓比/100)) (4捨5入到小數4位)
                        }

                        session.Result.afrs = repo.Create(mi_mast);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼</span>重複，請重新輸入。";
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
        public ApiResponse Update(MI_MAST mi_mast)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0125Repository(DBWork);
                    if (mi_mast.CANCEL_ID == "Y" && repo.CheckMmcodeRef(mi_mast.MMCODE))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "此院內碼已被參考，不可修改為作廢。";
                    }
                    else
                    {
                        mi_mast.UPDATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_IP = DBWork.ProcIP;

                        // 新增及修改時,若為一般物品,能設,通信則計算最小單價,優惠合約單價,優惠最小單價
                        // 2020.05.19增加衛材也套用此規則
                        var repo2 = new AA0038Repository(DBWork);
                        string mat_clsid = repo2.GetMatClsid(mi_mast.MAT_CLASS);

                        MI_WINVCTL invctl = repo.GetWinvctl(mi_mast.MMCODE);

                        if (mi_mast.MIN_ORDQTY == "" || mi_mast.MIN_ORDQTY == null)
                            mi_mast.MIN_ORDQTY = invctl.MIN_ORDQTY.ToString();
                        if (mi_mast.EXCH_RATIO == "" || mi_mast.EXCH_RATIO == null)
                            mi_mast.EXCH_RATIO = "1";

                        // 2022-03-31註解，已於INF_SET.HIS_MAST_N中處理
                        ////2021-02-22: 檢查MI_UNITEXCH是否有資料，不存在新增，有存在更新
                        //if (repo.CheckUnitExchExists(mi_mast.MMCODE, mi_mast.M_AGENNO, mi_mast.M_PURUN) == false)
                        //{
                        //    repo.InsertUnitExch(mi_mast.MMCODE, mi_mast.M_AGENNO, mi_mast.M_PURUN, mi_mast.EXCH_RATIO, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        //}
                        //else
                        //{
                        //    // 檢查現有exch_ratio，相同不更新
                        //    string current_exch_ratio = repo.GetExchRatio(mi_mast.MMCODE, mi_mast.M_AGENNO, mi_mast.M_PURUN);
                        //    if (current_exch_ratio != mi_mast.EXCH_RATIO)
                        //    {
                        //        repo.UpdateUnitExch(mi_mast.MMCODE, mi_mast.M_AGENNO, mi_mast.M_PURUN, mi_mast.EXCH_RATIO, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        //    }
                        //}

                        if (mat_clsid == "2" || mat_clsid == "3" || mat_clsid == "4" || mat_clsid == "5")
                        {
                            if (mi_mast.M_CONTPRICE == "" || mi_mast.M_CONTPRICE == null)
                                mi_mast.M_CONTPRICE = "0";
                            if (mi_mast.M_DISCPERC == "" || mi_mast.M_DISCPERC == null)
                                mi_mast.M_DISCPERC = "0";
                            else if (Convert.ToDouble(mi_mast.M_DISCPERC) > 100)
                                mi_mast.M_DISCPERC = "100";
                            // 若合約單價為0且是衛材則不做計算
                            if (Convert.ToDouble(mi_mast.M_CONTPRICE) != 0 || mat_clsid != "2")
                            {
                                mi_mast.UPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) / Convert.ToDouble(mi_mast.EXCH_RATIO), 4).ToString(); //最小單價=合約單價/廠商包裝轉換率 (4捨5入到小數4位)
                                mi_mast.DISC_CPRICE = Math.Round(Convert.ToDouble(mi_mast.M_CONTPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠合約單價=合約單價*(1-(折讓比/100)) (4捨5入到小數4位)
                                mi_mast.DISC_UPRICE = Math.Round(Convert.ToDouble(mi_mast.UPRICE) * (1 - (Convert.ToDouble(mi_mast.M_DISCPERC) / 100)), 4).ToString(); // 優惠最小單價=最小單價*(1-(折讓比/100)) (4捨5入到小數4位)
                            }
                        }

                        session.Result.afrs = repo.Update(mi_mast);
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
        public ApiResponse GetMmcodeCombo(FormDataCollection form)
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
                    AA0125Repository repo = new AA0125Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpGet]
        public ApiResponse GetDefaultBeginDate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0125Repository repo = new AA0125Repository(DBWork);
                    session.Result.etts = repo.GetDefaultBeginDate();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region upload
        [HttpPost]
        public ApiResponse GetImportSampleExcel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0125Repository repo = new AA0125Repository(DBWork);

                    DataTable result = null;

                    result = repo.GetImportSampleExcel();

                    JCLib.Excel.Export("次月衛材基本檔異動範本.xls", result);

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
        public ApiResponse UploadCheck()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<MI_MAST> list = new List<MI_MAST>();
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0125Repository repo = new AA0125Repository(DBWork);

                    #region data upload
                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                       new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("中文品名", "MMNAME_C"),
                        new HeaderItem("申請申購識別碼", "M_APPLYID"),
                        new HeaderItem("最小撥補量", "MIN_ORDQTY"),
                        new HeaderItem("申購計量單位","M_PURUN"),
                        new HeaderItem("廠商包裝轉換率","EXCH_RATIO"),
                        new HeaderItem("廠商代碼","M_AGENNO"),
                        new HeaderItem("合約單價","M_CONTPRICE"),
                        new HeaderItem("折讓比","M_DISCPERC"),
                        new HeaderItem("庫備識別碼","M_STOREID"),
                        new HeaderItem("合約識別碼","M_CONTID"),
                        new HeaderItem("生效起日(民國年月日)","BEGINDATE"),
                        new HeaderItem("生效迄日(民國年月日)","ENDDATE"),
                        new HeaderItem("環保或衛署許可證","M_PHCTNCO"),
                        new HeaderItem("環保證號效期","M_ENVDT")
                    };

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

                    headerItems = SetHeaderIndex(headerItems, headerRow);
                    // 確定該有的欄位都有，沒有的回傳錯誤訊息
                    string errMsg = string.Empty;
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

                    if (errMsg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("欄位錯誤，缺：{0}", errMsg);
                        return session.Result;
                    }

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        MI_MAST temp = new MI_MAST();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                        }
                        temp.Seq = i - 1;
                        list.Add(temp);
                    }
                    #endregion

                    foreach (MI_MAST item in list)
                    {
                        item.SaveStatus = "Y";

                        // 確認院內碼是否存在於現有基本檔
                        if (!repo.CheckMmcodeExists(item.MMCODE))
                        {
                            item.UploadMsg = "院內碼不存在於現有基本檔";
                            item.SaveStatus = "N";
                            continue;
                        }
                        // 判斷MI_MAST_N中是否有存在
                        bool mimastn_exists = repo.CheckExists(item.MMCODE);

                        string msg = string.Empty;
                        string temp_msg = string.Empty;
                        // 取得原始資料，若MI_MAST_N有資料，以MI_MAST_N為主
                        MI_MAST temp = mimastn_exists ? repo.GetMiMastNByMmcode(item.MMCODE) : repo.GetMiMastByMmcode(item.MMCODE);

                        // 取得物料分類
                        item.MAT_CLASS = temp.MAT_CLASS;

                        if (!repo.CheckMatClsid(item.MAT_CLASS))
                        {
                            item.UploadMsg = "物料分類錯誤";
                            item.SaveStatus = "N";
                            continue;
                        }

                        // 檢查代碼是否存在，申請申購識別碼、合約識別碼、申購劑量單位、廠商代碼
                        // 檢查代碼是否存在，申請申購識別碼 M_APPLYID
                        if (item.M_APPLYID != string.Empty)
                        {
                            if (!repo.CheckParamExists("MI_MAST", "M_APPLYID", item.M_APPLYID))
                            {
                                temp_msg = "申請申購識別碼錯誤";
                                msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                item.SaveStatus = "N";
                            }
                        }
                        // 檢查代碼是否存在，申購計量單位 M_PURUN
                        if (item.M_PURUN != string.Empty)
                        {
                            if (!repo.CheckUnitCodeExists(item.M_PURUN))
                            {
                                temp_msg = "申購計量單位錯誤";
                                msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                item.SaveStatus = "N";
                            }
                        }

                        if (item.MAT_CLASS != "02")
                        {
                            if (item.M_CONTID != string.Empty)
                            {
                                // 檢查代碼是否存在，合約識別碼 M_CONTID
                                if (!repo.CheckParamExists("MI_MAST", "M_CONTID", item.M_APPLYID))
                                {
                                    temp_msg = "合約識別碼錯誤";
                                    msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                    item.SaveStatus = "N";
                                }
                            }

                            if (item.M_AGENNO != string.Empty)
                            {
                                // 檢查代碼是否存在，廠商代碼 M_PURUN
                                if (!repo.CheckAgennoExists(item.M_AGENNO))
                                {
                                    temp_msg = "廠商代碼錯誤";
                                    msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                    item.SaveStatus = "N";
                                }
                            }
                            // 若合約單價有填，計算優惠合約單價、最小單價、優惠最小單價
                            if (item.M_CONTPRICE != string.Empty)
                            {
                                string min_orderqty = string.IsNullOrEmpty(item.MIN_ORDQTY) == false ? item.MIN_ORDQTY : (string.IsNullOrEmpty(temp.MIN_ORDQTY) == false ? temp.MIN_ORDQTY : "0");
                                string exch_ratio = string.IsNullOrEmpty(item.EXCH_RATIO) == false ? item.EXCH_RATIO : (string.IsNullOrEmpty(temp.EXCH_RATIO) == false ? temp.EXCH_RATIO : "1");
                                string m_discperc = item.M_DISCPERC == string.Empty ? temp.M_DISCPERC : item.M_DISCPERC;

                                if (m_discperc == "" || m_discperc == null)
                                    m_discperc = "0";
                                else if (Convert.ToDouble(m_discperc) > 100)
                                    m_discperc = "100";

                                var repo2 = new AA0038Repository(DBWork);
                                string mat_clsid = repo2.GetMatClsid(item.MAT_CLASS);

                                if (m_discperc == string.Empty || m_discperc == null)
                                {
                                    temp_msg = "合約單價錯誤：需更新合約單價但無折讓比";
                                    msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                    item.SaveStatus = "N";
                                }
                                else
                                {
                                    if (mat_clsid == "2" || mat_clsid == "3" || mat_clsid == "4" || mat_clsid == "5")
                                    {
                                        // 若合約單價為0且是衛材則不做計算
                                        if (Convert.ToDouble(item.M_CONTPRICE) != 0 || mat_clsid != "2")
                                        {
                                            item.UPRICE = Math.Round(Convert.ToDouble(item.M_CONTPRICE) / Convert.ToDouble(exch_ratio), 4).ToString(); //最小單價=合約單價/廠商包裝轉換率 (4捨5入到小數4位)
                                            item.DISC_CPRICE = Math.Round(Convert.ToDouble(item.M_CONTPRICE) * (1 - (Convert.ToDouble(m_discperc) / 100)), 4).ToString(); // 優惠合約單價=合約單價*(1-(折讓比/100)) (4捨5入到小數4位)
                                            item.DISC_UPRICE = Math.Round(Convert.ToDouble(item.UPRICE) * (1 - (Convert.ToDouble(m_discperc) / 100)), 4).ToString(); // 優惠最小單價=最小單價*(1-(折讓比/100)) (4捨5入到小數4位)
                                        }
                                        else
                                        {
                                            item.UPRICE = "0";
                                            item.DISC_CPRICE = "0";
                                            item.DISC_UPRICE = "0";
                                        }
                                    }
                                }
                            }
                        }
                        if (item.BEGINDATE == string.Empty || repo.CheckBeginDateValid(item.BEGINDATE) == false)
                        {
                            // 檢查生效起日
                            temp_msg = "生效起日錯誤";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                        }
                        if (item.ENDDATE == string.Empty)
                        {
                            // 檢查生效迄日
                            temp_msg = "生效迄日錯誤";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                        }

                        if (item.MAT_CLASS == "02" &&
                            (item.M_AGENNO != string.Empty ||
                             item.M_CONTPRICE != string.Empty ||
                             item.M_STOREID != string.Empty ||
                             item.M_CONTID != string.Empty ||
                             item.M_PHCTNCO != string.Empty ||
                             item.M_ENVDT != string.Empty))
                        {
                            temp_msg = "衛材可更改項目為申請申購識別碼、最小撥補量、申購計量單位、廠商包裝轉換率，其餘欄位不更新";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                        }

                        item.UploadMsg = msg;
                    }

                    session.Result.etts = list;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

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

        [HttpPost]
        public ApiResponse UploadConfirm(FormDataCollection form)
        {
            bool isYear = form.Get("isYear") == "Y";
            string itemString = form.Get("data");
            IEnumerable<MI_MAST> list = JsonConvert.DeserializeObject<IEnumerable<MI_MAST>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0125Repository repo = new AA0125Repository(DBWork);

                    foreach (MI_MAST item in list)
                    {

                        if (item.MIN_ORDQTY == string.Empty || item.MIN_ORDQTY == null)
                        {
                            MI_WINVCTL invctl = repo.GetWinvctl(item.MMCODE);
                            item.MIN_ORDQTY = invctl.MIN_ORDQTY.ToString();
                        }

                        // 判斷MI_MAST_N中是否有存在
                        bool mimastn_exists = repo.CheckExists(item.MMCODE);

                        //2021-02-22: 檢查MI_UNITEXCH是否有資料，不存在新增，有存在更新
                        // 取得原始資料，若MI_MAST_N有資料，以MI_MAST_N為主，避免未修改找不到資料
                        MI_MAST temp = mimastn_exists ? repo.GetMiMastNByMmcode(item.MMCODE) : repo.GetMiMastByMmcode(item.MMCODE);

                        string query_agen_no = string.IsNullOrEmpty(item.M_AGENNO) ? temp.M_AGENNO : item.M_AGENNO;
                        string query_m_purun = string.IsNullOrEmpty(item.M_PURUN) ? temp.M_PURUN : item.M_PURUN;
                        string query_exch_ratio = string.IsNullOrEmpty(item.EXCH_RATIO) ? repo.GetExchRatio(item.MMCODE, query_agen_no, query_m_purun) : item.EXCH_RATIO;

                        // 2022-03-31註解，已於INF_SET.HIS_MAST_N中處理
                        //if (repo.CheckUnitExchExists(item.MMCODE, query_agen_no, query_m_purun) == false)
                        //{
                        //    repo.InsertUnitExch(item.MMCODE, query_agen_no, query_m_purun, query_exch_ratio, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        //}
                        //else
                        //{
                        //    // 檢查現有exch_ratio，相同不更新
                        //    string current_exch_ratio = repo.GetExchRatio(item.MMCODE, query_agen_no, query_m_purun);
                        //    if (current_exch_ratio != query_exch_ratio)
                        //    {
                        //        repo.UpdateUnitExch(item.MMCODE, query_agen_no, query_m_purun, query_exch_ratio, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        //    }
                        //}

                        item.EXCH_RATIO = query_exch_ratio;

                        if (mimastn_exists)
                        {
                            item.CREATE_USER = DBWork.UserInfo.UserId;
                            item.UPDATE_USER = DBWork.UserInfo.UserId;
                            item.UPDATE_IP = DBWork.ProcIP;
                            //有存在更新現有項目
                            session.Result.afrs += repo.UpdateMiMastN(item, false, isYear);
                        }
                        else
                        {
                            // 不存在新增
                            repo.InsertMiMastN(temp);
                            item.CREATE_USER = DBWork.UserInfo.UserId;
                            item.UPDATE_USER = DBWork.UserInfo.UserId;
                            item.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs += repo.UpdateMiMastN(item, true, isYear);
                        }
                    }

                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }

        }

        #endregion

        #region uploadY

        [HttpPost]
        public ApiResponse GetImportSampleYExcel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0125Repository repo = new AA0125Repository(DBWork);

                    DataTable result = null;

                    result = repo.GetImportSampleYExcel();

                    JCLib.Excel.Export("年度衛材基本檔異動範本.xls", result);

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
        public ApiResponse UploadCheckY()
        {
            using (WorkSession session = new WorkSession(this))
            {

                List<MI_MAST> list = new List<MI_MAST>();
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0125Repository repo = new AA0125Repository(DBWork);

                    #region data upload
                    List<HeaderItem> headerItems = new List<HeaderItem>() {
                       new HeaderItem("院內碼", "MMCODE"),
                        new HeaderItem("中文品名", "MMNAME_C"),
                        new HeaderItem("英文品名", "MMNAME_E"),
                        new HeaderItem("申請申購識別碼", "M_APPLYID"),
                        new HeaderItem("最小撥補量", "MIN_ORDQTY"),
                        new HeaderItem("申購計量單位","M_PURUN"),
                        new HeaderItem("廠商包裝轉換率","EXCH_RATIO"),
                        new HeaderItem("廠商代碼","M_AGENNO"),
                        new HeaderItem("合約單價","M_CONTPRICE"),
                        new HeaderItem("折讓比","M_DISCPERC"),
                        new HeaderItem("庫備識別碼","M_STOREID"),
                        new HeaderItem("合約識別碼","M_CONTID"),
                        new HeaderItem("生效起日(民國年月日)","BEGINDATE"),
                        new HeaderItem("生效迄日(民國年月日)","ENDDATE"),
                        new HeaderItem("環保或衛署許可證","M_PHCTNCO"),
                        new HeaderItem("環保證號效期","M_ENVDT"),
                        new HeaderItem("是否供應契約","M_SUPPLYID"),
                        new HeaderItem("長","M_VOLL"),
                        new HeaderItem("寬","M_VOLW"),
                        new HeaderItem("高","M_VOLH"),
                        new HeaderItem("圓周","M_VOLC"),
                        new HeaderItem("材積轉換率","M_SWAP")
                    };

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

                    headerItems = SetHeaderIndex(headerItems, headerRow);
                    // 確定該有的欄位都有，沒有的回傳錯誤訊息
                    string errMsg = string.Empty;
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

                    if (errMsg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("欄位錯誤，缺：{0}", errMsg);
                        return session.Result;
                    }

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        MI_MAST temp = new MI_MAST();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                        }
                        temp.Seq = i - 1;
                        list.Add(temp);
                    }
                    #endregion

                    foreach (MI_MAST item in list)
                    {
                        item.SaveStatus = "Y";

                        // 確認院內碼是否存在於現有基本檔
                        if (!repo.CheckMmcodeExists(item.MMCODE))
                        {
                            item.UploadMsg = "院內碼不存在於現有基本檔";
                            item.SaveStatus = "N";
                            continue;
                        }
                        // 判斷MI_MAST_N中是否有存在
                        bool mimastn_exists = repo.CheckExists(item.MMCODE);

                        string msg = string.Empty;
                        string temp_msg = string.Empty;
                        // 取得原始資料，若MI_MAST_N有資料，以MI_MAST_N為主
                        MI_MAST temp = mimastn_exists ? repo.GetMiMastNByMmcode(item.MMCODE) : repo.GetMiMastByMmcode(item.MMCODE);

                        // 取得物料分類
                        item.MAT_CLASS = temp.MAT_CLASS;

                        if (!repo.CheckMatClsid(item.MAT_CLASS))
                        {
                            item.UploadMsg = "物料分類錯誤";
                            item.SaveStatus = "N";
                            continue;
                        }

                        // 檢查代碼是否存在，申請申購識別碼、合約識別碼、申購劑量單位、廠商代碼
                        // 檢查代碼是否存在，申請申購識別碼 M_APPLYID
                        if (item.M_APPLYID != string.Empty)
                        {
                            if (!repo.CheckParamExists("MI_MAST", "M_APPLYID", item.M_APPLYID))
                            {
                                temp_msg = "申請申購識別碼錯誤";
                                msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                item.SaveStatus = "N";
                            }
                        }
                        // 檢查代碼是否存在，申購計量單位 M_PURUN
                        if (item.M_PURUN != string.Empty)
                        {
                            if (!repo.CheckUnitCodeExists(item.M_PURUN))
                            {
                                temp_msg = "申購計量單位錯誤";
                                msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                item.SaveStatus = "N";
                            }
                        }
                        // 檢查代碼是否存在，合約識別碼 M_CONTID
                        if (item.M_CONTID != string.Empty)
                        {
                            if (!repo.CheckParamExists("MI_MAST", "M_CONTID", item.M_CONTID))
                            {
                                temp_msg = "合約識別碼錯誤";
                                msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                item.SaveStatus = "N";
                            }
                        }
                        // 若合約單價有填，計算優惠合約單價、最小單價、優惠最小單價
                        if (item.M_CONTPRICE != string.Empty)
                        {
                            string min_orderqty = string.IsNullOrEmpty(item.MIN_ORDQTY) == false ? item.MIN_ORDQTY : (string.IsNullOrEmpty(temp.MIN_ORDQTY) == false ? temp.MIN_ORDQTY : "0");
                            string exch_ratio = string.IsNullOrEmpty(item.EXCH_RATIO) == false ? item.EXCH_RATIO : (string.IsNullOrEmpty(temp.EXCH_RATIO) == false ? temp.EXCH_RATIO : "1");
                            string m_discperc = item.M_DISCPERC == string.Empty ? temp.M_DISCPERC : item.M_DISCPERC;

                            if (m_discperc == "" || m_discperc == null)
                                m_discperc = "0";
                            else if (Convert.ToDouble(m_discperc) > 100)
                                m_discperc = "100";

                            var repo2 = new AA0038Repository(DBWork);
                            string mat_clsid = repo2.GetMatClsid(item.MAT_CLASS);

                            if (m_discperc == string.Empty || m_discperc == null)
                            {
                                temp_msg = "合約單價錯誤：更新合約單價但無折讓比";
                                msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                item.SaveStatus = "N";
                            }
                            else
                            {
                                if (mat_clsid == "2" || mat_clsid == "3" || mat_clsid == "4" || mat_clsid == "5")
                                {
                                    // 若合約單價為0且是衛材則不做計算
                                    if (Convert.ToDouble(item.M_CONTPRICE) != 0 || mat_clsid != "2")
                                    {
                                        item.UPRICE = Math.Round(Convert.ToDouble(item.M_CONTPRICE) / Convert.ToDouble(exch_ratio), 4).ToString(); //最小單價=合約單價/廠商包裝轉換率 (4捨5入到小數4位)
                                        item.DISC_CPRICE = Math.Round(Convert.ToDouble(item.M_CONTPRICE) * (1 - (Convert.ToDouble(m_discperc) / 100)), 4).ToString(); // 優惠合約單價=合約單價*(1-(折讓比/100)) (4捨5入到小數4位)
                                        item.DISC_UPRICE = Math.Round(Convert.ToDouble(item.UPRICE) * (1 - (Convert.ToDouble(m_discperc) / 100)), 4).ToString(); // 優惠最小單價=最小單價*(1-(折讓比/100)) (4捨5入到小數4位)
                                    }
                                    else
                                    {
                                        item.UPRICE = "0";
                                        item.DISC_CPRICE = "0";
                                        item.DISC_UPRICE = "0";
                                    }
                                }
                            }
                        }

                        // 一般物品判斷項目：廠商代碼 M_PURUN、是否供應契約 M_SUPPLYID (Y/N)
                        if (item.MAT_CLASS != "02")
                        {
                            if (item.M_AGENNO != string.Empty)
                            {
                                // 檢查代碼是否存在，廠商代碼 M_PURUN
                                if (!repo.CheckAgennoExists(item.M_AGENNO))
                                {
                                    temp_msg = "廠商代碼錯誤";
                                    msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                    item.SaveStatus = "N";
                                }
                            }

                            if (item.M_SUPPLYID != string.Empty)
                            {
                                // 檢查代碼是否存在，是否供應契約 M_SUPPLYID (Y/N)
                                if (item.M_SUPPLYID != "Y" && item.M_SUPPLYID != "N")
                                {
                                    temp_msg = "是否供應契約錯誤";
                                    msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                                    item.SaveStatus = "N";
                                }
                            }
                        }
                        if (item.BEGINDATE == string.Empty)
                        {
                            // 檢查生效起日
                            temp_msg = "生效起日錯誤";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                        }
                        if (item.ENDDATE == string.Empty)
                        {
                            // 檢查生效迄日
                            temp_msg = "生效迄日錯誤";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                        }

                        if (item.MAT_CLASS == "02" &&
                            (item.M_STOREID != string.Empty ||
                             item.M_SUPPLYID != string.Empty ||
                             item.M_PHCTNCO != string.Empty ||
                             item.M_ENVDT != string.Empty))
                        {
                            temp_msg = @"衛材可更改項目為申請申購識別碼、最小撥補量、申購計量單位、廠商包裝轉換率、中文品名、英文品名、長、寬、高、
                                         圓周、材積轉換率、合約單價、折讓比、合約識別碼、生效起日、生效迄日
                                        ，其餘欄位不更新";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                        }

                        if (item.MAT_CLASS != "02" &&
                            (item.MMNAME_E != string.Empty))
                        {
                            temp_msg = @"一般物品可更改項目為申請申購識別碼、最小撥補量、申購計量單位、廠商包裝轉換率、廠商代碼、合約單價、
                                         中文品名、長、寬、高、圓周、材積轉換率、庫備識別碼、合約識別碼、是否供應契約、環保或衛署許可證、環保證號效期、
                                         生效起日、生效迄日
                                        ，其餘欄位不更新";
                            msg = msg == string.Empty ? temp_msg : string.Format("{0}、{1}", msg, temp_msg);
                            item.SaveStatus = "N";
                        }

                        item.UploadMsg = msg;
                    }

                    session.Result.etts = list;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #endregion
    }
}