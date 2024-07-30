using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System.Text;

namespace WebApp.Controllers.B
{
    public class BD0011Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
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
                    var repo = new BD0011Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, page, limit, sorters);
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
        public ApiResponse Create(BD0011 BD0011)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0011Repository(DBWork);
                    if (!repo.CheckExists(BD0011.DOCID, BD0011.THEME)) // 新增前檢查主鍵是否已存在
                    {
                        BD0011.CREATE_USER = User.Identity.Name;
                        BD0011.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.CreateM(BD0011);
                        session.Result.afrs = repo.CreateD(BD0011);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>識別號及主旨</span>重複，請重新輸入。";
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

        // 修改
        [HttpPost]
        public ApiResponse Update(BD0011 BD0011)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0011Repository(DBWork);
                    BD0011.CREATE_USER = User.Identity.Name;
                    BD0011.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.UpdateM(BD0011);
                    session.Result.afrs = repo.UpdateD(BD0011);

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
        public ApiResponse Delete(BD0011 BD0011)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0011Repository(DBWork);
                    if (repo.CheckExists(BD0011.DOCID, BD0011.THEME))
                    {
                        session.Result.afrs = repo.DeleteM(BD0011.DOCID, BD0011.THEME);
                        session.Result.afrs = repo.DeleteD(BD0011.DOCID, BD0011.THEME);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>識別號及主旨</span>不存在。";
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

        //取得識別號
        public string GetDocid()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0011Repository(DBWork);
                    var DOCID = repo.GetDocid();
                    return DOCID;
                }
                catch
                {
                    throw;
                }
            }
        }

        // 寄送MAIL
        [HttpPost]
        public ApiResponse SendMail(BD0011 BD0011)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BD0011Repository(DBWork);
                    BD0011.CREATE_USER = User.Identity.Name;
                    BD0011.UPDATE_IP = DBWork.ProcIP;
                    // 部分廠商
                    if (BD0011.OPT == "P")
                    {
                        string[] agen_nos = BD0011.AGEN_NO.Split(',');
                        for (int i = 0; i < agen_nos.Length; i++)
                        {
                            repo.CreateAGENNO(BD0011, agen_nos[i]);
                        }
                    }
                    // 全部廠商
                    else if (BD0011.OPT == "A")
                    {
                        repo.CreateAGENNO_ALL(BD0011);
                    }
                    // 藥品廠商
                    else if (BD0011.OPT == "E")
                    {
                        repo.CreateAGENNO_MED(BD0011);
                    }
                    // 衛材廠商
                    else if (BD0011.OPT == "T")
                    {
                        repo.CreateAGENNO_MAT(BD0011);
                    }
                    // 其他廠商
                    else if (BD0011.OPT == "O")
                    {
                        repo.CreateAGENNO_OTHER(BD0011);
                    }
                    // 一般物品廠商
                    else if (BD0011.OPT == "G")
                    {
                        repo.CreateAGENNO_GEN(BD0011);
                    }
                    session.Result.afrs = repo.SendMail(BD0011);

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
        public ApiResponse GetAGEN(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0011Repository(DBWork);
                    session.Result.etts = repo.GetAGEN();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ErrorLog(FormDataCollection form) {
            string docid = form.Get("p0");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0011Repository(DBWork);
                    session.Result.etts = repo.GetErrorLogs(docid);
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