using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.G;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Controllers.G
{
    public class GA0003Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0003Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1,p2, page, limit, sorters);
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
        public ApiResponse Create(TC_MMAGEN tc_mmagen)
        {
            using (WorkSession session = new WorkSession(this)) // 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0003Repository(DBWork);
                    if (repo.CheckMmcodeExists(tc_mmagen.MMCODE)) {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>電腦編號 " + tc_mmagen.MMCODE + "不存在</span>，請重新輸入。";

                        return session.Result;
                    }

                    if (!repo.CheckExists(tc_mmagen)) // 新增前檢查主鍵是否已存在
                    {
                        if (!repo.CheckExists2(tc_mmagen)) // 新增前檢查採購順序是否已存在
                        {
                            tc_mmagen.CREATE_USER = User.Identity.Name;
                            tc_mmagen.UPDATE_USER = User.Identity.Name;
                            tc_mmagen.UPDATE_IP = DBWork.ProcIP;
                            session.Result.afrs = repo.Create(tc_mmagen);
                            session.Result.etts = repo.Get(tc_mmagen);

                            ReCalculate(DBWork);
                        }
                        else
                        {
                            session.Result.afrs = 0;
                            session.Result.success = false;
                            session.Result.msg = "<span style='color:red'>電腦編號 " + tc_mmagen.MMCODE + " 之採購順序 " + tc_mmagen.PUR_SEQ + " 重複。</span>，請重新輸入。";
                        }
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>電腦編號 "+ tc_mmagen .MMCODE+ " ，藥商名稱 " + tc_mmagen.AGEN_NAMEC + " 已建檔。</span>，請重新輸入。";
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
        public ApiResponse Update(TC_MMAGEN tc_mmagen)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0003Repository(DBWork);

                    if (!repo.CheckExists2(tc_mmagen)) // 新增前檢查採購順序是否已存在
                    {
                        tc_mmagen.UPDATE_USER = User.Identity.Name;
                        tc_mmagen.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Update(tc_mmagen);
                        session.Result.etts = repo.Get(tc_mmagen);

                        ReCalculate(DBWork);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>電腦編號 " + tc_mmagen.MMCODE + " 之採購順序 " + tc_mmagen.PUR_SEQ + " 重複。</span>，請重新輸入。";
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
       [HttpPost]
        public ApiResponse Delete(TC_MMAGEN tc_mmagen)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new GA0003Repository(DBWork);
                    if (repo.CheckExists(tc_mmagen))
                    {
                        session.Result.afrs = repo.Delete(tc_mmagen);

                        ReCalculate(DBWork);
                    }
                    else
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = "<span style='color:red'>電腦編號 " + tc_mmagen.MMCODE + " ，藥商名稱 " + tc_mmagen.AGEN_NAMEC + " </span>不存在。";
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

        // 重新計算建議量
        public int ReCalculate(UnitOfWork DBWork)
        {
            try
            {
                GA0001Controller ctrl = new GA0001Controller();
                GA0001Repository repo = new GA0001Repository(DBWork);

                IEnumerable<TC_INVQMTR> list = repo.GetInvqmtrs();
                repo.DeleteInvqmtr();

                return ctrl.CalculateRCM(list, DBWork);
            }
            catch
            {
                DBWork.Rollback();
                throw;
            }
        }
    }
}