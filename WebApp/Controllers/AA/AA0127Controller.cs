using System.Net.Http.Formatting;
using System.Web.Http;
using System.Net.Http;
using JCLib.DB;
using WebApp.Repository.AA;
using WebApp.Models;
using System.Diagnostics;


namespace WebApp.Controllers.AA
{
    public class AA0127Controller : SiteBase.BaseApiController
    {
        
        [HttpGet]
        public ApiResponse GetDeptCombo()
        {
            using (WorkSession session = new WorkSession())
            {
                UnitOfWork DBWork = session.UnitOfWork;
                try
                {
                    AA0127Repository repo = new AA0127Repository(DBWork);
                    session.Result.etts = repo.GetDeptCombo();
                }
                catch when (!Debugger.IsAttached)
                {

                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse GetAll(FormDataCollection form)
        {
            var p0 = form.Get("p0");
            var p1 = form.Get("p1");
            var p2 = form.Get("p2");
            var p3 = form.Get("p3");
           
            var page = int.Parse(form.Get("page"));
            var start = int.Parse(form.Get("start"));
            var limit = int.Parse(form.Get("limit"));
            var sorters = form.Get("sort");

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0127Repository(DBWork);
                    session.Result.etts = repo.GetAll(p0, p1, p2, p3, page, limit, sorters);
                }
                catch
                {
                    throw;
                }
                return session.Result;
            }
        }

        public ApiResponse UpdateEnd(FormDataCollection form)
        {
            string txtday = form.Get("txtday");
            string seq = form.Get("seq");
            string dept = form.Get("dept");
            string mmcode = form.Get("mmcode");
            string fbno = form.Get("fbno");
          
            using (WorkSession session = new WorkSession(this))
            {
                ;
                UnitOfWork DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    string UPUSER = User.Identity.Name;
                    string UIP = DBWork.ProcIP;
                    AA0127Repository repo = new AA0127Repository(DBWork);

                    // session.Result.afrs = repo.UpdateEnd(medocm);
                    session.Result.afrs = repo.UpdateEnd(txtday,seq,dept,mmcode,fbno, UPUSER, UIP);
                    SP_MODEL sp = repo.UpdIncapv( User.Identity.Name, DBWork.ProcIP);
                    if (sp.O_RETID == "N")
                    {
                        session.Result.afrs = 0;
                        session.Result.success = false;
                        session.Result.msg = sp.O_ERRMSG;
                        return session.Result;
                    }
                    DBWork.Commit();
                }
                catch when (!Debugger.IsAttached)
                {
                    DBWork.Rollback();
                    //throw;
                }
                return session.Result;
            }
        }

        [HttpPost]
        public ApiResponse QueryDocno(FormDataCollection form)
        {
            var p0 = form.Get("p0"); // 申請單號

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new AA0127Repository(DBWork);
                    session.Result.etts = repo.QueryDocno(p0);
                }
                catch when (!Debugger.IsAttached)
                {

                }
                return session.Result;
            }
        }
    }
}