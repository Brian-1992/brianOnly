using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WebApp.Controllers.AA
{
    public class AA0035Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0035Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2,p3, DBWork.UserInfo.UserId , page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse MasterCreate(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0035Repository repo = new AA0035Repository(DBWork);
                    ME_DOCM me_docm = new ME_DOCM();
                    me_docm.DOCNO = repo.GetDocno();
                    if (!repo.CheckExists(me_docm.DOCNO))
                    {
                        me_docm.CREATE_USER = User.Identity.Name;
                        me_docm.UPDATE_USER = User.Identity.Name;
                        me_docm.UPDATE_IP = DBWork.ProcIP;
                        me_docm.APPID = User.Identity.Name;
                        me_docm.APPDEPT = form.Get("INID_NAME").Split(' ')[0];
                        me_docm.USEID = User.Identity.Name;
                        me_docm.FRWH = form.Get("FRWH");        // 核撥庫房
                        me_docm.MAT_CLASS = "01";

                        session.Result.afrs = repo.MasterCreate(me_docm);
                        session.Result.etts = repo.Get(me_docm.DOCNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>單據號碼</span>重複，請重新嘗試。";
                    }

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
        public ApiResponse MasterDelete(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0035Repository repo = new AA0035Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            bool flowIdValid = repo.ChceckFlowId01(tmp[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }

                            IEnumerable<ME_DOCD> docds = repo.GetMeDocds(tmp[i]);

                            foreach (ME_DOCD docd in docds) {

                                int docdAfrs = repo.DetailDelete(docd.DOCNO, docd.SEQ);

                                if (docdAfrs < 1) {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<span style='color:red'>項次刪除失敗</span>";
                                    return session.Result;
                                }

                                var me_doces = repo.GetMeDoces(docd.DOCNO, docd.SEQ);
                                if (me_doces.Any())
                                {
                                    int medoceResult = repo.MeDoceDelete(docd.DOCNO, docd.SEQ);

                                    if (medoceResult < 1)
                                    {
                                        DBWork.Rollback();
                                        session.Result.success = false;
                                        session.Result.msg = "細項刪除失敗";
                                        return session.Result;
                                    }
                                }
                            }

                            int docmAfrs = repo.MasterDelete(tmp[i]);
                            if (docmAfrs < 1)
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>主檔刪除失敗</span>";
                                return session.Result;
                            }
                        }

                    }
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
        public ApiResponse MasterUpdate(ME_DOCM me_docm)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0035Repository repo = new AA0035Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(me_docm.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    me_docm.UPDATE_USER = User.Identity.Name;
                    me_docm.UPDATE_IP = DBWork.ProcIP;
                    if (!repo.CheckMeDocdExists(me_docm.DOCNO)) // 傳入DOCNO檢查申請單是否有院內碼項次
                    {
                        session.Result.afrs = repo.MasterUpdate(me_docm);
                        session.Result.etts = repo.Get(me_docm.DOCNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>申請單號「" + me_docm.DOCNO + "」有院內碼項次，請先刪除所有項次才能修改。</span>";
                        return session.Result;
                    }


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
        public ApiResponse AllMeDocd(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0035Repository(DBWork);
                    string docno = form.Get("p0");
                    string wh_no = form.Get("p1");
                    //session.Result.etts = repo.GetMeDocds(docno, wh_no, page, limit, sorters);
                    var docds = repo.GetMeDocds(docno, wh_no, page, limit, sorters);

                    foreach (ME_DOCD docd in docds) {
                        docd.AMOUNT = (double.Parse(docd.APVQTY) * double.Parse(docd.M_CONTPRICE)).ToString();
                    }

                    session.Result.etts = docds;

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse AllMeDoce(FormDataCollection form)
        {
            var docno = form.Get("p0");
            var seq = form.Get("p1");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0035Repository(DBWork);
                    session.Result.etts = repo.GetMeDoces(docno, seq);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmdataByMmcode(FormDataCollection form)
        {
            string mmcode = form.Get("mmcode");
            string wh_no = form.Get("wh_no");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0076Repository repo = new AA0076Repository(DBWork);
                    if (!repo.CheckWhmmExist(mmcode, wh_no))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此院內碼不存在於此庫房中</span>，請重新輸入。";

                        return session.Result;
                    }

                    session.Result.etts = repo.GetMmdataByMmcode(mmcode);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        
        [HttpPost]
        public ApiResponse GetMeDocdMaxSeq(ME_DOCD me_docd) {
            string docno = me_docd.DOCNO;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0035Repository repo = new AA0035Repository(DBWork);
                    List<string> result = new List<string>();
                    session.Result.etts = repo.GetMeDocdMaxSeq(docno);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailCreate(ME_DOCD me_docd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0035Repository repo = new AA0035Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(me_docd.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    if (repo.CheckDocdExists(me_docd.DOCNO, me_docd.SEQ)) {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>項次</span>重複，請重新嘗試。";
                        return session.Result;
                    }

                    me_docd.APPQTY = me_docd.APVQTY;
                    me_docd.APVID = User.Identity.Name;
                    me_docd.CREATE_USER = User.Identity.Name;
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;

                    

                    if (repo.CreateMeDoce(me_docd.DOCNO, me_docd.SEQ, me_docd.WH_NO) != "Y")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>申請單號「" + me_docd.DOCNO + "」</span>，發生執行錯誤。";
                        return session.Result;
                    }

                    session.Result.afrs = repo.DetailCreate(me_docd);
                    session.Result.msg = repo.IsWexpid(me_docd.MMCODE);

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
        //public ApiResponse CreateMeDoec(ME_DOCD me_docd)
        //{
        //    string docno = me_docd.DOCNO;
        //    string seq = me_docd.SEQ;
        //    string wh_no = me_docd.WH_NO;

        //    using (WorkSession session = new WorkSession())
        //    {
        //        UnitOfWork DBWork = session.UnitOfWork;
        //        try
        //        {
        //            AA0035Repository repo = new AA0035Repository(DBWork);

        //            bool flowIdValid = repo.ChceckFlowId01(docno);
        //            if (flowIdValid == false)
        //            {
        //                session.Result.msg = "申請單狀態已變更，請重新查詢";
        //                session.Result.success = false;
        //                return session.Result;
        //            }

        //            var result = repo.CreateMeDoce(docno, seq, wh_no);



        //            session.Result.msg = result;
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse DetailDelete(ME_DOCD me_docd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0035Repository repo = new AA0035Repository(DBWork);

                    string[] tempDocnos = me_docd.DOCNO.Split(',');

                    string[] tempSeqs= me_docd.SEQ.Split(',');

                    bool flowIdValid = repo.ChceckFlowId01(tempDocnos[0]);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    for (int i = 0; i < tempSeqs.Length; i++) {
                        if (tempSeqs[i].Trim() != string.Empty) {
                            session.Result.afrs = repo.DetailDelete(tempDocnos[0], tempSeqs[i]);

                            var me_doces = repo.GetMeDoces(tempDocnos[0], tempSeqs[i]);
                            if (me_doces.Any()) {
                                int medoceResult = repo.MeDoceDelete(tempDocnos[0], tempSeqs[i]);

                                if (medoceResult < 1)
                                {
                                    DBWork.Rollback();
                                    session.Result.success = false;
                                    session.Result.msg = "細項刪除失敗";
                                    return session.Result;
                                }
                            }
                        }
                    }

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
        public ApiResponse DetailUpdate(ME_DOCD me_docd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0035Repository repo = new AA0035Repository(DBWork);

                    bool flowIdValid = repo.ChceckFlowId01(me_docd.DOCNO);
                    if (flowIdValid == false)
                    {
                        session.Result.msg = "申請單狀態已變更，請重新查詢";
                        session.Result.success = false;
                        return session.Result;
                    }

                    me_docd.APPQTY = me_docd.APVQTY;
                    me_docd.UPDATE_USER = User.Identity.Name;
                    me_docd.UPDATE_IP = DBWork.ProcIP;
                    if (repo.CheckMeDocdExists(me_docd.DOCNO, me_docd.SEQ)) // 傳入DOCNO檢查申請單是否有院內碼項次
                    {
                        session.Result.afrs = repo.DetailUpdate(me_docd);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>項次不存在，請重新輸入。</span>";
                        return session.Result;
                    }


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

        public ApiResponse MeDoceUpdate(ME_DOCE me_doce) {

            IEnumerable<ME_DOCE> me_doces = JsonConvert.DeserializeObject<IEnumerable<ME_DOCE>>(me_doce.ITEM_STRING);

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0035Repository repo = new AA0035Repository(DBWork);

                    foreach(ME_DOCE doce in me_doces) {
                        bool flowIdValid = repo.ChceckFlowId01(doce.DOCNO);
                        if (flowIdValid == false)
                        {
                            session.Result.msg = "申請單狀態已變更，請重新查詢";
                            session.Result.success = false;
                            return session.Result;
                        }

                        doce.UPDATE_USER = User.Identity.Name;
                        doce.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.MeDoceUpdate(doce);
                    }

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

        public ApiResponse UpdateStatusBySP(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0035Repository repo = new AA0035Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            bool flowIdValid = repo.ChceckFlowId01(tmp[i]);
                            if (flowIdValid == false)
                            {
                                session.Result.msg = "申請單狀態已變更，請重新查詢";
                                session.Result.success = false;
                                return session.Result;
                            }

                            SP_MODEL sp = repo.PostDoc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                            if (sp.O_RETID == "N")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = sp.O_ERRMSG;
                                return session.Result;
                            }
                        }
                    }
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

        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0076Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var wh_no = form.Get("WH_NO");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0035Repository repo = new AA0035Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, wh_no, page, limit, "");
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