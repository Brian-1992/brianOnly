using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.GB;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Controllers.GB
{
    public class GB0007Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetCombo(FormDataCollection form)
        {
            var grp_code = form.Get("GRP_CODE");
            var data_name = form.Get("DATA_NAME");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    ParamRepository repo = new ParamRepository(DBWork);
                    session.Result.etts = repo.GetCombo(grp_code, data_name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse QueryD(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    ParamRepository repo = new ParamRepository(DBWork);
                    IEnumerable<PARAM_D> result1 = repo.QueryD(page, limit, sorters, "HOSP_INFO", "", "");
                    IEnumerable<PARAM_D> result2 = repo.QueryD(page, limit, sorters, "M_CONTID2_LIMIT", "LIMIT", "");
                    session.Result.etts = result1.Concat(result2);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateD(PARAM_D param_d)
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ParamRepository repo = new ParamRepository(DBWork);
                    param_d.DATA_SEQ_O = param_d.DATA_SEQ;
                    session.Result.afrs = repo.UpdateD(param_d);
                    session.Result.etts = repo.GetD(param_d);

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
