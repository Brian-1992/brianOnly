using System;
using System.Collections.Generic;

namespace WebAppVen.Models
{
    public class WB_AIRHIS : JCLib.Mvc.BaseModel
    {
        public string AGEN_NO { get; set; }
        public string FBNO { get; set; }
        public string NAMEC { get; set; }
        public int SEQ { get; set; }
        public DateTime TXTDAY { get; set; }
        public DateTime CHK_DATE { get; set; }
        //public string AIR { get; set; }
        public DateTime CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        //public string DEPT { get; set; }
        public string EXTYPE { get; set; }
        public string STATUS { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_IP { get; set; }
        public string UPDATE_USER { get; set; }
        public string XSIZE { get; set; }
        //public string MAT { get; set; }
        //public string MEMO { get; set; }
        //public string SBNO { get; set; }
        //public DateTime EXP_DATE { get; set; }
        //public DateTime INPUT_DATE { get; set; }

        //public string MMCODE_p { get; set; }
        public string FBNO_p { get; set; }
        public DateTime TXTDAY_B_p { get; set; }
        public DateTime TXTDAY_E_p { get; set; }

        public string MMCODE_old { get; set; }
        public string FBNO_old { get; set; }
        public DateTime TXTDAY_old { get; set; }

        public string CHECK_RESULT { get; set; }
        public string IMPORT_RESULT { get; set; }

    }
}