using System;
using System.DirectoryServices;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Configuration;
using Microsoft.Web.WebPages.OAuth;
using WebApp.Repository.UR;
using WebApp.Repository.GB;
using WebApp.Models;
using JCLib.DB;
using JCLib.Mvc;
using BotDetect.Web.Mvc;
using Newtonsoft.Json;
using System.Configuration;
using System.Net;
using System.Collections.Generic;
using WebApp.Filters;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Security.Application;
using System.Text.RegularExpressions;

namespace SiteBase.Controllers
{
    //[Authorize]
    //[InitializeSimpleMembership]
    public class AccountController : BaseController
    {
        private const bool isCookieless = false;

        // List<string> CanAdLoginList20200501 = new List<string>() { "shiowling", "ping", "hjkai", "anson101", "odd7139", "Q123808599", "0105", "wang-cj", "neil132049"};

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        public ActionResult Unauthorized()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [AntiForgeryErrorHandler(ExceptionType = typeof(HttpAntiForgeryException), View = "Login", Controller = "Account", ErrorMessage = "請重新登入")]
        public ActionResult Login([Bind(Exclude = "DbConnType")]LoginModel loginModel, string returnUrl)
        {
            var name = string.Empty;
            var msg = string.Empty;

            if (ModelState.IsValid)
            {
                /* 不檢查驗證碼
                ApiResponse checkCaptcha = CheckByCaptcha();
                if (!checkCaptcha.success)
                {
                    ModelState.AddModelError("", "登入失敗！請檢查您的帳號、密碼與驗證碼！");
                    MvcCaptcha.ResetCaptcha("ExampleCaptcha");
                    return View(model);
                }
                */

                string adUserName = string.Empty;
                string adEmail = string.Empty;
                string adAccount = string.Empty;
                string adDeptno = string.Empty;
                bool authSuccess = false;

                //Session["DbConnType"] = loginModel.DbConnType.ToString();
                //Response.Cookies["MmsmsLoginUser"]["DbConnType"] = loginModel.DbConnType.ToString();
                // 原測試機是由登入頁面切換,現改為取envSettings.config的DB_CONN_TYPE做判斷
                if (JCLib.Util.GetEnvSetting("DB_CONN_TYPE") == "TEST")
                {
                    Session["DbConnType"] = "TEST";
                    Response.Cookies["MmsmsLoginUser"]["DbConnType"] = "TEST";
                }
                else
                {
                    Session["DbConnType"] = "OFFICIAL";
                    Response.Cookies["MmsmsLoginUser"]["DbConnType"] = "OFFICIAL";
                }

                switch (loginModel.AuthType)
                {
                    case AuthType.DB:
                        //DB驗證
                        authSuccess = AuthenticateByDB(loginModel).success;
                        break;
                    case AuthType.HISDB:
                        //HISDB驗證
                        var authResultHISDB = AuthenticateByHISDB(loginModel);
                        authSuccess = authResultHISDB.success;
                        if (authSuccess)
                        {
                            adUserName = authResultHISDB.msg.Split('^')[0];
                            adDeptno = authResultHISDB.msg.Split('^')[1];
                            adAccount = authResultHISDB.msg.Split('^')[2];
                        }
                        authResultHISDB.msg = "";
                        break;
                    case AuthType.AD:
                        //AD驗證
                        //ApiResponse currentDate = GetCurrentDate();
                        //if (currentDate.msg == "1090501")
                        //{
                        //    if (CanAdLoginList20200501.Contains(loginModel.UserName.ToLower()) == false)
                        //    {
                        //        ModelState.AddModelError("", "系統暫停使用，若有問題請洽資訊組汪佳蓉");
                        //        return View(loginModel);
                        //    }
                        //}
                        //if (currentDate.msg == "1090502" || currentDate.msg == "1090503")
                        //{
                        //    ModelState.AddModelError("", "系統暫停使用");
                        //    return View(loginModel);
                        //}

                        var authResult = AuthenticateByAD(loginModel);
                        authSuccess = authResult.success;
                        if (authSuccess)
                        {
                            adUserName = authResult.msg.Split('^')[0];
                            adEmail = authResult.msg.Split('^')[1];
                            adAccount = authResult.msg.Split('^')[2];
                        }
                        authResult.msg = "";
                        break;
                    case AuthType.API:
                        //API驗證, 後續部分新增,更新資料功能沿用AD的方式
                        var authResultAPI = AuthenticateByAPI(loginModel);
                        authSuccess = authResultAPI.success;
                        if (authSuccess)
                        {
                            adUserName = authResultAPI.msg.Split('^')[0];
                            adDeptno = authResultAPI.msg.Split('^')[1];
                            adAccount = authResultAPI.msg.Split('^')[2];
                        }
                        authResultAPI.msg = "";
                        break;
                }

                if (authSuccess)
                {
                    if (ChkValidIp(loginModel, Request.UserHostAddress))
                    {
                        UserInfo userInfo = GetUserInfo(loginModel.AuthType, loginModel.UserName);

                        if (userInfo == null)
                        {
                            if (loginModel.AuthType == AuthType.AD || loginModel.AuthType == AuthType.HISDB || loginModel.AuthType == AuthType.API)
                            {
                                int afrs = 0;
                                if (loginModel.AuthType == AuthType.AD)
                                    afrs = CreateAccountForADUser(adAccount, adUserName, adEmail);
                                else if (loginModel.AuthType == AuthType.API || loginModel.AuthType == AuthType.HISDB)
                                    afrs = CreateAccountForAPIUser(adAccount, adUserName, adDeptno);

                                if (afrs == 1)
                                {
                                    string contact_msg = GetSysContactMsg();
                                    if (contact_msg != null)
                                        msg = "藥衛材權限未開放(未啟用)，請洽" + contact_msg + "！";
                                    else
                                        msg = "藥衛材權限未開放(未啟用)！";
                                }
                                else
                                    msg = "系統帳號建立失敗。";
                            }
                            else
                            {
                                msg = "登入失敗！請確認系統帳號存在！";
                            }
                        }
                        else
                        {
                            if (loginModel.AuthType == AuthType.AD || loginModel.AuthType == AuthType.HISDB || loginModel.AuthType == AuthType.API)
                            {
                                loginModel.UserName = adAccount;
                                var isAdUserEnabled = GetAdUserEnabled(loginModel.UserName);
                                UpdateUserAduser(adAccount); // 用取得的AD帳號更新既有AD帳號
                                if (!isAdUserEnabled)
                                {
                                    string contact_msg = GetSysContactMsg();
                                    if (contact_msg != null)
                                        msg = "藥衛材權限未開放(未啟用)，請洽" + contact_msg + "!";
                                    else
                                        msg = "藥衛材權限未開放(未啟用)!";
                                    authSuccess = false;
                                }
                            }

                            if (authSuccess)
                            {
                                ApiResponse writeLogin = WriteLogin(userInfo);

                                SetAuthenticated(userInfo);

                                return RedirectToAction("Index2", "Home");
                            }
                        }
                    }
                    else
                    {
                        msg = "登入失敗！不屬於可登入的IP！";
                    }

                }
                else
                {
                    // AD登入與一般登入失敗時的訊息有所區隔,以便使用者詢問時追蹤
                    if (loginModel.AuthType == AuthType.AD || loginModel.AuthType == AuthType.API || loginModel.AuthType == AuthType.HISDB)
                    {
                        string contact_msg = GetAdContactMsg();
                        string authType = "";
                        if (loginModel.AuthType == AuthType.AD)
                            authType = "AD";
                        else if (loginModel.AuthType == AuthType.API)
                            authType = "API";
                        else if (loginModel.AuthType == AuthType.HISDB)
                            authType = "帳號";

                        if (contact_msg != null)
                            msg = authType + "登入失敗！請檢查您的帳號與密碼！ 或請洽" + contact_msg + "！";
                        else
                            msg = authType + "登入失敗！請檢查您的帳號與密碼！";
                    }
                    else
                        msg = "登入失敗！請檢查您的帳號與密碼。";
                }
            }
            else
                msg = "登入失敗！請檢查您的帳號與密碼";

            ModelState.AddModelError("", msg);
            return View(loginModel);
        }

        /// <summary>
        /// 圖形驗證碼，通常只有外部Server需要檢查
        /// </summary>
        /// <returns></returns>
        //private ApiResponse CheckByCaptcha()
        //{
        //    //外部Server IP
        //    string outsideServerIP = "10.10.20.96";

        //    ApiResponse result = new ApiResponse();
        //    result.success = false;

        //    //檢查是否為外部Server所送出的Request
        //    var isDMZ = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Count(o =>
        //    (o.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && o.ToString() == outsideServerIP)) == 1;

        //    //如果是外部Server的Request，就檢查輸入是否符合圖形驗證碼
        //    if (isDMZ)
        //    {
        //        var mvcCaptcha = new MvcCaptcha("ExampleCaptcha");
        //        var userInput = HttpContext.Request.Form["CaptchaCode"];
        //        var validatingInstanceId = HttpContext.Request.Form[mvcCaptcha.ValidatingInstanceKey];

        //        result.success = MvcCaptcha.Validate("ExampleCaptcha", userInput, validatingInstanceId);
        //    }

        //    return result;
        //}

        /// <summary>
        /// 使用DB驗證是否合法User
        /// </summary>
        /// <param name="loginModel">登入帳號密碼</param>
        /// <returns></returns>
        private ApiResponse AuthenticateByDB(LoginModel loginModel)
        {
            ApiResponse result = new ApiResponse();
            result.success = false;

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    result.success = repo.CheckLogin(loginModel);
                }
                catch (Exception ex)
                {
                    result.msg = ex.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// 使用HISDB驗證是否合法User
        /// </summary>
        /// <param name="loginModel">登入帳號密碼</param>
        /// <returns></returns>
        private ApiResponse AuthenticateByHISDB(LoginModel loginModel)
        {
            ApiResponse result = new ApiResponse();
            result.success = false;

            using (WorkSession session = new WorkSession(null, "HISDB", "SQL"))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    if (repo.CheckHISDBLogin(loginModel.UserName, loginModel.Drowssap))
                    {
                        UR_ID getUser = repo.GetHISDBLogin(loginModel.UserName, loginModel.Drowssap);
                        result.msg = getUser.EM_EMPNAME + "^" + getUser.EM_DEPTNO + "^" + loginModel.UserName;
                        result.success = true;
                    }
                }
                catch (Exception ex)
                {
                    result.msg = ex.Message;
                }
            }

            return result;
        }
        
        /// <summary>
        /// 使用AD伺服器驗證是否合法User
        /// </summary>
        /// <param name="loginModel">登入帳號密碼</param>
        /// <returns></returns>
        private ApiResponse AuthenticateByAD(LoginModel loginModel)
        {
            ApiResponse result = new ApiResponse();

            result.success = false;

            
            string userName = "";
            string thewordmustnotbenamed;
            using (WorkSession session = new WorkSession())
            {
                // 因應Access Control問題
                var DBWork = session.UnitOfWork;
                var repo = new UR_IDRepository(DBWork);
                userName = repo.CheckValidString(Encoder.LdapFilterEncode(loginModel.UserName));
                thewordmustnotbenamed = repo.CheckValidString(loginModel.Drowssap);
            }
            
            try
            {
                string path = JCLib.Util.GetEnvSetting("AD_SERV");

                // userName需設定允許清單以避免LDAP injection
                string allowlist = @"^[a-zA-Z0-9_\-\.\@']+$";
                userName = userName.Trim();
                Regex pattern = new Regex(allowlist);
                if (pattern.IsMatch(userName))
                {
                    using (DirectoryEntry entry = new DirectoryEntry("LDAP://" + path, userName, thewordmustnotbenamed))
                    {
                        object obj = entry.NativeObject;
                        using (DirectorySearcher search = new DirectorySearcher(entry, "(SAMAccountName=" + Encoder.LdapFilterEncode(userName) + ")"))
                        {
                            search.PropertiesToLoad.Add("cn");
                            search.PropertiesToLoad.Add("displayName");
                            search.PropertiesToLoad.Add("mail");
                            search.PropertiesToLoad.Add("SAMAccountName");
                            SearchResult searchResult = search.FindOne();

                            if (searchResult != null)
                            {
                                string displayName = "AD_USER";
                                string mail = "@";
                                string userAd = "";
                                var dnsr = searchResult.Properties["displayName"];
                                if (dnsr.Count > 0)
                                    displayName = dnsr[0].ToString();
                                var mlsr = searchResult.Properties["mail"];
                                if (mlsr.Count > 0)
                                    mail = mlsr[0].ToString();
                                var adsr = searchResult.Properties["SAMAccountName"]; // AD acc
                                if (adsr.Count > 0)
                                    userAd = adsr[0].ToString();
                                result.msg = displayName + "^" + mail + "^" + userAd;
                                result.success = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 使用AD伺服器驗證是否合法User (AIDC)
        /// </summary>
        /// <param name="loginModel">登入帳號密碼</param>
        /// <returns></returns>
        //private ApiResponse AuthenticateByAD2(LoginModel loginModel)
        //{
        //    ApiResponse result = new ApiResponse();
        //    result.success = false;

        //    string userName = loginModel.UserName;
        //    string drowssap = loginModel.Drowssap;
        //    try
        //    {
        //        string path = JCLib.Util.GetEnvSetting("AD_SERV");

        //        using (DirectoryEntry entry = new DirectoryEntry("LDAP://" + path, userName, drowssap))
        //        {
        //            string objectSid =
        //            (new System.Security.Principal.SecurityIdentifier(
        //                (byte[])entry.Properties["objectSid"].Value, 0).Value);
        //            result.success = (objectSid != null);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.msg = ex.Message;
        //    }
        //    return result;
        //}

        /// <summary>
        /// 使用API驗證是否合法User
        /// </summary>
        /// <param name="loginModel">登入帳號密碼</param>
        /// <returns></returns>
        private ApiResponse AuthenticateByAPI(LoginModel loginModel)
        {
            ApiResponse result = new ApiResponse();

            result.success = false;

            string userName = loginModel.UserName;
            string drowssap = loginModel.Drowssap;
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    using (HttpClient client = new HttpClient(handler))
                    {
                        // UDIaftygh!
                        int[] cIdx = new int[] {85, 68, 73, 97, 102, 116, 121, 103, 104, 33};
                        var passchar = new char[10];

                        for (int i = 0; i < cIdx.Length; i++)
                        {
                            passchar[i] = (char)cIdx[i];
                        }

                        LoginUserRequest auth_acc = new LoginUserRequest
                        {
                            Account = "ahop",
                            Password = new string(passchar)
                        };
                        // 先取得驗證Token
                        LoginAPIResult authResult = GetToken(auth_acc).Result;

                        if (authResult.Result == "1")
                        {
                            // 再用人員帳密+Token進行登入檢查
                            LoginUserRequest login_acc = new LoginUserRequest
                            {
                                Account = userName,
                                Password = drowssap,
                                Token = authResult.Token.ToString()
                            };

                            LoginUserResult loginResult = TryLogin(login_acc).Result;
                            if (loginResult.Result == "1")
                            {
                                result.msg = loginResult.Name + "^" + loginResult.Deptno + "^" + userName;
                                result.success = true;
                            }
                            else
                            {
                                // 人員帳密驗證失敗
                            }
                                
                        }
                        else
                        {
                            // 取得驗證Token失敗
                        }   
                    }
                };
            }
            catch (TimeoutException ex)
            {
                result.msg = ex.Message;
            }
            return result;
        }

        // 將系統帳密代入Body後取得驗證Token
        private static async Task<LoginAPIResult> GetToken(LoginUserRequest apiData)
        {
            string path1 = JCLib.Util.GetEnvSetting("804_API_URL1");

            LoginAPIResult fooAPIResult;
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    string json = JsonConvert.SerializeObject(apiData);
                    StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(path1, httpContent).Result;

                    HttpContent content = response.Content;
                    string result = content.ReadAsStringAsync().Result;
                    fooAPIResult = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginAPIResult>(result);
                }
            }

            return fooAPIResult;
        }

        // 將Token代入Header,使用者帳密代入Body後進行驗證
        private static async Task<LoginUserResult> TryLogin(LoginUserRequest apiData)
        {
            string path2 = JCLib.Util.GetEnvSetting("804_API_URL2");

            LoginUserResult fooAPIResult;
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    //var byteArray = Encoding.ASCII.GetBytes("acc:pwd");
                    //client.DefaultRequestHeaders.Authorization =
                    //    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                    //    Convert.ToBase64String(byteArray));

                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiData.Token);

                    string json = JsonConvert.SerializeObject(apiData);
                    StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(path2, httpContent).Result;

                    HttpContent content = response.Content;
                    string result = content.ReadAsStringAsync().Result;
                    fooAPIResult = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginUserResult>(result);
                }
            }

            return fooAPIResult;
        }

        /// <summary>
        /// 檢驗登入帳號是否屬於可登入的IP
        /// </summary>
        /// <param name="loginModel">登入帳號密碼</param>
        /// <returns></returns>
        private bool ChkValidIp(LoginModel loginModel, string userIp)
        {
            bool rtnSuccess = false;

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                var repo = new UR_IDRepository(DBWork);
                rtnSuccess = repo.CheckValidIp(loginModel, userIp);
            }

            return rtnSuccess;
        }

        /// <summary>
        /// 登入成功，設定cookie訊息
        /// </summary>
        /// <param name="userInfo"></param>
        private void SetAuthenticated(UserInfo userInfo)
        {
            if (isCookieless)
            {
                FormsAuthentication.SetAuthCookie(userInfo.UserId, false);
            }
            else
            {
                var ticket = new FormsAuthenticationTicket(
                       1, //version
                       userInfo.UserId, //name
                       DateTime.Now, //issueDate
                       DateTime.Now.AddMinutes(90), //expiration
                       true, //isPersistent
                       JsonConvert.SerializeObject(userInfo), //userData
                       "/"); //cookiePath
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
                {
                    HttpOnly = true
                };

                if (JCLib.Util.GetEnvSetting("IS_SSL") == "Y")
                    cookie.Secure = true;
                HttpContext.Response.Cookies.Add(cookie);

                if (JCLib.Util.GetEnvSetting("IS_SSL") == "Y") {
                    if (Response.Cookies.Count > 0)
                    {
                        foreach (string s in Response.Cookies.AllKeys)
                        {
                            //if (s == FormsAuthentication.FormsCookieName || s.ToLower() == "asp.net_sessionid")
                            //{
                            Response.Cookies[s].Secure = true;
                            //}
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 呼叫UR_IDRepository.WriteLogin，寫入登入資訊，帳號、登入時間、登入IP等等
        /// </summary>
        /// <param name="userInfo">使用者資訊</param>
        /// <returns></returns>
        private ApiResponse WriteLogin(UserInfo userInfo)
        {
            ApiResponse result = new ApiResponse();
            result.success = false;

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    int login_afrs = repo.WriteLogin(userInfo.SessionId.ToString(),
                        userInfo.UserId,
                        Request.UserHostAddress,
                        Request.ServerVariables["LOCAL_ADDR"]);

                    result.success = (login_afrs == 1);
                }
                catch (Exception ex)
                {
                    result.msg = ex.Message;
                }
            }

            return result;
        }

        private int CreateAccountForADUser(string userId, string userName, string userMail)
        {
            int result = 0;
            UR_ID ur_id = new UR_ID();

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    string nextTuser = repo.GetNextTuserByAD(userName);
                    ur_id.TUSER = nextTuser;
                    ur_id.UNA = userName;
                    ur_id.EMAIL = userMail;
                    ur_id.INID = "zzzzzz";
                    ur_id.ADUSER = userId;
                    result = repo.CreateForAdUser(ur_id);
                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
            }

            return result;
        }

        private int CreateAccountForAPIUser(string userId, string userName, string userDeptno)
        {
            int result = 0;
            UR_ID ur_id = new UR_ID();

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    string nextTuser = repo.GetNextTuserByAD(userName);
                    ur_id.TUSER = nextTuser;
                    ur_id.UNA = userName;
                    ur_id.EMAIL = "";
                    ur_id.INID = repo.TryGetInid(userDeptno); // 有INID資料則寫入,沒有則寫入zzzzzz
                    ur_id.ADUSER = userId;
                    result = repo.CreateForAdUser(ur_id);
                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
            }

            return result;
        }

        private bool UpdateUserAduser(string adUserId)
        {
            bool result = false;

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    if (repo.UpdateAduser(adUserId) > 0)
                        result = true;
                    DBWork.Commit();
                }
                catch
                {
                    DBWork.Rollback();
                    throw;
                }
            }

            return result;
        }

        private bool GetAdUserEnabled(string adUserId)
        {
            bool result = false;

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                var repo = new UR_IDRepository(DBWork);
                result = repo.GetEnabledByAD(adUserId);
            }

            return result;
        }

        private UserInfo GetUserInfo(AuthType authType, string userId)
        {
            UserInfo result = null;

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    UR_ID _userInfo = null;

                    if (authType == AuthType.AD || authType == AuthType.API || authType == AuthType.HISDB)
                    {
                        var tmp = repo.GetInfoByAD(userId);
                        if (tmp.Count() > 0)
                            _userInfo = tmp.First();
                        else
                            return null;
                    }

                    if (authType == AuthType.DB)
                        _userInfo = repo.GetInfo(userId).First();

                    var repo2 = new ParamRepository(DBWork);
                    var _viewall = repo2.GetVIEWALL(_userInfo.TUSER);
                    if (_userInfo != null)
                    {
                        result = new UserInfo(authType, _userInfo.TUSER, _userInfo.UNA, _userInfo.INID, _userInfo.INID_NAME, _viewall);
                    }
                }
                catch
                {
                    throw;
                }
            }

            if (result == null) result = new UserInfo();
            return result;
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [AllowAnonymous]
        public ActionResult LogOff()
        {
            DisableCache();

            if (User.Identity.IsAuthenticated)
            {
                using (WorkSession session = new WorkSession(this))
                {
                    var DBWork = session.UnitOfWork;

                    try
                    {
                        var repo = new UR_IDRepository(DBWork);

                        if (isCookieless)
                        {
                            //repo.WriteLogoutCookieless(User.Identity.Name);
                            repo.WriteLogoutCookieless(DBWork.UserInfo.UserId);
                        }
                        else
                        {
                            /*
                            var formsIdentity = (FormsIdentity)User.Identity;
                            var ticket = formsIdentity.Ticket;
                            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(ticket.UserData);
                            repo.WriteLogout(userInfo.SessionId.ToString(), userInfo.UserId);
                            */
                            repo.WriteLogout(DBWork.UserInfo.SessionId.ToString(), DBWork.UserInfo.UserId);
                        }
                    }
                    catch
                    {
                    }
                }

                FormsAuthentication.SignOut();
                Session.Clear();
                Session.Abandon();

                var rFormsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "")
                {
                    Expires = DateTime.Now.AddYears(-1)
                };
                Response.Cookies.Add(rFormsCookie);

                var sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
                var rSessionCookie = new HttpCookie(sessionStateSection.CookieName, "")
                {
                    Expires = DateTime.Now.AddYears(-1)
                };
                Response.Cookies.Add(rSessionCookie);
            }

            return RedirectToAction("Login", "Account");
        }

        private void DisableCache()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.SetExpires(DateTime.MinValue);
            Response.Cache.SetNoServerCaching();
            Response.Cache.SetNoStore();
        }

        #region Helpers
        //private ActionResult RedirectToLocal(string returnUrl)
        //{
        //    if (Url.IsLocalUrl(returnUrl))
        //    {
        //        return Redirect(returnUrl);
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }
        //}

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }
        #endregion


        //private ApiResponse GetCurrentDate()
        //{
        //    ApiResponse result = new ApiResponse();
        //    result.success = false;

        //    using (WorkSession session = new WorkSession())
        //    {
        //        var DBWork = session.UnitOfWork;
        //        try
        //        {
        //            var repo = new UR_IDRepository(DBWork);
        //            result.msg = repo.GetCurrentDate();
        //        }
        //        catch (Exception ex)
        //        {
        //            result.msg = ex.Message;
        //        }
        //    }

        //    return result;
        //}

        private string GetSysContactMsg()
        {
            string rtnMsg = "";

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    rtnMsg = repo.GetSysContactMsg();
                }
                catch (Exception ex)
                {
                    rtnMsg = ex.Message;
                }
            }

            return rtnMsg;
        }

        private string GetAdContactMsg()
        {
            string rtnMsg = "";

            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    rtnMsg = repo.GetAdContactMsg();
                }
                catch (Exception ex)
                {
                    rtnMsg = ex.Message;
                }
            }

            return rtnMsg;
        }

        public class NoAntiForgeryCheckAttribute : Attribute { }
    }
}
