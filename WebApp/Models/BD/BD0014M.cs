using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.BD
{
    public class BD0014M : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string 訂單編號 { get; set; }
        public string 訂購日期 { get; set; }
        public string 備註 { get; set; }
        public string 廠商 { get; set; }
        public string 電話 { get; set; }
        public string 傳真 { get; set; }
        public string 送貨地點 { get; set; }
        public string 列印日期 { get; set; }
        public string 藥材代碼 { get; set; }
        public string 藥材名稱 { get; set; }
        public string 出貨單位 { get; set; }
        public string 單位 { get; set; }
        public string 單價 { get; set; }
        public string 小計 { get; set; }
        public string 買斷寄庫 { get; set; }
        public string 合約到期日 { get; set; }
        public string 合約案號 { get; set; }
        public string 病人姓名 { get; set; }
        public string 病人病歷號 { get; set; }
        public string 明細備註 { get; set; }
        public string 分批交貨日期 { get; set; }


        public string DATA_NAME { get; set; }
        public string DATA_VALUE { get; set; } // 帶入至TEXTARE中
        public string DATA_REMARK { get; set; }
        public string MAT_CLASS { get; set; }
        public string EMAIL { get; set; }

        
    }
}