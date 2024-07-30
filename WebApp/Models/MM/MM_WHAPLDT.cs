using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class MM_WHAPLDT : JCLib.Mvc.BaseModel
    {
        public string APPLY_DATE { get; set; }
        public string APPLY_YEAR_MONTH { get; set; }
        public string APPLY_DAY { get; set; }
        public string WH_NO { get; set; }
        public string WH_NO_N { get; set; }
        public string WH_NO_OLD { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string APPLY_TYPE { get; set; }
    }
}