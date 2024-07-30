using System;
using System.IO;
using System.Web.Mvc;
using JCLib.DB;
using WebAppVen.Repository.UR;
using Newtonsoft.Json;

namespace SiteBase.Controllers
{
    public class FormController : Controller
    {
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult Show(string id, string id2)
        {
            return GetViewShow(id, id2);
        }
        // GET: /Form/Index/{id}/{id2}
        public ActionResult Index(string id, string id2)
        {
            return GetView("Index", id, id2);
        }
        // GET: /Form/Mobile/{id}/{id2}
        public ActionResult Mobile(string id, string id2)
        {
            return GetView("Mobile", id, id2);
        }
        public ActionResult Custom(string id, string id2)
        {
            return GetViewDelta(id, id2);
        }
        // GET: /Form/Index/{id}/{id2}
        public ActionResult DS(string id, string id2)
        {
            return GetView("DS", id, id2);
        }
        private ActionResult GetViewShow(string id, string id2)
        {
            string s;
            string sKeyVal = "";

            if (id2 != null)
            {
                if (Path.IsPathRooted(id2))
                    s = id;
                else
                    s = Path.Combine(id, id2);
            }
            else
            {
                s = id;
            }

            var guid = Guid.NewGuid();
            if (sKeyVal == "")
            {
                s += ".js?";
                ViewBag.Script = s + "guid=" + guid.ToString().Substring(0, 8); ;
            }
            else
            {
                s += ".js?" + sKeyVal;
                ViewBag.Script = s + "&guid=" + guid.ToString().Substring(0, 8);
            }

            return View();
        }
        private ActionResult GetView(string viewType, string id, string id2)
        {
            var sKeyVal = "";
            bool passCheck;
            //var form = string.Format("Form/{0}/{1}/{2}", viewType, id, id2);
            //if (true || User.Identity.IsAuthenticated && HasAccess(form, User.Identity.Name))
            //passCheck = ws.iBus1.Pins["Sys.PassCheck"].ToBool();

            if (!User.Identity.IsAuthenticated)
            {
                //return RedirectToAction("GenericErrorPage", "Home");
                return Redirect("../../../ErrorPage.html");
            }

            object _auth = null;
            var _hasAccess = false;
            var _jsUrl = "";
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    var repo = new UR_IDRepository(DBWork);
                    _auth = repo.CheckAccess(id, User.Identity.Name);
                    _hasAccess = (_auth != null);

                    if (_hasAccess)
                    {
                        var repo2 = new UR_MENURepository(DBWork);
                        _jsUrl = repo2.GetUrl(id);
                    }
                }
                catch
                {
                    throw;
                }
            }

            if (_hasAccess)
            {
                var s = _jsUrl;
                /*
                if (Request.QueryString.AllKeys.Length > 0)
                {
                    foreach (string sKey in Request.QueryString.AllKeys)
                    {
                        if (sKey != null)
                        {
                            sKeyVal += sKey + "=" + Request.QueryString[sKey] + "&";
                            sKeyVal = sKeyVal.Remove(sKeyVal.Length - 1);
                        }
                    }
                }*/

                var guid = Guid.NewGuid().ToString().Substring(0, 8);
                var cn = s.IndexOf('?') == -1 ? "?" : "&";
                ViewBag.Script = string.Format("{0}{1}guid={2}", s, cn, guid);
                ViewBag.Auth = JsonConvert.SerializeObject(_auth);
                /*
                if (sKeyVal == "")
                {
                    s += ".js?";
                    ViewBag.Script = s + "guid=" + guid.ToString().Substring(0, 8);
                }
                else
                {
                    s += ".js?" + sKeyVal;
                    ViewBag.Script = s + "&guid=" + guid.ToString().Substring(0, 8);
                }*/
            }
            else
            {
                ViewBag.Script = "AccessDenied.js";
            }
            return View();
        }
        //2016/06/03簽核程式直接Pass
        private ActionResult GetViewDelta(string id, string id2)
        {
            var s = "";
            var sKeyVal = "";
            bool passCheck;
            var form = string.Format("Form/Index/{0}/{1}", id, id2);
            //if (true || User.Identity.IsAuthenticated && HasAccess(form, User.Identity.Name))
            //passCheck = ws.iBus1.Pins["Sys.PassCheck"].ToBool();
            if (!User.Identity.IsAuthenticated && id != "DeltaFlow")
            {
                //return RedirectToAction("GenericErrorPage", "Home");
                return Redirect("../../../ErrorPage.html");
            }

            if (id == "DeltaFlow")
            {
                if (id2 != null)
                {
                    if (Path.IsPathRooted(id2))
                        s = id;
                    else
                        s = Path.Combine(id, id2);
                }
                else
                {
                    s = id;
                }
                if (Request.QueryString.AllKeys.Length > 0)
                {
                    var builder = new System.Text.StringBuilder();
                    builder.Append(sKeyVal);
                    foreach (string sKey in Request.QueryString.AllKeys)
                    {
                        if (sKey != null)
                        {
                            builder.Append(sKey + "=" + Request.QueryString[sKey] + "&");
                            sKeyVal = sKeyVal.Remove(sKeyVal.Length - 1);
                        }
                    }
                    sKeyVal = builder.ToString();
                }

                var guid = Guid.NewGuid();
                if (sKeyVal == "")
                {
                    s += "?";
                    ViewBag.Script = s + "guid=" + guid.ToString().Substring(0, 8); ;
                }
                else
                {
                    s += "?" + sKeyVal;
                    ViewBag.Script = s + "&guid=" + guid.ToString().Substring(0, 8);
                }
            }
            else
            {
                ViewBag.Script = "AccessDenied.js";
            }
            return View();
        }
    }
}
