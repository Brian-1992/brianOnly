using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0056D : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //子藥院內碼(畫面)
        public string MMNAME_E { get; set; } //藥品名稱(畫面)
        public string E_SONTRANSQTY { get; set; } //子藥轉換量(畫面)
        public string E_PARCODE { get; set; } //子藥註記代碼
        public string E_PARCODE_NAME { get; set; } //子藥註記名稱(畫面)
        public string E_PARCODE_CODE { get; set; } //子藥註記代碼+名稱
        public string E_PARORDCODE { get; set; } //母藥院內碼(畫面)
        public string UPDATE_IP { get; set; } //異動IP
        public string UPDATE_TIME { get; set; } //異動日期
        public string UPDATE_USER { get; set; } //異動人員
    }
}
