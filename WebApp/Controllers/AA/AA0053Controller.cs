using System.Net.Http.Formatting;
using System.Web.Http;
using System.Net.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Diagnostics;
namespace WebApp.Controllers.AA
{
    public class AA0053Controller : SiteBase.BaseApiController
    {
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
                    var repo = new AA0053Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, page, limit, sorters);
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
                    var repo = new AA0053Repository(DBWork);
                    session.Result.etts = repo.GetAllD(p0, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Create(ME_UIMAST me_uimast)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0053Repository(DBWork);

                    me_uimast.WH_NO = "PH1S";
                    if (!repo.CheckExists(me_uimast.WH_NO, me_uimast.MMCODE)) // 新增前檢查主鍵是否已存在
                    {
                        me_uimast.CREATE_USER = User.Identity.Name;
                        me_uimast.UPDATE_USER = User.Identity.Name;
                        me_uimast.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(me_uimast);         //問 session.Result.afrs
                        session.Result.etts = repo.Get(me_uimast.WH_NO, me_uimast.MMCODE);      //搜尋一筆，且在ext上loadStore
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房代碼與院內碼</span>重複，請重新輸入。";
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
        public ApiResponse Update(ME_UIMAST me_uimast)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0053Repository(DBWork);
                    me_uimast.WH_NO = "PH1S";
                    me_uimast.UPDATE_USER = User.Identity.Name;
                    me_uimast.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(me_uimast);
                    session.Result.etts = repo.Get(me_uimast.WH_NO, me_uimast.MMCODE); 

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
        public ApiResponse UpdateD(FormDataCollection form)
        {
            var mmcode = form.Get("MMCODE");
            var pack_unit0 = form.Get("PACK_UNIT0");
            var pack_qty0 = form.Get("PACK_QTY0");
            var pack_unit1 = form.Get("PACK_UNIT1");
            var pack_qty1 = form.Get("PACK_QTY1");
            var pack_unit2 = form.Get("PACK_UNIT2");
            var pack_qty2 = form.Get("PACK_QTY2");
            var pack_unit3 = form.Get("PACK_UNIT3");
            var pack_qty3 = form.Get("PACK_QTY3");
            var pack_unit4 = form.Get("PACK_UNIT4");
            var pack_qty4 = form.Get("PACK_QTY4");
            var pack_unit5 = form.Get("PACK_UNIT5");
            var pack_qty5 = form.Get("PACK_QTY5");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0053Repository(DBWork);
                    ME_UIMAST me_uimast = new ME_UIMAST();
                    me_uimast.WH_NO = "PH1S";
                    me_uimast.MMCODE = mmcode;
                    me_uimast.PACK_UNIT0 = pack_unit0;
                    me_uimast.PACK_QTY0 = pack_qty0;
                    me_uimast.PACK_UNIT1 = pack_unit1;
                    me_uimast.PACK_QTY1 = pack_qty1;
                    me_uimast.PACK_UNIT2 = pack_unit2;
                    me_uimast.PACK_QTY2 = pack_qty2;
                    me_uimast.PACK_UNIT3 = pack_unit3;
                    me_uimast.PACK_QTY3 = pack_qty3;
                    me_uimast.PACK_UNIT4 = pack_unit4;
                    me_uimast.PACK_QTY4 = pack_qty4;
                    me_uimast.PACK_UNIT5 = pack_unit5;
                    me_uimast.PACK_QTY5 = pack_qty5;
                    if (repo.CheckExists(me_uimast.WH_NO, me_uimast.MMCODE))
                    {
                        me_uimast.UPDATE_USER = User.Identity.Name;
                        me_uimast.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Update(me_uimast);
                        session.Result.etts = repo.Get(me_uimast.WH_NO, me_uimast.MMCODE);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>院內碼</span>不存在，請重新輸入。";

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
        public ApiResponse Delete(ME_UIMAST me_uimast)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0053Repository(DBWork);
                    if (repo.CheckExists(me_uimast.WH_NO, me_uimast.MMCODE))
                    {
                        session.Result.afrs = repo.Delete(me_uimast.WH_NO, me_uimast.MMCODE, me_uimast.PACK_UNIT);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房代碼與院內碼</span>不存在。";
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
        public ApiResponse GetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
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
                    AA0053Repository repo = new AA0053Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo(p0, p1, page, limit, "");

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetMMCODECombo2(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
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
                    AA0053Repository repo = new AA0053Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo2(p0, p1, page, limit, "");

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