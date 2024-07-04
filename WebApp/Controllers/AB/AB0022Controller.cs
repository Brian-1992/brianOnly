using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Repository.AB;
using WebApp.Models;

namespace WebApp.Controllers.AB
{
    public class AB0022Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AB0022Repository repo = new AB0022Repository(DBWork);
                    AB0022Repository.ME_DOCM_QUERY_PARAMS query = new AB0022Repository.ME_DOCM_QUERY_PARAMS();
                    query.DOCNO = form.Get("p1") == null ? "" : form.Get("p1");
                    query.APPID = form.Get("p2") == null ? "" : form.Get("p2").ToUpper();
                    query.APPDEPT = form.Get("p3") == null ? "" : form.Get("p3").ToUpper();
                    query.FRWH = form.Get("p4") == null ? "" : form.Get("p4").ToUpper();
                    query.TOWH = form.Get("p5") == null ? "" : form.Get("p5").ToUpper();
                    query.INID = DBWork.UserInfo.Inid;

                    query.DOCTYPE = form.Get("DOCTYPE");
                    query.FLOWID = form.Get("FLOWID") == null ? "" : form.Get("FLOWID");
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
                    AB0022Repository repo = new AB0022Repository(DBWork);
                    AB0022Repository.MI_MAST_QUERY_PARAMS query = new AB0022Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO");

                    //// 需判斷庫存量>0
                    //if (form.Get("IS_INV") != null && form.Get("IS_INV") == "1")
                    //    query.IS_INV = form.Get("IS_INV");


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
                    AB0022Repository repo = new AB0022Repository(DBWork);
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
        public ApiResponse GetFrwhCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0022Repository repo = new AB0022Repository(DBWork);
                    //session.Result.etts = repo.GetFrwhCombo(DBWork.UserInfo.Inid);
                    session.Result.etts = repo.GetFrwhCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetQueryAppidCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0022Repository repo = new AB0022Repository(DBWork);
                    session.Result.etts = repo.GetAppidCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTowh(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0022Repository(DBWork);
                    session.Result.etts = repo
                        .GetTowh(form.Get("INID"), int.Parse(form.Get("WH_GRADE")))
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, SUPPLY_INID = w.SUPPLY_INID });
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
                    AB0022Repository repo = new AB0022Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);

                    if (repo.CheckIsTowhCancelByWhno(form.Get("FRWH")))
                    {
                        session.Result.success = false;
                        session.Result.msg = "申請庫房已作廢，請重新選擇";
                        return session.Result;
                    }

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
                        me_docm.TOWH = form.Get("TOWH");        // 繳回庫房
                        me_docm.FRWH = form.Get("FRWH");        // 申請庫房
                        me_docm.DOCTYPE = "RN";
                        me_docm.FLOWID = "0401";
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
                    AB0022Repository repo = new AB0022Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);

                    if (repo.CheckIsTowhCancelByWhno(me_docm.FRWH))
                    {
                        session.Result.success = false;
                        session.Result.msg = "申請庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    if (repo.CheckIsTowhCancelByWhno(me_docm.TOWH))
                    {
                        session.Result.success = false;
                        session.Result.msg = "繳回庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    bool flowIdValid = repo.ChceckFlowId01(me_docm.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;

                    string towh = repo.GetTowh(me_docm.DOCNO);
                    if (towh == me_docm.TOWH.Trim()) // 如果核撥庫房一樣,則可以直接更新
                        session.Result.afrs = repo.MasterUpdate(me_docm);
                    else
                    {
                        if (!repo1.CheckMeDocdExists(me_docm.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                            session.Result.afrs = repo.MasterUpdate(me_docm);
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>申請單號「" + me_docm.DOCNO + "」已存在" + towh + "庫房院內碼項次，所以無法修改核撥庫房</span><br>如欲修改核撥庫房，請先刪除所有項次。";
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
                    AB0022Repository repo = new AB0022Repository(DBWork);
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

                            //if (!repo.CheckMeDocdExists(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            //{
                            repo.DetailAllDelete(tmp[i]);
                            session.Result.afrs = repo.MasterDelete(tmp[i]);
                            //}
                            //else
                            //{
                            //    session.Result.afrs = 0;
                            //    session.Result.success = false;
                            //    session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」有院內碼項次，請先刪除所有項次。</span>";
                            //    return session.Result;
                            //}
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
                    AB0022Repository repo = new AB0022Repository(DBWork);
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
                    me_docd.APLYITEM_NOTE = form.Get("APLYITEM_NOTE");

                    // 2021-02-25 檢查院內碼是否全院停用
                    if (repo.CheckOrderdcflagN(form.Get("MMCODE")) == false) {
                        session.Result.success = false;
                        session.Result.msg = "此院內碼<span style='color:red'>全院停用</span>，不可繳回";
                        return session.Result;
                    }

                    // 2021-02-25 檢查庫存量是否為0
                    if (repo.CheckInvqty0(me_docd.MMCODE, me_docd.DOCNO))
                    {
                        session.Result.success = false;
                        session.Result.msg = "此院內碼<span style='color:red'>庫存為0</span>，不可繳回";
                        return session.Result;
                    }

                    if (repo1.CheckWhmmExists(form.Get("TOWH2").Split(' ')[0], me_docd.MMCODE))
                    {
                        if (!repo1.CheckMeDocdExists_1(me_docd))
                        {
                            me_docd.CREATE_USER = User.Identity.Name;
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.DetailCreate(me_docd);

                            session.Result.msg = repo.IsWexpid(me_docd.MMCODE);

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
                        session.Result.msg = "<span style='color:red'>核撥庫房不存放此院內碼</span>，請重新輸入院內碼。";
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
                    AB0022Repository repo = new AB0022Repository(DBWork);
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
                    me_docd.APLYITEM_NOTE = form.Get("APLYITEM_NOTE");

                    // 2021-02-25 檢查院內碼是否全院停用
                    if (repo.CheckOrderdcflagN(form.Get("MMCODE")) == false)
                    {
                        session.Result.success = false;
                        session.Result.msg = "此院內碼<span style='color:red'>全院停用</span>，不可調撥";
                        return session.Result;
                    }

                    // 2021-02-25 檢查庫存量是否為0
                    if (repo.CheckInvqty0(me_docd.MMCODE, me_docd.DOCNO))
                    {
                        session.Result.success = false;
                        session.Result.msg = "此院內碼<span style='color:red'>庫存為0</span>，不可繳回";
                        return session.Result;
                    }

                    if (repo1.CheckWhmmExists(form.Get("TOWH2").Split(' ')[0], me_docd.MMCODE))
                    {
                        me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.DetailUpdate(me_docd);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>核撥庫房不存放此院內碼</span>，請重新輸入院內碼。";
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
                    AB0022Repository repo = new AB0022Repository(DBWork);
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
                    AB0022Repository repo = new AB0022Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');

                        foreach (string tempdocno in tmp)
                        {
                            if (repo.CheckIsTowhCancelByDocno(tempdocno, "FRWH"))
                            {
                                session.Result.success = false;
                                session.Result.msg = "申請庫房已作廢，請重新選擇";
                                return session.Result;
                            }

                            if (repo.CheckIsTowhCancelByDocno(tempdocno, "TOWH"))
                            {
                                session.Result.success = false;
                                session.Result.msg = "繳回庫房已作廢，請重新選擇";
                                return session.Result;
                            }

                            bool flowIdValid = repo.ChceckFlowId01(tempdocno);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }

                            if (repo1.CheckMeDocdExists(tempdocno) == false)
                            { // 傳入DOCNO檢查申請單是否有院內碼項次
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>申請單號「" + tempdocno + "」沒有院內碼項次</span>，請新增院內碼項次。";
                                return session.Result;
                            }

                            if (repo.DOC_CHECK_EXP(tempdocno) == false) {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>申請單號「" + tempdocno + "」未輸入效期資料</span>";
                                return session.Result;
                            }

                            //2021-02-25 檢查是否有全院停用品項
                            IEnumerable<string> orderdcMmcodes = repo.CheckOrderdcflagYByDocno(tempdocno);
                            if (orderdcMmcodes.Any())
                            {
                                session.Result.success = false;
                                string msg = string.Empty;
                                foreach (string mmcode in orderdcMmcodes) {
                                    if (msg != string.Empty) {
                                        msg += "、";
                                    }
                                    msg += mmcode;
                                }
                                session.Result.msg = string.Format("申請單號「<span style='color:red'>{0}</span>」有<span style='color:red'>全院停用</span>品項：<br>{1}</span>", tempdocno, msg);
                                return session.Result;
                            }

                            //2021-02-25 檢查是否有0庫存品項
                            IEnumerable<string> invqty0Mmcodes = repo.CheckInvqty0ByDocno(tempdocno);
                            if (invqty0Mmcodes.Any())
                            {
                                session.Result.success = false;
                                string msg = string.Empty;
                                foreach (string mmcode in invqty0Mmcodes)
                                {
                                    if (msg != string.Empty)
                                    {
                                        msg += "、";
                                    }
                                    msg += mmcode;
                                }
                                session.Result.msg = string.Format("申請單號「<span style='color:red'>{0}</span>」有<span style='color:red'>庫存為0</span>品項：<br>{1}</span>", tempdocno, msg);
                                return session.Result;
                            }

                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tempdocno;
                            me_docm.FLOWID = form.Get("FLOWID");
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;

                            // 產生ME_DOCE效期資料
                            SP_MODEL sp = repo.CreateMeDoceByDocno(me_docm.DOCNO);
                            if (sp.O_RETID == "N")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = sp.O_ERRMSG;
                                return session.Result;
                            }

                            // 狀態更新成0602
                            session.Result.afrs = repo.UpdateStatus(me_docm);
                        }


                        //for (int i = 0; i < tmp.Length; i++)
                        //{
                        //    if (repo1.CheckMeDocdExists(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                        //    {
                        //        ME_DOCM me_docm = new ME_DOCM();
                        //        me_docm.DOCNO = tmp[i];
                        //        me_docm.FLOWID = form.Get("FLOWID");
                        //        me_docm.UPDATE_USER = User.Identity.Name;
                        //        me_docm.UPDATE_IP = DBWork.ProcIP;

                        //        // 產生ME_DOCE效期資料
                        //        SP_MODEL sp = repo.CreateMeDoceByDocno(me_docm.DOCNO);
                        //        if (sp.O_RETID == "N")
                        //        {
                        //            session.Result.afrs = 0;
                        //            session.Result.success = false;
                        //            session.Result.msg = sp.O_ERRMSG;
                        //            return session.Result;
                        //        }

                        //        // 狀態更新成0602
                        //        session.Result.afrs = repo.UpdateStatus(me_docm);
                        //    }
                        //    else
                        //    {
                        //        session.Result.afrs = 0;
                        //        session.Result.success = false;
                        //        session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」沒有院內碼項次</span>，請新增院內碼項次。";
                        //        return session.Result;
                        //    }

                        //}
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
        public ApiResponse Copy(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0022Repository repo = new AB0022Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            string newDocno = repo1.GetDocno();
                            session.Result.afrs = repo.Copy(newDocno, tmp[i], User.Identity.Name, DBWork.ProcIP);
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
        public ApiResponse DeleteEmptyMaster()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0022Repository repo = new AB0022Repository(DBWork);
                    session.Result.afrs = repo.DeleteEmptyMaster(DBWork.UserInfo.UserId);
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