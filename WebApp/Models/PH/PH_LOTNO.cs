using System;
using System.Collections.Generic;


namespace WebApp.Models.PH
{
    public class PH_LOTNO : JCLib.Mvc.BaseModel
    {
        public string MAT_CLASS { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string EXP_DATE { get; set; }
        public string LOT_NO { get; set; }
        public string QTY { get; set; }
        public string MEMO { get; set; }
        public string SOURCE { get; set; }
        public string PO_NO { get; set; }
        public string STATUS { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_IP { get; set; }
        public string UPDATE_USER { get; set; }
        public string DOCNO { get; set; }
        public string CREATE_USER { get; set; }

        public string SEQ { get; set; }

    }
}