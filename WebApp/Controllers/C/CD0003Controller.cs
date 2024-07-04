using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace WebApp.Controllers.C
{
    public class CD0003Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse ID(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0003Repository(DBWork);
                    session.Result.etts = repo.GetID(p0, p1, p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0003Repository(DBWork);
                    //CD0003Repository.MI_WHMM_QUERY_PARAMS query = new CD0003Repository.MI_WHMM_QUERY_PARAMS();
                    session.Result.etts = repo.GetAll(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse CheckWhExist(FormDataCollection form)
        {
            var p0 = form.Get("p0");


            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CD0003Repository(DBWork);                    
                    session.Result.success = repo.CheckWhExist(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Insert(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CD0003Repository repo = new CD0003Repository(DBWork);
                    if (form.Get("WH_USERID") != "")
                    {
                        string wh_no = form.Get("WH_NO");
                        string tuser = form.Get("TUSER").Substring(0, form.Get("TUSER").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = tuser.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            CD0003M bc_whid = new CD0003M();
                            bc_whid.WH_USERID = tmp[i];
                            if (repo.CheckUserExist(wh_no, bc_whid.WH_USERID))
                            {
                                session.Result.success = false;
                                break;
                            }                            
                            bc_whid.WH_NO = wh_no;
                            bc_whid.CREATE_USER = DBWork.ProcUser;
                            session.Result.afrs = repo.Insert(bc_whid);
                        }
                    }
                    if (!session.Result.success)
                    {
                        DBWork.Rollback();
                    }
                    else DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse Delete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    CD0003Repository repo = new CD0003Repository(DBWork);
                    if (form.Get("WH_USERID") != "" && form.Get("WH_NO") != "")
                    {
                        string wh_no = form.Get("WH_NO").Substring(0, form.Get("WH_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string wh_userid = form.Get("WH_USERID").Substring(0, form.Get("WH_USERID").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp1 = wh_no.Split(',');
                        string[] tmp2 = wh_userid.Split(',');
                        for (int i = 0; i < tmp1.Length; i++)
                        {
                            CD0003M bc_whid = new CD0003M();
                            bc_whid.WH_NO = tmp1[i];
                            bc_whid.WH_USERID = tmp2[i];
                            session.Result.afrs = repo.Delete(bc_whid);
                        }
                    }
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


        [HttpPost]
        public ApiResponse GetWh_no(FormDataCollection form)
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
                    CD0003Repository repo = new CD0003Repository(DBWork);
                    CD0003Repository.MI_WHMAST_QUERY_PARAMS query = new CD0003Repository.MI_WHMAST_QUERY_PARAMS();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    query.WH_NAME = form.Get("WH_NAME") == null ? "" : form.Get("WH_NAME").ToUpper();
                    //query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    session.Result.etts = repo
                        .GetWh_no(query, page, limit, sorters)
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME, WH_KIND = w.WH_KIND, WH_GRADE = w.WH_GRADE });
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
                    CD0003Repository repo = new CD0003Repository(DBWork);
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

        [HttpPost]
        public ApiResponse GetInidWhCombo(FormDataCollection form)
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
                    CD0003Repository repo = new CD0003Repository(DBWork);
                    session.Result.etts = repo.GetInidWhCombo(p0, page, limit, "");
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