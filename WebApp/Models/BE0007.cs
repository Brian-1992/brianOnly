using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BE0007 : JCLib.Mvc.BaseModel
    {

        public string INVOICE { get; set; }
        public string INVOICE_DT { get; set; }
        public string MMCODE { get; set; }
        public string M_NHIKEY { get; set; }
        public string MMNAME_C { get; set; }
        public string AGEN_NO { get; set; }
        public string MAT_CLASS { get; set; }
        public string UNI_NO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string DISC_CPRICE { get; set; }
        public string DELI_QTY { get; set; }
        public string PAY_AMOUNT { get; set; }
        public string EXTRA_DISC_AMOUNT { get; set; }
        public string MORE_DISC_AMOUNT { get; set; }
        public string NHI_PRICE { get; set; }
        public string TOTAL_AMT { get; set; }
        public string PO_AMT { get; set; }
        public string PO_PRICE { get; set; }
        public string PO_NO { get; set; }
        public string TRANSNO { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string CREATE_USER { get; set; }
        public string ITEM_STRING { get; set; }
        public string SHOULD_PAY { get; set; }
        public string SPMMCODE { get; set; }
        public string CONTRACT_DISC_AMOUNT { get; set; }
        public string ACTUAL_PAY { get; set; }
        public string IS_INCLUDE_TAX { get; set; }
        public string UPDATE_TIME { get; set; }
        public string DATA_YM { get; set; }
        public string INVOICE_TYPE { get; set; }
        public string IN_AMOUNT { get; set; }

        public string ORI_EXTRA_DISC_AMOUNT { get; set; } // 原始折讓金額
    }
}