using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class BG0008 : JCLib.Mvc.BaseModel
    {
        public string AGEN_NO{ get; set; } //廠商代碼
        public string AGEN_NAMEC{ get; set; } //廠商名稱
        public string PO_NO{ get; set; } //訂單編號
        public string MMCODE{ get; set; } //院內碼
        public string MMNAME_C{ get; set; } //中文名稱
        public string MMNAME_E{ get; set; } //英文名稱
        public string M_PURUN { get; set; } //計量單位
        public string PO_QTY { get; set; } //訂單數量
        public string PO_PRICE{ get; set; } //單價
        public string DELI_QTY{ get; set; } //進貨量
        public string NOIN_QTY{ get; set; } //未進貨量
        public string AMOUNT { get; set; } //差異量
        public string AGEN_TEL { get; set; } //電話
        public string MAT_CLASS { get; set; } //物料類別
        public string DATA_ARR { get; set; } //DATA串連

        
    }
}