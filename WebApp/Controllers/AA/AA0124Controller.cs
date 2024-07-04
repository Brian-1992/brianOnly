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
    public class AA0124Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p4 = form.Get("p4");
            var p6 = form.Get("p6");
            var p7 = form.Get("p7"); // 庫備

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0124Repository(DBWork);
                    //string doctype = "MR2"; //衛材庫備doctype

                    //session.Result.etts = repo.GetAllM(p0, p1, p2, p4, doctype, p6);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, p4, p7, p6);
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
                    var repo = new AA0124Repository(DBWork);
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
                    var repo = new AA0124Repository(DBWork);
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
                    var repo = new AA0124Repository(DBWork);
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
                    var repo = new AA0124Repository(DBWork);

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
                    var repo = new AA0124Repository(DBWork);

                    ME_DOCD.UPDATE_USER = User.Identity.Name;
                    ME_DOCD.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(ME_DOCD);
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
            bool spFail = false;
            string mmcodes = string.Empty;
            foreach (ME_DOCD docd in me_docds)
            {
                if (mmcodes != string.Empty)
                {
                    mmcodes += ",";
                }
                mmcodes += ("'" + docd.MMCODE + "'");
            }
            IEnumerable<ME_DOCD> ori_docds = new List<ME_DOCD>();
            string docno = me_docds.First().DOCNO;

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    AA0124Repository repo = new AA0124Repository(DBWork);
                    var dseq = 1;

                    ori_docds = repo.GetCurrentDocds(docno, mmcodes);

                    // 更新ME_DOCD
                    DBWork.BeginTransaction();
                    foreach (ME_DOCD docd in me_docds)
                    {
                        docd.UPDATE_USER = User.Identity.Name;
                        docd.UPDATE_IP = DBWork.ProcIP;

                        session.Result.afrs = repo.UpdateMeDocd(docd);
                    }
                    DBWork.Commit();

                    // 呼叫stored_procedure
                    DBWork.BeginTransaction();
                    int i = 0;
                    foreach (ME_DOCD docd in me_docds)
                    {
                        dseq = int.Parse(docd.SEQ);
                        i++;
                        if (docd.STAT == "M")
                        {
                            var rtn = repo.CallProc2(docd.DOCNO, dseq, User.Identity.Name, DBWork.ProcIP);
                            if (rtn != "Y")
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>申請單號「" + docd.DOCNO + "」</span>，發生執行錯誤，" + rtn + "。";
                                spFail = true;
                                DBWork.Rollback();
                                break;
                            }
                        }
                    }
                    // 沒有失敗 直接commit結束
                    if (spFail == false)
                    {
                        DBWork.Commit();
                        return session.Result;
                    }

                    // sp回傳錯誤，還原已更新資料
                    DBWork.BeginTransaction();
                    if (spFail)
                    {
                        foreach (ME_DOCD ori in ori_docds)
                        {
                            session.Result.afrs = repo.UpdateMeDocd(ori);
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
                    var repo = new AA0124Repository(DBWork);
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
                    var repo = new AA0124Repository(DBWork);
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
                    AA0124Repository repo = new AA0124Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            {
                                if (repo.CheckExistsDN(tmp[i]))
                                {
                                    session.Result.afrs = 0;
                                    session.Result.success = false;
                                    session.Result.msg = "<span style='color:red'>此單明細尚有申請數量為0</span>不得核撥。";
                                }
                                else
                                {
                                    ME_DOCM me_docm = new ME_DOCM();
                                    me_docm.DOCNO = tmp[i];
                                    me_docm.UPDATE_USER = User.Identity.Name;
                                    me_docm.UPDATE_IP = DBWork.ProcIP;

                                    session.Result.afrs = repo.ApplyD(me_docm);
                                    var rtn = repo.CallProc(tmp[i], User.Identity.Name, DBWork.ProcIP);
                                    if (rtn == "N")
                                    {
                                        session.Result.afrs = 0;
                                        session.Result.success = false;
                                        session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」</span>，發生執行錯誤，" + rtn + "。";

                                        return session.Result;
                                    }
                                }
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」沒有院內碼項次</span>，請新增院內碼項次。";
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
        public ApiResponse GetDocnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0124Repository repo = new AA0124Repository(DBWork);
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
                    AA0124Repository repo = new AA0124Repository(DBWork);
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
                    AA0124Repository repo = new AA0124Repository(DBWork);
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
        public ApiResponse GetAppDeptCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0124Repository repo = new AA0124Repository(DBWork);
                    session.Result.etts = repo.GetAppDeptCombo();
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0124Repository repo = new AA0124Repository(DBWork);

                    List<string> taskids = repo.GetTaskid(User.Identity.Name).ToList();

                    string taskid = "3";
                    if (taskids.Contains("2"))
                    {
                        taskid = "2";
                    }

                    session.Result.etts = repo.GetMmCodeCombo(p0, taskid, page, limit, "");
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
                    AA0124Repository repo = new AA0124Repository(DBWork);
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
                    AA0124Repository repo = new AA0124Repository(DBWork);
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
                    AA0124Repository repo = new AA0124Repository(DBWork);
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
                    AA0124Repository repo = new AA0124Repository(DBWork);
                    List<string> taskids = repo.GetTaskid(User.Identity.Name).ToList();

                    if (taskids.Contains("2"))
                    {
                        session.Result.etts = repo.GetMatclass2Combo();
                    }
                    else
                    {
                        session.Result.etts = repo.GetMatclass3Combo();
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
        public ApiResponse GetStoreidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0124Repository repo = new AA0124Repository(DBWork);
                    session.Result.etts = repo.GetStoreidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        /*
        [HttpPost]
        public ApiResponse GetYN()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0124Repository repo = new AA0124Repository(DBWork);
                    session.Result.etts = repo.GetYN();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWhGrade()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0124Repository repo = new AA0124Repository(DBWork);
                    session.Result.etts = repo.GetWhGrade();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWhKind()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0124Repository repo = new AA0124Repository(DBWork);
                    session.Result.etts = repo.GetWhKind();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        */
    }
}