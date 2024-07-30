using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MM_ITEMS_COMP : JCLib.Mvc.BaseModel
    {
        public string INID { get; set; }
        public string COMPLEXITY { get; set; } 
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        // ============================
        public string INID_NAME { get; set; }
    }
}