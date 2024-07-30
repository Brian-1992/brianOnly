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
    public class FA0038Controller : ApiController
    {


        [HttpPost]
        public ApiResponse GetKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0038Repository repo = new FA0038Repository(DBWork);
                    session.Result.etts = repo.GetKindCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        [HttpPost]
        public ApiResponse ExcelA(FormDataCollection form)
        {
            var p1 = form.Get("p1").Trim();
            var p2 = form.Get("p2").Trim();

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0038Repository repo = new FA0038Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcelA(p1, p2));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ExcelB(FormDataCollection form)
        {
            var p1 = form.Get("p1").Trim();
            var p2 = form.Get("p2").Trim();

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0038Repository repo = new FA0038Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcelB(p1, p2));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ExcelC(FormDataCollection form)
        {
            var p1 = form.Get("p1").Trim();
            var p2 = form.Get("p2").Trim();

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0038Repository repo = new FA0038Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcelC(p1, p2));
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ExcelD(FormDataCollection form)
        {
            var p1 = form.Get("p1").Trim();
            var p2 = form.Get("p2").Trim();

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0038Repository repo = new FA0038Repository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcelD(p1, p2));
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