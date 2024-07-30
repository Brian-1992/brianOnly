using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using WebApp.Repository.UT;

namespace WebApp.Controllers.UT
{
    public class UT0002Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UT0001Repository(DBWork);
                    string list = repo.GetDoctype(p0);

                    var repo2 = new UT0002Repository(DBWork);
                    session.Result.etts = repo2.GetAllM(p0, list.Split(';')[0], list.Split(';')[1], User.Identity.Name, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AllD(FormDataCollection form)
        {
            var docno = form.Get("p0");
            var seq = form.Get("p1");
            var doctype = form.Get("doctype");
            var flowid = form.Get("flowid");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UT0002Repository(DBWork);
                    session.Result.etts = repo.GetAllD(flowid,doctype, docno,  seq, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse UpdateStatus(FormDataCollection form)
        {
            var docno = form.Get("DOCNO");
            var seq_list = form.Get("SEQ_LIST");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                bool isOK = true;
                try
                {
                    var repo = new UT0002Repository(DBWork);

                    string[] seqArray = seq_list.Split(',');

                    for (var i = 0; i < seqArray.Length; i++)
                    {
                        SP_MODEL sp = repo.WEXPINV_R(docno, Convert.ToInt16(seqArray[i]), User.Identity.Name, DBWork.ProcIP);
                        if (sp.O_RETID == "N")
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = sp.O_ERRMSG;
                            isOK = false;
                            break;
                        }
                    }
                    if (isOK)
                    {
                        DBWork.Commit();
                    }
                    else {
                        DBWork.Rollback();
                    }
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
    }
}