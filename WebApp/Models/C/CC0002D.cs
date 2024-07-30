using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class CC0002D : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string MMCODE { get; set; }
        public string PO_QTY { get; set; }
        public string M_STOREID { get; set; }
        public string STOREID { get; set; }
        public string ACC_QTY { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string M_PURUN { get; set; }
        public string BASE_UNIT { get; set; }
        // ==================================
        public string SEQ { get; set; }
        public string AGEN_NO { get; set; }
        public string LOT_NO { get; set; }
        public DateTime EXP_DATE { get; set; }
        public string BW_SQTY { get; set; }
        public string INQTY { get; set; }
        public string INQTY_O { get; set; }
        public string STATUS { get; set; }
        public string MEMO { get; set; }
        public string ACC_TIME { get; set; }
        public string ACC_USER { get; set; }
        // ==================================
        public string PR_DEPT { get; set; }
        public string INID { get; set; }
        public string INID_NAME { get; set; }
        public string PR_QTY { get; set; }
        public string DIST_QTY { get; set; }
        public string MAT_CLASS { get; set; }
        public string PO_QTY2 { get; set; }
        public string SWAP_PO_QTY { get; set; }
        public string PO_ACC_QTY { get; set; }
        public string REPLY_QTY { get; set; }
        public string WEXP_ID { get; set; }
        public string UNIT_SWAP { get; set; }
        public string WH_NO { get; set; }
        public string PURDATE { get; set; }
        // ==================================
        public string AGEN_NAME { get; set; }
        public string INVOICE { get; set; }
        public string NEXTMON { get; set; }
        public string DSUM_QTY { get; set; }
        public string PH_REPLY_SEQ { get; set; }
        public string RETAIN_QTY { get; set; }
        public string RETAIN_DISTQTY { get; set; }
    }
}