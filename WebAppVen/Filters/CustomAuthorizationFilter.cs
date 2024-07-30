using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace WebAppVen.Filters
{
    public class CustomAuthorizationFilter : AuthorizeAttribute
    {
        //private readonly Database _db = new Database();
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Request.IsLocal)
            {
                return true;
            }
            var identity = httpContext.User.Identity as FormsIdentity;
            if (identity?.Ticket != null)
            {
                //var userRoles = identity.Ticket.UserData.Split(',');
                //var userRoles = _db.Users.Find(identity.Ticket.UserData)?.Roles.Select(r => r.Name);

                //if (userRoles?.Intersect(Roles.Split(',')).Any() ?? false)//交集->具有某一角色->有權限
                //{
                    return true;
                //}
                //or get current action's roles from db and check the authorization
            }


            return false;
        }
    }
}