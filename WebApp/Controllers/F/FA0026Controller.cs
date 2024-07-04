using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System;
using System.Data;
using System.Web;
using System.IO;

namespace WebApp.Controllers.F
{
    public class FA0026Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0026Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMatClassCombo() //FA0026
        {
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0026Repository repo = new FA0026Repository(DBWork);
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
                    FA0026Repository repo = new FA0026Repository(DBWork);
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
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p0_Name = form.Get("p0_Name");
            var p1 = form.Get("p1").Trim();
            var p2 = form.Get("p2").Trim();

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0026Repository repo = new FA0026Repository(DBWork);
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();

                    result = repo.GetExcel(p0, p1, p2);
                    dtItems.Merge(result);

                    string str_UserDept = DBWork.UserInfo.InidName;
                    string str_m_storeid = p2 == "1" ? "庫備" : "非庫備";
                    string export_FileName = str_UserDept + p0_Name + str_m_storeid + "基本資料表";
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