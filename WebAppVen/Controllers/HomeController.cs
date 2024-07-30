using System.Web.Mvc;

namespace SiteBase.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            if (JCLib.Util.GetEnvSetting("DB_CONN_TYPE") == "TEST")
                ViewBag.DbConnType = "TEST";
            else
                ViewBag.DbConnType = "OFFICIAL";

            return View();
        }

        public ActionResult Mobile()
        {
            return View();
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

    }
    public class Bulletin
    {
        public string TITLE { get; set; }
        public string DES { get; set; }
        public string CREATE_DATE { get; set; }
    }
}
