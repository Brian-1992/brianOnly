using System;
using System.Collections.Generic;
using System.Linq;
using JCLib.DB;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebApp.Repository.AA;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0032Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
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
                    AA0032Repository repo = new AA0032Repository(DBWork);
                    AA0032Repository.ME_DOCM_QUERY_PARAMS query = new AA0032Repository.ME_DOCM_QUERY_PARAMS();

                    // 取得登入者有權限的庫房
                    string whid = "";
                    int i = 0;
                    var tmp = repo.GetWhId(User.Identity.Name).GetEnumerator();
                    while (tmp.MoveNext())
                    {
                        if (i == 0)
                            whid = tmp.Current.VALUE;
                        else
                            whid += "," + tmp.Current.VALUE;
                        i++;
                    }

                    query.DOCNO = form.Get("p1") == null ? "" : form.Get("p1").Trim();
                    query.APPID = form.Get("p2") == null ? "" : form.Get("p2").ToUpper().Trim();
                    query.FRWH = form.Get("p4") == null || form.Get("p4").Trim() == "" ? whid : form.Get("p4").ToUpper();
                    query.TOWH = form.Get("p5") == null || form.Get("p5").Trim() == "" ? whid : form.Get("p5").ToUpper();

                    query.DOCTYPE = "XR";
                    query.FLOWID = form.Get("FLOWID") == null ? "" : form.Get("FLOWID").Trim();
                    query.APPTIME_S = "";
                    query.APPTIME_E = "";


                    if (form.Get("d0") != null && form.Get("d0") != "")
                        query.APPTIME_S = form.Get("d0").Split('T')[0];  // yyyy-mm-ddT00:00:00
                    if (form.Get("d1") != null && form.Get("d1") != "")
                        query.APPTIME_E = form.Get("d1").Split('T')[0];

                    session.Result.etts = repo.GetAll(query, page, limit, sorters);
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0032Repository(DBWork);
                    AA0032Repository.ME_DOCD_QUERY_PARAMS query = new AA0032Repository.ME_DOCD_QUERY_PARAMS();
                    query.DOCNO = form.Get("p0");
                    session.Result.etts = repo.GetMeDocd(query, page, limit, sorters);
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
                    var repo = new AA0032Repository(DBWork);
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
                    AA0032Repository repo = new AA0032Repository(DBWork);
                    AA0032Repository.MI_MAST_QUERY_PARAMS query = new AA0032Repository.MI_MAST_QUERY_PARAMS();
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
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0").ToUpper();
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
                    AA0032Repository repo = new AA0032Repository(DBWork);
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
        public ApiResponse MasterCreate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0032Repository repo = new AA0032Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    ME_DOCM me_docm = new ME_DOCM();
                    me_docm.DOCNO = repo1.GetDocno();
                    if (!repo1.CheckExists(me_docm.DOCNO))
                    {
                        me_docm.CREATE_USER = User.Identity.Name;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        me_docm.APPID = User.Identity.Name;
                        me_docm.APPDEPT = form.Get("INID_NAME").Split(' ')[0];
                        me_docm.USEID = User.Identity.Name;
                        me_docm.TOWH = form.Get("TOWH");      
                        me_docm.FRWH = form.Get("FRWH");      
                        me_docm.DOCTYPE = "XR";
                        me_docm.FLOWID = "0301";
                        me_docm.MAT_CLASS = "01";

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
                    AA0032Repository repo = new AA0032Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(me_docm.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

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
                    AA0032Repository repo = new AA0032Repository(DBWork);

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
                    AA0032Repository repo = new AA0032Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO2");

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
                    me_docd.UP = form.Get("M_CONTPRICE");

                    if (repo.CheckMmcodeValid(me_docd.MMCODE) == false) {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>非可調整院內碼</span>，請重新輸入院內碼。";
                        return session.Result;
                    }

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
                    AA0032Repository repo = new AA0032Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO2");

                    bool flowIdValid = repo.ChceckFlowId01(me_docd.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    me_docd.SEQ = form.Get("SEQ");
                    me_docd.MMCODE = form.Get("MMCODE");
                    me_docd.APPQTY = form.Get("APPQTY");
                    me_docd.UP = form.Get("M_CONTPRICE");
                    if (repo1.CheckWhmmExists(form.Get("FRWH2").Split(' ')[0], me_docd.MMCODE))
                    {
                        me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.DetailUpdate(me_docd);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>出庫庫房不存放此院內碼</span>，請重新輸入院內碼。";
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
                    AA0032Repository repo = new AA0032Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp_docno.Length; i++) {
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
                    AA0032Repository repo = new AA0032Repository(DBWork);
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

                            if (repo.CheckMeDocdAppqty(tmp[i]))
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
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>異動單號「" + tmp[i] + "」院內碼項次轉換數量必須大於0</span>，請填寫轉換數量。";
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
