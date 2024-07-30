using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class UR_UIR : JCLib.Mvc.BaseModel
    {
        public string RLNO { get; set; }
        public string TUSER { get; set; }
        public string UNA { get; set; }
        public Nullable<System.DateTime> UIR_CREATE_DATE { get; set; }
        public string UIR_CREATE_BY { get; set; }
        public Nullable<System.DateTime> UIR_MODIFY_DATE { get; set; }
        public string UIR_MODIFY_BY { get; set; }
        public string RLNA { get; set; }
    }
}