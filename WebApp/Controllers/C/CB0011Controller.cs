using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Repository.C;
using JCLib.DB;
using System.Net.Http.Formatting;
using WebApp.Models.C;


namespace WebApp.Controllers.C
{
    public class CB0011Controller : SiteBase.BaseApiController
    {
        // GET: CB0011
        
        public ApiResponse GetBox(int start, int limit, string BOXNO, string BARCODE, string STATUS, string sort)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0011Repository(DBWork);
                    session.Result.etts = repo.GetBox(start, limit, BOXNO, BARCODE, STATUS, sort);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse GetXcategory()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0011Repository(DBWork);
                    session.Result.etts = repo.GetXcategory();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse AddBox(CB0011 CB0011)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CB0011Repository(DBWork);
                    CB0011.CREATE_USER = User.Identity.Name;
                    CB0011.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.AddBox(CB0011);
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
        public ApiResponse DelBox(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CB0011 CB0011 = new CB0011();
                    CB0011.BOXNO = form.Get("BOXNO");
                    var repo = new CB0011Repository(DBWork);
                    session.Result.afrs = repo.DeleteBox(CB0011);
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
        public ApiResponse UpdateBox(CB0011 CB0011)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CB0011Repository(DBWork);
                    CB0011.UPDATE_USER = User.Identity.Name;
                    CB0011.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateBox(CB0011);
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