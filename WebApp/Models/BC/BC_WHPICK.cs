using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BC_WHPICK : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string PICK_DATE { get; set; }
        public string DOCNO { get; set; }
        public string SEQ { get; set; }
        public string MMCODE { get; set; }
        public string APPQTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string APLYITEM_NOTE { get; set; }
        public string MAT_CLASS { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string WEXP_ID { get; set; }
        public string STORE_LOC { get; set; }
        public string PICK_USERID { get; set; }
        public string PICK_SEQ { get; set; }
        public string ACT_PICK_USERID { get; set; }
        public string ACT_PICK_QTY { get; set; }
        public string ACT_PICK_TIME { get; set; }
        public string ACT_PICK_NOTE { get; set; }
        public string HAS_CONFIRMED { get; set; }
        public string BOXNO { get; set; }
        public string BARCODE { get; set; }
        public string XCATEGORY { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string HAS_SHIPOUT { get; set; }
        // ===============================================
        public string LOT_NO { get; set; }
        public string CALC_TIME { get; set; }
        public string PICK_USERNAME { get; set; }
        public string WEXP_ID_DESC { get; set; }

        public string ITEM_STRING { get; set; }
        public string APPDEPT { get; set; }
        public string INV_QTY { get; set; }
        public string APPLY_NOTE { get; set; }
        public string NEED_PICK_ITEMS { get; set; }
        public string LACK_PICK_ITEMS { get; set; }
        public string VALID_DATE { get; set; }
        public string LOT_NO_F { get; set; }
        public string APPDEPTNAME { get; set; }
        public string SHIPOUTCNT { get; set; }


    }
}