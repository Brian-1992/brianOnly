using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MM_ABS_FEE : JCLib.Mvc.BaseModel
    {
        public string DATA_YM { get; set; }
        public string INID { get; set; }
        public string ABS_AMOUNT { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

    }
}