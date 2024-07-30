using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CN_RECORD : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string WH_NO { get; set; }
        public string WH_NAME { get; set; }
        public string QTY { get; set; }
        public string RQTY { get; set; }
        public string RQTY_SUM { get; set; }
        public string DISC_CPRICE { get; set; }
        public string QTY_CHK { get; set; }
        public string CREATE_USER { get; set; }
        public string CREATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_IP { get; set; }
    }
}