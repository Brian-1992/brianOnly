using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.AA;

namespace WebApp.Controllers.AA
{
    public class AA0141Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form) {
            string startDate = form.Get("startDate");
            string endDate = form.Get("endDate");

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;

                try
                {
                    var repo = new AA0141Repository(DBWork);
                    session.Result.etts = repo.GetMasterAll(startDate, endDate);
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailAll(FormDataCollection form) {
            string docno = form.Get("docno");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0141Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(docno);
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse DeleteM(FormDataCollection form) {
            string docnos = form.Get("docnos");
            IEnumerable<ME_DOCM> masters = JsonConvert.DeserializeObject<IEnumerable<ME_DOCM>>(docnos);

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();

                try {
                    var repo = new AA0141Repository(DBWork);

                    foreach (ME_DOCM master in masters) {
                        session.Result.afrs = repo.DeleteM(master.DOCNO);
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
        public ApiResponse CreateM(FormDataCollection form) {
            string frwh = form.Get("frwh");
            string apply_note = form.Get("apply_note");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0141Repository(DBWork);

                    string wh_kind = repo.GetWhKind(frwh);

                    ME_DOCM docm = new ME_DOCM()
                    {
                        DOCNO = repo.GetDocno(),
                        DOCTYPE = "NAJ",
                        APPID = DBWork.UserInfo.UserId,
                        APPDEPT = DBWork.UserInfo.Inid,
                        FRWH = frwh,
                        MAT_CLASS = wh_kind == "1" ? "01" : "02",
                        CREATE_USER = DBWork.UserInfo.UserId,
                        UPDATE_USER = DBWork.UserInfo.UserId,
                        UPDATE_IP = DBWork.ProcIP,
                        APPLY_NOTE = apply_note
                    };

                    session.Result.afrs = repo.CreateM(docm);

                    DBWork.Commit();
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse LoadDetailItems(FormDataCollection form) {
            string docno = form.Get("docno");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0141Repository(DBWork);
                    ME_DOCM frwh = repo.GetFrwh(docno);
                    IEnumerable<MI_WINVMON> invs = repo.GetInvs(frwh.WH_NO, frwh.WH_KIND);

                    foreach (MI_WINVMON inv in invs) {
                        string seq = repo.GetDocDSeq(docno);
                        
                        ME_DOCD docd = new ME_DOCD() {
                            DOCNO = docno,
                            SEQ = seq,
                            MMCODE = inv.MMCODE,
                            APPQTY = inv.INV_QTY,
                            CREATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_USER = DBWork.UserInfo.UserId,
                            UPDATE_IP = DBWork.ProcIP
                        };

                        session.Result.afrs += repo.CreateD(docd);
                    }

                    DBWork.Commit();
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Apply(FormDataCollection form) {
            string docnos = form.Get("docnos");
            IEnumerable<ME_DOCM> masters = JsonConvert.DeserializeObject<IEnumerable<ME_DOCM>>(docnos);

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0141Repository(DBWork);

                    foreach (ME_DOCM master in masters) {

                        bool flowIdValid = repo.ChceckFlowId01(master.DOCNO);
                        if (flowIdValid == false)
                        {
                            session.Result.msg = master.DOCNO + " 申請單狀態已變更，請重新查詢";
                            session.Result.success = false;
                            return session.Result;
                        }

                        IEnumerable<ME_DOCD> docds = repo.GetDs(master.DOCNO);
                        if (docds.Any() == false) {
                            session.Result.msg = master.DOCNO + " 無可過帳品項，請重新查詢";
                            session.Result.success = false;
                            return session.Result;
                        }

                        // 更新flowId
                        session.Result.afrs = repo.UpdateFlowId(master.DOCNO, DBWork.UserInfo.UserId, DBWork.ProcIP);
                        // 更新ME_DOCD
                        session.Result.afrs = repo.UpdateDocd(master.DOCNO, DBWork.UserInfo.UserId, DBWork.ProcIP);

                        ME_DOCM docm = repo.GetM(master.DOCNO);

                        docds = repo.GetDs(master.DOCNO);
                        foreach (ME_DOCD docd in docds) {

                            if (docd.MMCODE == "005AGG03") {
                                int i = 0;
                            }

                            MI_WINVMON ori_inv = repo.GetD(docd.WH_NO, docd.MMCODE);

                            // 更新MI_WHINV
                            session.Result.afrs = repo.UpdateWhinv(docd.WH_NO, docd.MMCODE, docd.APPQTY);

                            MI_WINVMON new_inv = repo.GetD(docd.WH_NO, docd.MMCODE);

                            // 新增MI_WHTRNS
                            MI_WHTRNS trns = new MI_WHTRNS()
                            {
                                WH_NO = docd.WH_NO,
                                MMCODE = docd.MMCODE,
                                TR_INV_QTY = (double.Parse(docd.APPQTY) * (-1)).ToString(),
                                TR_ONWAY_QTY = "0",
                                TR_DOCNO = master.DOCNO,
                                TR_DOCSEQ = docd.SEQ,
                                TR_FLOWID = docm.FLOWID,
                                TR_DOCTYPE = docm.DOCTYPE,
                                TR_IO = "I",
                                TR_MCODE = "ADJI",
                                BF_TR_INVQTY = ori_inv.INV_QTY,
                                AF_TR_INVQTY = new_inv.INV_QTY
                            };
                            session.Result.afrs = repo.InsertWhtrns(trns);
                        }
                    }

                    DBWork.Commit();

                    
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }

                return session.Result;
            }
        }


        [HttpGet]
        public ApiResponse GetWhnoComno() {
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0141Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo();
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }
    }
}