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
    public class AA0106Controller : SiteBase.BaseApiController
    {
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1");    //成立年月  SET_YM
            var p2 = form.Get("p2");    //單位類別  INID_FLAG
            var p3 = form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL
  
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p4 == true)
                    {
                        var repo = new AA0106Repository(DBWork);
                        string _p0 = p0.Substring(0, p0.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, page, limit, sorters);
                    }
                    else
                    {
                        var repo = new AA0106Repository(DBWork);
                        session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, page, limit, sorters);
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
            var p0 = form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1");    //成立年月  SET_YM
            var p2 = form.Get("p2");    //單位類別  INID_FLAG
            var p3 = form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL

            //var page = int.Parse(form.Get("page"));
            //var start = int.Parse(form.Get("start"));
            //var limit = int.Parse(form.Get("limit"));
            //var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    if (p4 == true)
                    {
                        var repo = new AA0106Repository(DBWork);
                        string _p0 = p0.Substring(0, p0.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        session.Result.etts = repo.Print(p0, p1, p2, p3, p4);
                    }
                    else
                    {
                        var repo = new AA0106Repository(DBWork);
                        session.Result.etts = repo.Print(p0, p1, p2, p3, p4);
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
            var p0 = form.Get("p0");    //物料分類  MAT_CLASS
            var p1 = form.Get("p1");    //成立年月  SET_YM
            var p2 = form.Get("p2");    //單位類別  INID_FLAG
            var p3 = form.Get("p3");    //是否庫備  M_STOREID
            var p4 = bool.Parse(form.Get("p4"));    //物料分類是否全選 clsALL

            //var page = int.Parse(form.Get("page"));
            //var start = int.Parse(form.Get("start"));
            //var limit = int.Parse(form.Get("limit"));
            //var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    //if (!string.IsNullOrWhiteSpace(p1))
                    //{
                    //    p1 = (int.Parse(p1.Substring(0, p1.Length < 5 ? 2 : 3)) + 1911).ToString() + "/" + p1.Substring(p1.Length < 5 ? 2 : 3, 2) ;
                    //}
                    //if (!string.IsNullOrWhiteSpace(p2))
                    //{
                    //    p2 = (int.Parse(p2.Substring(0, p2.Length < 5 ? 2 : 3)) + 1911).ToString() + "/" + p2.Substring(p2.Length < 5 ? 2 : 3, 2) ;
                    //}
                    if (p4 == true)
                    {
                        var repo = new AA0106Repository(DBWork);
                        string _p0 = p0.Substring(0, p0.Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = _p0.Split(',');
                        p0 = "";
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            p0 += "'" + tmp[i] + "',";
                        }
                        p0 = p0.Substring(0, p0.Length - 1);
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4));
                    }
                    else
                    {
                        var repo = new AA0106Repository(DBWork);
                        JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4));
                    }
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
                    var repo = new AA0106Repository(DBWork);
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
                    var repo = new AA0106Repository(DBWork);
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
        //            var repo = new AA0106Repository(DBWork);
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
                        var repo = new AA0106Repository(DBWork);
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
                        var repo = new AA0106Repository(DBWork);
                        session.Result.etts = repo.GetMMCodeCombo(p0, p1, clsALL, page, limit, sorters);
                    }

                    //AA0106Repository repo = new AA0106Repository(DBWork);
                    //session.Result.etts = repo.GetMMCodeCombo(p0, p1, page, limit, "");
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
