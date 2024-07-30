using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0118Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            //var p1 = form.Get("p1");
            //var p2 = form.Get("p2");
            //var p3 = form.Get("p3");
            //var p4 = form.Get("p4");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0118Repository(DBWork);
                    //session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, page, limit, sorters);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters);
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
        public ApiResponse Create(MI_MATCLASS mi_MATCLASS)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0118Repository(DBWork);
                    if (!repo.CheckExists(mi_MATCLASS.MAT_CLASS)) // 新增前檢查主鍵是否已存在
                    {
                        mi_MATCLASS.CREATE_USER = User.Identity.Name;
                        mi_MATCLASS.UPDATE_USER = User.Identity.Name;
                        mi_MATCLASS.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(mi_MATCLASS);
                        session.Result.etts = repo.Get(mi_MATCLASS.MAT_CLASS);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>物料分類代碼</span>重複，請重新輸入。";
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
        public ApiResponse Update(MI_MATCLASS mi_MATCLASS)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0118Repository(DBWork);
                    mi_MATCLASS.UPDATE_USER = User.Identity.Name;
                    mi_MATCLASS.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(mi_MATCLASS);
                    session.Result.etts = repo.Get(mi_MATCLASS.MAT_CLASS);

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
        public ApiResponse Delete(MI_MATCLASS mi_MATCLASS)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0118Repository(DBWork);
                    if (!repo.CheckExistsInOtherTable(mi_MATCLASS.MAT_CLASS, "MI_MAST") && !repo.CheckExistsInOtherTable(mi_MATCLASS.MAT_CLASS, "ME_DOCM"))
                    {
                        if (repo.CheckExists(mi_MATCLASS.MAT_CLASS))
                        {
                        session.Result.afrs = repo.Delete(mi_MATCLASS.MAT_CLASS);
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

        //匯出EXCEL
        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0118Repository repo = new AA0118Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0));
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