using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;

namespace WebApp.Controllers.AA
{
    public class AA0144Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse ClearCfmScan() {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0144Repository(DBWork);
                    session.Result.afrs = repo.ClearCfmScan(DBWork.UserInfo.UserId);

                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse GetCrDocsList(FormDataCollection form)
        {
            string crdocno = form.Get("crdocno");

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0144Repository(DBWork);
                    session.Result.etts = repo.GetCrDocsList(crdocno);
                    return session.Result;
                }
                catch (Exception e) {
                    throw;
                }
            }

        }

        [HttpPost]
        public ApiResponse GetCrDocsScan(FormDataCollection form) {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0144Repository(DBWork);
                    session.Result.etts = repo.GetCrDocsScan(DBWork.UserInfo.UserId);
                    return session.Result;
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Scan(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0144Repository(DBWork);

                    AA0144 temp = repo.GetCrDoc(crdocno);
                    if (temp == null) {
                        session.Result.success = false;
                        session.Result.msg = "查無單據，請重新確認";
                        return session.Result;
                    }

                    if (temp.CR_STATUS != "I") {
                        session.Result.success = false;
                        session.Result.msg = string.Format("緊急醫療出貨單狀態={0}，無法選取", temp.CR_STATUS_NAME);
                        return session.Result;
                    }

                    if (string.IsNullOrEmpty(temp.CFMSCAN) == false)
                    {
                        session.Result.success = false;
                        session.Result.msg = string.Format("緊急醫療出貨單已被{0}選取，無法再選取", temp.CR_STATUS_NAME);
                        return session.Result;
                    }

                    session.Result.afrs = repo.UpdateCfmScan(crdocno, DBWork.UserInfo.UserId);

                    DBWork.Commit();
                    return session.Result;
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Remove(FormDataCollection form) {
            string crdocno = form.Get("crdocno");

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0144Repository(DBWork);
                    session.Result.afrs = repo.Remove(crdocno);
                    session.Result.success = true;
                    DBWork.Commit();

                    return session.Result;
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Reject(FormDataCollection form) {

            IEnumerable<AA0144> crdocs = JsonConvert.DeserializeObject<IEnumerable<AA0144>>(form.Get("list"));
            

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0144Repository(DBWork);
                    foreach (AA0144 crdoc in crdocs) {
                        session.Result.afrs = repo.Reject(crdoc.CRDOCNO, DBWork.UserInfo.UserId);
                    }

                    DBWork.Commit();
                    session.Result.success = true;
                    return session.Result;
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse UpdateCfmQty(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            string cfmqty = form.Get("cfmqty");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0144Repository(DBWork);
                    session.Result.afrs = repo.UpdateCfmQty(crdocno, cfmqty, DBWork.UserInfo.UserId);
                    session.Result.success = true;

                    DBWork.Commit();

                    return session.Result;

                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse Confirm(FormDataCollection form) {
            IEnumerable<AA0144> crdocs = JsonConvert.DeserializeObject<IEnumerable<AA0144>>(form.Get("list"));


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0144Repository(DBWork);
                    foreach (AA0144 crdoc in crdocs)
                    {
                        session.Result.afrs = repo.Confirm(crdoc.CRDOCNO, DBWork.UserInfo.UserId);

                        if (crdoc.ISSMALL == "N")
                        {
                            string docno = repo.GetMedocmDocno(crdoc.TOWH);

                            // 若已存在，等1秒再進行
                            while (repo.CheckDocnoExists(docno)) {
                                Thread.Sleep(1000);

                                docno = repo.GetMedocmDocno(crdoc.TOWH);
                            }


                            string frwh = repo.GetFrwh();

                            session.Result.afrs = repo.InsertMedocm(docno, crdoc.INID, frwh, crdoc.TOWH);

                            string seq = repo.GetMedocdSeq(docno);
                            session.Result.afrs = repo.InsertMedocd(docno, seq, crdoc.ACKMMCODE, crdoc.CFMQTY,
                                                                    crdoc.AVG_PRICE, crdoc.CRDOCNO);
                            session.Result.afrs = repo.UpdateCrDocRdocno(crdoc.CRDOCNO, docno, seq);
                        }
                        else {
                            string dn = repo.GetPhSmallDn(crdoc.INID);

                            session.Result.afrs = repo.InsertPhSmallM(dn, crdoc.AGEN_NAMEC, crdoc.AGEN_NO, 
                                                                      crdoc.INID, crdoc.APPID, crdoc.WH_NAME, 
                                                                      crdoc.REQDATE, crdoc.TEL, crdoc.USEWHEN, 
                                                                      crdoc.USEWHERE, DBWork.ProcIP);

                            string seq = repo.GetPhSmallDSeq(dn);
                            session.Result.afrs = repo.InsertPhSmallD(seq, dn, crdoc.MEMO_DETAIL, 
                                                                      crdoc.ACKMMCODE, crdoc.INID, crdoc.M_PAYKIND, 
                                                                      crdoc.MMNAME_C, crdoc.CR_UPRICE, crdoc.CFMQTY,
                                                                      crdoc.BASE_UNIT, DBWork.ProcIP);
                            session.Result.afrs = repo.UpdateCrDocSmallDn(crdoc.CRDOCNO, dn, seq);
                        }
                    }

                    DBWork.Commit();
                    session.Result.success = true;
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