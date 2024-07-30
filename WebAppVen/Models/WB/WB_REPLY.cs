using System;
using System.Collections.Generic;

namespace WebAppVen.Models
{
    public class WB_REPLY : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string AGEN_NO { get; set; }
        public string MMCODE { get; set; }
        public string DNO { get; set; }
        public string SEQ { get; set; }
        public string DELI_DT { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string INQTY { get; set; }
        public string BW_SQTY { get; set; }
        public string INVOICE { get; set; }
        public string BARCODE { get; set; }
        public string MEMO { get; set; }
        public string STATUS { get; set; }
        public string FLAG { get; set; }
        public string INVOICE_DT { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string MMNAME_C { get; set; }
        public string M_PURUN { get; set; }
        public string MMNAME { get; set; }
        public string QTY { get; set; }
        public string PO_PRICE { get; set; }
        public Int32 TOT { get; set; }
        public string WEXP_ID { get; set; }
        public string CHECK_RESULT { get; set; }
        public string IMPORT_RESULT { get; set; }
        public string INVOICE_OLD { get; set; }
    }
}