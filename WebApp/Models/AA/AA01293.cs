using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.AA
{
    public class AA01293 : JCLib.Mvc.BaseModel
    {
        public string WH_NAME { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string WEXP_ID { get; set; }
        public float INV_QTY { get; set; }
        public float ONWAY_QTY { get; set; }
        public float APL_INQTY { get; set; }
        public float APL_OUTQTY { get; set; }
        public float TRN_INQTY { get; set; }
        public float TRN_OUTQTY { get; set; }
        public float ADJ_INQTY { get; set; }
        public float ADJ_OUTQTY { get; set; }
        public float BAK_INQTY { get; set; }
        public float BAK_OUTQTY { get; set; }
        public float REJ_OUTQTY { get; set; }
        public float DIS_OUTQTY { get; set; }
        public float EXG_INQTY { get; set; }
        public float EXG_OUTQTY { get; set; }
        public float MIL_INQTY { get; set; }
        public float MIL_OUTQTY { get; set; }
        public float INVENTORYQTY { get; set; }
        public float TUNEAMOUNT { get; set; }
        public float USE_QTY { get; set; }
        public float PRE_QTY { get; set; }
        public string CTDMDCCODE { get; set; }
    }
}