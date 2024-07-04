using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0086M : JCLib.Mvc.BaseModel
    {
        public string E_ITEMARMYNO { get; set; }        // 軍聯項次號 MI_MAST
        public string MMNAME_E { get; set; }            // 英文品名 MI_MAST
        public string E_COMPUNIT {get;set; }            // 成份
        public string E_MANUFACT { get; set; }          // 原製造商
        public string BASE_UNIT { get; set; }           // 計量單位代碼
        public string UP { get; set; }                  // 單價
        public string APVQTY { get; set; }              // 核撥數量(實際核撥數量)
        public string AMT { get; set; }                 // 金額


        public string PrintType { get; set; }           // 列印類別  轉出:frwh  轉入:towh 
        public string PrintUser { get; set; }           // 列印人員

        public string APPQTY { get; set; }
    }
}