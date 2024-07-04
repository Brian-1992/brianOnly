using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppVen.Models
{
    public class WB_MM_PO_M : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string AGEN_NO { get; set; }
        public string PO_TIME { get; set; }
        public string M_CONTID { get; set; }
        public string PO_STATUS { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string MEMO { get; set; }
        public string ISCONFIRM { get; set; }
        public string ISBACK { get; set; }
        public string PHONE { get; set; }
        public string SMEMO { get; set; }
        public string ISCOPY { get; set; }

        public string MMCODE { get; set; }
        public string PO_QTY { get; set; }
        public string PO_PRICE { get; set; }
        public string M_PURUN { get; set; }
        public string M_AGENLAB { get; set; }
        public string PO_AMT { get; set; }
        public string M_DISCPERC { get; set; }
        public string UNIT_SWAP { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }


        public string ISCR { get; set; }
    }
}