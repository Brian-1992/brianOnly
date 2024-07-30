using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class TC_INVCTL : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string M6AVG_USEQTY { get; set; }
        public string M3AVG_USEQTY { get; set; }
        public string M6MAX_USEQTY { get; set; }
        public string M3MAX_USEQTY { get; set; }
        public string BASE_UNIT { get; set; }

        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}