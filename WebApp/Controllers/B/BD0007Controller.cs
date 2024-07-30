using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.B;

namespace WebApp.Controllers.B
{
    public class BD0007Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p4 = form.Get("p4");

            DateTime startDate = DateTime.Parse(p0);
            DateTime endDate = DateTime.Parse(p1);

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0007Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetMasterAll(startDate, endDate, p2, p4, hospCode=="0");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MasterUpdate(MM_PO_M mmpom) {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    mmpom.UPDATE_USER = DBWork.UserInfo.UserId;
                    mmpom.UPDATE_IP = DBWork.ProcIP;

                    var repo = new BD0007Repository(DBWork);
                    session.Result.afrs = repo.MasterUpdate(mmpom);
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
        public ApiResponse DetailAll(FormDataCollection form)
        {
            string po_no = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0007Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(po_no);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SendEmail(FormDataCollection form) {


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0007Repository repo = new BD0007Repository(DBWork);
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
                        DBWork.Commit();
                    }
                }
                catch {
                    DBWork.Rollback();
                    throw;
                }

                return session.Result;
            }
        }
    }
}