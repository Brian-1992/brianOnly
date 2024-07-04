using System.Web;
using System.Web.Http;
using System.Web.Security;
using JCLib.Mvc;

namespace SiteBase
{
    [Authorize]
    //[WebApp.Filters.WebApiExceptionFilter]
    public class BaseApiController : ApiController, IWebDataProvider
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
            get { return ((HttpContextBase)Request.Properties["MS_HttpContext"]).Request.UserHostAddress; }
        }

        public string PageIndex
        {
            //ExtJS的Store預設會送出page參數，代表目前在第幾頁
            get { return ((HttpContextBase)Request.Properties["MS_HttpContext"]).Request.Form["page"]; }
        }

        public string PageSize
        {
            //ExtJS的Store預設會送出limit參數，代表每一頁的資料筆數，預設是25
            get { return ((HttpContextBase)Request.Properties["MS_HttpContext"]).Request.Form["limit"]; }
        }

        public string Sort
        {
            //ExtJS的Store預設會送出sort參數，包含資料排序欄位
            get { return ((HttpContextBase)Request.Properties["MS_HttpContext"]).Request.Form["sort"]; }
        }
    }
}
