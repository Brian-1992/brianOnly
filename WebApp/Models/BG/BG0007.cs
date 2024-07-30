using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BG0007 : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string M_PURUN { get; set; }
        public float PO_PRICE { get; set; }
        public float PO_QTY { get; set; }
        public float PO_AMT { get; set; }
        public float TOTSUM { get; set; }

    }
}
