using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BC0005M : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; } //訂單編號(畫面)
        public string AGEN_NO { get; set; } //廠商代碼
        public string AGEN_NAMEC { get; set; } //廠商名稱(畫面)
        public string PO_TIME { get; set; } //訂單時間
        public string M_CONTID { get; set; } //合約識別碼
        public string PO_STATUS { get; set; } //訂單狀態
        public string PO_STATUS_CODE { get; set; } //訂單狀態代碼+名稱(瀏覽)
        public string PO_STATUS_NAME { get; set; } //訂單狀態名稱(畫面)
        public string CREATE_TIME { get; set; } //建立日期
        public string CREATE_USER { get; set; } //建立人員
        public string UPDATE_TIME { get; set; } //異動日期
        public string UPDATE_USER { get; set; } //異動人員
        public string UPDATE_IP { get; set; } //異動IP
        public string MEMO { get; set; } //主備註-MAIL內容(畫面)
        public string MEMO_DISPLAY { get; set; } //主備註-MAIL內容(白底顯示)
        public string ISCONFIRM { get; set; } //是否確認彙總
        public string ISBACK { get; set; } //是否回覆
        public string PHONE { get; set; } //廠商電話
        public string SMEMO { get; set; } //特殊備註-MAIL內容特別註記(紅色顯示)(畫面)
        public string SMEMO_DISPLAY { get; set; } //特殊備註-MAIL內容特別註記(紅色顯示)(白底顯示)
        public string ISCOPY { get; set; } //Y-已複製到外網,N-未複製
        public string SDN { get; set; } //來源單號(畫面) from PH_SMALL_M(DN)
    }
}
