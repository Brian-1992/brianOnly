using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;

namespace WebApp.Controllers.AA
{
    public class AA0156Controller : ApiController
    {
        [HttpPost]
        public ApiResponse GetMclassCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0156Repository repo = new AA0156Repository(DBWork);
                    session.Result.etts = repo.GetMclassCombo(User.Identity.Name);
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