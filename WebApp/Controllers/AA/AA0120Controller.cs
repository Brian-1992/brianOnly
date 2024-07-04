using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0120Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            //var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0120Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢MI_MAST
        [HttpPost]
        public ApiResponse GetMiMast(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            //var p2 = form.Get("p2");
            //var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0120Repository(DBWork);
                    session.Result.etts = repo.GetMiMast();
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
                    AA0120Repository repo = new AA0120Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 查詢MI_UNITCODE
        [HttpPost]
        public ApiResponse GetMiUnitcode(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            //var p2 = form.Get("p2");
            //var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0120Repository(DBWork);
                    session.Result.etts = repo.GetMiUnitcode();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetPhVendor(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            //var p2 = form.Get("p2");
            //var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0120Repository(DBWork);
                    session.Result.etts = repo.GetPhVendor();
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
        public ApiResponse Create(MI_UNITEXCH mi_unitexch)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0120Repository(DBWork);
                    if (!repo.CheckExists(mi_unitexch.MMCODE, mi_unitexch.UNIT_CODE, mi_unitexch.AGEN_NO)) // 新增前檢查主鍵是否已存在
                    {
                        mi_unitexch.CREATE_USER = User.Identity.Name;
                        mi_unitexch.UPDATE_USER = User.Identity.Name;
                        mi_unitexch.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(mi_unitexch);
                        session.Result.etts = repo.Get(mi_unitexch.MMCODE, mi_unitexch.UNIT_CODE, mi_unitexch.AGEN_NO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼+計量單位代碼+廠商代碼</span>重複，請重新輸入。";
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
        public ApiResponse Update(MI_UNITEXCH mi_unitexch)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0120Repository(DBWork);
                    //if (!repo.CheckExists(mi_unitexch.MMCODE, mi_unitexch.UNIT_CODE, mi_unitexch.AGEN_NO)) // 更新前檢查主鍵是否已存在
                    //{
                        mi_unitexch.UPDATE_USER = User.Identity.Name;
                        mi_unitexch.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Update(mi_unitexch);
                        session.Result.etts = repo.Get(mi_unitexch.MMCODE, mi_unitexch.UNIT_CODE, mi_unitexch.AGEN_NO);
                    //}
                    //else
                    //{
                    //    session.Result.afrs = 0;
                    //    session.Result.success = false;
                    //    session.Result.msg = "<span style='color:red'>院內碼+計量單位代碼+廠商代碼</span>重複，請重新輸入。";
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
    }
}
