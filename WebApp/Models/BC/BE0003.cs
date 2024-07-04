using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BE0003 : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string AGEN_NO { get; set; }
        public string INVOICE { get; set; }
        public string INVOICE_DT { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string PO_PRICE { get; set; }
        public string DELI_QTY { get; set; }
        public string CKIN_QTY { get; set; }
        public string AMOUNT { get; set; }
        public string DELI_DT { get; set; }
        public string MEMO { get; set; }
        public string TRANSNO { get; set; }
        public string M_CONTID { get; set; }
        public string INVOICE_TOT { get; set; }
        public string M_PHCTNCO { get; set; }
        public string CKSTATUS { get; set; }
        public string CREATE_TIME { get; set; } //建立日期
        public string CREATE_USER { get; set; } //建立人員
        public string UPDATE_TIME { get; set; } //異動日期
        public string UPDATE_USER { get; set; } //異動人員
        public string UPDATE_IP { get; set; } //異動IP
        public string ITEM_STRING { get; set; }
        public string CHK_USER { get; set; }
        public string CHK_DT { get; set; }
        public string CHG_CNT { get; set; }
        public string CHG_LIST { get; set; }
        public string PO_QTY { get; set; }
        public string DNO { get; set; }
        public string INVOICE_OLD { get; set; }
        public string TRANSNO_OLD { get; set; }
    }
}
