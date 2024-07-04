using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MI_WHID : JCLib.Mvc.BaseModel 
    {
        public string WH_NO { get; set; }
        public string WH_USERID { get; set; }
        public string WH_UNA { get; set; }
        public string WH_NAME { get; set; }
        public string TASK_ID { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        // =====================================
        public string WH_KIND { get; set; }

        public string ORI_TASK_ID { get; set; }

        public string INID { get; set; }
        public string WH_GRADE { get; set; }

    }
}