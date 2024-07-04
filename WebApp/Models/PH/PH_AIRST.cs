using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class PH_AIRST : JCLib.Mvc.BaseModel
    {
        public string AGEN_NO { get; set; }
        public string FBNO { get; set; }
        public string NAMEC { get; set; }
        public int SEQ { get; set; }
        public DateTime TXTDAY { get; set; }
        public string AIR { get; set; }
        public string DEPT { get; set; }
        public string XSIZE { get; set; }
    }
}