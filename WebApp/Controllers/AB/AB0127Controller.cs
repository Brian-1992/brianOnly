using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Linq;
using System.Web;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Data;

namespace WebApp.Controllers.AB
{
    public class AB0127Controller : SiteBase.BaseApiController
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    string v_user = User.Identity.Name; //repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.GetAllM(p0, d0, d1, arr_p2, p3, v_user, page, limit, sorters);
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
                    var repo = new AB0127Repository(DBWork);
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
        public ApiResponse CreateM(ME_DOCM me_docm)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    string v_docno = repo.GetDailyDocno();
                    if (!repo.CheckExists(v_docno)) // 新增前檢查主鍵是否已存在
                    {
                        me_docm.MAT_CLASS = "01";
                        me_docm.DOCNO = v_docno;
                        me_docm.DOCTYPE = repo.getDoctype(me_docm.TOWH);
                        if (me_docm.DOCTYPE == "MR")
                            me_docm.FLOWID = "0101";
                        else
                            me_docm.FLOWID = "0601";
                        me_docm.APPID = User.Identity.Name;
                        me_docm.APPLY_KIND = "2";
                        me_docm.USEID = User.Identity.Name;
                        me_docm.USEDEPT = me_docm.APPDEPT;
                        me_docm.SRCDOCNO = v_docno;
                        me_docm.CREATE_USER = User.Identity.Name;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateM(me_docm);
                        session.Result.etts = repo.GetM(me_docm.DOCNO);

                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>申請單號</span>重複，請重新嘗試。";
                    }

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

        [HttpPost]
        public ApiResponse CreateD(ME_DOCD me_docd)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0127Repository(DBWork);
                    string errMsg = "";
                    string towh = repo.GetThisTowh(me_docd.DOCNO);
                    string frwh_d = repo.GetFrwhWithMmcode(towh, me_docd.MMCODE);
                    string hospCode = repo.GetHospCode();

                    bool flowIdValid = repo.CheckFlowId01(me_docd.DOCNO);
                    if (flowIdValid == false)
                        errMsg += "申請單狀態已變更，請重新查詢" + "</br>";
                    else if (!repo.CheckMmcode(me_docd.MMCODE))
                        errMsg += "<span style='color:red'>此院內碼「" + me_docd.MMCODE + "」不正確</span>，請確認。" + "</br>";
                    else if (repo.CheckExistsMM(me_docd.DOCNO, me_docd.MMCODE))
                        errMsg += "<span style='color:red'>此單明細已有重複的院內碼「" + me_docd.MMCODE + "」</span>，請確認。" + "</br>";
                    //花蓮需求
                    else if (repo.CheckMaxAppqtyFlag(me_docd.MMCODE, towh, frwh_d, me_docd.APPQTY) == "Y" && hospCode == "805")
                        errMsg += "<span style='color:red'>此院內碼「" + me_docd.MMCODE + "」</span>現存量+申請量不可超過基準量(庫房設定的單位請領量)，請確認。" + "</br>";
                    else if (repo.CheckMmcodeCancelSingle(me_docd.MMCODE))
                    {
                        errMsg += "<span style='color:red'>此院內碼「" + me_docd.MMCODE + "」</span>已全院停用，請確認。" + "</br>";
                    }

                    if (errMsg != "")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = errMsg;
                        DBWork.Rollback();
                        return session.Result;
                    }

                    if (!repo.CheckExistsD(me_docd.DOCNO)) // 新增前檢查主鍵是否已存在
                    {
                        me_docd.SEQ = "1";
                    }
                    else
                    {
                        me_docd.SEQ = repo.GetDocDSeq(me_docd.DOCNO);
                    }
                    me_docd.SRCDOCNO = me_docd.DOCNO;
                    me_docd.FRWH_D = frwh_d;
                    me_docd.CREATE_USER = User.Identity.Name;
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.CreateD(me_docd);
                    if (repo.CheckAppid(me_docd.DOCNO))
                    {
                        session.Result.afrs = repo.UpdateAppid(User.Identity.Name, DBWork.ProcIP, me_docd.DOCNO);
                    }
                    session.Result.etts = repo.GetD(me_docd.DOCNO, me_docd.SEQ);
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
                    var repo = new AB0127Repository(DBWork);

                    bool flowIdValid = repo.CheckFlowId01(ME_DOCM.DOCNO);
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
                    var repo = new AB0127Repository(DBWork);

                    bool flowIdValid = repo.CheckFlowId01(ME_DOCD.DOCNO);
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            bool flowIdValid = repo.CheckFlowId01(tmp[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }

                            session.Result.afrs = repo.DeleteAllD(tmp[i]);
                            session.Result.afrs = repo.DeleteM(tmp[i]);
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp_docno.Length; i++)
                        {
                            bool flowIdValid = repo.CheckFlowId01(tmp_docno[i]);
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string errMsg = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckIsTowhCancelByDocno(tmp[i]))
                                errMsg += string.Format("申請單號「{0}」申請庫房已作廢，請重新確認", tmp[i]) + "<br/>";
                            else if (repo.CheckDuplicateMmcode(tmp[i]))
                                errMsg += string.Format("申請單號「{0}」<span style='color:red'>品項院內碼重複</span>，請修改後再提出申請", tmp[i]) + "<br/>";

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
                                errMsg += string.Format("申請單號「{0}」<span style='color:red'>有已作廢品項</span>，請修改後再提出申請，品項如下：<br>{1}",
                                                         tmp[i], error_mmcodes) + "<br/>";
                            }

                            string frwh_d = repo.getDocTowh(tmp[i]); //取得出庫庫號
                            if (repo.CheckExistsM(tmp[i], "01"))
                                errMsg += "<span style='color:red'>此申請單狀態非申請中</span>不得提出申請。" + "<br/>";
                            else if (!repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                                errMsg += "<span style='color:red'>申請單號「" + tmp[i] + "」沒有院內碼項次</span>，請新增院內碼項次。" + "<br/>";
                            else if (repo.CheckExistsDN(tmp[i]))
                                errMsg += "<span style='color:red'>此單明細尚有申請數量為0</span>不得核撥。" + "<br/>";

                            //檢查是否所有品項申請量都為申請倍數
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
                                    session.Result.msg = string.Format("申請單號「{0}」<span style='color:red'>申請量不符合申請倍數</span>，請修改後再提出申請，品項如下：<br>{1}",
                                                                        tmp[i], error_mmcodes);
                                    return session.Result;
                                }
                            }
                            IEnumerable<ME_DOCD> exceed_docds = repo.CheckMmcodeMaxAppqtyFlag(tmp[i]);
                            if (exceed_docds.Any() && hospCode == "805") // 增加 805 判斷
                            {
                                string error_mmcodes = string.Empty;
                                foreach (ME_DOCD docd in exceed_docds)
                                {
                                    if (error_mmcodes != string.Empty)
                                    {
                                        error_mmcodes += "<br/>";
                                    }
                                    error_mmcodes += docd.MMCODE;
                                }
                                errMsg += string.Format("申請單號「{0}」<span style='color:red'>現存量+申請量不可超過基準量(庫房設定的單位請領量)</span>，品項如下：<br>{1}",
                                                         tmp[i], error_mmcodes) + "<br/>";
                            }

                            // 若msg不為空，表示有錯誤
                            if (errMsg != string.Empty)
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = errMsg;
                                DBWork.Rollback();
                                return session.Result;
                            }
                            //通過檢核
                            ME_DOCM me_docm = new ME_DOCM();
                            //string srcdocno = tmp[i];
                            me_docm.DOCNO = tmp[i];
                            me_docm.TOWH = frwh_d;
                            me_docm.DOCTYPE = repo.getDoctype(me_docm.TOWH);
                            if (me_docm.DOCTYPE == "MR")
                                me_docm.FLOWID = "0111";
                            else
                                me_docm.FLOWID = "0611";
                            me_docm.SENDAPVID = User.Identity.Name;
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            me_docm.ISARMY = repo.getDocIsarmy(tmp[i]);
                            me_docm.APPUNA = repo.getDocAppuna(tmp[i]);
                            IEnumerable<ME_DOCD> myEnum = repo.GetSplitValue(me_docm.DOCNO);
                            myEnum.GetEnumerator();
                            string item_FRWH = "", item_MCONTID = "";
                            foreach (var item in myEnum)
                            {
                                // 第一個核撥庫房&合約識別
                                if (item_FRWH == "" && item_MCONTID == "")
                                {
                                    me_docm.FRWH = item.FRWH_D;
                                    me_docm.M_CONTID = item.M_CONTID;
                                    repo.MasterUpdateFrwhMcontid(me_docm);
                                }
                                else
                                {
                                    // 拆單,並新建單號
                                    ME_DOCM me_docm_new = new ME_DOCM();
                                    me_docm_new.DOCNO = repo.GetDailyDocno();
                                    me_docm_new.CREATE_USER = User.Identity.Name;
                                    me_docm_new.UPDATE_USER = User.Identity.Name;
                                    me_docm_new.UPDATE_IP = DBWork.ProcIP;
                                    me_docm_new.APPID = User.Identity.Name;
                                    me_docm_new.APPLY_KIND = "2";
                                    me_docm_new.APPDEPT = DBWork.UserInfo.Inid;
                                    me_docm_new.USEDEPT = DBWork.UserInfo.Inid;
                                    me_docm_new.USEID = User.Identity.Name;
                                    me_docm_new.TOWH = me_docm.TOWH;        // 申請庫房
                                    me_docm_new.FRWH = item.FRWH_D;        // 核撥庫房
                                    me_docm_new.DOCTYPE = me_docm.DOCTYPE;
                                    me_docm_new.FLOWID = me_docm.FLOWID;
                                    me_docm_new.MAT_CLASS = "01";
                                    me_docm_new.SRCDOCNO = me_docm_new.DOCNO;
                                    me_docm_new.ISARMY = me_docm.ISARMY;
                                    me_docm_new.APPUNA = me_docm.APPUNA;
                                    me_docm_new.M_CONTID = item.M_CONTID;
                                    repo.CreateM(me_docm_new);
                                    // 將第二個核撥庫房+合約識別的項次,修改為新單號
                                    repo.DetailUpdateDocno(tmp[i], item.FRWH_D, item.M_CONTID, me_docm_new.DOCNO, User.Identity.Name, DBWork.ProcIP);

                                    // 以便後面UpdateStatus更新FLOWID用
                                    me_docm.DOCNO = me_docm_new.DOCNO;
                                }
                                item_FRWH = item.FRWH_D;
                                item_MCONTID = item.M_CONTID;

                                // 提出申請更新me_docd.apl_contime, 預帶申請量
                                session.Result.afrs = repo.ApplyD(me_docm);

                                // 狀態更新
                                session.Result.afrs = repo.ApplyM(me_docm);
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO");
                        string note = form.Get("NOTE");
                        string doctype = form.Get("DOCTYPE");
                        string matclass = form.Get("MAT_CLASS");
                        string newdocno = repo.GetDailyDocno();
                        session.Result.afrs = repo.SavepkM(docno, doctype, note, newdocno);
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
                    var repo = new AB0127Repository(DBWork);
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
                    var repo = new AB0127Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetSaveD(p2, p3, page, limit, sorters, hospCode);
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    string hospCode = repo.GetHospCode();
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
                            bool flowIdValid = repo.CheckFlowId01(tmp_docno[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }

                            string towh = repo.GetThisTowh(tmp_docno[i]);
                            string frwh_d = repo.GetFrwhWithMmcode(towh, tmp_mmcode[i]);
                            if (repo.CheckMaxAppqtyFlag(tmp_mmcode[i], towh, frwh_d, tmp_appqty[i]) == "Y" && hospCode == "805") // 增加 805 判斷
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = string.Format("<span style='color:red'>現存量+申請量不可超過基準量(庫房設定的單位請領量)，請確認", tmp_mmcode[i]);
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
                                me_docd.SRCDOCNO = tmp_docno[i];
                                me_docd.SEQ = v_seq;
                                me_docd.MMCODE = tmp_mmcode[i];
                                me_docd.APPQTY = tmp_appqty[i];
                                me_docd.FRWH_D = repo.GetFrwhWithMmcode(towh, me_docd.MMCODE);
                                me_docd.HIGH_QTY = repo.GetOperqtyWithMmcode(towh, tmp_mmcode[i]);
                                me_docd.S_INV_QTY = repo.GetSinvqtyWithMmcode(towh, tmp_mmcode[i]);
                                me_docd.INV_QTY = repo.GetInvqtyWithMmcode(towh, tmp_mmcode[i]);
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
            var p2 = form.Get("p2");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    session.Result.etts = repo.GetDocnopkCombo(p0, p1, p2);
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
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    session.Result.etts = repo.GetDocpknoteCombo(p0);
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    session.Result.etts = repo.GetFlowidCombo(User.Identity.Name);
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, p2, page, limit, "");
                }
                catch (Exception e)
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    AB0127Repository.MI_MAST_QUERY_PARAMS query = new AB0127Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS").ToUpper();
                    query.M_AGENNO = form.Get("M_AGENNO") == null ? "" : form.Get("M_AGENNO").ToUpper();
                    query.AGEN_NAME = form.Get("AGEN_NAME") == null ? "" : form.Get("AGEN_NAME").ToUpper();
                    query.MMCODE_Q1 = form.Get("MMCODE_Q1") == null ? true : bool.Parse(form.Get("MMCODE_Q1"));
                    query.MMCODE_Q2 = form.Get("MMCODE_Q2") == null ? true : bool.Parse(form.Get("MMCODE_Q2"));
                    query.DRUGSNAME = form.Get("DRUGSNAME") == null ? "" : form.Get("DRUGSNAME").ToUpper();
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
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
        public ApiResponse GetTowhCombo()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0127Repository repo = new AB0127Repository(DBWork);
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
        public ApiResponse GetLoginInfo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    var v_userid = User.Identity.Name;
                    var v_ip = DBWork.ProcIP;
                    var hospCode = repo.GetHospCode();
                    var ettsResult = repo.GetLoginInfo(v_userid, v_ip);

                    //寫入醫院CODE
                    foreach (var item in ettsResult)
                    {
                        item.HOSP_CODE = hospCode;
                    }
                    session.Result.etts = ettsResult;
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    bool checkPassed = true; //檢核有沒有通過 有通過就true 沒通過就false
                    string hospCode = repo.GetHospCode();

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
                    string[] arr = { "院內碼", "申請數量", "備註" };

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
                        foreach (string header in arr)
                        {
                            var isExist = false;
                            for (i = 0; i < cellCount; i++)
                            {
                                isExist = headerRow.GetCell(i).ToString() == header ? true : false;
                                if (isExist)
                                {
                                    break;
                                }
                            }

                            //有存在header就找下一個
                            isValid = isExist;

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

                    bool flowIdValid = repo.CheckFlowId01(v_docno);
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

                        string towh = repo.GetThisTowh(v_docno);

                        //加入至ME_DOCD中
                        #region 加入至ME_DOCD中
                        for (i = 0; i < newTable.Rows.Count; i++)
                        {
                            ME_DOCD me_docd = new ME_DOCD();

                            me_docd.MMCODE = newTable.Rows[i]["院內碼"].ToString().Trim();
                            me_docd.APPQTY = newTable.Rows[i]["申請數量"].ToString();
                            me_docd.APLYITEM_NOTE = newTable.Rows[i]["備註"].ToString().Trim();
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
                                //擷取月基準量
                                string high_qty = repo.GetOperqtyWithMmcode(towh, me_docd.MMCODE);
                                if (high_qty == null) { high_qty = "0"; }

                                string frwh_d = repo.GetFrwhWithMmcode(towh, me_docd.MMCODE);
                                //檢核庫房代碼
                                if (repo.CheckExistsMMCODE(me_docd.MMCODE) != true)
                                {
                                    me_docd.CHECK_RESULT = "此院內碼不存在";
                                }
                                else if (repo.CheckMatClassMMCODE("01", me_docd.MMCODE) != true)
                                {
                                    me_docd.CHECK_RESULT = "此院內碼物料分類非藥品";
                                }
                                else if (repo.Checkappqty(me_docd.MMCODE, me_docd.APPQTY) == true)
                                {
                                    me_docd.CHECK_RESULT = "申請量必須符合申請倍數";
                                }
                                else if (repo.CheckExistsMM(v_docno, me_docd.MMCODE) == true)
                                {
                                    me_docd.CHECK_RESULT = "此單明細已有重複的院內碼";
                                }
                                else if (repo.CheckMaxAppqtyFlag(me_docd.MMCODE, towh, frwh_d, me_docd.APPQTY) == "Y" && hospCode == "805") // 增加 805 判斷
                                {
                                    me_docd.CHECK_RESULT = "現存量+申請量不可超過基準量(庫房設定的單位請領量)";
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
                                else
                                {
                                    //1130314 北投申請數量=0不匯入訊息，僅提醒不卡住
                                    if (me_docd.APPQTY == "0")
                                    {
                                        me_docd.CHECK_RESULT += "【申請數量=0不匯入】";
                                    }
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
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    JCLib.Excel.Export("AB0127.xls", repo.GetExcel());
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 依據 DOCNO 匯出 DETAIL EXCEL
        [HttpPost]
        public ApiResponse DetailExcel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var p0 = form.Get("p0");
                    var p1 = form.Get("p1");
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    JCLib.Excel.Export(p0 + ".xls", repo.GetDetailExcel(p0, p1));
                }
                catch (Exception e)
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
                    var repo = new AB0127Repository(DBWork);
                    bool isDuplicate = false;

                    foreach (ME_DOCD data in me_docd)
                    {
                        bool flowIdValid = repo.CheckFlowId01(v_docno);
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
                                //申請量>0才新增ME_DOCD
                                if (int.Parse(data.APPQTY)> 0)
                                {
                                    string towh = repo.GetThisTowh(data.DOCNO);
                                    data.SRCDOCNO = v_docno;
                                    data.FRWH_D = repo.GetFrwhWithMmcode(towh, data.MMCODE);
                                    data.CREATE_USER = User.Identity.Name;
                                    data.UPDATE_USER = User.Identity.Name;
                                    data.UPDATE_IP = DBWork.ProcIP;
                                    data.GTAPL_RESON = "1";
                                    data.HIGH_QTY = repo.GetOperqtyWithMmcode(towh, data.MMCODE);
                                    data.S_INV_QTY = repo.GetSinvqtyWithMmcode(towh, data.MMCODE);
                                    data.INV_QTY = repo.GetInvqtyWithMmcode(towh, data.MMCODE);
                                    session.Result.afrs = repo.CreateD(data);
                                    if (repo.CheckAppid(data.DOCNO))
                                    {
                                        session.Result.afrs = repo.UpdateAppid(User.Identity.Name, DBWork.ProcIP, data.DOCNO);
                                    }
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

        [HttpPost]
        public ApiResponse GetDocAppAmout(FormDataCollection form)
        {
            var docno = form.Get("docno");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    session.Result.msg = repo.GetDocAppAmout(docno);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetIsArmyCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    session.Result.etts = repo.GetIsArmyCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse Cancel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM(tmp[i], "11"))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>此申請單狀態非未核可</span>不得取消送申請。";
                            }
                            else
                            {
                                ME_DOCM me_docm = new ME_DOCM();
                                me_docm.DOCNO = tmp[i];
                                me_docm.TOWH = repo.getDocTowh(tmp[i]);
                                me_docm.DOCTYPE = repo.getDoctype(me_docm.TOWH);
                                if (me_docm.DOCTYPE == "MR")
                                    me_docm.FLOWID = "0101";
                                else
                                    me_docm.FLOWID = "0601";
                                me_docm.SENDAPVID = User.Identity.Name;
                                me_docm.UPDATE_USER = User.Identity.Name;
                                me_docm.UPDATE_IP = DBWork.ProcIP;
                                session.Result.afrs = repo.ApplyM(me_docm);
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
        public void MasterExcel(FormDataCollection form)
        {

            var docnos = form.Get("docnos");

            using (WorkSession session = new WorkSession(this))
            {

                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {

                    AB0127Repository repo = new AB0127Repository(DBWork);
                    docnos = ReplaceHTMLEncode(docnos);

                    IEnumerable<ME_DOCM> docno_list = JsonConvert.DeserializeObject<IEnumerable<ME_DOCM>>(docnos);

                    string temp = string.Empty;
                    foreach (ME_DOCM docno in docno_list)
                    {
                        if (string.IsNullOrEmpty(temp) == false)
                        {
                            temp += ",";
                        }
                        temp += string.Format("'{0}'", docno.DOCNO);
                    }

                    DataTable data = null;

                    data = repo.GetMasterExcel(temp);

                    if (data.Rows.Count > 0)
                    {
                        var workbook = ExoprtToExcel(data);

                        //output
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            workbook.Write(memoryStream);
                            JCLib.Export.OutputFile(memoryStream, string.Format("藥品申請_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss")));
                            workbook.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }


            }
        }

        //製造EXCEL的內容
        public XSSFWorkbook ExoprtToExcel(DataTable data)
        {
            var wb = new XSSFWorkbook();
            var sheet = (XSSFSheet)wb.CreateSheet("Sheet1");

            IRow row = sheet.CreateRow(0);
            for (int i = 0; i < data.Columns.Count; i++)
            {
                row.CreateCell(i).SetCellValue(data.Columns[i].ToString());
            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                row = sheet.CreateRow(1 + i);
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(data.Rows[i].ItemArray[j].ToString());
                }
            }
            return wb;
        }
        [HttpPost]
        public ApiResponse CheckUnitrateFlg(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0127Repository repo = new AB0127Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string rtnMsg = "";

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckUnitrateFlg(tmp[i]) > 0)
                            {
                                if (rtnMsg != "")
                                {
                                    rtnMsg += ",";
                                }
                                rtnMsg = tmp[i];
                            }
                        }
                        session.Result.afrs = 0;
                        session.Result.success = true;
                        session.Result.msg = rtnMsg;
                        return session.Result;
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public string ReplaceHTMLEncode(string value)
        {
            value = value.Replace("&amp;", "\\");
            value = value.Replace("&quot;", "'");

            return value;
        }


        [HttpPost]
        public ApiResponse GetInvqty(FormDataCollection form)
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
                    var repo = new AB0127Repository(DBWork);
                    session.Result.etts = repo.GetInvqty(p0, page, limit, sorters);
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