using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BC;
using WebApp.Models;
using System.Data;

namespace WebApp.Controllers.BC
{
    public class BC0003Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterUpdate(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BC0003Repository(DBWork);
                    ph_small_m.APP_USER1 = User.Identity.Name;
                    ph_small_m.UPDATE_USER = User.Identity.Name;
                    ph_small_m.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.MasterUpdate(ph_small_m);
                    var repo1 = new BC0002Repository(DBWork);
                    session.Result.etts = repo1.MasterGet(ph_small_m.DN);

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
        public ApiResponse MasterReject(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ph_small_m.APP_USER1 = User.Identity.Name;
                    ph_small_m.UPDATE_USER = User.Identity.Name;
                    ph_small_m.UPDATE_IP = DBWork.ProcIP;
                    var repo1 = new BC0003Repository(DBWork);
                    session.Result.afrs = repo1.MasterReject(ph_small_m);
                    var repo2 = new BC0002Repository(DBWork);
                    session.Result.etts = repo2.MasterGet(ph_small_m.DN);
                    string email = repo2.getUser_MAIL_ADDRESS(ph_small_m.DN);
                    if (email != null && email != "")
                    {
                        BC0004Controller BC4 = new BC0004Controller();
                        BC4.sendRejectMail(ph_small_m, DBWork.UserInfo.UserName, email);
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
        public ApiResponse MasterApprove(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ph_small_m.APP_USER1 = User.Identity.Name;
                    ph_small_m.UPDATE_USER = User.Identity.Name;
                    ph_small_m.UPDATE_IP = DBWork.ProcIP;
                    var repo1 = new BC0003Repository(DBWork);
                    session.Result.afrs = repo1.MasterApprove(ph_small_m);
                    var repo2 = new BC0002Repository(DBWork);
                    session.Result.etts = repo2.MasterGet(ph_small_m.DN);

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
        public ApiResponse MasterApprove2(PH_SMALL_M ph_small_m)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ph_small_m.APP_USER1 = User.Identity.Name;
                    ph_small_m.UPDATE_USER = User.Identity.Name;
                    ph_small_m.UPDATE_IP = DBWork.ProcIP;
                    var repo1 = new BC0003Repository(DBWork);
                    session.Result.afrs = repo1.MasterApprove2(ph_small_m);
                    var repo2 = new BC0002Repository(DBWork);
                    session.Result.etts = repo2.MasterGet(ph_small_m.DN);

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
        //public ApiResponse MasterExport(PH_SMALL_M ph_small_m)
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {

        //            ph_small_m.UPDATE_USER = User.Identity.Name;
        //            ph_small_m.UPDATE_IP = DBWork.ProcIP;
        //            var repo1 = new BC0003Repository(DBWork);
        //            session.Result.afrs = repo1.MasterExport(ph_small_m);
        //            var repo2 = new BC0002Repository(DBWork);
        //            session.Result.etts = repo2.MasterGet(ph_small_m.DN);

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}
    }
}