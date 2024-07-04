using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WebApp.Controllers.AB
{
    public class AB0057Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = bool.Parse(form.Get("p5"));

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p5 == true)
                    {
                        var repo = new AB0057Repository(DBWork);
                        if (form.Get("p0") != "")
                        {
                            string _p0 = form.Get("p0").Substring(0, form.Get("p0").Length - 1); // 去除前端傳進來最後一個逗號
                            string[] tmp = _p0.Split(',');
                            p0 = "";
                            for (int i = 0; i < tmp.Length; i++)
                            {
                                p0 += "'" + tmp[i] + "',";
                            }
                            p0 = p0.Substring(0, p0.Length - 1);
                        }

                        session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, page, limit, sorters);
                    }
                    else
                    {
                        var repo = new AB0057Repository(DBWork);
                        session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, p5, page, limit, sorters);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse Print(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = bool.Parse(form.Get("p5"));

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p5 == true)
                    {
                        var repo = new AB0057Repository(DBWork);
                        string _p0 = form.Get("p0").Substring(0, form.Get("p0").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        session.Result.etts = repo.Print(p0, p1, p2, p3, p4, p5);
                    }
                    else
                    {
                        var repo = new AB0057Repository(DBWork);
                        session.Result.etts = repo.Print(p0, p1, p2, p3, p4, p5);
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = bool.Parse(form.Get("p5"));

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p5 == true)
                    {
                        var repo = new AB0057Repository(DBWork);
                        string _p0 = p0.Substring(0, p0.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4, p5));
                    }
                    else
                    {
                        var repo = new AB0057Repository(DBWork);
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4, p5));
                    }
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMATCombo(FormDataCollection form)
        {
            var WH_NO = form.Get("WH_NO");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0057Repository(DBWork);
                    var whno_kind = repo.GetWHNO_KIND(WH_NO);
                    if (whno_kind == "0")
                    {
                        session.Result.etts = repo.GetMATCombo_0();
                    }
                    else
                    {
                        session.Result.etts = repo.GetMATCombo();
                    }

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
                    var repo = new AB0057Repository(DBWork);
                    session.Result.etts = repo.GetWh_no(User.Identity.Name);
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
