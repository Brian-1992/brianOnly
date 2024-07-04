using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class PH_SMALL_D : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string DN { get; set; }
        public string MEMO { get; set; }
        public string MMCODE { get; set; }
        public string INID { get; set; }
        public string CHARGE { get; set; }
        public string NMSPEC { get; set; }
        public double PRICE { get; set; }
        public int QTY { get; set; }
        public string UNIT { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        // =========================================
        public int TOTAL_PRICE { get; set; }
        public string INIDNAME { get; set; }
        public string UK { get; set; }
        // =========================================
        public string CHECK_RESULT { get; set; }
        public string IMPORT_RESULT { get; set; }
        public string SUMQTY { get; set; }
        public string SUMPRICE { get; set; }
        public string OLD_MMCODE { get; set; }
        public string OLD_NMSPEC { get; set; }
        public string OLD_INID { get; set; }
    }
}