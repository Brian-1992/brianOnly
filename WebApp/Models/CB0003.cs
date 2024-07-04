using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CB0003 : JCLib.Mvc.BaseModel
    {
        public string MAT_CLSNAME { get; set; }
        public string MMCODE { get; set; }
        public string BARCODE { get; set; }
        public string XCATEGORY { get; set; }
        public string DESCRIPT { get; set; }
        public string STATUS { get; set; }
        public string STATUS_NAME { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
    }
}