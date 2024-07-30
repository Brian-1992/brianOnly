using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class CD0005D : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; } //項次(畫面)
        public string MMCODE { get; set; } //院內碼(畫面)
        public string MMNAME_C { get; set; } //中文品名(畫面)
        public string MMNAME_E { get; set; } //英文品名(畫面)
        public string APPQTY { get; set; } //申請數量(畫面)
        public string ACT_PICK_QTY { get; set; } //揀貨數量(畫面)
        public string BASE_UNIT { get; set; } //撥補單位(畫面)
    }
}
