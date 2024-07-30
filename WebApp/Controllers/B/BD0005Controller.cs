using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System.Collections;
using System.Collections.Generic;

namespace WebApp.Controllers.B
{
    public class BD0005Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            BD0005M v = new BD0005M();
            v.TWN_DATE_START = form.Get("TWN_DATE_START");
            v.TWN_DATE_END = form.Get("TWN_DATE_END");
            v.M_CONTID = form.Get("M_CONTID");
            v.MAT_CLASS = form.Get("P4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    v.UPDATE_USER = User.Identity.Name;
                    v.UPDATE_IP = DBWork.ProcIP;
                    var repo = new BD0005Repository(DBWork);
                    session.Result.etts = repo.GetAllM(v, page, limit, sorters);
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
            BD0005D qV = new BD0005D();
            qV.PO_NO = form.Get("PO_NO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0005Repository(DBWork);
                    session.Result.etts = repo.GetAllD(qV, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 修改
        [HttpPost]
        public ApiResponse Update(BD0005M v)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0005Repository(DBWork);
                    v.UPDATE_USER = User.Identity.Name;
                    v.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(v);
                    session.Result.etts = repo.Get(v);

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

        public IEnumerable<BD0005M> ReportsM(BD0005M v)
        {
            List<BD0005M> lst = new List<BD0005M>();
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0005Repository(DBWork);
                    lst = (List<BD0005M>)repo.ReportsM(v, 1, 99999, "");
                }
                catch
                {
                    throw;
                }
                return lst;
            }
        }

        public IEnumerable<BD0005M> ReportsD(BD0005M v)
        {
            List<BD0005M> lst = new List<BD0005M>();
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0005Repository(DBWork);
                    lst = (List<BD0005M>)repo.ReportsD(v, 1, 99999, "");
                }
                catch
                {
                    throw;
                }
                return lst;
            }
        }

        [HttpPost]
        public ApiResponse Create_ph_maillog(PH_MAILLOG v)
        {
            using (WorkSession session = new WorkSession()) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0005Repository(DBWork);
                    repo.Create_ph_maillog(v);
                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                }
                return session.Result;
            }
        }
        public ApiResponse UpdateStatus(BD0005M v)
        {
            using (WorkSession session = new WorkSession()) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0005Repository(DBWork);
                    repo.UpdateStatus(v);
                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                }
                return session.Result;
            }
        }

    }
}