using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BE0006 : JCLib.Mvc.BaseModel
    {     
        public string PO_NO { get; set; }
        public string M_CONTID { get; set; }
        public string AGEN_NO { get; set; }
        public string AGEN_NAME { get; set; }
        public string INVOICE { get; set; }
        public string INVOICE_DT { get; set; }
        public string EMAIL_DT { get; set; }
        public string REPLY_DT { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string PO_PRICE { get; set; }
        public string  PO_QTY { get; set; }
        public string AMOUNT { get; set; }
        public string AMOUNT_1 { get; set; }
        public string DELI_DT { get; set; }
        public string DELI_DT_1 { get; set; }
        public string DELI_QTY { get; set; }        
        public string PO_TIME { get; set; }
        public string agen_email { get; set; }
        public string TRANSNO { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateTime { get; set; }
        public string UpdateIp { get; set; }
        public string STATUS { get; set; }
        public string TWN_PO_TIME { get; set; }
        public string Batno { get; set; }
        public string MAT_CLASS { get; set; }        
    }

}