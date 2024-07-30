using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIS14SUPDETtransfer
{
    public class BC_BARCODE
    {
        public string MMCODE { get; set; } //院內碼(畫面)
        public string BARCODE { get; set; } //品項條碼(畫面)
        public string STATUS { get; set; } //使用代碼
        public string TRATIO { get; set; } //轉換率(畫面)
        public string XCATEGORY { get; set; } //條碼分類代碼
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string STATUS_NAME { get; set; } //使用代碼名稱(畫面)
        public string XCATEGORY_NAME { get; set; } //條碼分類代碼名稱(畫面)
        public string MMNAME_C { get; set; } //院內碼中文品名(畫面)
        public string MMNAME_E { get; set; } //院內碼英文品名(畫面)
        public string MMCODE_TEXT { get; set; } //院內碼(白底顯示)
        public string BARCODE_TEXT { get; set; } //品項條碼(白底顯示)
        public string STATUS_DISPLAY { get; set; } //使用代碼(紅底顯示)
        public string XCATEGORY_DISPLAY { get; set; } //條碼分類代碼(紅底顯示)
    }
}
