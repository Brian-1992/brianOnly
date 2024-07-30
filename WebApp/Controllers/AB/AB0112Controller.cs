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
    public class AB0112Controller : SiteBase.BaseApiController
    {
        // 查詢
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
                    var repo = new AB0112Repository(DBWork);
                    AB0112Repository.ME_DOCM_QUERY_PARAMS query = new AB0112Repository.ME_DOCM_QUERY_PARAMS();
                    query.MAT_CLASS = form.Get("p3");

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
                    query.APPTIME_S = "";
                    query.APPTIME_E = "";

                    if (form.Get("d0") != null && form.Get("d0") != "")
                        query.APPTIME_S = form.Get("d0").Split('T')[0];  // yyyy-mm-ddT00:00:00
                    if (form.Get("d1") != null && form.Get("d1") != "")
                        query.APPTIME_E = form.Get("d1").Split('T')[0];

                    query.USERID = DBWork.UserInfo.UserId;

                    session.Result.etts = repo.GetAll(query, arr_p2, arr_p3, page, limit, sorters);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetFlowidCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    session.Result.etts = repo.GetFlowidCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatClassQCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    session.Result.etts = repo.GetMatClassQCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatClassCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo(DBWork.UserInfo.UserId);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatClassComboByFrwh(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    session.Result.etts = repo.GetMatClassComboByFrwh(DBWork.UserInfo.UserId, form.Get("Frwh_no"));
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
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    AB0112Repository.MI_MAST_QUERY_PARAMS query = new AB0112Repository.MI_MAST_QUERY_PARAMS();
                    query.DOCNO = form.Get("DOCNO") == null ? "" : form.Get("DOCNO").ToUpper();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
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
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var docno = form.Get("DOCNO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, docno, page, limit, "");
                }
                catch (Exception e)
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
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    session.Result.etts = repo.GetDocnoCombo(form.Get("APPDEPT"), "RN1");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CheckExp(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0112Repository(DBWork);
                    session.Result.success = repo.CheckExp(form.Get("DOCNO"));
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢
        [HttpPost]
        public ApiResponse AllMeDocexp(FormDataCollection form)
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
                    var repo = new AB0112Repository(DBWork);
                    AB0112Repository.ME_DOCEMP_QUERY_PARAMS query = new AB0112Repository.ME_DOCEMP_QUERY_PARAMS();
                    query.DOCNO = form.Get("p0");
                    session.Result.etts = repo.GetMeDocexp(query, page, limit, sorters);
                }
                catch (Exception e)
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
                    var repo = new AB0112Repository(DBWork);
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
                    var repo = new AB0112Repository(DBWork);
                    session.Result.etts = repo.GetWhTaskId(User.Identity.Name);
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
                    AB0112Repository repo = new AB0112Repository(DBWork);
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
                    AB0112Repository repo = new AB0112Repository(DBWork);

                    if (!repo.CheckIsSameWhKind(form.Get("FRWH"), form.Get("TOWH")))
                    {
                        session.Result.success = false;
                        session.Result.msg = "出庫庫房與入庫庫房之庫別分類不同"; // WH_KIND 庫別分類(0藥品庫 1衛材庫 E能設 C通信)
                        return session.Result;
                    }

                    ME_DOCM me_docm = new ME_DOCM();
                    me_docm.DOCNO = repo.GetDailyDocno();
                    if (!repo.CheckExists(me_docm.DOCNO))
                    {
                        me_docm.CREATE_USER = User.Identity.Name;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        me_docm.APPID = User.Identity.Name;
                        me_docm.APPDEPT = form.Get("INID_NAME").Split(' ')[0];
                        me_docm.USEID = User.Identity.Name;
                        me_docm.MAT_CLASS = form.Get("MAT_CLASS");
                        me_docm.APPLY_NOTE = form.Get("APPLY_NOTE");
                        me_docm.FRWH = form.Get("FRWH");
                        me_docm.TOWH = form.Get("TOWH");
                        me_docm.DOCTYPE = form.Get("MAT_CLASS") == "01" ? "RN" : "RN1";
                        me_docm.FLOWID = form.Get("MAT_CLASS") == "01" ? "0401" : "1";

                        session.Result.afrs = repo.MasterCreate(me_docm);
                        session.Result.etts = repo.MasterGet(me_docm.DOCNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>繳回單號</span>重複，請重新嘗試。";
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
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterUpdate(me_docm);
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
                    AB0112Repository repo = new AB0112Repository(DBWork);
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
                    AB0112Repository repo = new AB0112Repository(DBWork);

                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            repo.DetailAllDelete(tmp[i]);
                            session.Result.afrs = repo.MasterDelete(tmp[i]);
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
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    ME_DOCEXP me_docexp = new ME_DOCEXP();
                    me_docexp.DOCNO = form.Get("DOCNO2");
                    me_docexp.MMCODE = form.Get("MMCODE");
                    me_docexp.APVQTY = form.Get("APVQTY");
                    me_docexp.LOT_NO = form.Get("LOT_NO");
                    me_docexp.EXP_DATE = form.Get("EXP_DATE");
                    me_docexp.ITEM_NOTE = form.Get("ITEM_NOTE");
                    me_docexp.SEQ = repo.GetDocexpSeq(me_docexp.DOCNO);

                    // 檢查此申請單是否已存在此院內碼
                    if (repo.CheckMeDocexpExists_1(me_docexp))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此申請單已存在此院內碼</span>，請重新確認。";
                        return session.Result;
                    }

                    // 檢查院內碼是否存在
                    if (repo.CheckMmcodeExist(me_docexp.DOCNO, me_docexp.MMCODE) == false)
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此院內碼不存在於庫存檔</span>，請重新確認。";
                        return session.Result;
                    }

                    // 檢查繳回數量是否大於0，若<=0跳出錯誤訊息
                    if (Convert.ToInt32(me_docexp.APVQTY) <= 0)
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>繳回數量必須大於0</span>，請重新確認。";
                        return session.Result;
                    }

                    // 檢查繳回數量是否大於庫存量，若<=0跳出錯誤訊息
                    if (repo.CheckAppqtyLargerThanInvqty(me_docexp.DOCNO, me_docexp.MMCODE, me_docexp.APVQTY))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>繳回數量超過現有庫存</span>，請重新確認。";
                        return session.Result;
                    }
                    
                    me_docexp.UPDATE_USER = User.Identity.Name;
                    me_docexp.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.DetailCreate(me_docexp);
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
        public ApiResponse DetailUpdate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    ME_DOCEXP me_docexp = new ME_DOCEXP();
                    me_docexp.DOCNO = form.Get("DOCNO2");
                    me_docexp.SEQ = form.Get("SEQ");
                    me_docexp.MMCODE = form.Get("MMCODE");
                    me_docexp.APVQTY = form.Get("APVQTY");
                    me_docexp.LOT_NO = form.Get("LOT_NO");
                    me_docexp.EXP_DATE = form.Get("EXP_DATE");
                    me_docexp.ITEM_NOTE = form.Get("ITEM_NOTE");
                    me_docexp.UPDATE_USER = User.Identity.Name;
                    me_docexp.UPDATE_IP = DBWork.ProcIP;
                    
                    // 檢查繳回數量是否大於庫存量，若<=0跳出錯誤訊息
                    if (repo.CheckAppqtyLargerThanInvqty(me_docexp.DOCNO, me_docexp.MMCODE, me_docexp.APVQTY))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>繳回數量超過現有庫存</span>，請重新確認。";
                        return session.Result;
                    }

                    // 檢查繳回數量是否大於庫存量，若<=0跳出錯誤訊息
                    if (repo.CheckAppqtyLargerThanInvqty(me_docexp.DOCNO, me_docexp.MMCODE, me_docexp.APVQTY))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>繳回數量超過現有庫存</span>，請重新確認。";
                        return session.Result;
                    }

                    session.Result.afrs = repo.DetailUpdate(me_docexp);

                    DBWork.Commit();
                }
                catch(Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        //送繳回
        [HttpPost]
        public ApiResponse UpdateStatus(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0112Repository repo = new AB0112Repository(DBWork);

                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckIsFrwhCancelByDocno(tmp[i]))
                            {
                                session.Result.success = false;
                                session.Result.msg = string.Format("繳回單號「{0}」繳回出庫庫房已作廢，請重新確認", tmp[i]);
                                return session.Result;
                            }

                            if (repo.CheckIsTowhCancelByDocno(tmp[i]))
                            {
                                session.Result.success = false;
                                session.Result.msg = string.Format("繳回單號「{0}」繳回入庫庫房已作廢，請重新確認", tmp[i]);
                                return session.Result;
                            }

                            if (repo.CheckMeDocexpExists(tmp[i])) // 傳入DOCNO檢查繳回單是否有院內碼項次
                            {
                                if (repo.CheckMeDocexpAppqty(tmp[i])) // 檢查申請單院內碼項次的申請數量不得<=0
                                {
                                    if (repo.CheckAppqtyInvqty(tmp[i])) // 檢查申請單院內碼項次的繳回數量不可>現有庫存量,有的話則不可繳回
                                    {
                                        ME_DOCM me_docm = new ME_DOCM();
                                        me_docm.DOCNO = tmp[i]; ;
                                        me_docm.UPDATE_USER = User.Identity.Name;
                                        me_docm.UPDATE_IP = DBWork.ProcIP;
                                        session.Result.afrs = repo.UpdateStatus(me_docm);
                                    }
                                    else
                                    {
                                        session.Result.afrs = 0;
                                        session.Result.success = false;
                                        session.Result.msg = "<span style='color:red'>繳回單號「" + tmp[i] + "」院內碼項次繳回數量大於現有庫存量</span>。";
                                        return session.Result;
                                    }
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
                catch(Exception e)
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
                    AB0112Repository repo = new AB0112Repository(DBWork);
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
                catch(Exception e)
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
                    AB0112Repository repo = new AB0112Repository(DBWork);
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
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    session.Result.etts = repo.GetFrwhCombo(User.Identity.Name);
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
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo(form.Get("Frwh_no"));
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
                    AB0112Repository repo = new AB0112Repository(DBWork);
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
        public ApiResponse UpdateStatus1(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0112Repository repo = new AB0112Repository(DBWork);

                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string msg = string.Empty;
                        List<string> DocmStatusError = new List<string>();    // 此申請單狀態非核撥中
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM1(tmp[i]))
                            {
                                DocmStatusError.Add(tmp[i]);
                                continue;
                            }
                        }
                        if (DocmStatusError.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 繳回單狀態非藥品點收中或衛材繳回中</span>：<br/>";
                            foreach (string temp in DocmStatusError)
                            {
                                if (tempString.Length > 0)
                                {
                                    tempString += "、";
                                }
                                tempString += temp;
                            }
                            msg += tempString;
                        }
                        // 若msg不為空，表示有錯誤
                        if (msg != string.Empty)
                        {
                            session.Result.success = false;
                            session.Result.msg = msg;
                            return session.Result;
                        }
                        // 所有申請單通過檢核，進行更新
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i]; ;
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.UpdateStatus1(me_docm);
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
        public ApiResponse GetLotno(FormDataCollection form)
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
                    AB0112Repository repo = new AB0112Repository(DBWork);
                    var mmcode = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    var docno = form.Get("DOCNO") == null ? "" : form.Get("DOCNO").ToUpper();
                    session.Result.etts = repo.GetLotno(mmcode, docno, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        //批號+效期+效期數量combox
        public ApiResponse GetLOT_NO(FormDataCollection form)
        {
            var FRWH = form.Get("FRWH");
            var MMCODE = form.Get("MMCODE");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0112Repository(DBWork);
                    session.Result.etts = repo.GetLOT_NO(FRWH, MMCODE);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        //帶出效期
        public string GetEXP_DATE(FormDataCollection form)
        {
            var FRWH = form.Get("FRWH");
            var MMCODE = form.Get("MMCODE");
            var LOT_NO = form.Get("LOT_NO");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0112Repository(DBWork);
                    return repo.GetEXP_DATE(FRWH, MMCODE, LOT_NO);
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}
