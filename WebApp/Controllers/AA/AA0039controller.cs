using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;

namespace WebApp.Controllers.AA
{
    public class AA0039Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0039Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0,p1,p2,p3,p4,p5,p6, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0039Repository repo = new AA0039Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4, p5, p6));
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
        public ApiResponse Create(MI_WHMAST MI_WHMAST)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0039Repository(DBWork);
                    if (!repo.CheckExists(MI_WHMAST.WH_NO)) // 新增前檢查主鍵是否已存在
                    {
                        MI_WHMAST.CREATE_USER = User.Identity.Name;
                        MI_WHMAST.UPDATE_USER = User.Identity.Name;
                        MI_WHMAST.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(MI_WHMAST);
                        if (MI_WHMAST.D1 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "1";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "1";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D2 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "2";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "2";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D3 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "3";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "3";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D4 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "4";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "4";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D5 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "5";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "5";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D6 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "6";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "6";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D7 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "7";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "7";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }

                        session.Result.etts = repo.Get(MI_WHMAST.WH_NO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房代碼</span>重複，請重新輸入。";
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
        public ApiResponse Update(MI_WHMAST MI_WHMAST)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0039Repository(DBWork);
                    var ws_err = "N";
                    if (MI_WHMAST.CANCEL_ID == "Y" )
                    {
                        if (repo.CheckCancel(MI_WHMAST.WH_NO) == 1)
                        {
                            ws_err = "Y";
                        }
                    }
                    if ( ws_err == "Y" )
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>庫房仍有庫存，不可作廢。";
                    }
                    else
                    {
                        MI_WHMAST.UPDATE_USER = User.Identity.Name;
                        MI_WHMAST.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Update(MI_WHMAST);

                        // 移除MI_WHID
                        if (MI_WHMAST.CANCEL_ID == "Y") {
                            session.Result.afrs = repo.DeleteWhid(MI_WHMAST.WH_NO);
                        }

                        if (MI_WHMAST.D1 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "1";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "1";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D2 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "2";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {

                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "2";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D3 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "3";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "3";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D4 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "4";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "4";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D5 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "5";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "5";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D6 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "6";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "6";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }
                        if (MI_WHMAST.D7 == "Y")
                        {
                            MI_WHMAST.WEEKDAY = "7";
                            if (!repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.CreateAuto(MI_WHMAST);
                            }
                        }
                        else
                        {
                            MI_WHMAST.WEEKDAY = "7";
                            if (repo.CheckAutoExists(MI_WHMAST.WH_NO, MI_WHMAST.WEEKDAY))
                            {
                                session.Result.afrs = repo.DeleteAuto(MI_WHMAST);
                            }
                        }

                        session.Result.etts = repo.Get(MI_WHMAST.WH_NO);
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

        // 刪除
        //[HttpPost]
        //public ApiResponse Delete(MI_WHMAST MI_WHMAST)
        //{
        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new AA0039Repository(DBWork);
        //            if (repo.CheckExists(MI_WHMAST.WH_NO))
        //            {
        //                session.Result.afrs = repo.Delete(MI_WHMAST.WH_NO);
        //            }
        //            else
        //            {
        //                session.Result.afrs = 0;
        //                session.Result.success = false;
        //                session.Result.msg = "<span style='color:red'>廠商碼</span>不存在。";
        //            }

        //            DBWork.Commit();
        //        }
        //        catch
        //        {
        //            DBWork.Rollback();
        //            throw;
        //        }
        //        return session.Result;
        //    }
        //}

        [HttpPost]
        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0039Repository repo = new AA0039Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetPWhnoCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0039Repository repo = new AA0039Repository(DBWork);
                    session.Result.etts = repo.GetPWhnoCombo(p0,p1);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetInidCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0039Repository repo = new AA0039Repository(DBWork);
                    session.Result.etts = repo.GetInidCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetYN()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0039Repository repo = new AA0039Repository(DBWork);
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
                    AA0039Repository repo = new AA0039Repository(DBWork);
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
                    AA0039Repository repo = new AA0039Repository(DBWork);
                    session.Result.etts = repo.GetWhKind();
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