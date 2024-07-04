using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;
using System.Web;
using NPOI.XSSF.UserModel;

namespace WebApp.Controllers.F
{
    public class FA0061Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0") == null ? string.Empty : form.Get("p0");    //查詢年月
            var p1 = form.Get("p1") == null ? string.Empty : form.Get("p1");    //合約期間累計結報金額 >= ?? 萬
            var p2 = form.Get("p2") == null ? string.Empty : form.Get("p2");    //核取方塊 合約期間累計結報金額>=採購上限金額
            var p3 = form.Get("p3") == null ? string.Empty : form.Get("p3");    //核取方塊 合約到期前 ?? 月
            var p4 = form.Get("p4") == null ? string.Empty : form.Get("p4");    //合約到期前 ?? 月

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {   
                    var repo = new FA0061Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4);
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
            var p0 = form.Get("p0") == null ? string.Empty : form.Get("p0");    //查詢年月
            var p1 = form.Get("p1") == null ? string.Empty : form.Get("p1");    //合約期間累計結報金額 >= ?? 萬
            var p2 = form.Get("p2") == null ? string.Empty : form.Get("p2");    //核取方塊 合約期間累計結報金額>=採購上限金額
            var p3 = form.Get("p3") == null ? string.Empty : form.Get("p3");    //核取方塊 合約到期前 ?? 月
            var p4 = form.Get("p4") == null ? string.Empty : form.Get("p4");    //合約到期前 ?? 月

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();

                    var repo = new FA0061Repository(DBWork);
                    result = repo.GetExcel (p0, p1, p2, p3, p4);
                    dtItems.Merge(result);                 

                    JCLib.Excel.Export(form.Get("FN"), dtItems);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse Excel_Detail(FormDataCollection form)
        {
            var p0 = form.Get("p0") == null ? string.Empty : form.Get("p0");    //查詢年月


            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();

                    var repo = new FA0061Repository(DBWork);
                    result = repo.GetExcel_Detail(p0);
                    dtItems.Merge(result);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }


        [HttpPost]
        public ApiResponse MMCODE_Detail(FormDataCollection form)
        {
            var p0 = form.Get("p0") == null ? string.Empty : form.Get("p0");    //查詢年月
            var mmcode = form.Get("mmcode"); //院內碼

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new FA0061Repository(DBWork);
                    session.Result.etts = repo.GetMMCODE_Detail(p0,mmcode);
                }
                catch
                {
                    throw;
                }

                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse MmcodeExcel(FormDataCollection form)
        {
            var p0 = form.Get("p0") == null ? string.Empty : form.Get("p0");    //查詢年月
            var mmcode = form.Get("mmcode"); //院內碼

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    DataTable result = new DataTable();
                    DataTable dtItems = new DataTable();

                    var repo = new FA0061Repository(DBWork);
                    result = repo.GetMmcodeExcel(p0,mmcode);
                    dtItems.Merge(result);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);
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