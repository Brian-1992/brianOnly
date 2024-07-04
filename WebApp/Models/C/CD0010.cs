using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class CD0010 : JCLib.Mvc.BaseModel
    {
        // -- Master
        public string DOCNO { get; set; }
        public string APPDEPT { get; set; }
        public string APPDEPTNAME { get; set; }
        public string APPNAME { get; set; }
        public string ITEM_SUM { get; set; }
        public string APPQTY_SUM { get; set; }
        public string LOT_NO { get; set; }
        public string ACT_PICK_QTY_SUM { get; set; }
        public string DIFFQTY_SUM { get; set; }

        //單據狀態
        public string FLOWID_DES { get; set; }
        //揀貨項數
        public string ACT_PICK_ITEM_SUM { get; set; }


        // -- Master query 
        public string WH_NO { get; set; }
        public string PICK_DATE_START { get; set; }
        public string PICK_DATE_END { get; set; }
        public string SHOPOUT_DATE_START { get; set; }
        public string SHOPOUT_DATE_END { get; set; }
        //public string DOCNO { get; set; }
        public string MMCODE { get; set; }
        public string ACT_PICK_USERID { get; set; }
        public string HAS_APPQTY { get; set; }
        public string HAS_CONFIRMED { get; set; }
        public string HAS_SHOPOUT { get; set; }

        // -- Detail 
        public string SEQ { get; set; } // 01.項次
        //public string MMCODE { get; set; } // 02.院內碼
        public string MMNAME_C { get; set; } // 03.中文品名
        public string MMNAME_E { get; set; } // 04.英文品名
        public string APPQTY { get; set; } // 05.申請數
        public string BASE_UNIT { get; set; } // 06.撥補單位
        public string STORE_LOC { get; set; } // 07.儲位
        public string ACT_PICK_USERNAME { get; set; } // 08.揀貨人員
        public string ACT_PICK_QTY { get; set; } // 09.揀貨數
        //public string HAS_CONFIRMED { get; set; } // 10.已確認
        public string BOXNO { get; set; } // 11.物流箱號
        public string HAS_SHIPOUT { get; set; } // 12.已出庫

        //核撥日
        public string APVDATE { get; set; }
        //差異品項數
        public string DIFFITEM_SUM { get; set; }
        //差異件數
        public string DIFFQTY { get; set; }
        public string USE_BOX_QTY { get; set; }
        public string PICK_USERID { get; set; }
    }
}
