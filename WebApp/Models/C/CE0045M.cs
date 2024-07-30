using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class CE0045M : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼(畫面)
        public string MMNAME_C { get; set; } //中文品名(畫面)
        public string MMNAME_E { get; set; } //英文品名(畫面)
        public string BASE_UNIT { get; set; }
        public string INV_QTY { get; set; }
        public string CHK_QTY { get; set; }
        public string MEMO { get; set; }
        public string OLD_INV_QTY { get; set; } // 上期結存
        public string CURRENT_APPLY_AMOUNT { get; set; } // 本月申請總量


        public string CHK_UID { get; set; }
        public string CHK_TIME { get; set; }
    }
}
