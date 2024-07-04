using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.G;
using WebApp.Models;
using System;
using System.Data;

namespace WebApp.Controllers.G
{
    public class GA0005Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("P0"); // 訂單編號
            var p1 = form.Get("P1"); // 訂購日期(起)
            var p2 = form.Get("P2"); // 訂購日期(迄)
            var p3 = form.Get("P3"); // 藥品名稱
            var p4 = form.Get("P4"); // 藥商種類
            var p5 = form.Get("P5"); // 訂單狀態
            var p6 = form.Get("P6"); // 藥品種類

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0005Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2,  p3, p4, p5, p6);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }// GET api/<controller>

        [HttpPost]
        public ApiResponse MasterAll(FormDataCollection form)
        {
            var p0 = form.Get("P0"); // 訂單編號
            var p1 = form.Get("P1"); // 訂購日期(起)
            var p2 = form.Get("P2"); // 訂購日期(迄)
            var p3 = form.Get("P3"); // 訂單狀態
            var p4 = form.Get("P4"); // 藥品種類

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0005Repository(DBWork);
                    session.Result.etts = repo.GetMasterAll(p0, p1, p2, p3, p4);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse DetailAll(FormDataCollection form)
        {
            var p0 = form.Get("P0"); // 訂單編號
            var p1 = form.Get("P1"); // 藥品名稱
            var p2 = form.Get("P2"); // 藥商種類

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0005Repository(DBWork);
                    session.Result.etts = repo.GetDetailAll(p0, p1, p2);

                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetNAMECombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0005Repository(DBWork);
                    session.Result.etts = repo.GetNAMECombo();
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
            var p0 = form.Get("P0"); // 訂單編號
            var p1 = form.Get("P1"); // 藥品種類
            string tc_type = "";
            if (p1 == "1")
                tc_type = "科學中藥";
            else if (p1 == "2")
                tc_type = "飲片";

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    var repo = new GA0005Repository(DBWork);

                    using (var dataSet = new DataSet())
                    {
                        foreach (string agen_namec in repo.GetAGEN_NAME(p0))
                        {
                            var dataTable = repo.GetExcel(p0, agen_namec);
                            dataTable.TableName = p0 + agen_namec + tc_type;
                            dataSet.Tables.Add(dataTable);
                        }
                        JCLib.Excel.ExportZip("中藥訂購單.zip", dataSet);
                    }
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