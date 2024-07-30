using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0047M : JCLib.Mvc.BaseModel
    {
        public string CHECK_RESULT { get; set; }
        public string IMPORT_RESULT { get; set; }
        public string APPLY_DATE { get; set; }
        public string APPLY_YEAR_MONTH { get; set; }
        public string APPLY_DAY { get; set; }
        public string WH_NO { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string APPLY_TYPE { get; set; }

    }
}