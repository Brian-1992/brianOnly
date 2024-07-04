using System;
using System.Collections.Generic;

namespace WebAppVen.Models
{
    public class WB_PUT_D : JCLib.Mvc.BaseModel
    {
        public string AGEN_NO { get; set; } //廠商碼
        public string MMCODE { get; set; } //三總院內碼
        public string TXTDAY { get; set; } //交易日期
        public string SEQ { get; set; } //交易流水號
        public string DEPT { get; set; } //責任中心
        public string EXTYPE { get; set; } //異動類別
        public string MEMO { get; set; } //備註(畫面)
        public int QTY { get; set; } //異動數量(畫面)
        public string STATUS { get; set; } //狀態
        public string CREATE_TIME { get; set; } //建立日期
        public string CREATE_USER { get; set; } //建立人員
        public string UPDATE_IP { get; set; } //異動IP
        public string UPDATE_TIME { get; set; } //異動日期
        public string UPDATE_USER { get; set; } //異動人員
        public string EXTYPE_NAME { get; set; } //異動類別(畫面)
        public string STATUS_NAME { get; set; } //狀態名稱(畫面)
        public string TXTDAY_TEXT { get; set; } //交易日期(畫面) (紅底顯示)
        public string TXTDAY_DISPLAY { get; set; } //交易日期 (白底顯示)
        public string EXTYPE_TEXT { get; set; } //異動類別 (紅底顯示)
        public string EXTYPE_DISPLAY { get; set; } //異動類別 (白底顯示)
        public string QTY_DISPLAY { get; set; } //交易日期 (白底顯示)
        public string STATUS_TEXT { get; set; } //狀態名稱(紅底顯示)
    }
}
