using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.AA
{
    public class AA0182 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string WH_NAME { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string EXP_CHK { get; set; }
        public string INV_QTY { get; set; }
        public string WH_INV_QTY { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}