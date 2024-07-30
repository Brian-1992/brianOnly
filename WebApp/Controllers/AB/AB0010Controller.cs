using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Security.AntiXss;

namespace WebApp.Controllers.AB
{
    public class AB0010Controller : SiteBase.BaseApiController
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
                    var repo = new AB0010Repository(DBWork);
                    AB0010Repository.ME_DOCM_QUERY_PARAMS query = new AB0010Repository.ME_DOCM_QUERY_PARAMS();
                    query.DOCNO = form.Get("p1");
                    //query.APPID = form.Get("p2");
                    query.TOWH = form.Get("p4").ToUpper();
                    query.FRWH = form.Get("p6").ToUpper();
                    query.APPDEPT = form.Get("p7").ToUpper();

                    //query.DOCTYPE = form.Get("DOCTYPE");
                    query.DOCTYPE = "MR";
                    //query.FLOWID = form.Get("FLOWID");
                    query.FLOWID = "0101,0100";
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
            //var page = int.Parse(form.Get("page"));
            //var start = int.Parse(form.Get("start"));
            //var limit = int.Parse(form.Get("limit"));
            //var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0010Repository(DBWork);
                    AB0010Repository.ME_DOCD_QUERY_PARAMS query = new AB0010Repository.ME_DOCD_QUERY_PARAMS();
                    query.DOCNO = form.Get("p0");
                    //session.Result.etts = repo.GetAllMeDocd(query, page, limit, sorters);
                    session.Result.etts = repo.GetAllMeDocd(query);
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
        public ApiResponse GetSupplyInid(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0010Repository(DBWork);
                    //List<UR_ID> myur_id = repo.GetUserInfo(User.Identity.Name);

                    //if (myur_id.Count > 0)
                    //    session.Result.etts = repo.GetSupplyInid(myur_id[0].INID);
                    session.Result.etts = repo.GetSupplyInid(User.Identity.Name).Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
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
        public ApiResponse GetSuggestQty(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0010Repository(DBWork);
                    //List<UR_ID> myur_id = repo.GetUserInfo(User.Identity.Name);

                    //if (myur_id.Count > 0)
                    //    session.Result.etts = repo.GetSupplyInid(myur_id[0].INID);
                    session.Result.etts = repo.GetSuggestQty(form.Get("WH_NO"), form.Get("MMCODE"));
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
                    var repo = new AB0010Repository(DBWork);
                    List<UR_ID> myur_id = repo.GetUserInfo(User.Identity.Name);

                    if (myur_id.Count > 0)
                    {
                        session.Result.etts = repo
                            .GetFrwh(myur_id[0].INID, form.Get("WH_GRADE"))
                            .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, SUPPLY_INID = w.SUPPLY_INID });
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
        public string GetGrade(FormDataCollection form)
        {
            string rtn = "";
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0010Repository(DBWork);
                    rtn = repo.GetGrade(AntiXssEncoder.HtmlEncode(form.Get("WH_NO"), true));
                }
                catch
                {
                    throw;
                }
                return rtn;
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
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    AB0010Repository.MI_MAST_QUERY_PARAMS query = new AB0010Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
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
                    AB0010Repository repo = new AB0010Repository(DBWork);
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
        public ApiResponse GetQtyInfo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    //session.Result.etts = repo.GetMMCodeCombo(p0, wh_no, page, limit, "");

                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO");
                    me_docd.MMCODE = form.Get("MMCODE");
                    string frwh = repo.GetFrwh(me_docd.DOCNO);
                    string towh = repo.GetTowh(me_docd.DOCNO);
                    me_docd.SUGGEST_QTY = repo.Get_SUGGEST_QTY(towh, me_docd.MMCODE);
                    me_docd.S_INV_QTY = repo.Get_S_INV_QTY(towh, me_docd.MMCODE);    // 上級庫庫存量
                    me_docd.INV_QTY = repo.Get_INV_QTY(towh, me_docd.MMCODE);    // 庫存量
                    me_docd.STORE_LOC = repo.GetStoreLoc(frwh, me_docd.MMCODE);

                    string tmp = "";
                    tmp = repo.Get_SAFE_OPER_QTY(towh, me_docd.MMCODE);
                    if (tmp != null && tmp.Trim() != "")
                    {
                        string[] tmp_qty = tmp.Split(',');
                        if (tmp_qty.Length > 0)
                        {
                            me_docd.SAFE_QTY = tmp_qty[0];  // 安全量
                            me_docd.OPER_QTY = tmp_qty[1];  // 基準量,呼叫Get_SAFE_OPER_QTY 取得的round(high_qty)填入
                            //me_docd.SAFE_QTY_90 = tmp_qty[2];  //安全量_90
                            //me_docd.HIGH_QTY_90 = tmp_qty[3];  //基準量_90 
                            //me_docd.APLY_QTY_90 = tmp_qty[4];  //建議申請量_90
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
                    me_docd.ISSPLIT = repo.GetISSPLIT(me_docd.DOCNO, me_docd.MMCODE);   // 是否拆單
                    me_docd.M_AGENNO = repo.GetM_AGENNO(me_docd.MMCODE);        // 廠商代碼

                    session.Result.etts = new ME_DOCD[] { me_docd };
                }
                catch (Exception e)
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
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    ME_DOCM me_docm = new ME_DOCM();

                    if (repo.CheckIsTowhCancelByWhno(form.Get("TOWH")))
                    {
                        session.Result.success = false;
                        session.Result.msg = "申請庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    me_docm.DOCNO = repo.GetDocno();
                    if (!repo.CheckExists(me_docm.DOCNO))
                    {
                        me_docm.CREATE_USER = User.Identity.Name;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        me_docm.APPID = User.Identity.Name;
                        //me_docm.APPDEPT = form.Get("INID_NAME").Split(' ')[0];
                        me_docm.APPDEPT = DBWork.UserInfo.Inid;
                        me_docm.USEID = User.Identity.Name;
                        me_docm.TOWH = form.Get("TOWH");        // 申請庫房
                        me_docm.FRWH = form.Get("FRWH");        // 核撥庫房
                        me_docm.DOCTYPE = "MR";
                        me_docm.FLOWID = "0101";
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
        public ApiResponse MasterDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
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
        public ApiResponse MasterUpdate(ME_DOCM me_docm)
        {
            using (
                WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
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

                    string frwh = repo.GetFrwh(me_docm.DOCNO);
                    if (frwh == me_docm.FRWH.Trim()) // 如果核撥庫房一樣,則可以直接更新
                        session.Result.afrs = repo.MasterUpdate(me_docm);
                    else
                    {
                        if (!repo.CheckMeDocdExists(me_docm.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                            session.Result.afrs = repo.MasterUpdate(me_docm);
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>申請單號「" + me_docm.DOCNO + "」已存在" + frwh + "庫房院內碼項次，所以無法修改核撥庫房</span><br>如欲修改核撥庫房，請先刪除所有項次。";
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
        public ApiResponse DetailCreate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO2");

                    bool flowIdValid = repo.ChceckFlowId01(me_docd.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    if (!repo.CheckMeDocdExists(me_docd.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                        me_docd.SEQ = "1";
                    else
                        me_docd.SEQ = repo.GetMaxSeq(me_docd.DOCNO);
                    me_docd.MMCODE = form.Get("MMCODE");
                    me_docd.APPQTY = form.Get("APPQTY");
                    me_docd.APLYITEM_NOTE = form.Get("APLYITEM_NOTE");
                    //me_docd.PACK_UNIT = form.Get("PACK_UNIT");
                    //me_docd.PACK_QTY = form.Get("PACK_QTY");

                    string towh = repo.GetTowh(me_docd.DOCNO);
                    me_docd.S_INV_QTY = repo.Get_S_INV_QTY(towh, me_docd.MMCODE);    // 上級庫庫存量
                    me_docd.INV_QTY = repo.Get_INV_QTY(towh, me_docd.MMCODE);    // 庫存量

                    string tmp = "";
                    tmp = repo.Get_SAFE_OPER_QTY(towh, me_docd.MMCODE);
                    if (tmp != null && tmp.Trim() != "")
                    {
                        string[] tmp_qty = tmp.Split(',');
                        if (tmp_qty.Length > 0)
                        {
                            me_docd.SAFE_QTY = tmp_qty[0];  // 安全量
                            me_docd.OPER_QTY = tmp_qty[1];  // 基準量,呼叫Get_SAFE_OPER_QTY 取得的round(high_qty)填入
                            //me_docd.SAFE_QTY_90 = tmp_qty[2];  //安全量_90
                            //me_docd.HIGH_QTY_90 = tmp_qty[3];  //基準量_90 
                            //me_docd.APLY_QTY_90 = tmp_qty[4];  //建議申請量_90
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
                    me_docd.PACK_TIMES = repo.GetPackTimes(towh, me_docd.MMCODE);
                    me_docd.E_ORDERDCFLAG = repo.Get_ORDERDCFLAG(me_docd.MMCODE);   // 藥品停用碼
                    if (me_docd.E_ORDERDCFLAG != "N")
                    {
                        session.Result.success = false;
                        session.Result.msg = "此院內碼<span style='color:red'>已全院停用</span>，不可申請。";
                        return session.Result;
                    }
                    me_docd.ISSPLIT = repo.GetISSPLIT(me_docd.DOCNO, me_docd.MMCODE);   // 是否拆單
                    me_docd.M_AGENNO = repo.GetM_AGENNO(me_docd.MMCODE);        // 廠商代碼

                    if (repo.CheckWhmmExists(form.Get("FRWH2").Split(' ')[0], me_docd.MMCODE))
                    {
                        if (!repo.CheckMeDocdExists_1(me_docd))
                        {
                            me_docd.CREATE_USER = User.Identity.Name;
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.DetailCreate(me_docd);
                            //session.Result.etts = repo.DetailGet(me_docd);
                            AB0010Repository.ME_DOCD_QUERY_PARAMS query = new AB0010Repository.ME_DOCD_QUERY_PARAMS();
                            query.DOCNO = me_docd.DOCNO;
                            query.SEQ = me_docd.SEQ;
                            session.Result.etts = repo.GetAllMeDocd(query);
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
                catch (Exception ex)
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
                    AB0010Repository repo = new AB0010Repository(DBWork);
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

                    string towh = repo.GetTowh(me_docd.DOCNO);
                    me_docd.S_INV_QTY = repo.Get_S_INV_QTY(towh, me_docd.MMCODE);    // 上級庫庫存量
                    me_docd.INV_QTY = repo.Get_INV_QTY(towh, me_docd.MMCODE);    // 庫存量

                    string tmp = "";
                    tmp = repo.Get_SAFE_OPER_QTY(towh, me_docd.MMCODE);
                    if (tmp != null && tmp.Trim() != "")
                    {
                        string[] tmp_qty = tmp.Split(',');
                        if (tmp_qty.Length > 0)
                        {
                            me_docd.SAFE_QTY = tmp_qty[0];  // 安全量
                            me_docd.OPER_QTY = tmp_qty[1];  // 基準量,呼叫Get_SAFE_OPER_QTY 取得的round(high_qty)填入
                            //me_docd.SAFE_QTY_90 = tmp_qty[2];  //安全量_90
                            //me_docd.HIGH_QTY_90 = tmp_qty[3];  //基準量_90 
                            //me_docd.APLY_QTY_90 = tmp_qty[4];  //建議申請量_90
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

                    if (repo.CheckWhmmExists(form.Get("FRWH2").Split(' ')[0], me_docd.MMCODE))
                    {
                        if (!repo.CheckMeDocdExists_1(me_docd))
                        {
                            me_docd.UPDATE_USER = User.Identity.Name;
                            me_docd.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.DetailUpdate(me_docd);
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
        public ApiResponse DetailDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp_docno.Length; i++)
                        {
                            if (tmp_docno[i] == "*") {
                                continue;
                            }
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
        public ApiResponse LoadLowItem(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    //AB0010Repository repo1 = new AB0010Repository(DBWork);

                    if (repo.CheckIsTowhCancelByDocno(form.Get("DOCNO")))
                    {
                        session.Result.success = false;
                        session.Result.msg = "申請庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    bool flowIdValid = repo.ChceckFlowId01(form.Get("DOCNO"));
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    IEnumerable<ME_DOCD> myEnum = null;
                    // 讀取ME_AB0012
                    myEnum = repo.LoadLowItem(form.Get("DOCNO"), form.Get("IS_AUTO"), form.Get("MMCODE_PREFIX"), form.Get("RESTRICTCODE123"), form.Get("IS_VACCINE"));

                    myEnum.GetEnumerator();
                    foreach (var item in myEnum)
                    {
                        // 寫入ME_DOCD
                        ME_DOCD me_docd = new ME_DOCD();
                        me_docd.DOCNO = form.Get("DOCNO");

                        if (!repo.CheckMeDocdExists(me_docd.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                            me_docd.SEQ = "1";
                        else
                            me_docd.SEQ = repo.GetMaxSeq(me_docd.DOCNO);
                        me_docd.MMCODE = item.MMCODE;
                        me_docd.APPQTY = item.APL_QTY;

                        string towh = repo.GetTowh(me_docd.DOCNO);
                        me_docd.S_INV_QTY = repo.Get_S_INV_QTY(towh, me_docd.MMCODE);    // 上級庫庫存量
                        me_docd.INV_QTY = repo.Get_INV_QTY(towh, me_docd.MMCODE);    // 庫存量

                        string tmp = "";
                        tmp = repo.Get_SAFE_OPER_QTY(towh, me_docd.MMCODE);
                        if (tmp != null && tmp.Trim() != "")
                        {
                            string[] tmp_qty = tmp.Split(',');
                            if (tmp_qty.Length > 0)
                            {
                                me_docd.SAFE_QTY = tmp_qty[0];  // 安全量
                                me_docd.OPER_QTY = tmp_qty[1];  // 基準量,呼叫Get_SAFE_OPER_QTY 取得的round(high_qty)填入
                                //me_docd.SAFE_QTY_90 = tmp_qty[2];  //安全量_90
                                //me_docd.HIGH_QTY_90 = tmp_qty[3];  //基準量_90 
                                //me_docd.APLY_QTY_90 = tmp_qty[4];  //建議申請量_90
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

                        me_docd.ISSPLIT = repo.GetISSPLIT(me_docd.DOCNO, me_docd.MMCODE);   // 是否拆單
                        me_docd.M_AGENNO = repo.GetM_AGENNO(me_docd.MMCODE);        // 廠商代碼

                        //if (!repo1.CheckMeDocdExists_1(me_docd))
                        //{
                        me_docd.CREATE_USER = User.Identity.Name;
                        me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        me_docd.PACK_TIMES = repo.GetPackTimes(towh, me_docd.MMCODE);
                        //me_docd.FRWH_D = repo.GetPwhno(DBWork.UserInfo.UserId);
                        session.Result.afrs = repo.DetailCreate(me_docd);
                    }
                }
                catch (Exception e)
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
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            string newDocno = repo.GetDocno();
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
        public ApiResponse UpdateStatus(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckMeDocdExists(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            {
                                if (repo.CheckMeDocdAppqty(tmp[i]))
                                {
                                    ME_DOCM me_docm = new ME_DOCM();
                                    me_docm.DOCNO = tmp[i];

                                    bool flowIdValid = repo.ChceckFlowId01(me_docm.DOCNO);
                                    if (flowIdValid == false)
                                    {
                                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                                        session.Result.success = false;
                                        return session.Result;
                                    }

                                    me_docm.FLOWID = "0102";    // 0102-藥庫核撥中
                                    me_docm.UPDATE_USER = User.Identity.Name;
                                    me_docm.UPDATE_IP = DBWork.ProcIP;

                                    // 2021-10-04 拆單
                                    IEnumerable<ME_DOCD> splitDatas = repo.GetSplitData(tmp[i]);
                                    foreach (ME_DOCD splitData in splitDatas)
                                    {
                                        // 產生新ME_DOCM
                                        string new_docno = repo.GetDocno();
                                        session.Result.afrs = repo.CreateNewDocm(tmp[i], new_docno, splitData.M_AGENNO, me_docm.FLOWID, DBWork.UserInfo.UserId, DBWork.ProcIP);
                                        // 新增ME_DOCD
                                        session.Result.afrs = repo.CreateNewDocd(tmp[i], new_docno, splitData.ISSPLIT, splitData.M_AGENNO, DBWork.UserInfo.UserId, DBWork.ProcIP);

                                        // 2020-06-23 新增: 送核撥更新me_docd.apl_contime
                                        // 2020-09-16 新增: 核撥量預帶申請量
                                        session.Result.afrs = repo.UpdateDocdAplcontime(new_docno, DBWork.UserInfo.UserId, DBWork.ProcIP);
                                        // 2020-07-20 新增: 更新狀態時更新申請時間為sysdate
                                        session.Result.afrs = repo.UpdateStatus(new_docno, me_docm.FLOWID, DBWork.UserInfo.UserId, DBWork.ProcIP);
                                    }
                                    // 刪除原申請單
                                    session.Result.afrs = repo.DeleteAllDocd(tmp[i]);
                                    session.Result.afrs = repo.MasterDelete(tmp[i]);
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
        public ApiResponse GetAppdeptCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    session.Result.etts = repo.GetAppdeptCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetUsedeptCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    session.Result.etts = repo.GetUsedeptCombo();
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
                    AB0010Repository repo = new AB0010Repository(DBWork);
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
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo();
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
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    session.Result.etts = repo.GetDocnoCombo(User.Identity.Name);
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
            var P0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    JCLib.Excel.Export("手動申請" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(P0));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetPackUnitCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    session.Result.etts = repo.GetPackUnitCombo(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CheckFlowid(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    string flowid = repo.GetFlowid(p0);
                    if (flowid == null)
                    {
                        session.Result.msg = "申請單已刪除";
                    }
                    if (flowid == "0102")
                    {
                        session.Result.msg = "申請單狀態已變更(0102 核撥中)";
                    }

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
        public ApiResponse CheckFlowidMulti(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                    string[] tmp = docno.Split(',');
                    string flowid;
                    string result = "";
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        flowid = repo.GetFlowid(tmp[i]);
                        if (flowid == null)
                        {
                            if (result == string.Empty)
                            {
                                result += "下列申請單狀態已變更：<br>";
                                result += string.Format("{0}：已刪除", tmp[i]);
                            }
                            else
                            {
                                result += string.Format("<br>{0}：已刪除", tmp[i]);
                            }
                        }
                        if (flowid == "0102")
                        {
                            if (result == string.Empty)
                            {
                                result += "下列申請單狀態已變更：<br>";
                                result += string.Format("{0}：已送核撥", tmp[i]);
                            }
                            else
                            {
                                result += string.Format("<br>{0}：已送核撥", tmp[i]);
                            }
                        }
                    }
                    session.Result.msg = result;
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse LoadAllItem(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0010Repository repo = new AB0010Repository(DBWork);
                    //AB0010Repository repo1 = new AB0010Repository(DBWork);

                    if (repo.CheckIsTowhCancelByDocno(form.Get("DOCNO")))
                    {
                        session.Result.success = false;
                        session.Result.msg = "申請庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    bool flowIdValid = repo.ChceckFlowId01(form.Get("DOCNO"));
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    // 讀取ME_AB0012
                    IEnumerable<ME_DOCD> myEnum = repo.LoadAllItem(form.Get("DOCNO"));
                    myEnum.GetEnumerator();
                    foreach (var item in myEnum)
                    {
                        // 寫入ME_DOCD
                        ME_DOCD me_docd = new ME_DOCD();
                        me_docd.DOCNO = form.Get("DOCNO");
                        if (!repo.CheckMeDocdExists(me_docd.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                            me_docd.SEQ = "1";
                        else
                            me_docd.SEQ = repo.GetMaxSeq(me_docd.DOCNO);
                        me_docd.MMCODE = item.MMCODE;
                        me_docd.APPQTY = item.APL_QTY;

                        string towh = repo.GetTowh(me_docd.DOCNO);
                        me_docd.S_INV_QTY = repo.Get_S_INV_QTY(towh, me_docd.MMCODE);    // 上級庫庫存量
                        me_docd.INV_QTY = repo.Get_INV_QTY(towh, me_docd.MMCODE);    // 庫存量

                        string tmp = "";
                        tmp = repo.Get_SAFE_OPER_QTY(towh, me_docd.MMCODE);
                        if (tmp != null && tmp.Trim() != "")
                        {
                            string[] tmp_qty = tmp.Split(',');
                            if (tmp_qty.Length > 0)
                            {
                                me_docd.SAFE_QTY = tmp_qty[0];  // 安全量
                                me_docd.OPER_QTY = tmp_qty[1];  // 基準量,呼叫Get_SAFE_OPER_QTY 取得的round(high_qty)填入
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

                        me_docd.ISSPLIT = repo.GetISSPLIT(me_docd.DOCNO, me_docd.MMCODE);   // 是否拆單
                        me_docd.E_ORDERDCFLAG = repo.Get_ORDERDCFLAG(me_docd.MMCODE);   // 藥品停用碼

                        //if (!repo1.CheckMeDocdExists_1(me_docd))
                        //{
                        me_docd.CREATE_USER = User.Identity.Name;
                        me_docd.UPDATE_USER = User.Identity.Name;
                        me_docd.UPDATE_IP = DBWork.ProcIP;
                        me_docd.PACK_TIMES = repo.GetPackTimes(towh, me_docd.MMCODE);
                        //me_docd.FRWH_D = repo.GetPwhno(DBWork.UserInfo.UserId);
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
    }
}