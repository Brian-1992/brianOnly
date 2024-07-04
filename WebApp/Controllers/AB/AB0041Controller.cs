using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BC;
using WebApp.Models;
using System;

namespace WebApp.Controllers.BC
{
    public class AB0041Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = bool.Parse(form.Get("p1"));
            //var p2 = form.Get("p2");
            //var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            //var p5 = form.Get("p5");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
  
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0041Repository(DBWork);
                    //session.Result.etts = repo.GetMasterAll(p0, p1, p2, p3, p4, p5, page, limit, sorters);
                    session.Result.etts = repo.GetMasterAll(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterCreate(ME_MDFM me_mdfm)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0041Repository(DBWork);
                    //me_mdfm.MDFM = repo.getNewDn(repo.getUserInid(User.Identity.Name));
                    if (!repo.CheckExists(me_mdfm.MDFM))
                    {
                        //me_mdfm.APP_USER = User.Identity.Name;
                        //me_mdfm.APP_INID = DBWork.UserInfo.Inid;
                        me_mdfm.CREATE_ID = User.Identity.Name;
                        me_mdfm.UPDATE_ID = User.Identity.Name;
                        me_mdfm.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.MasterCreate(me_mdfm);
                        session.Result.etts = repo.MasterGet(me_mdfm.MDFM);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>配方代碼</span>重複，請重新嘗試。";
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
        public ApiResponse MasterUpdate(ME_MDFM me_mdfm)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0041Repository(DBWork);
                    //me_mdfm.APP_USER = User.Identity.Name;
                    //me_mdfm.APP_INID = DBWork.UserInfo.Inid;
                    me_mdfm.UPDATE_ID = User.Identity.Name;
                    me_mdfm.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterUpdate(me_mdfm);
                    session.Result.etts = repo.MasterGet(me_mdfm.MDFM);

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
        public ApiResponse MasterDelete(ME_MDFM me_mdfm)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0041Repository(DBWork);
                    if (repo.CheckExists(me_mdfm.MDFM))
                    {
                        session.Result.afrs = repo.DetailDelete(me_mdfm.MDFM);
                        session.Result.afrs = repo.MasterDelete(me_mdfm.MDFM);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>配方代碼</span>不存在。";
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
        /*
        [HttpPost]
        public ApiResponse MasterAudit(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0041Repository(DBWork);
                    ph_small_m.UPDATE_USER = User.Identity.Name;
                    ph_small_m.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterAudit(ph_small_m);
                    session.Result.etts = repo.MasterGet(ph_small_m.DN);

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
        */
        // ===============================================================================================
        [HttpPost]
        public ApiResponse DetailAll(FormDataCollection form)
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
                    var repo = new AB0041Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailCreate(ME_MDFD me_mdfd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0041Repository(DBWork);
                    //me_mdfd.SEQ = repo.getNewSeq(me_mdfd.mmcode);
                    if (!repo.CheckMmExists(me_mdfd.MMCODE))
                    {                        
                        me_mdfd.CREATE_ID = User.Identity.Name;
                        me_mdfd.UPDATE_ID = User.Identity.Name;
                        me_mdfd.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.DetailCreate(me_mdfd);
                        session.Result.etts = repo.DetailGet(me_mdfd);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>成品代碼</span>重複，請重新嘗試。";
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
        public ApiResponse DetailUpdate(ME_MDFD me_mdfd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0041Repository(DBWork);
                    me_mdfd.UPDATE_ID = User.Identity.Name;
                    me_mdfd.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.DetailUpdate(me_mdfd);
                    session.Result.etts = repo.DetailGet(me_mdfd);

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
        public ApiResponse DetailDelete(ME_MDFD me_mdfd)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0041Repository(DBWork);
                    if (repo.CheckMmExists(me_mdfd.MMCODE))
                    {
                        session.Result.afrs = repo.DetailDelete(me_mdfd.MDFM, me_mdfd.MMCODE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>成品代碼</span>不存在。";
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

        //[HttpPost]
        //public ApiResponse GetMmdataByMmcode(FormDataCollection form)
        //{
        //    string mmcode = form.Get("MMCODE");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AB0041Repository repo = new AB0041Repository(DBWork);
        //            session.Result.etts = repo.GetMmdataByMmcode(mmcode);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse GetInidByTuser(FormDataCollection form)
        //{
        //    string tuser = form.Get("TUSER");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AB0041Repository repo = new AB0041Repository(DBWork);
        //            session.Result.etts = repo.GetInidByTuser(tuser);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse GetAgennmByAgenno(FormDataCollection form)
        //{
        //    string agen_no = form.Get("AGEN_NO");

        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AB0041Repository repo = new AB0041Repository(DBWork);
        //            session.Result.etts = repo.GetAgennmByAgenno(agen_no);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse GetInidCombo(FormDataCollection form)
        //{
        //    var p0 = form.Get("p0");
        //    var page = int.Parse(form.Get("page"));
        //    var start = int.Parse(form.Get("start"));
        //    var limit = int.Parse(form.Get("limit"));
        //    var sorters = form.Get("sort");
        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AB0041Repository repo = new AB0041Repository(DBWork);
        //            session.Result.etts = repo.GetInidCombo(p0, page, limit, "");
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse GetMmcodeCombo(FormDataCollection form)
        //{
        //    var p0 = form.Get("p0");
        //    var page = int.Parse(form.Get("page"));
        //    var start = int.Parse(form.Get("start"));
        //    var limit = int.Parse(form.Get("limit"));
        //    var sorters = form.Get("sort");
        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AB0041Repository repo = new AB0041Repository(DBWork);
        //            session.Result.etts = repo.GetMmcodeCombo(p0, page, limit, "");
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        //[HttpPost]
        //public ApiResponse GetAgennoCombo(FormDataCollection form)
        //{
        //    var p0 = form.Get("p0");
        //    var page = int.Parse(form.Get("page"));
        //    var start = int.Parse(form.Get("start"));
        //    var limit = int.Parse(form.Get("limit"));
        //    var sorters = form.Get("sort");
        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AB0041Repository repo = new AB0041Repository(DBWork);
        //            session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //} 
    }
}