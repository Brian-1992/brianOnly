using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class TC_MMAGEN : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string PUR_UNIT { get; set; }
        public double IN_PURPRICE { get; set; }
        public double PUR_SEQ { get; set; }        
        public DateTime CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_IP { get; set; }
        public string UPDATE_USER { get; set; }

    }
}