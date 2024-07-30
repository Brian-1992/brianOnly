using System;
using System.Collections.Generic;

namespace WebApp.Models.AA
{
    public class AA0133 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string PMN_INVQTY { get; set; }
        public double MN_INQTY { get; set; }
        public double USE_QTY { get; set; }
        public string INV_QTY { get; set; }
        public string CHK_QTY { get; set; }

        public string STORE_QTY { get; set; }
        public string STORE_QTY_TIME { get; set; }

        public string CONSUME_DATE_SUM { get; set; }
        public string CONSUME_D_SUM { get; set; }
        public string ORDER_TOTAL { get; set; }

    }
}

