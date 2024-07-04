using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace WebApp.Controllers.C
{
    public class CD0011Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 庫房代碼
            var p2 = form.Get("p2"); // 揀貨日期
            var p3 = form.Get("p3"); // 揀貨人員
            var p4 = form.Get("p4"); // 分配批次
            var p5 = form.Get("p5"); // 條碼
            var p6 = form.Get("p6"); // 申請單號
            var p7 = form.Get("p7"); // 申請單位
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0011Repository(DBWork);
                    var repo1 = new CD0008Repository(DBWork);

                    string getMmcode = p5;
                    if (p5 != null && p5 != "")
                    {
                        getMmcode = repo1.getMmcodeByScan(p5);
                    }
                    session.Result.etts = repo.GetAll(p0, p2, p3, p4, getMmcode, p6, p7, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ValidAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0011Repository(DBWork);
                    session.Result.etts = repo.GetValidAll(p0, p1, p2, p3);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse PickAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0011Repository(DBWork);
                    session.Result.etts = repo.GetPickAll(p0, p1, p2, p3);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SetActPickQty(FormDataCollection form)
        {
            string wh_no = form.Get("wh_no");
            string pick_date = form.Get("pick_date");
            string docno = form.Get("docno");
            string seq = form.Get("seq");
            string pick_userid = form.Get("pick_userid");
            string lot_no = form.Get("lot_no");
            string mmcode = form.Get("mmcode");
            string act_pick_qty = form.Get("act_pick_qty");
            string act_pick_note = form.Get("act_pick_note");
            string wexp_id = form.Get("wexp_id");
            string lot_noF = form.Get("lot_no_f");
            string valid_date = form.Get("valid_date").Split('T')[0];
            string setType = form.Get("setType"); // U:修改, D刪除
            // string[] submitT = form.Get("submitt").Split('ˋ');

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0011Repository(DBWork);
                    // 檢查目前揀貨申請單是否已分配揀貨批次，如沒有，則設定揀貨批次與預設揀貨人員
                    if (repo.chkLotNo(wh_no, docno) == "0")
                    {
                        string mat_class = repo.getMatClass(wh_no, docno, seq);
                        string apply_kind = repo.getApplyKind(wh_no, docno);
                        string newLotNo = repo.getNewLotNO(wh_no, docno, pick_date, mat_class, apply_kind);
                        repo.updateLotNo(wh_no, docno, newLotNo);
                        repo.updatePickUser(wh_no, docno, pick_userid);
                        repo.insertWhpicklot(wh_no, pick_date, newLotNo, pick_userid);
                        lot_no = newLotNo;
                    }
                    // 記錄實際揀貨內容資料
                    session.Result.afrs = repo.Update(wh_no, pick_date, docno, seq, pick_userid, lot_no, mmcode, act_pick_qty, act_pick_note, setType,
                        User.Identity.Name, DBWork.ProcIP);
                    // session.Result.etts = repo.Get(wh_no, pick_date, docno, seq, pick_userid, lot_no, mmcode);

                    // 若為批號效期管制才新增資料
                    if (wexp_id == "Y")
                    {
                        repo.DeleteWhpickValid(wh_no, pick_date, docno, seq);
                        repo.CreateWhpickValid(wh_no, pick_date, docno, seq, lot_noF, valid_date, act_pick_qty, User.Identity.Name, DBWork.ProcIP);
                    }
                    // 若效期項目有資料
                    //if (submitT.Length > 0 && submitT[0].Trim() != "")
                    //{
                    //    repo.DeleteWhpickValid(wh_no, pick_date, docno, seq);
                    //    // 記錄實際揀貨效期資料
                    //    foreach (string strData in submitT)
                    //    {
                    //        string lot_no_valid = strData.Split('^')[0];
                    //        string valid_date = strData.Split('^')[1];
                    //        valid_date = valid_date.PadLeft(7, '0');
                    //        string valid_date_y = (Convert.ToInt32(valid_date.Substring(0, 3)) + 1911).ToString();
                    //        string valid_date_m = valid_date.Substring(3, 2);
                    //        string valid_date_d = valid_date.Substring(5, 2);
                    //        valid_date = valid_date_y + "-" + valid_date_m + "-" + valid_date_d;

                    //        string act_pick_qty_valid = strData.Split('^')[2];
                    //        repo.CreateWhpickValid(wh_no, pick_date, docno, seq, lot_no_valid, valid_date, act_pick_qty_valid, User.Identity.Name, DBWork.ProcIP);
                    //    }
                    //}


                    string MAT_CLASS = repo.getMatClass(mmcode);
                    if (MAT_CLASS == "01")
                    {
                        // 藥品類單項更新申請單核撥量
                        repo.UpdateMeDocd(docno, seq, act_pick_qty, User.Identity.Name);
                        session.Result.msg = repo.procDistin(docno, User.Identity.Name, DBWork.ProcIP);
                        // 藥品直接設定出庫
                        repo.updateShipout(wh_no, pick_date, docno, seq); // 更新揀貨品項為出庫狀態
                        repo.insertWhpickShipout(wh_no, pick_date, docno, seq); // 新增出庫申請單品項資料
                        repo.insertDocexp(wh_no, pick_date, docno, seq, mmcode); // 新增效期資料到點收品項效期資料中
                    }
                    else
                    {
                        repo.UpdateMeDocd(docno, seq, act_pick_qty);
                        session.Result.msg = "Y";
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
        public ApiResponse SetPickAll(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 庫房代碼
            var p2 = form.Get("p2"); // 揀貨日期
            var p3 = form.Get("p3"); // 揀貨人員
            var p4 = form.Get("p4"); // 分配批次
            var p5 = form.Get("p5"); // 院內碼
            var p6 = form.Get("p6"); // 申請單號
            var p7 = form.Get("p7"); // 申請單位

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0011Repository(DBWork);
                    session.Result.afrs = repo.updatePickAll(p0, p2, p3, p4, p5, p6, p7);

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
        public ApiResponse ChkBarcode(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 庫房代碼
            var p2 = form.Get("p2"); // 揀貨日期
            var p3 = form.Get("p3"); // 揀貨人員
            var p4 = form.Get("p4"); // lot_no
            var p5 = form.Get("p5"); // 條碼
            var p6 = form.Get("p6"); // docno

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0011Repository(DBWork);

                    // 先找申請單號
                    if (repo.ChkDocno(p0, p2, p3, p5) > 0)
                        session.Result.msg = "D";
                    else
                    {
                        // 再找申請單位
                        if (repo.ChkAppdept(p0, p2, p3, p5) > 0)
                            session.Result.msg = "A";
                        else
                        {
                            // 最後找院內碼
                            p5 = repo.getChkMmcode(p5); // 檢查條碼是否有對應的院內碼,有則填入院內碼,否則保持原條碼
                            if (repo.ChkMmcode(p0, p2, p3, p4, p5, p6) > 0) // 有找到院內碼
                                session.Result.msg = p5;
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

        [HttpPost]
        public ApiResponse ChkMmcode(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 條碼

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0011Repository(DBWork);
                    session.Result.msg = repo.getChkMmcode(p0); // 檢查條碼是否有對應的院內碼,有則填入院內碼,否則保持原條碼
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse chkPickCnt(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 庫房代碼
            var p2 = form.Get("p2"); // 揀貨日期
            var p3 = form.Get("p3"); // 揀貨人員
            var p4 = form.Get("p4"); // 分配批次
            var p6 = form.Get("p6"); // 申請單號
            var p7 = form.Get("p7"); // 申請單位

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0011Repository(DBWork);
                    if (repo.chkPickCnt(p0, p2, p3, p4, p6, p7) > 0)
                        session.Result.msg = "T"; // 所有品項均揀貨完成但尚未出貨
                    else
                        session.Result.msg = "F";
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SetShipoutAll(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 庫房代碼
            var p2 = form.Get("p2"); // 揀貨日期
            var p3 = form.Get("p3"); // 揀貨人員
            var p4 = form.Get("p4"); // 分配批次
            var p6 = form.Get("p6"); // 申請單號
            var p7 = form.Get("p7"); // 申請單位

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CD0011Repository(DBWork);
                    session.Result.afrs += repo.updateShipoutAll(p0, p2, p3, p4, p6, p7);
                    session.Result.afrs += repo.insertWhpickShipoutAll(p0, p2, p3, p4, p6, p7);
                    session.Result.afrs += repo.insertDocexpAll(p0, p2, p3, p4, p6, p7);
                    session.Result.afrs += repo.UpdateMeDocdAll(p0, p2, p3, p4, p6, p7);
                    session.Result.afrs += repo.UpdateMeDocmShipout(p0, p2, p3, p4, p6, p7);

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

        // 庫房號碼
        [HttpPost]
        public ApiResponse GetWH_NoCombo(FormDataCollection form)
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
                    CD0011Repository repo = new CD0011Repository(DBWork);
                    session.Result.etts = repo.GetWH_NoCombo(p0, page, limit, "")
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 揀貨人員
        [HttpPost]
        public ApiResponse GetPickUserCombo(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            // var pick_date = form.Get("pick_date");
            var pick_user = form.Get("pick_user");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0011Repository repo = new CD0011Repository(DBWork);
                    // 庫房號碼為藥庫PH1S時，需先查詢此庫房待揀貨資料，以利緊急需求時可查詢到新增的申請單編號資料
                    if (wh_no.ToString() == "PH1S")
                    {
                        repo.insertNewWhpick(wh_no);
                        repo.insertNewWhpickDoc(wh_no);
                    }

                    session.Result.etts = repo.GetPickUserCombo(wh_no);
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
        public ApiResponse GetLotNoCombo(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var pick_date = form.Get("pick_date");
            var pick_user = form.Get("pick_user");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0011Repository repo = new CD0011Repository(DBWork);
                    session.Result.etts = repo.GetLotNoCombo(wh_no, pick_date, pick_user);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 揀貨申請單
        [HttpPost]
        public ApiResponse GetDocnoCombo(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var pick_date = form.Get("pick_date");
            var pick_user = form.Get("pick_user");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0011Repository repo = new CD0011Repository(DBWork);
                    session.Result.etts = repo.GetDocnoCombo(wh_no, pick_date, pick_user);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 揀貨申請單位
        [HttpPost]
        public ApiResponse GetAppdeptCombo(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var pick_date = form.Get("pick_date");
            var pick_user = form.Get("pick_user");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0011Repository repo = new CD0011Repository(DBWork);
                    session.Result.etts = repo.GetAppdeptCombo(wh_no, pick_date, pick_user);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetLotNoDataCombo(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var mmcode = form.Get("mmcode");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CD0011Repository repo = new CD0011Repository(DBWork);
                    session.Result.etts = repo.GetLotNoDataCombo(wh_no, mmcode);
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
                    CD0011Repository repo = new CD0011Repository(DBWork);
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

    }
}