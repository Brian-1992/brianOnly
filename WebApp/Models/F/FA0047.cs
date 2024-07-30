using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class FA0047 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MILTYPE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string DATA_YM { get; set; }
        public string INID { get; set; }        
        public string M_STOREID { get; set; }
        public string M_CONTID { get; set; }
        public string P_INV_QTY { get; set; }
        public string PMN_AVGPRICE { get; set; }
        public string PMN_AMT { get; set; }
        public string IN_QTY { get; set; }
        public string IN_PRICE { get; set; }
        public string IN_AMT { get; set; }
        public string OUT_QTY { get; set; }
        public string AVG_PRICE { get; set; }
        public string OUT_AMT { get; set; }
        public string INV_QTY { get; set; }     
        public string INV_PRICE { get; set; }
        public string INV_AMT { get; set; }
    }
}

