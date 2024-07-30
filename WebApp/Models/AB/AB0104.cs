using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AB0104 : JCLib.Mvc.BaseModel
    {
        public string TOWH { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string APPTIME { get; set; }
        public string APPID { get; set; }
        public string APPQTY { get; set; }
        public string FRWH { get; set; }
        public string APVTIME { get; set; }
        public string APVID { get; set; }
        public string APVQTY { get; set; }
        public string ACKTIME { get; set; }
        public string ACKID { get; set; }
        public string ACKQTY { get; set; }
        public string RCVQTY { get; set; }
        public string TRNAB_QTY { get; set; }
        public string TRNAB_RESON { get; set; }
        public string DOCNO { get; set; }
        public string FLOWID { get; set; }
        public string APPDEPT { get; set; }
    }
}