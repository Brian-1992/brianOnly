using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WebApp.Controllers.AA
{
    public class AA0104Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0") == null ? string.Empty : form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1") == null ? string.Empty : form.Get("p1");    //成立年月  SET_YM
            var p2 = form.Get("p2") == null ? string.Empty : form.Get("p2");    //單位類別  INID_FLAG
            var p3 = form.Get("p3") == null ? string.Empty : form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL
            var p5 = form.Get("p5") == null ? string.Empty : form.Get("p5");    //單位科別庫房代碼 

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p2 != "4")  //只有是否庫備Radio選擇單位科別才會抓取庫房代碼
                    {
                        p5 = null;
                    }

                    var repo = new AA0104Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p5);
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
            var p0 = form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1");    //成立年月  SET_YM
            var p2 = form.Get("p2");    //單位種類  INID_FLAG
            var p3 = form.Get("p3");    //是否庫備  M_STOREID
            var p5 = form.Get("p5");    //單位科別庫房代碼 

            //var page = int.Parse(form.Get("page"));
            //var start = int.Parse(form.Get("start"));
            //var limit = int.Parse(form.Get("limit"));
            //var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p2 != "4")  //只有是否庫備Radio選擇單位科別才會抓取庫房代碼
                    {
                        p5 = null;
                    }

                    var repo = new AA0104Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p5));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMatCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0104Repository(DBWork);
                    session.Result.etts = repo.GetMatCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetYMCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0104Repository(DBWork);
                    session.Result.etts = repo.GetYMCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //public ApiResponse GetWh_no()
        //{
        //    using (WorkSession session = new WorkSession(this))
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new AA0104Repository(DBWork);
        //            session.Result.etts = repo.GetWh_no();
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

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
                    var clsALL = bool.Parse(form.Get("clsALL"));
                    //var wh_no = form.Get("WH_NO");

                    if (clsALL == true)
                    {
                        var repo = new AA0104Repository(DBWork);
                        string _p0 = p0.Substring(0, p0.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        session.Result.etts = repo.GetMMCodeCombo(p0, p1, clsALL, page, limit, sorters);
                    }
                    else
                    {
                        var repo = new AA0104Repository(DBWork);
                        session.Result.etts = repo.GetMMCodeCombo(p0, p1, clsALL, page, limit, sorters);
                    }

                    //AA0104Repository repo = new AA0104Repository(DBWork);
                    //session.Result.etts = repo.GetMMCodeCombo(p0, p1, page, limit, "");
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
            var p0 = form.Get("p0");
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
                    AA0104Repository repo = new AA0104Repository(DBWork);
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



    }
}
