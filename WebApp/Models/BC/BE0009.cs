using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BE0009 : JCLib.Mvc.BaseModel
    {
        public string INVOICE { get; set; }
        public string MMCODE { get; set; }
        public string INVOICE_DT { get; set; }
        public string AGEN_NO { get; set; }
        public string M_NHIKEY { get; set; }
        public string INVOICE_QTY { get; set; }
        public string INVOICE_PRICE { get; set; }
        public string INVOICE_AMOUNT { get; set; }
        public string IS_INCLUDE_TAX { get; set; }
        public string INVMARK { get; set; }
        public string REBATESUM { get; set; }
        public string DISC_AMOUNT { get; set; }
        public string ACT_YM { get; set; }
        public string CREATE_TIME { get; set; } //建立日期
        public string CREATE_USER { get; set; } //建立人員
        public string UPDATE_TIME { get; set; } //異動日期
        public string UPDATE_USER { get; set; } //異動人員
        public string UPDATE_IP { get; set; } //異動IP
        public string AGEN_NAMEC { get; set; }
        public string NUI_NO { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string AMTPAID { get; set; }
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string UNI_NO { get; set; }
        public string DELI_DT { get; set; }
    }
}