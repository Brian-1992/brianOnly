using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BC0005D : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼(畫面)
        public string MMNAME_C { get; set; } //中文品名(畫面)
        public string MMNAME_E { get; set; } //英文品名(畫面)
        public string M_AGENLAB { get; set; } //廠牌(畫面)
        public string M_PURUN { get; set; } //申購計量單位(畫面)
        public string PO_PRICE { get; set; } //訂單單價(合約單價)(畫面)
        public string PO_QTY { get; set; } //訂單數量(包裝單位數量)(畫面)
        public string PO_AMT { get; set; } //總金額(畫面)
        public string MEMO { get; set; } //備註(畫面)
    }
}
