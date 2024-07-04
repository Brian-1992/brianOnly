using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;
using System.Web.Security;

namespace WebAppVen.Filters
{
    public class CustomAuthenticationFilter : IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            //MVC Request
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true)
                || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
            {
                return;
            }

            if (filterContext.Principal.Identity.IsAuthenticated && filterContext.Principal.Identity is FormsIdentity)
            {
                var identity = (FormsIdentity)filterContext.Principal.Identity;
                var ticket = identity.Ticket;

                if (!string.IsNullOrEmpty(ticket.UserData))
                {
                    //var roles = ticket.UserData.Split(',');
                    //filterContext.Principal = new GenericPrincipal(identity,roles);
                    /*
                    var user = _db.Users.FirstOrDefault(u => u.Id.ToString() == ticket.UserData);
                    if (user != null)
                    {
                        var roles = user.Roles.Select(r => r.Name).ToArray();
                        filterContext.Principal = new GenericPrincipal(identity, roles);

                    }
                    */
                }
            }
            else
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (filterContext.Result == null || filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                {"controller","Account"},
                {"action", "Login"},
                {"returnUrl", filterContext.HttpContext.Request.RawUrl }
            });
            }
            //or do something , add challenge to response 

        }
    }
}