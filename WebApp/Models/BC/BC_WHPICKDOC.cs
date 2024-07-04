using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BC_WHPICKDOC : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string PICK_DATE { get; set; }
        public string DOCNO { get; set; }
        public string APPLY_KIND { get; set; }
        public float COMPLEXITY { get; set; }
        public string LOT_NO { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }



        public string APPID { get; set; }
        public string APPITEM_SUM { get; set; }
        public string APPQTY_SUM { get; set; }
        public string APPCNT { get; set; }
        public string APPDEPT_NAME { get; set; }
        public string APPITEM_CNT { get; set; }

        public IEnumerable<BC_WHPICK> Picks { get; set; }
        public string ITEM_STRING { get; set; }
        public string PICK_USERID { get; set; }
        public string CALC_TIME { get; set; }


        public string APPLY_NOTE { get; set; }

        public string PICKER_NAME { get; set; }


        public string DOCNOS { get; set; }
        public string TOWH { get; set; }
        public string WH_NAME { get; set; }
        public string INID { get; set; }
        public string INID_NAME { get; set; }
        public string DOCNO_COUNTS { get; set; }
        public string MMCODE_COUNTS { get; set; }


        public BC_WHPICKDOC() {
            WH_NO = string.Empty;
            PICK_DATE = string.Empty;
            DOCNO = string.Empty;
            APPLY_KIND = string.Empty;
            COMPLEXITY = 0;
            LOT_NO = string.Empty;
            APPID = string.Empty;
            APPITEM_SUM = string.Empty;
            APPQTY_SUM = string.Empty;
            APPCNT = string.Empty;
            APPDEPT_NAME = string.Empty;
            APPITEM_CNT = string.Empty;
            APPLY_NOTE = string.Empty;
            PICKER_NAME = string.Empty;
        }
    }
}