using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_DOCD_EC : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string SEQ { get; set; }
        public string MMCODE { get; set; }
        public int APPQTY { get; set; }
        public int M_APPQTY { get; set; }
        public int S_APPQTY { get; set; }
        public string APVID { get; set; }
        public DateTime APVTIME { get; set; }
        public string APLYITEM_NOTE { get; set; }
        public DateTime CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTPRICE { get; set; }
        public string M_AGENNO { get; set; }
        public string M_PURUN { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string T_APPQTY { get; set; }
        public string EXCH_RATIO { get; set; }
        public string A_PACK { get; set; }
        public string M_PACK { get; set; }
        public string S_PACK { get; set; }
        public string T_PACK { get; set; }
        public string MIN_PRICE { get; set; }
        public string T_PRICE { get; set; }
        public string A_INV_QTY { get; set; }
        public string M_INV_QTY { get; set; }
        public string S_INV_QTY { get; set; }
        public string A_APV_QTY { get; set; }
        public string M_APV_QTY { get; set; }
        public string S_APV_QTY { get; set; }
        public string TOT_APPQTY { get; set; }
        public string TOT_APVQTY { get; set; }
        public string SUM_EX { get; set; }

    }
}
