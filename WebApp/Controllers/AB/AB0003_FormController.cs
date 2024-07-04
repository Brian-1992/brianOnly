using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using WebApp.Models;
using WebApp.Repository.AB;

namespace WebApp.Controllers.AB
{
    public class AB0003_FormController : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse Get(FormDataCollection form) {
            string docno = form.Get("docno");
            string mat_class = form.Get("mat_class");
            string m_storeid = form.Get("m_storeid");
            string mmcode = form.Get("mmcode");
            string mmname_c = form.Get("mmname_c");
            string mmname_e = form.Get("mmname_e");

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0003_FormRepository(DBWork);
                    session.Result.etts = repo.GetAll(docno, mat_class, m_storeid, mmcode, mmname_c, mmname_e);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }                
        }

        [HttpGet]
        public ApiResponse GetReasonCombo() {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0003_FormRepository(DBWork);
                    session.Result.etts = repo.GetReasonCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse InsertDetail(FormDataCollection form) {

            IEnumerable<ME_DOCD> list = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(form.Get("list"));

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0003_FormRepository(DBWork);

                    foreach (ME_DOCD item in list) {

                        bool flowIdValid = repo.ChceckFlowId01(item.DOCNO);
                        if (flowIdValid == false)
                        {
                            session.Result.msg = "申請單狀態已變更，請重新查詢";
                            session.Result.success = false;
                            return session.Result;
                        }

                        if (!repo.CheckMeDocdExists(item.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                            item.SEQ = "1";
                        else
                            item.SEQ = repo.GetMaxSeq(item.DOCNO);

                        item.CREATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_USER = DBWork.UserInfo.UserId;
                        item.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.InsertDetail(item);
                    }

                    DBWork.Commit();
                }
                catch(Exception e)
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }
    }
}