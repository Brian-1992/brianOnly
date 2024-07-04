using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Reporting.WebForms;
using System.Web.UI;
using System.Security;

namespace JCLib.Report
{
    public static class AntiforgeryChecker
    {
        public static void Check(Page page, System.Web.UI.WebControls.HiddenField antiforgery)
        {
            if (!page.IsPostBack)
            {
                // 這邊先用簡單的 guid，後面再看要不要使用 Antiforgery 處理
                Guid antiforgeryToken = Guid.NewGuid();
                page.Session["AntiforgeryToken"] = antiforgeryToken;
                antiforgery.Value = antiforgeryToken.ToString();
            }
            else
            {
                Guid stored = (Guid)page.Session["AntiforgeryToken"];
                Guid sent = new Guid(antiforgery.Value);
                if (sent != stored)
                {
                    throw new SecurityException("XSRF Attack Detected!");
                }
            }
        }
    }
}
