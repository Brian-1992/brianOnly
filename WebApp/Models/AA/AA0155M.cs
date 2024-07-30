using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0155M : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string APPTIME { get; set; }
        public string APPTIME_S { get; set; }
        public string APPTIME_E { get; set; }
        public string APPID { get; set; }
        public string FLOWID { get; set; }
        public string FRWH { get; set; }
        public string TOWH { get; set; }
        public string FRWH_NAME { get; set; }
        public string TOWH_NAME { get; set; }
        public string APPLY_KIND { get; set; }
        public string MAT_CLASS { get; set; }
        public string WH_NO { get; set; }
        public string WH_NO_NAME { get; set; }
        public string WH_KIND { get; set; }
        public string WH_GRADE { get; set; }
    }
}