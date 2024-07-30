using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using System;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Controllers.AA
{
    public class AA0228Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var pp = form.Get("pp");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            string[] arr_p2 = { };
            if (!string.IsNullOrEmpty(p2) && !string.IsNullOrWhiteSpace(p2))
            {
                arr_p2 = p2.Trim().Split(','); //用,分割
            }
            string[] arr_p3 = { };
            if (!string.IsNullOrEmpty(p3))
            {
                arr_p3 = p3.Trim().Split(','); //用,分割
            }

            string tuser = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0228Repository(DBWork);

                    session.Result.etts = repo.GetAllM(p0, p1, arr_p2, arr_p3, pp, tuser, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
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
                    var repo = new AA0228Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, page, limit, sorters);
                }
                catch
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
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0228Repository repo = new AA0228Repository(DBWork);
                    var id = User.Identity.Name;
                    session.Result.etts = repo.GetMatclassCombo(id);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetLoginInfo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0228Repository repo = new AA0228Repository(DBWork);
                    var v_userid = User.Identity.Name;
                    var v_ip = DBWork.ProcIP;
                    session.Result.etts = repo.GetLoginInfo(v_userid, v_ip);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFlowidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0228Repository repo = new AA0228Repository(DBWork);
                    string id = User.Identity.Name;
                    session.Result.etts = repo.GetFlowidCombo(id);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmcode(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0228Repository repo = new AA0228Repository(DBWork);
                    ME_DOCM query = new ME_DOCM();
                    query.DOCNO = form.Get("DOCNO") == null ? "" : form.Get("DOCNO").ToUpper();
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS").ToUpper();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.M_AGENNO = form.Get("M_AGENNO") == null ? "" : form.Get("M_AGENNO").ToUpper();
                    query.AGEN_NAME = form.Get("AGEN_NAME") == null ? "" : form.Get("AGEN_NAME").ToUpper();
                    session.Result.etts = repo.GetMmcode(query, page, limit, sorters);
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
            var p2 = form.Get("p2"); //docno
            var p3 = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0228Repository repo = new AA0228Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, p2, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetSelectMmcodeDetail(string MMCODE, string MAT_CLASS, string DOCNO)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AA0228Repository repo = new AA0228Repository(DBWork);
                    session.Result.etts = repo.GetSelectMmcodeDetail(MMCODE, MAT_CLASS, DOCNO);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeDocd(FormDataCollection form)
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
                    AA0228Repository repo = new AA0228Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeDocd(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        //批號+效期+效期數量combox
        public ApiResponse GetLOT_NO(FormDataCollection form)
        {
            var FRWH = form.Get("FRWH");
            var MMCODE = form.Get("MMCODE");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0228Repository(DBWork);
                    session.Result.etts = repo.GetLOT_NO(FRWH, MMCODE);
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
        public ApiResponse CreateM(ME_DOCM me_docm)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0228Repository(DBWork);
                    string v_docno = repo.GetDailyDocno();
                    if (!repo.CheckExistsM(v_docno)) // 新增前檢查主鍵是否已存在
                    {
                        var v_matclass = me_docm.MAT_CLASS;
                        var v_doctype = "WT";
                        var v_flowid = "1";
                        var v_whno = repo.GetWhno_mm1();
                        if (v_matclass == "01")
                        {
                            v_whno = repo.GetWhno_me1();
                        }

                        var v_apptime = me_docm.APPTIME_T;
                        me_docm.DOCNO = v_docno;
                        me_docm.DOCTYPE = v_doctype;
                        me_docm.FLOWID = v_flowid;
                        me_docm.FRWH = v_whno;
                        me_docm.TOWH = v_whno;
                        me_docm.APPID = User.Identity.Name;
                        me_docm.APPTIME = v_apptime;
                        me_docm.USEID = User.Identity.Name;
                        me_docm.CREATE_USER = User.Identity.Name;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateM(me_docm);
                        session.Result.etts = repo.GetM(me_docm.DOCNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>單號</span>重複，請重新輸入。";
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
        public ApiResponse CreateD(ME_DOCEXP me_docexp)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0228Repository(DBWork);

                    bool flowIdValid = repo.CheckFlowId(me_docexp.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "單據狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }
                    if (!repo.CheckExistsMMCODE(me_docexp.MMCODE))
                    {
                        session.Result.msg = "院內碼不存在";
                        session.Result.success = false;
                        return session.Result;
                    }
                    if (repo.CheckExistsMM(me_docexp.DOCNO, me_docexp.MMCODE, me_docexp.LOT_NO, me_docexp.EXP_DATE))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;

                        session.Result.msg = "<span style='color:red'>此單明細已有重複的院內碼「" + me_docexp.MMCODE + "」、批號、效期</span>，請確認。";
                    }
                    else
                    {
                        if (!repo.CheckExistsE(me_docexp.DOCNO)) // 新增前檢查主鍵是否已存在
                        {
                            me_docexp.SEQ = "1";
                        }
                        else
                        {
                            me_docexp.SEQ = repo.GetDocESeq(me_docexp.DOCNO);
                        }

                        // 暫存前端填寫的批號效期
                        string v_LOT_NO = me_docexp.LOT_NO;
                        string v_EXP_DATE = me_docexp.EXP_DATE_T;

                        // 取得最近效期批號
                        foreach (ME_DOCEXP docexp in repo.GetLotExp(me_docexp.DOCNO, me_docexp.MMCODE))
                        {
                            me_docexp.LOT_NO = docexp.LOT_NO;
                            me_docexp.EXP_DATE = docexp.EXP_DATE;
                        }

                        var v_up = 0.0000;
                        if (me_docexp.M_CONTPRICE == null)
                        {
                            me_docexp.M_CONTPRICE = "0";
                        }
                        v_up = double.Parse(me_docexp.M_CONTPRICE);
                        var v_qty = 0.0000;
                        v_qty = double.Parse(me_docexp.APVQTY);
                        var v_amt = 0.0000;
                        v_amt = v_qty * v_up;
                        var v_apvqty = 0.0000;
                        v_apvqty = v_qty;
                        me_docexp.APVQTY = v_apvqty.ToString();
                        me_docexp.C_TYPE = "2";
                        me_docexp.C_UP = me_docexp.M_CONTPRICE;
                        me_docexp.C_AMT = v_amt.ToString();
                        me_docexp.UPDATE_USER = User.Identity.Name;
                        me_docexp.UPDATE_IP = DBWork.ProcIP;
                        // 建一筆換出(從戰備庫拿到民庫) - 預代最近效期批號
                        session.Result.afrs = repo.CreateE(me_docexp);

                        me_docexp.SEQ = repo.GetDocESeq(me_docexp.DOCNO);
                        me_docexp.LOT_NO = v_LOT_NO;
                        me_docexp.EXP_DATE = v_EXP_DATE;
                        v_amt = v_qty * v_up * -1;
                        v_apvqty = v_qty * -1;
                        me_docexp.APVQTY = v_apvqty.ToString();
                        me_docexp.C_TYPE = "1";
                        me_docexp.C_AMT = v_amt.ToString();
                        // 建一筆換入(品項放到戰備庫) - 使用者填寫批號效期
                        session.Result.afrs = repo.CreateE(me_docexp);

                        session.Result.etts = repo.GetM(me_docexp.DOCNO);
                        DBWork.Commit();
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

        // 修改
        [HttpPost]
        public ApiResponse UpdateM(ME_DOCM me_docm)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0228Repository repo = new AA0228Repository(DBWork);

                    bool flowIdValid = repo.CheckFlowId(me_docm.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "單據狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    me_docm.APPID = User.Identity.Name;
                    var v_apptime = me_docm.APPTIME_T;
                    me_docm.APPTIME = v_apptime;
                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(me_docm);
                    session.Result.etts = repo.GetM(me_docm.DOCNO);

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
        public ApiResponse UpdateD(ME_DOCEXP docexp)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0228Repository repo = new AA0228Repository(DBWork);

                    bool flowIdValid = repo.CheckFlowId(docexp.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "單據狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    var v_EXP_DATE = docexp.EXP_DATE_T;
                    //docexp.EXP_DATE = repo.Getdate(v_EXP_DATE);

                    var adjPar = 1;
                    if (docexp.INOUT == "1")
                        adjPar = -1;
                    var v_up = 0.0000;
                    v_up = double.Parse(docexp.M_CONTPRICE);
                    var v_qty = 0.0000;
                    v_qty = double.Parse(docexp.APVQTY);
                    var v_amt = 0.0000;
                    v_amt = v_qty * v_up * adjPar;
                    var v_apvqty = 0.0000;
                    v_apvqty = v_qty * adjPar;
                    if (string.IsNullOrEmpty(docexp.LOT_NO))
                        docexp.LOT_NO = docexp.LOT_NO_N;
                    docexp.EXP_DATE = docexp.EXP_DATE.PadLeft(7, '0');
                    docexp.APVQTY = v_apvqty.ToString();
                    docexp.C_TYPE = docexp.INOUT;
                    docexp.C_UP = docexp.M_CONTPRICE;
                    docexp.C_AMT = v_amt.ToString();
                    docexp.UPDATE_USER = User.Identity.Name;
                    docexp.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateE(docexp);
                    session.Result.etts = repo.GetD(docexp.DOCNO);

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

        // 刪除
        [HttpPost]
        public ApiResponse DeleteM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0228Repository repo = new AA0228Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            bool flowIdValid = repo.CheckFlowId(tmp[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "單據狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }

                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i];
                            me_docm.FLOWID = "X";
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            // repo.DeleteAllD(tmp[i]);
                            session.Result.afrs = repo.DeleteM(me_docm);
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
        public ApiResponse DeleteD(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0228Repository repo = new AA0228Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp_docno.Length; i++)
                        {
                            bool flowIdValid = repo.CheckFlowId(tmp_docno[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "單據狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }
                            session.Result.afrs = repo.DeleteD(tmp_docno[i], tmp_seq[i]);
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
        public ApiResponse Apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0228Repository repo = new AA0228Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckFlowId(tmp[i]) == false)
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = tmp[i] + "<span style='color:red'>此單據狀態非未過帳</span>不得執行過帳。";
                            }
                            else
                            {
                                if (repo.CheckExistsE(tmp[i]))
                                {
                                    ME_DOCM me_docm = new ME_DOCM();
                                    me_docm.DOCNO = tmp[i];
                                    me_docm.FLOWID = "2";
                                    me_docm.FRWH = repo.GetDocmTowh(tmp[i]);
                                    me_docm.TOWH = me_docm.FRWH;
                                    me_docm.UPDATE_USER = User.Identity.Name;
                                    me_docm.UPDATE_IP = DBWork.ProcIP;

                                    foreach (ME_DOCEXP docexp in repo.GetDocE_detail(me_docm.DOCNO))
                                    {
                                        //if (!repo.CheckExistsDetail(me_docm.DOCNO, docexp.MMCODE))
                                        //{
                                            double v_appqty = double.Parse(docexp.APVQTY);
                                            ME_DOCD me_docd = new ME_DOCD();
                                            me_docd.DOCNO = me_docm.DOCNO;
                                            if (!repo.CheckExistsD(me_docm.DOCNO))
                                            {
                                                me_docd.SEQ = "1";
                                            }
                                            else
                                            {
                                                me_docd.SEQ = repo.GetDocDSeq(me_docm.DOCNO);
                                            }
                                            me_docd.MMCODE = docexp.MMCODE;
                                            me_docd.APPQTY = v_appqty.ToString();
                                            me_docd.APLYITEM_NOTE = docexp.ITEM_NOTE;
                                            me_docd.UPDATE_USER = User.Identity.Name;
                                            me_docd.UPDATE_IP = DBWork.ProcIP;
                                            session.Result.afrs = repo.CreateD(me_docd);
                                        //}
                                    }

                                    // POST_DOC
                                    foreach (ME_DOCD docd in repo.GetDocd_detail(me_docm.DOCNO))
                                    {
                                        docd.WH_NO = me_docm.FRWH;
                                        docd.INV_QTY = repo.GetWhinvQty(docd.WH_NO, docd.MMCODE);

                                        // 處理MI_WHINV資料
                                        if (!repo.CheckExistsDetail(me_docm.DOCNO, docd.MMCODE))
                                        {
                                            repo.CreateWhinv(docd);
                                        }
                                        else
                                        {
                                            repo.UpdateWhinv(docd);
                                        }
                                        // 建立MI_WHTRNS資料
                                        repo.CreateWhtrns(docd);
                                    }
                                    // 處理MI_WEXPINV
                                    foreach (ME_DOCEXP docexp in repo.GetMeDocExp(me_docm.DOCNO))
                                    {
                                        if (!repo.CheckExistsWexpinv(me_docm.FRWH, docexp.MMCODE, docexp.LOT_NO, docexp.EXP_DATE))
                                        {
                                            repo.CreateWexpinv(me_docm.FRWH, docexp.MMCODE, docexp.LOT_NO, docexp.EXP_DATE, docexp.APVQTY, User.Identity.Name, DBWork.ProcIP);
                                        }
                                        else
                                        {
                                            repo.UpdateWexpinv(me_docm.FRWH, docexp.MMCODE, docexp.LOT_NO, docexp.EXP_DATE, docexp.APVQTY, User.Identity.Name, DBWork.ProcIP);
                                        }
                                        // 更新儲位檔
                                        repo.UpdateWloc(me_docm.FRWH, docexp.MMCODE, docexp.APVQTY, DBWork.UserInfo.UserId, DBWork.ProcIP);
                                    }
                                    // 更新ME_DOCM的FLOWID
                                    repo.UpdateDocmFlowid(me_docm.DOCNO, "2", me_docm.UPDATE_USER, me_docm.UPDATE_IP);
                                }
                                else
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<span style='color:red'>單號「" + tmp[i] + "」沒有院內碼項次</span>，請新增院內碼項次。";
                                    return session.Result;
                                }
                            }
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
    }
}