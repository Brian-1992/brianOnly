using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Net;
using System.Net.Http;

namespace WebAppVen.Filters
{
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            string msg = context.Exception.GetBaseException().Message;

            context.Response = context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg);
        }
    }
}