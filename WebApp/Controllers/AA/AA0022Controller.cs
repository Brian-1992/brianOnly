﻿using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0022Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var pp = form.Get("pp");
            var p3 = "560000";
            var v_user = User.Identity.Name;
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0022Repository(DBWork);
                    
                    session.Result.etts = repo.GetAllM(p0, p1, p2, p3, pp, v_user, page, limit, sorters);
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0022Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
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
                    var repo = new AA0022Repository(DBWork);
                    if (!repo.CheckExists(ME_DOCM.DOCNO)) // 新增前檢查主鍵是否已存在
                    {
                        var v_inid = repo.GetUridInid(User.Identity.Name);
                        var v_twntime = repo.GetTwnsystime();
                        var v_matclass = ME_DOCM.MAT_CLASS;
                        var v_docno = repo.GetDocno();
                        var v_whno = "560000";
                        var v_apptime = ME_DOCM.APPTIME_T;
                        ME_DOCM.DOCNO = v_docno;
                        ME_DOCM.FRWH = v_whno;
                        //ME_DOCM.TOWH = v_whno;
                        ME_DOCM.APPID = User.Identity.Name;
                        ME_DOCM.APPDEPT = v_inid;
                        ME_DOCM.APPTIME = v_apptime;
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
        public ApiResponse CreateD(ME_DOCEXP docexp)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0022Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(docexp.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    //if (repo.CheckExistsMM(docexp.DOCNO, docexp.MMCODE))
                    //{
                    //    session.Result.afrs = 0;
                    //    session.Result.success = false;
                    //    session.Result.msg = "<span style='color:red'>此單明細已有重複的院內碼「" + docexp.MMCODE + "」</span>，請確認。";
                    //}
                    //else
                    //{
                    if (!repo.CheckExistsE(docexp.DOCNO)) // 新增前檢查主鍵是否已存在
                    {
                        docexp.SEQ = "1";
                    }
                    else
                    {
                        docexp.SEQ = repo.GetDocESeq(docexp.DOCNO);
                    }

                    var v_EXP_DATE = docexp.EXP_DATE_T;
                    ///ME_docexp.EXP_DATE = repo.Getdate(v_EXP_DATE);
                    docexp.EXP_DATE = v_EXP_DATE;
                    var v_num = 1;
                    if (docexp.INOUT == "1")
                    {
                        v_num = 1;
                    }
                    else
                    {
                        v_num = -1;
                    }
                    var v_up = 0.0000;
                    v_up = double.Parse(docexp.M_CONTPRICE);
                    var v_qty = 0.0000;
                    v_qty = double.Parse(docexp.APVQTY);
                    var v_amt = 0.0000;
                    v_amt = v_qty * v_up * v_num;
                    var v_apvqty = 0.0000;
                    v_apvqty = v_qty * v_num;
                    docexp.APVQTY = v_apvqty.ToString();
                    docexp.C_UP = docexp.M_CONTPRICE;
                    docexp.C_AMT = v_amt.ToString();
                    docexp.UPDATE_USER = User.Identity.Name;
                    docexp.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.CreateExp(docexp);

                    session.Result.etts = repo.GetM(docexp.DOCNO);
                    DBWork.Commit();
                    //}
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
                    var repo = new AA0022Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(ME_DOCM.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    var v_apptime = ME_DOCM.APPTIME_T;
                    ME_DOCM.APPTIME = v_apptime;
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
        public ApiResponse UpdateD(ME_DOCEXP docexp)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0026Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(docexp.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    var v_EXP_DATE = docexp.EXP_DATE;
                    docexp.EXP_DATE = repo.Getdate(v_EXP_DATE);

                    var v_num = 1;
                    if (docexp.INOUT == "1")
                    {
                        v_num = 1;
                    }
                    else
                    {
                        v_num = -1;
                    }
                    var v_up = 0.0000;
                    v_up = double.Parse(docexp.M_CONTPRICE);
                    var v_qty = 0.0000;
                    v_qty = double.Parse(docexp.APVQTY);
                    var v_amt = 0.0000;
                    v_amt = v_qty * v_up * v_num;
                    var v_apvqty = 0.0000;
                    v_apvqty = v_qty * v_num;
                    docexp.APVQTY = v_apvqty.ToString();
                    docexp.C_UP = docexp.M_CONTPRICE;
                    docexp.C_AMT = v_amt.ToString();
                    docexp.UPDATE_USER = User.Identity.Name;
                    docexp.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateE(docexp);
                    session.Result.etts = repo.GetD(docexp.DOCNO);

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
        public ApiResponse UpdateMeDocd()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0022Repository repo = new AA0022Repository(DBWork);
                    HttpContent requestContent = Request.Content;
                    string jsonContent = requestContent.ReadAsStringAsync().Result;
                    //NM_CONTACT contact = JsonConvert.DeserializeObject<NM_CONTACT>(jsonContent);
                    JObject obj = JsonConvert.DeserializeObject<JObject>(jsonContent);          // 先解第一層 {"item":[{"id":24,"part_no":"12223"},{...}]}
                    JArray ja = JsonConvert.DeserializeObject<JArray>(obj["item"].ToString());  // 解第二層
                    ME_DOCD me_docd = JsonConvert.DeserializeObject<ME_DOCD>(ja[0].ToString());
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateMeDocd(me_docd);

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
                    AA0022Repository repo = new AA0022Repository(DBWork);
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
                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i];
                            me_docm.FLOWID = "X";
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.ApplyM(me_docm);
                            //session.Result.afrs = repo.DeleteAllD(tmp[i]);
                            //session.Result.afrs = repo.DeleteM(tmp[i]);
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
                    AA0022Repository repo = new AA0022Repository(DBWork);
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
                    AA0022Repository repo = new AA0022Repository(DBWork);
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

                            if (repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            {
                                if (repo.CheckExistsDN(tmp[i]))
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<span style='color:red'>此單明細尚有申請數量為0</span>不得送審核。";
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
        public ApiResponse Apply2(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0022Repository repo = new AA0022Repository(DBWork);
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

                            if (repo.CheckExistsExp(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            {
                                if (repo.CheckExistsExpN(tmp[i]))
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<span style='color:red'>此單明細尚有申請數量為0</span>不得核可扣帳。";
                                }
                                else
                                {
                                    //(1)
                                    ME_DOCM me_docm = new ME_DOCM();
                                    me_docm.DOCNO = tmp[i];
                                    me_docm.FLOWID = "3";
                                    me_docm.UPDATE_USER = User.Identity.Name;
                                    me_docm.UPDATE_IP = DBWork.ProcIP;
                                    //session.Result.afrs = repo.ApplyM(me_docm);
                                    //(2)
                                    var v_length = 0;
                                    var v_mmcode = "";
                                    var v_note = "";
                                    var v_appqty = 0.0000;
                                    v_length = int.Parse(repo.GetDocENum(me_docm.DOCNO));
                                    for (int k = 1; k < (v_length + 1); k++)
                                    {
                                        v_mmcode = repo.GetDocEMmcode(me_docm.DOCNO, k.ToString());
                                        if (!repo.CheckExistsDMmcode(me_docm.DOCNO, v_mmcode))
                                        {
                                            v_appqty = double.Parse(repo.GetDocEMmcodeApvqty(me_docm.DOCNO, v_mmcode));
                                            v_note = repo.GetDocEMmcodeNote(me_docm.DOCNO, v_mmcode);
                                            ME_DOCD me_docd = new ME_DOCD();
                                            me_docd.DOCNO = me_docm.DOCNO;
                                            if (!repo.CheckExistsD(me_docm.DOCNO))
                                            {
                                                me_docd.SEQ = "1";
                                            }
                                            else
                                            {
                                                me_docd.SEQ = repo.GetDocDSeq(me_docm.DOCNO);
                                            }
                                            if (v_appqty != 0)
                                            {
                                                if (v_appqty > 0)
                                                {
                                                    me_docd.STAT = "1";
                                                }
                                                else
                                                {
                                                    me_docd.STAT = "2";
                                                }
                                            }
                                            me_docd.MMCODE = v_mmcode;
                                            me_docd.APPQTY = v_appqty.ToString();
                                            me_docd.APLYITEM_NOTE = v_note;
                                            me_docd.UPDATE_USER = User.Identity.Name;
                                            me_docd.UPDATE_IP = DBWork.ProcIP;
                                            session.Result.afrs = repo.CreateD(me_docd);
                                        }
                                    }
                                    //(3)
                                    var rtn = repo.CallProc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                                    if ( rtn != "Y")
                                    {
                                        session.Result.afrs = 0;
                                        session.Result.success = false;
                                        session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」</span>，發生執行錯誤，" + rtn + "。";
                                    }
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
        public ApiResponse GetDocnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0022Repository repo = new AA0022Repository(DBWork);
                    session.Result.etts = repo.GetDocnoCombo();
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
                    AA0022Repository repo = new AA0022Repository(DBWork);
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
                    AA0022Repository repo = new AA0022Repository(DBWork);
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
        public ApiResponse GetLotNoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0022Repository repo = new AA0022Repository(DBWork);
                    var w_exp = repo.GetMmcodeWexp(p0);
                    if (w_exp == "N")
                    {
                        session.Result.etts = repo.GetLotNo2Combo(p0);
                    }
                    else
                    {
                        session.Result.etts = repo.GetLotNoCombo(p0);
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
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
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
                    AA0022Repository repo = new AA0022Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, page, limit, "");
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
                    AA0022Repository repo = new AA0022Repository(DBWork);
                    AA0022Repository.MI_MAST_QUERY_PARAMS query = new AA0022Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
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
                    AA0022Repository repo = new AA0022Repository(DBWork);
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
                    AA0022Repository repo = new AA0022Repository(DBWork);
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
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0022Repository repo = new AA0022Repository(DBWork);
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
                    AA0022Repository repo = new AA0022Repository(DBWork);
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