using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models.ADC.GetMedicalCheckAccept
{
    public class GetMedicalCheckAcceptResult
    {
        public string DOCNO { get; set; }
        public int SEQ { get; set; }
        public string MMCODE { get; set; }
        public int APPQTY { get; set; }
        public int APVQTY { get; set; }
        public string APPDEPT { get; set; }
        public string USEDEPT { get; set; }
        public string STOCKCODE { get; set; }
        public string APVTIME { get; set; }
        public string APVID { get; set; }
        public string LOTNO { get; set; }
        public string EXPDATE { get; set; }
    }
}