using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Controllers.AA
{
    public class AA0027Controller : SiteBase.BaseApiController
    {
        // 查詢Master
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = "";
            var p1 = "";
            if (form.Get("p0") != "")
            {
                p0 = form.Get("p0").Substring(0, 10).Replace("-", "/");
            }

            if (form.Get("p1") != "")
            {
                p1 = form.Get("p1").Substring(0, 10).Replace("-", "/");
            }

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
                    var repo = new AA0027Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢Detail
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
                    var repo = new AA0027Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //新增Master
        [HttpPost]
        public ApiResponse CreateM(AA0027M AA0027M)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0027Repository(DBWork);
                    if (!repo.CheckExistsM(AA0027M.DOCNO)) // 新增前檢查主鍵是否已存在
                    {
                        AA0027M.UPDATE_IP = DBWork.ProcIP;
                        AA0027M.APPID = User.Identity.Name;
                        AA0027M.CREATE_USER = User.Identity.Name;
                        AA0027M.UPDATE_USER = User.Identity.Name;
                        session.Result.afrs = repo.CreateM(AA0027M);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>單據號碼</span>重複，請重新輸入。";
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

        //修改Master
        [HttpPost]
        public ApiResponse UpdateM(AA0027M AA0027M)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0027Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(AA0027M.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    AA0027M.UPDATE_USER = User.Identity.Name;
                    AA0027M.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(AA0027M);

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

        //撤銷Master
        [HttpPost]
        public ApiResponse BackM(AA0027M AA0027M)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0027Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(AA0027M.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    AA0027M.UPDATE_USER = User.Identity.Name;
                    AA0027M.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.BackM(AA0027M);

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

        //送審核Master
        [HttpPost]
        public ApiResponse RunM(AA0027M AA0027M)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0027Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(AA0027M.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    var i = 0;

                    AA0027M.UPDATE_USER = User.Identity.Name;
                    AA0027M.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.RunM_1(AA0027M);

                    var datas = repo.RunM_2(AA0027M.DOCNO);
                    DBWork.Commit();

                    foreach (AA0027D data in datas)
                    {
                        DBWork.BeginTransaction();
                        i = i + 1;
                        data.UPDATE_USER = User.Identity.Name;
                        data.UPDATE_IP = DBWork.ProcIP;
                        data.SEQ = i.ToString();
                        session.Result.afrs = repo.RunM_3(data);

                        DBWork.Commit();
                    }
                    DBWork.BeginTransaction();
                    var retid = repo.POST_DOC(AA0027M.DOCNO, AA0027M.UPDATE_USER, AA0027M.UPDATE_IP);
                    if (retid.Substring(0, 1) == "Y")
                    {
                        DBWork.Commit();
                    }
                    else
                    {
                        DBWork.Rollback();
                        session.Result.msg = retid.Substring(2);
                        session.Result.success = false;
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

        //新增Detail
        [HttpPost]
        public ApiResponse CreateD(AA0027D AA0027D)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0027Repository(DBWork);
                    bool flowIdValid = repo.ChceckFlowId01(AA0027D.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    if (repo.CheckMmcode(AA0027D.FRWH, AA0027D.MMCODE, AA0027D.LOT_NO)) // 新增前檢查院內碼、批號是否有對應到
                    {

                        AA0027D.UPDATE_USER = User.Identity.Name;
                        AA0027D.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateD(AA0027D);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼內無此批號</span>，請重新輸入。";
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

        //修改Detail
        [HttpPost]
        public ApiResponse UpdateD(AA0027D AA0027D)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0027Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(AA0027D.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    AA0027D.UPDATE_USER = User.Identity.Name;
                    AA0027D.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(AA0027D);

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

        //刪除Detail
        [HttpPost]
        public ApiResponse DeleteD(FormDataCollection form)
        {
            var DOCNO = form.Get("DOCNO");
            var SEQ = form.Get("SEQ");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0027Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    session.Result.afrs = repo.DeleteD(DOCNO, SEQ);

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

        //調帳庫別combox
        public ApiResponse GetWH_NO(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0027Repository(DBWork);
                    session.Result.etts = repo.GetWH_NO();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //取得調帳單號
        public string GetDocno()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0027Repository(DBWork);
                    return repo.GetDocno();
                }
                catch
                {
                    throw;
                }
            }
        }

        //取得建立人員
        public string GetAPPID_NAME()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0027Repository(DBWork);
                    return repo.GetAPPID_NAME(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
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
                    var repo = new AA0027Repository(DBWork);
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
                    var repo = new AA0027Repository(DBWork);
                    return repo.GetEXP_DATE(FRWH, MMCODE, LOT_NO);
                }
                catch
                {
                    throw;
                }
            }
        }

        [HttpPost]
        //帶出效期數量
        public string GetINV_QTY(FormDataCollection form)
        {
            var FRWH = form.Get("FRWH");
            var MMCODE = form.Get("MMCODE");
            var LOT_NO = form.Get("LOT_NO");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0027Repository(DBWork);
                    return repo.GetINV_QTY(FRWH, MMCODE, LOT_NO);
                }
                catch
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0").ToUpper();
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
                    AA0027Repository repo = new AA0027Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, p1, page, limit, "");
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
                    AA0027Repository repo = new AA0027Repository(DBWork);
                    AA0027Repository.MI_MAST_QUERY_PARAMS query = new AA0027Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.FRWH = form.Get("FRWH") == null ? "" : form.Get("FRWH").ToUpper();
                    session.Result.etts = repo.GetMmcode(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //帶出進貨單價
        [HttpPost]
        public string GetM_DISCPERC(FormDataCollection form)
        {
            var WH_NO = form.Get("FRWH");
            var MMCODE = form.Get("MMCODE");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0027Repository(DBWork);
                    return repo.GetM_DISCPERC(WH_NO, MMCODE);
                }
                catch
                {
                    throw;
                }
            }
        }

        //帶出合約單價
        [HttpPost]
        public string GetM_CONTPRICE(FormDataCollection form)
        {
            var WH_NO = form.Get("FRWH");
            var MMCODE = form.Get("MMCODE");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0027Repository(DBWork);
                    return repo.GetM_CONTPRICE(WH_NO, MMCODE);
                }
                catch
                {
                    throw;
                }
            }
        }

    }
}