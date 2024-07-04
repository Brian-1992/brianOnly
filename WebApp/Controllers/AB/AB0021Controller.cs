using System;
using System.Collections.Generic;
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

namespace WebApp.Controllers.AB
{
    public class AB0021Controller : SiteBase.BaseApiController
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
                    var repo = new AB0021Repository(DBWork);
                    AB0021Repository.ME_DOCM_QUERY_PARAMS query = new AB0021Repository.ME_DOCM_QUERY_PARAMS();
                    query.DOCNO = form.Get("p1");
                    query.MAT_CLASS = form.Get("p3");

                    if (repo.IsWhGrade1(form.Get("APPDEPT")))   // 庫房人員, 所有"繳回中"都可以看到
                    {
                        query.APPDEPT = "";
                        //query.FLOWID = "2";
                        //if (form.Get("TASKID") == "2")  //衛材
                        //{
                        //    if (form.Get("p3") == "02")
                        //        query.MAT_CLASS = form.Get("p3");
                        //    else if (form.Get("p3") == "")
                        //        query.MAT_CLASS = "02";
                        //    else
                        //        query.MAT_CLASS = "XX";
                        //}
                        //else if (form.Get("TASKID") == "3") // 一般物品
                        //{
                        //    if (form.Get("p3") == "02")  // 一般物品的庫房人員不能看 物料分類02 的繳回單
                        //        query.MAT_CLASS = "XX";
                        //    else if (form.Get("p3") == "") // 空值則抓取 物料分類03~08
                        //        query.MAT_CLASS = "03-08";
                        //    else
                        //        query.MAT_CLASS = form.Get("p3");
                        //}

                    }
                    else
                    {
                        query.APPDEPT = form.Get("APPDEPT");    // 申請人員, 只能看到同單位的
                        //query.FLOWID = "1";
                    }
                    query.FLOWID = form.Get("p2");
                    string p2 = form.Get("p2");
                    string[] arr_p2 = { };
                    if (!string.IsNullOrEmpty(p2))
                    {
                        arr_p2 = p2.Trim().Split(','); //用,分割
                    }
                    string p3 = form.Get("p3");
                    string[] arr_p3 = { };
                    if (!string.IsNullOrEmpty(p3))
                    {
                        arr_p3 = p3.Trim().Split(','); //用,分割
                    }
                    query.DOCTYPE = "RN1";
                    query.APPTIME_S = "";
                    query.APPTIME_E = "";

                    if (form.Get("d0") != null && form.Get("d0") != "")
                        query.APPTIME_S = form.Get("d0").Split('T')[0];  // yyyy-mm-ddT00:00:00
                    if (form.Get("d1") != null && form.Get("d1") != "")
                        query.APPTIME_E = form.Get("d1").Split('T')[0];

                    session.Result.etts = repo.GetAll(query, arr_p2, arr_p3, page, limit, sorters);
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
                    AB0021Repository repo = new AB0021Repository(DBWork);
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
        public ApiResponse GetMatClassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo();
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
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    AB0021Repository.MI_MAST_QUERY_PARAMS query = new AB0021Repository.MI_MAST_QUERY_PARAMS();
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS");
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO");
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    //query.WH_NO = form.Get("WH_NO");

                    //// 需判斷庫存量>0
                    //if (form.Get("IS_INV") != null && form.Get("IS_INV") == "1")
                    //    query.IS_INV = form.Get("IS_INV");

                    //// 需判斷是否公藥
                    //query.E_IFPUBLIC = form.Get("E_IFPUBLIC") == null ? "" : form.Get("E_IFPUBLIC");
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
            var mat_class = form.Get("MAT_CLASS");
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
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, mat_class, wh_no, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDocnoCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    session.Result.etts = repo.GetDocnoCombo(form.Get("APPDEPT"), "RN1");
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0021Repository(DBWork);
                    AB0021Repository.ME_DOCD_QUERY_PARAMS query = new AB0021Repository.ME_DOCD_QUERY_PARAMS();
                    query.DOCNO = form.Get("p0");
                    query.FRWH = form.Get("FRWH");
                    query.TOWH = form.Get("TOWH");
                    session.Result.etts = repo.GetMeDocd(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse IsWhGrade1(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0021Repository(DBWork);
                    if (repo.IsWhGrade1(form.Get("INID")))
                        session.Result.success = true;
                    else
                        session.Result.success = false;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWhTaskId(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0021Repository(DBWork);
                    session.Result.etts = repo.GetWhTaskId(User.Identity.Name);
                    //session.Result.etts = repo.GetWhTaskId(form.Get("USERID"));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTask()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    session.Result.msg = repo.GetTaskid(User.Identity.Name);
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
        public ApiResponse MasterCreate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);

                    if (repo.CheckIsTowhCancelByWhno(form.Get("FRWH"))) {
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
                        me_docm.MAT_CLASS = form.Get("MAT_CLASS");        
                        me_docm.APPLY_NOTE = form.Get("APPLY_NOTE");
                        //me_docm.FRWH = repo.GeFrwh(User.Identity.Name);
                        me_docm.FRWH = form.Get("FRWH");
                        me_docm.TOWH = repo.GeTowh();  
                        me_docm.DOCTYPE = "RN1";
                        me_docm.FLOWID = "1";

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
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;
                    //if (!repo.CheckMeDocdExists(me_docm.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                    //{
                    session.Result.afrs = repo.MasterUpdate(me_docm);
                    //}
                    //else
                    //{
                        //session.Result.afrs = 0;
                        //session.Result.success = false;
                        //session.Result.msg = "<span style='color:red'>申請單號「" + me_docm.DOCNO + "」有院內碼項次，請先刪除所有項次才能修改。</span>";
                        //return session.Result;
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
        public ApiResponse MasterReject(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    ME_DOCM me_docm = new ME_DOCM();
                    me_docm.DOCNO = form.Get("DOCNO");
                    me_docm.FLOWID = "X"; // 退回至申請中
                    me_docm.APPLY_NOTE = form.Get("APPLY_NOTE");
                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterReject(me_docm);

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
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            //if (!repo1.CheckMeDocdExists(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            //{
                                repo.DetailAllDelete(tmp[i]);
                                session.Result.afrs = repo.MasterDelete(tmp[i]);
                            //}
                            //else
                            //{
                                //session.Result.afrs = 0;
                                //session.Result.success = false;
                                //session.Result.msg = "<span style='color:red'>繳回單號「" + tmp[i] + "」有院內碼項次，請先刪除所有項次。</span>";
                                //return session.Result;
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
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    AB0010Repository repo1 = new AB0010Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO2");
                    if (!repo1.CheckMeDocdExists(me_docd.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                        me_docd.SEQ = "1";
                    else
                        me_docd.SEQ = repo.GetMaxSeq(me_docd.DOCNO);
                    me_docd.MMCODE = form.Get("MMCODE");
                    me_docd.APPQTY = form.Get("APPQTY");
                    me_docd.APLYITEM_NOTE = form.Get("APLYITEM_NOTE");

                    if (repo.CheckMmcodeExist(me_docd.MMCODE, form.Get("MAT_CLASS2").Split(' ')[0], form.Get("FRWH1")) == false)
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此院內碼不存在</span>，請重新輸入院內碼。";
                        return session.Result;
                    }
                    if (Convert.ToInt32(me_docd.APPQTY) <= 0)
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>繳回數量必須大於0</span>，請重新繳回數量。";
                        return session.Result;
                    }
                    if (repo1.CheckMeDocdExists_1(me_docd)) {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此申請單已存在此院內碼</span>，請重新輸入院內碼。";
                        return session.Result;
                    }

                    if (repo.CheckPastYear(form.Get("FRWH1"), me_docd.MMCODE) == false) {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此院內碼一年內無調入或核撥記錄</span>，請重新輸入院內碼。";
                        return session.Result;
                    }
                    

                    me_docd.CREATE_USER = User.Identity.Name;
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.DetailCreate(me_docd);
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
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    ME_DOCD me_docd = new ME_DOCD();
                    me_docd.DOCNO = form.Get("DOCNO2");
                    me_docd.SEQ = form.Get("SEQ");
                    me_docd.MMCODE = form.Get("MMCODE");
                    me_docd.APPQTY = form.Get("APPQTY");
                    me_docd.APLYITEM_NOTE = form.Get("APLYITEM_NOTE");
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.DetailUpdate(me_docd);

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
        public ApiResponse UpdateStatus(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0021Repository repo = new AB0021Repository(DBWork);
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
                                session.Result.msg = string.Format("繳回單號「{0}」繳回庫房已作廢，請重新確認", tmp[i]);
                                return session.Result;
                            }

                            if (repo1.CheckMeDocdExists(tmp[i])) // 傳入DOCNO檢查繳回單是否有院內碼項次
                            {
                                if (repo1.CheckMeDocdAppqty(tmp[i]))
                                {
                                    ME_DOCM me_docm = new ME_DOCM();
                                    me_docm.DOCNO = tmp[i];
                                    //me_docm.FLOWID = form.Get("FLOWID");
                                    me_docm.FLOWID = "2";
                                    me_docm.UPDATE_USER = User.Identity.Name;
                                    me_docm.UPDATE_IP = DBWork.ProcIP;
                                    session.Result.afrs = repo.UpdateStatus(me_docm);
                                }
                                else
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<span style='color:red'>繳回單號「" + tmp[i] + "」院內碼項次繳回數量必須大於0</span>，請填寫繳回數量。";
                                    return session.Result;
                                }
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>繳回單號「" + tmp[i] + "」沒有院內碼項次</span>，請新增院內碼項次。";
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
        public ApiResponse DetailDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp_docno.Length; i++)
                            session.Result.afrs = repo.DetailDelete(tmp_docno[i], tmp_seq[i]);
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
                    AB0021Repository repo = new AB0021Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            SP_MODEL sp = repo.PostDoc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                            if (sp.O_RETID == "N")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = sp.O_ERRMSG;
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
        public ApiResponse GetFrwhCombo()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0021Repository repo = new AB0021Repository(DBWork);

                    if (repo.IsWhGradeUser(p0))
                    {
                        session.Result.etts = repo.GetFrwhCombo2();
                    }
                    else
                    {
                        session.Result.etts = repo.GetFrwhCombo(p0);

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
        public ApiResponse GetLoginInfo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0021Repository repo = new AB0021Repository(DBWork);
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
