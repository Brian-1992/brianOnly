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
using System.Net;
using System.Net.Sockets;

namespace WebApp.Controllers.AB
{
    public class AB0118Controller : SiteBase.BaseApiController
    {
        #region " flylon "
        [HttpPost]
        public ApiResponse Update_HIS14_SUPDET_DOCNO(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                int update_cnt = 0;
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    string supdet_seq = form.Get("SUPDET_SEQ").Substring(0, form.Get("SUPDET_SEQ").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] tmp_supdet_seq = supdet_seq.Split(',');

                    string update_user = User.Identity.Name;
                    string update_ip = DBWork.ProcIP;

                    for (int i = 0; i < tmp_supdet_seq.Length; i++)
                    {
                        update_cnt += repo.Update_HIS14_SUPDET_DOCNO(tmp_supdet_seq[i], update_user, update_user, update_ip);
                    }
                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                session.Result.afrs = update_cnt;
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse ins_ME_DOCD_upd_HIS14_SUPDET(FormDataCollection form)
        {
            var docno = form.Get("docno");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0118Repository(DBWork);
                    // (1)取出暫存區的資料
                    IEnumerable<HIS14_SUPDET> TempData = repo.Select_HIS14_SUPDET_DOCNO(User.Identity.Name);
                    if (TempData.Count() == 0)
                    {
                        session.Result.msg = "查無資料可轉入請領單!";
                        session.Result.success = false;
                        return session.Result;
                    }
                    // (2)針對暫存區資料，採迴圈，執行下列步驟：
                    string mmcode = "", towh = "", high_qty = "", update_user = "", update_ip = "", v_seq = "";
                    int update_cnt = 0;
                    if (!repo.CheckExistsD(docno)) // 新增前檢查主鍵是否已存在
                    {
                        v_seq = "1";
                    }
                    else
                    {
                        v_seq = repo.GetDocDSeq(docno);
                    }
                    foreach (var item in TempData)
                    {
                        // 步驟1：insert ME_DOCD
                        ME_DOCD me_docd = new ME_DOCD();
                        mmcode = item.SUP_SKDIACODE;
                        towh = repo.GetTowh(docno);
                        high_qty = repo.GetOperqtyWithMmcode(towh, mmcode);
                        update_user = User.Identity.Name;
                        update_ip = DBWork.ProcIP;
                        if (update_cnt > 0)
                        {
                            v_seq = (int.Parse(v_seq) + 1).ToString();
                        }
                        me_docd.DOCNO = docno;
                        me_docd.SRCDOCNO = docno;
                        me_docd.FRWH_D = repo.GetFrwhWithMmcode(repo.GetThisTowh(me_docd.DOCNO), mmcode);
                        me_docd.SEQ = v_seq;
                        me_docd.MMCODE = mmcode;
                        me_docd.APPQTY = item.SUP_USEQTY;
                        me_docd.HIGH_QTY = high_qty;
                        me_docd.S_INV_QTY = repo.GetSinvqtyWithMmcode(towh, mmcode);
                        me_docd.INV_QTY = repo.GetInvqtyWithMmcode(towh, mmcode);
                        me_docd.CHINNAME = item.SUP_PATNAME;
                        me_docd.CHARTNO = item.SUP_MEDNO;
                        me_docd.CREATE_USER = update_user;
                        me_docd.UPDATE_USER = update_user;
                        me_docd.UPDATE_IP = update_ip;
                        update_cnt += repo.CreateD(me_docd);

                        // 步驟2：update HIS14_SUPDET
                        update_cnt += repo.Update_HIS14_SUPDET_DOCNO(item.SUPDET_SEQ, docno, update_user, update_ip); ;
                    }
                    session.Result.afrs = update_cnt;
                    DBWork.Commit();
                }
                catch (Exception ex)
                {
                    session.Result.success = false;
                    session.Result.msg = ex.Message;
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        } // 

        // 查詢
        [HttpPost]
        public ApiResponse AllT7Grid(FormDataCollection form)
        {
            HIS14_SUPDET q = new HIS14_SUPDET();
            q.SUP_USEDATE_S = form.Get("SUP_USEDATE_S"); // 消耗日期開始
            q.SUP_USEDATE_E = form.Get("SUP_USEDATE_E"); // 消耗日期結束
            q.MMCODE_OR_MMNAME_C = form.Get("MMCODE_OR_MMNAME_C"); // 院內碼或中文品名
            q.SUP_PATIDNO_OR_SUP_PATNAME = form.Get("SUP_PATIDNO_OR_SUP_PATNAME"); // 病患證號或姓名
            q.SUP_FEATOPID_OR_SUP_EMPNAME = form.Get("SUP_FEATOPID_OR_SUP_EMPNAME"); // 醫師代碼或姓名

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0118Repository(DBWork);
                    var v_inid = User.Identity.Name; //'admin', repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.AllT7Grid(q, v_inid, page, limit, sorters);
                }
                catch (Exception ex)
                {
                    throw;
                }
                return session.Result;
            }
        } // end of AllT7Grid
        // 查詢
        [HttpPost]
        public ApiResponse AllT8Grid(FormDataCollection form)
        {
            var SECTIONNO = form.Get("SECTIONNO"); // 科別
            var MMCODE = form.Get("MMCODE"); // 院內碼
            var page = int.Parse(form.Get("page"));
            //var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0118Repository(DBWork);
                    var v_inid = User.Identity.Name; //repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.AllT8Grid(SECTIONNO, MMCODE, page, limit, sorters);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        } // end of AllT8Grid


        [HttpPost]
        public ApiResponse GetCombo科別()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    session.Result.etts = repo.GetCombo科別(p0);
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetCombo科別_2()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    session.Result.etts = repo.GetCombo科別_2(p0);
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetCombo院內碼()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    session.Result.etts = repo.GetCombo院內碼(p0);
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmCodeCombo_2(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0118Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo_2(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmCodeCombo_3(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0118Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo_3(p0, page, limit, "");
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        // 新增
        [HttpPost]
        public ApiResponse Create_SEC_USEMM(HIS14_SUPDET v)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0118Repository(DBWork);
                    if (!repo.CheckExists_SEC_USEMM(v)) // 新增前檢查主鍵是否已存在
                    {
                        // v.SECTIONNO = sectionno;
                        // v.MMCODE = mmcode;
                        v.CREATE_USER = User.Identity.Name;
                        v.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create_SEC_USEMM(v); // repo.CreateM(me_docm);
                        session.Result.etts = repo.AllT8Grid(v.SECTIONNO, v.MMCODE, 1, 10, "[{\"property\":\"MMCODE\", \"direction\":\"\"}]");
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        String sMsg = "";

                        sMsg += "<span style='color:red'>";
                        sMsg += "科別='" + v.SECTIONNO + "', "; //me_docm.SECTIONNO
                        sMsg += "院內碼='" + v.MMCODE + "' ";
                        sMsg += "</span>";
                        sMsg += "，已存在於設定檔。";
                        session.Result.msg = sMsg;
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
        } // 

        // 刪除
        [HttpPost]
        public ApiResponse Delete_SEC_USEMM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    string sectionnos = form.Get("SECTIONNOS");
                    string mmcodes = form.Get("MMCODES");

                    if (
                        !String.IsNullOrEmpty(sectionnos) &&
                        !String.IsNullOrEmpty(mmcodes)
                    )
                    {
                        string[] ary_sectionno = sectionnos.Split(',');
                        string[] ary_mmcode = mmcodes.Split(',');
                        for (int i = 0; i < ary_sectionno.Length; i++)
                        {
                            string sectionno = ary_sectionno[i];
                            string mmcode = ary_mmcode[i];
                            if (
                                !String.IsNullOrEmpty(sectionno) &&
                                !String.IsNullOrEmpty(mmcode)
                            )
                            {
                                HIS14_SUPDET v = new HIS14_SUPDET();
                                v.SECTIONNO = sectionno;
                                v.MMCODE = mmcode;
                                session.Result.afrs = repo.Delete_SEC_USEMM(v);
                            }
                        } // end of for (int i = 0; i < ary_sectionno.Length; i++)
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

        #endregion // end of flylon

        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var d0 = form.Get("d0");
            var d1 = form.Get("d1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            string[] arrP2 = { };
            if (!string.IsNullOrEmpty(p2))
            {
                arrP2 = p2.Trim().Split(',');
            }
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0118Repository(DBWork);
                    string v_user = User.Identity.Name;
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetAllM(p0, d0, d1, arrP2, p3, p4, v_user, hospCode, page, limit, sorters);
                }
                catch (Exception ex)
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
                    var repo = new AB0118Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetAllD(p0, p1, hospCode, page, limit, sorters);
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
                    var repo = new AB0118Repository(DBWork);
                    string v_docno = repo.GetDailyDocno(); // 取得每日單號 select GET_DAILY_DOCNO from DUAL 
                    if (!repo.CheckExists(v_docno)) // 新增前檢查主鍵是否已存在
                    {
                        if (repo.CheckIsTowhCancelByWhno(me_docm.TOWH)) // 依庫房代碼(WH_NO)查庫房基本檔 是否申請庫房已作廢 select 1 from MI_WHMAST where cancel_id = 'N' and wh_no = :towh 
                        {
                            session.Result.success = false;
                            session.Result.msg = "申請庫房已作廢，請重新選擇";
                            return session.Result;
                        }

                        var v_matclass = me_docm.MAT_CLASS;
                        var v_whno = repo.GetFrwh();
                        var v_applykind = me_docm.APPLY_KIND;
                        var v_doctype = "MR5";
                        if (v_matclass == "02")
                        {
                            v_doctype = "MR6";
                        }

                        me_docm.DOCNO = v_docno;
                        me_docm.DOCTYPE = v_doctype;
                        me_docm.APPID = User.Identity.Name;
                        me_docm.FRWH = v_whno;
                        me_docm.APPLY_KIND = "2";
                        me_docm.USEID = User.Identity.Name;
                        me_docm.APPDEPT = DBWork.UserInfo.Inid;
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
                catch
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
                    var repo = new AB0118Repository(DBWork);
                    string errMsg = "";
                    string towh = repo.GetTowh(me_docd.DOCNO);
                    string frwh_d = repo.GetFrwhWithMmcode(towh, me_docd.MMCODE);
                    bool frwhIsSupply = repo.GetFrwhIsSupply(towh);  //上級庫是否為供應中心

                    bool flowIdValid = repo.ChceckFlowId01(me_docd.DOCNO);
                    if (flowIdValid == false)
                        errMsg += "申請單狀態已變更，請重新查詢" + "</br>";
                    else if (repo.CheckWHMM(me_docd.MMCODE, towh) == false)
                        errMsg += "<span style='color:red'>本庫房無法申領此院內碼</span>，請確認。" + "</br>";

                    IEnumerable<ME_DOCD> e_docds = repo.CheckMApplyidE(me_docd.DOCNO, false);
                    if (e_docds.Any())
                        errMsg += "<span style='color:red'>此院內碼不可申領</span>，請確認。" + "</br>";

                    if (!repo.CheckMmcode(me_docd.MMCODE, me_docd.DOCNO))
                        errMsg += "<span style='color:red'>此院內碼「" + me_docd.MMCODE + "」不正確</span>，請確認。" + "</br>";

                    //1120613骨科需允許重複院內碼但會將病歷號碼寫在備註，故改判斷院內碼+備註
                    //1121009 改判斷院內碼+備註+病人姓名+病歷號
                    //1121026 改判斷院內碼+病人姓名+病歷號
                    if (repo.CheckExistsMM(me_docd.DOCNO, me_docd.MMCODE, me_docd.CHINNAME, me_docd.CHARTNO))
                        errMsg += "<span style='color:red'>此單明細已有重複的院內碼「" + me_docd.MMCODE + "」+病人姓名+病歷號</span>，請確認。" + "</br>";
                    //花蓮需求
                    else if (repo.CheckMaxAppqtyFlag(me_docd.MMCODE, towh, me_docd.APPQTY, frwhIsSupply) == "Y")
                        errMsg += "<span style='color:red'>此院內碼「" + me_docd.MMCODE + "」</span>申請量不可超過基準量(庫房設定的單位請領量)，請確認。" + "</br>";


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
                    //ME_DOCD.APVID = User.Identity.Name;
                    //ME_DOCD.ACKID = User.Identity.Name;
                    me_docd.SRCDOCNO = me_docd.DOCNO;
                    me_docd.FRWH_D = repo.GetFrwhWithMmcode(repo.GetThisTowh(me_docd.DOCNO), me_docd.MMCODE);
                    me_docd.CREATE_USER = User.Identity.Name;
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.CreateD(me_docd);
                    if (repo.CheckAppid(me_docd.DOCNO))
                    {
                        session.Result.afrs = repo.UpdateAppid(User.Identity.Name, DBWork.ProcIP, me_docd.DOCNO);
                    }
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetD(me_docd.DOCNO, me_docd.SEQ, hospCode);
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
        public ApiResponse UpdateM(ME_DOCM me_docm)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0118Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(me_docm.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    me_docm.APPID = User.Identity.Name;
                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;
                    string hospCode = repo.GetHospCode();
                    session.Result.afrs = repo.UpdateM(me_docm);
                    session.Result.etts = repo.GetM(me_docm.DOCNO, hospCode);

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
        public ApiResponse UpdateD(ME_DOCD me_docd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0118Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(me_docd.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }
                    //1120613骨科需允許重複院內碼但會將病歷號碼寫在備註，故改判斷院內碼+備註
                    //1121009 改判斷院內碼+備註+病人姓名+病歷號
                    //1121026 改判斷院內碼+病人姓名+病歷號
                    if ((me_docd.CHINNAME != me_docd.CHINNAME_OLD) || (me_docd.CHARTNO != me_docd.CHARTNO_OLD))
                    {
                        if (repo.CheckExistsMM(me_docd.DOCNO, me_docd.MMCODE, me_docd.CHINNAME, me_docd.CHARTNO))
                        {
                            session.Result.msg = "<span style='color:red'>此單明細已有重複的院內碼「" + me_docd.MMCODE + "」+病人姓名+病歷號</span>，請確認。";
                            session.Result.success = false;
                            return session.Result;
                        }
                    }
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(me_docd);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetD(me_docd.DOCNO, me_docd.SEQ, hospCode);

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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        HashSet<string> docno_Hs = new HashSet<string>();

                        foreach (string s in tmp)
                        {
                            if (!docno_Hs.Contains(s))
                            {
                                docno_Hs.Add(s);
                            }
                        }

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckIsTowhCancelByDocno(tmp[i]))
                            {
                                session.Result.success = false;
                                session.Result.msg = string.Format("申請單號「{0}」申請庫房已作廢，請重新確認", tmp[i]);
                                return session.Result;
                            }

                            if (repo.CheckDuplicateMmcode(tmp[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = string.Format("申請單號「{0}」<span style='color:red'>品項院內碼+病患姓名+病歷號 重複</span>，請修改後再提出申請", tmp[i]);
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
                                session.Result.msg = string.Format("申請單號「{0}」<span style='color:red'>有已作廢品項</span>，請修改後再提出申請，品項如下：<br>{1}",
                                                                    tmp[i], error_mmcodes);
                                return session.Result;
                            }
                            // 2020-08-15 新增: 檢查時否有不可申領品項
                            IEnumerable<ME_DOCD> e_docds = repo.CheckMApplyidE(tmp[i], false);
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
                                session.Result.msg = string.Format("申請單號「{0}」<span style='color:red'>有不可申領品項</span>，請修改後再提出申請，品項如下：<br>{1}",
                                                                    tmp[i], error_mmcodes);
                                return session.Result;
                            }
                            // 2021-02-20 新增: 檢查時否有不可申領品項
                            IEnumerable<ME_DOCD> whmm_docds = repo.CheckApplyWHMM(tmp[i]);
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
                                session.Result.msg = string.Format("申請單號「{0}」<span style='color:red'>有本庫房不可申領品項</span>，請修改後再提出申請，品項如下：<br>{1}",
                                                                    tmp[i], error_mmcodes);
                                return session.Result;
                            }

                            // 2022-11-16 新增：檢查是否所有品項申請量都為申請倍數
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
                            if (exceed_docds.Any())
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
                                if (string.IsNullOrEmpty(error_mmcodes) == false)
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = string.Format("申請單號「{0}」<span style='color:red'>申請量不可超過基準量(庫房設定的單位請領量)</span>，品項如下：<br>{1}",
                                                                        tmp[i], error_mmcodes);
                                    return session.Result;
                                }
                            }

                            if (repo.CheckExistsM(tmp[i], "1"))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>此申請單狀態非申請中</span>不得提出申請。";
                            }
                            else
                            {
                                if (repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                                {
                                    if (repo.CheckExistsDN(tmp[i]))
                                    {
                                        session.Result.afrs = 0;
                                        session.Result.success = false;
                                        session.Result.msg = "<span style='color:red'>此單明細尚有申請數量為0</span>不得提出申請。";
                                    }
                                    else
                                    {
                                        ME_DOCM me_docm = new ME_DOCM();
                                        me_docm.DOCNO = tmp[i];
                                        me_docm.TOWH = repo.getDocTowh(tmp[i]);
                                        me_docm.MAT_CLASS = repo.getDocMatclass(tmp[i]);
                                        if (me_docm.MAT_CLASS == "02")
                                            me_docm.DOCTYPE = "MR6";
                                        else
                                            me_docm.DOCTYPE = "MR5";
                                        me_docm.ISARMY = repo.getDocIsarmy(tmp[i]);
                                        me_docm.APPUNA = repo.getDocAppuna(tmp[i]);
                                        me_docm.FLOWID = "11";  //未核可
                                        me_docm.SENDAPVID = User.Identity.Name;
                                        me_docm.UPDATE_USER = User.Identity.Name;
                                        me_docm.UPDATE_IP = DBWork.ProcIP;


                                        // 804(桃園)需額外依庫備/非庫備拆單; 其他則仍使用FRWH_D和M_CONTID拆單
                                        string hospCode = repo.GetHospCode();
                                        bool procMstoreid = false;
                                        if (hospCode == "804")
                                            procMstoreid = true; // true的時候增加M_STOREID的處理

                                        IEnumerable<ME_DOCD> myEnum = repo.GetSplitValue(me_docm.DOCNO, procMstoreid);
                                        myEnum.GetEnumerator();
                                        string item_FRWH = "", item_MCONTID = "", item_MSTOREID = "";
                                        foreach (var item in myEnum)
                                        {
                                            // 第一個核撥庫房&合約識別
                                            if (item_FRWH == "" && item_MCONTID == "" && procMstoreid == false)
                                            {
                                                me_docm.FRWH = item.FRWH_D;
                                                me_docm.M_CONTID = item.M_CONTID;
                                                repo.MasterUpdateFrwhMcontid(me_docm);
                                            }
                                            // 第一個核撥庫房&合約識別&庫備識別(for 桃園)
                                            else if (item_FRWH == "" && item_MCONTID == "" && item_MSTOREID == "" && procMstoreid == true)
                                            {
                                                me_docm.FRWH = item.FRWH_D;
                                                me_docm.M_CONTID = item.M_CONTID;
                                                me_docm.M_STOREID = item.M_STOREID;
                                                repo.MasterUpdateFrwhMcontid(me_docm, procMstoreid);
                                            }
                                            else
                                            {
                                                // 拆單,並新建單號
                                                ME_DOCM me_docm_new = new ME_DOCM();
                                                me_docm_new.DOCNO = repo.GetDailyDocno();

                                                //加入單列表
                                                if (!docno_Hs.Contains(me_docm_new.DOCNO))
                                                {
                                                    docno_Hs.Add(me_docm_new.DOCNO);
                                                }

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
                                                me_docm_new.MAT_CLASS = me_docm.MAT_CLASS;
                                                me_docm_new.SRCDOCNO = me_docm_new.DOCNO;
                                                me_docm_new.ISARMY = me_docm.ISARMY;
                                                me_docm_new.APPUNA = me_docm.APPUNA;
                                                me_docm_new.M_CONTID = item.M_CONTID;
                                                if (procMstoreid == true) // 增加M_STOREID的處理
                                                    me_docm_new.M_STOREID = item.M_STOREID;
                                                repo.CreateM(me_docm_new, procMstoreid);

                                                // 將屬於新單號的項次,修改為新單號
                                                if (procMstoreid == true) // 增加M_STOREID的處理
                                                    repo.DetailUpdateDocno(tmp[i], item.FRWH_D, item.M_CONTID, item.M_STOREID, me_docm_new.DOCNO, User.Identity.Name, DBWork.ProcIP);
                                                else
                                                    repo.DetailUpdateDocno(tmp[i], item.FRWH_D, item.M_CONTID, me_docm_new.DOCNO, User.Identity.Name, DBWork.ProcIP);

                                                // 以便後面UpdateStatus更新FLOWID用
                                                me_docm.DOCNO = me_docm_new.DOCNO;
                                            }
                                            item_FRWH = item.FRWH_D;
                                            item_MCONTID = item.M_CONTID;
                                            if (procMstoreid == true) // 增加M_STOREID的處理
                                                item_MSTOREID = item.M_STOREID;

                                            #region (for 桃園)衛材非合約申請單若超過15萬需拆單
                                            // (for 桃園)衛材非合約申請單若超過15萬需拆單
                                            if (hospCode == "804" && me_docm.MAT_CLASS == "02" && item.M_CONTID == "2")
                                            {
                                                // 新建申請單template
                                                ME_DOCM me_docm_temp = new ME_DOCM();
                                                me_docm_temp.CREATE_USER = User.Identity.Name;
                                                me_docm_temp.UPDATE_USER = User.Identity.Name;
                                                me_docm_temp.UPDATE_IP = DBWork.ProcIP;
                                                me_docm_temp.APPID = User.Identity.Name;
                                                me_docm_temp.APPLY_KIND = "2";
                                                me_docm_temp.APPDEPT = DBWork.UserInfo.Inid;
                                                me_docm_temp.USEDEPT = DBWork.UserInfo.Inid;
                                                me_docm_temp.USEID = User.Identity.Name;
                                                me_docm_temp.TOWH = me_docm.TOWH;        // 申請庫房
                                                me_docm_temp.FRWH = item.FRWH_D;        // 核撥庫房
                                                me_docm_temp.DOCTYPE = me_docm.DOCTYPE;
                                                me_docm_temp.FLOWID = me_docm.FLOWID;
                                                me_docm_temp.MAT_CLASS = me_docm.MAT_CLASS;
                                                me_docm_temp.SRCDOCNO = me_docm_temp.DOCNO;  //不確定這段有沒有作用

                                                //加入單列表
                                                if (!docno_Hs.Contains(me_docm_temp.DOCNO))
                                                {
                                                    docno_Hs.Add(me_docm_temp.DOCNO);
                                                }

                                                me_docm_temp.ISARMY = me_docm.ISARMY;
                                                me_docm_temp.APPUNA = me_docm.APPUNA;
                                                me_docm_temp.M_CONTID = item.M_CONTID;
                                                if (procMstoreid == true) // 增加M_STOREID的處理
                                                    me_docm_temp.M_STOREID = item.M_STOREID;

                                                int sumAmt = 0;
                                                foreach (ME_DOCD mmitem in repo.GetDocItems(me_docm.DOCNO, item.M_CONTID, me_docm_temp.M_STOREID))
                                                {
                                                    // 如果有單品項單價>=15萬, 則該品項依照申請數量拆單, 每單只放一個
                                                    if (Convert.ToInt32(mmitem.DISC_UPRICE) > 150000)
                                                    {
                                                        for (int j = 0; j < Convert.ToInt32(mmitem.APPQTY); j++)
                                                        {
                                                            me_docm_temp.DOCNO = repo.GetDailyDocno();
                                                            me_docm_temp.SRCDOCNO = me_docm_temp.DOCNO;

                                                            //加入單列表
                                                            if (!docno_Hs.Contains(me_docm_temp.DOCNO))
                                                            {
                                                                docno_Hs.Add(me_docm_temp.DOCNO);
                                                            }

                                                            repo.CreateM(me_docm_temp, procMstoreid);

                                                            // 最後一項則將原始項目修改為新單號; 否則複製項目到新單號
                                                            if (j == Convert.ToInt32(mmitem.APPQTY) - 1)
                                                                repo.DetailUpdateDocnoBySeq(me_docm.DOCNO, mmitem.SEQ, me_docm_temp.DOCNO, "1", User.Identity.Name, DBWork.ProcIP);
                                                            else
                                                                repo.CopyD(me_docm.DOCNO, mmitem.SEQ, me_docm_temp.DOCNO, "1", User.Identity.Name, DBWork.ProcIP);

                                                            // 提出申請更新me_docd
                                                            repo.ApplyD(me_docm_temp);

                                                            // 狀態更新
                                                            repo.ApplyM(me_docm_temp);
                                                        }
                                                    }
                                                    // 如果有單品項申請金額>=15萬, 則該品項獨立拿出來拆
                                                    else if (Convert.ToInt32(mmitem.APP_AMT) > 150000)
                                                    {
                                                        int splitQty = 0;
                                                        for (int j = 0; j < Convert.ToInt32(mmitem.APPQTY); j++)
                                                        {
                                                            sumAmt += Convert.ToInt32(mmitem.DISC_UPRICE);
                                                            // 累計單價金額達到15萬時拆單
                                                            if (sumAmt >= 150000)
                                                            {
                                                                splitQty = j; // 每單數量為j個
                                                                sumAmt = 0;
                                                                break;
                                                            }
                                                        }

                                                        int splitCnt = Convert.ToInt32(mmitem.APPQTY) / splitQty; // 需分成幾單
                                                        int splitRemain = Convert.ToInt32(mmitem.APPQTY) % splitQty; // 分完後的剩餘量
                                                        if (splitRemain > 0)
                                                            splitCnt++; // 若有餘數則需多一單放餘量

                                                        for (int j = 0; j < splitCnt; j++)
                                                        {
                                                            me_docm_temp.DOCNO = repo.GetDailyDocno();
                                                            //加入單列表
                                                            if (!docno_Hs.Contains(me_docm_temp.DOCNO))
                                                            {
                                                                docno_Hs.Add(me_docm_temp.DOCNO);
                                                            }

                                                            me_docm_temp.SRCDOCNO = me_docm_temp.DOCNO;
                                                            repo.CreateM(me_docm_temp, procMstoreid);

                                                            // 最後一項則將原始項目修改為新單號; 否則複製項目到新單號
                                                            if (j == splitCnt - 1)
                                                            {
                                                                // 若有餘數則最後一項的數量是splitRemain,否則為splitQty
                                                                if (splitRemain > 0)
                                                                    repo.DetailUpdateDocnoBySeq(me_docm.DOCNO, mmitem.SEQ, me_docm_temp.DOCNO, splitRemain.ToString(), User.Identity.Name, DBWork.ProcIP);
                                                                else
                                                                    repo.DetailUpdateDocnoBySeq(me_docm.DOCNO, mmitem.SEQ, me_docm_temp.DOCNO, splitQty.ToString(), User.Identity.Name, DBWork.ProcIP);
                                                            }
                                                            else
                                                                repo.CopyD(me_docm.DOCNO, mmitem.SEQ, me_docm_temp.DOCNO, splitQty.ToString(), User.Identity.Name, DBWork.ProcIP);

                                                            // 提出申請更新me_docd
                                                            repo.ApplyD(me_docm_temp);

                                                            // 狀態更新
                                                            repo.ApplyM(me_docm_temp);
                                                        }
                                                    }
                                                    // 其餘品項,累計金額達到15萬時拆新的單
                                                    else
                                                    {
                                                        // 還沒有累計金額時建立新單
                                                        if (sumAmt == 0)
                                                        {
                                                            me_docm_temp.DOCNO = repo.GetDailyDocno();

                                                            //加入單列表
                                                            if (!docno_Hs.Contains(me_docm_temp.DOCNO))
                                                            {
                                                                docno_Hs.Add(me_docm_temp.DOCNO);
                                                            }

                                                            me_docm_temp.SRCDOCNO = me_docm_temp.DOCNO;
                                                            repo.CreateM(me_docm_temp, procMstoreid);
                                                        }

                                                        sumAmt += Convert.ToInt32(mmitem.APP_AMT);
                                                        // 累計金額達到15萬時更新申請單狀態並拆新單
                                                        if (sumAmt >= 150000)
                                                        {
                                                            // 提出申請更新me_docd
                                                            repo.ApplyD(me_docm_temp);

                                                            // 狀態更新
                                                            repo.ApplyM(me_docm_temp);

                                                            // 新單累計金額初始值為目前處理項目的申請金額
                                                            sumAmt = Convert.ToInt32(mmitem.APP_AMT);

                                                            me_docm_temp.DOCNO = repo.GetDailyDocno();

                                                            //加入單列表
                                                            if (!docno_Hs.Contains(me_docm_temp.DOCNO))
                                                            {
                                                                docno_Hs.Add(me_docm_temp.DOCNO);
                                                            }

                                                            me_docm_temp.SRCDOCNO = me_docm_temp.DOCNO;
                                                            repo.CreateM(me_docm_temp, procMstoreid);

                                                            // 把目前處理的項目分到新的申請單
                                                            repo.DetailUpdateDocnoBySeq(me_docm.DOCNO, mmitem.SEQ, me_docm_temp.DOCNO, mmitem.APPQTY, User.Identity.Name, DBWork.ProcIP);
                                                        }
                                                        else
                                                        {
                                                            // 把目前處理的項目分到處理中的申請單
                                                            repo.DetailUpdateDocnoBySeq(me_docm.DOCNO, mmitem.SEQ, me_docm_temp.DOCNO, mmitem.APPQTY, User.Identity.Name, DBWork.ProcIP);
                                                        }
                                                    }
                                                }

                                                // 刪除所有品項都已移到新單的空白申請單
                                                if (repo.GetDocdCnt(me_docm.DOCNO) == 0)
                                                {
                                                    repo.DeleteM(me_docm.DOCNO);

                                                    //從單列表刪除
                                                    if (!docno_Hs.Contains(me_docm.DOCNO))
                                                    {
                                                        docno_Hs.Remove(me_docm.DOCNO);
                                                    }

                                                }
                                            }
                                            #endregion

                                            // 提出申請更新me_docd
                                            session.Result.afrs = repo.ApplyD(me_docm);

                                            // 狀態更新
                                            session.Result.afrs = repo.ApplyM(me_docm);
                                        }

                                        if (hospCode == "804")
                                        {
                                            // 若原始申請單已無品項則刪除
                                            if (repo.GetDocdCnt(tmp[i]) == 0)
                                            {
                                                repo.DeleteM(tmp[i]);

                                                //從單列表刪除
                                                if (!docno_Hs.Contains(tmp[i]))
                                                {
                                                    docno_Hs.Remove(tmp[i]);
                                                }
                                            }
                                        }
                                    }
                                    session.Result.etts = docno_Hs;
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO");
                        string note = form.Get("NOTE");
                        string doctype = form.Get("DOCTYPE");
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
            var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0118Repository(DBWork);
                    session.Result.etts = repo.GetPackD(p0, p1, p2, p3, p4, page, limit, sorters);
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
                    var repo = new AB0118Repository(DBWork);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                            string frwh_d = repo.GetFrwhWithMmcode(towh, tmp_mmcode[i]);
                            bool frwhIsSupply = repo.GetFrwhIsSupply(towh);  //上級庫是否為供應中心
                            if (repo.CheckMaxAppqtyFlag(tmp_mmcode[i], towh, tmp_appqty[i], frwhIsSupply) == "Y")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = string.Format("<span style='color:red'>申請量不可超過基準量(庫房設定的單位請領量)，請確認", tmp_mmcode[i]);
                                return session.Result;
                            }
                            if (repo.CheckExistsMM(tmp_docno[i], tmp_mmcode[i], null, null))
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
                                me_docd.FRWH_D = frwh_d;
                                //me_docd.FRWH_D = repo.GetFrwhWithMmcode(repo.GetThisTowh(me_docd.DOCNO), me_docd.MMCODE);
                                me_docd.SEQ = v_seq;
                                me_docd.MMCODE = tmp_mmcode[i];
                                me_docd.APPQTY = tmp_appqty[i];
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    var v_inid = repo.GetUridInid(User.Identity.Name);
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
            var p1 = form.Get("p1");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
        public ApiResponse GetFlowidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
            var p5 = form.Get("p5");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, p2, p3, p4, p5, page, limit, "");
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    AB0118Repository.MI_MAST_QUERY_PARAMS query = new AB0118Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS").ToUpper();
                    query.ISCONTID3 = form.Get("ISCONTID3") == null ? "" : form.Get("ISCONTID3").ToUpper();
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                    string[] arr = { "院內碼", "申請數量", "備註", "病患姓名", "病歷號" };

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
                            me_docd.APLYITEM_NOTE = newTable.Rows[i]["備註"].ToString().Trim();
                            me_docd.CHINNAME = newTable.Rows[i]["病患姓名"].ToString().Trim();
                            me_docd.CHARTNO = newTable.Rows[i]["病歷號"].ToString().Trim();
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
                                //擷取基準量
                                string high_qty = repo.GetOperqtyWithMmcode(towh, me_docd.MMCODE);
                                if (high_qty == null) { high_qty = "0"; }

                                bool frwhIsSupply = repo.GetFrwhIsSupply(towh);  //上級庫是否為供應中心
                                //檢核庫房代碼
                                if (repo.CheckExistsMMCODE(me_docd.MMCODE) != true)
                                {
                                    me_docd.CHECK_RESULT = "此院內碼不存在";
                                }
                                //else if (repo.CheckExistsMMCODE2(me_docd.MMCODE) != true)
                                //{
                                //    me_docd.CHECK_RESULT = "此院內碼不適用庫備申請";
                                //}
                                else if (repo.CheckMatClassMMCODE(v_mat_class, me_docd.MMCODE) != true)
                                {
                                    me_docd.CHECK_RESULT = "此院內碼不適用申請的物料分類";
                                }
                                else if (repo.Checkappqty(me_docd.MMCODE, me_docd.APPQTY) == true)
                                {
                                    me_docd.CHECK_RESULT = "申請量必須符合申請倍數";
                                }
                                else if (repo.CheckExistsMM(v_docno, me_docd.MMCODE, me_docd.CHINNAME, me_docd.CHARTNO) == true)
                                {
                                    me_docd.CHECK_RESULT = "此單明細已有重複的院內碼+備註+病人姓名+病歷號";
                                }
                                else if (repo.CheckWHMM(me_docd.MMCODE, towh) == false)
                                {
                                    me_docd.CHECK_RESULT = "本庫房無法申領此院內碼";
                                }
                                else if (repo.CheckMaxAppqtyFlag(me_docd.MMCODE, towh, me_docd.APPQTY, frwhIsSupply) == "Y")
                                {
                                    me_docd.CHECK_RESULT = "申請量不可超過基準量(庫房設定的單位請領量)";
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    JCLib.Excel.Export("AB0118.xls", repo.GetExcel());
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    JCLib.Excel.Export(p0 + ".xls", repo.GetDetailExcel(p0));
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
                    var repo = new AB0118Repository(DBWork);
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
                            string towh = repo.GetTowh(data.DOCNO);
                            data.SRCDOCNO = v_docno;
                            data.FRWH_D = repo.GetFrwhWithMmcode(repo.GetThisTowh(data.DOCNO), data.MMCODE);
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
                        catch
                        {
                            throw;
                        }
                        me_docd_list.Add(data);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
        public ApiResponse GetIscontid3Combo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    session.Result.etts = repo.GetYN();
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
                                me_docm.FLOWID = "1";  //申請中
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
        public ApiResponse GetLastHIS14SUPDET()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    session.Result.msg = repo.GetLastHIS14SUPDET();
                    session.Result.success = true;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Clear_Temp_HIS14_SUPDET(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
                    session.Result.afrs = repo.Clear_Temp_HIS14_SUPDET(User.Identity.Name);
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
        public ApiResponse CheckUnitrateFlg(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0118Repository repo = new AB0118Repository(DBWork);
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
    }
}
