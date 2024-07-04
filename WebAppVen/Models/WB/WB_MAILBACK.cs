using System;
using System.Collections.Generic;

namespace WebAppVen.Models
{
    public class WB_MAILBACK : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string AGEN_NO { get; set; }
        public string PO_NO { get; set; }
        public string BACK_DT { get; set; }
        public string STATUS { get; set; }
        public string UPDATE_IP { get; set; }
    }
}