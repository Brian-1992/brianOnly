using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebApp.Repository.F;
using System.Web.Http;
using WebApp.Models;
using Newtonsoft.Json;
using JCLib.DB;

namespace WebApp.Controllers.F
{
    public class FA0028Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0028Repository(DBWork);
                    session.Result.etts = repo.GetAll(User.Identity.Name, form.Get("p0"), form.Get("p1"), form.Get("p2"));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatClassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0028Repository repo = new FA0028Repository(DBWork);
                    session.Result.etts = repo.GetMatClassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMMCODECombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0028Repository repo = new FA0028Repository(DBWork);
                    session.Result.etts = repo.GetMMCODECombo(p0, page, limit, "");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0028Repository repo = new FA0028Repository(DBWork);
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
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("P0");
            var p1 = form.Get("P1").Trim();
            var p1_name = form.Get("P1_Name").Trim();
            var p2 = form.Get("P2").Trim();

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0028Repository repo = new FA0028Repository(DBWork);
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();

                    result = repo.GetExcel(p0, p1, p2);
                    dtItems.Merge(result);

                    string str_UserDept = DBWork.UserInfo.InidName;
                    string export_FileName = str_UserDept + p1_name + "儲位基本資料表";
                    JCLib.Excel.Export(export_FileName + ".xls", dtItems, (tmp_dt) => { return export_FileName; });
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
