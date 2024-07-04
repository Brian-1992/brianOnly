using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AB0126 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string E_RESTRICTCODE { get; set; }
        public string EXTRA_DATA { get; set; }
        public string WEXP_ID { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string INV_QTY { get; set; }
        public string APP_QTY { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}
