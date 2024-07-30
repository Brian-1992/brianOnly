using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0163M : JCLib.Mvc.BaseModel
    {
        public string ITEM_NAME { get; set; }
        public string AMT_1 { get; set; }
        public string AMT_2 { get; set; }
        public string REMARK { get; set; }

        public string MAT_CLASS { get; set; }
        public string P_INV_AMT { get; set; }
        public string INV_AMT { get; set; }
        public string USE_AMT { get; set; }
        public string LOWTURN_INV_AMT { get; set; }
        public string D_AMT { get; set; }
    }
}