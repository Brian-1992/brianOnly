using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            MvcHandler.DisableMvcResponseHeader = true;

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            this.Response.Headers["X-Content-Type-Options"] = "nosniff";
            var application = sender as HttpApplication;
            Response.Cache.SetCacheability(HttpCacheability.Private);  // HTTP 1.1.
            Response.Cache.AppendCacheExtension("no-store, must-revalidate");
            Response.Cache.SetNoStore();
            Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
            Response.AppendHeader("Expires", "0"); // Proxies.

            //// filter risk request
            //if (false == ValidateQueryString()
            //    || false == ValidateFormParameters()
            //)
            //{
            //    Response.StatusCode = 400;
            //    application.CompleteRequest();
            //}
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            HttpResponse response = HttpContext.Current.Response;
            response.Headers.Remove("X-AspNet-Version");
            response.Headers.Remove("Server");
        }

        private string[] EscapeUrlParameters = { "_dc" };

        /**
         * Validate query string
         */
        protected bool ValidateQueryString()
        {
            foreach (string key in Request.QueryString)
            {
                if (EscapeUrlParameters.Contains(key))
                {
                    continue;
                }

                string value = HttpUtility.UrlDecode(Request.QueryString[key]);

                if (false == CheckIntegerOverflow(value))
                {
                    return false;
                }

                //if (false == CheckSQLInjection(value))
                //{
                //    return false;
                //}
            }

            return true;
        }

        protected bool CheckIntegerOverflow(string value)
        {
            // 檢查字串是否為 [0-9] 的文字
            string pattern = @"^\d+$";
            Match match = Regex.Match(value, pattern);
            if (match.Success)
            {
                // 如果，嘗試 使用 32bit 轉換型別錯誤，那有可能低於或是高於 int.MinValue or int.MaxValue
                if (!int.TryParse(value, out int number))
                {
                    return false;
                }
            }

            return true;
        }

        // private string[] EscapeFormParameters = { "p0" };

        private string[] BlackListFormParameters = { "page", "start", "limit", "sort", "property", "direction" };
        /**
         * Validate form parameters
         */
        protected bool ValidateFormParameters()
        {
            HttpRequest request = base.Context.Request;

            foreach (string key in Request.Form.AllKeys)
            {
                if (BlackListFormParameters.Contains(key))
                {
                    string value = HttpUtility.UrlDecode(request.Form[key]);

                    if (false == CheckIntegerOverflow(value))
                    {
                        return false;
                    }
                }

                //if (false == CheckSQLInjection(value))
                //{
                //    return false;
                //}
            }

            return true;
        }

        protected bool CheckSQLInjection(string value)
        {
            string pattern = @"(\s*([\b\'\n\r\t\%_\\]*\s*(((select\s*.+\s*from\s*.+)|(insert\s*.+\s*into\s*.+)|(update\s*.+\s*set\s*.+)|(delete\s*.+\s*from\s*.+)|(drop\s*.+)|(truncate\s*.+)|(alter\s*.+)|(exec\s*.+)|(\s*(\|\||all|any|not|and|between|in|like|or|some|contains|containsall|containskey)\s*.+[\=\>\<=\!\~]+.+)|(let\s+.+[\=]\s*.*)|(begin\s*.*\s*end)|(\s*[\/\*]+\s*.*\s*[\*\/]+)|(\s*(\-\-)\s*.*\s+)|(\s*(contains|containsall|containskey)\s+.*)))(\s*[\;]\s*)*)+)";
            Match match = Regex.Match(value, pattern);

            if (match.Success)
            {
                return false;
            }

            return true;
        }
    }
}
