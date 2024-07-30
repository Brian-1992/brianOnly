using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class CB0002M : JCLib.Mvc.BaseModel
    {
        public string MAT_CLASS { get; set; } //物料分類(畫面)
        public string MMCODE { get; set; } //院內碼(畫面)
        public string MMNAME_C { get; set; } //中文品名(畫面)
        public string MMNAME_E { get; set; } //英文品名(畫面)
        public string BARCODE { get; set; } //條碼資料(畫面)
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string VENDER { get; set; }
        public string SEQ { get; set; }
        public string MAT_CLSNAME { get; set; }

        //惠軒///////////////////////////////////
        public string BARCODE_IMAGE_STR { get; set; }
        public string TRATIO { get; set; }
        public string BARCODE_OLD { get; set; }
        public string STATUS { get; set; }
        public string MAT_CLASS_CODE { get; set; } //MAT_CLASS

    }
}
