using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MI_UNITCODE : JCLib.Mvc.BaseModel
    {
        public string UNIT_CODE { get; set; }
        public string UI_NAME { get; set; }     // 自訂 UNIT_CODE + UI_CHANAME
        public string UI_CHANAME { get; set; }
        public string UI_ENGNAME { get; set; }
        public string UI_SNAME { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}