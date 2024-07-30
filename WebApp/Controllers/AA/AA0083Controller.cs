using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;

namespace WebApp.Controllers.AA
{
    public class AA0083Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0083Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetUseQTY(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0083Repository(DBWork);
                    var P_YM = repo.GetP_YM();
                    if (!string.IsNullOrEmpty(P_YM))
                    {
                        session.Result.etts = repo.GetUseQTY(p0, P_YM);
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
        public ApiResponse GetDetail(FormDataCollection form)
        {
            var WH_NAME = form.Get("WH_NAME");
            var MMCODE = form.Get("MMCODE");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0083Repository(DBWork);
                    session.Result.etts = repo.GetDetail(WH_NAME, MMCODE);
                }
                catch(Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeDocd(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var mat_class = form.Get("mat_class");
            var m_stroeid = form.Get("m_stroeid");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0083Repository repo = new AA0083Repository(DBWork);
                    session.Result.etts = repo.GetMMCode(p0, mat_class, m_stroeid, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //===================================================================
    }
}
