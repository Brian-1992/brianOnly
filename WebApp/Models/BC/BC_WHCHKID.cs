using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BC_WHCHKID : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string WH_CHKUID { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }


        public string WH_CHKUID_NAME { get; set; }
        public string WH_NAME { get; set; }
        public string WH_GRADE { get; set; }
        public string WH_KIND { get; set; }

        public string ITEM_STRING { get; set; }


        public string IS_SELECTED { get; set; }
        public string HAS_ENTRY { get; set; }
    }
}