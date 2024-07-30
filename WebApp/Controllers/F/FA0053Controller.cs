using System.Net.Http.Formatting;
using System.Web.Http;
using JCLib.DB;
using WebApp.Repository.F;
using WebApp.Models;

namespace WebApp.Controllers.F
{
    public class FA0053Controller : ApiController
    {

        
        [HttpPost]
        public ApiResponse GetKindCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0053Repository repo = new FA0053Repository(DBWork);
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
        public ApiResponse GetWhmastCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0053Repository repo = new FA0053Repository(DBWork);
                    session.Result.etts = repo.GetWhmastCombo();
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetMmCodeCombo(FormDataCollection form)
        {
            var p0 = form.Get("p0"); //動態mmcode
            var p1 = form.Get("p1"); //動態mmcode
            var v_mat = "01";
            if (p1 == "0")
            {
                v_mat = "01";
            }
            else if (p1 == "1")
            {
                v_mat = "02";
            }
            else
            {
                v_mat = "";
            }
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    FA0053Repository repo = new FA0053Repository(DBWork);
                    session.Result.etts = repo.GetMmCodeCombo(p0, v_mat, page, limit, "");
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