using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using WebApp.Repository.C;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Controllers.AB
{
    public class AB0091Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0").Trim();
            var p2 = form.Get("p2").Trim();
            var p3 = form.Get("p3").Trim();
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0091Repository(DBWork);
                    p3 = repo.ChkMmcode(p3);
                    session.Result.etts = repo.GetAll(p0, p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailAll(FormDataCollection form)
        {
            var p0 = form.Get("p0").Trim();
            var p1 = form.Get("p1").Trim(); // 目前選取資料的院內碼
            var p2 = form.Get("p2").Trim(); // 查詢條件的院內碼(同時做查詢物流箱編號用)
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0091Repository(DBWork);
                    //p2 = repo.ChkMmcode(p2);
                    session.Result.etts = repo.GetDetailAll(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse WexpAll(FormDataCollection form)
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
                    var repo = new AB0091Repository(DBWork);
                    session.Result.etts = repo.GetValidAll(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAckCnt(FormDataCollection form)
        {
            string docno = form.Get("docno");
            string getType = form.Get("getType"); // Y:已點收, N:未點收

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0091Repository(DBWork);
                    session.Result.msg = repo.GetAckCnt(docno, getType).ToString();

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
        public ApiResponse SetAckQty(FormDataCollection form)
        {
            string docno = form.Get("docno");
            string mmcode = form.Get("mmcode");
            string ackqty = form.Get("ackqty");
            string setType = form.Get("setType"); // U:修改, D刪除

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0091Repository(DBWork);
                    // setType現在應不會有D(取消), 20190911增列更新POSTID=4
                    session.Result.afrs += repo.Update(docno, mmcode, ackqty, setType, User.Identity.Name, DBWork.ProcIP); //update ME_DOCD
                    session.Result.afrs += repo.UpdateMI_WINVCTL(docno, mmcode);
                    repo.POST_DOC(docno, User.Identity.Name, DBWork.ProcIP);
                    ME_DOCM docm = repo.GetDocm(docno);
                    // 點收完後若全品項都已點收完,則繼續做[整張申請單點收完成]
                    if (repo.GetAckCnt(docno, "N") == 0)
                    {
                        // session.Result.afrs += repo.UpdateDocStatusDrm(docno, User.Identity.Name, DBWork.ProcIP); // 藥品
                        session.Result.afrs += repo.UpdateDocStatusNotDrm(docno, User.Identity.Name, DBWork.ProcIP); // 非藥品
                    }
                    else
                    {
                        // 若尚有未點收品項,則恢復成先前狀態
                        // session.Result.afrs += repo.UpdateDocStatusDrmRev(docno, User.Identity.Name, DBWork.ProcIP); // 藥品
                        session.Result.afrs += repo.UpdateDocStatusNotDrmRev(docno, User.Identity.Name, DBWork.ProcIP); // 非藥品
                    }

                    // 更新藥品以外申請單狀態改至CD0008做
                    // session.Result.afrs += repo.UpdateNotDrm(docno, mmcode, ackqty, setType, User.Identity.Name, DBWork.ProcIP);

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
        public ApiResponse SetTmpAckQty(FormDataCollection form)
        {
            string docno = form.Get("docno");
            string mmcode = form.Get("mmcode");
            string ackqty = form.Get("ackqty");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0091Repository(DBWork);
                    session.Result.afrs += repo.UpdateTmp(docno, mmcode, ackqty);
                    
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
        public ApiResponse SetDocComplete(FormDataCollection form)
        {
            string docno = form.Get("p0");
            string setType = form.Get("p1"); // 0:只更新狀態, 1:先更新未點收項目再更新狀態

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0091Repository(DBWork);
                    string isDrm = repo.getMatClassByDocno(docno);

                    if (setType == "1")
                        session.Result.afrs += repo.UpdateDetailAckid(docno, User.Identity.Name, DBWork.ProcIP, isDrm); //update ME_DOCD
                    session.Result.afrs += repo.UpdateMI_WINVCTL(docno);
                    // session.Result.afrs += repo.UpdateDocStatusDrm(docno, User.Identity.Name, DBWork.ProcIP); // 藥品
                    session.Result.afrs += repo.UpdateDocStatusNotDrm(docno, User.Identity.Name, DBWork.ProcIP); // 非藥品

                    if (isDrm == "Y")
                        repo.POST_DOC(docno, User.Identity.Name, DBWork.ProcIP);

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

        // 部門號碼
        [HttpPost]
        public ApiResponse GetWH_NoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0091Repository repo = new AB0091Repository(DBWork);
                    session.Result.etts = repo
                        .GetWH_NoCombo(p0, DBWork.ProcUser, page, limit, "")
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_USERID = w.WH_USERID });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH_NoComboSimple(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0091Repository repo = new AB0091Repository(DBWork);
                    session.Result.etts = repo.GetWH_NoComboSimple(DBWork.ProcUser);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 揀貨批次
        [HttpPost]
        public ApiResponse GetDocnoCombo(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0091Repository repo = new AB0091Repository(DBWork);
                    session.Result.etts = repo.GetDocnoCombo(wh_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 檢查MMCODE:
        // 非點收中品項->回傳mmnotfound
        // 點收中品項,已點收->回傳mmdup
        // 點收中品項,未點收->回傳MMCODE
        [HttpPost]
        public ApiResponse ChkMmcode(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 部門號碼
            var p1 = form.Get("p2"); // 申請單號
            var p2 = form.Get("p3"); // 院內碼

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    string rtnMsg = "";
                    var repo = new AB0091Repository(DBWork);
                    var repo1 = new CD0008Repository(DBWork);
                    string rtnMmcode = repo1.getMmcodeByScan(p2);
                    if (p1 != "" && p1 != null) // 有選申請單號, 查詢院內碼是否有在申請單中
                    {
                        string mmcodeChk = repo.ChkMmcode(p1, rtnMmcode); // 若查詢有資料則回傳MMCODE, 否則回傳mmnotfound
                        if (mmcodeChk == "notfound")
                            rtnMsg = "mmnotfound";
                        else
                        {
                            int dupChk = repo.ChkMmcodeDup(p1, rtnMmcode);
                            if (dupChk > 0)
                                rtnMsg = rtnMmcode;
                            else
                                rtnMsg = "mmdup";
                        }
                    }
                    else // 沒選申請單號, 
                    {
                        string mmcodeChk = repo.ChkMmcode(rtnMmcode);
                        if (mmcodeChk == "notfound")
                            rtnMsg = "mmnotfound";
                        else
                        {
                            rtnMsg = "querydoc";
                        }
                    }
                    
                    session.Result.msg = rtnMsg;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkScanner(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 條碼

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    string rtnMsg = "";
                    var repo = new AB0091Repository(DBWork);
                    rtnMsg = repo.getScannerVal(p0);

                    session.Result.msg = rtnMsg;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetChkUserWhno()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0091Repository repo = new AB0091Repository(DBWork);
                    session.Result.etts = repo.GetChkUserWhno(DBWork.ProcUser)
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse chkBcWhpick(FormDataCollection form)
        {
            var docno = form.Get("docno");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0091Repository(DBWork);
                    session.Result.msg = repo.chkBcWhpick(docno).ToString();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ackqtySync(FormDataCollection form)
        {
            string docno = form.Get("docno");
            string mmcode = form.Get("mmcode");
            string adjqty = form.Get("adjqty");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    DBWork.BeginTransaction();
                    try
                    {
                        string rtnMsg = "";
                        var repo = new AB0091Repository(DBWork);
                        int updRow = repo.updateAckqtyByAdjqty(docno, mmcode, Convert.ToInt32(adjqty));
                        if (updRow > 0)
                            rtnMsg = repo.getNewAckqty(docno, mmcode);

                        session.Result.msg = rtnMsg;
                        DBWork.Commit();
                    }
                    catch
                    {
                        DBWork.Rollback();
                        throw;
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        #region AB0091_1
        List<string> matDocTypes = new List<string>() { "MR1", "MR2", "MR3", "MR4"};
        List<string> matFlowIds = new List<string>() { "3","4"};
        List<string> mrFlowIds = new List<string>() { "0102", "0103" };
        List<string> msFlowIds = new List<string>() { "0602", "0603" };

        [HttpPost]
        public ApiResponse DetailQuery(FormDataCollection form) {

            var p0 = form.Get("p0").Trim(); // WH_NO
            var p2 = form.Get("p2").Trim(); // docno
            var p3 = form.Get("p3").Trim(); // barcode

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0091Repository(DBWork);
                    session.Result.etts = repo.DetailQuery(p2, p3);
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateAccAckQty(FormDataCollection form) {
            string docno = form.Get("docno");
            string mmcode = form.Get("mmcode");
            string tratio_string = string.IsNullOrEmpty(form.Get("tratio")) ? "1" : form.Get("tratio");
            string acktimes_string = string.IsNullOrEmpty(form.Get("acktimes")) ? "0" : form.Get("acktimes");
            string adjqty_string = string.IsNullOrEmpty(form.Get("adjqty")) ? "0" : form.Get("adjqty");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0091Repository(DBWork);

                    // 檢查申請單狀態
                    ME_DOCM docm = repo.GetDocm(docno);
                    bool flowIdValid = CheckFlowId(docm);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新確認";
                        session.Result.success = false;
                        return session.Result;
                    }

                    // 檢查該筆資料狀態
                    ME_DOCD docd = repo.GetDocd(docno, mmcode);
                    if (string.IsNullOrEmpty(docd.ACKID) == false) {
                        session.Result.msg = "此筆資料已點收，請重新確認";
                        session.Result.success = false;
                        return session.Result;
                    }

                    int tratio = int.Parse(tratio_string);
                    int acktimes = int.Parse(acktimes_string);
                    int adjqty = int.Parse(adjqty_string);
                    int addqty = tratio * acktimes + adjqty;

                    // 累計欄位加上數量，更新ACKQTY
                    session.Result.afrs = repo.UpdateAccAckQty(docno, mmcode, addqty, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    docd = repo.GetDocd(docno, mmcode);
                    // 核撥量 = 點收量，更新POSTID
                    if (docd.ACKQTY == docd.APVQTY) {
                        session.Result.afrs = repo.UpdateDocdPostId4(docno, mmcode, DBWork.UserInfo.UserId, DBWork.ProcIP); //update ME_DOCD
                        session.Result.afrs += repo.UpdateMI_WINVCTL(docno, mmcode);

                        repo.POST_DOC(docno, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    }

                    // 點收完後若全品項都已點收完,則繼續做[整張申請單點收完成]
                    if (repo.GetAckCnt(docno, "N") == 0)
                    {
                        // session.Result.afrs += repo.UpdateDocStatusDrm(docno, User.Identity.Name, DBWork.ProcIP); // 藥品
                        session.Result.afrs += repo.UpdateDocStatusNotDrm(docno, User.Identity.Name, DBWork.ProcIP); // 非藥品
                    }
                    else
                    {
                        // 若尚有未點收品項,則恢復成先前狀態
                        // session.Result.afrs += repo.UpdateDocStatusDrmRev(docno, User.Identity.Name, DBWork.ProcIP); // 藥品
                        session.Result.afrs += repo.UpdateDocStatusNotDrmRev(docno, User.Identity.Name, DBWork.ProcIP); // 非藥品
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
        public bool CheckFlowId(ME_DOCM docm) {
            if (matDocTypes.Contains(docm.DOCTYPE)) {
                if (matFlowIds.Contains(docm.FLOWID)) {
                    return true;
                }
            }
            if (docm.DOCTYPE == "MR") {
                if (mrFlowIds.Contains(docm.FLOWID)) {
                    return true;
                }
            }
            if (docm.DOCTYPE == "MS")
            {
                if (msFlowIds.Contains(docm.FLOWID))
                {
                    return true;
                }
            }

            return false;
        }

        [HttpPost]
        public ApiResponse GetCurrentAccAckQty(FormDataCollection form) {
            string docno = form.Get("docno");
            string mmcode = form.Get("mmcode");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {

                    var repo = new AB0091Repository(DBWork);
                    session.Result.msg = repo.GetAccAckQty(docno, mmcode);
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }
        #endregion
    }
}