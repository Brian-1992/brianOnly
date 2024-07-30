using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebAppVen.Repository.B;

namespace WebAppVen.Controllers.B
{
    public class BH0011Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            string start_date = form.Get("start_date");
            string end_date = form.Get("end_date");

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BH0011Repository(DBWork);
                    session.Result.etts = repo.GetAll(DBWork.UserInfo.UserId, crdocno, start_date, end_date);

                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse GetCrDocnoCombo(FormDataCollection form) {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;

                try
                {
                    var repo = new BH0011Repository(DBWork);
                    session.Result.etts = repo.GetDocnos(DBWork.UserInfo.UserId);

                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Update(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            string inqty = form.Get("inqty");
            string lot_no = form.Get("lot_no");
            string exp_date = form.Get("exp_date");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BH0011Repository(DBWork);

                    session.Result.afrs = repo.UpdateCrDoc(crdocno, inqty, lot_no, exp_date, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    session.Result.success = true;
                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e) {
                    session.Result.msg = e.Message;
                    session.Result.success = false;
                    DBWork.Rollback();
                    return session.Result;
                    throw;
                }
            }

        }

        [HttpPost]
        public ApiResponse Confirm(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BH0011Repository(DBWork);

                    session.Result.afrs = repo.UpdateCrDocStatus(crdocno, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    session.Result.success = true;
                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
            }
        }
    }
}