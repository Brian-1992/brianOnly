using System;
using System.Collections.Generic;

namespace WebAppVen.Models
{
    public class WB_PUT_M : JCLib.Mvc.BaseModel
    {
        public string AGEN_NO { get; set; } //廠商碼
        public string MMCODE { get; set; } //院內碼(畫面)
        public string DEPT { get; set; } //責任中心
        public string DEPTNAME { get; set; } //寄放地點
        public string MEMO { get; set; } //備註(畫面)
        public string MMNAME_C { get; set; } //中文品名(畫面)
        public string MMNAME_E { get; set; } //英文品名(畫面)
        public int QTY { get; set; } //現有寄放量(畫面)
        public string STATUS { get; set; } //狀態
        public string CREATE_TIME { get; set; } //建立日期
        public string CREATE_USER { get; set; } //建立人員
        public string UPDATE_IP { get; set; } //異動IP
        public string UPDATE_TIME { get; set; } //異動日期
        public string UPDATE_USER { get; set; } //異動人員
        public string AGEN_NAMEC { get; set; } //廠商碼+名稱(畫面)
        public string DEPT_NAME { get; set; } //寄放地點(畫面)
        public string STATUS_NAME { get; set; } //狀態名稱(畫面)
        public string AGEN_NAMEC_DISPLAY { get; set; } //廠商碼+名稱(白底顯示)
        public string AGEN_NAMEC_TEXT { get; set; } //廠商碼+名稱(紅底顯示)
        public string MMCODE_DISPLAY { get; set; } //院內碼+名稱(白底顯示)
        public string MMCODE_TEXT { get; set; } //院內碼+名稱(紅底顯示)
        public string DEPT_NAME_TEXT { get; set; } //寄放地點(紅底顯示)
        public string STATUS_TEXT { get; set; } //狀態(紅底顯示)
        public string QTY_DISPLAY { get; set; } //現有寄放量(白底顯示)
    }
}
