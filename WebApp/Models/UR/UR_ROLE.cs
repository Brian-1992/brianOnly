using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class UR_ROLE : JCLib.Mvc.BaseModel
    {
        public string RLNO1 { get; set; }
        public string RLNO { get; set; }
        public string RLNA { get; set; }
        public string RLDESC { get; set; }
        public Nullable<System.DateTime> ROLE_CREATE_DATE { get; set; }
        public string ROLE_CREATE_BY { get; set; }
        public Nullable<System.DateTime> ROLE_MODIFY_DATE { get; set; }
        public string ROLE_MODIFY_BY { get; set; }
    }
}