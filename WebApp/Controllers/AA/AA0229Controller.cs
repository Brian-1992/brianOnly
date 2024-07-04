using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0229Controller : SiteBase.BaseApiController
    {
        //查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
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
                    var repo = new AA0229Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        //新增
        [HttpPost]
        public ApiResponse Create(AA0229 input)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0229Repository(DBWork);
                    if (!repo.CheckExists(input.DATA_VALUE))
                    {
                        session.Result.afrs = repo.Create(input);
                        session.Result.etts = repo.Get(input.DATA_VALUE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>物料子類別代碼</span>重複，請重新輸入。";
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

        //修改
        [HttpPost]
        public ApiResponse Update(AA0229 input)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0229Repository(DBWork);
                    session.Result.afrs = repo.Update(input);
                    session.Result.etts = repo.Get(input.DATA_VALUE);
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
        public ApiResponse Delete(AA0229 input)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0229Repository(DBWork);
                    if (!repo.CheckExistsInOtherTable(input.DATA_VALUE, "MI_MAST") && !repo.CheckExistsInOtherTable(input.DATA_VALUE, "CHK_DETAIL"))
                    {
                        if (repo.CheckExists(input.DATA_VALUE))
                        {
                            session.Result.afrs = repo.Delete(input.DATA_VALUE);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>物料分類代碼</span>不存在。";
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>物料分類代碼</span>存在於其他表單，無法刪除。";
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