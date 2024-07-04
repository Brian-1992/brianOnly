using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;

namespace WebApp.Controllers.UR
{
    public class UR1021Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse QueryM(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 錯誤號碼
            var p1 = form.Get("p1"); // 姓名
            var p2 = form.Get("p2"); // 程式代碼
            var p3 = form.Get("p3"); // 發生時間(起)
            var p4 = form.Get("p4"); // 發生時間(迄)

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_ERR_MRepository repo = new UR_ERR_MRepository(DBWork);
                    session.Result.etts = repo.Query(p0, p1, p2, p3, p4);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        
        [HttpPost]
        public ApiResponse QueryD(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_ERR_DRepository repo = new UR_ERR_DRepository(DBWork);
                    session.Result.etts = repo.Query(p0);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse ExcelM(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    using (var dataSet1 = new System.Data.DataSet())
                    {
                        UR_ERR_MRepository repo1 = new UR_ERR_MRepository(DBWork);
                        var dataTable1 = repo1.GetExcelM(p0, p1, p2, p3, p4);
                        dataTable1.TableName = "錯誤項目";
                        dataSet1.Tables.Add(dataTable1);

                        UR_ERR_DRepository repo2 = new UR_ERR_DRepository(DBWork);
                        var dataTable2 = repo2.GetExcelD2(p0, p1, p2, p3, p4);
                        dataTable2.TableName = "參數明細";
                        dataSet1.Tables.Add(dataTable2);

                        JCLib.Excel.Export(form.Get("FN"), dataSet1);
                    }
                        
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        // 取單筆錯誤記錄的參數明細
        [HttpPost]
        public ApiResponse ExcelD(FormDataCollection form)
        {
            var p0 = form.Get("p0");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_ERR_DRepository repo = new UR_ERR_DRepository(DBWork);
                    JCLib.Excel.Export(form.Get("FN"), repo.GetExcelD(p0));
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
