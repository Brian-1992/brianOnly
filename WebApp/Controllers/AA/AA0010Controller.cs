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

namespace WebApp.Controllers.AA
{
    public class AA0010Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            string[] arr_p3 = { };
            if (!string.IsNullOrEmpty(p3))
            {
                arr_p3 = p3.Trim().Split(','); //用,分割
            }
            string[] arr_p5 = { };
            if (!string.IsNullOrEmpty(p5))
            {
                arr_p5 = p5.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0010Repository(DBWork);
                    //var taskid = repo.GetTaskid(User.Identity.Name);
                    //var doctype = "MR2";
                    //if (taskid == "2")
                    //{
                    //    doctype = "MR2";
                    //}
                    //else
                    //{
                    //    doctype = "MR1";
                    //}
                    session.Result.etts = repo.GetAllM(p0, p1, p2, arr_p3, p4, arr_p5, p6, page, limit, sorters);
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
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0010Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, p1, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 新增
        [HttpPost]
        public ApiResponse CreateM(ME_DOCM ME_DOCM)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0010Repository(DBWork);
                    if (!repo.CheckExists(ME_DOCM.DOCNO)) // 新增前檢查主鍵是否已存在
                    {
                        var v_inid = repo.GetUridInid(User.Identity.Name);
                        var v_twntime = repo.GetTwnsystime();
                        var v_matclass = ME_DOCM.MAT_CLASS;
                        var v_docno = v_inid + v_twntime + v_matclass;
                        var v_whno = repo.GetFrwh();
                        var v_num = repo.CheckApplyKind();
                        var v_applykind = "2";
                        if (v_num < 2)
                        {
                            v_applykind = "1";
                        }
                        else
                        {
                            v_applykind = "2";
                        }
                        ME_DOCM.DOCNO = v_docno;
                        ME_DOCM.APPID = User.Identity.Name;
                        ME_DOCM.FRWH = v_whno;
                        ME_DOCM.APPDEPT = v_inid;
                        ME_DOCM.APPLY_KIND = v_applykind;
                        ME_DOCM.USEID = User.Identity.Name;
                        ME_DOCM.USEDEPT = ME_DOCM.APPDEPT;
                        ME_DOCM.CREATE_USER = User.Identity.Name;
                        ME_DOCM.UPDATE_USER = User.Identity.Name;
                        ME_DOCM.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateM(ME_DOCM);
                        session.Result.etts = repo.GetM(ME_DOCM.DOCNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>代碼</span>重複，請重新輸入。";
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
        public ApiResponse CreateD(ME_DOCD ME_DOCD)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0010Repository(DBWork);
                    if (!repo.CheckExistsD(ME_DOCD.DOCNO)) // 新增前檢查主鍵是否已存在
                    {
                        ME_DOCD.SEQ = "1";
                    }
                    else
                    {
                        ME_DOCD.SEQ = repo.GetDocDSeq(ME_DOCD.DOCNO);
                    }
                    //ME_DOCD.APVID = User.Identity.Name;
                    //ME_DOCD.ACKID = User.Identity.Name;
                    ME_DOCD.CREATE_USER = User.Identity.Name;
                    ME_DOCD.UPDATE_USER = User.Identity.Name;
                    ME_DOCD.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.CreateD(ME_DOCD);
                    session.Result.etts = repo.GetM(ME_DOCD.DOCNO);
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
        // 修改
        [HttpPost]
        public ApiResponse UpdateM(ME_DOCM ME_DOCM)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0010Repository(DBWork);

                    ME_DOCM.UPDATE_USER = User.Identity.Name;
                    ME_DOCM.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(ME_DOCM);
                    session.Result.etts = repo.GetM(ME_DOCM.DOCNO);

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
        public ApiResponse UpdateD(ME_DOCD ME_DOCD)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0010Repository(DBWork);

                    ME_DOCD.UPDATE_USER = User.Identity.Name;
                    ME_DOCD.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(ME_DOCD);
                    var rtn = repo.CallProc(ME_DOCD.DOCNO, User.Identity.Name, DBWork.ProcIP);
                    if (rtn == "Y")
                    {
                        session.Result.afrs = repo.ApplyDD(ME_DOCD);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>申請單號「" + ME_DOCD.DOCNO + "」</span>，發生執行錯誤，" + rtn + "。";
                    }

                    session.Result.etts = repo.GetD(ME_DOCD.DOCNO);

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
        public ApiResponse UpdateMeDocd(ME_DOCD me_docd)
        {
            IEnumerable<ME_DOCD> me_docds = JsonConvert.DeserializeObject<IEnumerable<ME_DOCD>>(me_docd.ITEM_STRING);
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);

                    foreach (ME_DOCD docd in me_docds)
                    {
                        docd.UPDATE_USER = User.Identity.Name;
                        docd.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.UpdateMeDocd(docd);
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
        // 刪除
        [HttpPost]
        public ApiResponse DeleteM(ME_DOCM ME_DOCM)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0010Repository(DBWork);
                    if (repo.CheckExists(ME_DOCM.DOCNO))
                    {
                        if (repo.CheckExistsD(ME_DOCM.DOCNO))
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>明細尚有資料</span>不可刪除。";
                        }
                        else
                        {
                            session.Result.afrs = repo.DeleteM(ME_DOCM);
                            session.Result.etts = repo.GetM(ME_DOCM.DOCNO);
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>單號</span>不存在。";
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
        public ApiResponse DeleteD(ME_DOCD ME_DOCD)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0010Repository(DBWork);
                    if (repo.CheckExistsDD(ME_DOCD.DOCNO, ME_DOCD.SEQ))
                    {
                        session.Result.afrs = repo.DeleteD(ME_DOCD);
                        session.Result.etts = repo.GetD(ME_DOCD.DOCNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>單號項次</span>不存在。";
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
        public ApiResponse Apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        string msg = string.Empty;
                        List<ME_DOCD> ExpqtyLargerCM = new List<ME_DOCD>(); // 預計核撥量加調撥量大於民庫存量加戰備存量
                        List<ME_DOCD> ExpqtyLargerC = new List<ME_DOCD>();  // 預計核撥量大於民庫存量
                        List<string> DocmStatusError = new List<string>();    // 此申請單狀態非核撥中
                        List<string> EmptyDocd = new List<string>();  // 沒有院內碼項次

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM(tmp[i])) {
                                DocmStatusError.Add(tmp[i]);
                                continue;
                            }

                            IEnumerable<ME_DOCD> docds = repo.GetAllDocds(tmp[i]);
                            
                            // 沒有院內碼項次
                            if (docds.Any() == false) {
                                EmptyDocd.Add(tmp[i]);
                                continue;
                            }

                            foreach (ME_DOCD docd in docds) {
                                // 調撥量 = 0 & 預計撥發量 = 0 => 不須判斷
                                // 預計核撥量加調撥量大於民庫存量加戰備存量
                                if (docd.BW_MQTY == "0" && docd.EXPT_DISTQTY == "0") {
                                    continue;
                                }
                                // 調撥量 > 0 => 比對須加上軍庫量
                                // 預計核撥量大於民庫存量
                                if (float.Parse(docd.BW_MQTY) > 0) {
                                    if (repo.CheckExptBwQtyMmcode(docd.DOCNO, docd.MMCODE)) {
                                        ExpqtyLargerCM.Add(docd);
                                    }
                                }
                                // 調撥量 = 0 => 比對民庫量
                                if (float.Parse(docd.BW_MQTY) == 0)
                                {
                                    if (repo.CheckExptQtyMmcode(docd.DOCNO, docd.MMCODE))
                                    {
                                        ExpqtyLargerC.Add(docd);
                                    }
                                }
                            }
                        }
                        // 若list中有東西，串成文字
                        if (EmptyDocd.Any()) {
                            string tempString = string.Empty;
                            if (msg == string.Empty) {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 沒有院內碼項次之申請單</span>：<br/>";
                            foreach (string temp in EmptyDocd) {
                                if (tempString.Length > 0) {
                                    tempString += "、";
                                }
                                tempString += temp;
                            }
                            msg += tempString;
                        }
                        if (DocmStatusError.Any()) {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 申請單狀態非核撥中</span>：<br/>";
                            foreach (string temp in DocmStatusError)
                            {
                                if (tempString.Length > 0)
                                {
                                    tempString += "、";
                                }
                                tempString += temp;
                            }
                            msg += tempString;
                        }
                        if (ExpqtyLargerCM.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 預計核撥量加調撥量大於民庫存量加戰備存量</span>：<br/>";
                            int i = 1;
                            foreach (ME_DOCD temp in ExpqtyLargerCM)
                            {
                                
                                if (tempString.Length > 0)
                                {
                                    tempString += "<br>";
                                }
                                tempString += string.Format("{0}. 申請單號:{1} 院內碼:{2}", i, temp.DOCNO, temp.MMCODE);
                                i++;
                            }
                            msg += tempString;
                        }
                        if (ExpqtyLargerC.Any())
                        {
                            string tempString = string.Empty;
                            if (msg == string.Empty)
                            {
                                msg += "請檢查下列項目：";
                            }
                            msg += "<br/><span style='color:red'>● 預計核撥量大於民庫存量</span>：<br/>";
                            int i = 1;
                            foreach (ME_DOCD temp in ExpqtyLargerC)
                            {
                                if (tempString.Length > 0)
                                {
                                    tempString += "<br>";
                                }
                                tempString += string.Format("{0}. 申請單號:{1} 院內碼:{2}", i, temp.DOCNO, temp.MMCODE);
                                i++;
                            }
                            msg += tempString;
                        }
                        // 若msg不為空，表示有錯誤
                        if (msg != string.Empty) {
                            session.Result.success = false;
                            session.Result.msg = msg;
                            return session.Result;
                        }
                        // 所有申請單通過檢核，進行更新
                        for (int i = 0; i < tmp.Length; i++) {
                            ME_DOCM me_docm = new ME_DOCM();
                            me_docm.DOCNO = tmp[i];
                            me_docm.UPDATE_USER = User.Identity.Name;
                            me_docm.UPDATE_IP = DBWork.ProcIP;
                            var rtn = repo.CallProc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                            if (rtn == "Y")
                            {
                                session.Result.afrs = repo.ApplyD(me_docm);
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」</span>，發生執行錯誤，" + rtn + "。";
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
        public ApiResponse ApplyX(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsM(tmp[i]))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>此申請單狀態非核撥中</span>不得退回。";
                            }
                            else
                            {
                                if (repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                                {
                                    ME_DOCM me_docm = new ME_DOCM();
                                    me_docm.DOCNO = tmp[i];
                                    me_docm.UPDATE_USER = User.Identity.Name;
                                    me_docm.UPDATE_IP = DBWork.ProcIP;
                                    session.Result.afrs = repo.ApplyX(me_docm);
                                }
                                else
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」沒有院內碼項次</span>，請新增院內碼項次。";
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
        public ApiResponse GetAlert()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    if (repo.CheckExistsAlert())
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "您尚有臨時申請單未處理。";
                    }
                    else
                    {
                        session.Result.success = true;
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
        public ApiResponse GetTask()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    session.Result.msg = repo.GetTaskid(User.Identity.Name);
                    session.Result.success = true;
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetDocnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    //var taskid = repo.GetTaskid(User.Identity.Name);
                    session.Result.etts = repo.GetDocnoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetFlowidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    session.Result.etts = repo.GetFlowidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetApplyKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    session.Result.etts = repo.GetApplyKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetAppDeptCombo(FormDataCollection form)
        {
            var matclass = form.Get("matclass");
            var dates = form.Get("dates");
            var datee = form.Get("datee");
            var applykind = form.Get("applykind");
            var flowid = form.Get("flowid");
            var date3 = form.Get("date3");
            string[] arr_p3 = { };
            if (!string.IsNullOrEmpty(flowid))
            {
                arr_p3 = flowid.Trim().Split(','); //用,分割
            }
            string[] arr_p5 = { };
            if (!string.IsNullOrEmpty(matclass))
            {
                arr_p5 = matclass.Trim().Split(','); //用,分割
            }
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    session.Result.etts = repo.GetAppDeptCombo(arr_p5, dates, datee, applykind, arr_p3, date3);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, p2, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeDocd(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeDocd(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetFrwhCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    session.Result.etts = repo.GetFrwhCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetTowhCombo()
        {
            var p0 = User.Identity.Name;
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    session.Result.etts = repo.GetTowhCombo(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0010Repository repo = new AA0010Repository(DBWork);
                    //var taskid = repo.GetTaskid(User.Identity.Name);
                    //if (taskid == "2")
                    //{
                    //    session.Result.etts = repo.GetMatclass2Combo();
                    //}
                    //else
                    //{
                    //    session.Result.etts = repo.GetMatclass3Combo();
                    //}
                    session.Result.etts = repo.GetMatclassCombo();
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