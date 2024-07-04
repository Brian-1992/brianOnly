using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using System.Diagnostics;
using WebApp.Repository.AB;
using WebApp.Models;
using Newtonsoft.Json;
using Dapper;

namespace WebApp.Controllers.AB
{
    public class AB0048Controller : SiteBase.BaseApiController
    {
        // GET api/<controller>
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4") == "Y";
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0048Repository(DBWork);

                    IEnumerable<ME_UIMAST> list = repo.GetAll(p0, p1, p2, p3,p4, page, limit, sorters);
                    foreach (ME_UIMAST item in list)
                    {
                        item.DIFFER = repo.CheckPackExists(item.MMCODE, item.PACK_UNIT, item.PACK_QTY) ? "N" : "Y";
                    }
                    session.Result.etts = list;
                }
                catch(Exception e )
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
                    var repo = new AB0048Repository(DBWork);
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
                    var repo = new AB0048Repository(DBWork);
                    me_uimast.UPDATE_USER = User.Identity.Name;
                    me_uimast.UPDATE_IP = DBWork.ProcIP;
                    session.Result.afrs = repo.Update(me_uimast);
                    session.Result.etts = repo.Get(me_uimast.WH_NO, me_uimast.MMCODE); //可讓前端收到資料後關閉mask

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
        public ApiResponse GetWH_NOComboNotOne(FormDataCollection form) ////AB0036
        {
            var wh_no = form.Get("p0");
            var page = int.Parse(form.Get("page"));

            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0048Repository repo = new AB0048Repository(DBWork);
                    session.Result.etts = repo.GetWH_NOComboNotOne(User.Identity.Name, wh_no, page, limit, "")
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
                }
                catch
                {
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
                    var repo = new AB0048Repository(DBWork);
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
        public ApiResponse GetWH_NOComboOne(FormDataCollection form) //AA0048
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0048Repository repo = new AB0048Repository(DBWork);
                    session.Result.etts = repo.GetWH_NOComboOne(User.Identity.Name).Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetUnitCombo(FormDataCollection form)
        {
            var p0 = form.Get("p1");
            //var p1 = form.Get("p2");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0048Repository repo = new AB0048Repository(DBWork);
                    session.Result.etts = repo.GetUnitCombo(p0,  page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWH_NOCombo(FormDataCollection form) ////AB0036
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0048Repository repo = new AB0048Repository(DBWork);
                    session.Result.etts = repo.GetWH_NOCombo(User.Identity.Name)
                        .Select(w => new { WH_NO = w.WH_NO, WH_NAME = w.WH_NAME });

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
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
                    AB0048Repository repo = new AB0048Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, p1, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetCtdmdCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AB0048Repository repo = new AB0048Repository(DBWork);
                    session.Result.etts = repo.GetCtdmdCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetDifferList(FormDataCollection form) {
            string wh_no = form.Get("wh_no");
            string ctdmdccode = form.Get("ctdmdccode");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AB0048Repository(DBWork);
                    session.Result.etts = repo.GetDifferList(wh_no, ctdmdccode);
                }
                catch (Exception e) {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetPackUnitsCombo(FormDataCollection form) {
            string mmcode = form.Get("mmcode");
            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                try {
                    var repo = new AB0048Repository(DBWork);
                    session.Result.etts = repo.GetPackUnitsCombo(mmcode);
                } catch (Exception e) {
                    throw;
                }

                return session.Result;
            }

        }

        [HttpPost]
        public ApiResponse UpdateDifferList(FormDataCollection form) {
            string list = form.Get("list");

            IEnumerable<ME_UIMAST> items = JsonConvert.DeserializeObject<IEnumerable<ME_UIMAST>>(list);

            using (WorkSession session = new WorkSession(this)) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AB0048Repository(DBWork);
                    foreach (ME_UIMAST item in items) {
                        session.Result.afrs = repo.UpdateUimastByList(item.WH_NO, item.MMCODE, item.PACK_UNIT, item.PACK_QTY, item.PACK_TIMES, 
                                                                      DBWork.UserInfo.UserId, DBWork.ProcIP);
                    }

                    DBWork.Commit();
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    throw;
                }

                return session.Result;
            }
        }
    }
}