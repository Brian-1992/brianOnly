using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.BE;
using WebApp.Models;
using System;
using JCLib.DB.Tool;

namespace WebApp.Controllers.BE
{
    public class BE0002Controller : SiteBase.BaseApiController
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
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BE0002Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4, DBWork.UserInfo.Inid, hospCode, page, limit, sorters);
                }
                catch (Exception e)
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

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    BE0002Repository repo = new BE0002Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4, hospCode, DBWork.UserInfo.Inid));
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
        public ApiResponse Create(PH_VENDER ph_vender)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BE0002Repository(DBWork);
                    if (!repo.CheckExists(ph_vender.AGEN_NO)) // 新增前檢查主鍵是否已存在
                    {
                        if (ph_vender.MINODR_AMT == null)
                            ph_vender.MINODR_AMT = "0";
                        ph_vender.CREATE_USER = User.Identity.Name;
                        ph_vender.UPDATE_USER = User.Identity.Name;
                        ph_vender.UPDATE_IP = DBWork.ProcIP;
                        ph_vender.MAIN_INID = DBWork.UserInfo.Inid;
                        session.Result.afrs = repo.Create(ph_vender);
                        session.Result.etts = repo.Get(ph_vender.AGEN_NO);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>廠商碼</span>重複，請重新輸入。";
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
        public ApiResponse Update(PH_VENDER ph_vender)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new BE0002Repository(DBWork);
                    if (ph_vender.MINODR_AMT == null)
                        ph_vender.MINODR_AMT = "0";
                    ph_vender.UPDATE_USER = User.Identity.Name;
                    ph_vender.UPDATE_IP = DBWork.ProcIP;
                    ph_vender.MAIN_INID = DBWork.UserInfo.Inid;
                    session.Result.afrs = repo.Update(ph_vender);
                    session.Result.etts = repo.Get(ph_vender.AGEN_NO);

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
        //public ApiResponse Delete(PH_VENDER ph_vender)
        //{
        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        DBWork.BeginTransaction();
        //        try
        //        {
        //            var repo = new BE0002Repository(DBWork);
        //            if (repo.CheckExists(ph_vender.AGEN_NO))
        //            {
        //                session.Result.afrs = repo.Delete(ph_vender.AGEN_NO);
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
    }
}