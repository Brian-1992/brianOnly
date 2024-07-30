using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class WhMmInvqty
    {
        public string WH_NO { get; set;}
        public string WH_NAME { get; set; }
        public string MMCODE { get; set; }
        public string TOTAL_INV_QTY { get; set; }

        public IEnumerable<LotExpInv> LOT_EXP_INV { get; set; }
    }

    public class LotExpInv {
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string INV_QTY { get; set; }
    }
}