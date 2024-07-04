using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AB
{
    public class AB0075Controller : ApiController
    {
        //庫房代碼combox
        public ApiResponse GetWH_NO()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0075Repository(DBWork);
                    session.Result.etts = repo.GetWH_NO();
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