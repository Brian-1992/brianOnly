using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0027D : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼(畫面)
        public string MMCODE_TEXT { get; set; } //院內碼(紅底顯示)
        public string MMNAME_C { get; set; } //中文品名(畫面)
        public string MMNAME_E { get; set; } //英文品名(畫面)
        public string AGEN_NAME { get; set; } //廠商名稱(畫面)
        public string LOT_NO { get; set; } //批號(畫面)
        public string LOT_NO_TEXT { get; set; } //批號(紅底顯示)
        public string EXP_DATE { get; set; } //效期(畫面)
        public string APVQTY { get; set; } //退貨量(畫面)
        public string APVQTY_TEXT { get; set; } //退貨量(紅底顯示)
        public string INV_QTY { get; set; } //效期數量(畫面)
        public string BASE_UNIT { get; set; } //單位(紅底)
        public string C_TYPE { get; set; } //進貨/合約
        public string C_TYPE_NAME { get; set; } //進貨/合約(畫面)
        public string C_TYPE_NAME_TEXT { get; set; } //進貨/合約(紅底)
        public string IN_PRICE { get; set; } //進貨單價(畫面)
        public string CONTPRICE { get; set; } //合約單價(畫面)
        public string C_AMT { get; set; } //換貨金額(畫面)
        public string ITEM_NOTE { get; set; } //備註(畫面)
        public string SEQ { get; set; } //單據項次
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string UPDATE_TIME { get; set; }
        public string DOCNO { get; set; } //調帳單號
        public string C_UP { get; set; } //退換貨單價
        public string FRWH { get; set; }
    }
}
