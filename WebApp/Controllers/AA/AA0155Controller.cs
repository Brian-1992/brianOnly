using System;
using System.Collections.Generic;
using System.Linq;
using JCLib.DB;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AA
{
    public class AA0155Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    AA0155M query = new AA0155M();

                    query.DOCNO = form.Get("p1") == null ? "" : form.Get("p1").Trim();
                    query.MAT_CLASS = form.Get("p3") == null ? "" : form.Get("p3").ToUpper().Trim();
                    query.FRWH = form.Get("p4") == null || form.Get("p4").Trim() == "" ? "" : form.Get("p4").ToUpper();
                    query.FLOWID = form.Get("p6") == null ? "" : form.Get("p6").Trim();
                    query.APPTIME_S = form.Get("d0");
                    query.APPTIME_E = form.Get("d1");

                    session.Result.etts = repo.GetAllM(User.Identity.Name, query, page, limit, sorters);
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
        public ApiResponse AllD(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0155Repository(DBWork);
                    AA0155D query = new AA0155D();
                    query.DOCNO = form.Get("p0");
                    session.Result.etts = repo.GetAllD(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWhId(FormDataCollection form)
        {
            string rtn = "";
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0155Repository(DBWork);
                    session.Result.etts = repo.GetWhId(User.Identity.Name);
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
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    AA0155D query = new AA0155D();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO");
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
        public ApiResponse GetMclassCombo(FormDataCollection form)
        {
            var is_query = form.Get("IS_QUERY");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    session.Result.etts = repo.GetMclassCombo(User.Identity.Name, is_query);
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
            string wh_kind = form.Get("WH_KIND");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    session.Result.etts = repo.GetFrwhCombo(wh_kind, User.Identity.Name);
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
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    session.Result.etts = repo.GetFlowidCombo();
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
            var p0 = form.Get("p0").ToUpper();
            var docno = form.Get("DOCNO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, docno, page, limit, "");
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
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    AB0127Repository repo1 = new AB0127Repository(DBWork);
                    ME_DOCM me_docm = new ME_DOCM();
                    me_docm.DOCNO = repo1.GetDailyDocno();
                    if (!repo1.CheckExists(me_docm.DOCNO))
                    {
                        me_docm.CREATE_USER = User.Identity.Name;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        me_docm.APPID = User.Identity.Name;
                        me_docm.USEID = User.Identity.Name;
                        me_docm.TOWH = form.Get("TOWH").Split(' ')[0];
                        me_docm.FRWH = form.Get("FRWH").Split(' ')[0];
                        me_docm.MAT_CLASS = form.Get("MAT_CLASS");
                        if (me_docm.MAT_CLASS == "01")
                        {
                            me_docm.DOCTYPE = "XR3";
                            me_docm.FLOWID = "1901";
                        }
                        else
                        {
                            me_docm.DOCTYPE = "XR2";
                            me_docm.FLOWID = "1801";
                        }

                        session.Result.afrs = repo.MasterCreate(me_docm);
                        session.Result.etts = repo.MasterGet(me_docm.DOCNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>單據號碼</span>重複，請重新嘗試。";
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
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(me_docm.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    me_docm.TOWH = me_docm.TOWH.Split(' ')[0];
                    me_docm.FRWH = me_docm.FRWH.Split(' ')[0];
                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;

                    string frwh = repo1.GetFrwh(me_docm.DOCNO);
                    if (frwh == me_docm.FRWH.Trim()) // 如果出庫庫房一樣,則可以直接更新
                        session.Result.afrs = repo.MasterUpdate(me_docm);
                    else
                    {
                        if (!repo1.CheckMeDocdExists(me_docm.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                            session.Result.afrs = repo.MasterUpdate(me_docm);
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>異動單號「" + me_docm.DOCNO + "」已存在" + frwh + "出庫庫房院內碼項次，所以無法修改出庫庫房</span><br>如欲修改出庫庫房，請先刪除所有項次。";
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

        [HttpPost]
        public ApiResponse MasterDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0155Repository repo = new AA0155Repository(DBWork);

                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            bool flowIdValid = repo.ChceckFlowId01(tmp[i]);
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
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO");

                    bool flowIdValid = repo.ChceckFlowId01(me_docd.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    if (!repo1.CheckMeDocdExists(me_docd.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                        me_docd.SEQ = "1";
                    else
                        me_docd.SEQ = repo1.GetMaxSeq(me_docd.DOCNO);
                    me_docd.MMCODE = form.Get("MMCODE");
                    me_docd.APPQTY = form.Get("APPQTY");
                    me_docd.UP = form.Get("UP");

                    //if (repo.CheckMmcodeValid(me_docd.MMCODE) == false)
                    //{
                    //    session.Result.afrs = 0;
                    //    session.Result.success = false;
                    //    session.Result.msg = "<span style='color:red'>非可調整院內碼</span>，請重新輸入院內碼。";
                    //    return session.Result;
                    //}

                    if (!repo1.CheckMeDocdExists_1(me_docd))
                    {
                        me_docd.CREATE_USER = User.Identity.Name;
                        me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.DetailCreate(me_docd);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此異動單已存在此院內碼</span>，請重新輸入院內碼。";
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
        public ApiResponse DetailUpdate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO");
                    me_docd.SEQ = form.Get("SEQ");
                    me_docd.MMCODE = form.Get("MMCODE");
                    me_docd.APPQTY = form.Get("APPQTY");
                    me_docd.UP = form.Get("UP");

                    bool flowIdValid = repo.ChceckFlowId01(me_docd.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }
                    else if (repo1.CheckMeDocdExists_1(me_docd))
                    {
                        session.Result.msg = "<span style='color:red'>此異動單已存在此院內碼</span>，請重新輸入院內碼。";
                        session.Result.success = false;
                        return session.Result;
                    }

                    //if (repo1.CheckWhmmExists(form.Get("FRWH2").Split(' ')[0], me_docd.MMCODE))
                    //{
                    me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.DetailUpdate(me_docd);
                    //}
                    //else
                    //{
                    //    session.Result.afrs = 0;
                    //    session.Result.success = false;
                    //    session.Result.msg = "<span style='color:red'>出庫庫房不存放此院內碼</span>，請重新輸入院內碼。";
                    //}
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
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp_docno.Length; i++)
                        {
                            bool flowIdValid = repo.ChceckFlowId01(tmp_docno[i]);
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
                    AA0155Repository repo = new AA0155Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        // 先做資料檢查,都通過後再繼續做處理的部分
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            bool flowIdValid = repo.ChceckFlowId01(tmp[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = tmp[i] + "此單據狀態非未過帳，請重新確認";
                                session.Result.success = false;
                                return session.Result;
                            }

                            AB0010Repository repo1 = new AB0010Repository(DBWork);
                            if (!repo1.CheckMeDocdExists(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            {
                                session.Result.msg = tmp[i] + "此單據無院內碼項次，請重新確認";
                                session.Result.success = false;
                                return session.Result;
                            }

                            if (!repo.CheckMeDocdAppqty(tmp[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>異動單號「" + tmp[i] + "」院內碼項次轉換數量必須大於0</span>，請填寫轉換數量。";
                                return session.Result;
                            }
                        }

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            // 更新ME_DOCD金額
                            repo.DetailUpdateP(tmp[i]);

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
    }
}
