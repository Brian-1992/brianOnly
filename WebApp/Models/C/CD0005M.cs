using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class CD0005M : JCLib.Mvc.BaseModel
    {
        public string ACT_PICK_QTY_CODE { get; set; } //揀貨差異
        public string HAS_CONFIRMED { get; set; } //確認狀態
        public string WH_NO { get; set; } //庫房代碼
        public string PICK_DATE { get; set; } //揀貨日期
        public string LOT_NO { get; set; } //揀貨批號(畫面)
        public string DOCNO { get; set; } //申請單號碼(畫面)
        public string APPDEPT { get; set; } //申請單位
        public string PICK_USERID { get; set; } //分配揀貨人員代碼
        public string PICK_USERNAME { get; set; } //揀貨人員名稱(畫面)
        public string HAS_CONFIRMED_CODE { get; set; } //是否已確認揀貨結果
        public string CONFIRM_STATUS { get; set; } //是否已確認揀貨結果(畫面)
        public string ITEM_CNT_SUM { get; set; } //品項數(畫面)
        public string APPQTY_SUM { get; set; } //總件數(畫面)
        public int PICK_ITEM_CNT_SUM { get; set; } //已揀項數(畫面)
        public string ACT_PICK_QTY_SUM { get; set; } //已揀總件數(畫面)
    }
}
