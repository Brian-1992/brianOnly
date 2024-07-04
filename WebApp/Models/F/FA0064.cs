using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class FA0064 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }   //院內碼
        public string MMNAME_E { get; set; } //英文品名
        public string MMNAME_C { get; set; } //中文品名
        public string E_SCIENTIFICNAME { get; set; } //成份名稱           
        public string MED_LICENSE { get; set; } //許可證字號
        public string E_RESTRICTCODE { get; set; } //管制級別
        public string CANCEL_ID { get; set; } //是否全院停用
        public string DELI_QTY { get; set; } //進貨量
        public string TR_INV_QTY { get; set; } //轉讓北門
        public string APPQTY { get; set; } //退貨量
        public string PINVQTY { get; set; } //結存量
        public string BASE_UNIT { get; set; } //計量單位
        public string CNV_RATE { get; set; } //換算率
        public string CNV_DELI_QTY { get; set; } //換算後進貨量
        public string CNV_TR_INV_QTY { get; set; } //換算後轉讓北門
        public string CNV_APPQTY { get; set; } //換算後退貨量
        public string CNV_PINVQTY { get; set; } //換算後結存量
        public string DECLARE_UI { get; set; } //申報計量單位
        public string DECLARE_UI1 { get; set; } //申報計量單位
        public string CNV_RATE1 { get; set; } //換算率1    
    }
}