using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System;

namespace WebApp.Controllers.AB
{
    public class AB0028Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0028Repository(DBWork);
                    session.Result.etts = repo.GetAllM( User.Identity.Name, p0, p1, p2, p3,p4,p6,p7, page, limit, sorters);
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0028Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DeleteMbyNoDetail() {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    var repo = new AB0028Repository(DBWork);
                    session.Result.afrs = repo.DeleteMbyNoDetail(User.Identity.Name);

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
        public ApiResponse CreateM(ME_DOCM pME_DOCM)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                
                try
                {
                    var repo = new AB0028Repository(DBWork);
                    //檢查出入庫是否自己所管庫房
                    if (repo.CheckWh(User.Identity.Name,pME_DOCM.FRWH, pME_DOCM.TOWH) & (pME_DOCM.FRWH != pME_DOCM.TOWH)) {
                        var v_docno = repo.GetDocno();
                        pME_DOCM.DOCNO = v_docno;
                        pME_DOCM.APPID = User.Identity.Name;
                        pME_DOCM.DOCTYPE = "TR";
                        pME_DOCM.FLOWID = "0201";
                        pME_DOCM.APPDEPT = DBWork.UserInfo.Inid;
                        pME_DOCM.USEID = User.Identity.Name;
                        pME_DOCM.CREATE_USER = User.Identity.Name;
                        pME_DOCM.UPDATE_USER = User.Identity.Name;
                        pME_DOCM.UPDATE_IP = DBWork.ProcIP;
                        pME_DOCM.MAT_CLASS = "01";
                        session.Result.afrs = repo.CreateM(pME_DOCM);
                        session.Result.etts = repo.GetM(pME_DOCM.DOCNO);

                        DBWork.Commit();
                    } else {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        if (pME_DOCM.FRWH == pME_DOCM.TOWH)
                            session.Result.msg = "<span style='color:red'>調出庫房與調入庫房不可相同</span>，請重新輸入。";
                        else session.Result.msg = "<span style='color:red'>調出或調入非自己庫房</span>，請重新輸入。";
                        DBWork.Rollback();
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
        [HttpPost]
        public ApiResponse DeleteM(FormDataCollection form)
        {
            var DOCNO = form.Get("DOCNO");
            bool isOK = true;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0028Repository(DBWork);
                    //    session.Result.afrs = repo.DeleteAllD(DOCNO);
                    //    session.Result.afrs = repo.DeleteM(DOCNO);
                    //    session.Result.etts = repo.GetM(DOCNO);

                    //DBWork.Commit();
                    string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] tmp = docno.Split(',');
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        if (repo.Check0201(tmp[i]))
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>非調撥申請狀態</span>，無法修改。";
                            return session.Result;
                        }
                            session.Result.afrs = repo.DeleteAllD(tmp[i]);
                        if (session.Result.afrs == 0)
                        {
                            isOK = false;
                            break;
                        } else
                        {

                            session.Result.afrs = repo.DeleteM(tmp[i]);
                            if (session.Result.afrs == 0) {
                                isOK = false;
                                break;
                            }
                        }  
                    }
                    if (isOK) DBWork.Commit();
                    else DBWork.Rollback();
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
        public ApiResponse UpdateFLOWID0202(FormDataCollection form)
        {
            var DOCNO = form.Get("DOCNO");
            bool isOK = true;
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ME_DOCM m = new ME_DOCM();
                    string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1);
                    string[] docnoArray = docno.Split(',');
                    var repo = new AB0028Repository(DBWork);
                    m.UPDATE_USER = User.Identity.Name;
                    m.UPDATE_IP = DBWork.ProcIP;
                    m.FLOWID = "0202";
                    for (int i = 0; i < docnoArray.Length; i++)
                    {
                        m.DOCNO = docnoArray[i];

                        if (repo.Check0201(m.DOCNO))
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>非調撥申請狀態</span>，無法修改。";
                            return session.Result;
                        }

                        session.Result.afrs = repo.UpdateFLOWID(m);
                        if (session.Result.afrs == 0)
                        {
                            isOK = false;
                            break;
                        }
                        else {
                            session.Result.afrs = repo.UpdateALLApvQty(m);//提出申請時,預設核撥數量=申請數量
                            if (session.Result.afrs == 0)
                            {
                                isOK = false;
                                break;
                            }
                        }
                    }

                    if (isOK)
                        DBWork.Commit();
                    else
                        DBWork.Rollback();
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
        public ApiResponse UpdateM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0028Repository(DBWork);
                    ME_DOCM m = new ME_DOCM();
                    m.DOCNO = form.Get("DOCNO");
                    if (repo.Check0201(m.DOCNO))
                    {
                        m.FRWH = form.Get("FRWH");
                        m.TOWH = form.Get("TOWH");
                        if (repo.CheckWh(User.Identity.Name, m.FRWH, m.TOWH) & (m.FRWH != m.TOWH))
                        {
                            m.UPDATE_USER = User.Identity.Name;
                            m.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.UpdateM(m);
                            session.Result.etts = repo.GetM(m.DOCNO);

                            DBWork.Commit();
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            if (m.FRWH == m.TOWH)
                                session.Result.msg = "<span style='color:red'>調出庫房與調入庫房不可相同</span>，請重新輸入。";
                            else
                                session.Result.msg = "<span style='color:red'>調出或調入非自己庫房</span>，請重新輸入。";
                            DBWork.Rollback();
                        }
                    }
                    else {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>非調撥申請狀態</span>，無法修改。";
                        DBWork.Rollback();
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

        [HttpPost]
        public ApiResponse CreateD(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0028Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO");
                    me_docd.MMCODE = form.Get("MMCODE");
                    me_docd.APPQTY = form.Get("APPQTY");

                    

                    if (repo.Check0201(me_docd.DOCNO))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>非調撥申請狀態</span>，無法修改。";
                        return session.Result;
                    }

                    if (repo.CheckExistsMast(me_docd.DOCNO, me_docd.MMCODE)) // 檢查院內碼是否存在基本檔&可選
                    {
                        if (!repo.CheckExistsDKey(me_docd.DOCNO, me_docd.MMCODE)) // 新增前檢查主鍵是否已存在
                        {
                            me_docd.SEQ = repo.GetDocDSeq(me_docd.DOCNO);
                            me_docd.CREATE_USER = User.Identity.Name;
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.CreateD(me_docd);
                            session.Result.etts = repo.GetD(me_docd.DOCNO, me_docd.SEQ);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>院內碼</span>重複，請重新輸入。";
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼</span>不存在或不可選用，請重新輸入。";
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
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                bool isOK = true;
                try
                {
                    var repo = new AB0028Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); 
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] docnoArray = docno.Split(',');
                        string[] seqArray = seq.Split(',');
                        for (int i = 0; i < docnoArray.Length; i++) {

                            if (repo.Check0201(docnoArray[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>非調撥申請狀態</span>，無法修改。";
                                return session.Result;
                            }

                            session.Result.afrs = repo.DeleteD(docnoArray[i], seqArray[i]);
                            if (session.Result.afrs == 0)
                            {
                                isOK = false;
                                break;
                            }
                        }

                    }
                    if (isOK)
                        DBWork.Commit();
                    else
                        DBWork.Rollback();
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
        public ApiResponse UpdateD(FormDataCollection form)
        {
            var DOCNO = form.Get("DOCNO");
            var SEQ = form.Get("SEQ");
            var MMCODE = form.Get("MMCODE");
            var APPQTY = form.Get("APPQTY");
            
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0028Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = DOCNO;
                    me_docd.MMCODE = MMCODE;
                    me_docd.SEQ = SEQ;

                    if (repo.Check0201(me_docd.DOCNO))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>非調撥申請狀態</span>，無法修改。";
                        return session.Result;
                    }

                    if (repo.CheckExistsMast(me_docd.DOCNO, me_docd.MMCODE)) // 檢查院內碼是否存在基本檔&可選
                    {
                        if (!repo.CheckExistsDKeyByUpd(me_docd.DOCNO, me_docd.MMCODE, me_docd.SEQ))
                        {
                            me_docd.SEQ = SEQ;
                            me_docd.APPQTY = APPQTY;
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.UpdateD(me_docd);
                            session.Result.etts = repo.GetD(me_docd.DOCNO, me_docd.SEQ);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>院內碼</span>重複，請重新輸入。";

                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼</span>不存在或不可選用，請重新輸入。";

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
        public ApiResponse GetTowhCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0028Repository repo = new AB0028Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo(p0);
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
                    AB0028Repository repo = new AB0028Repository(DBWork);
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
        public ApiResponse GetMmcodeCombo(FormDataCollection form)
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
                    AB0028Repository repo = new AB0028Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CheckMmcodeValid(FormDataCollection form) {
            var mmcode = form.Get("mmcode");
            var wh_no = form.Get("wh_no");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    AB0028Repository repo = new AB0028Repository(DBWork);
                    session.Result.msg = repo.CheckMmcodeValid(wh_no, mmcode) ? "Y" : "N";
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }
    }
}