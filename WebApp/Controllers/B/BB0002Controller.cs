using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;


namespace WebApp.Controllers.B
{
    public class BB0002Controller : SiteBase.BaseApiController
    {
        // 查詢Master
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
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
                    var repo = new BB0002Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, p3, page, limit, sorters);
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
                    var repo = new BB0002Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, p2, page, limit, sorters);
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
        public ApiResponse CreateM(PH_PUT_M PH_PUT_M)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BB0002Repository(DBWork);
                    if (!repo.CheckExistsM(PH_PUT_M.AGEN_NO, PH_PUT_M.MMCODE, PH_PUT_M.DEPT)) // 新增前檢查主鍵是否已存在
                    {
                        var aa = PH_PUT_M.MMNAME_C;

                        PH_PUT_M.CREATE_USER = User.Identity.Name;
                        PH_PUT_M.UPDATE_USER = User.Identity.Name;
                        PH_PUT_M.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateM(PH_PUT_M);
                        session.Result.etts = repo.GetM(PH_PUT_M.AGEN_NO, PH_PUT_M.MMCODE, PH_PUT_M.DEPT);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>廠商碼、院內碼及寄放責任中心</span>重複，請重新輸入。";
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
        public ApiResponse UpdateM(PH_PUT_M PH_PUT_M)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BB0002Repository(DBWork);
                    PH_PUT_M.UPDATE_USER = User.Identity.Name;
                    PH_PUT_M.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(PH_PUT_M);
                    session.Result.etts = repo.GetM(PH_PUT_M.AGEN_NO, PH_PUT_M.MMCODE, PH_PUT_M.DEPT);

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
        public ApiResponse DeleteM(PH_PUT_M PH_PUT_M)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BB0002Repository(DBWork);
                    if (repo.CheckExistsM(PH_PUT_M.AGEN_NO, PH_PUT_M.MMCODE, PH_PUT_M.DEPT))
                    {
                        session.Result.afrs = repo.DeleteM(PH_PUT_M);
                        session.Result.etts = repo.GetM(PH_PUT_M.AGEN_NO, PH_PUT_M.MMCODE, PH_PUT_M.DEPT);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>廠商碼:" + PH_PUT_M.AGEN_NO + " 院內碼:" + PH_PUT_M.MMCODE + " </span> 不存在。";
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
        public ApiResponse CreateD(PH_PUT_D PH_PUT_D)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BB0002Repository(DBWork);
                    if (!repo.CheckExistsD_1(PH_PUT_D.AGEN_NO, PH_PUT_D.MMCODE, PH_PUT_D.TXTDAY, PH_PUT_D.SEQ, PH_PUT_D.DEPT)) // 新增前檢查主鍵是否已存在
                    {
                        PH_PUT_D.CREATE_USER = User.Identity.Name;
                        PH_PUT_D.UPDATE_USER = User.Identity.Name;
                        PH_PUT_D.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateD(PH_PUT_D);
                        session.Result.etts = repo.GetD_2(PH_PUT_D.AGEN_NO, PH_PUT_D.MMCODE, PH_PUT_D.TXTDAY, PH_PUT_D.SEQ, PH_PUT_D.DEPT);
                        if (PH_PUT_D.EXTYPE == "20")
                        {
                            session.Result.afrs = repo.UpdateM_Qty(PH_PUT_D);
                        }

                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>廠商碼、院內碼及交易日期</span>重複，請重新輸入。";
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
        public ApiResponse UpdateD(PH_PUT_D PH_PUT_D)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BB0002Repository(DBWork);
                    PH_PUT_D.UPDATE_USER = User.Identity.Name;
                    PH_PUT_D.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(PH_PUT_D);
                    session.Result.etts = repo.GetD_1(PH_PUT_D.AGEN_NO, PH_PUT_D.MMCODE, PH_PUT_D.TXTDAY_TEXT, PH_PUT_D.SEQ, PH_PUT_D.DEPT);

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
        public ApiResponse DeleteD(PH_PUT_D PH_PUT_D)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BB0002Repository(DBWork);
                    if (repo.CheckExistsD_2(PH_PUT_D.AGEN_NO, PH_PUT_D.MMCODE, PH_PUT_D.TXTDAY_TEXT, PH_PUT_D.SEQ, PH_PUT_D.DEPT))
                    {
                        session.Result.afrs = repo.DeleteD(PH_PUT_D);
                        session.Result.etts = repo.GetD_1(PH_PUT_D.AGEN_NO, PH_PUT_D.MMCODE, PH_PUT_D.TXTDAY_TEXT, PH_PUT_D.SEQ, PH_PUT_D.DEPT);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>廠商碼:" + PH_PUT_D.AGEN_NO + " 院內碼:" + PH_PUT_D.MMCODE + " 交易日期:" + PH_PUT_D.TXTDAY + " </span> 不存在。";
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

        //廠商碼combox
        public ApiResponse GetAGEN_NO()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BB0002Repository(DBWork);
                    session.Result.etts = repo.GetAGEN_NO();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //寄放責任中心combox
        public ApiResponse GetINIDNAME()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BB0002Repository(DBWork);
                    session.Result.etts = repo.GetINIDNAME();
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
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BB0002Repository repo = new BB0002Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3));
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BB0002Repository repo = new BB0002Repository(DBWork);
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
                    BB0002Repository repo = new BB0002Repository(DBWork);
                    BB0002Repository.MI_MAST_QUERY_PARAMS query = new BB0002Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    session.Result.etts = repo.GetMmcode(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 依院內碼帶出廠商碼
        [HttpPost]
        public string GetAgenno(FormDataCollection form)
        {
            var MMCODE = form.Get("MMCODE");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BB0002Repository(DBWork);
                    var AGENNNO = repo.GetAgenno(MMCODE);
                    return AGENNNO;
                }
                catch
                {
                    throw;
                }
            }
        }

        //匯出帳務明細
        [HttpPost]
        public ApiResponse Excel_D(FormDataCollection form)
        {
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BB0002Repository repo = new BB0002Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel_D(p4, p5));
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