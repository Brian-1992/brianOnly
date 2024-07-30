using System;
using System.Collections.Generic;

namespace WebAppVen.Models
{
    public class WB_AIRST : JCLib.Mvc.BaseModel
    {
        public string AGEN_NO { get; set; }
        public string FBNO { get; set; }
        public string NAMEC { get; set; }
        public int SEQ { get; set; }
        public DateTime TXTDAY { get; set; }
        public DateTime CHK_DATE { get; set; }
        public DateTime EXP_DATE { get; set; }
        public DateTime INPUT_DATE { get; set; }
        public string AIR { get; set; }
        public string MAT { get; set; }
        public string MEMO { get; set; }
        public string SBNO { get; set; }
        public string DEPT { get; set; }
        public string XSIZE { get; set; }
    }
}