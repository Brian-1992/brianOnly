using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json;

using System.Data;

namespace WebApp.Controllers.C
{
    public class AB0079Controller : SiteBase.BaseApiController
    {


        public ApiResponse QueryD(FormDataCollection form)   //SELECT * FROM UR_ID where INID tab1  
        {
            var p0 = "";
            var p1a = "";
            var p1b = "";
            var p2a = "";
            var p2b = "";
            var p3a = "";
            var p3b = "";
            var p4a = "";
            var p4b = "";

            p0 = form.Get("p0");      // 統計類別
            p1a = form.Get("p1a");    // 查詢月份起
            p1b = form.Get("p1b");    // 查詢月份至
            p2a = form.Get("p2a");    // 醫師起
            p2b = form.Get("p2b");    // 醫師至
            p3a = form.Get("p3a");    // 科室起
            p3b = form.Get("p3b");    // 科室至
            p4a = form.Get("p4a");    // 院內碼起
            p4b = form.Get("p4b");    // 院內碼至

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0079Repository repo = new AB0079Repository(DBWork);
                    session.Result.etts = repo.GetAB0079Detail(p0, p1a, p1b, p2a, p2b, p3a, p3b, p4a, p4b, page, limit, sorters);
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
            var p1a = form.Get("p1a").Trim();
            var p1b = form.Get("p1b").Trim();
            var p2a = form.Get("p2a").Trim();
            var p2b = form.Get("p2b").Trim();
            var p3a = form.Get("p3a").Trim();
            var p3b = form.Get("p3b").Trim();
            var p4a = form.Get("p4a").Trim();
            var p4b = form.Get("p4b").Trim();

            string str_CreatYM = "";

            if (!string.IsNullOrEmpty(p1a) && !string.IsNullOrEmpty(p1b))
            {
                int iFrY = Convert.ToInt32(p1a.Substring(0, 3));
                int iToY = Convert.ToInt32(p1b.Substring(0, 3));
                int iFrM = Convert.ToInt32(p1a.Substring(3, 2));
                int iToM = Convert.ToInt32(p1b.Substring(3, 2));
                for (int tmpY = iFrY; tmpY <= iToY; tmpY++)
                {
                    for (int tmpM = iFrM; tmpM <= iToM; tmpM++)
                    {
                        if (string.IsNullOrEmpty(str_CreatYM))
                        {
                            str_CreatYM += tmpY.ToString() + tmpM.ToString("00");
                        }
                        else
                        {
                            str_CreatYM += ", " + tmpY.ToString() + tmpM.ToString("00");
                        }
                    }
                }
            }

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0079Repository repo = new AB0079Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    //dtItems.Columns.Add("項次", typeof(int));
                    //dtItems.Columns["項次"].AutoIncrement = true;
                    //dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    //dtItems.Columns["項次"].AutoIncrementStep = 1;

                    //DataTable result = repo.GetExcel(p0, p1a, p1b, p2a, p2b, p3a, p3b, p4a, p4b);
                    DataTable result = repo.GetExcel(p0, str_CreatYM, p2a, p2b, p3a, p3b, p4a, p4b);

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

        [HttpPost]
        public ApiResponse OrderDrCombo(FormDataCollection form)    // 醫師
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0079Repository repo = new AB0079Repository(DBWork);
                    session.Result.etts = repo.GetOrderDrCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse SectionNoCombo(FormDataCollection form)  // 科室
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0079Repository repo = new AB0079Repository(DBWork);
                    session.Result.etts = repo.GetSectionNoCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse OrderCodeCombo(FormDataCollection form)  // 院內碼
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0079Repository repo = new AB0079Repository(DBWork);
                    session.Result.etts = repo.GetOrderCodeCombo(p0, page, limit, "");
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
