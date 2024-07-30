using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.UT
{
    public class UT_DOCM : JCLib.Mvc.BaseModel
    {
        public string FLOWID { get; set; }
        public string DOCTYPE { get; set; }
        public string EXP_STATUS { get; set; }
        public Int16 SEQ { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string FRWH { get; set; }
        public string FRWH_N { get; set; }
        public string TOWH_N { get; set; }
        public float QTY { get; set; }
        //public Int16 EXP_CNT { get; set; }
        public string EDIT_TYPE { get; set; }
        public string IS_CLOSE { get; set; }
        public string IS_ADD { get; set; }
    }
}