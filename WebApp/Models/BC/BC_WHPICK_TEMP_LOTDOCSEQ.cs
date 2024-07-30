using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BC_WHPICK_TEMP_LOTDOCSEQ : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string CALC_TIME { get; set; }
        public string LOT_NO { get; set; }
        public string DOCNO { get; set; }
        public string SEQ { get; set; }
        public string MMCODE { get; set; }
        public string APPQTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string STORE_LOC { get; set; }
        public string PICK_SEQ { get; set; }
        public string GROUP_NO { get; set; }
        public string CREATE_USER { get; set; }
        public string CREATE_DATE { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }



        public string APPITEM_SUM { get; set; }
        public string APPQTY_SUM { get; set; }
        public string ITEM_STRING { get; set; }
        public string PICK_USERID { get; set; }
        public string PICK_DATE { get; set; }
    }
}