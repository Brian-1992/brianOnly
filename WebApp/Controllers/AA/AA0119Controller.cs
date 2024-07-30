using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0119Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));  //start:0
            var limit = int.Parse(form.Get("limit"));  //pageSize=20
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0119Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters); //撈出object
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
        public ApiResponse Create(MI_UNITCODE mi_unitcode)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0119Repository(DBWork);
                    if (!repo.CheckExists(mi_unitcode.UNIT_CODE)) // 新增前檢查主鍵是否已存在
                    {
                        mi_unitcode.CREATE_USER = User.Identity.Name;
                        mi_unitcode.UPDATE_USER = User.Identity.Name;
                        mi_unitcode.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(mi_unitcode);         //問 session.Result.afrs
                        session.Result.etts = repo.Get(mi_unitcode.UNIT_CODE); //搜尋一筆，且在ext上loadStore
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>計量單位代碼</span>已建檔，請重新輸入。";
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
        public ApiResponse Update(MI_UNITCODE mi_unitcode)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0119Repository(DBWork);
                    mi_unitcode.UPDATE_USER = User.Identity.Name;
                    mi_unitcode.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(mi_unitcode);
                    session.Result.etts = repo.Get(mi_unitcode.UNIT_CODE);

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
        public ApiResponse Delete(MI_UNITCODE mi_unitcode)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0119Repository(DBWork);
                    if (!repo.CheckExistsInOtherTable(mi_unitcode.UNIT_CODE, "MI_UNITEXCH"))
                    {
                        if (repo.CheckExists(mi_unitcode.UNIT_CODE))
                        {
                            session.Result.afrs = repo.Delete(mi_unitcode.UNIT_CODE);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>計量單位代碼</span>不存在。";
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>計量單位代碼</span>存在於其他表單，無法刪除。";
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