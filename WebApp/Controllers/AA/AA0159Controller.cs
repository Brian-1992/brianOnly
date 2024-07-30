using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using WebApp.Repository.UR;
using System;
using System.Web;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WebApp.Controllers.AA
{
    public class AA0159Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters);
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

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.Get(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetExcelExample(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    JCLib.Excel.Export("匯入藥材檔.xls", repo.GetExcelExample());
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
        public ApiResponse Create(MI_MAST_HISTORY mi_mast)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0159Repository(DBWork);
                    mi_mast.CREATE_USER = User.Identity.Name;
                    mi_mast.UPDATE_USER = User.Identity.Name;
                    mi_mast.UPDATE_IP = DBWork.ProcIP;
                    var EFFSTARTDATE = mi_mast.EFFSTARTDATE_T;
                    if (repo.CheckEffstratdate(EFFSTARTDATE))
                    {
                        mi_mast.MIMASTHIS_SEQ = repo.GetHisSeq();
                        mi_mast.EFFSTARTDATE = EFFSTARTDATE;
                        session.Result.afrs = repo.Create(mi_mast); //新增
                        var E_CODATE = mi_mast.E_CODATE_T;
                        if (E_CODATE != null)
                        {
                            mi_mast.E_CODATE = E_CODATE;
                            repo.UpdateHisECodate(mi_mast);
                        }
                        var BEGINDATE_14 = mi_mast.BEGINDATE_14_T;
                        if (BEGINDATE_14 != null)
                        {
                            mi_mast.BEGINDATE_14 = BEGINDATE_14;
                            repo.UpdateHisBegindate(mi_mast);
                        }
                        session.Result.etts = repo.Get(mi_mast.MMCODE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "生效日期必須大於今天。";
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
        public ApiResponse Update(MI_MAST_HISTORY mi_mast)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo1 = new AA0158Repository(DBWork);
                    if (mi_mast.CANCEL_ID == "Y" && repo1.CheckMmcodeRef(mi_mast.MMCODE))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "此院內碼已被參考，不可修改為作廢。";
                    }
                    else
                    {
                        var repo2 = new AA0159Repository(DBWork);
                        mi_mast.CREATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_IP = DBWork.ProcIP;

                        var EFFSTARTDATE = mi_mast.EFFSTARTDATE_T;
                        if (repo2.CheckEffstratdate(EFFSTARTDATE))
                        {
                            mi_mast.EFFSTARTDATE = EFFSTARTDATE;
                            var Maxseq = repo2.GetMaxSeq(mi_mast.MMCODE);
                            repo2.UpdateEffenddate(Maxseq, mi_mast.EFFSTARTDATE);
                            session.Result.afrs = repo2.Update(mi_mast); //更新

                            var E_CODATE = mi_mast.E_CODATE_T;
                            if (E_CODATE != null)
                            {
                                mi_mast.E_CODATE = E_CODATE;
                                repo2.UpdateHisECodate(mi_mast);
                            }
                            var BEGINDATE_14 = mi_mast.BEGINDATE_14_T;
                            if (BEGINDATE_14 != null)
                            {
                                mi_mast.BEGINDATE_14 = BEGINDATE_14;
                                repo2.UpdateHisBegindate(mi_mast);
                            }
                            session.Result.etts = repo2.Get(mi_mast.MMCODE);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "生效日期必須大於今天。";
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
        public ApiResponse Delete(FormDataCollection form)
        {
            var seq = form.Get("SEQ");
            var sdate = form.Get("SDATE");
            var mmcode = form.Get("MMCODE");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0159Repository(DBWork);
                    //var effstartdate = sdate.Substring(0, 4) + "/" + sdate.Substring(5, 2) + "/" + sdate.Substring(8, 2);
                    if (repo.CheckEffstratdate(sdate))
                    {
                        session.Result.afrs = repo.Delete(seq); //刪除
                        var Maxseq = repo.GetMaxSeq(mmcode);
                        repo.UpdateEffenddateN(Maxseq);
                        session.Result.etts = repo.Get(mmcode);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "生效日期必須大於今天，才可以刪除。";
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
        public ApiResponse GetMMCodeFromMastHisCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeFromMastHisCombo(p0, p1, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassSubCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetMatclassSubCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetBaseunitCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetBaseunitCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetYnCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetYnCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetRestriCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetRestriCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetESourceCodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetESourceCodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetERestrictCodeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetERestrictCodeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWarbakCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetWarbakCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetOnecostCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetOnecostCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetHealthPayCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetHealthPayCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetCostKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetCostKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWastKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetWastKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetSpXfeeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetSpXfeeCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetOrderKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetOrderKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetDrugKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetDrugKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetSpDrugCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetSpDrugCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetFastDrugCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetFastDrugCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMContidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetMContidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetTouchCaseCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetTouchCaseCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetCommonCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetCommonCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MStoreidComboGet()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.MStoreidComboGet();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAgennoCombo(FormDataCollection form)
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
                    AA0159Repository repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
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
            /**
             * 
             * 以下為 匯入彈出視窗用到的 Controllers
             * 
             */
            List<HeaderItem> headerItems = new List<HeaderItem>() {
                new HeaderItem("院內碼", "MMCODE"),
                new HeaderItem("生效起始時間", "EFFSTARTDATE"),
                new HeaderItem("是否作廢", "CANCEL_ID"),
                new HeaderItem("健保代碼", "M_NHIKEY"),
                new HeaderItem("健保自費碼", "HEALTHOWNEXP"),
                new HeaderItem("學名", "DRUGSNAME"),
                new HeaderItem("英文品名", "MMNAME_E"),
                new HeaderItem("中文品名", "MMNAME_C"),
                new HeaderItem("許可證號", "M_PHCTNCO"),
                new HeaderItem("許可證效期", "M_ENVDT"),
                new HeaderItem("申請廠商", "ISSUESUPPLY"),
                new HeaderItem("製造商", "E_MANUFACT"),
                new HeaderItem("藥材單位", "BASE_UNIT"),
                new HeaderItem("出貨包裝單位", "M_PURUN"),
                new HeaderItem("每包裝出貨量", "UNITRATE"),
                new HeaderItem("與HIS單位換算比值", "TRUTRATE"),
                //物料分類
                //new HeaderItem("物料分類", "MAT_CLASS"),
                new HeaderItem("物料子類別", "MAT_CLASS_SUB"),
                new HeaderItem("管制級數", "E_RESTRICTCODE"),
                new HeaderItem("是否戰備", "WARBAK"),
                new HeaderItem("是否可單一計價", "ONECOST"),
                new HeaderItem("是否健保給付", "HEALTHPAY"),
                new HeaderItem("費用分類", "COSTKIND"),
                new HeaderItem("是否正向消耗", "WASTKIND"),
                new HeaderItem("是否為特材", "SPXFEE"),
                new HeaderItem("採購類別", "ORDERKIND"),
                new HeaderItem("小採需求醫師", "CASEDOCT"),
                new HeaderItem("中西藥類別", "DRUGKIND"),
                new HeaderItem("廠商代碼", "M_AGENNO"),
                new HeaderItem("廠牌", "M_AGENLAB"),
                new HeaderItem("合約案號", "CASENO"),
                new HeaderItem("付款方式", "E_SOURCECODE"),
                new HeaderItem("合約方式", "M_CONTID"),
                new HeaderItem("聯標項次", "E_ITEMARMYNO"),
                new HeaderItem("健保價", "NHI_PRICE"),
                new HeaderItem("成本價", "DISC_CPRICE"),
                new HeaderItem("決標價", "M_CONTPRICE"),
                new HeaderItem("合約到期日", "E_CODATE"),
                new HeaderItem("聯標契約總數量", "CONTRACTAMT"),
                new HeaderItem("聯標項次契約總價", "CONTRACTSUM"),
                new HeaderItem("合約類別", "TOUCHCASE"),
                new HeaderItem("特殊品項", "SPDRUG"),
                new HeaderItem("急救品項", "FASTDRUG"),
                new HeaderItem("是否常用品項", "COMMON"),
                new HeaderItem("特材號碼", "SPMMCODE"),
                new HeaderItem("二次折讓數量", "DISCOUNT_QTY"),
                new HeaderItem("申請倍數", "APPQTY_TIMES"),
                new HeaderItem("二次優惠單價", "DISC_COST_UPRICE"),
                new HeaderItem("是否點滴", "ISIV"),
                new HeaderItem("庫備識別碼", "M_STOREID"),
            };

            using (WorkSession session = new WorkSession(this))
            {
                List<MI_MAST_HISTORY> list = new List<MI_MAST_HISTORY>();
                List<MI_MAST_HISTORY> final_list = new List<MI_MAST_HISTORY>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                IWorkbook workBook;
                try
                {
                    AA0159Repository repo = new AA0159Repository(DBWork);
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
                    IRow headerRow = sheet.GetRow(0);   //由第一列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum; //欄位數目
                    //int i, j;
                    #endregion

                    // 確定該有的欄位都有，沒有的回傳錯誤訊息
                    #region check excel header
                    headerItems = SetHeaderIndex(headerItems, headerRow);
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
                    # endregion

                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row is null) { continue; }

                        MI_MAST_HISTORY temp = new MI_MAST_HISTORY();
                        foreach (HeaderItem item in headerItems)
                        {
                            string value = row.GetCell(item.Index) == null ? string.Empty : row.GetCell(item.Index).ToString();
                            if (temp.GetType().GetProperty(item.FieldName).PropertyType == typeof(string))
                            {
                                temp.GetType().GetProperty(item.FieldName).SetValue(temp, value, null);
                            }
                            else if (temp.GetType().GetProperty(item.FieldName).PropertyType == typeof(DateTime))
                            {
                                if (!string.IsNullOrEmpty(value))
                                {
                                    DateTime d = DateTime.ParseExact(value, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                                    temp.GetType().GetProperty(item.FieldName).SetValue(temp, d, null);
                                }
                            }
                        }
                        list.Add(temp);
                    }

                    // 儲存 院內碼 的字串，如果 匯入時，此字串重複，直接過濾
                    List<string> checkList = new List<string>();

                    int j;
                    foreach (MI_MAST_HISTORY item in list)
                    {
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        bool breakloop = false;

                        // 檢查院內碼
                        #region 檢查 key 值
                        if (string.IsNullOrEmpty(item.MMCODE))
                        {
                            item.CHECK_RESULT = "院內碼為必填";
                            checkPassed = false;
                            final_list.Add(item);
                            continue;
                        }
                        else if (checkList.Contains(item.MMCODE))
                        {
                            item.CHECK_RESULT = "院內碼重複";
                            checkPassed = false;
                            final_list.Add(item);
                            continue;
                        }
                        checkList.Add(item.MMCODE);
                        #endregion

                        #region 檢查必填欄位
                        Dictionary<string, string> requiredCols;
                        // 如果不存在，代表資料為新增，必填欄位要有值
                        breakloop = false;
                        if (!repo.CheckExistsMMCODE(item.MMCODE)) // Create
                        {
                            requiredCols = new Dictionary<string, string>{
                                { "MMCODE", "院內碼" }, { "EFFSTARTDATE", "生效起始時間" },{ "E_RESTRICTCODE", "管制級數" },
                                { "WARBAK", "是否戰備" },
                                { "ONECOST", "是否可單一計價" }, { "HEALTHPAY", "是否健保給付" }, { "COSTKIND", "費用分類" },
                                { "WASTKIND", "是否正向消耗" }, { "SPXFEE", "是否為特材" }, { "ORDERKIND", "採購類別" },
                                { "DRUGKIND", "中西藥類別" }, { "E_SOURCECODE", "付款方式" }, { "M_CONTID", "合約方式" },
                                { "TOUCHCASE", "合約類別" }, { "SPDRUG", "特殊品項" }, { "FASTDRUG", "急救品項" },
                                { "COMMON", "是否常用品項" }
                            };
                        }
                        else // Update
                        {
                            requiredCols = new Dictionary<string, string>{
                                { "MMCODE", "院內碼" }
                            };
                        }

                        foreach (KeyValuePair<string, string> mapItem in requiredCols)
                        {
                            var value = item.GetType().GetProperty(mapItem.Key).GetValue(item, null).ToString();

                            if (string.IsNullOrEmpty(value))
                            {
                                item.CHECK_RESULT = mapItem.Value + " 為必填欄位";
                                checkPassed = false;
                                breakloop = true;
                                final_list.Add(item);
                                break;
                            }
                        }
                        if (breakloop) continue;
                        #endregion

                        #region 檢查 枚舉值
                        if (!string.IsNullOrEmpty(item.CANCEL_ID) && item.CANCEL_ID != "Y" && item.CANCEL_ID != "N")
                        {
                            item.CHECK_RESULT = "是否作廢:  必須等於Y, N";
                            checkPassed = false;
                            final_list.Add(item);
                            continue;
                        }

                        if (!string.IsNullOrEmpty(item.M_STOREID) && item.M_STOREID != "0" && item.M_STOREID != "1")
                        {
                            item.CHECK_RESULT = "庫備識別碼: 必須等於0, 1";
                            checkPassed = false;
                            final_list.Add(item);
                            continue;
                        }
                        #endregion

                        #region 檢查 數值欄位
                        Dictionary<string, string> numberCols = new Dictionary<string, string>{
                            { "DISCOUNT_QTY", "二次折讓數量" }, { "APPQTY_TIMES", "申請倍數" }, { "DISC_COST_UPRICE", "二次優惠單價" },
                            { "DISC_CPRICE", "成本價" }, { "M_CONTPRICE", "決標價" }, { "NHI_PRICE", "健保價" },
                            { "CONTRACTAMT", "聯標契約總數量" }, { "UNITRATE", "出貨單位" },
                        };
                        breakloop = false;
                        double dNumber;
                        foreach (KeyValuePair<string, string> mapItem in numberCols)
                        {
                            if (!string.IsNullOrEmpty(item.GetType().GetProperty(mapItem.Key).GetValue(item, null).ToString()))
                            {
                                var value = item.GetType().GetProperty(mapItem.Key).GetValue(item, null).ToString();

                                if (double.TryParse(value, out dNumber))
                                {
                                    if (dNumber < 0)
                                    {
                                        item.CHECK_RESULT = mapItem.Value + ":  必須大於等於1";
                                        checkPassed = false;
                                        final_list.Add(item);
                                        breakloop = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    item.CHECK_RESULT = mapItem.Value + ":  必須為數值";
                                    checkPassed = false;
                                    final_list.Add(item);
                                    breakloop = true;
                                    break;
                                }
                            }
                        }
                        if (breakloop) continue;
                        #endregion

                        #region 檢查 參數值
                        // check paramd
                        var paramds = new Dictionary<string, string>{
                                { "E_RESTRICTCODE", "管制級數" }, { "WARBAK", "是否戰備" }, { "ONECOST", "是否可單一計價" },
                                { "HEALTHPAY", "是否健保給付" }, { "COSTKIND", "費用分類" }, { "WASTKIND", "是否正向消耗" },
                                { "SPXFEE", "是否為特材" }, { "ORDERKIND", "採購類別" }, { "DRUGKIND", "中西藥類別" },
                                { "E_SOURCECODE", "付款方式" }, { "M_CONTID", "合約方式" }, { "TOUCHCASE", "合約類別" },
                                { "SPDRUG", "特殊品項" }, { "FASTDRUG", "急救品項" }, { "COMMON", "是否常用品項" }
                            };

                        breakloop = false;
                        foreach (KeyValuePair<string, string> mapItem in paramds)
                        {
                            if (!string.IsNullOrEmpty(item.GetType().GetProperty(mapItem.Key).GetValue(item, null).ToString()))
                            {
                                var value = item.GetType().GetProperty(mapItem.Key).GetValue(item, null).ToString();

                                if (!repo.CheckExistsPARAM_D(mapItem.Key, value))
                                {
                                    item.CHECK_RESULT = mapItem.Value + " 不存在或是有誤";
                                    checkPassed = false;
                                    breakloop = true;
                                    final_list.Add(item);
                                    break;
                                }
                            }
                        }
                        if (breakloop) continue;
                        #endregion

                        // 若無問題補上 OK
                        if (string.IsNullOrEmpty(item.CHECK_RESULT))
                        {
                            item.CHECK_RESULT = "OK";
                        }

                        final_list.Add(item);
                    }

                    session.Result.etts = final_list;
                    session.Result.msg = checkPassed.ToString();
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
        public ApiResponse InsertFromXls(FormDataCollection formData)
        {
            /**
             * 
             * 以下為 匯入彈出視窗用到的 Controllers
             * 
             */
            List<HeaderItem> headerItems = new List<HeaderItem>() {
                new HeaderItem("院內碼", "MMCODE"),
                new HeaderItem("生效起始時間", "EFFSTARTDATE"),
                new HeaderItem("是否作廢", "CANCEL_ID"),
                new HeaderItem("健保代碼", "M_NHIKEY"),
                new HeaderItem("健保自費碼", "HEALTHOWNEXP"),
                new HeaderItem("學名", "DRUGSNAME"),
                new HeaderItem("英文品名", "MMNAME_E"),
                new HeaderItem("中文品名", "MMNAME_C"),
                new HeaderItem("許可證號", "M_PHCTNCO"),
                new HeaderItem("許可證效期", "M_ENVDT"),
                new HeaderItem("申請廠商", "ISSUESUPPLY"),
                new HeaderItem("製造商", "E_MANUFACT"),
                new HeaderItem("藥材單位", "BASE_UNIT"),
                new HeaderItem("出貨包裝單位", "M_PURUN"),
                new HeaderItem("每包裝出貨量", "UNITRATE"),
                new HeaderItem("與HIS單位換算比值", "TRUTRATE"),
                //物料分類
                //new HeaderItem("物料分類", "MAT_CLASS"),
                new HeaderItem("物料子類別", "MAT_CLASS_SUB"),
                new HeaderItem("管制級數", "E_RESTRICTCODE"),
                new HeaderItem("是否戰備", "WARBAK"),
                new HeaderItem("是否可單一計價", "ONECOST"),
                new HeaderItem("是否健保給付", "HEALTHPAY"),
                new HeaderItem("費用分類", "COSTKIND"),
                new HeaderItem("是否正向消耗", "WASTKIND"),
                new HeaderItem("是否為特材", "SPXFEE"),
                new HeaderItem("採購類別", "ORDERKIND"),
                new HeaderItem("小採需求醫師", "CASEDOCT"),
                new HeaderItem("中西藥類別", "DRUGKIND"),
                new HeaderItem("廠商代碼", "M_AGENNO"),
                new HeaderItem("廠牌", "M_AGENLAB"),
                new HeaderItem("合約案號", "CASENO"),
                new HeaderItem("付款方式", "E_SOURCECODE"),
                new HeaderItem("合約方式", "M_CONTID"),
                new HeaderItem("聯標項次", "E_ITEMARMYNO"),
                new HeaderItem("健保價", "NHI_PRICE"),
                new HeaderItem("成本價", "DISC_CPRICE"),
                new HeaderItem("決標價", "M_CONTPRICE"),
                new HeaderItem("合約到期日", "E_CODATE"),
                new HeaderItem("聯標契約總數量", "CONTRACTAMT"),
                new HeaderItem("聯標項次契約總價", "CONTRACTSUM"),
                new HeaderItem("合約類別", "TOUCHCASE"),
                new HeaderItem("特殊品項", "SPDRUG"),
                new HeaderItem("急救品項", "FASTDRUG"),
                new HeaderItem("是否常用品項", "COMMON"),
                new HeaderItem("特材號碼", "SPMMCODE"),
                new HeaderItem("二次折讓數量", "DISCOUNT_QTY"),
                new HeaderItem("申請倍數", "APPQTY_TIMES"),
                new HeaderItem("二次優惠單價", "DISC_COST_UPRICE"),
                new HeaderItem("是否點滴", "ISIV"),
                new HeaderItem("庫備識別碼", "M_STOREID"),
            };

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<MI_MAST_HISTORY> mi_mast_historys = JsonConvert.DeserializeObject<IEnumerable<MI_MAST_HISTORY>>(formData["data"]);

                List<string> checkDuplicate = new List<string>();

                List<MI_MAST_HISTORY> mi_hist_list = new List<MI_MAST_HISTORY>();
                try
                {
                    var repo = new AA0159Repository(DBWork);

                    foreach (MI_MAST_HISTORY dataMI_MAST_HISTORY in mi_mast_historys)
                    {
                        dataMI_MAST_HISTORY.CREATE_USER = User.Identity.Name;
                        dataMI_MAST_HISTORY.UPDATE_USER = User.Identity.Name;
                        dataMI_MAST_HISTORY.UPDATE_IP = DBWork.ProcIP;

                        if (repo.CheckExistsMMCODE(dataMI_MAST_HISTORY.MMCODE))
                        {
                            MI_MAST_HISTORY mi_mast_history = repo.Get(dataMI_MAST_HISTORY.MMCODE).First();

                            foreach (HeaderItem headerItem in headerItems)
                            {
                                if (headerItem.FieldName == "MMCODE") continue;

                                string strFieldName = headerItem.FieldName;
                                var property = dataMI_MAST_HISTORY.GetType().GetProperty(strFieldName);

                                var newValue = property.GetValue(dataMI_MAST_HISTORY, null).ToString();

                                if (!string.IsNullOrEmpty(newValue))
                                {
                                    if (property.PropertyType == typeof(string))
                                    {
                                        mi_mast_history.GetType().GetProperty(strFieldName).SetValue(mi_mast_history, newValue.ToString());
                                    }
                                }
                            }
                            mi_mast_history.MONEYCHANGE = "Y";
                            repo.Update(mi_mast_history);
                        }
                        else
                        {
                            dataMI_MAST_HISTORY.MONEYCHANGE = "Y";
                            dataMI_MAST_HISTORY.MIMASTHIS_SEQ = repo.GetHisSeq();
                            repo.Create(dataMI_MAST_HISTORY);
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
        public ApiResponse GetTxt()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    ExportTxt();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

        }


        public void ExportTxt()
        {
            string fileName = "填寫說明.txt";

            if (HttpContext.Current == null) return;

            HttpResponse res = HttpContext.Current.Response;
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.GetEncoding("big5"));
            string line = Content();
            Encoding utf8 = Encoding.GetEncoding("utf-8");
            Encoding big51 = Encoding.GetEncoding("big5");
            sw.WriteLine(line); //write data
            sw.Close();
            res.BufferOutput = false;
            res.Clear();
            res.ClearHeaders();
            res.ContentEncoding = utf8;
            res.Charset = "utf-8";
            res.ContentType = "application/octet-stream;charset=utf-8";
            res.AddHeader("Content-Disposition",
                        "attachment; filename=" + HttpUtility.HtmlEncode(HttpUtility.UrlEncode(fileNameFilter(fileName, ".txt"), System.Text.Encoding.UTF8)));
            res.BinaryWrite(ms.ToArray());
            res.Flush();
            res.End();

            ms.Close();
            ms.Dispose();
        }

        static string fileNameFilter(string fileName, string extention)
        {
            string allowlist = @"^[\u4e00-\u9fa5_.\-a-zA-Z0-9]+$";
            fileName = fileName.Trim();
            Regex pattern = new Regex(allowlist);
            if (pattern.IsMatch(fileName))
            {
                return fileName;
            }
            else
            {
                return "downloadFile" + extention;
            }
        }

        public string Content()
        {
            // 以下文字可以用 純文字編輯 細明體 (或中英等寬字型)
            return @"
欄位  欄位名稱              是否必填    代碼說明                                         資料類型
A     院內碼                必填                                                         文數字13碼
B     學名                  可以不填                                                     文數字300碼
C     英文品名              可以不填    英文品名、中文品名 二個欄位至少填一個            文數字300碼
D     中文品名              可以不填    英文品名、中文品名 二個欄位至少填一個            文數字250碼
E     健保代碼              可以不填                                                     文數字20碼
F     健保自費碼            可以不填                                                     文數字16碼
G     許可證號    	    可以不填                                                     文數字120碼
H     許可證效期            可以不填                                                     文數字8碼
I     申請廠商              可以不填                                                     文數字128碼
J     製造商                可以不填                                                     文數字200碼
K     藥材單位              必填        相當於 舊藥衛材的「藥材單位」；
                                        必須先在「計量單位維護」功能建立資料             文數字6碼
L     出貨包裝單位          必填        出貨包裝單位 
                                        必須先在「計量單位維護」功能建立資料             文數字6碼
M     每包裝出貨量          必填        相當於 每包裝出貨量(單位/包裝)，
                                        每一包裝 等於 多少單位量                         數值
N     物料子類別       	    必填        相當於舊藥衛材的「類別代碼」                     文數字2碼
O     是否常用品項          必填        1：非常用品, 2：常用品, 3：藥品, 4：檢驗         文數字1碼
P     與HIS單位換算比值     必填        預設值為1                                        數值
Q     是否可單一計價        必填        0：不具計價性, 1：可單一計價, 2：不可單一計價    文數字1碼
R     是否健保給付          可以不填    0：不具計價性, 1：健保給付, 2：自費              文數字1碼
S     費用分類              可以不填    0：不具計價性, 1：材料費, 2：處置費,
                                        3：兩者皆有                                      文數字1碼
T     管制級數              必填        N：非管制用藥, 0：其它列管藥品, 
                                        1：第一級管制用藥, 2：第二級管制用藥,
                                        3：第三級管制用藥, 4：第四級管制用藥             文數字1碼
U     是否戰備              必填        0：非戰備, 1：戰備                               文數字1碼
V     是否正向消耗          必填        0：否, 1：是                                     文數字1碼
W     是否為特材            必填        0：非特材, 1：特材                               文數字1碼
X     採購類別              必填        0：無, 1：常備品項, 2：小額採購                  文數字1碼
Y     小採需求醫師          可以不填                                                     文數字30碼
Z     是否作廢              必填        N：使用, Y：作廢                                 文數字1碼
AA    庫備識別碼            必填        0：非庫備, 1：庫備； 
                                        除了804之外，其他醫院都填0；                     文數字1碼
AB    中西藥類別            必填        0：非藥品, 1：西藥, 2：中藥                      文數字1碼
AC    是否點滴              必填        N：非點滴, Y：點滴                               文數字1碼
AD    特殊品項              必填        0：非特殊品項, 1：特殊品項                       文數字1碼
AE    特材號碼              可以不填                                                     文數字12碼
AF    急救品項              必填        0：非急救品項, 1：急救品項                       文數字1碼
AG    申請倍數              可以不填    預設值為1                                        數值
AH    廠商代碼              可以不填    必須先在「廠商資料維護」功能建立資料             文數字8碼
AI    合約案號              可以不填                                                     文數字30碼
AJ    廠牌                  可以不填                                                     文數字64碼
AK    合約類別              必填        0：無合約, 1：院內選項, 2：非院內選項,
                                        3：院內自辦合約；                                文數字1碼
AL    合約到期日            可以不填                                                     日期
AM    付款方式              可以不填    P：買斷, C：寄售, R：核醫, N：其它               文數字1碼
AN    合約方式              可以不填    0：合約品項, 2：非合約, 3：不能申請(零購)        文數字1碼
AO    成本價                可以不填    優惠合約單價                                     數值
AP    決標價                可以不填    合約單價                                         數值
AQ    二次折讓數量          可以不填                                                     數值
AR    二次優惠單價          可以不填                                                     數值，小數2位
AS    聯標項次              可以不填    軍聯項次號                                       文數字20碼
AT    健保價                可以不填    健保給付價                                       數值
AU    聯標契約總數量        可以不填                                                     數值
AV    聯標項次契約總價      可以不填                                                     文數字30碼
";
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
        public ApiResponse GetMiMast(FormDataCollection form) {
            string mmcode = form.Get("mmcode");

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0159Repository(DBWork);
                    session.Result.etts = repo.GetMiMast(mmcode);
                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }

            }

        }
    }
}