using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class FA0082M : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string FRWH { get; set; }
        public string FRWH_N { get; set; }
        public string TOWH { get; set; }
        public string TOWH_N { get; set; }
        public string MAT_CLASS { get; set; }
        public string MAT_CLASS_N { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string MMNAME_CE { get; set; }
        public string BASE_UNIT { get; set; }
        public string APPTIME { get; set; }
        public string APPTIME_T { get; set; }
        public string APPQTY { get; set; }
        public string ACKTIME { get; set; }
        public string ACKTIME_T { get; set; }
        public string ACKQTY { get; set; }
        public string APVTIME { get; set; }
        public string APVTIME_T { get; set; }
        public string APVQTY { get; set; }
        public string FLOWID { get; set; }
        public string FLOWID_N { get; set; }
        public string MAT_CLASS_SUB { get; set; }
        public string MAT_CLASS_SUB_N { get; set; }
    }
}