using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0080 : JCLib.Mvc.BaseModel
    {
        public string DATA_YM { get; set; }//年月
        public string MMCODE { get; set; }//藥品代碼
        public string MMNAME_E { get; set; }//品名
        public string E_RESTRICTCODE { get; set; }//管制級別
        public string BASE_UNIT { get; set; }//單位
        public int APL_INQTY { get; set; }//本期入庫
        public int APL_OUTQTY { get; set; }//本期出庫
        public int INVENTORYQTY { get; set; }//盤點差異量
        public int ADJ_QTY { get; set; }//調帳數量
        public int INV_QTY { get; set; }//本期結存量
        public int LAST_INVMONQTY { get; set; }//上月結存量
    }
}