using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using System.Data;
using WebApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WebApp.Controllers.AA
{
    public class AA0138Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var drugType = form.Get("p1");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0138Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetAll(drugType, hospCode=="0");
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
            var p1 = form.Get("p1");
            bool p2 = form.Get("p2") == "Y";

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0138Repository repo = new AA0138Repository(DBWork);
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();
                    string hospCode = repo.GetHospCode();
                    result = repo.GetExcel(p1, p2, hospCode=="0");
                    dtItems.Merge(result);

                    string str_UserDept = DBWork.UserInfo.InidName;
                    string export_FileName = "各衛星庫房管制藥品清點";
                    JCLib.Excel.Export(export_FileName + ".xls", dtItems, (tmp_dt) => { return export_FileName; });
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse WhData(FormDataCollection form)
        {
            var drugType = form.Get("p1");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0138Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetWhData(drugType, hospCode=="0");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse WhExcel(FormDataCollection form)
        {
            var drugType = form.Get("p1");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0138Repository(DBWork);
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();
                    string hospCode = repo.GetHospCode();
                    result = repo.GetWhExcel(drugType, hospCode=="0");
                    dtItems.Merge(result);

                    string str_UserDept = DBWork.UserInfo.InidName;
                    string export_FileName = "各衛星庫房管制藥品清點庫房現況";
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