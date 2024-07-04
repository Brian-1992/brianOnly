using System;
using System.Collections.Generic;
using System.Linq;
using JCLib.DB;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebApp.Repository.AB;
using WebApp.Models;

namespace WebApp.Controllers.AB
{
    public class AB0009Controller : SiteBase.BaseApiController
    {
        public ApiResponse CreateMrDoc(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0009Repository repo = new AB0009Repository(DBWork);
                    SP_MODEL sp = repo.CreateMrDoc();
                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = sp.O_ERRMSG;
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = true;
                    }
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
