using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;


namespace WebApp.Controllers.AB
{
    public class AB0042Controller : SiteBase.BaseApiController
    {
        // 查詢Master
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
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
                    var repo = new AB0042Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, page, limit, sorters);
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0042Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
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
        public ApiResponse CreateM(ME_PCAM ME_PCAM)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0042Repository(DBWork);
                    if (!repo.CheckExistsM(ME_PCAM.PCACODE)) // 新增前檢查主鍵是否已存在
                    {
                        ME_PCAM.CREATE_ID = User.Identity.Name;
                        ME_PCAM.UPDATE_ID = User.Identity.Name;
                        ME_PCAM.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateM(ME_PCAM);
                        //session.Result.etts = repo.GetM(PH_PUT_M.AGEN_NO, PH_PUT_M.MMCODE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>PCA固定處方頭</span>重複，請重新輸入。";
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
        public ApiResponse UpdateM(ME_PCAM ME_PCAM)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0042Repository(DBWork);
                    ME_PCAM.UPDATE_ID = User.Identity.Name;
                    ME_PCAM.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(ME_PCAM);
                    //session.Result.etts = repo.GetM(ME_PCAM.AGEN_NO);

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

        //刪除Master
        [HttpPost]
        public ApiResponse DeleteM(ME_PCAM ME_PCAM)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0042Repository(DBWork);
                    if (repo.CheckExistsM(ME_PCAM.PCACODE))
                    {
                        session.Result.afrs = repo.DeleteM_M(ME_PCAM);
                        session.Result.afrs = repo.DeleteM_D(ME_PCAM);
                        //session.Result.etts = repo.GetM(PH_PUT_M.AGEN_NO, PH_PUT_M.MMCODE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>PCA固定處方頭:" + ME_PCAM.PCACODE + " </span> 不存在。";
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

        //新增Detail
        [HttpPost]
        public ApiResponse CreateD(ME_PCAD ME_PCAD)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0042Repository(DBWork);
                    if (!repo.CheckExistsD(ME_PCAD.PCACODE, ME_PCAD.MMCODE)) // 新增前檢查主鍵是否已存在
                    {
                        ME_PCAD.CREATE_ID = User.Identity.Name;
                        ME_PCAD.UPDATE_ID = User.Identity.Name;
                        ME_PCAD.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateD(ME_PCAD);
                        //session.Result.etts = repo.GetD_2(ME_PCAD.AGEN_NO, PH_PUT_D.MMCODE, PH_PUT_D.TXTDAY, PH_PUT_D.SEQ);

                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>PCA固定處方頭及院內碼</span>重複，請重新輸入。";
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
        public ApiResponse UpdateD(ME_PCAD ME_PCAD)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0042Repository(DBWork);
                    ME_PCAD.UPDATE_ID = User.Identity.Name;
                    ME_PCAD.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(ME_PCAD);
                    //session.Result.etts = repo.GetD_1(PH_PUT_D.AGEN_NO, PH_PUT_D.MMCODE, PH_PUT_D.TXTDAY_TEXT, PH_PUT_D.SEQ);

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
        public ApiResponse DeleteD(ME_PCAD ME_PCAD)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0042Repository(DBWork);
                    if (repo.CheckExistsD(ME_PCAD.PCACODE, ME_PCAD.MMCODE))
                    {
                        session.Result.afrs = repo.DeleteD(ME_PCAD);
                        //session.Result.etts = repo.GetD(PH_PUT_D.AGEN_NO, PH_PUT_D.MMCODE, PH_PUT_D.TXTDAY_TEXT, PH_PUT_D.SEQ);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>PCA固定處方頭:" + ME_PCAD.PCACODE + " 院內碼:" + ME_PCAD.MMCODE + " </span> 不存在。";
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
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
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
                    AB0042Repository repo = new AB0042Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
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
                    AB0042Repository repo = new AB0042Repository(DBWork);
                    AB0042Repository.MI_MAST_QUERY_PARAMS query = new AB0042Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.E_ORDERUNIT = form.Get("E_ORDERUNIT") == null ? "" : form.Get("E_ORDERUNIT").ToUpper();
                    query.E_PATHNO = form.Get("E_PATHNO") == null ? "" : form.Get("E_PATHNO").ToUpper();
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
        //取得使用途徑及醫囑單位
        public ApiResponse GetE_ORDERUNIT(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0042Repository(DBWork);
                    session.Result.etts = repo.GetE_ORDERUNIT(form.Get("MMCODE"));
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