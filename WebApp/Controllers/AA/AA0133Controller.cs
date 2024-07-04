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
using System.Linq;

namespace WebApp.Controllers.AA
{
    public class AA0133Controller : SiteBase.BaseApiController
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
                    var repo = new AA0133Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo.GetAll(p0, drugType, hospCode == "0");
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse GetWH(FormDataCollection form)
        {
            bool isAll = form.Get("isAll") == "Y";
            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0133Repository repo = new AA0133Repository(DBWork);
                    string hospCode = repo.GetHospCode();
                    session.Result.etts = repo
                        .GetWH(DBWork.UserInfo.UserId, DBWork.UserInfo.Inid, isAll, hospCode=="0")
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
        public ApiResponse Excel(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            bool p2 = form.Get("p2") == "Y";

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0133Repository repo = new AA0133Repository(DBWork);
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();
                    string hospCode = repo.GetHospCode();
                    result = repo.GetExcel(p0, p1, p2, hospCode=="0");
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
    }
}