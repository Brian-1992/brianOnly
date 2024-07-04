using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.C;

namespace WebApp.Controllers.C
{
    public class CE0014Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse Urid(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
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
                    var repo = new CE0014Repository(DBWork);
                    session.Result.etts = repo.GetUrid(p0, p1, p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse All(FormDataCollection form)
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
                    var repo = new CE0014Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Create(BC_WHCHKID bc_whchkid) {

            IEnumerable<BC_WHCHKID> bcWhchkids = JsonConvert.DeserializeObject<IEnumerable<BC_WHCHKID>>(bc_whchkid.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0014Repository(DBWork);

                    //Delete
                    foreach (BC_WHCHKID item in bcWhchkids)
                    {
                        if (repo.CheckExist(item)) {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = string.Format("此庫房已包含此人員({0})資料，將重新顯示資料",item.WH_CHKUID);

                            return session.Result;
                        }

                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(item);
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
        public ApiResponse Delete(BC_WHCHKID bc_whchkid)
        {

            IEnumerable<BC_WHCHKID> bcWhchkids = JsonConvert.DeserializeObject<IEnumerable<BC_WHCHKID>>(bc_whchkid.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CE0014Repository(DBWork);

                    //Delete
                    foreach (BC_WHCHKID item in bcWhchkids)
                    {
                        session.Result.afrs = repo.Delete(item);
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

        #region popup

        [HttpPost]
        public ApiResponse GetWhno(FormDataCollection form)
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
                    CE0014Repository repo = new CE0014Repository(DBWork);
                    CE0014Repository.MI_WHMAST_QUERY_PARAMS query = new CE0014Repository.MI_WHMAST_QUERY_PARAMS();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    query.WH_NAME = form.Get("WH_NAME") == null ? "" : form.Get("WH_NAME").ToUpper();
                    //query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    session.Result.etts = repo.GetWhno(query, page, limit, sorters)
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_GRADE = w.WH_GRADE });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        #endregion

        #region combo

        [HttpPost]
        public ApiResponse GetWhnoCombo(FormDataCollection form)
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
                    CE0014Repository repo = new CE0014Repository(DBWork);
                    session.Result.etts = repo
                        .GetWhnoCombo(p0, DBWork.UserInfo.UserId, page, limit, "")
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_GRADE = w.WH_GRADE });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetInidCombo(FormDataCollection form)
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
                    CE0014Repository repo = new CE0014Repository(DBWork);
                    session.Result.etts = repo.GetInidCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        #endregion

    }
}