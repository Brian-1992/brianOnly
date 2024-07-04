using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0126Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
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
                    var repo = new AA0126Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Get(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var loadType = form.Get("loadType");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0126Repository(DBWork);
                    session.Result.etts = repo.Get(p0, loadType);
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
        public ApiResponse Create(MI_MAST mi_mast)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0126Repository(DBWork);
                    if (!repo.CheckExists(mi_mast.MMCODE))
                    {
                        mi_mast.CREATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_IP = DBWork.ProcIP;

                        if (mi_mast.MIN_ORDQTY == "" || mi_mast.MIN_ORDQTY == null)
                            mi_mast.MIN_ORDQTY = "0";
                        if (mi_mast.EXCH_RATIO == "" || mi_mast.EXCH_RATIO == null)
                            mi_mast.EXCH_RATIO = "1";

                        session.Result.afrs = repo.Create(mi_mast);
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

        // 修改
        [HttpPost]
        public ApiResponse Update(MI_MAST mi_mast)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo1 = new AA0038Repository(DBWork);
                    if (mi_mast.CANCEL_ID == "Y" && repo1.CheckMmcodeRef(mi_mast.MMCODE))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "此院內碼已被參考，不可修改為作廢。";
                    }
                    else
                    {
                        var repo2 = new AA0126Repository(DBWork);
                        mi_mast.UPDATE_USER = User.Identity.Name;
                        mi_mast.UPDATE_IP = DBWork.ProcIP;

                        if (mi_mast.MIN_ORDQTY == "" || mi_mast.MIN_ORDQTY == null)
                            mi_mast.MIN_ORDQTY = "0";
                        if (mi_mast.EXCH_RATIO == "" || mi_mast.EXCH_RATIO == null)
                            mi_mast.EXCH_RATIO = "1";

                        session.Result.afrs = repo2.Update(mi_mast);
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
                    AA0126Repository repo = new AA0126Repository(DBWork);
                    AA0126Repository.MI_MAST_QUERY_PARAMS query = new AA0126Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    //query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
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
            //var wh_no = form.Get("WH_NO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0126Repository repo = new AA0126Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
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