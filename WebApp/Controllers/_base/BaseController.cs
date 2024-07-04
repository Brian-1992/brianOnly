using System.Web.Mvc;
using System.Web.Security;
using JCLib.Mvc;

namespace SiteBase
{
    [Authorize]
    public class BaseController : Controller, IWebDataProvider
    {
        public string UserInfo
        {
            get { return ((FormsIdentity)User.Identity).Ticket.UserData; }
        }

        public string ProcUser
        {
            get { return User.Identity.Name; }
        }

        public string ProcIP
        {
            get { return Request.UserHostAddress; }
        }

        public string PageIndex
        {
            get { return Request.Form["page"]; }
        }

        public string PageSize
        {
            get { return Request.Form["limit"]; }
        }

        public string Sort
        {
            get { return Request.Form["sort"]; }
        }
    }
}
