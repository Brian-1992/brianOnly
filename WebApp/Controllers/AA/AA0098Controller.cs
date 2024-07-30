using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WebApp.Controllers.AA
{
    public class AA0098Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0098Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, p3);
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
        public ApiResponse CreateM(AA0098 aa0098)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0098Repository(DBWork);
                    aa0098.CREATE_USER = User.Identity.Name;
                    aa0098.UPDATE_USER = User.Identity.Name;
                    aa0098.UPDATE_IP = DBWork.ProcIP;
                    if (!repo.CheckExists(aa0098.MMCODEB)) // 新增前檢查主鍵是否已存在
                    {//CREATE
                        session.Result.afrs = repo.CreateM(aa0098);
                    }
                    else
                    {//UPDATE
                        session.Result.afrs = repo.UpdateM(aa0098);
                    }
                    if (!repo.CheckExistsUnit(aa0098.MMCODEB,aa0098.M_PURUNB,aa0098.M_AGENNOB)) // 新增前檢查主鍵是否已存在
                    {//CREATE
                        session.Result.afrs = repo.CreateUnit(aa0098);
                    }
                    else
                    {//UPDATE
                        session.Result.afrs = repo.UpdateUnit(aa0098);
                    }
                    if (!repo.CheckExistsInv(aa0098.MMCODEB)) // 新增前檢查主鍵是否已存在
                    {//CREATE
                        session.Result.afrs = repo.CreateInv(aa0098);
                    }

                    session.Result.etts = repo.GetM(aa0098.MMCODEB);
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
        public ApiResponse CreateN(AA0098 aa0098)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0098Repository(DBWork);
                    aa0098.CREATE_USER = User.Identity.Name;
                    aa0098.UPDATE_USER = User.Identity.Name;
                    aa0098.UPDATE_IP = DBWork.ProcIP;
                    if (!repo.CheckExistsN(aa0098.MMCODEB)) // 新增前檢查主鍵是否已存在
                    {//CREATE
                        session.Result.afrs = repo.CreateN(aa0098);
                    }
                    else
                    {//UPDATE
                        session.Result.afrs = repo.UpdateN(aa0098);
                    }
                    if (!repo.CheckExistsUnit(aa0098.MMCODEB, aa0098.M_PURUNB, aa0098.M_AGENNOB)) // 新增前檢查主鍵是否已存在
                    {//CREATE
                        session.Result.afrs = repo.CreateUnit(aa0098);
                    }
                    else
                    {//UPDATE
                        session.Result.afrs = repo.UpdateUnit(aa0098);
                    }
                    if (!repo.CheckExistsInv(aa0098.MMCODEB)) // 新增前檢查主鍵是否已存在
                    {//CREATE
                        session.Result.afrs = repo.CreateInv(aa0098);
                    }

                    session.Result.etts = repo.GetM(aa0098.MMCODEB);
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
        public ApiResponse GetMMCodeDocd(FormDataCollection form)
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
                    AA0098Repository repo = new AA0098Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeDocd(p0, page, limit, "");
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