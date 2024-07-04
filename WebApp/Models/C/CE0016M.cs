using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CE0016M : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT {get;set;}
        public string STORE_LOC { get; set; }
        public string INV_QTY { get; set; }
        public string CHK_QTY { get; set; }
        public string CHK_REMARK { get; set; }
        public string CHK_UID { get; set; }
    }
}