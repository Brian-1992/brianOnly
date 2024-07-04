using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.UR;
using WebApp.Models;

namespace WebApp.Controllers.UR
{
    public class UR1026Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse QueryM(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 錯誤號碼
            var p1 = form.Get("p1"); // 姓名
            var p2 = form.Get("p2"); // 程式代碼
            var p3 = form.Get("p3"); // 發生時間(起)
            var p4 = form.Get("p4"); // 發生時間(迄)
            var p5 = form.Get("p5"); // 函式代碼

            using (WorkSession session = new WorkSession(this))
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    UR_FUNC_LOG_MRepository repo = new UR_FUNC_LOG_MRepository(DBWork);
                    session.Result.etts = repo.Query(p0, p1, p2, p3, p4, p5);
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
                    UR_FUNC_LOG_DRepository repo = new UR_FUNC_LOG_DRepository(DBWork);
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
            var p5 = form.Get("p5");

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    using (var dataSet1 = new System.Data.DataSet())
                    {
                        UR_FUNC_LOG_MRepository repo1 = new UR_FUNC_LOG_MRepository(DBWork);
                        var dataTable1 = repo1.GetExcelM(p0, p1, p2, p3, p4, p5);
                        dataTable1.TableName = "記錄項目";
                        dataSet1.Tables.Add(dataTable1);

                        UR_FUNC_LOG_DRepository repo2 = new UR_FUNC_LOG_DRepository(DBWork);
                        var dataTable2 = repo2.GetExcelD2(p0, p1, p2, p3, p4, p5);
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
                    UR_FUNC_LOG_DRepository repo = new UR_FUNC_LOG_DRepository(DBWork);
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
