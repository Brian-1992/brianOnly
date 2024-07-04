using System.Web.Mvc;
using WebApp.Filters;

namespace WebApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomAuthenticationFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
