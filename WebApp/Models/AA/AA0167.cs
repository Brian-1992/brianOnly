using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0167 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string WH_NAME_C { get; set; }
        public string PMN_INVQTY { get; set; }
        public string APL_INQTY { get; set; }
        public string APL_OUTQTY { get; set; }
        public string INVENTORYQTY { get; set; }
        public string ADJ_QTY { get; set; }
        public string INV_QTY_End { get; set; }
        public string INV_QTY_Now { get; set; }
        public string E_ORDERDCFLAG { get; set; }

        public string DATA_YM { get; set; }
    }
}