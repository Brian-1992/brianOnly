using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;

namespace WebApp.Controllers.AB
{
    public class AB0033Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p2 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0033Repository(DBWork);
                    var v_inid = User.Identity.Name; //repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.GetAllM(p0, p2, v_inid, page, limit, sorters);
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
                    var repo = new AB0033Repository(DBWork);
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
        public ApiResponse CreateM(MM_PACK_M MM_PACK_M)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0033Repository(DBWork);
                    if (!repo.CheckExists(MM_PACK_M.DOCNO)) // 新增前檢查主鍵是否已存在
                    {
                        var v_inid = repo.GetUridInid(User.Identity.Name);
                        var v_twntime = repo.GetTwnsystime();
                        var v_matclass = MM_PACK_M.MAT_CLASS;
                        var v_docno = v_inid + v_twntime + v_matclass;
                        var v_whno = repo.GetFrwh();


                        MM_PACK_M.DOCNO = v_docno;
                        MM_PACK_M.APPID = User.Identity.Name;
                        //MM_PACK_M.APPDEPT = v_inid;
                        MM_PACK_M.CREATE_USER = User.Identity.Name;
                        MM_PACK_M.UPDATE_USER = User.Identity.Name;
                        MM_PACK_M.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateM(MM_PACK_M);
                        session.Result.etts = repo.GetM(MM_PACK_M.DOCNO);
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
        public ApiResponse CreateD(MM_PACK_D MM_PACK_D)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0033Repository(DBWork);
                    var v_doctype = repo.GeStoreid(MM_PACK_D.DOCNO);
                    var v_storeid = "";
                    if (v_doctype == "MR1" || v_doctype == "MR2")
                    {
                        v_storeid = "1";
                    }
                    else
                    {
                        v_storeid = "0";
                    }
                    if (!repo.CheckMmcode(MM_PACK_D.MMCODE, MM_PACK_D.DOCNO, v_storeid))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此院內碼「" + MM_PACK_D.MMCODE + "」不正確</span>，請確認。";
                    }
                    else
                    {
                        if (repo.CheckExistsMM(MM_PACK_D.DOCNO, MM_PACK_D.MMCODE))
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>此單明細已有重複的院內碼「" + MM_PACK_D.MMCODE + "」</span>，請確認。";
                        }
                        else
                        {
                            if (!repo.CheckExistsD(MM_PACK_D.DOCNO)) // 新增前檢查主鍵是否已存在
                            {
                                MM_PACK_D.SEQ = "1";
                            }
                            else
                            {
                                MM_PACK_D.SEQ = repo.GetDocDSeq(MM_PACK_D.DOCNO);
                            }
                            MM_PACK_D.CREATE_USER = User.Identity.Name;
                            MM_PACK_D.UPDATE_USER = User.Identity.Name;
                            MM_PACK_D.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.CreateD(MM_PACK_D);
                            session.Result.etts = repo.GetD(MM_PACK_D.DOCNO, MM_PACK_D.SEQ);
                            DBWork.Commit();
                        }
                    }
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
        public ApiResponse UpdateM(MM_PACK_M MM_PACK_M)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0033Repository(DBWork);

                    MM_PACK_M.UPDATE_USER = User.Identity.Name;
                    MM_PACK_M.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(MM_PACK_M);
                    session.Result.etts = repo.GetM(MM_PACK_M.DOCNO);

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
        public ApiResponse UpdateD(MM_PACK_D MM_PACK_D)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0033Repository(DBWork);

                    MM_PACK_D.UPDATE_USER = User.Identity.Name;
                    MM_PACK_D.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(MM_PACK_D);
                    session.Result.etts = repo.GetD(MM_PACK_D.DOCNO, MM_PACK_D.SEQ);

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

        // 刪除
        [HttpPost]
        public ApiResponse DeleteM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0033Repository repo = new AB0033Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            //if (!repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申請單是否有院內碼項次
                            //{
                            session.Result.afrs = repo.DeleteAllD(tmp[i]);
                            session.Result.afrs = repo.DeleteM(tmp[i]);
                            //}
                            //else
                            //{
                            //    session.Result.afrs = 0;
                            //    session.Result.success = false;
                            //    session.Result.msg = "<span style='color:red'>申請單號「" + tmp[i] + "」有院內碼項次，請先刪除所有項次。</span>";
                            //    return session.Result;
                            //}
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
        public ApiResponse DeleteD(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0033Repository repo = new AB0033Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string seq = form.Get("SEQ").Substring(0, form.Get("SEQ").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_seq = seq.Split(',');
                        for (int i = 0; i < tmp_docno.Length; i++)
                            session.Result.afrs = repo.DeleteD(tmp_docno[i], tmp_seq[i]);
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
        public ApiResponse Apply(MM_PACK_M MM_PACK_M)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0033Repository(DBWork);
                    if (repo.CheckExists(MM_PACK_M.DOCNO))
                    {
                        if (!repo.CheckExistsD(MM_PACK_M.DOCNO))
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>此單無明細</span>不得核撥。";
                        }
                        else
                        {
                            if (repo.CheckExistsDN(MM_PACK_M.DOCNO))
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>此單明細尚有申請數量為0</span>不得核撥。";
                            }
                            else
                            {
                                MM_PACK_M.UPDATE_USER = User.Identity.Name;
                                MM_PACK_M.UPDATE_IP = DBWork.ProcIP;
                                session.Result.afrs = repo.ApplyM(MM_PACK_M);
                                session.Result.afrs = repo.ApplyD(MM_PACK_M);
                                session.Result.etts = repo.GetM(MM_PACK_M.DOCNO);
                            }
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
        public ApiResponse GetDocnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0033Repository repo = new AB0033Repository(DBWork);
                    var v_inid = repo.GetUridInid(User.Identity.Name);
                    session.Result.etts = repo.GetDocnoCombo(v_inid);
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
                    AB0033Repository repo = new AB0033Repository(DBWork);
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
                    AB0033Repository repo = new AB0033Repository(DBWork);
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
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var doctype = form.Get("p3");
            var p3 = "";
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0033Repository repo = new AB0033Repository(DBWork);
                    if (doctype == "MR1" || doctype == "MR2")
                    {
                        p3 = "1";
                    }
                    else
                    {
                        p3 = "0";
                    }
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, p3, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmcode(FormDataCollection form)
        {
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            var storeid = "0";
            var doctype = form.Get("DOCTYPE").ToUpper();
            if (doctype == "MR1" || doctype == "MR2")
            {
                storeid = "1";
            }
            else
            {
                storeid = "0";
            }
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0033Repository repo = new AB0033Repository(DBWork);
                    AB0033Repository.MI_MAST_QUERY_PARAMS query = new AB0033Repository.MI_MAST_QUERY_PARAMS();
                    query.MMCODE = form.Get("MMCODE") == null ? "" : form.Get("MMCODE").ToUpper();
                    query.MMNAME_C = form.Get("MMNAME_C") == null ? "" : form.Get("MMNAME_C").ToUpper();
                    query.MMNAME_E = form.Get("MMNAME_E") == null ? "" : form.Get("MMNAME_E").ToUpper();
                    query.STOREID = storeid;
                    query.MAT_CLASS = form.Get("MAT_CLASS") == null ? "" : form.Get("MAT_CLASS").ToUpper();
                    session.Result.etts = repo.GetMmcode(query, page, limit, sorters);
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
                    AB0033Repository repo = new AB0033Repository(DBWork);
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
                    AB0033Repository repo = new AB0033Repository(DBWork);
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
                    AB0033Repository repo = new AB0033Repository(DBWork);
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
        public ApiResponse GetMatclassCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0033Repository repo = new AB0033Repository(DBWork);
                    if (p0 == "MR1" || p0 == "MR3")
                    {
                        session.Result.etts = repo.GetMatclass3Combo();
                    }
                    else if (p0 == "MR2" || p0 == "MR4")
                    {
                        session.Result.etts = repo.GetMatclass2Combo();
                    }
                    else { 
                    session.Result.etts = repo.GetMatclassCombo();
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
        public ApiResponse GetDoctypeCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0033Repository repo = new AB0033Repository(DBWork);
                    session.Result.etts = repo.GetDoctypeCombo();
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
                    AB0033Repository repo = new AB0033Repository(DBWork);
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
                    AB0033Repository repo = new AB0033Repository(DBWork);
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
                    AB0033Repository repo = new AB0033Repository(DBWork);
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