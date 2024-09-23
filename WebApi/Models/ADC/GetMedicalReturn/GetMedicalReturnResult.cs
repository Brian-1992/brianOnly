using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models.ADC.GetMedicalReturn
{
    public class GetMedicalReturnResult
    {
        public string DOCNO { get; set; }
        public int SEQ { get; set; }
        public string MMCODE { get; set; }
        public int APPQTY { get; set; }
        public string APPDEPT { get; set; }
        public string USEDEPT { get; set; }
        public string APPLYTIME { get; set; }
        public string USEID { get; set; }//有很多人員代碼的欄位
        public string FRWH { get; set; }
        public string TOWH { get; set; }
        public string LOTNO { get; set; }
        public string EXPDATE { get; set; }
    }
}