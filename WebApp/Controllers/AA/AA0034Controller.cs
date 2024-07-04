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
    public class AA0034Controller : SiteBase.BaseApiController
    {
        // 查詢
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
                    var repo = new AA0034Repository(DBWork);
                    var repo1 = new AB0021Repository(DBWork);
                    AA0034Repository.ME_DOCM_QUERY_PARAMS query = new AA0034Repository.ME_DOCM_QUERY_PARAMS();
                    query.DOCNO = form.Get("p1");
                    query.MAT_CLASS = form.Get("p3");

                    if (repo1.IsWhGrade1(form.Get("APPDEPT")))   // 庫房人員
                    {
                        query.APPDEPT = "";

                        if (form.Get("TASKID") == "2")  //衛材
                        {
                            if (form.Get("p3") == "02")
                                query.MAT_CLASS = form.Get("p3");
                            else if (form.Get("p3") == "")
                                query.MAT_CLASS = "02";
                            else
                                query.MAT_CLASS = "XX";
                        }
                        else if (form.Get("TASKID") == "3") // 一般物品
                        {
                            if (form.Get("p3") == "02")  // 一般物品的庫房人員不能看 物料分類02 的繳回單
                                query.MAT_CLASS = "XX";
                            else if (form.Get("p3") == "") // 空值則抓取 物料分類03~08
                                query.MAT_CLASS = "03-08";
                            else
                                query.MAT_CLASS = form.Get("p3");
                        }

                    }
                    else
                        query.MAT_CLASS = "XX";    // 申請人員則不能看

                    query.FLOWID = form.Get("p2");
                    query.DOCTYPE = form.Get("DOCTYPE");
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
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0034Repository(DBWork);
                    AA0034Repository.ME_DOCD_QUERY_PARAMS query = new AA0034Repository.ME_DOCD_QUERY_PARAMS();
                    query.DOCNO = form.Get("p0");
                    query.DATA_YM = repo.GetYm();
                    session.Result.etts = repo.GetMeDocd(query);
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
                    AA0034Repository repo = new AA0034Repository(DBWork);
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
        public ApiResponse GetMatClassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0034Repository repo = new AA0034Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo();
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
                    AA0034Repository repo = new AA0034Repository(DBWork);
                    AA0034Repository.MI_MAST_QUERY_PARAMS query = new AA0034Repository.MI_MAST_QUERY_PARAMS();
                    query.WH_NO = form.Get("WH_NO");
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS");
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();

                    //// 需判斷庫存量>0
                    //if (form.Get("IS_INV") != null && form.Get("IS_INV") == "1")
                    //    query.IS_INV = form.Get("IS_INV");

                    //// 需判斷是否公藥
                    //query.E_IFPUBLIC = form.Get("E_IFPUBLIC") == null ? "" : form.Get("E_IFPUBLIC");
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
            var p0 = form.Get("p0");
            var mat_class = form.Get("MAT_CLASS");
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
                    AA0034Repository repo = new AA0034Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, wh_no, mat_class, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmcodeInvQty(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AA0034Repository repo = new AA0034Repository(DBWork);
                    AA0034Repository.MI_MAST_QUERY_PARAMS query = new AA0034Repository.MI_MAST_QUERY_PARAMS();
                    query.WH_NO = form.Get("WH_NO");
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS");
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    session.Result.etts = repo.GetMmcodeInvQty(query);
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
                    AA0034Repository repo = new AA0034Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    AB0021Repository repo2 = new AB0021Repository(DBWork);
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
                        me_docm.MAT_CLASS = form.Get("MAT_CLASS");
                        me_docm.APPLY_NOTE = form.Get("APPLY_NOTE");
                        //me_docm.FRWH = repo2.GeFrwh(me_docm.APPDEPT);
                        me_docm.FRWH = "560000";
                        //me_docm.TOWH = repo2.GeFrwh(me_docm.APPDEPT);  // 要指定TOWH,不然最後呼叫POST_DOC會有 庫房代碼錯誤
                        me_docm.TOWH = "560000";
                        me_docm.DOCTYPE = "SP1";
                        me_docm.FLOWID = "1";

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
                    AA0034Repository repo = new AA0034Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(me_docm.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;
                    //if (!repo.CheckMeDocdExists(me_docm.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                    //{
                    session.Result.afrs = repo.MasterUpdate(me_docm);
                    //}
                    //else
                    //{
                    //session.Result.afrs = 0;
                    //session.Result.success = false;
                    //session.Result.msg = "<span style='color:red'>申請單號「" + me_docm.DOCNO + "」有院內碼項次，請先刪除所有項次才能修改。</span>";
                    //return session.Result;
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
        public ApiResponse MasterDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0034Repository repo = new AA0034Repository(DBWork);
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
                    AA0034Repository repo = new AA0034Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    AB0021Repository repo2 = new AB0021Repository(DBWork);
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
                        me_docd.SEQ = repo2.GetMaxSeq(me_docd.DOCNO);
                    me_docd.MMCODE = form.Get("MMCODE");
                    me_docd.APPQTY = form.Get("APPQTY");
                    me_docd.APLYITEM_NOTE = form.Get("APLYITEM_NOTE");
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
                        session.Result.msg = "<span style='color:red'>此申請單已存在此院內碼</span>，請重新輸入院內碼。";
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
                    AA0034Repository repo = new AA0034Repository(DBWork);
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
                    me_docd.APLYITEM_NOTE = form.Get("APLYITEM_NOTE");
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.DetailUpdate(me_docd);

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
                    AA0034Repository repo = new AA0034Repository(DBWork);
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
                    AA0034Repository repo = new AA0034Repository(DBWork);
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