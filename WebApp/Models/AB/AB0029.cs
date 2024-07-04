using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.AB
{
    public class AB0029 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string WH_NAME { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string MMCODE { get; set; }
        public float INV_QTY { get; set; }
    }
}