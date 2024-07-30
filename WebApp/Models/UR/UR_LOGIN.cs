using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class UR_LOGIN : JCLib.Mvc.BaseModel
    {
        public string SID { get; set; }
        public string TUSER { get; set; }
        public string UNA { get; set; }
        public DateTime? LOGIN_DATE { get; set; }
        public DateTime? LOGIN_DATE_B { get; set; }
        public DateTime? LOGIN_DATE_E { get; set; }
        public DateTime? LOGOUT_DATE { get; set; }
        public string USER_IP { get; set; }
        public string AP_IP { get; set; }
    }
}