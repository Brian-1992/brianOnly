using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JCLib.DB;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MMSMSREPORT.MMReport;
using MMSMSREPORT.Models;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;

namespace WebApp.Controllers.AB
{
    public class AB0012Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    AB0012Repository.ME_DOCM_QUERY_PARAMS query = new AB0012Repository.ME_DOCM_QUERY_PARAMS();
                    query.DOCNO = form.Get("p1") == null ? "" : form.Get("p1");
                    query.APPID = form.Get("p2") == null ? "" : form.Get("p2").ToUpper();
                    query.APPDEPT = form.Get("p3") == null ? "" : form.Get("p3").ToUpper();
                    query.TOWH = form.Get("p4") == null ? "" : form.Get("p4").ToUpper();
                    query.FRWH = form.Get("p5") == null ? "" : form.Get("p5").ToUpper();

                    query.DOCTYPE = form.Get("DOCTYPE");
                    query.FLOWID = form.Get("FLOWID") == null ? "" : form.Get("FLOWID");
                    query.APPTIME_S = "";
                    query.APPTIME_E = "";

                    if (form.Get("d0") != null && form.Get("d0") != "")
                        query.APPTIME_S = form.Get("d0").Split('T')[0];  // yyyy-mm-ddT00:00:00
                    if (form.Get("d1") != null && form.Get("d1") != "")
                        query.APPTIME_E = form.Get("d1").Split('T')[0];

                    session.Result.etts = repo.GetAll(query);
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
                    var repo = new AB0012Repository(DBWork);
                    AB0012Repository.ME_DOCD_QUERY_PARAMS query = new AB0012Repository.ME_DOCD_QUERY_PARAMS();
                    query.DOCNO = form.Get("p0");
                    //query.FRWH = form.Get("FRWH") == null ? "" : form.Get("FRWH").ToUpper();
                    //query.TOWH = form.Get("TOWH") == null ? "" : form.Get("TOWH").ToUpper();
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
        public ApiResponse GetMultiMmcode(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    AB0012Repository.MI_MAST_QUERY_PARAMS query = new AB0012Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO");

                    query.FRWH = form.Get("FRWH") == null ? "" : form.Get("FRWH");
                    query.TOWH = form.Get("TOWH") == null ? "" : form.Get("TOWH");
                    query.E_RESTRICTCODE = form.Get("E_RESTRICTCODE") == null ? "" : form.Get("E_RESTRICTCODE");

                    //// 需判斷庫存量>0
                    //if (form.Get("IS_INV") != null && form.Get("IS_INV") == "1")
                    //    query.IS_INV = form.Get("IS_INV");


                    session.Result.etts = repo.GetMultiMmcode(query);
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
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    AB0012Repository.MI_MAST_QUERY_PARAMS query = new AB0012Repository.MI_MAST_QUERY_PARAMS();
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
                    AB0012Repository repo = new AB0012Repository(DBWork);
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
        public ApiResponse GetTowhCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFrwh(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0012Repository(DBWork);
                    session.Result.etts = repo.GetFrwh(form.Get("INID"), form.Get("WH_GRADE"))
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
        public ApiResponse Winctl_AB0012(FormDataCollection form)
        {
            var wh_no = form.Get("WH_NO");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {

                    MMRep_AB0012 ab0012 = new MMRep_AB0012();

                    AB0012Repository repo = new AB0012Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);

                    IEnumerable<ME_DOCD> myEnum = repo.GetWinctl(wh_no);
                    myEnum.GetEnumerator();
                    foreach (var item in myEnum)
                    {
                        // 寫入ME_DOCD
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
                        me_docd.MMCODE = item.MMCODE;
                        me_docd.APPQTY = item.APPQTY;
                        me_docd.FRWH_D = item.FRWH_D;
                        me_docd.SAFE_QTY = item.SAFE_QTY;
                        me_docd.OPER_QTY = item.OPER_QTY;
                        me_docd.PACK_QTY = item.PACK_QTY;
                        me_docd.PACK_UNIT = item.PACK_UNIT;
                        me_docd.E_ORDERDCFLAG = item.E_ORDERDCFLAG;

                        me_docd.CREATE_USER = User.Identity.Name;
                        me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.DetailCreate(me_docd);
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
        public ApiResponse MasterCreate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    ME_DOCM me_docm = new ME_DOCM();
                    me_docm.DOCNO = repo1.GetDocno();

                    if (repo.CheckIsTowhCancelByWhno(form.Get("TOWH")))
                    {
                        session.Result.success = false;
                        session.Result.msg = "申請庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    if (!repo1.CheckExists(me_docm.DOCNO))
                    {
                        me_docm.CREATE_USER = User.Identity.Name;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        me_docm.APPID = User.Identity.Name;
                        me_docm.APPDEPT = form.Get("INID_NAME").Split(' ')[0];
                        me_docm.USEID = User.Identity.Name;
                        me_docm.TOWH = form.Get("TOWH");        // 申請庫房
                        me_docm.FRWH = form.Get("FRWH") == null ? "" : form.Get("FRWH");        // 核撥庫房
                        me_docm.DOCTYPE = "MS";
                        me_docm.FLOWID = "0601";
                        me_docm.MAT_CLASS = "01";

                        session.Result.afrs = repo.MasterCreate(me_docm);
                        //session.Result.etts = repo.MasterGet(me_docm.DOCNO);
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
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;

                    bool flowIdValid = repo.ChceckFlowId01(me_docm.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    if (repo.CheckIsTowhCancelByWhno(me_docm.TOWH))
                    {
                        session.Result.success = false;
                        session.Result.msg = "申請庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    string frwh = repo1.GetFrwh(me_docm.DOCNO);
                    //if (frwh == me_docm.FRWH.Trim()) // 如果核撥庫房一樣,則可以直接更新
                    //    session.Result.afrs = repo.MasterUpdate(me_docm);
                    //else
                    //{
                    if (!repo1.CheckMeDocdExists(me_docm.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                        session.Result.afrs = repo.MasterUpdate(me_docm);
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>申請單號「" + me_docm.DOCNO + "」已存在" + frwh + "庫房院內碼項次，所以無法修改核撥庫房</span><br>如欲修改核撥庫房，請先刪除所有項次。";
                        return session.Result;
                    }
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
                    AB0012Repository repo = new AB0012Repository(DBWork);
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
                    AB0012Repository repo = new AB0012Repository(DBWork);
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
                    //if (repo1.CheckWhmmExists(form.Get("FRWH2").Split(' ')[0], me_docd.MMCODE))
                    //{
                    if (!repo1.CheckMeDocdExists_1(me_docd))
                    {
                        me_docd.CREATE_USER = User.Identity.Name;
                        me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        me_docd.FRWH_D = repo.GetFrwhWithMmcode(repo.GetThisTowh(me_docd.DOCNO), me_docd.MMCODE);
                        session.Result.afrs = repo.DetailCreate(me_docd);

                        // 將第一個院內碼項次的核撥庫房回寫ME_DOCM
                        if (me_docd.SEQ == "1")
                        {
                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = me_docd.DOCNO;
                            me_docm.FRWH = me_docd.FRWH_D;
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            repo.MasterUpdateFrwh(me_docm);
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此申請單已存在此院內碼</span>，請重新輸入院內碼。";
                    }
                    //}
                    //else
                    //{
                    //    session.Result.afrs = 0;
                    //    session.Result.success = false;
                    //    session.Result.msg = "<span style='color:red'>核撥庫房不存放此院內碼</span>，請重新輸入院內碼。";
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
        public ApiResponse DetailCreate_multi(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號

                        string[] tmp_mmcode = mmcode.Split(',');
                        for (int i = 0; i < tmp_mmcode.Length; i++)
                        {
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
                            me_docd.MMCODE = tmp_mmcode[i];
                            me_docd.APPQTY = "0";

                            string towh = repo.GetTowh(me_docd.DOCNO);
                            string tmp = "";
                            tmp = repo.Get_SAFE_OPER_QTY(towh, me_docd.MMCODE);
                            if (tmp != null && tmp.Trim() != "")
                            {
                                string[] tmp_qty = tmp.Split(',');
                                if (tmp_qty.Length > 0)
                                {
                                    me_docd.SAFE_QTY = tmp_qty[0];  // 安全量
                                    me_docd.OPER_QTY = tmp_qty[1];  // 基準量
                                }
                            }
                            tmp = "";
                            tmp = repo.Get_PACK(towh, me_docd.MMCODE);
                            if (tmp != null && tmp.Trim() != "")
                            {
                                string[] tmp_pack = tmp.Split(',');
                                if (tmp_pack.Length > 0)
                                {
                                    me_docd.PACK_QTY = tmp_pack[0]; // 包裝劑量
                                    me_docd.PACK_UNIT = tmp_pack[1]; // 包裝單位
                                }
                            }

                            me_docd.E_ORDERDCFLAG = repo.Get_ORDERDCFLAG(me_docd.MMCODE);   // 藥品停用碼

                            if (!repo1.CheckMeDocdExists_1(me_docd))
                            {
                                me_docd.CREATE_USER = User.Identity.Name;
                                me_docd.UPDATE_USER = User.Identity.Name;
                                me_docd.UPDATE_IP = DBWork.ProcIP;
                                me_docd.FRWH_D = repo.GetFrwhWithMmcode(repo.GetThisTowh(me_docd.DOCNO), me_docd.MMCODE);
                                session.Result.afrs = repo.DetailCreate(me_docd);

                                // 將第一個院內碼項次的核撥庫房回寫ME_DOCM
                                if (me_docd.SEQ == "1")
                                {
                                    ME_DOCM me_docm = new ME_DOCM();
                                    me_docm.DOCNO = me_docd.DOCNO;
                                    me_docm.FRWH = me_docd.FRWH_D;
                                    me_docm.UPDATE_USER = User.Identity.Name;
                                    me_docm.UPDATE_IP = DBWork.ProcIP;
                                    repo.MasterUpdateFrwh(me_docm);
                                }
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>此申請單已存在此院內碼</span>，請重新輸入院內碼。";
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
        public ApiResponse DetailUpdate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
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
                    me_docd.FRWH_D = repo.GetFrwhWithMmcode(repo.GetThisTowh(me_docd.DOCNO), me_docd.MMCODE);
                    me_docd.APLYITEM_NOTE = form.Get("APLYITEM_NOTE");

                    if (me_docd.FRWH_D == "PH1S")
                    {
                        double pack_qty = Convert.ToDouble(repo.GetThisPackQty(me_docd.DOCNO, me_docd.SEQ));
                        if (Convert.ToDouble(me_docd.APPQTY) >= pack_qty && Convert.ToDouble(me_docd.APPQTY) % pack_qty == 0)
                        {
                            string minSeq = repo.GetMinSeq(me_docd.DOCNO);

                            // 將第一個院內碼項次的核撥庫房回寫ME_DOCM
                            if (me_docd.SEQ == minSeq)
                            {
                                ME_DOCM me_docm = new ME_DOCM();
                                me_docm.DOCNO = me_docd.DOCNO;
                                me_docm.FRWH = me_docd.FRWH_D;
                                me_docm.UPDATE_USER = User.Identity.Name;
                                me_docm.UPDATE_IP = DBWork.ProcIP;
                                repo.MasterUpdateFrwh(me_docm);
                            }
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.DetailUpdate(me_docd);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>申請數量必須是最小撥補量的倍數</span>，請重新輸入申請數量。";
                        }
                    }
                    else
                    {
                        string minSeq = repo.GetMinSeq(me_docd.DOCNO);

                        // 將第一個院內碼項次的核撥庫房回寫ME_DOCM
                        if (me_docd.SEQ == minSeq)
                        {
                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = me_docd.DOCNO;
                            me_docm.FRWH = me_docd.FRWH_D;
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            repo.MasterUpdateFrwh(me_docm);
                        }
                        me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.DetailUpdate(me_docd);
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
                    AB0012Repository repo = new AB0012Repository(DBWork);
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

                            repo.AB0012_NULL(tmp_docno[i], tmp_seq[i]);
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
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckIsTowhCancelByDocno(tmp[i]))
                            {
                                session.Result.success = false;
                                session.Result.msg = string.Format("申請單號「{0}」申請庫房已作廢，請重新確認", tmp[i]);
                                return session.Result;
                            }

                            if (repo1.CheckMeDocdExists(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            {
                                if (repo1.CheckMeDocdAppqty(tmp[i]))
                                {
                                    ME_DOCM me_docm = new ME_DOCM();
                                    me_docm.DOCNO = tmp[i];

                                    bool flowIdValid = repo.ChceckFlowId01(tmp[i]);
                                    if (flowIdValid == false)
                                    {
                                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                                        session.Result.success = false;
                                        return session.Result;
                                    }

                                    //me_docm.FLOWID = form.Get("FLOWID");
                                    me_docm.FLOWID = "0602";
                                    me_docm.UPDATE_USER = User.Identity.Name;
                                    me_docm.UPDATE_IP = DBWork.ProcIP;

                                    IEnumerable<ME_DOCD> myEnum = repo.GetFrwhValue(me_docm.DOCNO);
                                    myEnum.GetEnumerator();
                                    int a = 1;
                                    foreach (var item in myEnum)
                                    {
                                        if (a != 1)
                                        {
                                            string towh = "";
                                            AB0012Repository.ME_DOCM_QUERY_PARAMS query = new AB0012Repository.ME_DOCM_QUERY_PARAMS();
                                            query.DOCNO = tmp[i];
                                            query.FLOWID = "";
                                            query.APPTIME_S = "";
                                            query.APPTIME_E = "";
                                            IEnumerable<ME_DOCM> docmEnum = repo.GetAll(query);
                                            docmEnum.GetEnumerator();
                                            foreach (var docmItem in docmEnum)
                                                towh = docmItem.TOWH;

                                            // 拆單,並新建單號
                                            ME_DOCM me_docm_new = new ME_DOCM();
                                            me_docm_new.DOCNO = repo1.GetDocno();
                                            me_docm_new.CREATE_USER = User.Identity.Name;
                                            me_docm_new.UPDATE_USER = User.Identity.Name;
                                            me_docm_new.UPDATE_IP = DBWork.ProcIP;
                                            me_docm_new.APPID = User.Identity.Name;
                                            me_docm_new.APPDEPT = DBWork.UserInfo.Inid;
                                            me_docm_new.USEID = User.Identity.Name;
                                            me_docm_new.TOWH = towh;        // 申請庫房
                                            me_docm_new.FRWH = item.FRWH_D;        // 核撥庫房
                                            me_docm_new.DOCTYPE = "MS";
                                            me_docm_new.FLOWID = "0601";
                                            me_docm_new.MAT_CLASS = "01";
                                            repo.MasterCreate(me_docm_new);

                                            // 將第二種核撥庫房的項次,修改為新單號
                                            repo.DetailUpdateDocno(tmp[i], item.FRWH_D, me_docm_new.DOCNO, User.Identity.Name, DBWork.ProcIP);

                                            // 以便後面UpdateStatus更新FLOWID用
                                            me_docm.DOCNO = me_docm_new.DOCNO;
                                        }
                                        else
                                        {
                                            // 第一種核撥庫房
                                            me_docm.FRWH = item.FRWH_D;
                                            repo.MasterUpdateFrwh(me_docm);
                                        }

                                        // 2020-06-23 新增: 送核撥更新me_docd.apl_contime
                                        // 2020-09-16 新增: 核撥量預帶申請量
                                        session.Result.afrs = repo.UpdateDocdAplcontime(tmp[i], DBWork.UserInfo.UserId, DBWork.ProcIP);

                                        // 狀態更新成0602
                                        // 2020-07-20: 更新狀態時更新申請時間為sysdate
                                        session.Result.afrs = repo.UpdateStatus(me_docm);

                                        a++;
                                    }
                                }
                                else
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」院內碼項次申請數量必須大於0</span>，請填寫申請數量。";
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
                    AB0012Repository repo = new AB0012Repository(DBWork);
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
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("P0");
            var p1 = form.Get("P1").Trim().ToUpper();
            var p2 = form.Get("P2").Trim().ToUpper();
            //var p1_name = form.Get("P1_Name").Trim();
            //var p2 = form.Get("P2").Trim();

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();

                    result = repo.GetExcel(p0, p1, p2);
                    dtItems.Merge(result);

                    //string str_UserDept = DBWork.UserInfo.InidName;
                    //string export_FileName = str_UserDept + p1_name + "藥品核撥報表";
                    string export_FileName = p0 + "公藥申請報表";
                    string title = "申請單號 " + p0 + " 公藥申請報表";
                    JCLib.Excel.Export(export_FileName + ".xls", dtItems, (tmp_dt) => { return title; });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse GetFrwhComboQ()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    session.Result.etts = repo.GetFrwhComboQ();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetTowhComboQ()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    session.Result.etts = repo.GetTowhComboQ(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetRestrictCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0012Repository repo = new AB0012Repository(DBWork);
                    session.Result.etts = repo.GetRestrictCombo();
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
