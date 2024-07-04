using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System;
namespace WebApp.Controllers.B
{
    public class BD0006Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0006Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetMasterAll(p0, p1, p4, hospCode=="0", page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0006Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetDetailAll(p0, p1, p4, hospCode == "0", page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SendEmail(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    BD0006Repository repo = new BD0006Repository(DBWork);
                    if (form.Get("PO_NO") != "")
                    {
                        string po_nos = form.Get("PO_NO").Substring(0, form.Get("PO_NO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = po_nos.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            MM_PO_M mmpom = new MM_PO_M();
                            mmpom.PO_NO = tmp[i];
                            mmpom.PO_STATUS = "84";
                            mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                            mmpom.UPDATE_IP = DBWork.ProcIP;

                            session.Result.afrs = repo.SendEmail(mmpom);
                        }
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
        public ApiResponse ExportExcel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p4 = form.Get("p4");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BD0006Repository repo = new BD0006Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    //JCLib.Excel.Export(form.Get("FN") + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(p0, p1));
                    JCLib.Excel.Export("廠商回覆狀態" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(p0, p1, p4, hospCode=="0"));
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