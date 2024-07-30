using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.C;

namespace WebApp.Controllers.C
{
    public class CE0027Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {

            var chk_ym = form.Get("p0");
            var ContentType = form.Get("p1");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0027Repository(DBWork);
                    session.Result.etts = repo.GetMasterAll(chk_ym, ContentType); 
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDetails(FormDataCollection form)
        {
            var chk_ym = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0027Repository(DBWork);

                    session.Result.etts = repo.GetDetails(chk_ym);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetChkPeriod(FormDataCollection form) {
            string chk_ym = form.Get("chk_ym");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CE0027Repository(DBWork);

                    session.Result.msg = repo.GetChkPeriod(chk_ym);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //==========================================================================================
    }
}