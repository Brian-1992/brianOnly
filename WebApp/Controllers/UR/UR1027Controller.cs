using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;
using System;

namespace WebApp.Controllers.UR
{
    public class UR1027Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 發訊人
            var p1 = form.Get("p1"); // 訊息內容
            var p2 = form.Get("p2"); // 訊息日期(起)
            var p3 = form.Get("p3"); // 訊息日期(訖)
            var p4 = form.Get("p4"); // 包含自己發送的訊息
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_MSGRepository(DBWork);

                    DBWork.BeginTransaction();
                    // 查詢時將訊息改為已讀
                    repo.UpdateReadFlag(User.Identity.Name);

                    DBWork.Commit();


                    string varINCLUDE_SELF = Convert.ToBoolean(p4) == true ? "Y" : "N";
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, varINCLUDE_SELF, User.Identity.Name, page, limit, sorters);
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
        public ApiResponse DialogAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_MSGRepository(DBWork);

                    DBWork.BeginTransaction();
                    // 查詢時將訊息改為已讀
                    repo.UpdateReadFlag(User.Identity.Name);

                    DBWork.Commit();

                    session.Result.etts = repo.GetDialogAll(p0, User.Identity.Name, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetSendUserCombo(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_MSGRepository repo = new UR_MSGRepository(DBWork);
                    session.Result.etts = repo.GetSendUserCombo(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetReceiveUserCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    string varP0 = Convert.ToBoolean(p0) == true ? "Y" : "N";
                    UR_MSGRepository repo = new UR_MSGRepository(DBWork);
                    session.Result.etts = repo.GetReceiveUserCombo(varP0, User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse SendMsg(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            string[] tmp = p0.Split(',');
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    UR_MSGRepository repo = new UR_MSGRepository(DBWork);
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        session.Result.afrs += repo.SendMsg(tmp[i], p1, User.Identity.Name);
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

        // 供底層呼叫檢查是否有需提醒的新訊息
        [HttpPost]
        public ApiResponse GetChkUrMsg(FormDataCollection form)
        {
            var p0 = form.Get("IS_FIRST_CHK");
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    UR_MSGRepository repo = new UR_MSGRepository(DBWork);
                    string rtnMsg = "0";
                    if (p0 == "Y")
                    {
                        // 登入時,檢查是否有超過7天未讀訊息
                        rtnMsg = repo.GetOldNotReadCnt(User.Identity.Name);
                    }

                    if (Convert.ToInt32(rtnMsg) > 0)
                    {
                        session.Result.msg = "O^" + rtnMsg;
                        return session.Result;
                    }
                    else
                    {
                        // 檢查是否有尚未通知的訊息
                        rtnMsg = repo.GetAlertCnt(User.Identity.Name);
                        session.Result.msg = "N^" + rtnMsg;
                    }
                    
                    // 確認完是否有通知後將FLAG設為Y
                    repo.UpdateAlertFlag(User.Identity.Name);

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
    }
}