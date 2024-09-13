using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models.ADC
{
    public class getAdcMedOrdResult
    {
        public string DOCNO { get; set; }
        public int SEQ { get; set; }
        public string MMCODE { get; set; }
        public int APPQTY { get; set; }
        public int APVQTY { get; set; }
        //public string APPDEPT { get; set; }
        //public string USEDEPT { get; set; }
        public string STOCKCODE { get; set; }
        public string APPLYTIME { get; set; }
        //public string USEID { get; set; }//有很多人員代碼的欄位
        public string LOTNO { get; set; }
        public string EXPDATE { get; set; }
    }
}