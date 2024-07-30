using System;
using System.Collections.Generic;

namespace WebAppVen.Models
{
    public class ME_MAILBACK : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string AGEN_NO { get; set; }
        public string MMCODE { get; set; }
        public string EXP_DATE { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_QTY { get; set; }
        public string BACK_DT { get; set; }
        public string STATUS { get; set; }
        public string CREATE_TIME { get; set; }
        public string UPDATE_IP { get; set; }
        public string MAIL_NO { get; set; }
    }
}