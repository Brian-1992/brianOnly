using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace WebApp.Controllers.AB
{
    public class AB0098Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0098Repository(DBWork);
                    AB0098Repository.ME_DOCM_QUERY_PARAMS query = new AB0098Repository.ME_DOCM_QUERY_PARAMS();
                    query.FRWH = form.Get("p1") == null ? "" : form.Get("p1");
                    query.FLOWID = form.Get("p2") == null ? "" : form.Get("p2");
                    query.DOCTYPE = "AJ";
                    query.APPTIME_S = "";
                    query.APPTIME_E = "";
                    query.USERID = DBWork.UserInfo.UserId;
                    query.USERNAME = DBWork.UserInfo.UserName;

                    if (form.Get("d0") != null && form.Get("d0") != "")
                        query.APPTIME_S = form.Get("d0").Split('T')[0];  // yyyy-mm-ddT00:00:00
                    if (form.Get("d1") != null && form.Get("d1") != "")
                        query.APPTIME_E = form.Get("d1").Split('T')[0];

                    session.Result.etts = repo.GetAll(query);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢
        [HttpPost]
        public ApiResponse AllMeDocd(FormDataCollection form)
        {
            //var page = int.Parse(form.Get("page"));
            //var start = int.Parse(form.Get("start"));
            //var limit = int.Parse(form.Get("limit"));
            //var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0098Repository(DBWork);
                    AB0098Repository.ME_DOCD_QUERY_PARAMS query = new AB0098Repository.ME_DOCD_QUERY_PARAMS();
                    query.DOCNO = form.Get("p0");
                    //session.Result.etts = repo.GetAllMeDocd(query, page, limit, sorters);
                    session.Result.etts = repo.GetAllMeDocd(query);
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
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);
                    session.Result.etts = repo.GetFrwhCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFlowidCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);
                    session.Result.etts = repo.GetFlowidCombo("AJ");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTranskindCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);
                    session.Result.etts = repo.GetTranskindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterCreate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    ME_DOCM me_docm = new ME_DOCM();
                    string hospCode = repo.GetHospCode();
                    if (hospCode == "0")
                    {
                        me_docm.DOCNO = repo1.GetDocno();
                    }
                    else {
                        me_docm.DOCNO = repo.GetDailyDocno();
                    }
                    
                    if (!repo1.CheckExists(me_docm.DOCNO))
                    {
                        me_docm.CREATE_USER = User.Identity.Name;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        me_docm.APPID = User.Identity.Name;
                        me_docm.APPDEPT = DBWork.UserInfo.Inid;
                        me_docm.USEID = User.Identity.Name;
                        me_docm.FRWH = form.Get("FRWH");        // 核撥庫房
                        me_docm.TOWH = form.Get("FRWH");    // 讓postdoc可以正常執行
                        me_docm.DOCTYPE = "AJ";
                        me_docm.FLOWID = "1702";
                        me_docm.MAT_CLASS = "01";
                            
                        session.Result.afrs = repo.MasterCreate(me_docm);
                        session.Result.etts = repo.MasterGet(me_docm.DOCNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>異動單號</span>重複，請重新嘗試。";
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
        public ApiResponse MasterUpdate(ME_DOCM me_docm)
        {
            using (
                WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId02(me_docm.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;

                    if (!repo.CheckMeDocdExists(me_docm.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                        session.Result.afrs = repo.MasterUpdate(me_docm);
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>異動單號「" + me_docm.DOCNO + "」已存在" + me_docm.FRWH + "庫房院內碼項次，所以無法修改調帳庫別</span><br>如欲修改調帳庫別，請先刪除所有項次。";
                        return session.Result;
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
        public ApiResponse MasterDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            bool flowIdValid = repo.ChceckFlowId02(tmp[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }

                            repo.DetailAllDelete(tmp[i]);
                            session.Result.afrs = repo.MasterDelete(tmp[i]);
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
        public ApiResponse DetailCreate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO2");

                    bool flowIdValid = repo.ChceckFlowId02(me_docd.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    if (!repo.CheckMeDocdExists(me_docd.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                        me_docd.SEQ = "1";
                    else
                        me_docd.SEQ = repo1.GetMaxSeq(me_docd.DOCNO);
                    me_docd.MMCODE = form.Get("MMCODE");
                    me_docd.APVQTY = form.Get("APVQTY");
                    me_docd.APLYITEM_NOTE = form.Get("APLYITEM_NOTE");
                    me_docd.TRANSKIND = form.Get("TRANSKIND");
                    me_docd.UP = form.Get("UP");
                    me_docd.AMT = form.Get("AMT");

                    string frwh = repo.GetFrwh(me_docd.DOCNO);
                    me_docd.INV_QTY = repo1.Get_INV_QTY(frwh, me_docd.MMCODE);    // 庫存量

                    if (repo1.CheckWhmmExists(form.Get("FRWH2").Split(' ')[0], me_docd.MMCODE))
                    {
                        if (!repo1.CheckMeDocdExists_1(me_docd))
                        {
                            me_docd.CREATE_USER = User.Identity.Name;
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.DetailCreate(me_docd);
                            //session.Result.etts = repo.DetailGet(me_docd);
                            AB0098Repository.ME_DOCD_QUERY_PARAMS query = new AB0098Repository.ME_DOCD_QUERY_PARAMS();
                            query.DOCNO = me_docd.DOCNO;
                            query.SEQ = me_docd.SEQ;
                            session.Result.etts = repo.GetAllMeDocd(query);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>此申請單已存在此院內碼</span>，請重新輸入院內碼。";
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房不存放此院內碼</span>，請重新輸入院內碼。";
                    }

                    DBWork.Commit();
                }
                catch (Exception ex)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailUpdate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO2");

                    bool flowIdValid = repo.ChceckFlowId02(me_docd.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    me_docd.SEQ = form.Get("SEQ");
                    me_docd.MMCODE = form.Get("MMCODE");
                    me_docd.APVQTY = form.Get("APVQTY");
                    me_docd.APLYITEM_NOTE = form.Get("APLYITEM_NOTE");
                    me_docd.TRANSKIND = form.Get("TRANSKIND");
                    me_docd.UP = form.Get("UP");
                    me_docd.AMT = form.Get("AMT");

                    string frwh = repo.GetFrwh(me_docd.DOCNO);
                    me_docd.INV_QTY = repo.Get_INV_QTY(frwh, me_docd.MMCODE);    // 庫存量碼

                    if (repo1.CheckWhmmExists(form.Get("FRWH2").Split(' ')[0], me_docd.MMCODE))
                    {
                        if (!repo1.CheckMeDocdExists_1(me_docd))
                        {
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.DetailUpdate(me_docd);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>此申請單已存在此院內碼</span>，請重新輸入院內碼。";
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房不存放此院內碼</span>，請重新輸入院內碼。";
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
        public ApiResponse DetailDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp_docno.Length; i++) {
                            bool flowIdValid = repo.ChceckFlowId02(tmp_docno[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }
                            session.Result.afrs = repo.DetailDelete(tmp_docno[i], tmp_seq[i]);
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
        public ApiResponse UpdateStatusBySP(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');

                        //檢查申請單明細與是否有填備註
                        string no_detail_docnos = string.Empty;
                        string no_note_docnos = string.Empty;
                        string qty_0_docnos = string.Empty;
                        string check_result = string.Empty;
                        for (int i = 0; i < tmp.Length; i++) {

                            bool flowIdValid = repo.ChceckFlowId02(tmp[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }

                            // 是否有明細
                            if (repo.HasDetail(tmp[i]) == false)
                            {
                                if (no_detail_docnos != string.Empty) {
                                    no_detail_docnos += "、";
                                }
                                no_detail_docnos += tmp[i];
                            }
                            // 是否有備註未填
                            if (repo.HasNoteEmpty(tmp[i]))
                            {
                                if (no_note_docnos != string.Empty)
                                {
                                    no_note_docnos += "、";
                                }
                                no_note_docnos += tmp[i];
                            }
                            // 是否有調整量為0
                            if (repo.HasZero(tmp[i]))
                            {
                                if (qty_0_docnos != string.Empty)
                                {
                                    qty_0_docnos += "、";
                                }
                                qty_0_docnos += tmp[i];
                            }
                        }
                        if (no_detail_docnos != string.Empty) {
                            check_result += string.Format("<span style='color:red'>無明細</span>申請單號如下，請先新增明細：<br>{0}<br><br>", no_detail_docnos);
                        }
                        if (no_note_docnos != string.Empty)
                        {
                            check_result += string.Format("<span style='color:red'>備註未填寫</span>申請單號如下，請填寫備註：<br>{0}<br><br>", no_note_docnos);
                        }
                        if (qty_0_docnos != string.Empty)
                        {
                            check_result += string.Format("<span style='color:red'>調整量為0</span>申請單號如下，請修改調整量：<br>{0}<br><br>", qty_0_docnos);
                        }
                        if (check_result != string.Empty) {
                            session.Result.msg = check_result;
                            session.Result.success = false;
                            return session.Result;
                        }

                        // 檢核通過才可
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            SP_MODEL sp = repo.PostDoc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                            if (sp.O_RETID == "N")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = sp.O_ERRMSG;
                                return session.Result;
                            }

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
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0") == null ? "" : form.Get("p0").ToUpper();
            var wh_no = form.Get("WH_NO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, wh_no, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetHospCode()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0098Repository repo = new AB0098Repository(DBWork);
                    session.Result.msg = repo.GetHospCode();
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
