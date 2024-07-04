using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.C;

namespace WebApp.Controllers.C
{
    public class CE0002Controller : SiteBase.BaseApiController
    {
        #region params

        public class OnWayQtyCheck {
            public string WH_KIND { get; set; }
            public string DOCTYPES { get; set; }
            public string MAT_CLASSES { get; set; }
            public string CHK_TYPE { get; set; }
            public string FLOWIDS { get; set; }
            public string APPLY_KIND { get; set; }
        }

        Dictionary<string, List<string>> chkTypeList = new Dictionary<string, List<string>>() {
            { "02", new List<string>() { "02"} },
            { "07", new List<string>() { "07"} },
            { "08", new List<string>() { "08"} },
            { "0X", new List<string>() { "03", "04", "05", "06"} },
        };

        #endregion

        #region 主檔
        public ApiResponse MasterAll(FormDataCollection form)
        {

            var wh_no = form.Get("p0");
            var chk_ym = form.Get("p1");
            var keeper = form.Get("p2");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    IEnumerable<CHK_MAST> masters = repo.GetMasterAll(wh_no, chk_ym, keeper, page, limit, sorters);
                    foreach (CHK_MAST master in masters)
                    {
                        master.CHK_TYPE_NAME = repo.GetChkWhkindName(master.CHK_WH_KIND, master.CHK_TYPE);
                    }
                    session.Result.etts = masters;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpGet]
        public ApiResponse CurrentDate()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    int yyyy = DateTime.Now.Year;
                    int m = DateTime.Now.Month;
                    int d = DateTime.Now.Day;
                    string yyy = (yyyy - 1911).ToString();
                    string mm = m > 9 ? m.ToString() : string.Format("0{0}", m);
                    string dd = d > 9 ? d.ToString() : string.Format("0{0}", d);

                    string msg = string.Format("{0}{1}", yyy, mm);

                    session.Result.msg = msg;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        public ApiResponse InsertMaster(FormDataCollection form)
        {
            CHK_MAST master = new CHK_MAST();
            master.CHK_WH_NO = form.Get("CHK_WH_NO");
            master.CHK_YM = form.Get("CHK_YM");
            master.CHK_YMD = form.Get("CHK_YMD");
            master.CHK_CLASS = form.Get("CHK_CLASS");
            master.CHK_WH_GRADE = form.Get("CHK_WH_GRADE");
            master.CHK_WH_KIND = form.Get("CHK_WH_KIND");
            master.CHK_PERIOD = form.Get("CHK_PERIOD");
            master.CHK_TYPE = form.Get("CHK_TYPE");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    string canCreateMsg = CheckCanCreate(master);
                    if (canCreateMsg != string.Empty) {
                        session.Result.success = false;
                        session.Result.msg = canCreateMsg;
                        return session.Result;
                    }

                    string period = master.CHK_PERIOD == "D" ? "D" : "A";
                    // 2022-01-28: 以月結年月判斷開單seq，註解
                    //string chk_ym = period != "D" ? master.CHK_YM : master.CHK_YMD.Substring(0, 5);
                    string currentSeq = GetCurrentSeq(master.CHK_WH_NO, master.CHK_YM);
                    master.CHK_NO = string.Format("{0}{1}{2}{3}{4}", master.CHK_WH_NO, master.CHK_YM, period, master.CHK_TYPE, currentSeq);

                    if (master.CHK_PERIOD == "D")
                    {
                        master.CHK_YM = master.CHK_YMD;
                    }
                    master.CHK_KEEPER = DBWork.UserInfo.UserId;
                    master.CREATE_USER = DBWork.UserInfo.UserId;
                    master.UPDATE_USER = DBWork.UserInfo.UserId;
                    master.UPDATE_IP = DBWork.ProcIP;
                    // 月盤 / 季盤
                    if (master.CHK_PERIOD == "M" || master.CHK_PERIOD == "S")
                    {
                        if (repo.CheckExists(master.CHK_WH_NO, master.CHK_YM, master.CHK_PERIOD, master.CHK_TYPE, master.CHK_CLASS)) // 新增前檢查主鍵是否已存在
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "項目重複，請重新輸入。";
                            return session.Result;
                        }
                    }
                    else
                    {
                        string chk_status = repo.CheckPreStatus(master.CHK_WH_NO, master.CHK_YM, master.CHK_PERIOD, master.CHK_TYPE, master.CHK_CLASS);
                        if (chk_status != "3" && chk_status != "P" && chk_status !=  null)
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = string.Format("相同類別盤點單尚未完成，請先完成盤點後再另開盤點單");
                            return session.Result;
                        }
                    }

                    session.Result.afrs = repo.InsertChkMast(master);
                    AddAll(master.CHK_WH_NO, master.CHK_NO, master.CHK_WH_GRADE, master.CHK_WH_KIND, master.CHK_TYPE, master.CHK_CLASS, master.CHK_PERIOD);
                    
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
        public string CheckCanCreate(CHK_MAST master) {
            if (master.CHK_WH_KIND == "E" || master.CHK_WH_KIND == "C")
            {
                if (master.CHK_PERIOD != "M")
                {
                    return string.Format("{0}庫房僅能開立月盤單", master.CHK_WH_KIND == "E" ? "能設" : "通信");
                }
            }
            else if (master.CHK_WH_KIND == "1")
            {
                if (master.CHK_WH_GRADE == "1" && master.CHK_PERIOD == "D")
                {
                    return string.Format("中央庫房僅能開立月盤單");
                }
                if (master.CHK_WH_GRADE != "1" && master.CHK_PERIOD == "M")
                {
                    return string.Format("衛星庫房衛材庫盤點單由系統統一開立，無法自行開單");
                }
            }
            else if (master.CHK_WH_KIND == "0" &&
                    (master.CHK_WH_GRADE == "3" || master.CHK_WH_GRADE == "4") &&
                    master.CHK_PERIOD == "M" && master.CHK_TYPE == "3"
                    ) {
                return string.Format("衛星庫房藥品庫不可開立1~3級管制藥月盤單");
            }
            return string.Empty;
        }
        public string GetChkNo(CHK_MAST master)
        {
            string period = master.CHK_PERIOD == "D" ? "D" : "A";

            string chk_ym = period != "D" ? master.CHK_YM : master.CHK_YMD.Substring(0, 5);

            string currentSeq = GetCurrentSeq(master.CHK_WH_NO, chk_ym);
            return string.Format("{0}{1}{2}{3}{4}", master.CHK_WH_NO, master.CHK_YM, period, master.CHK_TYPE, currentSeq);
        }
        public string GetCurrentSeq(string wh_no, string ym)
        {
            string maxNo = string.Empty;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    maxNo = repo.GetCurrentSeq(wh_no, ym);
                }
                catch
                {
                    throw;
                }
                return maxNo;
            }
        }

        [HttpPost]
        public ApiResponse DeleteMaster(FormDataCollection form)
        {
            var itemString = form.Get("ITEM_STRING");
            var preLevel = form.Get("preLevel") == null ? string.Empty : form.Get("preLevel");
            IEnumerable<CHK_MAST> masters = JsonConvert.DeserializeObject<IEnumerable<CHK_MAST>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    foreach (CHK_MAST master in masters)
                    {

                        if (repo.CheckDetailEntry(master.CHK_NO) == false)
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = string.Format("{0} 已有數量輸入，無法刪除", master.CHK_NO);
                            return session.Result;
                        }
                        if (repo.CheckG2DetailEntry(master.CHK_NO) == false)
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = string.Format("{0} 已有數量輸入，無法刪除", master.CHK_NO);
                            return session.Result;
                        }


                        // 刪detail
                        session.Result.afrs = repo.DeleteChkDetail(master.CHK_NO);
                        // 刪科室病房人員對照
                        session.Result.afrs = repo.DeleteNouid(master.CHK_NO, string.Empty);
                        // 刪藥局盤點detail
                        session.Result.afrs = repo.DeleteG2Detail(master.CHK_NO, string.Empty);
                        session.Result.afrs = repo.DeleteDetailTempAll(master.CHK_NO);
                        // 刪藥局盤點庫存量
                        session.Result.afrs = repo.DeleteG2Whinv(master.CHK_NO);
                        // 刪藥局盤點輸入上傳檔
                        session.Result.afrs = repo.DeleteG2Updn(master.CHK_NO, string.Empty);
                        // 刪除藥庫存量記錄檔
                        session.Result.afrs = repo.DeleteChkPH1SWhinv(master.CHK_NO);
                        // 刪master
                        session.Result.afrs = repo.DeleteChkMast(master.CHK_NO);
                        // 復原前一盤狀態
                        if (preLevel != string.Empty)
                        {
                            session.Result.afrs = repo.UpdatePreMaster(master.CHK_NO1, preLevel, DBWork.UserInfo.UserId, DBWork.ProcIP);
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
        #endregion

        #region 盤點明細
        [HttpPost]
        public ApiResponse GetDetailInclude(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            var chk_status = form.Get("chk_status");
            var windowNewOpen = form.Get("windowNewOpen");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var list = form.Get("list");
            var sortersString = form.Get("sort");

            IEnumerable<CE0002Repository.Sorter> sorters = JsonConvert.DeserializeObject<IEnumerable<CE0002Repository.Sorter>>(sortersString);


            IEnumerable<CHK_DETAIL> currentIncludes = list == null ? new List<CHK_DETAIL>() : JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(list);
            //string currentMmcodes = GetCurrentIncludeMmcodes(currentIncludes);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    //if (int.Parse(chk_status) > 0) {
                    //    IEnumerable<CHK_DETAIL> includes = repo.GetIncludeDetails(chk_no, page, limit, sortersString, string.Empty);
                    //    includes = SetDetailValues(includes);
                    //    foreach (CE0002Repository.Sorter sorter in sorters) {
                    //        includes = includes.AsQueryable().OrderBy(string.Format("{0} {1}", sorter.property, sorter.direction)).ToList();
                    //    }

                    //    includes = includes.Take(page * 10).ToList();
                    //    session.Result.etts =  includes;
                    //    return session.Result;
                    //}

                    if (windowNewOpen == "Y")
                    {
                        repo.DeleteDetailTempAll(chk_no);

                        IEnumerable<CHK_DETAIL> includes = repo.GetIncludeDetails(chk_no, page, limit, string.Empty, string.Empty);
                        includes = SetDetailValues(includes);
                        foreach (CHK_DETAIL include in includes)
                        {
                            CHK_DETAIL_TEMP temp = GetDetailTempItem(include);
                            temp.CHK_NO = chk_no;
                            session.Result.afrs = repo.InsertDetailTemp(temp);
                        }
                    }

                    session.Result.etts = repo.DetailTempAll(chk_no, sortersString);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public IEnumerable<CHK_DETAIL> SetDetailValues(IEnumerable<CHK_DETAIL> details)
        {
            details = GetIncludeGroups(details);
            foreach (CHK_DETAIL detail in details)
            {
                string store_loc_name = string.Empty;
                string store_loc = string.Empty;
                int i = 0;
                double inv_qty = 0;
                foreach (CHK_DETAIL tempDetail in detail.tempDetails)
                {
                    i++;
                    if (tempDetail.STORE_QTYC == null || tempDetail.STORE_QTYC.Trim() == string.Empty)
                    {
                        tempDetail.STORE_QTYC = "0";
                    }
                    //store_loc_name += (tempDetail.STORE_LOC + "-" + tempDetail.STORE_QTYC.ToString());
                    store_loc_name += (tempDetail.STORE_LOC);
                    store_loc += (tempDetail.STORE_LOC);

                    if (inv_qty != double.Parse(tempDetail.STORE_QTYC))
                    {
                        inv_qty += double.Parse(tempDetail.STORE_QTYC);
                    }


                    if (i < detail.tempDetails.Count())
                    {
                        store_loc_name += "<br>";
                        store_loc += ",";
                    }
                }
                detail.STORE_LOC_NAME = store_loc_name;
                detail.STORE_LOC = store_loc;
                detail.INV_QTY = inv_qty;
                detail.CHK_QTY = GetChkQty(detail.tempDetails);
            }

            return details;
        }
        public string GetChkQty(IEnumerable<CHK_DETAIL> tempDetails) {
            double result = 0;
            bool hasChecked = false;
            foreach (CHK_DETAIL detail in tempDetails) {
                if (detail.CHK_TIME != string.Empty) {
                    hasChecked = true;
                    result += double.Parse(detail.CHK_QTY);
                }
            }
            if (hasChecked == false) {
                return string.Empty;
            }
            return result.ToString();
        }

        public CHK_DETAIL_TEMP GetDetailTempItem(CHK_DETAIL detail)
        {
            return new CHK_DETAIL_TEMP()
            {
                CHK_NO = detail.CHK_NO,
                WH_NO = detail.WH_NO,
                MMCODE = detail.MMCODE,
                MMNAME_C = detail.MMNAME_C,
                MMNAME_E = detail.MMNAME_E,
                BASE_UNIT = detail.BASE_UNIT,
                INV_QTY = detail.INV_QTY.ToString(),
                CHK_QTY = detail.CHK_QTY,
                STORE_LOC = detail.STORE_LOC,
                STORE_LOC_NAME = detail.STORE_LOC_NAME,
                CHK_UID = detail.CHK_UID
            };
        }

        public IEnumerable<CHK_DETAIL> GetIncludeGroups(IEnumerable<CHK_DETAIL> includes)
        {
            var o = from a in includes
                    group a by new
                    {
                        WH_NO = a.WH_NO,
                        MMCODE = a.MMCODE,
                        MMNAME_C = a.MMNAME_C,
                        MMNAME_E = a.MMNAME_E,
                        BASE_UNIT = a.BASE_UNIT,
                        M_PURUN = a.M_PURUN,
                        M_CONTPRICE = a.M_CONTPRICE,
                        MAT_CLASS = a.MAT_CLASS,
                        M_STOREID = a.M_STOREID,
                        CHK_REMARK = a.CHK_REMARK,
                        CHK_UID = a.CHK_UID,
                        CHK_UID_NAME = a.CHK_UID_NAME,
                        STATUS_INI = a.STATUS_INI,
                        STATUS_INI_NAME = a.STATUS_INI_NAME,
                    } into g
                    select new CHK_DETAIL
                    {
                        WH_NO = g.Key.WH_NO,
                        MMCODE = g.Key.MMCODE,
                        MMNAME_C = g.Key.MMNAME_C,
                        MMNAME_E = g.Key.MMNAME_E,
                        BASE_UNIT = g.Key.BASE_UNIT,
                        M_PURUN = g.Key.M_PURUN,
                        M_CONTPRICE = g.Key.M_CONTPRICE,
                        MAT_CLASS = g.Key.MAT_CLASS,
                        M_STOREID = g.Key.M_STOREID,
                        CHK_REMARK = g.Key.CHK_REMARK,
                        CHK_UID = g.Key.CHK_UID,
                        CHK_UID_NAME = g.Key.CHK_UID_NAME,
                        STATUS_INI = g.Key.STATUS_INI,
                        STATUS_INI_NAME = g.Key.STATUS_INI_NAME,
                        tempDetails = g.ToList()
                    };

            return o.ToList();
        }

        [HttpPost]
        public ApiResponse GetDetailExclude(FormDataCollection form)
        {
            // GetExcludeDetails
            var wh_no = form.Get("wh_no");
            var chk_no = form.Get("chk_no");
            var chk_wh_grade = form.Get("chk_wh_grade");
            var chk_wh_kind = form.Get("chk_wh_kind");
            var chk_type = form.Get("chk_type");
            var chk_class = form.Get("chk_class");
            var chk_period = form.Get("chk_period");
            var f_u_price = form.Get("F_U_PRICE") == null ? 0 : float.Parse(form.Get("F_U_PRICE"));
            var f_number = form.Get("F_NUMBER") == null ? 0 : float.Parse(form.Get("F_NUMBER"));
            var f_amount = form.Get("F_AMOUNT") == null ? 0 : float.Parse(form.Get("F_AMOUNT"));

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            IEnumerable<CHK_DETAIL> excludes = new List<CHK_DETAIL>();

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    if (wh_no == "ANE1")
                    {
                        excludes = repo.GetExcludeDetailsANE(wh_no, chk_type, chk_no);
                        excludes = excludes.GroupBy(x => x.MMCODE).Select(g => g.First()).ToList();
                        session.Result.etts = excludes;
                        return session.Result;
                    }


                    excludes = repo.GetExcludeDetails(wh_no, chk_wh_grade, chk_wh_kind, chk_type, chk_no, chk_class, chk_period, f_u_price, f_number, f_amount, page, limit, sorters);

                    excludes = excludes.GroupBy(x => x.MMCODE).Select(g => g.First()).ToList();

                    session.Result.etts = excludes;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse InsertDetailTemp(FormDataCollection form)
        {
            var listString = form.Get("list");
            var chk_wh_grade = form.Get("chk_wh_grade");
            IEnumerable<CHK_DETAIL_TEMP> list = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL_TEMP>>(listString);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    foreach (CHK_DETAIL_TEMP item in list)
                    {
                        if (chk_wh_grade == "1")
                        {
                            if (item.STORE_LOC.Trim() == string.Empty)
                            {
                                //continue;
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "所選品項無儲位，請先設定。";
                                return session.Result;
                            }


                        }
                        item.INV_QTY = item.STORE_QTYC;
                        session.Result.afrs = repo.InsertDetailTemp(item);
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
        public ApiResponse DeleteDetailTemp(FormDataCollection form)
        {
            var listString = form.Get("list");
            IEnumerable<CHK_DETAIL_TEMP> list = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL_TEMP>>(listString);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    foreach (CHK_DETAIL_TEMP item in list)
                    {
                        session.Result.afrs = repo.DeleteDetailTemp(item);
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
        public ApiResponse DeleteDetailTemps(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    session.Result.afrs = repo.DeleteDetailTempAll(chk_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailSave(FormDataCollection form)
        {
            //IEnumerable<CHK_DETAIL> details = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(detailInput.ITEM_STRING);
            var chk_no = form.Get("chk_no");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    //CHK_DETAIL firstItem = details.First<CHK_DETAIL>();
                    // 刪除原本detail
                    session.Result.afrs = repo.DeleteChkDetail(chk_no);
                    string chk_wh_grade = repo.GetChkWhGrade(chk_no);
                    string chk_wh_kind = repo.GetChkWhKind(chk_no);

                    //foreach (CHK_DETAIL detail in details)
                    //{
                    //    detail.CHK_QTY = "0";
                    //    detail.CHK_REMARK = "";
                    //    detail.CHK_TIME = "";
                    //    detail.STATUS_INI = "0";
                    //    detail.CREATE_USER = DBWork.UserInfo.UserId;
                    //    detail.UPDATE_USER = DBWork.UserInfo.UserId;
                    //    detail.UPDATE_IP = DBWork.ProcIP;

                    //    // 新增detail
                    //    session.Result.afrs = repo.InsertChkdetail(detail);
                    //}
                    IEnumerable<CHK_DETAIL_TEMP> temps = repo.DetailTempAll(chk_no, string.Empty);

                    foreach (CHK_DETAIL_TEMP temp in temps)
                    {
                        CHK_DETAIL detail = new CHK_DETAIL()
                        {
                            CHK_NO = chk_no,
                            WH_NO = temp.WH_NO,
                            MMCODE = temp.MMCODE,
                            STATUS_INI = "0",
                            CHK_UID = temp.CHK_UID,
                            CREATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_IP = DBWork.ProcIP
                        };
                        session.Result.afrs = repo.InsertChkdetail(detail, chk_wh_grade, chk_wh_kind);
                    }



                    repo.DeleteDetailTempAll(chk_no);

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
        public ApiResponse GetPickUsers(FormDataCollection form)
        {
            string wh_no = form.Get("wh_no");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    session.Result.etts = repo.GetPickUsers(wh_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CreateSheet(FormDataCollection form)
        {
            var detailsString = form.Get("details");
            var chk_no = form.Get("chk_no");
            var chk_wh_kind = form.Get("chk_wh_kind");
            var chk_wh_grade = form.Get("chk_wh_grade");
            var usersString = form.Get("users");
            var wh_kind = form.Get("wh_kind");
            var orderway = form.Get("orderway");
            var is_distri = (form.Get("is_distri") == "true");

            //IEnumerable<CHK_DETAIL> details = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(detailsString);
            IEnumerable<BC_WHCHKID> users = JsonConvert.DeserializeObject<IEnumerable<BC_WHCHKID>>(usersString);

            //details = details.OrderBy(x => x.MMCODE).ThenBy(x => x.STORE_LOC).ToList<CHK_DETAIL>();
            //details = details.AsQueryable().OrderBy(string.Format("{0} ASC", orderway)).ToList();
            //details = OrderByAsc(details, orderway);


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    IEnumerable<CHK_DETAIL_TEMP> temps = repo.DetailTempOrder(chk_no, new CE0002Repository.Sorter() { property = orderway });
                    IEnumerable<CHK_DETAIL> details = GetChkDetails(temps);
                    // 中央庫房 藥庫 能設 通信
                    if (chk_wh_grade == "1")
                    {
                        if (chk_wh_kind == "1")
                        {
                            return setWhkind1(chk_no,details, chk_wh_grade);
                        }
                        else if (chk_wh_kind == "0")
                        {
                            if (is_distri)
                            {
                                return setWhkind0DistY(chk_no, details, users, chk_wh_grade);
                            }
                            else
                            {
                                string wh_no = details.FirstOrDefault().WH_NO;
                                users = repo.GetPickUsers(wh_no);
                                return setWhkind0DistN(chk_no, details, users, chk_wh_grade);
                            }

                        }
                        else
                        {
                            foreach (CHK_DETAIL detail in details)
                            {
                                detail.WH_KIND = chk_wh_kind;
                            }

                            return setWhkindCE(details);
                        }
                    }

                    // 科室病房
                    return setWard(chk_no, details, users, chk_wh_grade, chk_wh_kind);



                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        public IEnumerable<CHK_DETAIL> GetChkDetails(IEnumerable<CHK_DETAIL_TEMP> temps)
        {
            List<CHK_DETAIL> list = new List<CHK_DETAIL>();
            foreach (CHK_DETAIL_TEMP temp in temps)
            {
                CHK_DETAIL detail = new CHK_DETAIL()
                {
                    CHK_NO = temp.CHK_NO,
                    WH_NO = temp.WH_NO,
                    MMCODE = temp.MMCODE,
                    CHK_UID = temp.CHK_UID
                };
                list.Add(detail);
            }
            return list;
        }
        public ApiResponse setWhkind1(string chk_no, IEnumerable<CHK_DETAIL> details, string chk_wh_kind)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    CHK_DETAIL firstItem = details.First<CHK_DETAIL>();
                    // 刪除原本detail
                    session.Result.afrs = repo.DeleteChkDetail(firstItem.CHK_NO);
                    string chk_wh_grade = repo.GetChkWhGrade(firstItem.CHK_NO);

                    foreach (CHK_DETAIL detail in details)
                    {
                        detail.CHK_QTY = "0";
                        detail.CHK_REMARK = "";
                        detail.CHK_TIME = "";
                        detail.STATUS_INI = "1";
                        detail.CREATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_IP = DBWork.ProcIP;

                        // 新增detail
                        session.Result.afrs = repo.InsertChkdetail(detail, chk_wh_grade, "1");
                    }

                    if (repo.GetDetailCount(details.Take(1).FirstOrDefault().CHK_NO) == 0)
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "盤點單無盤點項目，請重新確認。";
                        return session.Result;
                    }

                    session.Result.afrs = repo.UpdateMaster(firstItem.CHK_NO);

                    repo.DeleteDetailTempAll(firstItem.CHK_NO);

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
        public ApiResponse setWhkind0DistN(string chk_no, IEnumerable<CHK_DETAIL> details, IEnumerable<BC_WHCHKID> users, string chk_wh_grade)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    // 刪除原本detail
                    session.Result.afrs = repo.DeleteChkDetail(chk_no);

                    List<CHK_DETAIL> temps = details.GroupBy(x => x.MMCODE).Select(g => g.First()).ToList();

                    foreach (CHK_DETAIL detail in details)
                    {
                        detail.STORE_QTYC = detail.INV_QTY.ToString();
                        detail.CHK_QTY = "";
                        detail.CHK_REMARK = "";
                        detail.CHK_TIME = "";
                        detail.STATUS_INI = "1";
                        detail.CREATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_IP = DBWork.ProcIP;

                        // 新增detail
                        session.Result.afrs = repo.InsertChkdetail(detail, chk_wh_grade, "0");
                    }

                    if (repo.GetDetailCount(chk_no) == 0)
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "盤點單無盤點項目，請重新確認。";
                        return session.Result;
                    }

                    foreach (BC_WHCHKID user in users)
                    {
                        CHK_NOUID nouid = new CHK_NOUID()
                        {
                            CHK_NO = chk_no,
                            CHK_UID = user.WH_CHKUID,
                            CREATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_IP = DBWork.ProcIP
                        };

                        session.Result.afrs = repo.InsertChknouid(nouid);
                    }

                    session.Result.afrs = repo.UpdateMaster(chk_no);

                    repo.DeleteDetailTempAll(chk_no);

                    // 當下申請量資料寫入CHK_PH1S_WHINV
                    session.Result.afrs = repo.InsertChkPH1SWhinv(chk_no, DBWork.UserInfo.UserId, DBWork.ProcIP);

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
        public ApiResponse setWhkind0DistY(string chk_no, IEnumerable<CHK_DETAIL> details, IEnumerable<BC_WHCHKID> users, string chk_wh_grade)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    CHK_DETAIL firstItem = details.First<CHK_DETAIL>();
                    // 刪除原本detail
                    session.Result.afrs = repo.DeleteChkDetail(firstItem.CHK_NO);

                    // 每人應分配項數
                    int base_num = details.Count() / users.Count();
                    // 零頭數
                    int mod_num = details.Count() % users.Count();
                    int group_count = 0;
                    int index = 0;

                    foreach (CHK_DETAIL detail in details)
                    {
                        detail.STORE_QTYC = detail.INV_QTY.ToString();
                        detail.CHK_QTY = "";
                        detail.CHK_REMARK = "";
                        detail.CHK_TIME = "";
                        detail.STATUS_INI = "1";
                        detail.CREATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_IP = DBWork.ProcIP;

                        if (group_count < base_num)
                        {
                            group_count++;
                            detail.CHK_UID = users.Skip(index).Take(1).FirstOrDefault<BC_WHCHKID>().WH_CHKUID;
                        }
                        else if (group_count == base_num && mod_num > 0)
                        {
                            mod_num--;
                            group_count = 0;
                            detail.CHK_UID = users.Skip(index).Take(1).FirstOrDefault<BC_WHCHKID>().WH_CHKUID;
                            index++;
                        }
                        else if (group_count == base_num && mod_num == 0)
                        {
                            group_count = 1;
                            index++;
                            detail.CHK_UID = users.Skip(index).Take(1).FirstOrDefault<BC_WHCHKID>().WH_CHKUID;

                        }

                        // 新增detail
                        session.Result.afrs = repo.InsertChkdetail(detail, chk_wh_grade, "0");
                    }

                    if (repo.GetDetailCount(details.Take(1).FirstOrDefault().CHK_NO) == 0)
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "盤點單無盤點項目，請重新確認。";
                        return session.Result;
                    }

                    session.Result.afrs = repo.UpdateMaster(firstItem.CHK_NO);

                    repo.DeleteDetailTempAll(firstItem.CHK_NO);

                    // 當下申請量資料寫入CHK_PH1S_WHINV
                    session.Result.afrs = repo.InsertChkPH1SWhinv(chk_no, DBWork.UserInfo.UserId, DBWork.ProcIP);

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
        public ApiResponse setWhkindCE(IEnumerable<CHK_DETAIL> details)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    CHK_DETAIL firstItem = details.First<CHK_DETAIL>();
                    // 刪除原本detail
                    session.Result.afrs = repo.DeleteChkDetail(firstItem.CHK_NO);

                    foreach (CHK_DETAIL detail in details)
                    {
                        detail.CHK_QTY = "";
                        detail.CHK_REMARK = "";
                        detail.CHK_TIME = "";
                        detail.CHK_UID = DBWork.UserInfo.UserId;
                        detail.STATUS_INI = "1";
                        detail.CREATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_IP = DBWork.ProcIP;

                        // 新增detail
                        session.Result.afrs = repo.InsertChkdetailCE(detail);
                    }

                    if (repo.GetDetailCount(details.Take(1).FirstOrDefault().CHK_NO) == 0)
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "盤點單無盤點項目，請重新確認。";
                        return session.Result;
                    }

                    session.Result.afrs = repo.UpdateMaster(firstItem.CHK_NO);

                    repo.DeleteDetailTempAll(firstItem.CHK_NO);

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
        public ApiResponse setWard(string chk_no, IEnumerable<CHK_DETAIL> details, IEnumerable<BC_WHCHKID> users, string chk_wh_grade, string chk_wh_kind)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    // 刪除原本detail
                    session.Result.afrs = repo.DeleteChkDetail(chk_no);

                    foreach (CHK_DETAIL detail in details)
                    {
                        detail.STORE_QTYC = detail.INV_QTY.ToString();
                        detail.CHK_QTY = "";
                        detail.CHK_REMARK = "";
                        detail.CHK_TIME = "";
                        detail.STATUS_INI = "1";
                        detail.CREATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_IP = DBWork.ProcIP;

                        // 新增detail
                        session.Result.afrs = repo.InsertChkdetail(detail, chk_wh_grade, chk_wh_kind);
                    }

                    if (repo.GetDetailCount(chk_no) == 0)
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "盤點單無盤點項目，請重新確認。";
                        return session.Result;
                    }

                    foreach (BC_WHCHKID user in users)
                    {
                        CHK_NOUID nouid = new CHK_NOUID()
                        {
                            CHK_NO = chk_no,
                            CHK_UID = user.WH_CHKUID,
                            CREATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_IP = DBWork.ProcIP
                        };

                        session.Result.afrs = repo.InsertChknouid(nouid);
                    }

                    session.Result.afrs = repo.UpdateMaster(chk_no);

                    repo.DeleteDetailTempAll(chk_no);

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
        public ApiResponse AddAll(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var chk_no = form.Get("chk_no");
            var chk_wh_grade = form.Get("chk_wh_grade");
            var chk_wh_kind = form.Get("chk_wh_kind");
            var chk_type = form.Get("chk_type");
            var chk_class = form.Get("chk_class");
            var chk_period = form.Get("chk_period");

            return AddAll(wh_no, chk_no, chk_wh_grade, chk_wh_kind, chk_type, chk_class, chk_period);
        }

        public ApiResponse AddAll(string wh_no, string chk_no, string chk_wh_grade, string chk_wh_kind, string chk_type, string chk_class, string chk_period)
        {
            List<CHK_DETAIL> all = new List<CHK_DETAIL>();

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    if (wh_no == "ANE1")
                    {
                        all = repo.GetAllinDetailsANE(wh_no, chk_type, chk_no).ToList<CHK_DETAIL>();
                    }
                    else if (chk_wh_kind == "E" || chk_wh_kind == "C")
                    {
                        all = repo.GetAllinDetailsCE(wh_no, chk_type, chk_no, chk_class, chk_wh_kind).ToList<CHK_DETAIL>();
                    }
                    else
                    {
                        all = repo.GetAllinDetails(wh_no, chk_wh_grade, chk_wh_kind, chk_type, chk_no, chk_class, chk_period).ToList<CHK_DETAIL>();
                        // 衛星庫房衛材庫: 新增 本月新申領上月不存在品項
                        if (chk_wh_kind == "1" &&
                            (chk_wh_grade != "1" && chk_wh_grade != "C" && chk_wh_grade != "E"))
                        {
                            List<CHK_DETAIL> tempList = repo.GetAllinDetailsNoPre(wh_no, chk_wh_grade, chk_wh_kind, chk_type, chk_no, chk_class, chk_period).ToList();
                            all.AddRange(tempList);
                        }
                    }

                    // 刪除原本detail
                    session.Result.afrs = repo.DeleteChkDetail(chk_no);

                    all = all.GroupBy(x => new { x.WH_NO, x.MMCODE }).Select(g => g.First()).ToList();

                    foreach (CHK_DETAIL item in all)
                    {
                        item.CHK_NO = chk_no;
                        item.STATUS_INI = "0";
                        item.CREATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;

                        if (chk_wh_kind == "E" || chk_wh_kind == "C")
                        {
                            item.WH_KIND = chk_wh_kind;
                            session.Result.afrs = repo.InsertChkdetailCE(item);
                        }
                        else
                        {
                            // 新增detail
                            session.Result.afrs = repo.InsertChkdetail(item, chk_wh_grade, chk_wh_kind);
                        }
                    }

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #endregion

        #region 藥局

        [HttpPost]
        public ApiResponse MedChkItems(FormDataCollection form)
        {
            string chk_no = form.Get("chk_no");
            string wh_no = form.Get("wh_no");
            string chk_ym = form.Get("chk_ym");
            string chk_period = form.Get("chk_period");
            string chk_type = form.Get("chk_type");
            string seq1 = form.Get("seq1");
            string seq2 = form.Get("seq2");
            string mmcode1 = form.Get("mmcode1");
            string mmcode2 = form.Get("mmcode2");
            bool chkpredateNullonly = form.Get("chkpredateNullonly") == "Y";


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                IEnumerable<CHK_DETAIL> items = null;
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    if (chk_period == "P")
                    {
                        items = repo.GetChkGrade2Ps(wh_no, chk_ym);
                    }
                    else
                    {
                        items = repo.GetMedItems(wh_no, chk_type);
                    }

                    // 不存在於CHK_G2_WHINV 新增
                    if (repo.ChkG2WhinvExist(chk_no) == false)
                    {
                        foreach (CHK_DETAIL item in items)
                        {
                            CHK_G2_WHINV whinv = new CHK_G2_WHINV()
                            {
                                CHK_NO = chk_no,
                                WH_NO = wh_no,
                                MMCODE = item.MMCODE,
                                CREATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_IP = DBWork.ProcIP,
                                SEQ = repo.GetMedSeq(chk_no)
                            };
                            repo.InsertChkG2Whinv(whinv);
                        }
                    }

                    session.Result.etts = repo.GetChkG2Whinvs(chk_no, seq1, seq2, mmcode1, mmcode2, chkpredateNullonly);

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
        public ApiResponse SetPreDate(FormDataCollection form)
        {
            var itemString = form.Get("item_string");

            IEnumerable<CHK_G2_WHINV> whinvs = JsonConvert.DeserializeObject<IEnumerable<CHK_G2_WHINV>>(itemString);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    var repo = new CE0002Repository(DBWork);

                    foreach (CHK_G2_WHINV whinv in whinvs)
                    {
                        whinv.UPDATE_USER = DBWork.UserInfo.UserId;
                        whinv.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.UpdateChkG2Whinv(whinv);
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
        public ApiResponse MedCreateSheet(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            var chk_ym = form.Get("chk_ym");
            var usersString = form.Get("users");

            IEnumerable<BC_WHCHKID> users = JsonConvert.DeserializeObject<IEnumerable<BC_WHCHKID>>(usersString);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    if (repo.CheckMedChkPreDatesNull(chk_no)) {
                        session.Result.success = false;
                        session.Result.msg = "尚有未設定預計盤點日之品項，無法開立盤點單";
                        return session.Result;
                    }

                    foreach (BC_WHCHKID user in users)
                    {
                        CHK_GRADE2_UPDN item = new CHK_GRADE2_UPDN()
                        {
                            CHK_NO = chk_no,
                            CHK_YM = chk_ym,
                            CHK_UID = user.WH_CHKUID,
                            UP_DATE = string.Empty,
                            DN_DATE = string.Empty,
                            UPDN_STATUS = "1",
                            CREATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_IP = DBWork.ProcIP
                        };
                        session.Result.afrs = repo.InsertGrade2UpDn(item);
                    }

                    IEnumerable<CHK_G2_WHINV> whinvs = repo.GetAllChkG2Whinvs(chk_no);
                    IEnumerable<CHK_G2_DETAIL> details = GetG2Details(whinvs, users);

                    foreach (CHK_G2_DETAIL detail in details)
                    {
                        detail.STATUS_INI = "1";
                        detail.CREATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_USER = DBWork.UserInfo.UserId;
                        detail.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.InsertG2Detail(detail);
                    }


                    session.Result.afrs = repo.MedUpdateMaster(chk_no, users.Count(), DBWork.UserInfo.UserId, DBWork.ProcIP);

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
        public IEnumerable<CHK_G2_DETAIL> GetG2Details(IEnumerable<CHK_G2_WHINV> whinvs, IEnumerable<BC_WHCHKID> users)
        {
            List<CHK_G2_DETAIL> details = new List<CHK_G2_DETAIL>();
            foreach (CHK_G2_WHINV whinv in whinvs)
            {
                foreach (BC_WHCHKID user in users)
                {
                    CHK_G2_DETAIL detail = new CHK_G2_DETAIL()
                    {
                        CHK_NO = whinv.CHK_NO,
                        WH_NO = whinv.WH_NO,
                        MMCODE = whinv.MMCODE,
                        CHK_UID = user.WH_CHKUID,
                        STATUS_INI = "1"
                    };
                    details.Add(detail);
                }
            }
            return details;
        }

        #endregion

        #region combo
        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    if (DBWork.UserInfo.Inid == "560000")   // 中央庫房 藥庫 能設 通信
                    {
                        IEnumerable<MI_WHID> whids560000 = repo.GetWhidInfo(DBWork.UserInfo.UserId);

                        session.Result.etts = GetWhnoCombo560000Items(whids560000, DBWork);
                        return session.Result;
                    }

                    IEnumerable<CE0002Repository.WhnoComboItem> whids = repo.GetWhnoCombo(DBWork.UserInfo.UserId);
                    List<CE0002Repository.WhnoComboItem> tempList = whids.Where(x => x.WH_KIND == "0" && x.WH_GRADE == "2").ToList();
                    if (tempList.Any())     // 藥局
                    {
                        session.Result.etts = whids;
                    }
                    else
                    {      // 科室病房
                        tempList = whids.ToList();
                        List<CE0002Repository.WhnoComboItem> newList = new List<CE0002Repository.WhnoComboItem>();
                        foreach (CE0002Repository.WhnoComboItem item in tempList)
                        {
                            newList.Add(item);
                            newList.Add(repo.GetWhnoComboItem(item.INID, "1", string.Empty));
                        }

                        newList.AddRange(repo.GetWhnosBySupplyInid(DBWork.UserInfo.Inid, "1"));

                        IEnumerable<CE0002Repository.WhnoComboItem> whidsWard
                                = from a in newList
                                  orderby a.WH_NO ascending
                                  group a by a.WH_NO into g
                                  select g.First();

                        session.Result.etts = whidsWard;
                    }

                    //session.Result.etts = repo.GetWhnoCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public IEnumerable<CE0002Repository.WhnoComboItem> GetWhnoCombo560000Items(IEnumerable<MI_WHID> whids, UnitOfWork dbWork)
        {

            var repo = new CE0002Repository(dbWork);

            List<CE0002Repository.WhnoComboItem> list = new List<CE0002Repository.WhnoComboItem>();
            foreach (MI_WHID whid in whids)
            {
                // 排除戰備庫 wh_grade = '5'
                if (whid.WH_GRADE == "5") {
                    continue;
                }
                CE0002Repository.WhnoComboItem item = new CE0002Repository.WhnoComboItem();
                if (whid.TASK_ID == "1")    // 藥庫
                {
                    item = repo.GetWhnoComboItem(whid.INID, "0", whid.WH_GRADE);
                    if (item != null) {
                        list.Add(item);
                    }
                }
                else if (whid.TASK_ID == "2" || whid.TASK_ID == "3")    // 中央庫房
                {
                    item = repo.GetWhnoComboItem(whid.INID, "1", whid.WH_GRADE);
                    if (item != null) {
                        list.Add(item);
                    }
                }
                else if (whid.TASK_ID == "4")   // 能設
                {
                    item = repo.GetWhnoComboItem(whid.INID, "E", whid.WH_GRADE);
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
                else if (whid.TASK_ID == "5")
                { // 通信
                    item = repo.GetWhnoComboItem(whid.INID, "C", whid.WH_GRADE);
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }

            }

            IEnumerable<CE0002Repository.WhnoComboItem> wh_nos = from a in list
                                                                 group a by a.WH_NO into g
                                                                 select g.First();

            return wh_nos;
        }


        #endregion

        #region 科室病房衛材月盤手動新增項目 2019-12-05新增
        [HttpPost]
        public ApiResponse GetAddItemList(FormDataCollection form)
        {
            var chk_no = form.Get("chk_no");
            var wh_no = form.Get("wh_no");
            var chk_wh_grade = form.Get("chk_wh_grade");
            var chk_wh_kind = form.Get("chk_wh_kind");
            var chk_type = form.Get("chk_type");
            var chk_class = form.Get("chk_class");
            var chk_period = form.Get("chk_period");
            var mmcode = form.Get("mmcode");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    session.Result.etts = repo.GetAddItemList(chk_no, wh_no, chk_wh_grade, chk_wh_kind, chk_type, chk_class, chk_period, mmcode);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse AddItems(FormDataCollection form)
        {
            var list = form.Get("list");
            IEnumerable<CHK_DETAIL> items = JsonConvert.DeserializeObject<IEnumerable<CHK_DETAIL>>(list);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    string chk_no = items.Take(1).FirstOrDefault<CHK_DETAIL>().CHK_NO;

                    //if (repo.CheckDetailEntry(chk_no) == false)
                    //{
                    //    session.Result.afrs = 0;
                    //    session.Result.success = false;
                    //    session.Result.msg = string.Format("{0} 已有數量輸入，無法新增", chk_no);
                    //    return session.Result;
                    //}

                    foreach (CHK_DETAIL item in items)
                    {
                        session.Result.afrs = repo.AddItem(item.CHK_NO, item.WH_NO, item.MMCODE, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    }

                    session.Result.afrs = repo.UpdateMaster(chk_no);

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
        #endregion

        #region 修改可盤點人員 2019-12-20新增
        [HttpPost]
        public ApiResponse ChangeUid(FormDataCollection form)
        {
            string usersString = form.Get("users");
            string chk_no = form.Get("chk_no");
            string chk_ym = form.Get("chk_ym");
            bool is_ward = (form.Get("is_ward") == "Y");


            IEnumerable<BC_WHCHKID> users = JsonConvert.DeserializeObject<IEnumerable<BC_WHCHKID>>(usersString);

            IEnumerable<BC_WHCHKID> new_users = users.Where(x => x.HAS_ENTRY == "N").ToList<BC_WHCHKID>();

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    string excludes = GetExcludeUids(users);
                    // 移除未輸入之盤點人員(科室病房)
                    session.Result.afrs = repo.DeleteNouid(chk_no, excludes);
                    // 移除未輸入之盤點人員(藥局)
                    session.Result.afrs = repo.DeleteG2Updn(chk_no, excludes);
                    session.Result.afrs = repo.DeleteG2Detail(chk_no, excludes);

                    if (is_ward == false) //藥局
                    {
                        foreach (BC_WHCHKID nUsers in new_users)
                        {
                            CHK_GRADE2_UPDN item = new CHK_GRADE2_UPDN()
                            {
                                CHK_NO = chk_no,
                                CHK_YM = chk_ym,
                                CHK_UID = nUsers.WH_CHKUID,
                                UP_DATE = string.Empty,
                                DN_DATE = string.Empty,
                                UPDN_STATUS = "1",
                                CREATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_IP = DBWork.ProcIP
                            };
                            session.Result.afrs = repo.InsertGrade2UpDn(item);
                        }

                        IEnumerable<CHK_G2_WHINV> whinvs = repo.GetAllChkG2Whinvs(chk_no);
                        IEnumerable<CHK_G2_DETAIL> details = GetG2Details(whinvs, new_users);

                        foreach (CHK_G2_DETAIL detail in details)
                        {
                            detail.STATUS_INI = "1";
                            detail.CREATE_USER = DBWork.UserInfo.UserId;
                            detail.UPDATE_USER = DBWork.UserInfo.UserId;
                            detail.UPDATE_IP = DBWork.ProcIP;

                            session.Result.afrs = repo.InsertG2Detail(detail);
                        }

                        session.Result.afrs = repo.MedUpdateChkTotal(chk_no);
                    }
                    else
                    {  //科室病房
                        foreach (BC_WHCHKID user in new_users)
                        {
                            CHK_NOUID nouid = new CHK_NOUID()
                            {
                                CHK_NO = chk_no,
                                CHK_UID = user.WH_CHKUID,
                                CREATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_IP = DBWork.ProcIP
                            };

                            session.Result.afrs = repo.InsertChknouid(nouid);
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

        public string GetExcludeUids(IEnumerable<BC_WHCHKID> users)
        {
            string result = string.Empty;
            foreach (BC_WHCHKID user in users)
            {
                if (user.HAS_ENTRY == "Y")
                {
                    if (result.Trim() != string.Empty)
                    {
                        result += ",";
                    }
                    result += string.Format("'{0}'", user.WH_CHKUID);
                }
            }
            return result;
        }

        [HttpPost]
        public ApiResponse CurrentUids(FormDataCollection form)
        {
            string chk_no = form.Get("chk_no");
            bool is_ward = (form.Get("is_ward") == "Y");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    CE0002Repository repo = new CE0002Repository(DBWork);
                    session.Result.etts = repo.GetUidList(chk_no, is_ward);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        #region 科室病房一鍵產生盤點單 2020-01-02新增
        public ApiResponse CreateSheetOneClick(FormDataCollection form)
        {
            var usersString = form.Get("users");
            var masterString = form.Get("master");

            CHK_MAST master = JsonConvert.DeserializeObject<CHK_MAST>(masterString);
            //CHK_MAST master = JsonConvert.DeserializeObject<IEnumerable<CHK_MAST>>(masterString).FirstOrDefault();
            IEnumerable<BC_WHCHKID> users = JsonConvert.DeserializeObject<IEnumerable<BC_WHCHKID>>(usersString);

            List<CHK_DETAIL> all = new List<CHK_DETAIL>();

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    string canCreateMsg = CheckCanCreate(master);
                    if (canCreateMsg != string.Empty)
                    {
                        session.Result.success = false;
                        session.Result.msg = canCreateMsg;
                        return session.Result;
                    }

                    string period = master.CHK_PERIOD == "D" ? "D" : "A";
                    string chk_ym = period != "D" ? master.CHK_YM : master.CHK_YMD.Substring(0, 5);
                    string currentSeq = GetCurrentSeq(master.CHK_WH_NO, chk_ym);
                    master.CHK_NO = string.Format("{0}{1}{2}{3}{4}", master.CHK_WH_NO, master.CHK_YM, period, master.CHK_TYPE, currentSeq);
                    if (master.CHK_PERIOD == "D")
                    {
                        master.CHK_YM = master.CHK_YMD;
                    }
                    master.CHK_KEEPER = DBWork.UserInfo.UserId;
                    master.CREATE_USER = DBWork.UserInfo.UserId;
                    master.UPDATE_USER = DBWork.UserInfo.UserId;
                    master.UPDATE_IP = DBWork.ProcIP;

                    // 月盤 / 季盤 / 抽盤(目前僅藥局使用，作為月盤)
                    if (master.CHK_PERIOD == "M" || master.CHK_PERIOD == "S" || master.CHK_PERIOD == "P")
                    {
                        if (repo.CheckExists(master.CHK_WH_NO, master.CHK_YM, master.CHK_PERIOD, master.CHK_TYPE, master.CHK_CLASS)) // 新增前檢查主鍵是否已存在
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "項目重複，請重新輸入。";
                            return session.Result;
                        }
                    }
                    else
                    {

                        string chk_status = repo.CheckPreStatus(master.CHK_WH_NO, master.CHK_YM, master.CHK_PERIOD, master.CHK_TYPE, master.CHK_CLASS);
                        
                        if (chk_status != "3")
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = string.Format("相同類別盤點單尚未完成，請先完成盤點後再另開盤點單");
                            return session.Result;
                        }
                    }

                    session.Result.afrs = repo.InsertChkMast(master);

                    if (master.CHK_WH_NO == "ANE1")
                    {
                        all = repo.GetAllinDetailsANE(master.CHK_WH_NO, master.CHK_TYPE, master.CHK_NO).ToList<CHK_DETAIL>();
                    }
                    else if (master.CHK_WH_KIND == "E" || master.CHK_WH_KIND == "C")
                    {
                        all = repo.GetAllinDetailsCE(master.CHK_WH_NO, master.CHK_TYPE, master.CHK_NO, master.CHK_CLASS, master.CHK_WH_KIND).ToList<CHK_DETAIL>();
                    }
                    else
                    {
                        all = repo.GetAllinDetails(master.CHK_WH_NO, master.CHK_WH_GRADE, master.CHK_WH_KIND, master.CHK_TYPE, master.CHK_NO, master.CHK_CLASS, master.CHK_PERIOD).ToList<CHK_DETAIL>();
                        // 衛星庫房衛材庫: 新增 本月新申領上月不存在品項
                        if (master.CHK_WH_KIND == "1" && 
                            (master.CHK_WH_GRADE != "1" && master.CHK_WH_GRADE != "C" && master.CHK_WH_GRADE != "E")) {
                            List<CHK_DETAIL> tempList = repo.GetAllinDetailsNoPre(master.CHK_WH_NO, master.CHK_WH_GRADE, master.CHK_WH_KIND, master.CHK_TYPE, master.CHK_NO, master.CHK_CLASS, master.CHK_PERIOD).ToList();
                            all.AddRange(tempList);
                        }
                    }
                    all = all.GroupBy(x => new { x.WH_NO, x.MMCODE }).Select(g => g.First()).ToList();

                    string status_ini = "1";
                    if (master.CHK_WH_GRADE == "1" && master.CHK_WH_KIND != "E" && master.CHK_WH_KIND != "C")
                    {
                        status_ini = "0";
                    }

                    foreach (CHK_DETAIL item in all)
                    {
                        item.CHK_NO = master.CHK_NO;
                        item.STATUS_INI = status_ini;
                        item.CREATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;

                        if (master.CHK_WH_KIND == "E" || master.CHK_WH_KIND == "C")
                        {
                            item.CHK_UID = DBWork.UserInfo.UserId;
                            item.WH_KIND = master.CHK_WH_KIND;
                            session.Result.afrs = repo.InsertChkdetailCE(item);
                        }
                        else
                        {
                            session.Result.afrs = repo.InsertChkdetail(item, master.CHK_WH_GRADE, master.CHK_WH_KIND);
                        }
                    }
                    if (master.CHK_WH_GRADE != "1" &&
                        ((master.CHK_WH_GRADE == "2" && master.CHK_WH_GRADE == "0") == false)
                        )
                    {
                        foreach (BC_WHCHKID user in users)
                        {
                            CHK_NOUID nouid = new CHK_NOUID()
                            {
                                CHK_NO = master.CHK_NO,
                                CHK_UID = user.WH_CHKUID,
                                CREATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_USER = DBWork.UserInfo.UserId,
                                UPDATE_IP = DBWork.ProcIP
                            };

                            session.Result.afrs = repo.InsertChknouid(nouid);
                        }
                    }

                    if (status_ini == "1")
                    {
                        session.Result.afrs = repo.UpdateMaster(master.CHK_NO);
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
        #endregion

        #region 藥庫-認可庫存為0的品項 2020-02-27新增
        public ApiResponse Invqty0Confirm(FormDataCollection form)
        {
            string chk_nos = form.Get("chk_nos");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    session.Result.afrs = repo.Invqty0Confirm(chk_nos, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    session.Result.afrs = repo.UpdateChkmastNum(chk_nos, DBWork.UserInfo.UserId, DBWork.ProcIP);
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


        #endregion

        #region 中央庫房-有已點收(flowid=5)的單子不可開盤點單
        [HttpPost]
        public ApiResponse CheckMeDocm5(FormDataCollection form) {
            string wh_no = form.Get("wh_no");
            string wh_kind = form.Get("wh_kind");
            string chk_type = form.Get("chk_type");
            string chk_class = form.Get("chk_class");

            IEnumerable<OnWayQtyCheck> onwayQtyCheckList = new List<OnWayQtyCheck>()
            {
                new OnWayQtyCheck(){ WH_KIND = "1", CHK_TYPE="1", DOCTYPES = "'MR1','MR2'", FLOWIDS = "'3','4','5'", APPLY_KIND = string.Empty}, //庫備
                new OnWayQtyCheck(){ WH_KIND = "1", CHK_TYPE="0", DOCTYPES = "'MR3','MR4'", FLOWIDS = "'3','4','5'", APPLY_KIND = string.Empty}, //非庫備
                new OnWayQtyCheck(){ WH_KIND = "1", CHK_TYPE="3", DOCTYPES = "'MR1','MR2','MR3','MR4'", FLOWIDS = "'3','4','5'", APPLY_KIND = "3"}, //小額採購
                new OnWayQtyCheck(){ WH_KIND = "0", DOCTYPES = "'MR','MS'", FLOWIDS = "'0103','0104','0603','0604'", APPLY_KIND = string.Empty}
            };

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);

                    OnWayQtyCheck qtyCheck = null;
                    foreach (OnWayQtyCheck check in onwayQtyCheckList) {
                        if (check.WH_KIND == wh_kind && check.CHK_TYPE == chk_type) {
                            qtyCheck = check;
                        }
                    }

                    string matClasses = GetChkTypeString(chk_class);

                    int counts = repo.CheckMeDocm5(qtyCheck.DOCTYPES, qtyCheck.FLOWIDS, matClasses, qtyCheck.APPLY_KIND);

                    session.Result.afrs = counts;
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }
        public string GetChkTypeString(string chk_type) {

            List<string> list = chkTypeList[chk_type];

            string result = string.Empty;
            foreach (string item in list) {
                if (result != string.Empty) {
                    result += ",";
                }
                result += ("'" + item + "'");
            }
            return result;
        }
        #endregion

        #region 以月結方式處理在途量 2020-03-24新增
        [HttpPost]
        public ApiResponse CopeOnWayQty(FormDataCollection form) {
            string wh_no = form.Get("wh_no");
            string chk_type = form.Get("chk_type");
            string chk_class = form.Get("chk_class");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    string set_ym = repo.GetSetYM();

                    List<string> matClasses = chkTypeList[chk_class];

                    foreach (string matClass in matClasses) {
                        SP_MODEL sp = repo.PostUack(set_ym, chk_type, matClass);

                        if (sp.O_RETID == "N")
                        {
                            session.Result.success = false;
                            session.Result.msg = matClass + " " + sp.O_ERRMSG;
                            return session.Result;
                        }
                    }

                    session.Result.success = true;

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
        #endregion

        #region 2020-05-22 配合藥局需求 CHK_G2_WHINV新增項次欄位 查詢新增項次院內碼欄位
        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
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
                    CE0002Repository repo = new CE0002Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion

        #region  2020-12-09 新增: 科室病房檢查是否有在途量，有的話不可開單
        [HttpPost]
        public ApiResponse CheckOnwayQty(FormDataCollection form)
        {
            string wh_no = form.Get("wh_no");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    int counts = repo.CheckOnwayQty(wh_no);

                    session.Result.afrs = counts;
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }

        }
        #endregion

        #region 2021-10-20 檢查開單後是否有點收品項或有在途量但不再盤點單內
        [HttpPost]
        public ApiResponse CheckNeedDetailAdd(FormDataCollection form) {
            string wh_no = form.Get("wh_no");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    IEnumerable<CHK_ADD_ITEM> list = repo.GetNeedDetailAddList(wh_no);
                    if (list.Any())
                    {
                        session.Result.success = true;
                    }
                    else {
                        session.Result.success = false;
                    }

                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
                
            }
        }
        [HttpPost]
        public ApiResponse AddDetailNotExists(FormDataCollection form) {
            string wh_no = form.Get("wh_no");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try {
                    var repo = new CE0002Repository(DBWork);
                    // 找到代加入清單
                    IEnumerable<CHK_ADD_ITEM> list = repo.GetNeedDetailAddList(wh_no);
                    // 依類別整理
                    List<CHK_ADD_ITEM> chk_no_list = GetChkNoList(list);
                    foreach (CHK_ADD_ITEM item in chk_no_list) {
                        // 設定MMCODE_COUNT、MMCODE_STRING、CHK_TYPE
                        if (item.CHK_TYPE == "3")
                        {
                            item.CHK_TYPE_NAME = "小採";
                        }
                        if (item.CHK_TYPE == "0")
                        {
                            item.CHK_TYPE_NAME = "非庫備";
                        }
                        if (item.CHK_TYPE == "1")
                        {
                            item.CHK_TYPE_NAME = "庫備";
                        }
                        item.MMCODE_COUNT = item.MMCODE_LIST.Count().ToString();
                        string mmcodes = string.Empty;
                        foreach (CHK_ADD_ITEM mmcodeInfo in item.MMCODE_LIST)
                        {
                            if (mmcodes != string.Empty)
                            {
                                mmcodes += "、";
                            }
                            mmcodes += mmcodeInfo.MMCODE;
                        }
                        item.MMCODE_STRING = mmcodes;

                        // 取得可新增之盤點單號
                        CHK_ADD_ITEM temp = repo.GetChkNoFromAddItem(item.WH_NO, item.CHK_TYPE);
                        if (temp == null) {
                            item.RESULT = "無可新增品項之盤點單，請開單後手動新增";
                            continue;
                        }
                        item.CHK_NO = temp.CHK_NO;
                        item.CHK_LEVEL = temp.CHK_LEVEL;
                        if (temp.CHK_STATUS == "2")
                        {
                            item.CHK_STATUS_NAME = "調整";
                            item.RESULT = "盤點單已輸入完成，請先完成本階段盤點後至下一盤開單並新增";
                            continue;
                        }
                        if (temp.CHK_STATUS == "3") {
                            item.CHK_STATUS_NAME = "鎖單";
                            item.RESULT = "盤點單已完成，請至下一盤開單並新增";
                            continue;
                        }
                        if (temp.CHK_STATUS == "P")
                        {
                            item.CHK_STATUS_NAME = "過帳";
                            item.RESULT = "盤點單已過帳，無法新增";
                            continue;
                        }
                        if (temp.CHK_STATUS == "0") {
                            item.CHK_STATUS_NAME = "開立";
                        }
                        if (temp.CHK_STATUS == "1")
                        {
                            item.CHK_STATUS_NAME = "盤中";
                        }

                        // 新增
                        foreach (CHK_ADD_ITEM mmcodeInfo in item.MMCODE_LIST) {
                            session.Result.afrs = repo.AddDetail(temp.CHK_NO, mmcodeInfo.MMCODE, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }

                        // update CHK_MAST
                        session.Result.afrs = repo.UpdateChkmastTotal(temp.CHK_NO, DBWork.UserInfo.UserId, DBWork.ProcIP);

                        item.RESULT = "院內碼已新增";
                    }
                    DBWork.Commit();
                    session.Result.etts = chk_no_list;
                    return session.Result ;
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
            }
        }
        public List<CHK_ADD_ITEM> GetChkNoList(IEnumerable<CHK_ADD_ITEM> addItemList) {
            var o = from a in addItemList
                    group a by new
                    {
                        CHK_TYPE = a.CHK_TYPE,
                        WH_NO = a.WH_NO
                    } into g
                    select new CHK_ADD_ITEM
                    {
                        CHK_TYPE = g.Key.CHK_TYPE,
                        WH_NO = g.Key.WH_NO,
                        MMCODE_LIST = g.ToList()
                    };
            return o.ToList();
        }
        #endregion

        #region 
        [HttpGet]
        public ApiResponse GetCurrentSetYm() {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0002Repository(DBWork);
                    session.Result.msg = repo.GetSetYM();

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