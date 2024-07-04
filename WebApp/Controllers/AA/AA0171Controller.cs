using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0171Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var P0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0171Repository(DBWork);
                    session.Result.etts = repo.GetAllM(P0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

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
                    AA0171Repository repo = new AA0171Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0171Repository repo = new AA0171Repository(DBWork);
                    if (form.Get("MMCODE") != "")
                    {
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = mmcode.Split(',');
                        var v_num = repo.CheckWhMast();
                        //B.檢查登入者是否有藥庫權限，若無權限跳出提示「無藥庫權限，請重新確認」
                        if (!repo.CheckExistsUser(User.Identity.Name))
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "無藥庫權限，請重新確認。";
                            return session.Result;
                        }

                        //C.檢查是否有有效藥局庫房，若為0則跳出提示「無藥局可核撥，請重新確認」；若大於0則跳出提示「可核撥藥局數量不為1，需由藥局自行提出申請」
                        if (v_num == 0)
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "無藥局可核撥，請重新確認。";
                            return session.Result;
                        }
                        else if (v_num > 1)
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "可核撥藥局數量不為1，需由藥局自行提出申請。";
                            return session.Result;
                        }

                        //D.新增ME_DOCM
                        var v_docno = repo.GetDailyDocno();
                        var v_inid = repo.GetUridInid(User.Identity.Name);
                        var v_towh = repo.GetTowh();
                        ME_DOCM me_docm = new ME_DOCM();
                        me_docm.DOCNO = v_docno;
                        me_docm.DOCTYPE = "MR";
                        me_docm.FLOWID = "0102";
                        me_docm.APPID = User.Identity.Name;
                        me_docm.APPLY_KIND = "2";
                        me_docm.APPDEPT = v_inid;
                        me_docm.USEID = User.Identity.Name;
                        me_docm.USEDEPT = v_inid;
                        me_docm.TOWH = v_towh;
                        me_docm.MAT_CLASS = "01";
                        me_docm.APPLY_NOTE = "藥庫批次核撥";
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        repo.CreateM(me_docm);
                        //E.針對勾選資料逐筆處理
                        ME_DOCD me_docd = new ME_DOCD();
                        var v_s_inv_qty = "0";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            v_s_inv_qty = repo.GetsInvQty("whno_me1", tmp[i]);
                            me_docd.DOCNO = v_docno;
                            if (!repo.CheckExistsD(v_docno)) // 新增前檢查主鍵是否已存在
                            {
                                me_docd.SEQ = "1";
                            }
                            else
                            {
                                me_docd.SEQ = repo.GetDocDSeq(v_docno);
                            }
                            me_docd.MMCODE = tmp[i];
                            me_docd.APPQTY = v_s_inv_qty;
                            me_docd.APVQTY = v_s_inv_qty;
                            me_docd.APVID = User.Identity.Name;
                            me_docd.ACKQTY = v_s_inv_qty;
                            me_docd.ACKID = User.Identity.Name;
                            me_docd.S_INV_QTY = repo.GetsInvQty("whno_me1", tmp[i]);
                            me_docd.INV_QTY = repo.GetInvQty(v_towh, tmp[i]);
                            me_docd.SAFE_QTY = repo.GetSafeQty(v_towh, tmp[i]);
                            me_docd.OPER_QTY = repo.GetOperQty(v_towh, tmp[i]);
                            me_docd.EXPT_DISTQTY = v_s_inv_qty;
                            me_docd.PICK_QTY = v_s_inv_qty;
                            me_docd.DIS_USER = User.Identity.Name;
                            me_docd.CREATE_USER = User.Identity.Name;
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            repo.CreateD(me_docd);
                        }
                        //E.逐筆執行完成後以v_docno更新ME_DOCD
                        me_docd.DOCNO = v_docno;
                        me_docd.CREATE_USER = User.Identity.Name;
                        me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        repo.UpdateD(me_docd);
                        //F.執行SP：POST_DOC(為進行核撥)
                        var rtn = repo.CallProc(v_docno, User.Identity.Name, DBWork.ProcIP);
                        if (rtn != "Y")
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>申請單號「" + v_docno + "」</span>，發生核撥執行錯誤，" + rtn + "。";
                            return session.Result;
                        }
                        //G.更新ME_DOCD的post_id
                        me_docd.DOCNO = v_docno;
                        me_docd.CREATE_USER = User.Identity.Name;
                        me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        repo.UpdateDP(me_docd);
                        //H.再次執行POST_DOC(為進行點收)
                        var rtn2 = repo.CallProc(v_docno, User.Identity.Name, DBWork.ProcIP);
                        if (rtn2 != "Y")
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>申請單號「" + v_docno + "」</span>，發生點收執行錯誤，" + rtn + "。";
                            return session.Result;
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
    }
}