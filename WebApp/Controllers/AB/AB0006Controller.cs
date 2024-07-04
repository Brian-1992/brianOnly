using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System.Collections.Generic;
using System.Web;
using NPOI.SS.UserModel;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using Newtonsoft.Json;
using System.Linq;

namespace WebApp.Controllers.AB
{
    public class AB0006Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
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
                    var repo = new AB0006Repository(DBWork);
                    var v_inid = User.Identity.Name; //repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.GetAllM(p0, d0, d1, arr_p2, p3, v_inid, page, limit, sorters);
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
                    var repo = new AB0006Repository(DBWork);
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
                    var repo = new AB0006Repository(DBWork);
                    if (!repo.CheckExists(ME_DOCM.DOCNO)) // 新增前檢查主鍵是否已存在
                    {
                        if (repo.CheckApplyable() == false)
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "目前非您非庫備申請期間。";
                            return session.Result;
                        }

                        //if (repo.CheckExistsM3(ME_DOCM.MAT_CLASS, ME_DOCM.TOWH, ME_DOCM.DOCTYPE) && repo.CheckExistsM1(ME_DOCM.MAT_CLASS, ME_DOCM.TOWH, ME_DOCM.DOCTYPE))
                        //{
                        //    session.Result.afrs = 0;
                        //    session.Result.success = false;
                        //    session.Result.msg = "尚有<span style='color:red'>未點收</span>申請單，請先執行點收。";
                        //}
                        //else
                        //{
                        if (repo.CheckIsTowhCancelByWhno(ME_DOCM.TOWH))
                        {
                            session.Result.success = false;
                            session.Result.msg = "申請庫房已作廢，請重新選擇";
                            return session.Result;
                        }
                        var v_inid = repo.GetUridInid(User.Identity.Name);
                            var v_twntime = repo.GetTwnsystime();
                            var v_matclass = ME_DOCM.MAT_CLASS;
                            var v_docno = ME_DOCM.TOWH + v_twntime + v_matclass;
                            var v_whno = repo.GetFrwh();
                            var v_MR = repo.CheckApplyKind();
                            var v_applykind = "1";
                            var v_num = repo.CheckApplyKindNum(v_matclass, ME_DOCM.TOWH);
                            var v_matname = repo.GetMatclass(v_matclass);
                            ME_DOCM.DOCNO = v_docno;
                            ME_DOCM.APPID = User.Identity.Name;
                            ME_DOCM.FRWH = v_whno;
                            ME_DOCM.APPDEPT = v_inid;
                            ME_DOCM.APPLY_KIND = v_applykind;
                            ME_DOCM.USEID = User.Identity.Name;
                            ME_DOCM.USEDEPT = ME_DOCM.APPDEPT;
                            ME_DOCM.CREATE_USER = User.Identity.Name;
                            ME_DOCM.UPDATE_USER = User.Identity.Name;
                            ME_DOCM.UPDATE_IP = DBWork.ProcIP;
                            if (v_MR == "Y")
                            {
                                if (v_num > 0)
                                {

                                    var v_docno_o = repo.GetApplyKindDocno(v_matclass, ME_DOCM.TOWH);
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "您本月的<span style='color:red'>「" + v_matname + "」</span>已申請過，申請單號:<span style='color:red'>「" + v_docno_o + "」</span>。";
                                }
                                else
                                {
                                    session.Result.afrs = repo.CreateM(ME_DOCM);
                                    session.Result.etts = repo.GetM(ME_DOCM.DOCNO);
                                }
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "目前非您非庫備申請期間。";
                            }
                        //}
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
                    var repo = new AB0006Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(ME_DOCD.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    string towh = repo.GetTowh(ME_DOCD.DOCNO);
                    if (repo.CheckWHMM(ME_DOCD.MMCODE, towh) == false)
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>本庫房無法申領此院內碼</span>，請確認。";
                        return session.Result;
                    }


                    if (!repo.CheckMmcode(ME_DOCD.MMCODE, ME_DOCD.DOCNO))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此院內碼「" + ME_DOCD.MMCODE + "」不正確</span>，請確認。";
                    }
                    else
                    {
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
                            if (repo.CheckAppid(ME_DOCD.DOCNO))
                            {
                                session.Result.afrs = repo.UpdateAppid(User.Identity.Name, DBWork.ProcIP, ME_DOCD.DOCNO);
                            }
                            session.Result.etts = repo.GetD(ME_DOCD.DOCNO, ME_DOCD.SEQ);
                            DBWork.Commit();
                        }
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
                    var repo = new AB0006Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(ME_DOCM.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    ME_DOCM.APPID = User.Identity.Name;
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
                    var repo = new AB0006Repository(DBWork);

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
                    AB0006Repository repo = new AB0006Repository(DBWork);
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
                    AB0006Repository repo = new AB0006Repository(DBWork);
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
                    AB0006Repository repo = new AB0006Repository(DBWork);
                    AB0003Repository repoAB0003 = new AB0003Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {


                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckApplyable(tmp[i]) == false)
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = string.Format("申請單號「{0}」超過申請期限，無法送核撥，請重新確認", tmp[i]);
                                return session.Result;
                            }

                            if (repo.CheckIsTowhCancelByDocno(tmp[i]))
                            {
                                session.Result.success = false;
                                session.Result.msg = string.Format("申請單號「{0}」申請庫房已作廢，請重新確認", tmp[i]);
                                return session.Result;
                            }

                            if (repoAB0003.CheckDuplicateMmcode(tmp[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = string.Format("申請單號「{0}」<span>申請院內碼重複</span>，請修改後再送核撥", tmp[i]);
                                return session.Result;
                            }
                            // 2022-12-07 新增: 檢查是否有取消品項
                            IEnumerable<ME_DOCD> cancel_docds = repo.CheckMmcodeCancel(tmp[i]);
                            if (cancel_docds.Any())
                            {
                                string error_mmcodes = string.Empty;
                                foreach (ME_DOCD docd in cancel_docds)
                                {
                                    if (error_mmcodes != string.Empty)
                                    {
                                        error_mmcodes += "<br/>";
                                    }
                                    error_mmcodes += docd.MMCODE;
                                }

                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = string.Format("申請單號「{0}」<span style='color:red'>有已作廢品項</span>，請修改後再送核撥，品項如下：<br>{1}",
                                                                    tmp[i], error_mmcodes);
                                return session.Result;
                            }
                            // 2020-08-15 新增: 檢查時否有不可申領品項
                            IEnumerable<ME_DOCD> e_docds = repoAB0003.CheckMApplyidE(tmp[i], true);
                            if (e_docds.Any())
                            {
                                string error_mmcodes = string.Empty;
                                foreach (ME_DOCD docd in e_docds)
                                {
                                    if (error_mmcodes != string.Empty)
                                    {
                                        error_mmcodes += "<br/>";
                                    }
                                    error_mmcodes += docd.MMCODE;
                                }

                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = string.Format("申請單號「{0}」<span style='color:red'>有不可申領品項</span>，請修改後再送核撥，品項如下：<br>{1}",
                                                                    tmp[i], error_mmcodes);
                                return session.Result;
                            }
                            // 2021-02-20 新增: 檢查時否有不可申領品項
                            IEnumerable<ME_DOCD> whmm_docds = repoAB0003.CheckApplyWHMM(tmp[i]);
                            if (whmm_docds.Any())
                            {
                                string error_mmcodes = string.Empty;
                                foreach (ME_DOCD docd in whmm_docds)
                                {
                                    if (error_mmcodes != string.Empty)
                                    {
                                        error_mmcodes += "<br/>";
                                    }
                                    error_mmcodes += docd.MMCODE;
                                }

                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = string.Format("申請單號「{0}」<span style='color:red'>有不可申領品項</span>，請修改後再送核撥，品項如下：<br>{1}",
                                                                    tmp[i], error_mmcodes);
                                return session.Result;
                            }
                            // 2022-11-16 新增：檢查是否所有品項皆為非庫備
                            IEnumerable<ME_DOCD> mstoreid_docds = repo.CheckMstoreid(tmp[i], "0");
                            if (mstoreid_docds.Any())
                            {
                                string error_mmcodes = string.Empty;
                                foreach (ME_DOCD docd in mstoreid_docds)
                                {
                                    if (error_mmcodes != string.Empty)
                                    {
                                        error_mmcodes += "<br/>";
                                    }
                                    error_mmcodes += docd.MMCODE;
                                }

                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = string.Format("申請單號「{0}」<span style='color:red'>有庫備品項</span>，請修改後再送核撥，品項如下：<br>{1}",
                                                                    tmp[i], error_mmcodes);
                                return session.Result;
                            }


                            // 2022-11-16 新曾：檢查是否所有品項申請量都為最小撥補量的倍數
                            IEnumerable<ME_DOCD> appqty_docds = repo.CheckMinOrderQty(tmp[i]);
                            foreach (ME_DOCD docd in appqty_docds)
                            {
                                string error_mmcodes = string.Empty;
                                if (docd.IS_APPQTY_VALID == "N")
                                {
                                    if (error_mmcodes != string.Empty)
                                    {
                                        error_mmcodes += "<br/>";
                                    }
                                    error_mmcodes += docd.MMCODE;
                                }

                                if (string.IsNullOrEmpty(error_mmcodes) == false)
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = string.Format("申請單號「{0}」<span style='color:red'>申請量不為最小撥補量倍數</span>，請修改後再送核撥，品項如下：<br>{1}",
                                                                        tmp[i], error_mmcodes);
                                    return session.Result;
                                }
                            }

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
        public ApiResponse Savepk(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0006Repository repo = new AB0006Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO");
                        string note = form.Get("NOTE");
                        var v_matclass = form.Get("MAT_CLASS");
                        var v_inid = repo.GetUridInid(User.Identity.Name);
                        var v_twntime = repo.GetTwnsystime();
                        var newdocno = v_inid + v_twntime + v_matclass;
                        session.Result.afrs = repo.SavepkM(docno, note, newdocno);
                        session.Result.afrs = repo.SavepkD(docno, newdocno);
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
        public ApiResponse GetPackD(FormDataCollection form)
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
                    var repo = new AB0006Repository(DBWork);
                    session.Result.etts = repo.GetPackD(p0, p1, p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetSaveD(FormDataCollection form)
        {
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
                    var repo = new AB0006Repository(DBWork);
                    session.Result.etts = repo.GetSaveD(p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse InsFromPk(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0006Repository repo = new AB0006Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1);
                        string appqty = form.Get("APPQTY").Substring(0, form.Get("APPQTY").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_mmcode = mmcode.Split(',');
                        string[] tmp_appqty = appqty.Split(',');
                        string v_seq = "1";
                        ME_DOCD me_docd = new ME_DOCD();
                        for (int i = 0; i < tmp_docno.Length; i++)
                        {
                            bool flowIdValid = repo.ChceckFlowId01(tmp_docno[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }

                            string towh = repo.GetTowh(tmp_docno[i]);
                            if (repo.CheckWHMM(tmp_mmcode[i], towh) == false)
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = string.Format("<span style='color:red'>本庫房無法申領此院內碼({0})</span>，請確認。", tmp_mmcode[i]);
                                return session.Result;
                            }


                            if (repo.CheckExistsMM(tmp_docno[i], tmp_mmcode[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>此單明細已有重複的院內碼「" + tmp_mmcode[i] + "」</span>，請確認。";
                                break;
                            }
                            else
                            {
                                if (!repo.CheckExistsD(tmp_docno[i])) // 新增前檢查主鍵是否已存在
                                {
                                    v_seq = "1";
                                }
                                else
                                {
                                    v_seq = repo.GetDocDSeq(tmp_docno[i]);
                                }
                                me_docd.DOCNO = tmp_docno[i];
                                me_docd.SEQ = v_seq;
                                me_docd.MMCODE = tmp_mmcode[i];
                                me_docd.APPQTY = tmp_appqty[i];
                                me_docd.CREATE_USER = User.Identity.Name;
                                me_docd.UPDATE_USER = User.Identity.Name;
                                me_docd.UPDATE_IP = DBWork.ProcIP;
                                session.Result.afrs = repo.CreateD(me_docd);
                                if (repo.CheckAppid(tmp_docno[i]))
                                {
                                    session.Result.afrs = repo.UpdateAppid(User.Identity.Name, DBWork.ProcIP, tmp_docno[i]);
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
        public ApiResponse GetDocnopkCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0006Repository repo = new AB0006Repository(DBWork);
                    var v_inid = repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.GetDocnopkCombo(p0, p1);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetDocpknoteCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0006Repository repo = new AB0006Repository(DBWork);
                    var v_inid = repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.GetDocpknoteCombo(p0, p1);
                }
                catch
                {
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
                    AB0006Repository repo = new AB0006Repository(DBWork);
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
                    AB0006Repository repo = new AB0006Repository(DBWork);
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
        public ApiResponse GetApplyKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0006Repository repo = new AB0006Repository(DBWork);
                    session.Result.etts = repo.GetApplyKindCombo();
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
            var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0006Repository repo = new AB0006Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, p2, p3, p4, page, limit, "");
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
                    AB0006Repository repo = new AB0006Repository(DBWork);
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
                    AB0006Repository repo = new AB0006Repository(DBWork);
                    AB0006Repository.MI_MAST_QUERY_PARAMS query = new AB0006Repository.MI_MAST_QUERY_PARAMS();
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
        public ApiResponse GetFrwhCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0006Repository repo = new AB0006Repository(DBWork);
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
                    AB0006Repository repo = new AB0006Repository(DBWork);
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
                    AB0006Repository repo = new AB0006Repository(DBWork);
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
                    AB0006Repository repo = new AB0006Repository(DBWork);
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
        public ApiResponse GetReasonCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0006Repository repo = new AB0006Repository(DBWork);
                    session.Result.etts = repo.GetReasonCombo();
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
                var v_docno = HttpContext.Current.Request.Form["docno"];
                IWorkbook workBook;

                try
                {
                    AB0006Repository repo = new AB0006Repository(DBWork);
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
                    //IRow headerRow = sheet.GetRow(0); //由第一列取標題做為欄位名稱
                    IRow headerRow = sheet.GetRow(0); //由第二列取標題做為欄位名稱
                    int cellCount = headerRow.LastCellNum; //欄位數目
                    int i, j;


                    bool isValid = true;
                    string[] arr = { "院內碼", "申請數量" };

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

                    bool flowIdValid = repo.ChceckFlowId01(v_docno);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

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
                        //for (i = 1; i <= sheet.LastRowNum; i++)
                        //略過第0列(說明列)和第1列(標題列)，一直處理至最後一列
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

                        string towh = repo.GetTowh(v_docno);

                        //加入至ME_DOCD中
                        #region 加入至ME_DOCD中
                        for (i = 0; i < newTable.Rows.Count; i++)
                        {
                            ME_DOCD me_docd = new ME_DOCD();

                            me_docd.MMCODE = newTable.Rows[i]["院內碼"].ToString().Trim();
                            me_docd.APPQTY = newTable.Rows[i]["申請數量"].ToString();
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
                                else if (repo.CheckExistsMMCODE2(me_docd.MMCODE) != true)
                                {
                                    me_docd.CHECK_RESULT = "此院內碼不適用非庫備申請";
                                }
                                else if (repo.CheckMatClassMMCODE(v_mat_class, me_docd.MMCODE) != true)
                                {
                                    me_docd.CHECK_RESULT = "此院內碼不適用申請的物料分類";
                                }
                                else if (repo.Checkappqty(me_docd.MMCODE, me_docd.APPQTY) == true)
                                {
                                    me_docd.CHECK_RESULT = "申請量必須為最小撥補量的倍數";
                                }
                                else if (repo.CheckExistsMM(v_docno, me_docd.MMCODE) == true)
                                {
                                    me_docd.CHECK_RESULT = "此單明細已有重複的院內碼";
                                }
                                else if (repo.CheckWHMM(me_docd.MMCODE, towh) == false)
                                {
                                    me_docd.CHECK_RESULT = "本庫房無法申領此院內碼";
                                }
                                else
                                {
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
        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0006Repository repo = new AB0006Repository(DBWork);
                    JCLib.Excel.Export("AB0006.xls", repo.GetExcel());
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //確認更新
        [HttpPost]
        public ApiResponse Insert(FormDataCollection formData)
        {
            var v_docno = formData.Get("docno");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                IEnumerable<ME_DOCD> me_docd = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(formData["data"]);

                List<string> checkDuplicate = new List<string>();

                List<ME_DOCD> me_docd_list = new List<ME_DOCD>();
                try
                {
                    var repo = new AB0006Repository(DBWork);
                    bool isDuplicate = false;

                    foreach (ME_DOCD data in me_docd)
                    {
                        bool flowIdValid = repo.ChceckFlowId01(v_docno);
                        if (flowIdValid == false)
                        {
                            session.Result.msg = "申請單狀態已變更，請重新查詢";
                            session.Result.success = false;
                            return session.Result;
                        }

                        data.DOCNO = v_docno;
                        if (checkDuplicate.Contains(data.MMCODE)) //檢查list有沒有已經insert過的MMCODE
                        {
                            isDuplicate = true;
                            session.Result.msg = isDuplicate.ToString();
                            break;
                        }
                        else
                        {
                            checkDuplicate.Add(data.MMCODE);

                            try
                            {
                                if (!repo.CheckExistsD(data.DOCNO)) // 新增前檢查主鍵是否已存在
                                {
                                    data.SEQ = "1";
                                }
                                else
                                {
                                    data.SEQ = repo.GetDocDSeq(data.DOCNO);
                                }
                                data.CREATE_USER = User.Identity.Name;
                                data.UPDATE_USER = User.Identity.Name;
                                data.UPDATE_IP = DBWork.ProcIP;
                                data.GTAPL_RESON = "1";
                                session.Result.afrs = repo.CreateD(data);
                                if (repo.CheckAppid(data.DOCNO))
                                {
                                    session.Result.afrs = repo.UpdateAppid(User.Identity.Name, DBWork.ProcIP, data.DOCNO);
                                }
                            }
                            catch
                            {
                                throw;
                            }
                            me_docd_list.Add(data);
                        }
                    }

                    session.Result.etts = me_docd_list;

                    if (isDuplicate == false)
                    {
                        DBWork.Commit();
                    }
                    else
                    {
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
    }
}