using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BC_WHPICK_TEMP_LOTDOC : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string CALC_TIME { get; set; }
        public string LOT_NO { get; set; }
        public string DOCNO { get; set; }
        public string CREATE_USER { get; set; }
        public string CREATE_DATE { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}