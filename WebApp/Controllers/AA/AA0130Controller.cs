using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models.MI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using WebApp.Models;
using System;

namespace WebApp.Controllers.AA
{
    public class AA0130Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0130Repository(DBWork);
                    session.Result.etts = repo.GetAll(page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Update(MI_MNSET pMI_MNSET)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0130Repository(DBWork);
                    session.Result.afrs = repo.Update(pMI_MNSET);
                    UR_BULLETIN bulletin = new UR_BULLETIN
                    {
                        TITLE = "月結設定(AA0130)",
                        CONTENT = pMI_MNSET.SET_YM + "月結日調整為" + pMI_MNSET.SET_CTIME,
                        TARGET = "I",
                        ON_DATE = System.DateTime.Now,
                        VALID = "1",
                        CREATE_BY = DBWork.ProcUser,
                        UPLOAD_KEY = Guid.NewGuid().ToString()
                    };
                    repo.Create(bulletin);

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