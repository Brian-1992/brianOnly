using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System;

namespace WebApp.Controllers.F
{
    public class FA0067Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var MAT_CLASS = form.Get("P0");
            var YYYMM_B = form.Get("P1");
            var YYYMM_E = form.Get("P2");
            var P3 = form.Get("P3");
            var P4 = form.Get("P4");
            var MMCODE = form.Get("P5");
            var ISCHK = form.Get("P7");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0067Repository(DBWork);
                    session.Result.etts = repo.GetAll(MAT_CLASS, YYYMM_B, YYYMM_E, P3, P4, MMCODE, ISCHK, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMATCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0067Repository(DBWork);
                    session.Result.etts = repo.GetMATCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0067Repository repo = new FA0067Repository(DBWork);
                    session.Result.etts = repo.GetMMCodeCombo(p0, page, limit, "");
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
            var MAT_CLASS = form.Get("P0");
            var YYYMM_B = form.Get("P1");
            var YYYMM_E = form.Get("P2");
            var P3 = form.Get("P3");
            var P4 = form.Get("P4");
            var MMCODE = form.Get("P5");
            var MAT_CLASS_N = form.Get("P6");
            var ISCHK = form.Get("P7");

            string str = "";

            if (P4 == "1")
            {
                str = "庫備";
            }

            if (P4 == "0")
            {
                str = "非庫備";
            }
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0067Repository repo = new FA0067Repository(DBWork);

                    JCLib.Excel.Export("進銷存報表" + "_" + DateTime.Now.ToString("yyyyMMddhhmm") + ".xls", repo.GetExcel(MAT_CLASS, YYYMM_B, YYYMM_E, P3, P4, MMCODE, ISCHK),
                         (dt) =>
                         {
                             return string.Format("{0}{1}{2}至{3}{4}進銷存報表", DBWork.UserInfo.InidName, MAT_CLASS_N.Length > 2 ? MAT_CLASS_N.Substring(3) : "", YYYMM_B, YYYMM_E, str);
                         });

                }//--{ 使用者單位v_user_deptname} +{ 物料分類名稱}+{ 開始年月}+至 +{ 結束年月}{ 庫備 / 非庫備 + 
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetHospCode()
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0067Repository(DBWork);
                    session.Result.success = true;
                    session.Result.msg = repo.GetHospCode();
                }
                catch (Exception e)
                {
                    throw;
                }
                return session.Result;
            }
        }
    }
}