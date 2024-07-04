﻿using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0131Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            string[] arr_p2 = { };
            if (!string.IsNullOrEmpty(p2))
            {
                arr_p2 = p2.Trim().Split(','); //用,分割
            }

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0131Repository(DBWork);
                    //var v_inid = repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.GetAllM(p0, d0, d1, arr_p2, page, limit, sorters);
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
                    var repo = new AA0131Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, page, limit, sorters);
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
        public ApiResponse CreateM(ME_DOCM ME_DOCM)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0131Repository(DBWork);
                    if (!repo.CheckExists(ME_DOCM.DOCNO)) // 新增前檢查主鍵是否已存在
                    {
                        var v_inid = repo.GetUridInid(User.Identity.Name);
                        var v_twntime = repo.GetTwnsystime();
                        var v_matclass = ME_DOCM.MAT_CLASS;
                        var v_docno = ME_DOCM.TOWH + v_twntime + v_matclass;
                        var v_whno = repo.GetFrwh();
                        ME_DOCM.DOCNO = v_docno;
                        ME_DOCM.APPID = User.Identity.Name;
                        ME_DOCM.FRWH = v_whno;
                        //ME_DOCM.APPDEPT = v_inid;
                        ME_DOCM.APPLY_KIND = "2";
                        ME_DOCM.USEID = User.Identity.Name;
                        ME_DOCM.USEDEPT = ME_DOCM.APPDEPT;
                        ME_DOCM.CREATE_USER = User.Identity.Name;
                        ME_DOCM.UPDATE_USER = User.Identity.Name;
                        ME_DOCM.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateM(ME_DOCM);
                        session.Result.etts = repo.GetM(ME_DOCM.DOCNO);

                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>代碼</span>重複，請重新輸入。";
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
        public ApiResponse CreateD(ME_DOCD ME_DOCD)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0131Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(ME_DOCD.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    if (repo.CheckExistsMM(ME_DOCD.DOCNO, ME_DOCD.MMCODE))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此單明細已有重複的院內碼「" + ME_DOCD.MMCODE + "」</span>，請確認。";
                    }
                    else
                    {
                        if (!repo.CheckExistsD(ME_DOCD.DOCNO)) // 新增前檢查主鍵是否已存在
                        {
                            ME_DOCD.SEQ = "1";
                        }
                        else
                        {
                            ME_DOCD.SEQ = repo.GetDocDSeq(ME_DOCD.DOCNO);
                        }
                        ME_DOCD.CREATE_USER = User.Identity.Name;
                        ME_DOCD.UPDATE_USER = User.Identity.Name;
                        ME_DOCD.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateD(ME_DOCD);
                        session.Result.etts = repo.GetD(ME_DOCD.DOCNO, ME_DOCD.SEQ);
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
        public ApiResponse UpdateM(ME_DOCM ME_DOCM)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0131Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(ME_DOCM.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    ME_DOCM.UPDATE_USER = User.Identity.Name;
                    ME_DOCM.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(ME_DOCM);
                    session.Result.etts = repo.GetM(ME_DOCM.DOCNO);

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
        public ApiResponse UpdateD(ME_DOCD ME_DOCD)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0131Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(ME_DOCD.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    ME_DOCD.UPDATE_USER = User.Identity.Name;
                    ME_DOCD.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(ME_DOCD);
                    session.Result.etts = repo.GetD(ME_DOCD.DOCNO, ME_DOCD.SEQ);

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
                    AA0131Repository repo = new AA0131Repository(DBWork);
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

                            //if (!repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            //{
                            session.Result.afrs = repo.DeleteAllD(tmp[i]);
                            session.Result.afrs = repo.DeleteM(tmp[i]);
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
        public ApiResponse DeleteD(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0131Repository repo = new AA0131Repository(DBWork);
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
                    AA0131Repository repo = new AA0131Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM(tmp[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>此申請單狀態非申請中</span>不得核撥。";
                            }
                            else
                            {
                                if (repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                                {
                                    if (repo.CheckExistsDN(tmp[i]))
                                    {
                                        session.Result.afrs = 0;
                                        session.Result.success = false;
                                        session.Result.msg = "<span style='color:red'>此單明細尚有申請數量為0</span>不得核撥。";
                                    }
                                    else
                                    {
                                        ME_DOCM me_docm = new ME_DOCM();
                                        me_docm.DOCNO = tmp[i];
                                        me_docm.FLOWID = "2";
                                        me_docm.UPDATE_USER = User.Identity.Name;
                                        me_docm.UPDATE_IP = DBWork.ProcIP;
                                        session.Result.afrs = repo.ApplyM(me_docm);
                                        session.Result.afrs = repo.ApplyD(me_docm);
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
        public ApiResponse GetDocnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0131Repository repo = new AA0131Repository(DBWork);
                    var v_inid = repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.GetDocnoCombo(v_inid);
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
                    AA0131Repository repo = new AA0131Repository(DBWork);
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
        public ApiResponse GetAppDeptCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0131Repository repo = new AA0131Repository(DBWork);
                    session.Result.etts = repo.GetAppDeptCombo();
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
            var p2 = form.Get("p2");
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
                    AA0131Repository repo = new AA0131Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, p2, p3, page, limit, "");
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
                    AA0131Repository repo = new AA0131Repository(DBWork);
                    AA0131Repository.MI_MAST_QUERY_PARAMS query = new AA0131Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS").ToUpper();
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
                    AA0131Repository repo = new AA0131Repository(DBWork);
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
        public ApiResponse GetFrwhCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0131Repository repo = new AA0131Repository(DBWork);
                    session.Result.etts = repo.GetFrwhCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetTowhCombo()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0131Repository repo = new AA0131Repository(DBWork);
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
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0131Repository repo = new AA0131Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo();
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
                    AA0131Repository repo = new AA0131Repository(DBWork);
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


    }
}