using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AA0027M : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; } //調帳單號(畫面)
        public string FLOWID { get; set; } //狀態代碼
        public string FLOWID_NAME { get; set; } //狀態名稱(畫面)
        public string UPDATE_TIME { get; set; } //調帳日期(畫面)
        public string FRWH { get; set; } //調帳庫別
        public string FRWH_NAME { get; set; } //調帳庫別(畫面)
        public string FRWH_CODE { get; set; } //調帳庫別(紅底顯示)
        public string APPLY_NOTE { get; set; } //備註(畫面)
        public string APPID { get; set; } //建立人員(畫面)
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string APPTIME { get; set; } //申請時間(畫面)
        public string APPTIME_TEXT { get; set; } //申請時間(白框顯示)
        public string APP_ID_NAME { get; set; }
    }
}
