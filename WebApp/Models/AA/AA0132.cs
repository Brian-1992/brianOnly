using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0132 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string PMN_INVQTY { get; set; }
        public string apl_inqty { get; set; }
        public string apl_outqty { get; set; }

        public string STORE_QTY { get; set; }
        public string AVG_PRICE { get; set; }
        public string store_amount { get; set; }
        public string STORE_QTYM { get; set; }
        public string MIL_PRICE { get; set; }

        public string EXG_INQTY { get; set; }
        public string EXG_OUTQTY { get; set; }
        public string CaddM { get; set; }
        public string chk_qty { get; set; }
        public string pack_qty { get; set; }

        public string CHK_REMARK { get; set; }

        public string STORE_QTYC { get; set; }

    }
}