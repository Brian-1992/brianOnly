using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0059Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllMI_WHMAST(FormDataCollection form)
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
                    var repo = new AA0059Repository(DBWork);
                    session.Result.etts = repo.GetAllMI_WHMAST(p0, p1, p2, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AllMI_MAST(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0059Repository(DBWork);
                    session.Result.etts = repo.GetAllMI_MAST(p0, p1, p2, p4, p5, p6, page, limit, sorters);
                }
                catch (Exception e)
                {
                    string ex = e.ToString();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AllMI_WHMM(FormDataCollection form)
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
                    var repo = new AA0059Repository(DBWork);
                    //AA0059Repository.MI_WHMM_QUERY_PARAMS query = new AA0059Repository.MI_WHMM_QUERY_PARAMS();
                    session.Result.etts = repo.GetAllMI_WHMM(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AllMI_MMWH(FormDataCollection form)
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
                    var repo = new AA0059Repository(DBWork);
                    //AA0059Repository.MI_WHMM_QUERY_PARAMS query = new AA0059Repository.MI_WHMM_QUERY_PARAMS();
                    session.Result.etts = repo.GetAllMI_MMWH(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse InsertWH(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0059Repository repo = new AA0059Repository(DBWork);
                    if (form.Get("WH_NO") != "")
                    {
                        string mmcode = form.Get("MMCODE");
                        string wh_no = form.Get("WH_NO").Substring(0, form.Get("WH_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = wh_no.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            AA0059M mi_whmm = new AA0059M();
                            mi_whmm.WH_NO = tmp[i];
                            if (repo.CheckWHMMExist(mi_whmm.WH_NO, mmcode))
                            {
                                session.Result.success = false;
                                break;
                            }
                            mi_whmm.MMCODE = mmcode;
                            mi_whmm.CREATE_USER = DBWork.ProcUser;
                            session.Result.afrs = repo.InsertWH(mi_whmm);
                        }
                    }
                    if (!session.Result.success)
                    {
                        DBWork.Rollback();
                    }
                    else
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
        public ApiResponse InsertMM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0059Repository repo = new AA0059Repository(DBWork);
                    if (form.Get("MMCODE") != "")
                    {
                        string wh_no = form.Get("WH_NO");
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = mmcode.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            AA0059M mi_whmm = new AA0059M();
                            mi_whmm.MMCODE = tmp[i];
                            if (repo.CheckWHMMExist(wh_no, mi_whmm.MMCODE))
                            {
                                session.Result.success = false;
                                break;
                            }
                            mi_whmm.WH_NO = wh_no;
                            mi_whmm.CREATE_USER = DBWork.ProcUser;
                            session.Result.afrs = repo.InsertMM(mi_whmm);
                        }
                    }
                    if (!session.Result.success)
                    {
                        DBWork.Rollback();
                    }
                    else
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

        /*
        [HttpPost]
        public ApiResponse Delete(AA0059M mi_whmm)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0059Repository(DBWork);
                    session.Result.afrs = repo.Delete(mi_whmm);

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
    */
        [HttpPost]
        public ApiResponse Delete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0059Repository repo = new AA0059Repository(DBWork);
                    if (form.Get("MMCODE") != "" && form.Get("WH_NO") != "")
                    {
                        string wh_no = form.Get("WH_NO").Substring(0, form.Get("WH_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp1 = wh_no.Split(',');
                        string[] tmp2 = mmcode.Split(',');
                        for (int i = 0; i < tmp1.Length; i++)
                        {
                            AA0059M mi_whmm = new AA0059M();
                            mi_whmm.WH_NO = tmp1[i];
                            mi_whmm.MMCODE = tmp2[i];
                            session.Result.afrs = repo.Delete(mi_whmm);
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
                    AA0059Repository repo = new AA0059Repository(DBWork);
                    AA0059Repository.MI_MAST_QUERY_PARAMS query = new AA0059Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
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
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
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
                    AA0059Repository repo = new AA0059Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
                }
                catch
                {
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
                    AA0059Repository repo = new AA0059Repository(DBWork);
                    AA0059Repository.MI_WHMAST_QUERY_PARAMS query = new AA0059Repository.MI_WHMAST_QUERY_PARAMS();
                    query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    query.WH_NAME = form.Get("WH_NAME") == null ? "" : form.Get("WH_NAME").ToUpper();
                    //query.WH_NO = form.Get("WH_NO") == null ? "" : form.Get("WH_NO").ToUpper();
                    session.Result.etts = repo.GetWh_no(query, page, limit, sorters)
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
                    AA0059Repository repo = new AA0059Repository(DBWork);
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
        public ApiResponse CheckWhExist(FormDataCollection form)
        {
            var p0 = form.Get("p0");


            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0059Repository(DBWork);
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
        public ApiResponse CheckMmExist(FormDataCollection form)
        {
            var p0 = form.Get("p0");


            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0059Repository(DBWork);
                    session.Result.success = repo.CheckMmExist(p0);
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