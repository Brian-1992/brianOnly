using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.AA
{
    public class AA0129 : JCLib.Mvc.BaseModel
    {
        public string WH_NAME { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public float INV_QTY { get; set; }
        public float ONWAY_QTY { get; set; }
        public float SAFE_QTY { get; set; }
        public float OPER_QTY { get; set; }
        public float SHIP_QTY { get; set; }
        public float HIGH_QTY { get; set; }
    
    }
}