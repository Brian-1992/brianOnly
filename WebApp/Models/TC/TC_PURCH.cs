using System;

namespace WebApp.Models
{
    public class TC_PURCH : JCLib.Mvc.BaseModel
    {
        public string PUR_NO { get; set; }
        public string PUR_DATE { get; set; }
        public string TC_TYPE { get; set; }
        public string PUR_UNM { get; set; }
        public string PUR_NOTE { get; set; }
        public string PURCH_ST { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string PUR_QTY { get; set; }
        public string PUR_UNIT { get; set; }
        public string IN_PURPRICE { get; set; }
        public string PUR_AMOUNT { get; set; }
    }
}