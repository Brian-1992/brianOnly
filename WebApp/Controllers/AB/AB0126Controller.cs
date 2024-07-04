using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using WebApp.Models.UT;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Data;
using System.Web;

namespace WebApp.Controllers.AB
{
    public class AB0126Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetColumns(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0126Repository(DBWork);

                    List<ColumnItem> columns = new List<ColumnItem>();
                    foreach (ColumnItem item in repo.GetColumnItems())
                    {
                        columns.Add(new ColumnItem { TEXT = item.TEXT, DATAINDEX = item.DATAINDEX });
                    }

                    session.Result.etts = columns;

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

        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
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
                    var repo = new AB0126Repository(DBWork);
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
        public ApiResponse GetLotNo(FormDataCollection form)
        {
            var frwh = form.Get("p0");
            var mmcode = form.Get("p1");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0126Repository(DBWork);
                    session.Result.etts = repo.GetLotNo(frwh, mmcode);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCode(FormDataCollection form)
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
                    AB0126Repository repo = new AB0126Repository(DBWork);
                    session.Result.etts = repo.GetMMCode(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTowhCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0126Repository repo = new AB0126Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFrwhCombo(FormDataCollection form)
        {
            var frwh = form.Get("p0");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0126Repository repo = new AB0126Repository(DBWork);
                    session.Result.etts = repo.GetFrwhCombo(frwh, User.Identity.Name);
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetInvQty(FormDataCollection form)
        {
            var mmcode = form.Get("MMCODE");
            var frwh = form.Get("FRWH");
            var towh = form.Get("TOWH");
            var app_qty = form.Get("APP_QTY");
            if (String.IsNullOrEmpty(app_qty))
                app_qty = "0";
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    string frwhRtnStr = "";
                    string towhRtnStr = "";
                    AB0126Repository repo = new AB0126Repository(DBWork);

                    int frwh_qty = 0;
                    if (!String.IsNullOrEmpty(frwh))
                    {
                        frwh_qty = repo.GetWhQty(mmcode, frwh);
                        int frwh_result = frwh_qty - Convert.ToInt32(app_qty);
                        frwhRtnStr = frwh_qty + " - " + app_qty + " = " + frwh_result; // 出庫庫房有選時才顯示出庫庫房的庫存量
                    }

                    int towh_qty = 0;
                    if (!String.IsNullOrEmpty(towh))
                    {
                        towh_qty = repo.GetWhQty(mmcode, towh);
                        int towh_result = towh_qty + Convert.ToInt32(app_qty);
                        towhRtnStr = towh_qty + " + " + app_qty + " = " + towh_result; // 入庫庫房有選時才顯示入庫庫房的庫存量
                    }

                    // 出庫庫存量 frwh_qty - app_qty = frwh_result
                    // 入庫庫存量 towh_qty + app_qty = towh_result
                    session.Result.etts = new List<object> { new { frwh_qty = frwh_qty, towh_qty = towh_qty } };
                    session.Result.msg = frwhRtnStr + "^" + towhRtnStr;
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }

        // 調撥
        [HttpPost]
        public ApiResponse SetQty(FormDataCollection form)
        {
            var mmcode = form.Get("mmcode");
            var towh = form.Get("towh");
            var frwh = form.Get("frwh");
            var app_qty = form.Get("app_qty");
            var wexp_id = form.Get("wexp_id");
            IEnumerable<AB0126> list = JsonConvert.DeserializeObject<IEnumerable<AB0126>>(form.Get("list"));

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0126Repository(DBWork);

                    string chkErrMsg = "";
                    if (repo.ChkMmcodeValid(mmcode) == 0)
                        chkErrMsg = "無效的院內碼" + mmcode;
                    else if (repo.CheckWhCancelByWhno(frwh))
                        chkErrMsg = "調入庫房已作廢，請重新選擇";
                    else if (repo.CheckWhCancelByWhno(towh))
                        chkErrMsg = "調出庫房已作廢，請重新選擇";
                    else if (frwh == towh)
                        chkErrMsg = "出庫庫房與入庫庫房不可相同";
                    //else if (repo.CheckFrwhMmcodeValid(frwh, mmcode) == false)
                    //    chkErrMsg = "院內碼不存在於調出庫房，請檢查庫房是否正確";

                    if (chkErrMsg != "")
                    {
                        session.Result.success = false;
                        session.Result.msg = chkErrMsg;
                        DBWork.Rollback();
                        return session.Result;
                    }

                    // 基本檢查都通過後才繼續流程
                    // Step1: 調撥申請
                    // 建立申請單
                    string v_docno = repo.GetDailyDocno();
                    ME_DOCM pME_DOCM = new ME_DOCM();
                    pME_DOCM.DOCNO = v_docno;
                    pME_DOCM.APPID = User.Identity.Name;
                    pME_DOCM.FRWH = frwh;
                    pME_DOCM.TOWH = towh;
                    pME_DOCM.FLOWID = "0201";
                    pME_DOCM.DOCTYPE = "TR";
                    pME_DOCM.USEID = User.Identity.Name;
                    pME_DOCM.CREATE_USER = User.Identity.Name;
                    pME_DOCM.UPDATE_USER = User.Identity.Name;
                    pME_DOCM.UPDATE_IP = DBWork.ProcIP;
                    pME_DOCM.MAT_CLASS = "01";
                    session.Result.afrs = repo.CreateM(pME_DOCM);

                    // 建立明細
                    ME_DOCD pME_DOCD = new ME_DOCD();
                    pME_DOCD.DOCNO = v_docno;
                    pME_DOCD.SEQ = repo.GetDocDSeq(v_docno);
                    pME_DOCD.MMCODE = mmcode;
                    pME_DOCD.APPQTY = app_qty;
                    pME_DOCD.CREATE_USER = User.Identity.Name;
                    pME_DOCD.UPDATE_USER = User.Identity.Name;
                    pME_DOCD.UPDATE_IP = DBWork.ProcIP;
                    repo.CreateD(pME_DOCD);

                    // 提出申請
                    pME_DOCM.FLOWID = "0202";
                    repo.UpdateFLOWID(pME_DOCM);
                    repo.UpdateALLApvQty(pME_DOCM); //提出申請時,預設核撥數量=申請數量

                    // Step2: 執行調撥
                    // 執行調撥
                    pME_DOCM.FLOWID = "0203"; // 未實際使用

                    // 批號效期
                    //// POST_DOCEXP會逐項比對ME_DOCEXP的數量是否與ME_DOCD相符,
                    //// 又本功能一單只有一MMCODE,且ME_DOCD的MMCODE不可重複,故批號效期只能有一筆有APP_QTY的資料
                    // ->ME_DOCD現在可以有相同MMCODE,不需再限制只能有一筆
                    // 目前轉檔程式不會寫WEXP_ID,故移除WEXP_ID相關限制
                    short expSeq = 1;
                    int itemCnt = 0;
                    string strYYY = "", strMM = "", strDD = "";
                    foreach (AB0126 item in list)
                    {
                        UT_DOCD ut_docd = new UT_DOCD();
                        ut_docd.DOCNO = v_docno;
                        ut_docd.SEQ = expSeq;
                        ut_docd.MMCODE = mmcode;
                        ut_docd.LOT_NO = item.LOT_NO;
                        
                        if (item.EXP_DATE.Length == 7)
                        {
                             strYYY = item.EXP_DATE.Substring(0, 3);
                             strMM = item.EXP_DATE.Substring(3, 2);
                             strDD = item.EXP_DATE.Substring(5, 2);
                        }
                        else {
                             strYYY = item.EXP_DATE.Substring(0, 2);
                             strMM = item.EXP_DATE.Substring(2, 2);
                             strDD = item.EXP_DATE.Substring(4, 2);
                        }
                        string strYYYY = (Convert.ToInt32(strYYY) + 1911).ToString();
                        ut_docd.EXP_DATE = DateTime.ParseExact(strYYYY + "/" + strMM + "/" + strDD, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
                        ut_docd.APVQTY = item.APP_QTY;
                        ut_docd.UPDATE_USER = User.Identity.Name;
                        ut_docd.UPDATE_IP = DBWork.ProcIP;

                        // 檢查調出庫房是否存在MI_WEXPINV
                        if (repo.CheckWexpinvExists(frwh, mmcode, item.LOT_NO, item.EXP_DATE) == false)
                        {
                            // 不存在，新增
                            session.Result.afrs = repo.InsertWexpinv(frwh, mmcode, item.EXP_DATE, item.LOT_NO, item.APP_QTY,
                                                                    DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }

                        repo.CreateDocexp(ut_docd);
                        expSeq++;
                        itemCnt++;
                    }
                    //// 前端已經有限制只能維護一筆批號效期資料,這裡再檢查一次不允許多筆
                    //if (itemCnt > 1)
                    //{
                    //    session.Result.afrs = 0;
                    //    session.Result.success = false;
                    //    session.Result.msg = "一次只能調撥一筆批號效期資料";
                    //    DBWork.Rollback();
                    //    return session.Result;
                    //}

                    pME_DOCD.APVID = User.Identity.Name;
                    repo.UpdateALLApvid(pME_DOCD);
                    SP_MODEL sp = repo.POST_DOC(pME_DOCM.DOCNO, User.Identity.Name, DBWork.ProcIP);
                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "執行調撥 " + sp.O_ERRMSG;
                        DBWork.Rollback();
                        return session.Result;
                    }

                    // Step3: 確認調撥
                    // 確認入庫
                    pME_DOCM.FLOWID = "0299"; // 未實際使用
                    pME_DOCD.ACKID = User.Identity.Name;
                    repo.UpdateALLAck(pME_DOCD);
                    repo.UpdateFstackDate(pME_DOCM.TOWH, pME_DOCM.DOCNO);

                    sp = repo.POST_DOC(pME_DOCM.DOCNO, User.Identity.Name, DBWork.ProcIP);
                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "確認調撥 " + sp.O_ERRMSG;
                        DBWork.Rollback();
                        return session.Result;
                    }

                    foreach (AB0126 item in list)
                    {
                        // 出庫庫房-扣除
                        repo.UpdateWloc(frwh, mmcode, item.APP_QTY, "-", item.UPDATE_USER, item.UPDATE_IP);
                        // 檢查調出庫房是否存在MI_WEXPINV
                        if (repo.CheckWexpinvExists(frwh, mmcode, item.LOT_NO, item.EXP_DATE) == false)
                        {
                            // 不存在，新增一筆數量為0(扣除本次調撥量之後的值)
                            repo.InsertWexpinv(frwh, mmcode, item.EXP_DATE, item.LOT_NO, "0",
                                    DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }
                        else
                        {
                            repo.UpdateWexpinv(frwh, mmcode, item.EXP_DATE, item.LOT_NO, item.APP_QTY, "-", DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }

                        // 入庫庫房-增加
                        // 若MI_WLOCINV無對應資料則新增, 有則更新
                        if (repo.CheckWlocinvExists(towh, mmcode) == false)
                            repo.InsertWloc(towh, mmcode, item.APP_QTY, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        else
                            repo.UpdateWloc(towh, mmcode, item.APP_QTY, "+", DBWork.UserInfo.UserId, DBWork.ProcIP);

                        // 若MI_WEXPINV無對應資料則新增, 有則更新
                        if (repo.CheckWexpinvExists(towh, mmcode, item.LOT_NO, item.EXP_DATE) == false)
                            repo.InsertWexpinv(towh, mmcode, item.EXP_DATE, item.LOT_NO, item.APP_QTY, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        else
                            repo.UpdateWexpinv(towh, mmcode, item.EXP_DATE, item.LOT_NO, item.APP_QTY, "+", DBWork.UserInfo.UserId, DBWork.ProcIP);
                    }

                    session.Result.msg = v_docno;

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

        // 批次調撥
        [HttpPost]
        public ApiResponse SetQtyBatch(FormDataCollection form)
        {
            var towh = form.Get("towh");
            var frwh = form.Get("frwh");
            IEnumerable<ME_DOCD> list = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(form.Get("list"));

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0126Repository(DBWork);

                    string chkErrMsg = "";
                    if (repo.CheckWhCancelByWhno(frwh))
                        chkErrMsg = "調入庫房已作廢，請重新選擇";
                    else if (repo.CheckWhCancelByWhno(towh))
                        chkErrMsg = "調出庫房已作廢，請重新選擇";
                    else if (frwh == towh)
                        chkErrMsg = "出庫庫房與入庫庫房不可相同";

                    if (chkErrMsg != "")
                    {
                        session.Result.success = false;
                        session.Result.msg = chkErrMsg;
                        DBWork.Rollback();
                        return session.Result;
                    }

                    // 基本檢查都通過後才繼續流程
                    // Step1: 調撥申請
                    // 建立申請單
                    string v_docno = repo.GetDailyDocno();
                    ME_DOCM pME_DOCM = new ME_DOCM();
                    pME_DOCM.DOCNO = v_docno;
                    pME_DOCM.APPID = User.Identity.Name;
                    pME_DOCM.FRWH = frwh;
                    pME_DOCM.TOWH = towh;
                    pME_DOCM.FLOWID = "0201";
                    pME_DOCM.DOCTYPE = "TR";
                    pME_DOCM.USEID = User.Identity.Name;
                    pME_DOCM.CREATE_USER = User.Identity.Name;
                    pME_DOCM.UPDATE_USER = User.Identity.Name;
                    pME_DOCM.UPDATE_IP = DBWork.ProcIP;
                    pME_DOCM.MAT_CLASS = "01";
                    session.Result.afrs = repo.CreateM(pME_DOCM);

                    // 建立明細
                    foreach (ME_DOCD item in list)
                    {
                        if (repo.ChkMmcodeValid(item.MMCODE) == 0)
                        {
                            session.Result.success = false;
                            session.Result.msg = "無效的院內碼" + item.MMCODE;
                            DBWork.Rollback();
                            return session.Result;
                        }
                        else
                        {
                            ME_DOCD pME_DOCD = new ME_DOCD();
                            pME_DOCD.DOCNO = v_docno;
                            pME_DOCD.SEQ = repo.GetDocDSeq(v_docno);
                            pME_DOCD.MMCODE = item.MMCODE;
                            pME_DOCD.APPQTY = item.APPQTY;
                            pME_DOCD.CREATE_USER = User.Identity.Name;
                            pME_DOCD.UPDATE_USER = User.Identity.Name;
                            pME_DOCD.UPDATE_IP = DBWork.ProcIP;
                            repo.CreateD(pME_DOCD);
                        }
                    }

                    // 提出申請
                    pME_DOCM.FLOWID = "0202";
                    repo.UpdateFLOWID(pME_DOCM);
                    repo.UpdateALLApvQty(pME_DOCM); //提出申請時,預設核撥數量=申請數量

                    // Step2: 執行調撥
                    // 執行調撥
                    pME_DOCM.FLOWID = "0203"; // 未實際使用

                    // 批號效期
                    short expSeq = 1;
                    foreach (ME_DOCD item in list)
                    {
                        if (item.LOT_NO != "" && item.EXPDATE != "")
                        {
                            UT_DOCD ut_docd = new UT_DOCD();
                            ut_docd.DOCNO = v_docno;
                            ut_docd.SEQ = expSeq;
                            ut_docd.MMCODE = item.MMCODE;
                            ut_docd.LOT_NO = item.LOT_NO;
                            string strYYY = item.EXPDATE.Substring(0, 3);
                            string strMM = item.EXPDATE.Substring(3, 2);
                            string strDD = item.EXPDATE.Substring(5, 2);
                            string strYYYY = (Convert.ToInt32(strYYY) + 1911).ToString();
                            ut_docd.EXP_DATE = DateTime.ParseExact(strYYYY + "/" + strMM + "/" + strDD, "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
                            ut_docd.APVQTY = item.APPQTY;
                            ut_docd.UPDATE_USER = User.Identity.Name;
                            ut_docd.UPDATE_IP = DBWork.ProcIP;

                            // 檢查調出庫房是否存在MI_WEXPINV
                            if (repo.CheckWexpinvExists(frwh, item.MMCODE, item.LOT_NO, item.EXPDATE) == false)
                            {
                                // 不存在，新增
                                session.Result.afrs = repo.InsertWexpinv(frwh, item.MMCODE, item.EXPDATE, item.LOT_NO, item.APPQTY,
                                                                        DBWork.UserInfo.UserId, DBWork.ProcIP);
                            }

                            repo.CreateDocexp(ut_docd);
                        }
                        expSeq++;
                    }

                    ME_DOCD tME_DOCD = new ME_DOCD();
                    tME_DOCD.DOCNO = v_docno;
                    tME_DOCD.CREATE_USER = User.Identity.Name;
                    tME_DOCD.UPDATE_USER = User.Identity.Name;
                    tME_DOCD.UPDATE_IP = DBWork.ProcIP;
                    tME_DOCD.APVID = User.Identity.Name;
                    repo.UpdateALLApvid(tME_DOCD);
                    SP_MODEL sp = repo.POST_DOC(pME_DOCM.DOCNO, User.Identity.Name, DBWork.ProcIP);
                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "執行調撥 " + sp.O_ERRMSG;
                        DBWork.Rollback();
                        return session.Result;
                    }

                    // Step3: 確認調撥
                    // 確認入庫
                    pME_DOCM.FLOWID = "0299"; // 未實際使用
                    tME_DOCD.ACKID = User.Identity.Name;
                    repo.UpdateALLAck(tME_DOCD);
                    repo.UpdateFstackDate(pME_DOCM.TOWH, pME_DOCM.DOCNO);

                    sp = repo.POST_DOC(pME_DOCM.DOCNO, User.Identity.Name, DBWork.ProcIP);
                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "確認調撥 " + sp.O_ERRMSG;
                        DBWork.Rollback();
                        return session.Result;
                    }

                    foreach (ME_DOCD item in list)
                    {
                        // 出庫庫房-扣除
                        repo.UpdateWloc(frwh, item.MMCODE, item.APPQTY, "-", item.UPDATE_USER, item.UPDATE_IP);
                        if (item.LOT_NO != "" && item.EXPDATE != "")
                        {
                            // 檢查調出庫房是否存在MI_WEXPINV
                            if (repo.CheckWexpinvExists(frwh, item.MMCODE, item.LOT_NO, item.EXPDATE) == false)
                            {
                                // 不存在，新增一筆數量為0(扣除本次調撥量之後的值)
                                repo.InsertWexpinv(frwh, item.MMCODE, item.EXPDATE, item.LOT_NO, "0",
                                        DBWork.UserInfo.UserId, DBWork.ProcIP);
                            }
                            else
                            {
                                repo.UpdateWexpinv(frwh, item.MMCODE, item.EXPDATE, item.LOT_NO, item.APPQTY, "-", DBWork.UserInfo.UserId, DBWork.ProcIP);
                            }
                        }

                        // 入庫庫房-增加
                        // 若MI_WLOCINV無對應資料則新增, 有則更新
                        if (repo.CheckWlocinvExists(towh, item.MMCODE) == false)
                            repo.InsertWloc(towh, item.MMCODE, item.APPQTY, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        else
                            repo.UpdateWloc(towh, item.MMCODE, item.APPQTY, "+", DBWork.UserInfo.UserId, DBWork.ProcIP);

                        if (item.LOT_NO != "" && item.EXPDATE != "")
                        {
                            // 若MI_WEXPINV無對應資料則新增, 有則更新
                            if (repo.CheckWexpinvExists(towh, item.MMCODE, item.LOT_NO, item.EXPDATE) == false)
                                repo.InsertWexpinv(towh, item.MMCODE, item.EXPDATE, item.LOT_NO, item.APPQTY, DBWork.UserInfo.UserId, DBWork.ProcIP);
                            else
                                repo.UpdateWexpinv(towh, item.MMCODE, item.EXPDATE, item.LOT_NO, item.APPQTY, "+", DBWork.UserInfo.UserId, DBWork.ProcIP);
                        }
                    }

                    session.Result.msg = v_docno;

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

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0126Repository repo = new AB0126Repository(DBWork);
                    JCLib.Excel.Export("AB0126.xls", repo.GetExcel());
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
            using (WorkSession session = new WorkSession(this))
            {

                List<ME_DOCD> list = new List<ME_DOCD>();
                UnitOfWork DBWork = session.UnitOfWork;
                var HttpPostedFile = HttpContext.Current.Request.Files["file"];
                var v_mat_class = HttpContext.Current.Request.Form["matclass"];
                var towh = HttpContext.Current.Request.Form["towh"];
                var frwh = HttpContext.Current.Request.Form["frwh"];
                IWorkbook workBook;

                try
                {
                    AB0126Repository repo = new AB0126Repository(DBWork);
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
                    IRow headerRow = sheet.GetRow(0); //由第一列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum; //欄位數目
                    int i, j;


                    bool isValid = true;
                    string[] arr = { "院內碼", "調撥量", "批號", "效期(YYYMMDD)" };

                    for (j = 0; j < cellCount; j++)
                    {
                        isValid = headerRow.GetCell(j) == null ? false : true;
                        if (!isValid)
                        {
                            session.Result.msg = "檔案格式不同，請下載範本來更新。";
                            break;
                        }
                    }

                    //檢查檔案中欄位名稱是否符合
                    if (isValid)
                    {
                        for (i = 0; i < cellCount; i++)
                        {
                            isValid = headerRow.GetCell(i).ToString() == arr[i] ? true : false;
                            if (!isValid)
                            {
                                break;
                            }
                        }
                    }

                    if (!isValid)
                    {
                        session.Result.msg = "檔案格式不同，請下載範本來更新。";
                    }
                    #endregion


                    DataTable dtTable = new DataTable();
                    DataRow datarow = dtTable.NewRow();
                    string arrCheckResult = "";
                    int nullnum = 0; //判斷是否整列為空

                    if (isValid)
                    {
                        #region 建立DataTable
                        for (i = 0; i < cellCount; i++)
                        //以欄位文字為名新增欄位，此處全視為字串型別以求簡化
                        {
                            dtTable.Columns.Add(
                                  new DataColumn(headerRow.GetCell(i).StringCellValue));
                        }
                        dtTable.Columns.Add("檢核結果");

                        //略過第0列(標題列)，一直處理至最後一列
                        for (i = 1; i <= sheet.LastRowNum; i++)
                        {
                            IRow row = sheet.GetRow(i);
                            datarow = dtTable.NewRow();
                            arrCheckResult = "OK";
                            nullnum = 0;
                            //依先前取得的欄位數逐一設定欄位內容
                            for (j = 0; j < cellCount; j++)
                            {
                                if (row == null)
                                {
                                    nullnum = cellCount;
                                    break;
                                }
                                datarow[j] = row.GetCell(j) == null ? "" : row.GetCell(j).ToString();
                            }
                            if (nullnum != cellCount)
                            {
                                datarow[cellCount] = arrCheckResult;
                                dtTable.Rows.Add(datarow);
                            }
                        }

                        dtTable.DefaultView.Sort = "院內碼";
                        dtTable = dtTable.DefaultView.ToTable();
                        #endregion

                        DataTable newTable = dtTable.Clone();
                        newTable = dtTable;

                        //加入至ME_DOCD中
                        #region 加入至ME_DOCD中
                        for (i = 0; i < newTable.Rows.Count; i++)
                        {
                            ME_DOCD me_docd = new ME_DOCD();

                            me_docd.MMCODE = newTable.Rows[i]["院內碼"].ToString().Trim();
                            me_docd.APPQTY = newTable.Rows[i]["調撥量"].ToString().Trim();
                            me_docd.LOT_NO = newTable.Rows[i]["批號"].ToString().Trim();
                            me_docd.EXPDATE = newTable.Rows[i]["效期(YYYMMDD)"].ToString().Trim();
                            me_docd.CHECK_RESULT = "OK";

                            //資料是否被使用者填入更新值
                            bool dataUpdated = false;

                            //如果有任何一格不是空的
                            if (
                                me_docd.MMCODE != "" ||
                                me_docd.APPQTY != "" ||
                                me_docd.APPQTY != "0"
                                )
                            {
                                //表示資料有被更新
                                dataUpdated = true;
                            }

                            //若庫房代碼不是空的且資料有更新過
                            if (newTable.Rows[i]["院內碼"].ToString() != "" && dataUpdated == true)
                            {
                                //檢核庫房代碼
                                if (repo.CheckExistsMMCODE(me_docd.MMCODE) != true)
                                {
                                    me_docd.CHECK_RESULT = "此院內碼不存在";
                                }
                                else if (repo.CheckMatClassMMCODE("01", me_docd.MMCODE) != true)
                                {
                                    me_docd.CHECK_RESULT = "此院內碼物料分類非藥品";
                                }
                                else if (list.Exists(x => x.MMCODE == me_docd.MMCODE))
                                {
                                    me_docd.CHECK_RESULT = "匯入明細已有重複的院內碼";
                                }
                                else
                                {
                                    int appqty = 0;
                                    try
                                    {
                                        appqty = Convert.ToInt32(newTable.Rows[i]["調撥量"].ToString().Trim());
                                    }
                                    catch (Exception ex)
                                    {
                                        me_docd.CHECK_RESULT = "調撥量不是正確的數字";
                                    }
                                    // 顯示調入和調出庫房,現存量分別加減調撥量的結果
                                    int towh_invqty = repo.GetWhQty(me_docd.MMCODE, towh);
                                    int frwh_invqty = repo.GetWhQty(me_docd.MMCODE, frwh);
                                    me_docd.A_INV_QTY = towh_invqty + "+" + appqty + "=" + (towh_invqty + appqty);
                                    me_docd.B_INV_QTY = frwh_invqty + "-" + appqty + "=" + (frwh_invqty - appqty);

                                    // 批號效期檢查
                                    if (me_docd.LOT_NO != "" || me_docd.EXPDATE != "")
                                    {
                                        if (me_docd.LOT_NO == "" || me_docd.EXPDATE == "") // 批號效期任一欄有填時,則兩欄都需填寫
                                            me_docd.CHECK_RESULT = "批號及效期未填寫完整";
                                        else if (repo.CheckTwndate(me_docd.EXPDATE) != true)
                                            me_docd.CHECK_RESULT = "效期格式不正確";
                                    }

                                    if (me_docd.CHECK_RESULT == "OK")
                                    {
                                        me_docd.CHECK_RESULT = "";

                                        //刪除最後的逗點
                                        if (me_docd.CHECK_RESULT == "")
                                        {
                                            me_docd.CHECK_RESULT = "OK";
                                        }
                                        else
                                        {
                                            me_docd.CHECK_RESULT = me_docd.CHECK_RESULT.Substring(0, me_docd.CHECK_RESULT.Length - 2);
                                        }
                                    };

                                }
                                if (me_docd.CHECK_RESULT != "OK")
                                {
                                    checkPassed = false;
                                }
                                //產生一筆資料
                                list.Add(me_docd);
                            }
                        }
                        #endregion
                    }

                    if (!isValid)
                    {
                        session.Result.success = false;
                    }
                    else
                    {
                        session.Result.etts = list;
                        session.Result.msg = checkPassed.ToString();
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
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
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
                    AB0126Repository repo = new AB0126Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

    }
}