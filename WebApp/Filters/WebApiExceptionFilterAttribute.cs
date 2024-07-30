using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Filters;
using System.Net.Http.Formatting;
using WebApp.Models;
using WebApp.Repository.UR;
using JCLib.DB;

namespace WebApp.Filters
{
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            string msg = context.Exception.GetBaseException().Message;
            string st = context.Exception.StackTrace;
            string ctrl = context.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerType.Name;
            string act = context.ActionContext.ActionDescriptor.ActionName;
            string tuser = context.ActionContext.RequestContext.Principal.Identity.Name;
            string ip = "";

            
            for (int i=0; i<context.Request.Properties.Count; i++)
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
                    catch (IndexOutOfRangeException e) {
                        ip = "";
                    }
                    break;
                }
            }
            string idno = LogUrErrM(msg, st, ctrl, act, tuser, ip);

            ApiResponse apiResponse = new ApiResponse();
            apiResponse.msg = "Internal Server Error " + "(Err No. " + idno + ")";
            apiResponse.success = false;

            List<UR_ERR_D> urErrDList = new List<UR_ERR_D>();
            string requestContent = context.Request.Content.ReadAsStringAsync().Result;
            if ("" != requestContent)
            {
                UR_ERR_D ur_err_d = new UR_ERR_D();
                ur_err_d.IDNO = idno;
                ur_err_d.PN = "Request.Content";
                ur_err_d.PV = requestContent;
                urErrDList.Add(ur_err_d);
            }

            int actionArgumentsCount = context.ActionContext.ActionArguments.Count;
            if (actionArgumentsCount > 0)
            {
                for(int i=0; i< actionArgumentsCount; i++)
                {
                    KeyValuePair<string, object> actionArgument = context.ActionContext.ActionArguments.ElementAt(i);
                    List<KeyValuePair<string, string>> kvPairList = null;
                    string actionArgumentkey = actionArgument.Key;
                    object actionArgumentObj = actionArgument.Value;
                    string typeName = actionArgumentObj.GetType().Name;
                    string typeFullName = actionArgumentObj.GetType().FullName;
                    Type type = actionArgumentObj.GetType();
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
                                UR_ERR_D ur_err_d = new UR_ERR_D();
                                ur_err_d.IDNO = idno;
                                ur_err_d.PN = key;
                                ur_err_d.PV = value;
                                urErrDList.Add(ur_err_d);
                            }
                        }
                    }
                    else if (typeFullName.StartsWith("WebApp.Models"))    // WebApp.Models.xxx
                    {
                        PropertyInfo[] propertyInfos = type.GetProperties();
                        if (propertyInfos != null && propertyInfos.Length > 0)
                        {
                            for (int j = 0; j < propertyInfos.Length; j++)
                            {
                                UR_ERR_D ur_err_d = new UR_ERR_D();
                                ur_err_d.IDNO = idno;
                                // ur_err_d.PN = "(" + type.Name + ")" + actionArgumentkey + "." + propertyInfos[j].Name;
                                ur_err_d.PN = actionArgumentkey + "." + propertyInfos[j].Name;
                                ur_err_d.PV = "" + propertyInfos[j].GetValue(actionArgumentObj);
                                urErrDList.Add(ur_err_d);
                            }
                        }
                    }
                    else
                    {
                        UR_ERR_D ur_err_d = new UR_ERR_D();
                        ur_err_d.IDNO = idno;
                        ur_err_d.PN = actionArgumentkey;
                        ur_err_d.PV = actionArgumentObj.ToString();
                        urErrDList.Add(ur_err_d);
                    }
                }
            }

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repoD = new UR_ERR_DRepository(DBWork);

                    for (int i=0; i < urErrDList.Count; i++)
                    {
                        UR_ERR_D ur_err_d = urErrDList.ElementAt(i);
                        if (ur_err_d.PV.Length > 4000)
                        {
                            ur_err_d.PV = ur_err_d.PV.Substring(0, 3500);
                        }
                        repoD.Create(ur_err_d);
                    }
                }
                catch (TimeoutException e)
                {
                    idno = "";
                }
                catch (IndexOutOfRangeException e)
                {
                    idno = "";
                }
            }

            // context.Response = context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg);
            context.Response = context.Request.CreateResponse<ApiResponse>(apiResponse);
        }


        private string LogUrErrM(string msg, string st, string ctrl, string act, string tuser, string ip)
        {
            string idno = "";
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repoM = new UR_ERR_MRepository(DBWork);
                    UR_ERR_M ur_err_m = new UR_ERR_M();
                    ur_err_m.MSG = msg;
                    if (st.Length > 4000)
                    {
                        st = st.Substring(0, 3500);
                    }
                    ur_err_m.ST = st;
                    ur_err_m.CTRL = ctrl;
                    ur_err_m.ACT = act;
                    ur_err_m.TUSER = tuser;
                    ur_err_m.IP = ip;
                    idno = repoM.GetNextIdno();
                    ur_err_m.IDNO = idno;
                    repoM.Create(ur_err_m);
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
    }
}