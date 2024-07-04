using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_DOCC : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string SEQ { get; set; }
        public int CHECKSEQ { get; set; }
        public string GENWAY { get; set; }
        public string ACKQTY { get; set; }
        public string ACKTIME { get; set; }
        public string ACKID { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}