using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models.ADC
{
    public class getMedicalAllocateResult
    {
        public string DOCNO { get; set; }
        public int SEQ { get; set; }
        public string FRWH { get; set; }
        public string TOWH { get; set; }
        public string MMCODE { get; set; }
        public int APPQTY { get; set; }
        public int APVQTY { get; set; }
        public int ACKQTY { get; set; }
        public string LOTNO { get; set; }
        public string EXPDATE { get; set; }
    }
}