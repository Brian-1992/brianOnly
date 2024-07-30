using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CHK_EXPLOC
    {
        public string CHK_NO { get; set; }
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string EXP_DATE { get; set; }
        public string LOT_NO { get; set; }
        public string STORE_LOC { get; set; }
        public string TRN_QTY { get; set; }
        public string ORI_INV_QTY { get; set; }
        public string USE_QTY { get; set; }
        public string IsExpired { get; set; }
    }
}