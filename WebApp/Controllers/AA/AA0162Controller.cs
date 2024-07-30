using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Repository.AB;
using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0162Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p1 = form.Get("p1");
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            string[] arr_p2 = { };
            if (!string.IsNullOrEmpty(p2))
            {
                arr_p2 = p2.Trim().Split(','); //用,分割
            }
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0162Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p1, d0, d1, arr_p2, p3, User.Identity.Name, page, limit, sorters);
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0162Repository(DBWork);
                    string docno = form.Get("p0");
                    string wh_no = form.Get("p1");
                    var docds = repo.GetAllD(docno, wh_no, page, limit, sorters);

                    //foreach (ME_DOCD docd in docds)
                    //{
                    //    docd.AMOUNT = (double.Parse(docd.APVQTY) * double.Parse(docd.M_CONTPRICE)).ToString();
                    //}

                    session.Result.etts = docds;

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
                    AA0162Repository repo = new AA0162Repository(DBWork);
                    AA0162Repository.MI_MAST_QUERY_PARAMS query = new AA0162Repository.MI_MAST_QUERY_PARAMS();
                    query.DOCNO = form.Get("DOCNO") == null ? "" : form.Get("DOCNO").ToUpper();
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
                    AA0162Repository repo = new AA0162Repository(DBWork);
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
        public ApiResponse GetMclassQCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0162Repository repo = new AA0162Repository(DBWork);
                    session.Result.etts = repo.GetMclassQCombo(User.Identity.Name);
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
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0162Repository repo = new AA0162Repository(DBWork);
                    session.Result.etts = repo.GetMclassCombo(User.Identity.Name);
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
                    AA0162Repository repo = new AA0162Repository(DBWork);
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
        public ApiResponse MasterCreate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0162Repository repo = new AA0162Repository(DBWork);
                    ME_DOCM me_docm = new ME_DOCM();
                    me_docm.DOCNO = repo.GetDailyDocno();
                    if (!repo.CheckExists(me_docm.DOCNO))
                    {
                        me_docm.CREATE_USER = User.Identity.Name;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        me_docm.APPID = User.Identity.Name;
                        me_docm.USEID = User.Identity.Name;
                        me_docm.APPDEPT = form.Get("APPDEPT_NAME").Split(' ')[0];
                        me_docm.FRWH = form.Get("FRWH").Split(' ')[0];
                        me_docm.MAT_CLASS = form.Get("MAT_CLASS");
                        me_docm.APPLY_NOTE = form.Get("APPLY_NOTE");
                        if (me_docm.MAT_CLASS == "01")
                        {
                            me_docm.DOCTYPE = "SP";
                            me_docm.FLOWID = "0501";
                        }
                        else if (me_docm.MAT_CLASS == "02")
                        {
                            me_docm.DOCTYPE = "SP1";
                            me_docm.FLOWID = "1";
                        }

                        session.Result.afrs = repo.MasterCreate(me_docm);
                        session.Result.etts = repo.MasterGet(me_docm.DOCNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>報廢單號</span>重複，請重新嘗試。";
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
                    AA0162Repository repo = new AA0162Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(me_docm.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

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
                            session.Result.msg = "<span style='color:red'>報廢單號「" + me_docm.DOCNO + "」已存在" + frwh + "出庫庫房院內碼項次，所以無法修改出庫庫房</span><br>如欲修改出庫庫房，請先刪除所有項次。";
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
                    AA0162Repository repo = new AA0162Repository(DBWork);

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
        public ApiResponse DetailCreate(ME_DOCEXP me_docexp)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0162Repository repo = new AA0162Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(me_docexp.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    if (!repo1.CheckMeDocexpExists(me_docexp.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                        me_docexp.SEQ = "1";
                    else
                        me_docexp.SEQ = repo1.GetMaxSeqForDocexp(me_docexp.DOCNO);

                    if (!repo.CheckExpExisted(me_docexp, false))
                    {
                        me_docexp.UPDATE_USER = User.Identity.Name;
                        me_docexp.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.DetailCreate(me_docexp);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此單明細已有重複的院內碼「" + me_docexp.MMCODE + "」</span>，請確認。";
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
        public ApiResponse DetailUpdate(ME_DOCEXP me_docexp)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0162Repository repo = new AA0162Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(me_docexp.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }
                    else if (repo.CheckExpExisted(me_docexp, true))
                    {
                        session.Result.msg = "<span style='color:red'>院內碼、批號或效期重複</span>，請重新輸入。";
                        session.Result.success = false;
                        return session.Result;
                    }

                    me_docexp.UPDATE_USER = User.Identity.Name;
                    me_docexp.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.DetailUpdate(me_docexp);

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
                    AA0162Repository repo = new AA0162Repository(DBWork);
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
                DBWork.BeginTransaction();
                try
                {
                    AA0162Repository repo = new AA0162Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            bool flowIdValid = repo.ChceckFlowId01(tmp[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = tmp[i] + "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }
                            else
                            {
                                if (repo.CheckExistsE(tmp[i]))
                                {
                                    var v_docno = tmp[i];
                                    var v_mmcode = "";
                                    var v_note = "";
                                    var v_appqty = 0.0000;                                     

                                    foreach (ME_DOCM Mmcode in repo.GetDocEMmcode_All(v_docno))
                                    {
                                        v_mmcode = Mmcode.MMCODE;
                                        if (!repo.CheckExistsDMmcode(v_docno, v_mmcode))
                                        {
                                            v_appqty = double.Parse(repo.GetDocEMmcodeApvqty(v_docno, v_mmcode));
                                            v_note = repo.GetDocEMmcodeNote(v_docno, v_mmcode);
                                            ME_DOCD me_docd = new ME_DOCD();
                                            me_docd.DOCNO = v_docno;
                                            if (!repo.CheckExistsD(v_docno))
                                            {
                                                me_docd.SEQ = "1";
                                            }
                                            else
                                            {
                                                me_docd.SEQ = repo.GetDocDSeq(v_docno);
                                            }
                                            me_docd.MMCODE = v_mmcode;
                                            me_docd.APPQTY = v_appqty.ToString();
                                            me_docd.APLYITEM_NOTE = v_note;
                                            me_docd.UPDATE_USER = User.Identity.Name;
                                            me_docd.UPDATE_IP = DBWork.ProcIP;
                                            session.Result.afrs = repo.CreateD(me_docd);
                                        }
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
                                else
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」沒有院內碼項次</span>，請新增院內碼項次。";
                                    return session.Result;
                                }
                              
                            }
                            // 更新批號效期及儲位的庫存量
                            AA0147Controller cont = new AA0147Controller();
                            List<ME_DOCD> data_list = repo.GetMeDocExpWexpidNs(tmp[i]).ToList<ME_DOCD>();
                            cont.updateExpLocInv(data_list, DBWork);
                        }
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

    }
}