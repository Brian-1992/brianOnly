using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class PH_SMALL_MAIL : JCLib.Mvc.BaseModel
    {
        public string SEND_TO { get; set; }
        public string MAIL_ADD { get; set; }
        public string MEMO { get; set; }
    }
}