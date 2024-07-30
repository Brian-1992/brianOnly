using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Configuration;
using WebAppVen.Repository.UR;
using WebAppVen.Models;
using JCLib.DB;
using JCLib.Mvc;
using Newtonsoft.Json;
using WebAppVen.Filters;
using BotDetect.Web.Mvc;

namespace SiteBase.Controllers
{
    [Authorize]
    //[InitializeSimpleMembership]
    public class AccountController : Controller
    {
        public const bool isCookieless = false;
        public const string TF_Login = "TraZLIZ";
        public const string TF_LoginAD = "TraZLIZAD";

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            //將顯示用訊息從 TempData 取出
            var errmsg = TempData["Timeout"] as string;
            if (!string.IsNullOrWhiteSpace(errmsg))
            {
                ViewBag.Timeout = errmsg;//將顯示訊息放入 ViewBag 供 view 使用
            }

            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [AntiForgeryErrorHandler(ExceptionType = typeof(HttpAntiForgeryException), View = "Login", Controller = "Account", ErrorMessage = "請重新登入")]

        public ActionResult Login([Bind(Exclude = "Flag")] LoginModel model, string returnUrl)
        {
            var name = string.Empty;
            var msg = string.Empty;
            var acct = model.UserName;
            model.Flag = "0";

            if (Request.Url.Host != Request.UrlReferrer.Host) {
                throw new Exception("Referer validate fail");
            }

            MvcCaptcha mvcCaptcha = new MvcCaptcha("WebAppVenCaptcha");
            string userInput = HttpContext.Request.Form["CaptchaCode"];
            string validatingInstanceId = HttpContext.Request.Form[mvcCaptcha.ValidatingInstanceKey];

            if (!MvcCaptcha.Validate("WebAppVenCaptcha", userInput, validatingInstanceId))
            {
                ModelState.AddModelError("", "登入失敗！請檢查您的帳號、密碼與驗證碼！");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "登入失敗！請檢查您的帳號與密碼！");
                return View(model);
            }

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

            bool login_success = false;
            UserInfo user = null;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    login_success = repo.CheckLogin(model);
                    if (login_success)
                    {
                        int login_afrs = repo.WriteLogin("", acct, Request.UserHostAddress, Request.ServerVariables["LOCAL_ADDR"]);
                        login_success = (login_afrs == 1) && login_success;
                        if (login_success) { DBWork.Commit(); }
                        else { msg = string.Format("登入發生錯誤[{0}]。", login_afrs.ToString()); }
                        var _userInfo = repo.GetInfo(acct).First();
                        if (_userInfo != null)
                        {
                            user = new UserInfo(AuthType.DB, acct, _userInfo.UNA, _userInfo.INID, _userInfo.INID_NAME, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DBWork.Rollback();
                    msg = "登入發生錯誤[E]。";
                }
            }

            if (user == null) user = new UserInfo();

            if (login_success)
            {
                if (isCookieless)
                {
                    FormsAuthentication.SetAuthCookie(acct, false);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var ticket = new FormsAuthenticationTicket(
                           1, //version
                           acct, //name
                           DateTime.Now, //issueDate
                           DateTime.Now.AddMinutes(30), //expiration
                           false, //isPersistent
                           JsonConvert.SerializeObject(user), //userData
                           "/"); //cookiePath
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
                    {
                        HttpOnly = true
                    };
                    cookie.Secure = true;
                    HttpContext.Response.Cookies.Add(cookie);

                    /* 廠商用網站的沒有行動裝置首頁
                    if (model.UseMobile)
                        return RedirectToAction("Mobile", "Home");
                    else
                        return RedirectToAction("Index", "Home");
                        */
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                if (msg.Length == 0)
                {
                    msg = "登入失敗！請檢查您的帳號、密碼與驗證碼！";
                }
            }

            ModelState.AddModelError("", msg);
            return View(model);
        }

        // POST: /Account/LogOff

        [HttpPost]
        [NoAntiForgeryCheckAttribute]
        public ActionResult LogOff()
        {
            DisableCache();

            if (User.Identity.IsAuthenticated)
            {
                using (WorkSession session = new WorkSession())
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

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        #endregion

    }

    public class NoAntiForgeryCheckAttribute : Attribute { }
}
