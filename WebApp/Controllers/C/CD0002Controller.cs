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
using static WebApp.Repository.C.CD0002Repository;

namespace WebApp.Controllers.C
{


    public class CD0002Controller : SiteBase.BaseApiController
    {
        public ApiResponse MasterAll(FormDataCollection form) {
            var wh_no = form.Get("p0");
            var pick_date = form.Get("p1");
            var apply_kind = form.Get("p2");
            var isDistributed = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            bool resetComplexity = (form.Get("resetComplexity") == "Y");

            if (apply_kind == "3") {
                return MasterAllKind3(wh_no, pick_date, isDistributed, resetComplexity);
            }

            return MasterAllKind12(wh_no, pick_date, apply_kind, isDistributed, resetComplexity);
        }
        public ApiResponse MasterAllKind12(string wh_no, string pick_date, string apply_kind, string isDistributed, bool resetComplexity) {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    string user = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    if (isDistributed == "=" && resetComplexity)
                    {
                       
                        session.Result.afrs = repo.DeleteBcWhpick(wh_no);
                        session.Result.afrs = repo.DeleteBcWhpickdoc(wh_no);

                        session.Result.afrs = repo.InsertBcwhpick(wh_no, pick_date, string.Empty, apply_kind, user, ip);

                        //依庫房複雜度預設
                        IEnumerable<WhnoDocmCounts> whnoDocms = repo.GetWhnoDocmCounts(wh_no);
                        foreach (WhnoDocmCounts whnoDocm in whnoDocms) {
                            session.Result.afrs = repo.InsertBcwhpickdocByTowh(wh_no, pick_date, whnoDocm.AVG_COMPLEXITY.ToString(),whnoDocm.TOWH, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }

                        session.Result.afrs = repo.InsertBcwhpickdoc(wh_no, pick_date, string.Empty, user, ip);
                    }

                    DBWork.Commit();

                    session.Result.etts = repo.GetMasterAllByTowh(wh_no, pick_date, apply_kind, isDistributed, true);

                    
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse MasterAllKind3(string wh_no, string pick_date, string isDistributed, bool resetComplexity) {
            using (WorkSession session = new WorkSession(this)) {

                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    string user = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    if (isDistributed == "=" && resetComplexity)
                    {

                        session.Result.afrs = repo.DeleteBcWhpick(wh_no);
                        session.Result.afrs = repo.DeleteBcWhpickdoc(wh_no);

                        session.Result.afrs = repo.InsertBcwhpick(wh_no, pick_date, string.Empty, "3", user, ip);
                        session.Result.afrs = repo.InsertBcwhpickdocKind3(wh_no, pick_date, string.Empty, user, ip);
                    }

                    DBWork.Commit();

                    session.Result.etts = repo.GetMasterAllByTowh(wh_no, pick_date, "3", isDistributed, true);


                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse DetailAll(FormDataCollection form) {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(p0, p1, p2);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse DetailAllByDocnos(FormDataCollection form) {
            string docnos = form.Get("docnos");
            string wh_no = form.Get("wh_no");
            string pick_date = form.Get("pick_date");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.etts = repo.GetDetailAllByDocnos(wh_no, pick_date, docnos);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #region 查詢新申請單資料

        [HttpPost]
        public ApiResponse GetMeDocms(FormDataCollection form) {
            var wh_no = form.Get("p0");
            string runDelete = form.Get("runDelete");
            // var p1 = form.Get("p1");
            // var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);

                    if (runDelete == "true") {
                        session.Result.afrs = repo.DeleteBcWhpick(wh_no);
                        session.Result.afrs = repo.DeleteBcWhpickdoc(wh_no);
                    }

                    session.Result.etts = repo.GetMeDocms(wh_no, page, limit, sorters);

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
        public ApiResponse GetMedocds(FormDataCollection form)
        {
            var docno = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.etts = repo.GetMeDocds(docno);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

        }

        [HttpPost]
        // 依申請單號轉入待揀貨清單，目前未使用此功能，暫時保留
        public ApiResponse TransferToBcwhpick(FormDataCollection form) {
            var docno = form.Get("DOCNO");
            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string user = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    var repo = new CD0002Repository(DBWork);


                    session.Result.afrs = repo.InsertBcwhpick(wh_no, pick_date, docno, string.Empty,user, ip);

                    session.Result.afrs = repo.InsertBcwhpickdoc(wh_no, pick_date, docno, user, ip);

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

        #region 設定值日生
        public ApiResponse UpdateDuty(FormDataCollection form) {
            var userId = form.Get("WH_USERID");
            var wh_no = form.Get("WH_NO");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);

                    string update_user = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    session.Result.afrs = repo.DeleteDuty(wh_no, update_user, ip);

                    session.Result.afrs = repo.UpdateDuty(wh_no, userId, update_user, ip);

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

        #region 複雜度設定

        [HttpPost]
        public ApiResponse GetComplexMaster(FormDataCollection form) {
            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.etts = repo.GetComplexMaster(wh_no, pick_date);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetComplexDetail(FormDataCollection form)
        {
            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var docno = form.Get("DOCNO");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.etts = repo.GetComplexDetail(wh_no, pick_date, docno);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse UpdateComplexity(FormDataCollection form) {
            BC_WHPICKDOC pick = new BC_WHPICKDOC();
            pick.WH_NO = form.Get("WH_NO");
            pick.PICK_DATE = form.Get("PICK_DATE");
            pick.DOCNO = form.Get("DOCNO");
            pick.COMPLEXITY = float.Parse(form.Get("COMPLEXITY"));

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.afrs = repo.UpdateComplexity(pick);

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

        #region by inid 2020-04-06
        public ApiResponse UpdateComplexityByInid(FormDataCollection form)
        {
            string wh_no = form.Get("wh_no");
            string pick_date = form.Get("pick_date");
            string complexity = form.Get("complexity");
            string docno_counts = form.Get("docno_counts");
            string docnos = form.Get("docnos");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.afrs = repo.UpdateComplexityByInid(wh_no, pick_date, complexity, docno_counts, docnos);

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

        #endregion

        #region 分配揀貨員
        public class DistriItem {
            public List<BC_WHPICKDOC>  Docs { get; set; }
            public float ComplexSum { get; set; }
            public int AppitemSum { get; set; }

            public List<string> Inids { get; set; }
            public int DocnoSum { get; set; }
            public List<string> Docnos { get; set; }

            public DistriItem() {
                Docs = new List<BC_WHPICKDOC>();
                ComplexSum = 0;
                AppitemSum = 0;
                Inids = new List<string>();
                DocnoSum = 0;
                Docnos = new List<string>();
            }
        }
        [HttpPost]
        public ApiResponse GetMasterCount(FormDataCollection form) {
            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var apply_kind = form.Get("APPLY_KIND");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.afrs = repo.GetMasterCount(wh_no, pick_date, apply_kind);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }

        }

        [HttpPost]
        // 查詢是否已排有臨時申請單揀貨批次資料(如max_logno>1000則表示已有安排，否則沒有)
        public ApiResponse GetTempPicklotNo(FormDataCollection form) {
            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.afrs = repo.GetTempMaxLotNo(wh_no, pick_date);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse SetTempSheets(FormDataCollection form) {
            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var dutyUser = form.Get("DUTY_USER");
            var max_lot_no = int.Parse(form.Get("MAX_LOT_NO"));
            var insertLast = form.Get("INSERT_LAST");

            bool isTemp = true;
            bool isTempExists = max_lot_no > 1000;

            int insertLotNo = insertLast == "true" ? max_lot_no : (isTempExists ? (max_lot_no +1) : 1001) ;

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string user = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    var repo = new CD0002Repository(DBWork);

                    if (insertLast != "true") {
                        session.Result.afrs = repo.InsertBcwhpicklot(wh_no, pick_date, insertLotNo, dutyUser, user, ip);
                    }
                    
                    session.Result.afrs = repo.UpdateBcwhpickdocTemp(wh_no, pick_date, insertLotNo, user, ip, isTemp, isTempExists);
                    session.Result.afrs = repo.UpdateBcwhpickTemp(wh_no, pick_date, insertLotNo, dutyUser, user, ip);

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

        // 分配常態申請單
        public ApiResponse DistributeRegular(FormDataCollection form) {

            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var userCount = form.Get("USER_COUNT");

            DistriItem[] array = new DistriItem[int.Parse(userCount)];
            for (int i = 0; i < array.Length; i++) {
                array[i] = new DistriItem();
            }

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                string userId = DBWork.UserInfo.UserId;
                string ip = DBWork.ProcIP;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    // 記錄現在時間
                    string calc_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                    // 刪除批次分配站存歷史紀錄
                    int deleteLotsum = repo.DeleteTemplotsum(wh_no);
                    // 刪除批次分配申請單站存歷史資料
                    int deleteLotdoc = repo.DeleteTemplotdoc(wh_no);
                    
                    // 查詢最大分配揀貨批次號碼
                    int max_lot_no = repo.GetMaxLotNo(wh_no, pick_date);

                    // 取得要分配的doc
                    IEnumerable<BC_WHPICKDOC> docs = repo.GetBcWhpickdoc(wh_no, pick_date);

                    foreach (BC_WHPICKDOC doc in docs)
                    {
                        doc.Picks = repo.GetDetailAll(doc.WH_NO, doc.PICK_DATE, doc.DOCNO);
                    }

                    array = GetDistriResult(docs, array);

                    for (int i = 0; i < array.Length; i++) {
                        max_lot_no++;
                        int sumResult = repo.InsertTemplotsum(wh_no, calc_time, max_lot_no, array[i].ComplexSum, array[i].Docs.Count(), array[i].AppitemSum, userId, ip);

                        int docResult = 0;
                        foreach (BC_WHPICKDOC doc in array[i].Docs) {
                            docResult += repo.InsertTemplotdoc(wh_no, calc_time, max_lot_no, doc.DOCNO);
                        }
                    }

                    session.Result.etts = repo.GetTemplotsums(wh_no, calc_time);


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

        public DistriItem[] GetDistriResult(IEnumerable<BC_WHPICKDOC> list, DistriItem[] array) {

            foreach (BC_WHPICKDOC item in list) {
                int minIndex = GetMinComplexIndex(array);
                array[minIndex].Docs.Add(item);
                array[minIndex].ComplexSum += item.COMPLEXITY;
                array[minIndex].AppitemSum += item.Picks.Count();
            }

            return array;
        }
        public int GetMinComplexIndex(DistriItem[] array) {
            int currentMinIndex = 0;
            for (int i = 0; i < array.Length; i++) {
                if (array[i].ComplexSum < array[currentMinIndex].ComplexSum) {
                    currentMinIndex = i;
                }
            }
            return currentMinIndex;
        }

        [HttpPost]
        public ApiResponse SetPickUser(BC_WHPICKDOC bc_whpick) {

            IEnumerable<BC_WHPICKDOC> picks = JsonConvert.DeserializeObject<IEnumerable<BC_WHPICKDOC>>(bc_whpick.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    string userId = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    foreach (BC_WHPICKDOC pick in picks)
                    {
                        session.Result.afrs = repo.InsertBcwhpicklot(pick.WH_NO, pick.PICK_DATE, int.Parse(pick.LOT_NO), pick.PICK_USERID, userId, ip);

                        session.Result.afrs = repo.UpdateBcwhpickdocReg(pick.WH_NO, pick.PICK_DATE, pick.CALC_TIME, int.Parse(pick.LOT_NO), userId, ip);

                        session.Result.afrs = repo.UpdateBcwhpickReg(pick.WH_NO, pick.PICK_DATE, pick.CALC_TIME, int.Parse(pick.LOT_NO), pick.PICK_USERID, userId, ip);
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

        #region by inid 2020-04-06
        public ApiResponse DistributeRegularByInid(FormDataCollection form)
        {

            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var userCount = form.Get("USER_COUNT");

            DistriItem[] array = new DistriItem[int.Parse(userCount)];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new DistriItem();
            }

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                string userId = DBWork.UserInfo.UserId;
                string ip = DBWork.ProcIP;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    // 記錄現在時間
                    string calc_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                    // 刪除批次分配站存歷史紀錄
                    int deleteLotsum = repo.DeleteTemplotsum(wh_no);
                    // 刪除批次分配申請單站存歷史資料
                    int deleteLotdoc = repo.DeleteTemplotdoc(wh_no);

                    // 查詢最大分配揀貨批次號碼
                    int max_lot_no = repo.GetMaxLotNo(wh_no, pick_date);

                    //依inid與mat_class取得docnos
                    IEnumerable<BC_WHPICKDOC> inids = repo.GetMasterAllByTowh(wh_no, pick_date, "1", "=", false).OrderByDescending(x => x.COMPLEXITY);

                    array = GetDistriResultByInid(inids, array);

                    for (int i = 0; i < array.Length; i++) {
                        max_lot_no ++;
                        // 存入暫存檔
                        int sumResult = repo.InsertTemplotsum(wh_no, calc_time, max_lot_no, array[i].ComplexSum, array[i].DocnoSum, array[i].AppitemSum, userId, ip);

                        int docResult = 0;
                        foreach (string docno in array[i].Docnos) {
                            docResult += repo.InsertTemplotdoc(wh_no, calc_time, max_lot_no, docno);
                        }
                    }

                    DBWork.Commit();

                    IEnumerable<BC_WHPICK_TEMP_LOTSUM> sums = repo.GetTemplotsumsN(wh_no, calc_time);


                    session.Result.etts = GetArrangedTemolotsums(sums);
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        public DistriItem[] GetDistriResultByInid(IEnumerable<BC_WHPICKDOC> list, DistriItem[] array)
        {

            foreach (BC_WHPICKDOC item in list)
            {
                int minIndex = GetMinComplexIndex(array);
                array[minIndex].Docs.Add(item);
                array[minIndex].ComplexSum += item.COMPLEXITY;

                array[minIndex].AppitemSum += int.Parse(item.MMCODE_COUNTS);
                array[minIndex].Inids.Add(item.INID);
                array[minIndex].DocnoSum += int.Parse(item.DOCNO_COUNTS);
                array[minIndex].Docnos.AddRange(GetDocnos(item.DOCNOS));
            }

            return array;
        }
        public List<string> GetDocnos(string docnos_input) {
            string[] splitString = docnos_input.Split(',');
            List<string> list = new List<string>();
            foreach (string temp in splitString) {
                list.Add(temp.Replace("'", string.Empty));
            }
            return list;
        }
        public List<BC_WHPICK_TEMP_LOTSUM> GetArrangedTemolotsums(IEnumerable<BC_WHPICK_TEMP_LOTSUM> sums) {
            var o = from a in sums
                    group a by new { a.WH_NO, a.CALC_TIME, a.LOT_NO, a.DOCNO_SUM, a.APPITEM_SUM, a.COMPLEXITY_SUM } into g
                    select new BC_WHPICK_TEMP_LOTSUM {
                        WH_NO = g.Key.WH_NO,
                        CALC_TIME = g.Key.CALC_TIME,
                        LOT_NO = g.Key.LOT_NO,
                        DOCNO_SUM = g.Key.DOCNO_SUM,
                        APPITEM_SUM = g.Key.APPITEM_SUM,
                        COMPLEXITY_SUM = g.Key.COMPLEXITY_SUM,
                        DOCNOS_02 = GetDocnos(g.Where(x=>x.MATCLS_GRP == "02").Select(x=>x.DOCNO)),
                        DOCNOS_0X = GetDocnos(g.Where(x => x.MATCLS_GRP == "0X").Select(x => x.DOCNO)),
                    };

            return o.ToList();
        }
        public string GetDocnos(IEnumerable<string> docnos) {
            string result = string.Empty;
            foreach (string docno in docnos) {
                if (result != string.Empty) {
                    result += ",";
                }
                result += string.Format("'{0}'", docno);
            }
            return result;
        }
        #endregion

        #region kind3 2020-04-07
        public ApiResponse SetKind3Sheets(FormDataCollection form) {
            string wh_no = form.Get("wh_no");
            string pick_date = form.Get("pick_date");

            using (WorkSession session = new WorkSession(this)) {

                var DBWork = session.UnitOfWork;
                string userId = DBWork.UserInfo.UserId;
                string ip = DBWork.ProcIP;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);

                    // 查詢最大分配揀貨批次號碼
                    int max_lot_no = repo.GetMaxLotNo(wh_no, pick_date);
                    max_lot_no++;

                    IEnumerable<BC_WHPICKDOC> temp_whpickdocs = repo.GetMasterAllByTowh(wh_no, pick_date, "3", "=", false);
                    string docnos = String.Join(", ", temp_whpickdocs.Select(x => x.DOCNOS).ToList());

                    IEnumerable<string> noManagerid = repo.CheckManageridExists(wh_no, pick_date, docnos);
                    if (noManagerid.Any()) {
                        session.Result.success = false;
                        string temp = String.Join("、", noManagerid.Select(x => x).ToList());
                        session.Result.msg = string.Format("下列院內碼尚未設定品項管理人員，請先設定<br>{0}",temp);
                        return session.Result;

                    }
                    string firstManagerid = repo.GetKind3Pickusers(wh_no, pick_date, docnos);
                    session.Result.afrs = repo.InsertBcwhpicklot(wh_no, pick_date, max_lot_no, firstManagerid, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    session.Result.afrs = repo.UpdateKind3PickDocs(wh_no, pick_date, docnos, max_lot_no, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    session.Result.afrs = repo.UpdateKind3Picks(wh_no, pick_date, docnos, max_lot_no, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    DBWork.Commit();

                }
                catch {
                    DBWork.Rollback();
                    throw;
                }
                

                return session.Result;
            }
        }
        #endregion

        #endregion

        #region 已排揀貨批次
        [HttpPost]
        public ApiResponse DistributedMaster(FormDataCollection form) {

            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var apply_kind = form.Get("APPLY_KIND");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.etts = repo.GetBcwhpicklots(wh_no, pick_date, apply_kind);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse DistributedDetail(FormDataCollection form)
        {

            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var lot_no = int.Parse(form.Get("LOT_NO"));
            var sorters = form.Get("sorter");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.etts = repo.GetPicksByLotno(wh_no, pick_date, lot_no, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }



        [HttpPost]
        public ApiResponse CancelDistributed(FormDataCollection form) {
            //var wh_no = form.Get("WH_NO");
            //var pick_date = form.Get("PICK_DATE");
            //var lot_no = form.Get("LOT_NO");
            var itemString = form.Get("ITEM_STRING");

            IEnumerable<BC_WHPICKLOT> items = JsonConvert.DeserializeObject<IEnumerable<BC_WHPICKLOT>>(itemString);


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string user = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    var repo = new CD0002Repository(DBWork);

                    foreach (BC_WHPICKLOT item in items) {
                        session.Result.afrs = repo.UpdateBcwhpickPickuser(item.WH_NO, item.PICK_DATE, item.LOT_NO, user, ip);
                        session.Result.afrs = repo.UpdateBcwhpickdocLotno(item.WH_NO, item.PICK_DATE, item.LOT_NO, user, ip);
                        session.Result.afrs = repo.DeleteBcwhpicklot(item.WH_NO, item.PICK_DATE, item.LOT_NO);
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

        #region 全部重新分配

        public ApiResponse ClearDistribution(FormDataCollection form) {
            var wh_no = form.Get("WH_NO");
            var pick_date = form.Get("PICK_DATE");
            var apply_kind = form.Get("APPLY_KIND");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string user = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    var repo = new CD0002Repository(DBWork);

                    session.Result.afrs = repo.UpdateBcwhpickForClearAll(wh_no, pick_date, apply_kind, user, ip);

                    session.Result.afrs = repo.DeleteBcwhpicklotForClearAll(wh_no, pick_date, apply_kind);

                    session.Result.afrs = repo.UpdateBcwhpickdocForClearAll(wh_no, pick_date, apply_kind, user, ip);
                    
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

        #region combo
        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDutyUserCombo(DutyUser user)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0002Repository(DBWork);
                    session.Result.etts = repo.GetDutyUserCombo(user.WH_NO);
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