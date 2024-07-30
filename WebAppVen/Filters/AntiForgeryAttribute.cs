using SiteBase.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebAppVen.Filters
{
    public class AntiForgeryAttribute : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext authorizationContext) {
            if (authorizationContext.RequestContext.HttpContext.Request.HttpMethod != "POST") {
                return;
            }
            if (authorizationContext.ActionDescriptor.GetCustomAttributes(typeof(NoAntiForgeryCheckAttribute), true).Length > 0) {
                return;
            }
            new ValidateAntiForgeryTokenAttribute().OnAuthorization(authorizationContext);
        }
    }
}