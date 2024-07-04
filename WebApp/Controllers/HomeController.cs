using JCLib.DB;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using WebApp.Repository.UR;

namespace SiteBase.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {

        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Index2()
        {
            DisableCache();

            IEnumerable<NestMenuView> nestMenuView = null;
            using (WorkSession session = new WorkSession(this))
            {
                var DBWork = session.UnitOfWork;
                UR_MENURepository repo = new UR_MENURepository(DBWork);
                nestMenuView = repo.GetMenuIndex2(null, null, User.Identity.Name);
                ViewBag.AuthType = DBWork.UserInfo.AuthType;

                // 原測試機是由登入頁面切換,現改為取envSettings.config的DB_CONN_TYPE做判斷
                // ViewBag.DbConnType = Session["DbConnType"];
                if (JCLib.Util.GetEnvSetting("DB_CONN_TYPE") == "TEST")
                    ViewBag.DbConnType = "TEST";
                else
                    ViewBag.DbConnType = "OFFICIAL";
                ViewBag.UNA = DBWork.UserInfo.UserName;
            }
            return View(nestMenuView);
        }

        /*
        public ActionResult Dashboard(FormCollection form)
        {
            var r = Shared.Process(this.ws, "TraBULLETINGet", form);

            List<Bulletin> bulletin = new List<Bulletin>();

            foreach (DataRow dr in r.ds.Tables["T1"].Rows)
            {
                bulletin.Add(new Bulletin { TITLE = dr["TITLE"].ToString(), DES = dr["DES"].ToString(), CREATE_DATE = dr["CREATE_DATE"].ToString() });
            }
            return View(bulletin);
        }*/

        [AllowAnonymous]
        public ActionResult GenericErrorPage()
        {
            return View();
        }


        private void DisableCache()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.SetExpires(System.DateTime.MinValue);
            Response.Cache.SetNoServerCaching();
            Response.Cache.SetNoStore();
        }

    }

    public class Bulletin
    {
        public string TITLE { get; set; }
        public string DES { get; set; }
        public string CREATE_DATE { get; set; }
    }
}
