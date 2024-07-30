using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0183Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var P0 = form.Get("p0");
            var P1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0183Repository(DBWork);
                    session.Result.etts = repo.GetAllM(P0, P1, page, limit, sorters);
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0183Repository repo = new AA0183Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Apply(FormDataCollection form)
        {
            var v_frwh = form.Get("FRWH");
            var v_towh = form.Get("TOWH");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0183Repository repo = new AA0183Repository(DBWork);
                    var frwh_kind = repo.GetWhkind(v_frwh);
                    var towh_kind = repo.GetWhkind(v_towh);

                    

                    if (form.Get("MMCODE") != "")
                    {
                       
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                        string appqty = form.Get("APPQTY").Substring(0, form.Get("APPQTY").Length - 1);
                        string[] tmp_mmcode = mmcode.Split(',');
                        string[] tmp_appqty = appqty.Split(',');

                        //2023-10-05: 檢查庫房類別與院內碼是否匹配

                        for (int i = 0; i < tmp_mmcode.Length; i++) {
                            string mat_class = repo.GetMatClass(tmp_mmcode[i]);
                            if (string.IsNullOrEmpty(mat_class))
                            {
                                session.Result.success = false;
                                session.Result.msg = "院內碼未設定物料類別，請重新確認";
                                return session.Result;
                            }
                            if (mat_class == "02" && frwh_kind != "1")
                            {
                                session.Result.success = false;
                                session.Result.msg = "出庫庫房已重新選擇，請重新查詢";
                                return session.Result;
                            }
                            if (mat_class == "01" && frwh_kind != "0")
                            {
                                session.Result.success = false;
                                session.Result.msg = "出庫庫房已重新選擇，請重新查詢";
                                return session.Result;
                            }
                        }

                        //新增ME_DOCM
                        var v_docno = repo.GetDocno();
                        var v_inid = repo.GetUridInid(User.Identity.Name);
                        ME_DOCM me_docm = new ME_DOCM();
                        me_docm.DOCNO = v_docno;
                        if (frwh_kind == "0")
                        {
                            me_docm.DOCTYPE = "MR";
                            me_docm.MAT_CLASS = "01";
                            me_docm.FLOWID = "0102";
                            me_docm.APPLY_NOTE = "藥庫批次核撥";
                        }
                        else
                        {
                            me_docm.DOCTYPE = "MR2";
                            me_docm.MAT_CLASS = "02";
                            me_docm.FLOWID = "2";
                            me_docm.APPLY_NOTE = "衛材庫批次核撥";
                        }
                        me_docm.APPID = User.Identity.Name;
                        me_docm.APPDEPT = v_inid;
                        me_docm.USEID = User.Identity.Name;
                        me_docm.USEDEPT = v_inid;
                        me_docm.TOWH = v_towh;
                        me_docm.FRWH = v_frwh;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        repo.CreateM(me_docm);
                        //針對勾選資料逐筆處理
                        ME_DOCD me_docd = new ME_DOCD();
                        var v_pack_unit = "";
                        var v_pack_times = "";
                        for (int i = 0; i < tmp_mmcode.Length; i++)
                        {
                            me_docd.DOCNO = v_docno;
                            if (!repo.CheckExistsD(v_docno)) // 新增前檢查主鍵是否已存在
                            {
                                me_docd.SEQ = "1";
                            }
                            else
                            {
                                me_docd.SEQ = repo.GetDocDSeq(v_docno);
                            }
                            me_docd.MMCODE = tmp_mmcode[i];
                            me_docd.APPQTY = tmp_appqty[i];
                            me_docd.APVQTY = tmp_appqty[i];
                            me_docd.APVID = User.Identity.Name;
                            me_docd.ACKQTY = tmp_appqty[i];
                            me_docd.ACKID = User.Identity.Name;
                            me_docd.S_INV_QTY = repo.GetInvQty(v_frwh, tmp_mmcode[i]);
                            me_docd.INV_QTY = repo.GetInvQty(v_towh, tmp_mmcode[i]);
                            me_docd.SAFE_QTY = repo.GetSafeQty(v_towh, tmp_mmcode[i]);
                            me_docd.OPER_QTY = repo.GetOperQty(v_towh, tmp_mmcode[i]);
                            if (!repo.CheckPackUnit(v_towh, tmp_mmcode[i])) 
                            {
                                v_pack_unit = "";
                                v_pack_times = "1";
                            }
                            else
                            {
                                v_pack_unit = repo.GetPackUnit(v_towh, tmp_mmcode[i]);
                                v_pack_times = repo.GetPackTimes(v_towh, tmp_mmcode[i]);
                            }
                            me_docd.PACK_UNIT = v_pack_unit;
                            me_docd.PACK_TIMES = v_pack_times;
                            me_docd.E_ORDERDCFLAG = repo.GetEOrderdcflag(tmp_mmcode[i]);

                            if (frwh_kind == "0")
                            {
                                me_docd.APLYITEM_NOTE = "藥庫批次核撥";
                            }
                            else
                            {
                                me_docd.APLYITEM_NOTE = "衛材庫批次核撥";
                            }
                            me_docd.ISSPLIT = "N";
                            me_docd.M_AGENNO = repo.GetMAgenno(tmp_mmcode[i]);
                            me_docd.POSTID = "3";
                            me_docd.EXPT_DISTQTY = tmp_appqty[i];
                            me_docd.PICK_QTY = tmp_appqty[i];
                            me_docd.DIS_USER = User.Identity.Name;
                            me_docd.CREATE_USER = User.Identity.Name;
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            repo.CreateD(me_docd);
                        }

                        //執行SP：POST_DOC(進行核撥)
                        var rtn = repo.CallProc(v_docno, User.Identity.Name, DBWork.ProcIP);
                        if (rtn != "Y")
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>申請單號「" + v_docno + "」</span>，發生核撥執行錯誤，" + rtn + "。";
                        }
                        else
                        {
                            //若【出庫庫房】.WH_KIND ='0'(藥品)，更新ME_DOCD的POSTID為 '4'待點收。
                            if (frwh_kind == "0")
                            {
                                me_docd.DOCNO = v_docno;
                                me_docd.CREATE_USER = User.Identity.Name;
                                me_docd.UPDATE_USER = User.Identity.Name;
                                me_docd.UPDATE_IP = DBWork.ProcIP;
                                repo.UpdateDP(me_docd);
                            }
                            //再次執行POST_DOC(為進行點收)
                            var rtn2 = repo.CallProc(v_docno, User.Identity.Name, DBWork.ProcIP);
                            if (rtn2 != "Y")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>申請單號「" + v_docno + "」</span>，發生點收執行錯誤，" + rtn + "。";
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
        public ApiResponse GetFrwhCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0183Repository repo = new AA0183Repository(DBWork);
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
        public ApiResponse GetTowhCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0183Repository repo = new AA0183Repository(DBWork);
                    var wh_kind = "0";
                    if (p0 != "")
                    {
                        wh_kind = repo.GetWhkind(p0);
                    }
                    session.Result.etts = repo.GetTowhCombo(wh_kind);
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