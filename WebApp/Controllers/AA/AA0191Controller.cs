using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using WebApp.Repository.AA;

namespace WebApp.Controllers.AA
{
    public class AA0191Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse Query(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0191Repository repo = new AA0191Repository(DBWork);
                    session.Result.etts = repo.Get(DBWork.UserInfo.UserId);
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Update(UR_ID ur_id)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0191Repository(DBWork);
                    ur_id.UPDATE_USER = User.Identity.Name;
                    ur_id.UPDATE_IP = DBWork.ProcIP;
                    ur_id.TUSER = DBWork.UserInfo.UserId;
                    session.Result.afrs = repo.Update(ur_id);
                    session.Result.etts = repo.Get(ur_id.TUSER);

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
    }
}
