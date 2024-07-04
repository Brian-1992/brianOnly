using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System;
using System.Data;

namespace WebApp.Controllers.AB
{
    public class AB0067Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetApplyWH_NOComboOne(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0067Repository repo = new AB0067Repository(DBWork);
                    session.Result.etts = repo.GetApplyWH_NOComboOne();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWriteOffWH_NOComboOne(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0067Repository repo = new AB0067Repository(DBWork);
                    session.Result.etts = repo.GetWriteOffWH_NOComboOne();
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