using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BG0011 : JCLib.Mvc.BaseModel
    {
        public string AGEN_NO { get; set; }         // 廠商代碼
        public string AGEN_NAMEC { get; set; }      // 廠商名稱
        public string INVOICE { get; set; }         // 發票號碼
        public string UNI_NO { get; set; }          // 統一編號
        public string MONEY_1 { get; set; }         // 實付金額
        public string MONEY_2 { get; set; }         // 合約優惠
        public string MONEY_3 { get; set; }         // 應付金額
    }
}