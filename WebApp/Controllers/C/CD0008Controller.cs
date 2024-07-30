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
    public class CD0008Controller : SiteBase.BaseApiController
    {
        public ApiResponse GetWhnoCombo() {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                string userId = DBWork.UserInfo.UserId;
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(userId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDocnoCombo(FormDataCollection form) {
            var wh_no = form.Get("P0");
            var pick_date = form.Get("P1");
            var isOut = form.Get("P2") == "Y";

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                string userId = DBWork.UserInfo.UserId;
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    session.Result.etts = repo.GetDocnoCombo(wh_no, pick_date, isOut);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetChkWh(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                string userId = DBWork.UserInfo.UserId;
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    if (repo.GetChkWh(User.Identity.Name) > 0)
                        session.Result.msg = "Y";
                    else
                        session.Result.msg = "N";
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse All(FormDataCollection form) {
            var wh_no = form.Get("P0");
            var pick_date = form.Get("P1");
            var isOut = form.Get("P2") == "Y";
            var docno = form.Get("P3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    session.Result.etts = repo.All(wh_no, pick_date, isOut, docno, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetBcBox(FormDataCollection form) {
            var input = form.Get("input");
            var source = form.Get("source");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    if (source == "boxNo") {
                        session.Result.etts = repo.GetBcBoxByBoxno(input);
                    }else {
                        session.Result.etts = repo.GetBcBoxByBarcode(input);
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
        public ApiResponse PutInBox(BC_WHPICK whpick) {
            IEnumerable<BC_WHPICK> picks = JsonConvert.DeserializeObject<IEnumerable<BC_WHPICK>>(whpick.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    string userId = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;
                    
                    foreach (BC_WHPICK pick in picks)
                    {
                        pick.PICK_DATE = DateTime.Parse(pick.PICK_DATE).ToString("yyyy-MM-dd");
                        pick.UPDATE_USER = userId;
                        pick.UPDATE_IP = ip;
                        session.Result.afrs = repo.UpdateBcwhpick(pick);
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
        [HttpPost]
        public ApiResponse PutOutBox(BC_WHPICK whpick)
        {
            IEnumerable<BC_WHPICK> picks = JsonConvert.DeserializeObject<IEnumerable<BC_WHPICK>>(whpick.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    string userId = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    foreach (BC_WHPICK pick in picks)
                    {
                        pick.PICK_DATE = DateTime.Parse(pick.PICK_DATE).ToString("yyyy-MM-dd");
                        pick.BOXNO = "";
                        pick.BARCODE = "";
                        pick.XCATEGORY = "";
                        pick.CREATE_USER = userId;
                        pick.UPDATE_USER = userId;
                        pick.UPDATE_IP = ip;
                        session.Result.afrs = repo.UpdateBcwhpick(pick);
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
        [HttpPost]
        public ApiResponse ShipOut(BC_WHPICK bc_whpick) {
            IEnumerable<BC_WHPICK> picks = JsonConvert.DeserializeObject<IEnumerable<BC_WHPICK>>(bc_whpick.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    string userId = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    foreach (BC_WHPICK pick in picks)
                    {
                        pick.PICK_DATE = DateTime.Parse(pick.PICK_DATE).ToString("yyyy/MM/dd");
                        pick.CREATE_USER = userId;
                        pick.UPDATE_USER = userId;
                        pick.UPDATE_IP = ip;
                        session.Result.afrs = repo.UpdateBcwhpickForShipout(pick); // 設定申請品項出庫狀態
                        session.Result.afrs = repo.InsertBcWhpickShipout(pick); // 新增出庫申請單品項資料
                        session.Result.afrs = repo.InsertMeDocexpShipout(pick); // 新增效期資料到點收品項效期資料中

                        // 未揀貨藥品補呼叫proc轉狀態
                        if (pick.MAT_CLASS == "01" 
                            && (pick.ACT_PICK_USERID == null || pick.ACT_PICK_USERID.Trim() == "")
                            && (pick.ACT_PICK_QTY == null || pick.ACT_PICK_QTY == "" || pick.ACT_PICK_QTY == "0"))
                        {
                            repo.UpdateMeDocd(pick.DOCNO, pick.SEQ);
                            repo.POST_DOC(pick.DOCNO, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }

                        if (repo.GetBcWhpickCnt(pick) == 0)
                        {
                            if (pick.MAT_CLASS != "01")
                            {
                                // 若申請單品項都已出庫,則進行更新明細/改變狀態等動作
                                foreach (BC_WHPICK pickInDoc in repo.GetBcWhpickByDocno(pick))
                                {
                                    // 更新申請單明細檔揀貨資料
                                    session.Result.afrs = repo.UpdateMeDocdForShipout(pickInDoc);
                                }
                            }
                            
                            if (pick.MAT_CLASS == "01")
                            {
                                // repo.POST_DOC(pick.DOCNO, pick.UPDATE_USER, pick.UPDATE_IP); // 藥品
                            }
                            else
                                repo.UpdateNotDrm(pick.DOCNO, pick.UPDATE_USER, pick.UPDATE_IP); // 衛材及其他類
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
        [HttpPost]
        public ApiResponse chkBcWhpick(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var pick_date = form.Get("pick_date");
            var docno = form.Get("docno");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    session.Result.msg = repo.chkBcWhpick(wh_no, pick_date, docno).ToString();

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmcodeScan(FormDataCollection form)
        {
            var barcode = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    session.Result.msg = repo.getMmcodeByScan(barcode).ToString();

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse docShipOut(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var pick_date = form.Get("pick_date");
            var docno = form.Get("docno");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    foreach (BC_WHPICK eachData in repo.getDocBcWhpick(wh_no, pick_date, docno))
                    {
                        string v_act_pick_qty;
                        if (eachData.ACT_PICK_USERID == null || eachData.ACT_PICK_USERID == "")
                            v_act_pick_qty = "0";
                        else
                            v_act_pick_qty = eachData.ACT_PICK_QTY;
                        eachData.ACT_PICK_QTY = v_act_pick_qty;

                        if (eachData.PICK_DATE != null)
                            eachData.PICK_DATE = DateTime.Parse(eachData.PICK_DATE).ToString("yyyy/MM/dd");
                        if (eachData.ACT_PICK_TIME != null)
                            eachData.ACT_PICK_TIME = DateTime.Parse(eachData.ACT_PICK_TIME).ToString("yyyy/MM/dd");

                        // 未揀貨藥品或非藥品類更新申請單明細資料
                        if ((eachData.MAT_CLASS == "01"
                            && (eachData.ACT_PICK_USERID == null || eachData.ACT_PICK_USERID == "")
                            && eachData.HAS_SHIPOUT == null)
                            || eachData.MAT_CLASS != "01")
                        {
                            // 更新申請單明細
                            repo.UpdateMeDocdForShipout(eachData);
                            if (eachData.MAT_CLASS == "01"
                                && (eachData.ACT_PICK_USERID == null || eachData.ACT_PICK_USERID == "")
                                && eachData.HAS_SHIPOUT == null)
                            {
                                repo.POST_DOC(eachData.DOCNO, DBWork.UserInfo.UserId, DBWork.ProcIP);
                            }
                        }
                        if (eachData.HAS_SHIPOUT == null)
                        {
                            repo.UpdateBcwhpickForShipout(eachData);
                            repo.InsertBcWhpickShipout(eachData);
                            repo.InsertMeDocexpShipout(eachData);
                        }
                    }
                    repo.UpdateNotDrm(docno, DBWork.UserInfo.UserId, DBWork.ProcIP);

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
        public ApiResponse ShipOutCancel(BC_WHPICK bc_whpick)
        {
            IEnumerable<BC_WHPICK> picks = JsonConvert.DeserializeObject<IEnumerable<BC_WHPICK>>(bc_whpick.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0008Repository(DBWork);
                    string userId = DBWork.UserInfo.UserId;
                    string ip = DBWork.ProcIP;

                    foreach (BC_WHPICK pick in picks)
                    {
                        pick.PICK_DATE = DateTime.Parse(pick.PICK_DATE).ToString("yyyy-MM-dd");
                        pick.HAS_SHIPOUT = "";
                        pick.CREATE_USER = userId;
                        pick.UPDATE_USER = userId;
                        pick.UPDATE_IP = ip;
                        session.Result.afrs = repo.UpdateBcwhpickCancelShipout(pick);
                        session.Result.afrs = repo.DeleteBcWhpickShipout(pick);
                        session.Result.afrs = repo.DeleteMeDocexpShipout(pick);
                        // 藥品之外(mat_class<>'01')的申請單需更新申請單狀態為揀料中狀態
                        if (pick.MAT_CLASS != "01")
                            session.Result.afrs = repo.UpdateMeDocmCancelShipout(pick);
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
    }
}