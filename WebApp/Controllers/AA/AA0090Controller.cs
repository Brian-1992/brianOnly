using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0090Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0090Repository(DBWork);
                    if (p0 != "")
                    {
                        session.Result.etts = repo.GetAll(p0, page, limit, sorters);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>查詢院內碼不可為空</span>，請重新輸入。";
                    }

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //院內碼下拉式選單(搜尋)
        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0090Repository repo = new AA0090Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //院內碼彈出式視窗(搜尋)
        [HttpPost]
        public ApiResponse GetMmcode(FormDataCollection form)
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
                    AA0090Repository repo = new AA0090Repository(DBWork);
                    AA0090Repository.MI_MAST_QUERY_PARAMS query = new AA0090Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                 /*   query.INSUORDERCODE = form.Get("INSUORDERCODE") == null ? "" : form.Get("INSUORDERCODE").ToUpper();
                    query.ORDERHOSPNAME = form.Get("ORDERHOSPNAME") == null ? "" : form.Get("ORDERHOSPNAME").ToUpper();
                    query.ORDEREASYNAME = form.Get("ORDEREASYNAME") == null ? "" : form.Get("ORDEREASYNAME").ToUpper();
                    query.SCIENTIFICNAME = form.Get("SCIENTIFICNAME") == null ? "" : form.Get("SCIENTIFICNAME").ToUpper();
                    query.MINCURECONSISTENCY = form.Get("MINCURECONSISTENCY") == null ? "" : form.Get("MINCURECONSISTENCY").ToUpper();
                    query.MAXCURECONSISTENCY = form.Get("MAXCURECONSISTENCY") == null ? "" : form.Get("MAXCURECONSISTENCY").ToUpper();
                    query.PEARBEGIN = form.Get("PEARBEGIN") == null ? "" : form.Get("PEARBEGIN").ToUpper();
                    query.PEAREND = form.Get("PEAREND") == null ? "" : form.Get("PEAREND").ToUpper();
                    query.TROUGHBEGIN = form.Get("TROUGHBEGIN") == null ? "" : form.Get("TROUGHBEGIN").ToUpper();
                    query.TROUGHEND = form.Get("TROUGHEND") == null ? "" : form.Get("TROUGHEND").ToUpper();
                    query.DANGERBEGIN = form.Get("DANGERBEGIN") == null ? "" : form.Get("DANGERBEGIN").ToUpper();
                    query.DANGEREND = form.Get("DANGEREND") == null ? "" : form.Get("DANGEREND").ToUpper();
                    query.TDMFLAG = form.Get("TDMFLAG") == null ? "" : form.Get("TDMFLAG").ToUpper();
                    query.TDMMEMO1 = form.Get("TDMMEMO1") == null ? "" : form.Get("TDMMEMO1").ToUpper();
                    query.TDMMEMO2 = form.Get("TDMMEMO2") == null ? "" : form.Get("TDMMEMO2").ToUpper();
                    query.TDMMEMO3 = form.Get("TDMMEMO3") == null ? "" : form.Get("TDMMEMO3").ToUpper();
                    query.UDSERVICEFLAG = form.Get("UDSERVICEFLAG") == null ? "" : form.Get("UDSERVICEFLAG").ToUpper();
                    query.UDPOWDERFLAG = form.Get("UDPOWDERFLAG") == null ? "" : form.Get("UDPOWDERFLAG").ToUpper();
                    query.AIRDELIVERY = form.Get("AIRDELIVERY") == null ? "" : form.Get("AIRDELIVERY").ToUpper();*/

                    session.Result.etts = repo.GetMmcode(query, page, limit, sorters);
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