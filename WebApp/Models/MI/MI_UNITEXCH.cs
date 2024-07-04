using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MI_UNITEXCH : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string UNIT_CODE { get; set; }
        public string AGEN_NO { get; set; }
        public string EXCH_RATIO { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        // =================== 湘倫 ======================
        public string MMNAME_C { get; set; }
        public string UI_CHANAME { get; set; }
        public string AGEN_NAMEC { get; set; }
        // ===============================================
    }
}