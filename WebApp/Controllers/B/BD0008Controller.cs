using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.B;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WebApp.Controllers.B
{
    public class BD0008Controller : SiteBase.BaseApiController
    {
        [HttpPost]
        public ApiResponse All(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");

            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0008Repository(DBWork);
                    if (!repo.CheckMasterdExists(p0))
                    {
                        DBWork.BeginTransaction();
                        repo.InsertRecYM(p0, p1);
                        DBWork.Commit();

                    }
                    session.Result.etts = repo.GetAll(p0, p1, p2,  page, limit, sorters);
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }



        [HttpPost]
        public ApiResponse Update(PH_EQPD ph_eqpd)
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    BD0008Repository repo = new BD0008Repository(DBWork);

                    //PH_EQPD ph_eqpd = new PH_EQPD();
                    //ph_eqpd.RECYM = form.Get("RECYM");
                    //ph_eqpd.WH_NO = form.Get("WH_NO");
                    //ph_eqpd.MMCODE = form.Get("MMCODE");
                    //ph_eqpd.M_PURUN = form.Get("M_PURUN");
                    //ph_eqpd.M_CONTPRICE = float.Parse( form.Get("M_CONTPRICE"));
                    //ph_eqpd.ADVISEQTY = float.Parse(form.Get("ADVISEQTY"));
                    //ph_eqpd.STKQTY = int.Parse(form.Get("STKQTY"));
                    //ph_eqpd.ESTQTY = float.Parse(form.Get("ESTQTY"));
                    //ph_eqpd.CONTRACNO = form.Get("CONTRACNO");
                    //ph_eqpd.AMOUNT = float.Parse(form.Get("AMOUNT"));

                    ph_eqpd.CREATE_USER= User.Identity.Name;
                    ph_eqpd.UPDATE_USER = User.Identity.Name;
                    ph_eqpd.UPDATE_IP = DBWork.ProcIP;

                    session.Result.afrs = repo.Update(ph_eqpd);
                    //session.Result.etts = repo.Get(ph_eqpd);

                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
                return session.Result;
            }
        }



        public ApiResponse GetWhnoCombo()
        {
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0008Repository(DBWork);
                    session.Result.etts = repo.GetWhnoCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }
        public ApiResponse GetSum(string p0, string p1, string p2)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0008Repository(DBWork);
                    session.Result.etts = repo.GetSum(p0, p1, p2);
                    //session.Result.etts = repo.GetReportSum(p0, p1, p2);
                    
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse GetCount(string wh_no, string recym,string contracno)
        {

            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new BD0008Repository(DBWork);
                    session.Result.etts = repo.GetCount(wh_no, recym, contracno);
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
