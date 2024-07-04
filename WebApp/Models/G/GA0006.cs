using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class GA0006 : JCLib.Mvc.BaseModel
    {
        public string DATA_YM { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MUSE_QTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string CREATE_TIME_T { get; set; }
        public DateTime CREATE_TIME { get; set; }
        
    }
}