using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.CB;
using WebApp.Models;
using System;
using System.Linq;

namespace WebApp.Controllers.CB
{
    public class CB0010Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
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
                    var repo = new CB0010Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, page, limit, sorters);
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
        public ApiResponse Create(BC_STLOC bC_STLOC)
        {


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CB0010Repository(DBWork);

                    if (!repo.CheckExists(bC_STLOC))
                    {
                        bC_STLOC.CREATE_USER = DBWork.ProcUser;
                        bC_STLOC.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(bC_STLOC);
                        session.Result.etts = repo.Get(bC_STLOC.WH_NO, bC_STLOC.STORE_LOC);
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = "庫房代碼&儲位代碼重複無法新增";

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
        public ApiResponse Update(BC_STLOC bC_STLOC)
        {
            

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {

                    var repo = new CB0010Repository(DBWork);

                    if (repo.CheckExists(bC_STLOC))
                    {
                        bC_STLOC.UPDATE_USER = User.Identity.Name;
                        bC_STLOC.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Update(bC_STLOC);

                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = "庫房代碼&儲位代碼不存在無法修改";
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

        // 刪除
        [HttpPost]
        public ApiResponse Delete(BC_STLOC bC_STLOC)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {

                    var repo = new CB0010Repository(DBWork);

                    if (repo.CheckExistsMI_WLOCINV(bC_STLOC))
                    {
                        session.Result.afrs = repo.Delete(bC_STLOC);

                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = "庫房仍有庫存,不可以刪除";

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

        public ApiResponse GetXcategoryCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0010Repository(DBWork);
                    session.Result.etts = repo.GetXcategoryCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH_NoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            //var wh_no = form.Get("WH_NO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CB0010Repository repo = new CB0010Repository(DBWork);
                    session.Result.etts = repo
                        .GetWH_NoCombo(p0, page, limit, "")
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_GRADE = w.WH_GRADE });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse Excel(FormDataCollection form)
        {
            var WH_NO = form.Get("WH_NO");
            var STORE_LOC = form.Get("STORE_LOC");
            var FLAG = form.Get("FLAG");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CB0010Repository repo = new CB0010Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    JCLib.Excel.Export("儲位條碼對照維護" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(WH_NO, STORE_LOC, FLAG));
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