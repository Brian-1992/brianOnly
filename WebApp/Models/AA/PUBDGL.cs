using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class PUBDGL : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string MMCODE { get; set; }
        public int APPQTY { get; set; }
        public string HIGH_QTY { get; set; }
        public string INV_QTY { get; set; }
        public int ACKQTY { get; set; }
        public string MEMO { get; set; }
        public DateTime CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string UPRICE { get; set; }
        public string APPAMT { get; set; }
        public string ISISSUE { get; set; }
        public string ISDEF { get; set; }
        public string ISWAS { get; set; }
        public string IS_DEL { get; set; }
        public string DISC_UPRICE { get; set; }

    }
}
