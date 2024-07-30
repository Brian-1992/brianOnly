using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace WebApp.Controllers.AA
{
    public class AA0016Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse QueryME(FormDataCollection form)
        {
            var pf = form.Get("pf"); // 使用程式
            var p0 = form.Get("p0"); // 申請日(起)
            var p1 = form.Get("p1"); // 申請日(迄)
            var p2 = form.Get("p2"); // 庫房
            var p3 = form.Get("p3"); // 異動類別
            var p4 = form.Get("p4");// 狀態
            var p5 = User.Identity.Name;
            var p6 = form.Get("p6");// 申請部門
            var p7 = form.Get("p7");// 申請人員
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");


            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0016Repository(DBWork);
                    session.Result.etts = repo.QueryME(pf, p0, p1, p2, p3, p4, p5, p6, p7, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse QueryMEDOCD(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 申請單號
            var p1 = form.Get("p1"); // 申請單號

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");


            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0016Repository(DBWork);
                    session.Result.etts = repo.QueryMEDOCD(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse UpdateEnd(FormDataCollection form)
        {
            string dno = form.Get("dno");
            using (WorkSession session = new WorkSession(this))
            {
                ;
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string UPUSER = User.Identity.Name;
                    string UIP = DBWork.ProcIP;
                    AA0016Repository repo = new AA0016Repository(DBWork);

                    // session.Result.afrs = repo.UpdateEnd(medocm);

                    if (repo.POST_DOC(dno, User.Identity.Name, DBWork.ProcIP) == "Y")
                    {
                        session.Result.afrs = repo.UpdateEnd(dno, User.Identity.Name, DBWork.ProcIP);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>申請單號「" + dno + "」</span>，發生執行錯誤。";
                    }
                    DBWork.Commit();
                }
                catch when (!Debugger.IsAttached)
                {
                    DBWork.Rollback();
                    //throw;
                }
                return session.Result;
            }
        }

        public ApiResponse Update(ME_DOCM medocm)
        {
            using (WorkSession session = new WorkSession(this))
            {
                //List<QUOTN> list = new List<QUOTN>();
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0016Repository repo = new AA0016Repository(DBWork);
                    medocm.CONFIRMSWITCH = medocm.CONFIRMSWITCH.Substring(0, 1);
                    medocm.GTAPL_RESON = medocm.GTAPL_RESON.Substring(0, 2);
                    medocm.CREATE_USER = User.Identity.Name;
                    medocm.UPDATE_USER = User.Identity.Name;
                    medocm.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.Update(medocm);
                    DBWork.Commit();
                }
                catch when (!Debugger.IsAttached)
                {
                    DBWork.Rollback();
                    //throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetStatusCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0016Repository(DBWork);
                    session.Result.etts = repo.GetStatusCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetGridWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0016Repository(DBWork);
                    session.Result.etts = repo.GetGridWhnoCombo();
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