using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System;

namespace WebApp.Controllers.C
{
    public class CB0002Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse AllM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0002Repository(DBWork);
                    session.Result.etts = repo.GetAllM(p0, p1, p2, p3, p4, p5, page, limit, sorters);
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
        public ApiResponse Create(CB0002M BC_BARCODE)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CB0002Repository(DBWork);

                    //2020-05-11 新增: 檢查院內碼是否存在
                    if (repo.CheckMmcodeExists(BC_BARCODE.MMCODE) == false) {
                        session.Result.success = false;
                        session.Result.msg = "[院內碼]不存在,無法新增";

                        return session.Result;
                    }

                    if (!repo.CheckExists(BC_BARCODE))
                    {
                        BC_BARCODE.CREATE_USER = DBWork.ProcUser;
                        BC_BARCODE.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(BC_BARCODE);
                        session.Result.etts = repo.Get(BC_BARCODE.MMCODE, BC_BARCODE.BARCODE);
                    }
                    else
                    {
                        session.Result.success = false;
                        session.Result.msg = "[條碼]已存在,無法新增";
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
        public ApiResponse Update(CB0002M BC_BARCODE)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CB0002Repository(DBWork);
                    BC_BARCODE.UPDATE_USER = User.Identity.Name;
                    BC_BARCODE.UPDATE_IP = DBWork.ProcIP;
                    if (BC_BARCODE.BARCODE == BC_BARCODE.BARCODE_OLD)
                    {
                        session.Result.afrs = repo.Update(BC_BARCODE);
                        session.Result.etts = repo.Get(BC_BARCODE.MMCODE, BC_BARCODE.BARCODE);
                    }
                    else
                    {
                        if (!repo.CheckExists(BC_BARCODE))
                        {
                            session.Result.afrs = repo.Update(BC_BARCODE);
                            session.Result.etts = repo.Get(BC_BARCODE.MMCODE, BC_BARCODE.BARCODE);
                        }
                        else
                        {
                            session.Result.success = false;
                            session.Result.msg = "[條碼]已存在,無法新增";
                        }
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
        public ApiResponse Delete(CB0002M BC_BARCODE)
        {
            using (WorkSession session = new WorkSession(this))
            {
                                session.Result.msg = "";
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CB0002Repository(DBWork);
                    BC_BARCODE.UPDATE_USER = User.Identity.Name;
                    BC_BARCODE.UPDATE_IP = DBWork.ProcIP;
                    if (repo.ChkMmcodeCnt(BC_BARCODE.MMCODE) > 1)
                        session.Result.afrs = repo.Delete(BC_BARCODE);
                    else
                        session.Result.msg = "此院內碼只有一筆資料，不可刪除。";
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
        public ApiResponse GetMmcodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var mat_class = form.Get("mat_class");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0002Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeCombo(p0, mat_class, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse GetCLSNAME()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0002Repository(DBWork);
                    session.Result.etts = repo.GetCLSNAME();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmcodeData(FormDataCollection form)
        {
            var mmcode = form.Get("mmcode");
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0002Repository(DBWork);
                    session.Result.etts = repo.GetMmcodeData(mmcode);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse Excel(FormDataCollection form)
        {
            var MAT_CLASS = form.Get("MAT_CLASS");
            var MMCODE = form.Get("MMCODE");
            var MMNAME_C = form.Get("MMNAME_C");
            var STATUS = form.Get("STATUS");
            var BARCODE = form.Get("BARCODE");
            var MMNAME_E = form.Get("MMNAME_E");
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CB0002Repository repo = new CB0002Repository(DBWork);
                    string dateTime = (DateTime.Now.Year - 1911).ToString() + DateTime.Now.ToString("MM");
                    JCLib.Excel.Export("品項條碼對照" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(MAT_CLASS, MMCODE, MMNAME_C, STATUS, BARCODE, MMNAME_E));
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