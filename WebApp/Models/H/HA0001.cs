using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.H
{
    public class HA0001 : JCLib.Mvc.BaseModel
    {
        public string REMITNO { get; set; }
        public string DATA_YM { get; set; }
        public string REMITDATE { get; set; }
        public string AGEN_NO { get; set; }
        public string AGEN_NAME { get; set; }
        public string AGEN_TEL { get; set; }
        public string AGEN_ADD { get; set; }
        public string AGEN_BANK_14 { get; set; }
        public string BANKNAME { get; set; }
        public string AGEN_ACC { get; set; }
        public string PO_AMT { get; set; }
        public string ADDORSUB_AMT { get; set; }
        public string AMTPAYABLE { get; set; }
        public string DISC_AMT { get; set; }
        public string REBATE_AMT { get; set; }
        public string AMTPAID { get; set; }
        public string PROCFEE { get; set; }
        public string MGTFEE { get; set; }
        public string REMIT { get; set; }
        public string ISREMIT { get; set; }
        public string XFRMEMO { get; set; }
        public string CHKMSG { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}