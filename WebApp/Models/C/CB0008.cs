using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CB0008 : JCLib.Mvc.BaseModel
    {
        // MI_WHINV
        public string WH_NO { get; set; }
        public string INV_QTY { get; set; }
        //MI_MAST
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string M_STOREID { get; set; }
        public string M_AGENNO { get; set; }
        public string M_AGENLAB { get; set; }
        // BC_BARCODE
        public string BARCODE { get; set; }
        //MI_WLOCINV
        public string STORE_LOC { get; set; }
        public string MAT_CLASS { get; set; }

    }
}