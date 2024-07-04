using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using System.Data;

namespace WebApp.Controllers.AA
{
    public class AA0082Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 月份別
            var p1 = form.Get("p1"); // 排序

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0082Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ChkTempData(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 月份別
            var p1 = form.Get("p1"); // 排序

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0082Repository(DBWork);
                    int sqlCount = repo.GetTempCount(p0);
                    string sign_time = null;
                    if (sqlCount > 0)
                        sign_time = repo.GetTempST(p0);

                    if (sqlCount > 0 && sign_time != null && sign_time != "")
                    {
                        session.Result.msg = "sign"; // 此月份資料已轉正式,只能查詢
                    }
                    else if (sqlCount > 0 && (sign_time == null || sign_time == ""))
                    {
                        session.Result.msg = "dup"; // 此月份資料已存在是否要覆蓋?
                    }
                    else // 新增然後查詢
                    {
                        session.Result.afrs = repo.InsertTemp(p0, User.Identity.Name, DBWork.ProcIP);
                        session.Result.msg = "query";
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
        public ApiResponse RpTempData(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 月份別
            var p1 = form.Get("p1"); // 排序

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new AA0082Repository(DBWork);
                    session.Result.afrs = repo.DeleteTemp(p0);
                    session.Result.afrs = repo.InsertTemp(p0, User.Identity.Name, DBWork.ProcIP);
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
        public ApiResponse Set(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))// 如果要取得DBWork資訊(如ProcIP),WorkSession需代入this
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var p0 = form.Get("p0"); // 月份別
                    var p1 = form.Get("p1"); // 排序選項
                    var property = form.Get("property"); // 欄位 
                    var direction = form.Get("direction"); // 排序方向
                    var UPDATE_USER = User.Identity.Name;
                    var UPDATE_IP = DBWork.ProcIP;
                
                    var repo = new AA0082Repository(DBWork);
                    var dt = new DataTable();
                    dt = repo.Set_GetDataTable(p0, p1, property, direction);
                    bool isOK = true;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (int.Parse(dt.Rows[i]["ROWITEM"].ToString().Trim()) <= 90)
                        {
                            session.Result.afrs = repo.UpdateMiMast(p0, "1", UPDATE_USER, UPDATE_IP, dt.Rows[i]["MMCODE"].ToString().Trim());
                            if (session.Result.afrs == 0)
                            {
                                isOK = false;
                                break;
                            }
                            //session.Result.afrs = repo.UpdateMiWinvctl(20, 30, float.Parse(dt.Rows[i]["DAYAVGQTY"].ToString().Trim()), dt.Rows[i]["MMCODE"].ToString().Trim());
                            //if (session.Result.afrs == 0)
                            //{
                            //    isOK = false;
                            //    break;
                            //}
                        }
                        else
                        {
                            session.Result.afrs = repo.UpdateMiMast(p0, "2", UPDATE_USER, UPDATE_IP, dt.Rows[i]["MMCODE"].ToString().Trim());
                            if (session.Result.afrs == 0)
                            {
                                isOK = false;
                                break;
                            }
                            //session.Result.afrs = repo.UpdateMiWinvctl(12, 20, float.Parse(dt.Rows[i]["DAYAVGQTY"].ToString().Trim()), dt.Rows[i]["MMCODE"].ToString().Trim());
                            //if (session.Result.afrs == 0)
                            //{
                            //    isOK = false;
                            //    break;
                            //}
                        }
                    }
                    if (isOK)
                        DBWork.Commit();
                    else
                        DBWork.Rollback();
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
        public ApiResponse SetPurtype(FormDataCollection form)
        {
            var mmcode = form.Get("mmcode");
            var purtype = form.Get("purtype");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    AA0082Repository repo = new AA0082Repository(DBWork);
                    session.Result.afrs = repo.SetPurtype(mmcode, purtype);
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
        public ApiResponse SetMastPurtype(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var p0 = form.Get("p0"); // 月份別
                    AA0082Repository repo = new AA0082Repository(DBWork);
                    session.Result.afrs += repo.UpdateSignTime(p0);
                    session.Result.afrs = repo.SetMastPurtype(p0, User.Identity.Name, DBWork.ProcIP); // PH_PURTYPE_T轉更新MI_MAST
                    session.Result.afrs += repo.UpdateMiWinvctlT1(p0); // MI_WINVCTL更新甲案
                    session.Result.afrs += repo.UpdateMiWinvctlT2(p0); // MI_WINVCTL更新乙案
                    // session.Result.afrs += repo.DeleteTemp(); // 刪除PH_PURTYPE_T資料
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
        public void Excel(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0082Repository(DBWork);

                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(form.Get("p0"), form.Get("p1")));
                }
                catch
                {
                    throw;
                }
            }
        }
        
    }
}