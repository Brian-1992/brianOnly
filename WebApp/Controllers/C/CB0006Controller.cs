using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.C;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Data;

namespace WebApp.Controllers.C
{
    public class CB0006Controller : SiteBase.BaseApiController
    {
        // 查詢
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
            var p4 = form.Get("p4");
            var p5 = form.Get("p5");
            var p6 = form.Get("p6");
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0006Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, p4,p5,p6, page, limit, sorters, User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public void GetExcel(FormDataCollection form)
        {

            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    CB0006Repository repo = new CB0006Repository(DBWork);

                    DataTable dtItems = new DataTable();
                    dtItems.Columns.Add("項次", typeof(int));
                    dtItems.Columns["項次"].AutoIncrement = true;
                    dtItems.Columns["項次"].AutoIncrementSeed = 1;
                    dtItems.Columns["項次"].AutoIncrementStep = 1;

                    DataTable result = null;

                    result = repo.GetExcel(form.Get("TS"));


                    dtItems.Merge(result);

                    JCLib.Excel.Export(form.Get("FN"), dtItems);

                    //JCLib.Excel.Export(form.Get("FN"), repo.GetExcel(p0, p1, p2, p3, p4, p5));
                    //session.Result.etts = repo.GetExcel(p0, p1, p2, p3, p4,p5);
                }
                catch
                {
                    throw;
                }
            }
        }

        [HttpPost]
        public ApiResponse SetBcItmanagers(BC_ITMANAGER bc_itmanager) {


            IEnumerable<BC_ITMANAGER> bcItmanagers = JsonConvert.DeserializeObject<IEnumerable<BC_ITMANAGER>>(bc_itmanager.ITEM_STRING);

            var inserts = bcItmanagers.Where(x => x.MANAGERID != "").Where(x => x.OLD_MANAGERID == "").ToList<BC_ITMANAGER>();
            var updates = bcItmanagers.Where(x => x.MANAGERID != "").Where(x => x.OLD_MANAGERID != "").ToList<BC_ITMANAGER>();
            var deletes = bcItmanagers.Where(x => x.MANAGERID == "").Where(x => x.OLD_MANAGERID != "").ToList<BC_ITMANAGER>();


            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new CB0006Repository(DBWork);

                    //Create
                    foreach (BC_ITMANAGER bc_manager in inserts)
                    {
                        bc_manager.CREATE_USER = User.Identity.Name;
                        bc_manager.UPDATE_USER = User.Identity.Name;
                        bc_manager.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Create(bc_manager);
                    }

                    //Update
                    foreach (BC_ITMANAGER bc_manager in updates)
                    {
                        bc_manager.UPDATE_USER = User.Identity.Name;
                        bc_manager.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Update(bc_manager);
                    }

                    //Delete
                    foreach (BC_ITMANAGER bc_manager in deletes)
                    {
                        bc_manager.UPDATE_USER = User.Identity.Name;
                        bc_manager.UPDATE_IP = DBWork.ProcIP;
                        session.Result.afrs = repo.Delete(bc_manager);
                    }

                    DBWork.Commit();
                }
                catch(Exception e)
                {
                    DBWork.Rollback();
                    return session.Result; //throw;
                }

                return session.Result;

            }
        }

        public ApiResponse GetUserInid() {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0006Repository(DBWork);
                    session.Result.etts = repo.GetUserInid(User.Identity.Name);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetMatclassCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0006Repository(DBWork);
                    session.Result.etts = repo.GetMatclassCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetManageridCombo(BC_ITMANAGER bc_itmanager)
        {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0006Repository(DBWork);
                    session.Result.etts = repo.GetManageridCombo(bc_itmanager.WH_NO);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetWhnoCombo() {
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new CB0006Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo(User.Identity.Name);
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