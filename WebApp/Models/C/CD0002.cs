using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CD0002 : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string PICK_DATE { get; set; }
        public string DOCNO { get; set; }
        public string SEQ { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string APPQTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string STORE_LOC { get; set; }

        public string SORTER { get; set; }
        public string LOT_NO { get; set; }
        public string PICK_USERID { get; set; }


        public string APLYITEM_NOTE { get; set; }

        public string APPDEPT { get; set; }
        public string APPDEPTNAME { get; set; }
        public string MATCLS_GRP { get; set; }
        public string APPID { get; set; }
        public string APPID_NAME { get; set; }
        public string EXT { get; set; }
        public string Barcode { get; set; }
        public int TempSeq { get; set; }
    }

    public class WhnoDocmCounts : JCLib.Mvc.BaseModel
    {
        public string TOWH { get; set; }
        public int TOTAL_DOCM_KIND1 { get; set; }
        public decimal DEFAULT_COMPLEXITY { get; set; }
        public decimal AVG_COMPLEXITY { get; set; }
    }
}