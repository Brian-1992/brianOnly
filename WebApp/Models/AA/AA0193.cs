using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0193_M : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string APPDEPT { get; set; }
        public string INID_NAME { get; set; }
    }
    public class AA0193_D : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MCODE_NAME { get; set; }
        public string BASE_UNIT { get; set; }
        public string ACKQTY { get; set; }
        public string HIGH_QTY { get; set; }
        public string ISWAS { get; set; }
        public string ISDEF { get; set; }
    }
}