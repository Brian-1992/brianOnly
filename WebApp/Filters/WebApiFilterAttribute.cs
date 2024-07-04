using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebApp.Models;
using WebApp.Repository.UR;

namespace WebApp.Filters
{
    public class WebApiActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext context)
       {
            if (JCLib.Util.GetEnvSetting("ACSRF") == "Y")
                ValidateRequestCSRFToken(context.Request);

            string ctrl = context.ActionDescriptor.ControllerDescriptor.ControllerType.Name;
            string ctrlFullName = context.ActionDescriptor.ControllerDescriptor.ControllerType.FullName;
            string act = context.ActionDescriptor.ActionName;
            string tuser = context.RequestContext.Principal.Identity.Name;
            string ip = "";

            for (int i = 0; i < context.Request.Properties.Count; i++)
            {
                KeyValuePair<string, object> requestProp = context.Request.Properties.ElementAt(i);
                if ("MS_HttpContext" == requestProp.Key)
                {
                    try
                    {
                        System.Web.HttpContextWrapper httpContextWrapper = (System.Web.HttpContextWrapper)requestProp.Value;
                        string clientIp = httpContextWrapper.Request.Headers["X-Forwarded-For"];
                        // X-Forwarded-For, Proxy-Client-IP, WL-Proxy-Client-IP, HTTP_CLIENT_IP, HTTP_X_FORWARDED_FOR
                        if (clientIp == null || clientIp.Length == 0 || clientIp.ToLower() == "unknown")
                        {
                            // clientIp = httpContextWrapper.Request.ServerVariables["REMOTE_ADDR"];
                            clientIp = httpContextWrapper.Request.UserHostAddress;
                        }
                        ip = clientIp;
                    }
                    catch (TimeoutException e)
                    {
                        ip = "";
                    }
                    break;
                }
            }
            string idno = LogUrFuncM(ctrl, act, tuser, ip);

            List<UR_FUNC_LOG_D> urFuncDList = new List<UR_FUNC_LOG_D>();
            int actionArgumentsCount = context.ActionArguments.Count;
            if (actionArgumentsCount > 0)
            {
                for (int i = 0; i < actionArgumentsCount; i++)
                {
                    KeyValuePair<string, object> actionArgument = context.ActionArguments.ElementAt(i);
                    List<KeyValuePair<string, string>> kvPairList = null;
                    string actionArgumentkey = actionArgument.Key;
                    object actionArgumentObj = actionArgument.Value;
                    string typeName = actionArgumentObj != null ? actionArgumentObj.GetType().Name : string.Empty;
                    string typeFullName = actionArgumentObj != null ? actionArgumentObj.GetType().FullName : string.Empty;
                    Type type = actionArgumentObj != null ? actionArgumentObj.GetType() : null;
                    
                    if (typeName == "FormDataCollection")    // System.Net.Http.Formatting.FormDataCollection
                    {
                        FormDataCollection formDataCollection = (FormDataCollection)actionArgument.Value;
                        kvPairList = formDataCollection.ToList();
                        if (kvPairList != null && kvPairList.Count > 0)
                        {
                            for (int j = 0; j < kvPairList.Count; j++)
                            {
                                KeyValuePair<string, string> kvp = kvPairList.ElementAt(j);
                                string key = actionArgumentkey + "." + kvp.Key;
                                string value = kvp.Value;
                                UR_FUNC_LOG_D ur_func_d = new UR_FUNC_LOG_D();
                                ur_func_d.IDNO = idno;
                                ur_func_d.PN = key;
                                ur_func_d.PV = value;
                                urFuncDList.Add(ur_func_d);
                            }
                        }
                    }
                    else if (typeFullName.StartsWith("WebApp.Models"))    // WebApp.Models.xxx
                    {
                        if(type != null) {
                            PropertyInfo[] propertyInfos = type.GetProperties();
                            if (propertyInfos != null && propertyInfos.Length > 0)
                            {
                                for (int j = 0; j < propertyInfos.Length; j++)
                                {
                                    UR_FUNC_LOG_D ur_func_d = new UR_FUNC_LOG_D();
                                    ur_func_d.IDNO = idno;
                                    ur_func_d.PN = actionArgumentkey + "." + propertyInfos[j].Name;
                                    ur_func_d.PV = "" + propertyInfos[j].GetValue(actionArgumentObj);
                                    urFuncDList.Add(ur_func_d);
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        UR_FUNC_LOG_D ur_func_d = new UR_FUNC_LOG_D();
                        ur_func_d.IDNO = idno;
                        ur_func_d.PN = actionArgumentkey;
                        ur_func_d.PV = actionArgumentObj != null ? actionArgumentObj.ToString() : string.Empty;
                        urFuncDList.Add(ur_func_d);
                    }
                }
            }

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repoD = new UR_FUNC_LOG_DRepository(DBWork);

                    for (int i = 0; i < urFuncDList.Count; i++)
                    {
                        UR_FUNC_LOG_D ur_func_d = urFuncDList.ElementAt(i);
                        //長度真的超過4000 DB substr 會死掉
                        if (ur_func_d.PV.Length > 4000)
                        {
                            ur_func_d.PV = ur_func_d.PV.Substring(0, 4000);
                        }
                        repoD.Create(ur_func_d);
                    }
                }
                catch (Exception e)
                {
                    idno = "";
                }
            }

        }

        private string LogUrFuncM(string ctrl, string act, string tuser, string ip)
        {
            string idno = "";
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repoM = new UR_FUNC_LOG_MRepository(DBWork);
                    UR_FUNC_LOG_M ur_func_m = new UR_FUNC_LOG_M();
                    ur_func_m.CTRL = ctrl;
                    ur_func_m.ACT = act;
                    ur_func_m.TUSER = tuser;
                    ur_func_m.IP = ip;
                    idno = repoM.GetNextIdno();
                    ur_func_m.IDNO = idno;
                    repoM.Create(ur_func_m);
                    DBWork.Commit();
                }
                catch (Exception e)
                {
                    DBWork.Rollback();
                    // To-Do: Log(e.StackTrace);
                }
            }
            return idno;
        }
        /// <summary>
        /// 檢查 每次的請求是否有帶 csrf 
        /// </summary>
        /// <param name="request"></param>
        void ValidateRequestCSRFToken(HttpRequestMessage request)
        {
            string cookieToken = "";
            string formToken = "";

            IEnumerable<string> tokenHeaders;
            if (request.Headers.TryGetValues("X-CSRF-TOKEN", out tokenHeaders))
            {
                string[] tokens = tokenHeaders.First().Split(':');
                if (tokens.Length == 2)
                {
                    cookieToken = tokens[0].Trim();
                    formToken = tokens[1].Trim();
                }
            }
            AntiForgery.Validate(cookieToken, formToken);
        }

    }
}