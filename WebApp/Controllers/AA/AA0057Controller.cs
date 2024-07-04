using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Data;

namespace WebApp.Controllers.AA
{
    public class AA0057Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p00 = form.Get("p00");
            var p01 = (form.Get("p01") == "") ? form.Get("p00") : form.Get("p01");
            var p02 = form.Get("p02");
            var p03 = form.Get("p03");
            var p04 = form.Get("p04");
            var p05 = form.Get("p05");
            var p06 = form.Get("p06");
            var p07 = form.Get("p07");
            var p08 = form.Get("p08");
            var p09 = form.Get("p09");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");
            var p17 = form.Get("p17");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0057Repository(DBWork);
                    session.Result.etts = repo.GetAll(p00, p01, p02, p03, p04, p05, p06, p07, p08, p09, p10, p11, p12, p13, p14, p15, p16, p17);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p00 = form.Get("p00");
            var p01 = (form.Get("p01") == "") ? form.Get("p00") : form.Get("p01");
            var p02 = form.Get("p02");
            var p03 = form.Get("p03");
            var p04 = form.Get("p04");
            var p05 = form.Get("p05");
            var p06 = form.Get("p06");
            var p07 = form.Get("p07");
            var p08 = form.Get("p08"); //1080621 -> 2019-06-21T00:00:00
            var p09 = form.Get("p09");
            var p10 = form.Get("p10");
            var p11 = form.Get("p11");
            var p12 = form.Get("p12");
            var p13 = form.Get("p13");
            var p14 = form.Get("p14");
            var p15 = form.Get("p15");
            var p16 = form.Get("p16");
            var p17 = form.Get("p17");

            if (!string.IsNullOrWhiteSpace(p08))
            { 
                p08 = (int.Parse(p08.Substring(0, p08.Length < 7 ? 2 : 3)) + 1911).ToString() + "-" + p08.Substring(p08.Length < 7 ? 2 : 3, 2) + "-" + p08.Substring(p08.Length < 7 ? 4 : 5, 2);
            };

            if (!string.IsNullOrWhiteSpace(p09))
            {
                p09 = (int.Parse(p09.Substring(0, p09.Length < 7 ? 2 : 3)) + 1911).ToString() + "-" + p09.Substring(p09.Length < 7 ? 2 : 3, 2) + "-" + p09.Substring(p09.Length < 7 ? 4 : 5, 2);
            };

            if (!string.IsNullOrWhiteSpace(p10)) 
            {
                p10 = (int.Parse(p10.Substring(0, p10.Length < 7 ? 2 : 3)) + 1911).ToString() + "-" + p10.Substring(p10.Length < 7 ? 2 : 3, 2) + "-" + p10.Substring(p10.Length < 7 ? 4 : 5, 2);
            };

            if (!string.IsNullOrWhiteSpace(p11))
            {
                p11 = (int.Parse(p11.Substring(0, p11.Length < 7 ? 2 : 3)) + 1911).ToString() + "-" + p11.Substring(p11.Length < 7 ? 2 : 3, 2) + "-" + p11.Substring(p11.Length < 7 ? 4 : 5, 2);
            };

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0057Repository repo = new AA0057Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = repo.GetExcel(p00, p01, p02, p03, p04, p05, p06, p07, p08, p09, p10, p11, p12, p13, p14, p15, p16, p17);

                    dtItems.Merge(result);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetOrderDCFlag()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0057Repository(DBWork);
                    session.Result.etts = repo.GetOrderDCFlag();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetCaseFrom()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0057Repository(DBWork);
                    session.Result.etts = repo.GetCaseFrom();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetInsuSignI()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0057Repository(DBWork);
                    session.Result.etts = repo.GetInsuSignI();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetInsuSignO()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0057Repository(DBWork);
                    session.Result.etts = repo.GetInsuSignO();
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