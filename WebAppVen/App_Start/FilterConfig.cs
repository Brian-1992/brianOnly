using System.Web;
using System.Web.Mvc;

using WebAppVen.Filters;

namespace WebAppVen
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new AntiForgeryAttribute());
            filters.Add(new CustomAuthenticationFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
