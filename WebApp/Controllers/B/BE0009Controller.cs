using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BE;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using NPOI.SS.UserModel;
using System.Web;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Linq;
using System.Data;

namespace WebApp.Controllers.BE
{
    public class BE0009Controller : SiteBase.BaseApiController
    {
        // 查詢_未列帳
        [HttpPost]
        public ApiResponse Master0(FormDataCollection form)
        {
            var pACT_YM = form.Get("act_ym");
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort") == null ? "" : form.Get("sort");
            string[] arr_p0 = { };
            if (!string.IsNullOrEmpty(p0))
            {
                arr_p0 = p0.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0009Repository(DBWork);
                    session.Result.etts = repo.GetMaster0(pACT_YM, arr_p0, p1, p2, p3, p4, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        // 查詢_列帳
        [HttpPost]
        public ApiResponse Master1(FormDataCollection form)
        {
            var pACT_YM = form.Get("act_ym");
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort") == null ? "" : form.Get("sort");
            string[] arr_p0 = { };
            if (!string.IsNullOrEmpty(p0))
            {
                arr_p0 = p0.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0009Repository(DBWork);
                    session.Result.etts = repo.GetMaster1(pACT_YM, arr_p0, p1, p2, p3, p4, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        // 查詢_明細
        [HttpPost]
        public ApiResponse Detail(FormDataCollection form)
        {
            var pINVOICE = form.Get("invoice");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort") == null ? "" : form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0009Repository(DBWork);
                    session.Result.etts = repo.GetDetail(pINVOICE, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        // 查詢_明細
        [HttpPost]
        public ApiResponse DetailForm(FormDataCollection form)
        {
            var pINVOICE = form.Get("invoice");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort") == null ? "" : form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0009Repository(DBWork);
                    session.Result.etts = repo.GetDetailForm(pINVOICE, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterAdd(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BE0009Repository repo = new BE0009Repository(DBWork);
                    if (form.Get("INVOICE") != "")
                    {
                        string act_ym = form.Get("ACT_YM");
                        string invoice = form.Get("INVOICE").Substring(0, form.Get("INVOICE").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = invoice.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            BE0009 be0009 = new BE0009();
                            be0009.INVOICE = tmp[i];
                            be0009.ACT_YM = act_ym;
                            be0009.UPDATE_USER = User.Identity.Name;
                            be0009.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.MasterAdd(be0009);
                        }
                    }
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse MasterRemove(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BE0009Repository repo = new BE0009Repository(DBWork);
                    if (form.Get("INVOICE") != "")
                    {
                        string invoice = form.Get("INVOICE").Substring(0, form.Get("INVOICE").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = invoice.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            BE0009 be0009 = new BE0009();
                            be0009.INVOICE = tmp[i];
                            be0009.UPDATE_USER = User.Identity.Name;
                            be0009.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.MasterRemove(be0009);
                        }
                    }
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse MasterFinish(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BE0009Repository repo = new BE0009Repository(DBWork);
                    string act_ym = form.Get("ACT_YM");

                    if (act_ym != "")
                    {
                        if (repo.CheckActYmExists(act_ym))
                        {
                            BE0009 be0009 = new BE0009();
                            be0009.ACT_YM = act_ym;
                            be0009.UPDATE_USER = User.Identity.Name;
                            be0009.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.MasterFinish(be0009);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>列帳年月:" + act_ym + "</span> 無資料可設定，請重新輸入。";
                        }

                    }
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetActymCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0009Repository repo = new BE0009Repository(DBWork);
                    session.Result.etts = repo.GetActymCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0009Repository repo = new BE0009Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAgennoCombo(FormDataCollection form)
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
                    BE0009Repository repo = new BE0009Repository(DBWork);
                    session.Result.etts = repo.GetAgennoCombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse getINVMARK(FormDataCollection form)
        {
            string act_ym = form.Get("ACT_YM");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0009Repository repo = new BE0009Repository(DBWork);
                    session.Result.etts = repo.GetINVMARK(act_ym);
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