using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BE;
using WebApp.Models.PH;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WebApp.Controllers.B
{
    public class BE0004Controller :  SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterCreate(PH_LOTNO ph_lotno)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BE0004Repository(DBWork);
                    ph_lotno.CREATE_USER = User.Identity.Name;
                    ph_lotno.UPDATE_USER = User.Identity.Name;
                    ph_lotno.UPDATE_IP = DBWork.ProcIP;
                    ph_lotno.SEQ = repo.getSeq();
                    session.Result.afrs = repo.MasterCreate(ph_lotno);
                    session.Result.etts = repo.MasterGet(ph_lotno.MMCODE, ph_lotno.SEQ);
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
        public ApiResponse MasterUpdate(PH_LOTNO ph_lotno)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
              
                try
                {
                    var repo = new BE0004Repository(DBWork);
                    ph_lotno.UPDATE_USER = User.Identity.Name;
                    ph_lotno.UPDATE_IP = DBWork.ProcIP;
                    if (ph_lotno.SOURCE == "V")
                    {
                        session.Result.afrs = repo.MasterUpdateM(ph_lotno);
                        session.Result.etts = repo.MasterGet(ph_lotno.MMCODE, ph_lotno.SEQ);
                    }
                    else
                    {
                        session.Result.afrs = repo.MasterUpdate(ph_lotno);
                        session.Result.etts = repo.MasterGet(ph_lotno.MMCODE, ph_lotno.SEQ);
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
        public ApiResponse MasterAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0004Repository(DBWork);
                    session.Result.etts = repo.GetMasterAll(p0,p1, p2, p3,p4,p5, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMatClassCombo() //FA0017
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0004Repository repo = new BE0004Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo();
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