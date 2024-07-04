using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BD0022 : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string PO_TIME { get; set; }
        public string AGEN_NO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string PO_AMT { get; set; }
        public string MEMO { get; set; }
        public string PO_AMT_SUM { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME { get; set; }
        public string M_PURUN { get; set; }
        public string ITEM_NO { get; set; }
        public string BASE_UNIT { get; set; }
        public string PO_QTY { get; set; }
        public string PO_PRICE { get; set; }
        public string DELI_STATUS { get; set; }
    }
}