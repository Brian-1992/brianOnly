using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System;
namespace WebApp.Controllers.B
{
    public class BD0003Controller : SiteBase.BaseApiController
    {

        [HttpPost]
        public ApiResponse GetPH_MAILSP(FormDataCollection form)
        {
            //var p0 = form.Get("p0");
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
                    var repo = new BD0003Repository(DBWork);
                    session.Result.etts = repo.GetPH_MAILSP_ALL(p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse MasterCreateBD(PH_MAILSP ph_mailsp)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0003Repository(DBWork);
                    if (!repo.CheckExistsPH_MAILSP(ph_mailsp.INID, ph_mailsp.M_CONTID, ph_mailsp.MAT_CLASS))
                    {
                        ph_mailsp.CREATE_USER = User.Identity.Name;
                        ph_mailsp.UPDATE_USER = User.Identity.Name;
                        ph_mailsp.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreatePH_MAILSP(ph_mailsp);
                        session.Result.etts = repo.GetPH_MAILSP(ph_mailsp.INID, ph_mailsp.M_CONTID, ph_mailsp.MAT_CLASS);
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "MAIL訊息<span style='color:red'>重複</span>，請重新輸入。";
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
        [HttpPost]
        public ApiResponse MasterUpdateBD(PH_MAILSP ph_mailsp)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0003Repository(DBWork);
                    ph_mailsp.UPDATE_USER = User.Identity.Name;
                    ph_mailsp.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdatePH_MAILSP(ph_mailsp);
                    session.Result.etts = repo.GetPH_MAILSP(ph_mailsp.INID, ph_mailsp.M_CONTID, ph_mailsp.MAT_CLASS);

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
        public ApiResponse MasterDeleteBD(PH_MAILSP ph_mailsp)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0003Repository(DBWork);

                    session.Result.afrs = repo.DeletePH_MAILSP(ph_mailsp.INID, ph_mailsp.M_CONTID, ph_mailsp.MAT_CLASS);

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
        // ===============================================
        /*   for AA0051  table PH_MAILSP_M, PH_MAILSP_D */
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            var p5 = form.Get("p5");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0003Repository(DBWork);
                    session.Result.etts = repo.GetMasterAll(p1, p2, p3, page, limit, sorters,p5);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterCreate(PH_MAILSP_M ph_mailsp_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0003Repository(DBWork);
                    //ph_mailsp_m.MGROUP = (repo.getUserInid(User.Identity.Name));
                    if (!repo.CheckExists(ph_mailsp_m.MSGRECNO, ph_mailsp_m.MSGNO))
                    {
                        ph_mailsp_m.CREATE_USER = User.Identity.Name;
                        ph_mailsp_m.UPDATE_USER = User.Identity.Name;
                        ph_mailsp_m.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.MasterCreate(ph_mailsp_m);
                        session.Result.etts = repo.MasterGet(ph_mailsp_m.MSGRECNO, ph_mailsp_m.MSGNO);
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>MAIL訊息編號</span>重複，請重新輸入。";
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
 
        [HttpPost]
        public ApiResponse MasterUpdate(PH_MAILSP_M ph_mailsp_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0003Repository(DBWork);
                    ph_mailsp_m.UPDATE_USER = User.Identity.Name;
                    ph_mailsp_m.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterUpdate(ph_mailsp_m);
                    session.Result.etts = repo.MasterGet(ph_mailsp_m.MSGRECNO, ph_mailsp_m.MSGNO);

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
        public ApiResponse MasterDelete(PH_MAILSP_M ph_mailsp_m)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0003Repository(DBWork);
                    //if (repo.CheckExists(ph_mailsp_m.MGROUP))
                    //{
                        session.Result.afrs = repo.DetailDeleteAll(ph_mailsp_m.MSGRECNO, ph_mailsp_m.MSGNO);
                        session.Result.afrs = repo.MasterDelete(ph_mailsp_m.MSGNO, ph_mailsp_m.MSGRECNO);
                    //}
                    //else
                    //{
                    //    session.Result.afrs = 0;
                    //    session.Result.success = false;
                    //    session.Result.msg = "<span style='color:red'>申請單號</span>不存在。";
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

 
        [HttpPost]
        public ApiResponse MasterAudit(PH_MAILSP_M ph_mailsp_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0003Repository(DBWork);
                    ph_mailsp_m.UPDATE_USER = User.Identity.Name;
                    ph_mailsp_m.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterAudit(ph_mailsp_m);
                    session.Result.etts = repo.MasterGet( ph_mailsp_m.MSGRECNO, ph_mailsp_m.MSGNO);

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
        public ApiResponse DetailAll(FormDataCollection form)
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
                    var repo = new BD0003Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(p0,p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailCreate(PH_MAILSP_D ph_mailsp_d)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0003Repository(DBWork);
                    //ph_mailsp_d. = repo.getNewSeq(ph_mailsp_d.DN);
                    //if (!repo.CheckExists(ph_mailsp_d.DN, ph_mailsp_d.SEQ))
                    ph_mailsp_d.CREATE_USER = User.Identity.Name;
                    ph_mailsp_d.UPDATE_IP = DBWork.ProcIP;

                    if (!repo.CheckExists_D(ph_mailsp_d))
                    {
                        session.Result.afrs = repo.DetailCreate(ph_mailsp_d);
                        session.Result.etts = repo.DetailGet(ph_mailsp_d);
                        DBWork.Commit();
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>廠商/合約識別</span>重複，請重新輸入。";
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

        [HttpPost]
        public ApiResponse DetailUpdate(PH_MAILSP_D ph_mailsp_d)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0003Repository(DBWork);
                    ph_mailsp_d.UPDATE_USER = User.Identity.Name;
                    ph_mailsp_d.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.DetailUpdate(ph_mailsp_d);
                    session.Result.etts = repo.DetailGet(ph_mailsp_d);

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
        public ApiResponse DetailDelete(PH_MAILSP_D ph_mailsp_d)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0003Repository(DBWork);                 
                    session.Result.afrs = repo.DetailDelete(ph_mailsp_d);
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
        public ApiResponse GetMgroupCombo(FormDataCollection form)
             
        {
            string inid = form.Get("inid");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0003Repository repo = new BD0003Repository(DBWork);
                    session.Result.etts = repo.GetMgroupCombo(inid);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAngenoCombo(FormDataCollection form)
        {
            //string inid = form.Get("inid");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0003Repository repo = new BD0003Repository(DBWork);
                    session.Result.etts = repo.GetAngenoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAngenoAllCombo(FormDataCollection form)
        {
            //string inid = form.Get("inid");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0003Repository repo = new BD0003Repository(DBWork);
                    session.Result.etts = repo.GetAngenoAllCombo();
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