using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0063Controller : SiteBase.BaseApiController
    {
        // 查詢Master
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            //var p2 = form.Get("p2");
            var p3 = bool.Parse(form.Get("p3"));
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p3 == true)
                    {
                        var repo = new AA0063Repository(DBWork);
                        string _p0 = form.Get("p0").Substring(0, form.Get("p0").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        session.Result.etts = repo.GetAll(p0, p1, p3, p4, p5, p6, p7, p8, page, limit, sorters);
                    }
                    else
                    {
                        var repo = new AA0063Repository(DBWork);
                        session.Result.etts = repo.GetAll(p0, p1, p3, p4, p5, p6, p7, p8, page, limit, sorters);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //匯出Excel
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            //var p2 = form.Get("p2");
            var p3 = bool.Parse(form.Get("p3"));
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7");
            var p8 = form.Get("p8");
            //var page = int.Parse(form.Get("page"));
            //var start = int.Parse(form.Get("start"));
            //var limit = int.Parse(form.Get("limit"));
            //var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    //if (!string.IsNullOrWhiteSpace(p5))
                    //{
                    //    p5 = (int.Parse(p5.Substring(0, p5.Length < 7 ? 2 : 3)) + 1911).ToString() + "/" + p5.Substring(p5.Length < 7 ? 2 : 3, 2) + "/" + p5.Substring(p5.Length < 7 ? 4 : 5, 2);
                    //}
                    //if (!string.IsNullOrWhiteSpace(p6))
                    //{
                    //    p6 = (int.Parse(p6.Substring(0, p6.Length < 7 ? 2 : 3)) + 1911).ToString() + "/" + p6.Substring(p6.Length < 7 ? 2 : 3, 2) + "/" + p6.Substring(p6.Length < 7 ? 4 : 5, 2);
                    //}
                    if (p3 == true)
                    {
                        var repo = new AA0063Repository(DBWork);
                        string _p0 = form.Get("p0").Substring(0, form.Get("p0").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        //session.Result.etts = repo.GetExcel(p0, p1, p2, p3, p4, p5, p6);
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p3, p4, p5, p6, p7, p8));
                    }
                    else
                    {
                        var repo = new AA0063Repository(DBWork);
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p3, p4, p5, p6, p7, p8));
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetCLSNAME()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var p0 = User.Identity.Name;
                    var repo = new AA0063Repository(DBWork);
                    session.Result.etts = repo.GetCLSNAME();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetFLOWID()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0063Repository(DBWork);
                    session.Result.etts = repo.GetFLOWID();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
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
                    var p0 = form.Get("MAT_CLASS");
                    var p1 = form.Get("MMCODE");
                    //var wh_no = form.Get("WH_NO");
                    AA0063Repository repo = new AA0063Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

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
                    AA0063Repository repo = new AA0063Repository(DBWork);
                    AA0063Repository.MI_MAST_QUERY_PARAMS query = new AA0063Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS").ToUpper();
                    //query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    session.Result.etts = repo.GetMmcode(query, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWH_NoCombo(FormDataCollection form)
        {
            var p0 = form.Get("query");
            //var wh_no = form.Get("WH_NO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0063Repository repo = new AA0063Repository(DBWork);
                    session.Result.etts = repo.GetWH_NoCombo(p0, page, limit, "")
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_GRADE = w.WH_GRADE });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetWh_no()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0062Repository(DBWork);
                    session.Result.etts = repo.GetWh_no();
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