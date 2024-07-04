using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class UR_INID_GRP : JCLib.Mvc.BaseModel
    {
        public string GRP_NO { get; set; }
        public string GRP_NAME { get; set; }
        public DateTime CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public DateTime? UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}