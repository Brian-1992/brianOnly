using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AB;
using WebApp.Models;

namespace WebApp.Controllers.AB
{
    public class AB0138Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0138Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, page, limit, sorters);
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0138Repository(DBWork);
                    var p1 = User.Identity.Name;
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
        public ApiResponse CreateM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0138Repository(DBWork);

                    var v_inid = repo.GetUridInid(User.Identity.Name);
                    var v_docno = repo.GetDocno();

                    PUBDGM pubdgm = new PUBDGM();
                    pubdgm.DOCNO = v_docno;
                    pubdgm.APPID = User.Identity.Name;
                    pubdgm.APPDEPT = v_inid;
                    pubdgm.MEMO = "";
                    pubdgm.STATUS = "N";
                    pubdgm.IS_DEL = "N";
                    pubdgm.CREATE_USER = User.Identity.Name;
                    pubdgm.UPDATE_USER = User.Identity.Name;
                    pubdgm.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.CreateM(pubdgm);

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
        public ApiResponse CreateD(PUBDGL pubdgl)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0138Repository(DBWork);
                    if (!repo.CheckMmcode(pubdgl.MMCODE,pubdgl.DOCNO))
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>此藥材代碼「" + pubdgl.MMCODE + "」不正確</span>，請確認。";
                    }
                    else
                    {
                        if (repo.CheckExistsMM(pubdgl.DOCNO, pubdgl.MMCODE))
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>此單明細已有重複的藥材代碼「" + pubdgl.MMCODE + "」</span>，請確認。";
                        }
                        else
                        {
                            string vs_highqty = repo.GetHighQty(User.Identity.Name, pubdgl.MMCODE);
                            if (vs_highqty == null)
                            {
                                pubdgl.HIGH_QTY = "0";
                            }
                            else
                            {
                                pubdgl.HIGH_QTY = vs_highqty;
                            }
                            string vs_invqty = repo.GetInvQty(User.Identity.Name, pubdgl.MMCODE);
                            if (vs_invqty == null)
                            {
                                pubdgl.INV_QTY = "0";
                            }
                            else
                            {
                                pubdgl.INV_QTY = vs_invqty;
                            }
                            pubdgl.MEMO = "";
                            pubdgl.ISISSUE = "N";
                            pubdgl.ISWAS = "N";
                            pubdgl.IS_DEL = "N";
                            pubdgl.ACKQTY = 0;
                            pubdgl.CREATE_USER = User.Identity.Name;
                            pubdgl.UPDATE_USER = User.Identity.Name;
                            pubdgl.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.CreateD(pubdgl);
                            session.Result.etts = repo.GetD(User.Identity.Name, pubdgl.DOCNO, pubdgl.MMCODE);
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
        [HttpPost]
        public ApiResponse UpdateD(PUBDGL pubdgl)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0138Repository(DBWork);

                    pubdgl.UPDATE_USER = User.Identity.Name;
                    pubdgl.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateD(pubdgl);
                    session.Result.etts = repo.GetD(User.Identity.Name, pubdgl.DOCNO, pubdgl.MMCODE);

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
        public ApiResponse DeleteM(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0138Repository repo = new AB0138Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            PUBDGM pubdgm = new PUBDGM();
                            pubdgm.DOCNO = tmp[i];
                            pubdgm.UPDATE_USER = User.Identity.Name;
                            pubdgm.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.DeleteM(pubdgm);
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
                    AB0138Repository repo = new AB0138Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string mmcode = form.Get("MMCODE").Substring(0, form.Get("MMCODE").Length - 1);
                        string[] tmp_docno = docno.Split(',');
                        string[] tmp_mmcode = mmcode.Split(',');
                        for (int i = 0; i < tmp_docno.Length; i++)
                        {
                            PUBDGL pubdgl = new PUBDGL();
                            pubdgl.DOCNO = tmp_docno[i];
                            pubdgl.MMCODE = tmp_mmcode[i];
                            pubdgl.UPDATE_USER = User.Identity.Name;
                            pubdgl.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.DeleteD(pubdgl);
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
        public ApiResponse Apply(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AB0138Repository repo = new AB0138Repository(DBWork);
                    if (form.Get("DOCNO") != "")
                    {
                        string docno = form.Get("DOCNO").Substring(0, form.Get("DOCNO").Length - 1); // 去除前端傳進來最後一個逗號
                        string[] tmp = docno.Split(',');
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (repo.CheckExistsD(tmp[i])) // 傳入DOCNO檢查申購單是否有藥材代碼項次
                            {
                                PUBDGM pubdgm = new PUBDGM();
                                pubdgm.DOCNO = tmp[i];
                                pubdgm.UPDATE_USER = User.Identity.Name;
                                pubdgm.UPDATE_IP = DBWork.ProcIP;
                                session.Result.afrs = repo.ApplyD(pubdgm);
                                session.Result.afrs = repo.ApplyM(pubdgm);
                            }
                            else
                            {
                                session.Result.afrs = 0;
                                session.Result.success = false;
                                session.Result.msg = "<span style='color:red'>補撥單號「" + tmp[i] + "」沒有藥材代碼</span>，請新增藥材代碼。";
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
                    AB0138Repository repo = new AB0138Repository(DBWork);
                    var p1 = User.Identity.Name;
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        

        [HttpPost]
        public ApiResponse GetIsDefCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0138Repository repo = new AB0138Repository(DBWork);
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
        public ApiResponse GetLoginInfo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0138Repository repo = new AB0138Repository(DBWork);
                    var v_userid = User.Identity.Name;
                    var v_ip = DBWork.ProcIP;
                    session.Result.etts = repo.GetLoginInfo(v_userid, v_ip);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDocAppAmout(FormDataCollection form)
        {
            var docno = form.Get("docno");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0138Repository repo = new AB0138Repository(DBWork);
                    session.Result.msg = repo.GetDocAppAmout(docno);
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