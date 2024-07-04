using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ME_DOCD : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string SEQ { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }    // table MI_MAST
        public string APPQTY { get; set; }
        public string APVQTY { get; set; }
        public string APVTIME { get; set; }
        public string APVID { get; set; }
        public string ACKQTY { get; set; }
        public string ACKID { get; set; }
        public string ACKTIME { get; set; }
        public string STAT { get; set; }
        public string RDOCNO { get; set; }
        public string RSEQ { get; set; }
        public string EXPT_DISTQTY { get; set; }
        public string LACK_QTY { get; set; }
        public string PICK_QTY { get; set; }
        public string PICK_USER { get; set; }
        public string PICK_TIME { get; set; }
        public string APLYITEM_NOTE { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
    }
}