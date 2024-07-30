using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using Dapper;
using WebApp.Repository.AB;
using WebApp.Models;
using System;
using System.Collections.Generic;
using WebApp.Controllers.AA;
using Newtonsoft.Json;

namespace WebApp.Controllers.AB
{
    public class AB0029Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var fid = form.Get("fid");
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0029Repository(DBWork);
                    session.Result.etts = repo.GetAllM(fid, User.Identity.Name, p0, p1, p2, p3, p4, p5, p6, p7, page, limit, sorters);
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
                    var repo = new AB0029Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAllWH(FormDataCollection form)
        {
            var wh_no = form.Get("wh_no");
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0029Repository(DBWork);
                    session.Result.etts = repo.GetAllWH(wh_no, p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse CreateM(ME_DOCM pME_DOCM)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try
                {
                    var repo = new AB0029Repository(DBWork);

                    if (repo.CheckIsTowhCancelByWhno(pME_DOCM.TOWH))
                    {
                        session.Result.success = false;
                        session.Result.msg = "調入庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    if (repo.CheckIsTowhCancelByWhno(pME_DOCM.FRWH))
                    {
                        session.Result.success = false;
                        session.Result.msg = "調出庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    string hospCode = repo.GetHospCode();
                    var v_docno = "";
                    if (hospCode == "0")
                    {
                        v_docno = repo.GetDocno();   //三總用
                    }
                    else
                    {
                        v_docno = repo.GetDailyDocno();  //國軍用
                    }
                    if (!repo.CheckExists(v_docno))
                    {
                        pME_DOCM.DOCNO = v_docno;
                        pME_DOCM.APPID = User.Identity.Name;
                        //pME_DOCM.DOCTYPE = "TR";,TR1
                        pME_DOCM.FLOWID = "0201";
                        //pME_DOCM.APPDEPT = 暫時
                        pME_DOCM.USEID = User.Identity.Name;
                        //pME_DOCM.USEDEPT = 暫時 pME_DOCM.APPDEPT;
                        pME_DOCM.CREATE_USER = User.Identity.Name;
                        pME_DOCM.UPDATE_USER = User.Identity.Name;
                        pME_DOCM.UPDATE_IP = DBWork.ProcIP;
                        pME_DOCM.MAT_CLASS = null;
                        session.Result.afrs = repo.CreateM(pME_DOCM);
                        session.Result.etts = repo.GetM(pME_DOCM.DOCNO);
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
        public ApiResponse DeleteM(ME_DOCM pME_DOCM)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0029Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(pME_DOCM.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    session.Result.afrs = repo.DeleteAllD(pME_DOCM);
                    session.Result.afrs = repo.DeleteM(pME_DOCM);
                    session.Result.etts = repo.GetM(pME_DOCM.DOCNO);

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
        // 退回
        [HttpPost]
        public ApiResponse UpdateFLOWID0201(ME_DOCM pME_DOCM)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0029Repository(DBWork);
                    pME_DOCM.UPDATE_USER = User.Identity.Name;
                    pME_DOCM.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateFLOWID(pME_DOCM);
                    session.Result.etts = repo.GetM(pME_DOCM.DOCNO);

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
        // 提出申請
        [HttpPost]
        public ApiResponse UpdateFLOWID0202(ME_DOCM pME_DOCM)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0029Repository(DBWork);

                    if (repo.CheckIsTowhCancelByDocno(pME_DOCM.DOCNO, "TOWH"))
                    {
                        session.Result.success = false;
                        session.Result.msg = "調入庫房已作廢，請重新選擇";
                        return session.Result;
                    }
                    if (repo.CheckIsTowhCancelByDocno(pME_DOCM.DOCNO, "FRWH"))
                    {
                        session.Result.success = false;
                        session.Result.msg = "調出庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    bool flowIdValid = repo.ChceckFlowId01(pME_DOCM.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    pME_DOCM.UPDATE_USER = User.Identity.Name;
                    pME_DOCM.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateFLOWID(pME_DOCM);
                    session.Result.afrs = repo.UpdateALLApvQty(pME_DOCM);//提出申請時,預設核撥數量=申請數量
                    session.Result.etts = repo.GetM(pME_DOCM.DOCNO);

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
        // 取消調撥
        [HttpPost]
        public ApiResponse UpdateFLOWID0204(ME_DOCM pME_DOCM)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0029Repository(DBWork);
                    pME_DOCM.UPDATE_USER = User.Identity.Name;
                    pME_DOCM.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.ClearAcktime(pME_DOCM);
                    session.Result.afrs = repo.UpdateFLOWID0204(pME_DOCM);

                    SP_MODEL sp = repo.POST_DOC(pME_DOCM.DOCNO, User.Identity.Name, DBWork.ProcIP);

                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = sp.O_ERRMSG;
                        DBWork.Rollback();
                    }
                    else
                    {
                        DBWork.Commit();
                        session.Result.etts = repo.GetM(pME_DOCM.DOCNO);
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
        // 確認入庫、執行調撥
        [HttpPost]
        public ApiResponse UpdateFLOWID(ME_DOCM pME_DOCM)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0029Repository(DBWork);

                    pME_DOCM.UPDATE_USER = User.Identity.Name;
                    pME_DOCM.UPDATE_IP = DBWork.ProcIP;
                    if (pME_DOCM.FLOWID == "0203") //執行調撥
                    {
                        ME_DOCD d = new ME_DOCD();
                        d.DOCNO = pME_DOCM.DOCNO;
                        d.APVID = User.Identity.Name;
                        d.UPDATE_USER = User.Identity.Name;
                        d.UPDATE_IP = DBWork.ProcIP;
                        repo.UpdateALLApvid(d);
                    }
                    else //確認入庫
                    {
                        ME_DOCD d = new ME_DOCD();
                        d.DOCNO = pME_DOCM.DOCNO;
                        d.ACKID = User.Identity.Name;
                        d.UPDATE_USER = User.Identity.Name;
                        d.UPDATE_IP = DBWork.ProcIP;
                        repo.UpdateALLAck(d);
                        if (repo.CheckExistsMI_WHMAST(pME_DOCM.TOWH))
                        {
                            repo.UpdateFstackDate(pME_DOCM.TOWH, pME_DOCM.DOCNO);
                        }

                        if (pME_DOCM.UP == null) { pME_DOCM.UP = "[]"; }
                        // 更新批號效期及儲位的庫存量
                        IEnumerable<ME_DOCD> data_list = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(pME_DOCM.UP);
                        AA0147Controller cont = new AA0147Controller();
                        if (data_list == null) data_list = new List<ME_DOCD>();
                        cont.updateExpLocInv(data_list, DBWork);
                    }
                    SP_MODEL sp = repo.POST_DOC(pME_DOCM.DOCNO, User.Identity.Name, DBWork.ProcIP);
                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = sp.O_ERRMSG;
                        DBWork.Rollback();
                    }
                    else
                    {
                        DBWork.Commit();
                        session.Result.etts = repo.GetM(pME_DOCM.DOCNO);
                    }
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
        public ApiResponse UpdateM(ME_DOCM pME_DOCM)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0029Repository(DBWork);

                    if (repo.CheckIsTowhCancelByWhno(pME_DOCM.FRWH))
                    {
                        session.Result.success = false;
                        session.Result.msg = "調出庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    if (repo.CheckIsTowhCancelByWhno(pME_DOCM.TOWH))
                    {
                        session.Result.success = false;
                        session.Result.msg = "調入庫房已作廢，請重新選擇";
                        return session.Result;
                    }

                    bool flowIdValid = repo.ChceckFlowId01(pME_DOCM.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    pME_DOCM.UPDATE_USER = User.Identity.Name;
                    pME_DOCM.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(pME_DOCM);
                    session.Result.etts = repo.GetM(pME_DOCM.DOCNO);

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
        public ApiResponse CreateD(ME_DOCD pME_DOCD)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0029Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(pME_DOCD.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    //2023-08-09新增
                    if (repo.CheckCancelIdY(pME_DOCD.MMCODE))
                    {
                        session.Result.msg = "院內碼已全院停用，請重新確認";
                        session.Result.success = false;
                        return session.Result;
                    }

                    if (repo.CheckFrwhMmcodeValid(pME_DOCD.DOCNO, pME_DOCD.MMCODE) == false)
                    {
                        session.Result.msg = "院內碼不存在於調出庫房，請檢查庫別是否正確";
                        session.Result.success = false;
                        return session.Result;
                    }

                    //2023-08-09新增
                    if (repo.CheckFrwhInvqty0(pME_DOCD.DOCNO, pME_DOCD.MMCODE) == false)
                    {
                        session.Result.msg = "調出庫房無庫存量，請重新確認";
                        session.Result.success = false;
                        return session.Result;
                    }

                    if (!repo.CheckExistsDKey(pME_DOCD.DOCNO, pME_DOCD.MMCODE)) // 新增前檢查主鍵是否已存在
                    {
                        pME_DOCD.SEQ = repo.GetDocDSeq(pME_DOCD.DOCNO);
                        pME_DOCD.CREATE_USER = User.Identity.Name;
                        pME_DOCD.UPDATE_USER = User.Identity.Name;
                        pME_DOCD.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateD(pME_DOCD);
                        session.Result.etts = repo.GetD(pME_DOCD.DOCNO, pME_DOCD.SEQ);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼</span>重複，請重新輸入。";
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
        public ApiResponse DeleteD(ME_DOCD ME_DOCD)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0029Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(ME_DOCD.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    session.Result.afrs = repo.DeleteD(ME_DOCD);
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
        // 修改申請數量
        [HttpPost]
        public ApiResponse UpdateD(ME_DOCD ME_DOCD)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0029Repository(DBWork);

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
        // 修改調撥數量
        [HttpPost]
        public ApiResponse UpdateDApvQty(ME_DOCD ME_DOCD)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0029Repository(DBWork);
                    ME_DOCD.APVID = User.Identity.Name;
                    ME_DOCD.UPDATE_USER = User.Identity.Name;
                    ME_DOCD.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateDApvQty(ME_DOCD);
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
        // 修改點收數量
        [HttpPost]
        public ApiResponse UpdateDAckQty(ME_DOCD pME_DOCD)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0029Repository(DBWork);
                    pME_DOCD.ACKID = User.Identity.Name;
                    pME_DOCD.UPDATE_USER = User.Identity.Name;
                    pME_DOCD.UPDATE_IP = DBWork.ProcIP;
                    ME_DOCC c = new ME_DOCC
                    {
                        DOCNO = pME_DOCD.DOCNO,
                        SEQ = pME_DOCD.SEQ,
                        CHECKSEQ = 0,
                        GENWAY = "1",
                        ACKQTY = pME_DOCD.ACKQTY,
                        ACKID = pME_DOCD.ACKID,
                        CREATE_USER = User.Identity.Name,
                        UPDATE_USER = pME_DOCD.UPDATE_USER,
                        UPDATE_IP = pME_DOCD.UPDATE_IP
                    };
                    if (pME_DOCD.TRNAB_QTY == "" || pME_DOCD.TRNAB_QTY == null)
                        pME_DOCD.TRNAB_QTY = "0";
                    if (pME_DOCD.TRNAB_QTY == "0" && pME_DOCD.TRNAB_RESON != "")
                        pME_DOCD.TRNAB_RESON = "";
                    session.Result.afrs = repo.UpdateDAckQty(pME_DOCD);
                    if (repo.CheckExistsME_DOCC(pME_DOCD.DOCNO, pME_DOCD.SEQ))
                    {
                        session.Result.afrs = repo.UpdateME_DOCC(c);
                    }
                    else
                    {
                        session.Result.afrs = repo.CreateME_DOCC(c);
                    }

                    session.Result.etts = repo.GetD(pME_DOCD.DOCNO, pME_DOCD.SEQ);
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
        public ApiResponse GetInidCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {

                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0029Repository repo = new AB0029Repository(DBWork);
                    session.Result.etts = repo.GetInidCombo(p0, User.Identity.Name);
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
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0029Repository repo = new AB0029Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo(p0, User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetQFrwhCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0029Repository repo = new AB0029Repository(DBWork);
                    session.Result.etts = repo.GetQFrwhCombo(p0, User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetFrwhCombo(FormDataCollection form)
        {
            var action = form.Get("action");
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0029Repository repo = new AB0029Repository(DBWork);
                    session.Result.etts = repo.GetFrwhCombo(action, p0);
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
                    AB0029Repository repo = new AB0029Repository(DBWork);
                    AB0029Repository.MI_MAST_QUERY_PARAMS query = new AB0029Repository.MI_MAST_QUERY_PARAMS();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
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
        public ApiResponse GetMmcodeCombo(FormDataCollection form)
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
                    AB0029Repository repo = new AB0029Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetTrnabResonCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0029Repository repo = new AB0029Repository(DBWork);
                    session.Result.etts = repo.GetTrnabResonCombo();
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAllWexp(FormDataCollection form)
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
                    var repo = new AB0029Repository(DBWork);
                    session.Result.etts = repo.GetAllWexp(p0, p1, page, limit, sorters);
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
            var tuser = User.Identity.Name;
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0029Repository repo = new AB0029Repository(DBWork);
                    JCLib.Excel.Export("AB0029.xls", repo.GetExcel(tuser, p0, p1, p2, p3, p4, p5, p6, p7));
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
                    AB0029Repository repo = new AB0029Repository(DBWork);
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
        public ApiResponse Cancel_apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0029Repository repo = new AB0029Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM(tmp[i], "0202"))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>此申請單狀態非調出中</span>不得取消送申請。";
                            }
                            else
                            {
                                ME_DOCM me_docm = new ME_DOCM();
                                me_docm.DOCNO = tmp[i];
                                me_docm.FLOWID = "0201";  //調撥申請
                                me_docm.SENDAPVID = User.Identity.Name;
                                me_docm.UPDATE_USER = User.Identity.Name;
                                me_docm.UPDATE_IP = DBWork.ProcIP;
                                session.Result.afrs = repo.UpdateFLOWID(me_docm);
                                session.Result.afrs = repo.UpdateALLApvQtyForCancel(me_docm);//退回申請時,預設核撥數量=0
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
    }
}