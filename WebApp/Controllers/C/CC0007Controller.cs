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
    public class CC0007Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse GetCrDoc(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CC0007Repository(DBWork);
                    if (repo.CheckCrdocnoExist(crdocno) == false) {
                        session.Result.msg = "無此三聯單編號，請重新確認";
                        session.Result.success = false;
                        return session.Result;
                    }

                    // 檢查三聯單狀態，VALUE = cr_status，TEXT = cr_status_name，EXTRA1 = po_no
                    COMBO_MODEL crStatus = repo.GetCrStatus(crdocno);
                    if (crStatus.VALUE != "N") {
                        session.Result.success = false;
                        session.Result.msg = string.Format("狀態：{0}，無法接收", crStatus.TEXT);
                        return session.Result;
                    }
                    COMBO_MODEL poStatus = repo.GetPoStatus(crStatus.EXTRA1);
                    if (poStatus.VALUE != "82" && poStatus.VALUE != "85") {
                        session.Result.success = false;
                        session.Result.msg = string.Format("訂單狀態：{0}，無法接收", poStatus.TEXT);
                        return session.Result;
                    }

                    session.Result.etts = repo.GetCrdoc(crdocno);

                    return session.Result;
                    
                }
                catch (Exception e) {
                    throw;
                }
            }
        }

        public ApiResponse SetData(FormDataCollection form) {
            string crdocno = form.Get("crdocno");
            string po_no = form.Get("po_no");
            string mmcode = form.Get("mmcode");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CC0007Repository(DBWork);
                    string sp_msg = string.Empty;

                    IEnumerable<CC0007> datas = repo.GetPoDs(crdocno, po_no, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    // insert BC_CS_ACC_LOG
                    // insert BC_CS_DIST_LOG

                    List<string> seqList = new List<string>();
                    foreach (CC0007 data in datas) {
                        data.SEQ = repo.GetAccLogSeq();
                        seqList.Add(data.SEQ);
                        data.UserId = DBWork.UserInfo.UserId;

                        session.Result.afrs += repo.InsertAccLog(data);

                        session.Result.afrs += repo.InsertDistLog(data);
                    }

                    // update PH_REPLY
                    session.Result.afrs = repo.UpdatePhReply(crdocno, po_no, DBWork.UserInfo.UserId, DBWork.ProcIP);

                    // sp: inv_set.po_docin
                    sp_msg = repo.procDocin(po_no, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    if (sp_msg != "Y") {
                        DBWork.Rollback();
                        session.Result.msg = string.Format("接收失敗，{0}", sp_msg);
                        session.Result.success = false;
                        return session.Result;
                    }
                    // sp: inv_set.dist_in
                    sp_msg = repo.procDistin(po_no, mmcode, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    if (sp_msg != "Y")
                    {
                        DBWork.Rollback();
                        session.Result.msg = string.Format("接收失敗，{0}", sp_msg);
                        session.Result.success = false;
                        return session.Result;
                    }
                    // sp: CC0002_submit
                    sp_msg = repo.procCC0002(po_no, DBWork.UserInfo.UserId, DBWork.ProcIP);
                    if (sp_msg != "000")
                    {
                        DBWork.Rollback();
                        session.Result.msg = string.Format("接收失敗，{0}", sp_msg);
                        session.Result.success = false;
                        return session.Result;
                    }

                    // update CR_DOC.cr_status
                    session.Result.afrs = repo.UpdateCrdocStatus(crdocno);

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
    }
}