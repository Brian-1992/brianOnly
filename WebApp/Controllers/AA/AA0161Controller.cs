using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0161Controller : SiteBase.BaseApiController
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
                    var repo = new AA0161Repository(DBWork);
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
        public ApiResponse Create(SEC_MAST sec_mast)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0161Repository(DBWork);
                    if (!repo.CheckExists(sec_mast.SECTIONNO)) // 新增前檢查主鍵是否已存在
                    {
                        sec_mast.CREATE_USER = User.Identity.Name;
                        sec_mast.UPDATE_USER = User.Identity.Name;
                        sec_mast.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(sec_mast);
                        session.Result.etts = repo.Get(sec_mast.SECTIONNO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>科別代碼</span>重複，請重新輸入。";
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
        public ApiResponse Update(SEC_MAST sec_mast)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0161Repository(DBWork);
                    sec_mast.UPDATE_USER = User.Identity.Name;
                    sec_mast.UPDATE_IP = DBWork.ProcIP;
                    if (sec_mast.SEC_ENABLE == "N")
                    {
                        if (repo.CheckExistsInCalloc(sec_mast.SECTIONNO))
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>科別代碼</span>有對應的成本分攤比率，無法設定為停用。";
                        }
                    }
                    session.Result.afrs = repo.Update(sec_mast);
                    session.Result.etts = repo.Get(sec_mast.SECTIONNO);

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
        public ApiResponse Delete(SEC_MAST sec_mast)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0161Repository(DBWork);
                    if (!repo.CheckExistsInCalloc(sec_mast.SECTIONNO))
                    {
                        if (repo.CheckExists(sec_mast.SECTIONNO))
                        {
                            session.Result.afrs = repo.Delete(sec_mast.SECTIONNO);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>科別代碼</span>不存在。";
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>科別代碼</span>有對應的成本分攤比率，無法設定為停用。";
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
    }
}